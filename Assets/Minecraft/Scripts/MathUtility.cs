#region

using Minecraft.XLua.Src;
using UnityEngine;

#endregion

namespace Minecraft.Scripts
{
  [LuaCallCSharp]
  public static class MathUtility
  {
    public static Vector3Int FloorToInt(this Vector3 vec)
    {
      var x = Mathf.FloorToInt(vec.x);
      var y = Mathf.FloorToInt(vec.y);
      var z = Mathf.FloorToInt(vec.z);
      return new Vector3Int(x, y, z);
    }

    public static Vector3Int RoundToInt(this Vector3 vec)
    {
      var x = Mathf.RoundToInt(vec.x);
      var y = Mathf.RoundToInt(vec.y);
      var z = Mathf.RoundToInt(vec.z);
      return new Vector3Int(x, y, z);
    }

    public static Vector3 RotatePoint(Vector3 point, Quaternion rotation, Vector3 pivot)
    {
      return rotation * (point - pivot) + pivot;
    }
  }
}