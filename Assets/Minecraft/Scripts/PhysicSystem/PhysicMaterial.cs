using System;
using Minecraft.Scripts.Lua;
using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts.PhysicSystem
{
    [Serializable]
    [GCOptimize]
    public struct PhysicMaterial : ILuaCallCSharp
    {
        [Range(0, 1)] public float CoefficientOfRestitution; // e
        public float CoefficientOfStaticFriction;
        public float CoefficientOfDynamicFriction;

        public static float CombineValue(float left, float right)
        {
            return Mathf.Max(left, right); // 这里直接用 Max 了，PhysicMaterialCombine 太麻烦了
        }
    }
}
