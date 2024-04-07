using Minecraft.XLua.Src;

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
