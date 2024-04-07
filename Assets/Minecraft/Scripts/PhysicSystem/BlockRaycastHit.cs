using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Lua;
using UnityEngine;

namespace Minecraft.Scripts.PhysicSystem
{
    public readonly struct BlockRaycastHit : ILuaCallCSharp
    {
        public Vector3Int Position { get; }

        public Vector3 Normal { get; }

        public IWorld World { get; }

        public BlockData Block { get; }


        public BlockRaycastHit(Vector3Int pos, Vector3 normal, IWorld world, BlockData block)
        {
            Position = pos;
            Normal = normal;
            World = world;
            Block = block;
        }
    }
}
