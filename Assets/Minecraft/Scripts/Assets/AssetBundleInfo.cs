#region

using System;
using Minecraft.Scripts.Lua;

#endregion

namespace Minecraft.Scripts.Assets
{
  [Serializable]
  public sealed class AssetBundleInfo : ILuaCallCSharp
  {
    public string FileName;
    public string[] Assets;
    public string[] Dependencies;
  }
}