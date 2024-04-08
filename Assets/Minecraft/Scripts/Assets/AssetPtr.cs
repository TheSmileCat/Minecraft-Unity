#region

using System;
using Minecraft.Scripts.Lua;

#endregion

namespace Minecraft.Scripts.Assets
{
  [Serializable]
  public class AssetPtr : IEquatable<AssetPtr>, ILuaCallCSharp
  {
    public static readonly AssetPtr NullPtr = new(string.Empty);

    public string AssetGUID;

    public AssetPtr() : this(string.Empty)
    {
    }

    public AssetPtr(string assetGUID)
    {
      AssetGUID = assetGUID;
    }

    public bool Equals(AssetPtr other)
    {
      return StringComparer.OrdinalIgnoreCase.Equals(AssetGUID, other.AssetGUID);
    }

    public override bool Equals(object obj)
    {
      return (obj is AssetPtr ptr) && Equals(ptr);
    }

    public override int GetHashCode()
    {
      return AssetGUID.GetHashCode();
    }

    public static bool operator ==(AssetPtr left, AssetPtr right)
    {
      var f0 = left is null;
      var f1 = right is null;

      if (f0 && f1)
      {
        return true;
      }
      else if (!(f0 || f1))
      {
        return left.Equals(right);
      }
      else
      {
        return false;
      }
    }

    public static bool operator !=(AssetPtr left, AssetPtr right)
    {
      return !(left == right);
    }
  }
}