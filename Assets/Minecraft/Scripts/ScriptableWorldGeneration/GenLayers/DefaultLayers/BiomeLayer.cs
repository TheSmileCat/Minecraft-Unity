#region

using System;
using Minecraft.Scripts.Configurations;
using Unity.Collections;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration.GenLayers.DefaultLayers
{
  public class BiomeLayer : StatelessGenLayer
  {
    public BiomeLayer(int seed, StatelessGenLayer parent) : base(seed, parent)
    {
    }

    public override NativeInt2DArray GetInts(int areaX, int areaY, int areaWidth, int areaHeight, Allocator allocator)
    {
      var parentResult = m_Parent.GetInts(areaX, areaY, areaWidth, areaHeight, allocator);

      for (var i = 0; i < areaHeight; ++i)
      {
        for (var j = 0; j < areaWidth; ++j)
        {
          var random = new Random(GetChunkSeed(areaX + j, areaY + i));

          if (parentResult[i, j] == (int)BiomeId.Plains)
          {
            var r = random.Next(10);

            if (r >= 0 && r < 1)
            {
              parentResult[i, j] = (int)BiomeId.Forest;
            }
            else if (r >= 1 && r < 2)
            {
              parentResult[i, j] = (int)BiomeId.Mountains;
            }
            else if (r >= 2 && r < 3)
            {
              parentResult[i, j] = (int)BiomeId.Swamp;
            }
            else if (r >= 3 && r < 5)
            {
              parentResult[i, j] = (int)BiomeId.Desert;
            }
            else if (r >= 5 && r < 7)
            {
              parentResult[i, j] = (int)BiomeId.Taiga;
            }
            else if (r >= 7 && r < 8)
            {
              parentResult[i, j] = (int)BiomeId.Savanna;
            }
          }
        }
      }

      return parentResult;
    }
  }
}