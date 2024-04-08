#region

using Unity.Collections;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration.GenLayers.DefaultLayers
{
  public class ZoomLayer : StatelessGenLayer
  {
    public ZoomLayer(int seed, StatelessGenLayer parent) : base(seed, parent)
    {
    }

    public override unsafe NativeInt2DArray GetInts(int areaX, int areaY, int areaWidth, int areaHeight,
      Allocator allocator)
    {
      var parentAreaX = areaX > 0 ? areaX / 2 : (areaX - 1) / 2;
      var parentAreaY = areaY > 0 ? areaY / 2 : (areaY - 1) / 2;

      var parentWidth = areaWidth / 2 + 2;
      var parentHeight = areaHeight / 2 + 2;

      var parentRes = m_Parent.GetInts(parentAreaX, parentAreaY, parentWidth, parentHeight, allocator);
      var tempWidth = (parentWidth - 1) * 2;
      var tempHeight = (parentHeight - 1) * 2;

      var temp = new NativeInt2DArray(tempHeight, tempWidth, allocator);

      for (var parentY = 0; parentY < parentHeight - 1; ++parentY)
      {
        var parentValue = parentRes[parentY, 0];
        var parentValueY1 = parentRes[parentY + 1, 0];

        for (var parentX = 0; parentX < parentWidth - 1; ++parentX)
        {
          var randomSeed = GetChunkSeed((parentX + parentAreaX) * 2, (parentY + parentAreaY) * 2);

          var parentValueX1 = parentRes[parentY, parentX + 1];
          var parentValueX1Y1 = parentRes[parentY + 1, parentX + 1];

          temp[parentY * 2, parentX * 2] = parentValue;

          var array = stackalloc int[2] { parentValue, parentValueY1 };
          temp[parentY * 2 + 1, parentX * 2] = SelectRandom(randomSeed, array, 2);

          array[1] = parentValueX1;
          temp[parentY * 2, parentX * 2 + 1] = SelectRandom(randomSeed, array, 2);

          temp[parentY * 2 + 1, parentX * 2 + 1] =
            SelectModeOrRandom(randomSeed, parentValue, parentValueX1, parentValueY1, parentValueX1Y1);

          parentValue = parentValueX1;
          parentValueY1 = parentValueX1Y1;
        }
      }

      var result = new NativeInt2DArray(areaHeight, areaWidth, allocator);
      var areaOffsetX = Mathf.Abs(areaX % 2);
      var areaOffsetY = Mathf.Abs(areaY % 2);

      for (var resultY = 0; resultY < areaHeight; ++resultY)
      {
        for (var resultX = 0; resultX < areaWidth; ++resultX)
        {
          result[resultY, resultX] = temp[resultY + areaOffsetY, resultX + areaOffsetX];
        }
      }

      parentRes.Dispose();
      temp.Dispose();
      return result;
    }

    public static StatelessGenLayer Magnify(int seed, StatelessGenLayer layer, int times)
    {
      for (var i = 0; i < times; i++)
      {
        layer = new ZoomLayer(seed + 1, layer);
      }

      return layer;
    }
  }
}