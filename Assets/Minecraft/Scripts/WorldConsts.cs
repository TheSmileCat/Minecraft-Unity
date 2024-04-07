using Minecraft.XLua.Src;

namespace Minecraft.Scripts
{
    [LuaCallCSharp]
    public static class WorldConsts
    {
        public const int ChunkHeight = 256;

        public const int ChunkWidth = 16;

        public const int BlockCountInChunk = ChunkWidth * ChunkHeight * ChunkWidth;
    }
}