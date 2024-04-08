#region

using System;
using System.Collections.Generic;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Entities;
using Minecraft.Scripts.Lua;
using Minecraft.Scripts.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
using static Minecraft.Scripts.WorldConsts;

#endregion

namespace Minecraft.Scripts.PlayerControls
{
  public class FluidInteractor : MonoBehaviour, ILuaCallCSharp
  {
    [Serializable]
    private class FluidInfo
    {
      public string BlockName;
      public float VelocityMultiplier;
      public int ViewDistance;

      [ColorUsage(true, true)] [FormerlySerializedAs("AmbientColor")]
      public Color AmbientColorDay;

      [ColorUsage(true, true)] public Color AmbientColorNight;
    }

    [SerializeField] private FluidInfo[] m_Fluids;

    private Dictionary<string, FluidInfo> m_FluidMap;
    private string m_BlockAtHead = null;
    private string m_BlockAtBody = null;

    private void Start()
    {
      m_FluidMap = new Dictionary<string, FluidInfo>();

      for (var i = 0; i < m_Fluids.Length; i++)
      {
        m_FluidMap.Add(m_Fluids[i].BlockName, m_Fluids[i]);
      }
    }

    public void UpdateState(IAABBEntity entity, Transform camera, out float velocityMultiplier)
    {
      CheckHead(entity, camera);
      velocityMultiplier = CheckBody(entity);
    }

    private void CheckHead(IAABBEntity entity, Transform camera)
    {
      var pos = camera.position;
      var y = Mathf.FloorToInt(pos.y);

      if (y < 0 || y >= ChunkHeight)
      {
        return;
      }

      var x = Mathf.FloorToInt(pos.x);
      var z = Mathf.FloorToInt(pos.z);
      var block = entity.World.RWAccessor.GetBlock(x, y, z);

      if (block.InternalName != m_BlockAtHead && m_FluidMap.TryGetValue(block.InternalName, out var info))
      {
        m_BlockAtHead = block.InternalName;
        ShaderUtility.ViewDistance = info.ViewDistance;
        ShaderUtility.WorldAmbientColorDay = info.AmbientColorDay;
        ShaderUtility.WorldAmbientColorNight = info.AmbientColorNight;
      }
    }

    private float CheckBody(IAABBEntity entity)
    {
      var aabb = entity.BoundingBox + entity.Position;
      var center = aabb.Center.FloorToInt();
      var minY = Mathf.FloorToInt(aabb.Min.y);
      var maxY = Mathf.FloorToInt(aabb.Max.y);

      BlockData blockData = null;
      var index = int.MaxValue;

      for (var y = minY; y < maxY; y++)
      {
        var block = entity.World.RWAccessor.GetBlock(center.x, y, center.z);

        // 根据 m_Fluids 数组元素的顺序来确定方块
        for (var i = 0; i < m_Fluids.Length; i++)
        {
          // 越靠前，优先级越高
          if (i >= index)
          {
            break;
          }

          if (block.InternalName == m_Fluids[i].BlockName)
          {
            blockData = block;
            index = i;
            break;
          }
        }
      }

      if (blockData != null && m_BlockAtBody != blockData.InternalName)
      {
        m_BlockAtBody = blockData.InternalName;
        return m_Fluids[index].VelocityMultiplier;
      }

      return m_FluidMap.TryGetValue(m_BlockAtBody, out var info) ? info.VelocityMultiplier : 1; // default is 1
    }
  }
}