using Minecraft.Scripts.Configurations;
using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts.PhysicSystem
{
    [LuaCallCSharp]
    public static class BlockPhysicsUtility
    {
        public static AABB? GetBoundingBox(this BlockData block, int x, int y, int z, IWorld world, bool checkCollisionFlags)
        {
            return GetBoundingBox(block, new Vector3Int(x, y, z), world, checkCollisionFlags);
        }

        public static AABB? GetBoundingBox(this BlockData block, Vector3Int position, IWorld world, bool checkCollisionFlags)
        {
            if (block == null || (checkCollisionFlags && block.HasFlag(BlockFlags.IgnoreCollisions)))
            {
                return null;
            }

            BlockMesh mesh = world.BlockDataTable.GetMesh(block.Mesh.Value);
            Quaternion rotation = world.RWAccessor.GetBlockRotation(position.x, position.y, position.z, Quaternion.identity);
            return AABB.Rotate(mesh.BoundingBox, rotation, mesh.Pivot) + position;
        }
    }
}