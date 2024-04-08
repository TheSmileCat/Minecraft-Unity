#region

using System;
using Minecraft.Scripts.Lua;

#endregion

namespace Minecraft.Scripts.Assets
{
  [Serializable]
  public class AssetInfo : ILuaCallCSharp
  {
    public string AssetName;
    public string AssetBundleName;
  }
}