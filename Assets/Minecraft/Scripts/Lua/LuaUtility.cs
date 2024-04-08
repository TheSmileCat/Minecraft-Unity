#region

using Minecraft.XLua.Src;
using UnityEngine;

#endregion

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