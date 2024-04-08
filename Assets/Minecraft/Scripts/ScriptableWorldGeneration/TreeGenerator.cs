#region

using System;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.PhysicSystem;
using UnityEngine;
using static Minecraft.Scripts.WorldConsts;
using Random = System.Random;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration
{
  [CreateAssetMenu(menuName = "Minecraft/WorldGeneration/TreeGenerator")]
  public class TreeGenerator : StatelessGenerator
  {
    [SerializeField] [Range(5, 10)] protected int m_MinTreeHeight = 5;
    [SerializeField] [Range(0, 5)] protected int m_MaxTreeHeightDelta = 3;
    [SerializeField] protected string m_WoodBlock;
    [SerializeField] protected string m_LeavesBlock;
    [SerializeField] protected string[] m_BlocksToGrowOn;
    [SerializeField] protected string[] m_ReplaceableBlocks;

    protected bool IsBlockReplaceable(BlockData block)
    {
      return Array.IndexOf(m_ReplaceableBlocks, block.InternalName) != -1;
    }

    protected bool CanTreeGrow(int x, int y, int z, BlockData[,,] blocks, int height)
    {
      // 不超出世界边界
      if (y < 1 || (y + height + 1) >= ChunkHeight)
      {
        return false;
      }

      var downBlock = blocks[x, y - 1, z];
      if (Array.IndexOf(m_BlocksToGrowOn, downBlock.InternalName) == -1)
      {
        return false;
      }

      // 检查所有方块可替换
      for (var by = y; by <= y + height + 1; by++)
      {
        var xzSize = 1;

        // 底端
        if (by == y)
        {
          xzSize = 0;
        }

        // 顶端
        if (by >= y + height - 1)
        {
          xzSize = 2;
        }

        // 检查这个平面所有方块可替换
        for (var bx = x - xzSize; bx <= x + xzSize; bx++)
        {
          for (var bz = z - xzSize; bz <= z + xzSize; bz++)
          {
            if (!IsBlockReplaceable(blocks[bx, by, bz]))
            {
              return false;
            }
          }
        }
      }

      return true;
    }

    protected virtual void Generate(int x, int y, int z, ChunkPos pos, BlockData[,,] blocks, IWorld world,
      Random random)
    {
      WorldUtility.WorldSpaceToAccessorSpacePosition(pos.XOZ, ref x, ref y, ref z);
      var height = random.Next(m_MaxTreeHeightDelta) + m_MinTreeHeight;

      if (!CanTreeGrow(x, y, z, blocks, height))
      {
        return;
      }

      var wood = world.BlockDataTable.GetBlock(m_WoodBlock);
      var leaves = world.BlockDataTable.GetBlock(m_LeavesBlock);

      // 生成叶子
      for (var ly = y + height - 3; ly <= y + height; ly++)
      {
        var restHeight = ly - (y + height);
        var xzSize = 1 - restHeight / 2;

        for (var lx = x - xzSize; lx <= x + xzSize; lx++)
        {
          var xOffset = lx - x;

          for (var lz = z - xzSize; lz <= z + xzSize; lz++)
          {
            var zOffset = lz - z;

            // 不在边缘4个点
            if (Math.Abs(xOffset) != xzSize || Math.Abs(zOffset) != xzSize || (restHeight != 0 && random.Next(2) != 0))
            {
              ref var block = ref blocks[lx, ly, lz];

              if (IsBlockReplaceable(block))
              {
                block = leaves;
              }
            }
          }
        }
      }

      // 生成木头
      for (var wy = y; wy < y + height; wy++)
      {
        ref var block = ref blocks[x, wy, z];

        if (IsBlockReplaceable(block))
        {
          block = wood;
        }
      }
    }

    public override void Generate(IWorld world, ChunkPos pos, BlockData[,,] blocks, Quaternion[,,] rotations,
      byte[,] heightMap, GenerationHelper helper, GenerationContext context)
    {
      var biome = context.Biomes[8, 8];
      var rand = new Random(pos.X ^ pos.Z ^ helper.Seed);

      var treesPerChunk = biome.TreesPerChunk;

      if (rand.NextDouble() < biome.ExtraTreeChance)
      {
        ++treesPerChunk;
      }

      for (var i = 0; i < treesPerChunk; i++)
      {
        var x = rand.Next(12) + 2;
        var z = rand.Next(12) + 2;

        var h = 0;

        for (var y = ChunkHeight - 1; y >= 0; y--)
        {
          if (blocks[x, y, z].PhysicState == PhysicState.Solid)
          {
            h = y + 1;
            break;
          }
        }

        Generate(pos.X + x, h, pos.Z + z, pos, blocks, world, rand);
      }
    }
  }
}