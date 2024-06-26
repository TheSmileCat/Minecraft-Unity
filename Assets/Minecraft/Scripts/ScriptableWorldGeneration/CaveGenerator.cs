﻿#region

using System;
using System.Collections.Generic;
using Minecraft.Scripts.Configurations;
using UnityEngine;
using static Minecraft.Scripts.WorldConsts;
using Random = System.Random;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration
{
  [CreateAssetMenu(menuName = "Minecraft/WorldGeneration/CaveGenerator")]
  public class CaveGenerator : StatelessGenerator, ISerializationCallbackReceiver
  {
    [SerializeField] private int m_Range = 8;
    [SerializeField] private string m_AirBlock;
    [SerializeField] private string m_SandBlock;
    [SerializeField] private string m_GravelBlock;
    [SerializeField] private string m_WaterBlock;
    [SerializeField] private string m_LavaBlock;
    [SerializeField] private string[] m_ReplaceableBlocks;

    [NonSerialized] private HashSet<string> m_ReplaceableBlockSet;


    public override void Generate(IWorld world, ChunkPos pos, BlockData[,,] blocks, Quaternion[,,] rotations,
      byte[,] heightMap, GenerationHelper helper, GenerationContext context)
    {
      pos = ChunkPos.Get(pos.X / ChunkWidth, pos.Z / ChunkWidth); // 。。。

      var range = m_Range;
      var biome = context.Biomes[ChunkWidth / 2, ChunkWidth / 2];
      var random = new Random(helper.Seed);

      var rand1 = random.Next();
      var rand2 = random.Next();

      // 遍历周围 (range * 2 + 1) * (range * 2 + 1) 的区块，默认 range = 8
      for (var x = pos.X - range; x <= pos.X + range; x++)
      {
        for (var z = pos.Z - range; z <= pos.Z + range; z++)
        {
          var randX = x * rand1;
          var randZ = z * rand2;
          random = new Random(randX ^ randZ ^ helper.Seed);

          RecursiveGenerate(world, ChunkPos.Get(x, z), pos, blocks, biome, random);
        }
      }
    }

    private void RecursiveGenerate(IWorld world, ChunkPos pos, ChunkPos center, BlockData[,,] blocks, BiomeData biome,
      Random random)
    {
      // 之前根据 chunkXZ 设置种子了
      var seedPointCount = random.Next(15);

      // 仅 1/7 概率生成洞穴
      if (random.Next(7) != 0)
      {
        seedPointCount = 0;
      }

      for (var i = 0; i < seedPointCount; i++)
      {
        // 在 chunk 内 x = 0-16, y = 8~127, z = 0-16 随机选种子点
        float seedPointX = pos.X * ChunkWidth + random.Next(ChunkWidth);
        float seedPointY = random.Next(120) + 8;
        float seedPointZ = pos.Z * ChunkWidth + random.Next(ChunkWidth);
        var directionCount = 1;

        var seedPoint = new Vector3(seedPointX, seedPointY, seedPointZ);

        // 四分之一概率挖一个默认的洞
        if (random.Next(4) == 0)
        {
          // 使用默认参数挖出一个洞
          AddTunnel(world, center, blocks, biome, random.Next(), seedPoint, random);
          directionCount += random.Next(4);
        }

        // 向几个方向挖洞
        for (var j = 0; j < directionCount; j++)
        {
          var yawAngle = (float)random.NextDouble() * Mathf.PI * 2.0f;
          var pitchAngle = ((float)random.NextDouble() - 0.5f) * 2.0f / 8.0f;
          var rangeScale = (float)random.NextDouble() * 2.0f + (float)random.NextDouble();

          if (random.Next(10) == 0)
          {
            // 扩大到1~3倍
            rangeScale *= (float)random.NextDouble() * 3.0f + 1.0f;
          }

          AddTunnel(world, center, blocks, biome, random.Next(), seedPoint, rangeScale, yawAngle, pitchAngle, 0, 0, 1);
        }
      }
    }

    protected void AddTunnel(IWorld world, ChunkPos center, BlockData[,,] blocks, BiomeData biome, int seed,
      Vector3 seedPoint, Random random)
    {
      AddTunnel(world, center, blocks, biome, seed, seedPoint, 1.0f + (float)random.NextDouble() * 6.0f, 0, 0, -1, -1,
        0.5f);
    }

    // 挖洞
    protected void AddTunnel(IWorld world, ChunkPos center, BlockData[,,] blocks, BiomeData biome, int seed,
      Vector3 seedPoint, float rangeScale, float yawAngle, float pitchAngle, int smallRange, int bigRange,
      float heightScale)
    {
      float centerBlockX = center.X * ChunkWidth + 8;
      float centerBlockZ = center.Z * ChunkWidth + 8;

      float yawAngleOffset = 0;
      float pitchAngleOffset = 0;
      var random = new Random(seed);

      if (bigRange <= 0)
      {
        var tmp = (m_Range - 1) * 16;
        bigRange = tmp - random.Next(tmp / 4);
      }

      var smallRangeIsNull = false;

      if (smallRange == -1)
      {
        smallRange = bigRange / 2;
        smallRangeIsNull = true;
      }

      // 可能的扩展点
      var keyPoint = random.Next(bigRange / 2) + bigRange / 4;

      var flag = random.Next(6) == 0;

      // 循环挖出一条道路
      for (; smallRange < bigRange; smallRange++)
      {
        // 用 sin 从 1 到 0 过渡
        var xzRange = 1.5f + Mathf.Sin(smallRange * Mathf.PI / bigRange) * rangeScale;
        var yRange = xzRange * heightScale;

        // 向 yawAngle、pitchAngle 方向偏移一个单位
        var cos = Mathf.Cos(pitchAngle);
        var sin = Mathf.Sin(pitchAngle);
        seedPoint.x += Mathf.Cos(yawAngle) * cos;
        seedPoint.y += sin;
        seedPoint.z += Mathf.Sin(yawAngle) * cos;

        // 1/6 概率俯仰角衰减较慢
        if (flag)
        {
          pitchAngle *= 0.92f;
        }
        else
        {
          pitchAngle *= 0.7f;
        }

        pitchAngle += pitchAngleOffset * 0.1f;
        yawAngle += yawAngleOffset * 0.1f;
        pitchAngleOffset *= 0.9f;
        yawAngleOffset *= 0.75f;
        pitchAngleOffset += (float)((random.NextDouble() - random.NextDouble()) * random.NextDouble() * 2.0f);
        yawAngleOffset += (float)((random.NextDouble() - random.NextDouble()) * random.NextDouble() * 4.0f);

        if (!smallRangeIsNull && smallRange == keyPoint && rangeScale > 1 && bigRange > 0)
        {
          // 向左右两边扩展
          AddTunnel(world, center, blocks, biome, random.Next(), seedPoint, (float)random.NextDouble() * 0.5f + 0.5f,
            yawAngle - Mathf.PI / 2f, pitchAngle / 3.0f, smallRange, bigRange, 1);
          AddTunnel(world, center, blocks, biome, random.Next(), seedPoint, (float)random.NextDouble() * 0.5f + 0.5f,
            yawAngle + Mathf.PI / 2f, pitchAngle / 3.0f, smallRange, bigRange, 1);
          return;
        }

        if (smallRangeIsNull || random.Next(4) != 0)
        {
          var xDist = seedPoint.x - centerBlockX;
          var zDist = seedPoint.z - centerBlockZ;
          float restRange = bigRange - smallRange;
          var range = rangeScale + 2.0f + 16.0f;

          if (xDist * xDist + zDist * zDist - restRange * restRange > range * range)
          {
            return;
          }

          // 种子点在中心方块附近（不在的话说明不在这个区块内不用管）
          if (seedPoint.x >= centerBlockX - 16 - xzRange * 2
              && seedPoint.z >= centerBlockZ - 16 - xzRange * 2
              && seedPoint.x <= centerBlockX + 16 + xzRange * 2
              && seedPoint.z <= centerBlockZ + 16 + xzRange * 2)
          {
            var startX = Mathf.FloorToInt(seedPoint.x - xzRange) - center.X * ChunkWidth - 1;
            var endX = Mathf.FloorToInt(seedPoint.x + xzRange) - center.X * ChunkWidth + 1;
            var startY = Mathf.FloorToInt(seedPoint.y + yRange) + 1;
            var endY = Mathf.FloorToInt(seedPoint.y - yRange) - 1;
            var startZ = Mathf.FloorToInt(seedPoint.z - xzRange) - center.Z * ChunkWidth - 1;
            var endZ = Mathf.FloorToInt(seedPoint.z + xzRange) - center.Z * ChunkWidth + 1;

            // 限制坐标范围
            startX = Mathf.Max(startX, 0);
            endX = Mathf.Min(endX, 16);
            startY = Mathf.Min(startY, 248);
            endY = Mathf.Max(endY, 1);
            startZ = Mathf.Max(startZ, 0);
            endZ = Mathf.Min(endZ, 16);

            // 判断是不是海洋，如果在海洋则不生成洞穴
            var isOcean = false;

            for (var x = startX; !isOcean && x < endX; x++)
            {
              for (var z = startZ; !isOcean && z < endZ; z++)
              {
                for (var y = startY + 1; !isOcean && y >= endY - 1; y--)
                {
                  if (y >= 0 && y < 256)
                  {
                    if (blocks[x, y, z].InternalName == m_WaterBlock)
                    {
                      isOcean = true;
                    }

                    // 只判断边界
                    if (y != endY - 1
                        && x != startX
                        && x != endX - 1
                        && z != startZ
                        && z != endZ - 1)
                    {
                      y = endY;
                    }
                  }
                }
              }
            }

            if (!isOcean)
            {
              // 挖掉一个椭球内的方块
              for (var x = startX; x < endX; x++)
              {
                // （归一化的距离）
                var xDist1 = ((x + center.X * ChunkWidth) + 0.5f - seedPoint.x) / xzRange;

                for (var z = startZ; z < endZ; z++)
                {
                  var zDist1 = ((z + center.Z * ChunkWidth) + 0.5f - seedPoint.z) / xzRange;
                  var isTopBlock = false;

                  // 平面上平方距离 < 1
                  if (xDist1 * xDist1 + zDist1 * zDist1 < 1)
                  {
                    // 先获取高度 (改为二分)
                    var height = 64;
                    var upBound = 255;
                    var downbound = 1;

                    while (upBound != downbound)
                    {
                      var y = (upBound + downbound) / 2;

                      if (blocks[x, y, z].InternalName == m_AirBlock
                          && blocks[x, y - 1, z].InternalName != m_AirBlock)
                      {
                        height = y;
                        break;
                      }
                      else if (blocks[x, y, z].InternalName == m_AirBlock)
                      {
                        upBound = y - 1;
                      }
                      else
                      {
                        downbound = y + 1;
                      }
                    }

                    for (var y = startY; y > endY; --y)
                    {
                      var yDist = ((y - 1) + 0.5f - seedPoint.y) / yRange;

                      // 空间平方距离 < 1
                      if (yDist > -0.7f)
                      {
                        if (xDist1 * xDist1 + yDist * yDist + zDist1 * zDist1 < 1)
                        {
                          var curBlock = blocks[x, y, z];
                          var upBlock = blocks[x, y + 1, z];

                          if (y == height - 1)
                          {
                            isTopBlock = true;
                          }

                          // 把这个方块替换为空气或岩浆
                          DigBlock(world, x, y, z, center, blocks, biome, isTopBlock, curBlock, upBlock);
                        }
                      }
                      else
                      {
                        break;
                      }
                    }
                  }
                }
              }

              if (smallRangeIsNull)
              {
                break;
              }
            }
          }
        }
      }
    }

    protected bool IsBlockReplaceable(BlockData curBlock, BlockData upBlock)
    {
      return m_ReplaceableBlockSet.Contains(curBlock.InternalName)
             || ((curBlock.InternalName == m_SandBlock || curBlock.InternalName == m_GravelBlock) &&
                 upBlock.InternalName != m_WaterBlock);
    }

    protected void DigBlock(IWorld world, int x, int y, int z, ChunkPos center, BlockData[,,] blocks, BiomeData biome,
      bool foundTop, BlockData block, BlockData upBlock)
    {
      var top = world.BlockDataTable.GetBlock(biome.TopBlock);
      var filler = world.BlockDataTable.GetBlock(biome.FillerBlock);

      if (IsBlockReplaceable(block, upBlock) || block == top || block == filler)
      {
        var airBlock = world.BlockDataTable.GetBlock(m_AirBlock);
        var sandBlock = world.BlockDataTable.GetBlock(m_SandBlock);
        var lavaBlock = world.BlockDataTable.GetBlock(m_LavaBlock);

        // y < 10 放置岩浆
        if (y < 10)
        {
          // 设置为岩浆
          blocks[x, y, z] = lavaBlock;
        }
        else
        {
          // 设置为空气
          blocks[x, y, z] = airBlock;

          if (upBlock.InternalName == m_SandBlock)
          {
            // 如果上面的方块是沙子则替换为沙石
            blocks[x, y + 1, z] = sandBlock;
          }

          if (foundTop && blocks[x, y - 1, z] == filler)
          {
            // 如果挖开了顶层方块则把下面的方块设置为 biome 顶层方块
            blocks[x, y - 1, z] = top;
          }
        }
      }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      m_ReplaceableBlockSet = new HashSet<string>(m_ReplaceableBlocks);
    }
  }
}