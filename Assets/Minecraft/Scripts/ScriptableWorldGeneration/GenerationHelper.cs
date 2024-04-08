#region

using Minecraft.Scripts.Lua;
using Minecraft.Scripts.Noises;
using Minecraft.Scripts.ScriptableWorldGeneration.GenLayers;

#endregion

namespace Minecraft.Scripts.ScriptableWorldGeneration
{
  public class GenerationHelper : ILuaCallCSharp
  {
    public int Seed;

    public GenericNoise<PerlinNoise> DepthNoise;
    public GenericNoise<PerlinNoise> MainNoise;
    public GenericNoise<PerlinNoise> MaxNoise;
    public GenericNoise<PerlinNoise> MinNoise;
    public GenericNoise<PerlinNoise> SurfaceNoise;

    public float[,] BiomeWeights;
    public StatelessGenLayer GenLayers;
  }
}