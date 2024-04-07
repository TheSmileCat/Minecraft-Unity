using System;
using Minecraft.Scripts.Lua;

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