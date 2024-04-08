#region

using System;
using Minecraft.Scripts.Configurations;
using Unity.Collections;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration.GenLayers.DefaultLayers
{
  public class AddRiverLayer : StatelessGenLayer
  {
    public AddRiverLayer(int seed, StatelessGenLayer parent) : base(seed, parent)
    {
    }

    public override NativeInt2DArray GetInts(int areaX, int areaY, int areaWidth, int areaHeight, Allocator allocator)
    {
      var parentAreaX = areaX - 1;
      var parentAreaY = areaY - 1;
      var parentWidth = areaWidth + 2;
      var parentHeight = areaHeight + 2;

      var parentRes = m_Parent.GetInts(parentAreaX, parentAreaY, parentWidth, parentHeight, allocator);
      var result = new NativeInt2DArray(areaHeight, areaWidth, allocator);

      for (var y = 0; y < areaHeight; ++y)
      {
        for (var x = 0; x < areaWidth; ++x)
        {
          var randomValueY1 = parentRes[y + 1, x];
          var randomValueX2Y1 = parentRes[y + 1, x + 2];
          var randomValueX1 = parentRes[y, x + 1];
          var randomValueX1Y2 = parentRes[y + 2, x + 1];
          var randomValueX1Y1 = parentRes[y + 1, x + 1];

          int tempX, tempY;

          if (x + areaX >= 0)
          {
            tempX = (x + areaX) / 64;
          }
          else
          {
            tempX = (x + areaX) / 64 - 1;
          }

          if (y + areaY >= 0)
          {
            tempY = (y + areaY) / 64;
          }
          else
          {
            tempY = (y + areaY) / 64 - 1;
          }

          var seed = GetChunkSeed(tempX, tempY);
          var rand = new Random(seed);

          if (randomValueX1Y1 == randomValueY1 && randomValueX1Y1 == randomValueX1 &&
              randomValueX1Y1 == randomValueX2Y1 && randomValueX1Y1 == randomValueX1Y2)
          {
            result[y, x] = randomValueX1Y1;
          }
          else if (rand.Next(5) == 0)
          {
            if (randomValueX1Y1 != (int)BiomeId.Ocean && randomValueX1Y1 != (int)BiomeId.Beach)
            {
              result[y, x] = (int)BiomeId.River;
            }
          }
          else
          {
            result[y, x] = randomValueX1Y1;
          }
        }
      }

      parentRes.Dispose();
      return result;
    }
  }
}