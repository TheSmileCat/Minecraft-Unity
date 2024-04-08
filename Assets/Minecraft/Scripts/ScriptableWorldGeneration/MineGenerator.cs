#region

using System;
using Minecraft.Scripts.Configurations;
using UnityEngine;
using static Minecraft.Scripts.WorldConsts;
using Random = System.Random;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration
{
  [CreateAssetMenu(menuName = "Minecraft/WorldGeneration/MineGenerator")]
  public class MineGenerator : StatelessGenerator
  {
    [SerializeField] private string m_OreBlock;
    [SerializeField] private int m_Size = 17;
    [SerializeField] private int m_Count = 20;
    [SerializeField] private int m_MinHeight = 0;
    [SerializeField] private int m_MaxHeight = 128;
    [SerializeField] private string[] m_ReplaceableBlocks;

    public override void Generate(IWorld world, ChunkPos pos, BlockData[,,] blocks, Quaternion[,,] rotations,
      byte[,] heightMap, GenerationHelper helper, GenerationContext context)
    {
      var minHeight = m_MinHeight;
      var maxHeight = m_MaxHeight;
      var count = m_Count;
      var ore = world.BlockDataTable.GetBlock(m_OreBlock);

      var chunkSeed = pos.X ^ pos.Z ^ helper.Seed ^ ore.ID;
      var random = new Random(chunkSeed);

      if (minHeight > maxHeight)
      {
        var tmp = minHeight;
        minHeight = maxHeight;
        maxHeight = tmp;
      }
      else if (maxHeight == minHeight)
      {
        if (minHeight < ChunkHeight - 1)
        {
          ++maxHeight;
        }
        else
        {
          --minHeight;
        }
      }

      for (var i = 0; i < count; i++)
      {
        var x = pos.X + random.Next(ChunkWidth);
        var y = random.Next(minHeight, maxHeight);
        var z = pos.Z + random.Next(ChunkWidth);
        Generate(x, y, z, blocks, random, ore, count);
      }
    }

    public void Generate(int x, int y, int z, BlockData[,,] blocks, Random random, BlockData ore, int count)
    {
      // 在xz平面上的方向
      var angle = (float)random.NextDouble() * Mathf.PI;
      var xRange = Mathf.Sin(angle) * count / 8.0f;
      var zRange = Mathf.Cos(angle) * count / 8.0f;

      // 起始点和结束点
      var startX = (x + 8) + xRange;
      var endX = (x + 8) - xRange;
      var startZ = (z + 8) + zRange;
      var endZ = (z + 8) - zRange;
      float startY = (y - 2) + random.Next(3);
      float endY = (y - 2) + random.Next(3);

      for (var i = 0; i < count; i++)
      {
        // 插值参数
        var t = (float)i / count;

        // 椭球中心
        var centerX = Mathf.Lerp(startX, endX, t);
        var centerY = Mathf.Lerp(startY, endY, t);
        var centerZ = Mathf.Lerp(startZ, endZ, t);

        // 椭球尺寸（可以看出 XZ 和 Y 尺寸一样，应该是球）
        var scale = (float)random.NextDouble() * count / 16.0f;
        var diameterXZ = (Mathf.Sin(Mathf.PI * t) + 1) * scale + 1;
        var diameterY = (Mathf.Sin(Mathf.PI * t) + 1) * scale + 1;

        // 椭球包围盒
        var minX = Mathf.FloorToInt(centerX - diameterXZ * 0.5f);
        var minY = Mathf.FloorToInt(centerY - diameterY * 0.5f);
        var minZ = Mathf.FloorToInt(centerZ - diameterXZ * 0.5f);
        var maxX = Mathf.FloorToInt(centerX + diameterXZ * 0.5f);
        var maxY = Mathf.FloorToInt(centerY + diameterY * 0.5f);
        var maxZ = Mathf.FloorToInt(centerZ + diameterXZ * 0.5f);

        // 把这个椭球里的方块替换为矿石
        for (var dx = minX; dx <= maxX; dx++)
        {
          var xDist = (dx + 0.5f - centerX) / (diameterXZ * 0.5f);

          // 参考椭球方程
          if (xDist * xDist < 1)
          {
            for (var dy = minY; dy <= maxY; dy++)
            {
              var yDist = (dy + 0.5f - centerY) / (diameterY * 0.5f);

              if (dy < 0 || dy >= ChunkHeight)
              {
                continue;
              }

              // 参考椭球方程
              if (xDist * xDist + yDist * yDist < 1)
              {
                for (var dz = minZ; dz <= maxZ; dz++)
                {
                  var zDist = (dz + 0.5f - centerZ) / (diameterXZ * 0.5f);

                  // 参考椭球方程
                  if (xDist * xDist + yDist * yDist + zDist * zDist < 1)
                  {
                    SetBlock(dx, dy, dz, blocks, ore);
                  }
                }
              }
            }
          }
        }
      }
    }

    private void SetBlock(int x, int y, int z, BlockData[,,] blocks, BlockData ore)
    {
      x %= ChunkWidth;
      z %= ChunkWidth;

      if (x < 0)
      {
        x += ChunkWidth;
      }

      if (z < 0)
      {
        z += ChunkWidth;
      }

      ref var block = ref blocks[x, y, z];

      if (Array.IndexOf(m_ReplaceableBlocks, block.InternalName) != -1)
      {
        block = ore;
      }
    }
  }
}