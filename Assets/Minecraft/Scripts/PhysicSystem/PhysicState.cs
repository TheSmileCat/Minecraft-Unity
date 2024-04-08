#region

using Minecraft.XLua.Src;

#endregion

namespace Minecraft.Scripts.PhysicSystem
{
  [GCOptimize]
  [LuaCallCSharp]
  public enum PhysicState
  {
    /// <summary>
    /// 固体
    /// </summary>
    Solid,

    /// <summary>
    /// 流体
    /// </summary>
    Fluid
  }
}