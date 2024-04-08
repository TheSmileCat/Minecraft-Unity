#region

using Minecraft.Scripts.Configurations;
using UnityEngine;

#endregion

namespace Minecraft.Scripts
{
  public interface IWorldRWAccessor : IWorldRAccessor
  {
    bool SetBlock(int x, int y, int z, BlockData value, Quaternion rotation, ModificationSource source);

    bool SetAmbientLightLevel(int x, int y, int z, int value, ModificationSource source);
  }
}