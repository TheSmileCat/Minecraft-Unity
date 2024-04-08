#region

using System;
using Minecraft.XLua.Src;

#endregion

namespace Minecraft.Scripts.Collections
{
  [LuaCallCSharp]
  public static class HashUtility
  {
    public static bool IsPrime(int value)
    {
      if (value <= 1)
      {
        return false;
      }

      if ((value & 1) == 0)
      {
        return value == 2;
      }

      var sqrt = (int)Math.Sqrt(value);

      for (var i = 2; i <= sqrt; i++)
      {
        if (value % i == 0)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// 返回一个大于等于 3 和 <paramref name="min"/> 的素数。
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public static int GetPrimeCapacity(int min)
    {
      min = Math.Max(3, min);

      for (var i = min | 1; i < int.MaxValue; i += 2)
      {
        if (IsPrime(i))
        {
          return i;
        }
      }

      return min;
    }
  }
}