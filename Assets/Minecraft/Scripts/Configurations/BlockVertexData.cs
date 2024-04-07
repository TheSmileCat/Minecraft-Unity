using System;
using Minecraft.Scripts.Lua;
using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts.Configurations
{
    [Serializable]
    [GCOptimize]
    public struct BlockVertexData : ILuaCallCSharp
    {
        public Vector3 Position;
        public Vector2 UV;
        public BlockFaceCorner CornerInFace;
    }
}
