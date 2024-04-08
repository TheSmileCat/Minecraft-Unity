#region

using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Lua;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration
{
  public abstract class StatelessGenerator : ScriptableObject, ILuaCallCSharp
  {
    public abstract void Generate(IWorld world, ChunkPos pos, BlockData[,,] blocks, Quaternion[,,] rotations,
      byte[,] heightMap, GenerationHelper helper, GenerationContext context);
  }
}