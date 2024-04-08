#region

using Minecraft.Scripts.Configurations;
using Unity.Collections;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration.GenLayers.DefaultLayers
{
  public class AddBeachLayer : StatelessGenLayer
  {
    public AddBeachLayer(int seed, StatelessGenLayer parent) : base(seed, parent)
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
          var parentValue = parentRes[y, x];
          var parentValueX2 = parentRes[y, x + 2];
          var parentValueY2 = parentRes[y + 2, x];
          var parentValueX2Y2 = parentRes[y + 2, x + 2];
          var parentValueX1Y1 = parentRes[y + 1, x + 1];

          if (parentValueX1Y1 != 0 &&
              (parentValue == 0 || parentValueX2 == 0 || parentValueY2 == 0 || parentValueX2Y2 == 0))
          {
            if (parentValueX1Y1 != (int)BiomeId.Mountains)
            {
              result[y, x] = (int)BiomeId.Beach;
            }
          }
          else
          {
            result[y, x] = parentValueX1Y1;
          }
        }
      }

      parentRes.Dispose();
      return result;
    }
  }
}