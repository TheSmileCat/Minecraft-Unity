using Minecraft.Scripts.Audio;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Entities;
using Minecraft.Scripts.Lua;
using Minecraft.Scripts.Rendering;
using Minecraft.Scripts.ScriptableWorldGeneration;
using UnityEngine;

namespace Minecraft.Scripts
{
    public interface IWorld : ILuaCallCSharp, IHotfixable
    {
        bool Initialized { get; }

        IWorldRWAccessor RWAccessor { get; }

        Transform PlayerTransform { get; }

        Camera MainCamera { get; }

        AudioManager AudioManager { get; }

        LuaManager LuaManager { get; }

        ChunkManager ChunkManager { get; }

        SectionRenderingManager RenderingManager { get; }

        EntityManager EntityManager { get; }

        BlockTable BlockDataTable { get; }

        BiomeTable BiomeDataTable { get; }

        WorldGeneratePipeline WorldGenPipeline { get; }

        int MaxTickBlockCountPerFrame { get; set; }

        int MaxLightBlockCountPerFrame { get; set; }

        void LightBlock(int x, int y, int z, ModificationSource source);

        void TickBlock(int x, int y, int z);

        void MarkBlockMeshDirty(int x, int y, int z, ModificationSource source);
    }
}
