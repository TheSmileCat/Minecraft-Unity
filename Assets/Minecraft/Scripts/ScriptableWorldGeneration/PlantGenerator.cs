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
  [CreateAssetMenu(menuName = "Minecraft/WorldGeneration/PlantGenerator")]
  public class PlantGenerator : StatelessGenerator
  {
    protected enum PlantType
    {
      Grass,
      Flower,
      Cacti,
      Mushroom
    }

    [SerializeField] protected PlantType m_PlantType;
    [SerializeField] protected string m_PlantBlock;
    [SerializeField] [Range(1, 10)] protected int m_MinPlantHeight = 1;
    [SerializeField] [Range(0, 5)] protected int m_MaxPlantHeightDelta = 0;
    [SerializeField] protected string[] m_BlocksToGrowOn;
    [SerializeField] protected string[] m_ReplaceableBlocks;


    protected bool IsBlockReplaceable(BlockData block)
    {
      return Array.IndexOf(m_ReplaceableBlocks, block.InternalName) != -1;
    }

    protected bool CanGrowPlant(int x, int y, int z, BlockData[,,] blocks, int height)
    {
      if (y < 0 || (y + height) >= ChunkHeight)
      {
        return false;
      }

      if (Array.IndexOf(m_BlocksToGrowOn, blocks[x, y, z].InternalName) == -1)
      {
        return false;
      }

      for (var by = y + 1; by <= y + height; by++)
      {
        if (!IsBlockReplaceable(blocks[x, by, z]))
        {
          return false;
        }
      }

      return true;
    }

    public override void Generate(IWorld world, ChunkPos pos, BlockData[,,] blocks, Quaternion[,,] rotations,
      byte[,] heightMap, GenerationHelper helper, GenerationContext context)
    {
      var biome = context.Biomes[8, 8];
      var random = new Random(pos.X ^ pos.Z ^ helper.Seed);
      var plantBlock = world.BlockDataTable.GetBlock(m_PlantBlock);
      var height = random.Next(m_MaxPlantHeightDelta) + m_MinPlantHeight;
      var count = random.Next(m_PlantType switch
      {
        PlantType.Grass => biome.GrassPerChunk,
        PlantType.Flower => biome.FlowersPerChunk,
        PlantType.Cacti => biome.CactiPerChunk,
        PlantType.Mushroom => biome.MushroomsPerChunk,
        _ => throw new NotImplementedException()
      });

      for (var i = 0; i < count; i++)
      {
        var x = random.Next(ChunkWidth);
        var z = random.Next(ChunkWidth);

        var h = 0;

        for (var y = ChunkHeight - 1; y >= 0; y--)
        {
          if (blocks[x, y, z].PhysicState == PhysicState.Solid)
          {
            h = y;
            break;
          }
        }

        if (CanGrowPlant(x, h, z, blocks, height))
        {
          for (var y = h + 1; y <= h + height; y++)
          {
            blocks[x, y, z] = plantBlock;
          }
        }
      }
    }
  }
}