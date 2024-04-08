#region

using System;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Rendering;
using Minecraft.XLua.Src;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#endregion

namespace Minecraft.Scripts.Entities
{
  [DisallowMultipleComponent]
  public class LuaBlockEntity : Entity, IRenderableEntity
  {
    [NonSerialized] private BlockMeshBuilder<ushort> m_MeshBuilder;

    [NonSerialized] private BlockData m_Block;
    [NonSerialized] private LuaTable m_ContextTable;


    [field: NonSerialized] public bool EnableRendering { get; set; }

    [field: NonSerialized] public Mesh SharedMesh { get; private set; }

    [field: NonSerialized] public Material SharedMaterial { get; private set; }

    [field: NonSerialized] public MaterialPropertyBlock MaterialProperty { get; private set; }


    protected override void OnInitialize()
    {
      base.OnInitialize();

      SharedMesh = new Mesh();
      SharedMesh.MarkDynamic();
      MaterialProperty = new MaterialPropertyBlock();
      m_MeshBuilder = BlockMeshBuilder<ushort>.CreateBlockEntityMeshBuilder(true);

      EnableRendering = true;
      SharedMaterial = null;
      m_Block = null;

      OnCollisions += OnCollisionsCallback;
    }

    public void SetBlockAndPosition(BlockData block, Vector3Int position)
    {
      InitializeEntityIfNot();

      m_Block = block;
      SharedMaterial = World.BlockDataTable.GetMaterial(block.Material.Value);
      m_Transform.position = position;

      BuildMesh(position);

      m_ContextTable = World.LuaManager.CreateTable();
      m_Block.EntityInit(World, this, m_ContextTable);
    }

    public override void OnRecycle()
    {
      m_Block.EntityDestroy(World, this, m_ContextTable);

      base.OnRecycle();

      SharedMesh.Clear(false);
      MaterialProperty.Clear();
      // m_MeshBuilder.ClearBuffers();

      EnableRendering = true;
      SharedMaterial = null;
      m_Block = null;
      m_ContextTable.Dispose();
      m_ContextTable = null;
    }

    private void BuildMesh(Vector3Int position)
    {
      // 渲染在原点的位置
      m_MeshBuilder.AddBlock(position, -position, m_Block, World.RWAccessor);
      m_MeshBuilder.ApplyAndClearBuffers(SharedMesh, MeshTopology.Triangles, false, Allocator.Temp);

      SharedMesh.RecalculateNormals();
      SharedMesh.RecalculateTangents();
      SharedMesh.RecalculateBounds();
    }

    private void Update()
    {
      m_Block.EntityUpdate(World, this, m_ContextTable);
    }

    protected override void FixedUpdate()
    {
      m_Block.EntityFixedUpdate(World, this, m_ContextTable);
      base.FixedUpdate();

      if (m_Transform.position.y < -20)
      {
        // 掉出世界了
        World.EntityManager.DestroyEntity(this);
      }
    }

    private void OnCollisionsCallback(CollisionFlags flags)
    {
      m_Block.EntityOnCollisions(World, this, flags, m_ContextTable);
    }

    public void Render(int layer, Camera camera, ShadowCastingMode castShadows, bool receiveShadows)
    {
      var matrix = Matrix4x4.TRS(m_Transform.position, Quaternion.identity, Vector3.one);
      Graphics.DrawMesh(SharedMesh, matrix, SharedMaterial, layer, camera, 0, MaterialProperty, castShadows,
        receiveShadows, null, false);
    }
  }
}