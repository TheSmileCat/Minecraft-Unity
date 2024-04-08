#region

using System;
using Object = UnityEngine.Object;

#endregion

namespace Minecraft.Scripts.Assets
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public sealed class EnsureAssetTypeAttribute : Attribute
  {
    public Type AssetType { get; }

    public EnsureAssetTypeAttribute(Type type)
    {
      AssetType = type ?? typeof(Object);
    }
  }
}