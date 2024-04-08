#region

using System;
using Minecraft.Scripts.Configurations;
using Unity.Collections;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration.GenLayers.DefaultLayers
{
  public class IslandLayer : StatelessGenLayer
  {
    public IslandLayer(int seed, StatelessGenLayer parent) : base(seed, parent)
    {
    }

    public override NativeInt2DArray GetInts(int areaX, int areaY, int areaWidth, int areaHeight, Allocator allocator)
    {
      var result = new NativeInt2DArray(areaHeight, areaWidth, allocator);

      for (var i = 0; i < areaHeight; ++i)
      {
        for (var j = 0; j < areaWidth; ++j)
        {
          var random = new Random(GetChunkSeed(areaX + j, areaY + i));
          result[i, j] = random.Next(5) == 0 ? 1 : 0;
        }
      }

      if (-areaWidth < areaX && areaX <= 0 && -areaHeight < areaY && areaY <= 0)
      {
        result[-areaY, -areaX] = (int)BiomeId.Plains;
      }

      return result;
    }
  }
}