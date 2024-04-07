using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts.Lua
{
    [LuaCallCSharp]
    public static class LuaUtility
    {
        public static ParticleSystem GetParticleSystem(GameObject go)
        {
            return go.GetComponent<ParticleSystem>();
        }
    }
}
