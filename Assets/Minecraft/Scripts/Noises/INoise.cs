using Minecraft.Scripts.Lua;
using UnityEngine;

namespace Minecraft.Scripts.Noises
{
    public interface INoise : ILuaCallCSharp
    {
        float Noise(float x, float y, float z);

        void Noise(float[,,] noise, Vector3 offset, Vector3 scale);

        void Noise(float[,,] noise, Vector3 offset, Vector3 scale, float noiseScale, bool add);
    }
}
