using Minecraft.Scripts.Entities;
using Minecraft.Scripts.Lua;
using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts.Configurations
{
    public interface IBlockBehaviour : ICSharpCallLua
    {
        void init(IWorld world, BlockData block);

        void tick(int x, int y, int z);

        void place(int x, int y, int z);

        void destroy(int x, int y, int z);

        void click(int x, int y, int z);

        void entity_init(IAABBEntity entity, LuaTable context);

        void entity_destroy(IAABBEntity entity, LuaTable context);

        void entity_update(IAABBEntity entity, LuaTable context);

        void entity_fixed_update(IAABBEntity entity, LuaTable context);

        void entity_on_collisions(IAABBEntity entity, CollisionFlags flags, LuaTable context);
    }
}
