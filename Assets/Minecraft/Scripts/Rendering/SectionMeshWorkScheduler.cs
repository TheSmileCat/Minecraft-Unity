#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Minecraft.Scripts.Lua;
using Minecraft.Scripts.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using static Minecraft.Scripts.Rendering.RenderingUtility;

#if ENABLE_32BIT_MESH_INDEX
using SMeshBuilder = Minecraft.Rendering.SectionMeshBuilder<int>;
#else
using SMeshBuilder = Minecraft.Scripts.Rendering.BlockMeshBuilder<ushort>;
#endif

#endregion

namespace Minecraft.Scripts.Rendering
{
  [DisallowMultipleComponent]
  public class SectionMeshWorkScheduler :
    WorkScheduler<SectionMeshWorkScheduler.MainThreadWork, SectionMeshWorkScheduler.AsyncWork>, ILuaCallCSharp
  {
    public struct MainThreadWork
    {
      public Vector3Int Section { get; }
      public Chunk3x3Accessor Accessor { get; }

      public MainThreadWork(Vector3Int section, Chunk3x3Accessor accessor)
      {
        Section = section;
        Accessor = accessor;
      }
    }

    public struct AsyncWork
    {
      public Mesh Mesh { get; }
      public Vector3Int Section { get; }
      public float Priority { get; }
      public Chunk3x3Accessor Accessor { get; }

      public AsyncWork(Mesh mesh, Vector3Int section, float priority, Chunk3x3Accessor accessor)
      {
        Mesh = mesh;
        Section = section;
        Priority = priority;
        Accessor = accessor;
      }
    }

    private struct BuildSectionMeshJob : IJobParallelFor
    {
      private static List<MainThreadWork> s_Works;
      private static List<SMeshBuilder> s_MeshBuilders;

      private Mesh.MeshDataArray m_Meshes;

      private void Execute(int index)
      {
        var work = s_Works[index];
        var builder = s_MeshBuilders[index];
        builder.AddSection(work.Section, work.Accessor);
        builder.ApplyAndClearBuffers(m_Meshes[index], SectionMeshTopology);
      }

      public static void Execute(Mesh.MeshDataArray meshes, List<MainThreadWork> works, List<SMeshBuilder> meshBuilders)
      {
        s_Works = works;
        s_MeshBuilders = meshBuilders;

        var job = new BuildSectionMeshJob { m_Meshes = meshes };
        job.Schedule(meshes.Length, 1).Complete();

        s_Works = null;
        s_MeshBuilders = null;
      }

      void IJobParallelFor.Execute(int index) => Execute(index);
    }

    private class BuildableMesh
    {
      public Mesh Mesh { get; set; }
      public SMeshBuilder Builder { get; }

      public BuildableMesh(SMeshBuilder builder)
      {
        Mesh = null;
        Builder = builder;
      }
    }


    private List<Mesh> m_MeshBuffer; // main thread
    private List<SMeshBuilder> m_MeshBuilderBuffer; // main thread
    private ConcurrentQueue<BuildableMesh> m_BuildableMeshes; // worker thread
    private SendOrPostCallback m_ApplyMeshAction; // worker thread
    private SynchronizationContext m_SyncContext; // worker thread
    private int m_SubMeshCount;

    protected override void OnInitialize()
    {
      m_MeshBuffer = new List<Mesh>();
      m_MeshBuilderBuffer = new List<SMeshBuilder>();
      m_BuildableMeshes = new ConcurrentQueue<BuildableMesh>();
      m_ApplyMeshAction = ApplyMeshOnMainThread;
      m_SyncContext = SynchronizationContext.Current; // get the UnitySynchronizationContext
    }

    public void Initialize(IWorld world)
    {
      m_SubMeshCount = world.BlockDataTable.MaterialCount;
      StartWorkerThread();
    }

    public void ScheduleWork(Mesh mesh, Vector3Int section, Chunk3x3Accessor accessor)
    {
      Profiler.BeginSample("SectionMeshWorkScheduler.ScheduleWork");
      AddWork(new MainThreadWork(section, accessor));
      m_MeshBuffer.Add(mesh);
      Profiler.EndSample();
    }

    public void ScheduleAsyncWork(Mesh mesh, Vector3Int section, float priority, Chunk3x3Accessor accessor)
    {
      Profiler.BeginSample("SectionMeshWorkScheduler.ScheduleAsyncWork");
      AddWork(new AsyncWork(mesh, section, priority, accessor));
      Profiler.EndSample();
    }

    private void EnsureMeshBuilderBufferSize(int count)
    {
      if (m_MeshBuilderBuffer.Count >= count)
      {
        return;
      }

      for (var i = m_MeshBuilderBuffer.Count; i < count; i++)
      {
        m_MeshBuilderBuffer.Add(SMeshBuilder.CreateSectionMeshBuilder(m_SubMeshCount, true, true, true));
      }
    }

    private void ApplyMeshOnMainThread(object state)
    {
      Profiler.BeginSample("SectionMeshWorkScheduler.ApplyMeshOnMainThread");

      var mesh = state as BuildableMesh;

      if (mesh.Mesh)
      {
        mesh.Builder.ApplyAndClearBuffers(mesh.Mesh, SectionMeshTopology, false, Allocator.Temp);
        CalculateMeshNTBData(mesh.Mesh);
        m_BuildableMeshes.Enqueue(mesh);
      }

      Profiler.EndSample();
    }

    public override void ClearWorks(Action<AsyncWork> callback)
    {
      m_MeshBuffer.Clear();
      base.ClearWorks(callback);
    }

    protected override void DoMainThreadWorks(List<MainThreadWork> works)
    {
      EnsureMeshBuilderBufferSize(works.Count);

      if (works.Count == 1)
      {
        Profiler.BeginSample("SectionMeshWorkScheduler.BuildMesh (MainThread)");

        var mesh = m_MeshBuffer[0];
        var work = works[0];
        var builder = m_MeshBuilderBuffer[0];
        builder.AddSection(work.Section, work.Accessor);
        builder.ApplyAndClearBuffers(mesh, SectionMeshTopology, false, Allocator.Temp);
        CalculateMeshNTBData(mesh);

        Profiler.EndSample();
      }
      else
      {
        Profiler.BeginSample("SectionMeshWorkScheduler.ParallelBuildMeshes (MainThread)");

        var meshDataArray = Mesh.AllocateWritableMeshData(works.Count);
        BuildSectionMeshJob.Execute(meshDataArray, works, m_MeshBuilderBuffer);
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, m_MeshBuffer, SectionMeshUpdateFlags);

        for (var i = 0; i < m_MeshBuffer.Count; i++)
        {
          CalculateMeshNTBData(m_MeshBuffer[i]);
        }

        Profiler.EndSample();
      }
    }

    protected override void DoAsyncWork(in AsyncWork work)
    {
      if (!m_BuildableMeshes.TryDequeue(out var mesh))
      {
        var builder = SMeshBuilder.CreateSectionMeshBuilder(m_SubMeshCount, true, true, true);
        mesh = new BuildableMesh(builder);
      }

      mesh.Mesh = work.Mesh;
      mesh.Builder.AddSection(work.Section, work.Accessor);
      m_SyncContext.Post(m_ApplyMeshAction, mesh);
    }

    protected override int CompareAsyncWork(AsyncWork x, AsyncWork y)
    {
      return (int)(x.Priority - y.Priority);
    }

    protected static void CalculateMeshNTBData(Mesh mesh)
    {
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      mesh.RecalculateBounds();
    }
  }
}