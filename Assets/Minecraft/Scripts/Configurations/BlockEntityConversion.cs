using Minecraft.XLua.Src;

namespace Minecraft.Scripts.Configurations
{
    [GCOptimize]
    [LuaCallCSharp]
    public enum BlockEntityConversion
    {
        Never = 0,
        Initial = 1,
        Conditional = 2
    }
}
