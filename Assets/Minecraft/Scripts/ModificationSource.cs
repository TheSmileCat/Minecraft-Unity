#region

using Minecraft.XLua.Src;

#endregion

namespace Minecraft.Scripts
{
  [GCOptimize]
  [LuaCallCSharp]
  public enum ModificationSource
  {
    InternalOrSystem = 0,

    PlayerAction = 1
  }
}