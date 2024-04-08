#region

using System;
using System.Collections.Generic;
using Minecraft.Scripts.Lua;

#endregion

namespace Minecraft.Scripts.Assets
{
  [Serializable]
  public class AssetCatalog : ILuaCallCSharp
  {
    public const string FileName = "catalog.json";

    public Dictionary<string, AssetInfo> Assets;
    public Dictionary<string, AssetBundleInfo> AssetBundles;
  }
}