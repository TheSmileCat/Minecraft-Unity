#region

using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Lua;
using Minecraft.Scripts.PhysicSystem;
using Minecraft.XLua.Src;
using UnityEngine;
using UnityEngine.Events;
using PhysicMaterial = Minecraft.Scripts.PhysicSystem.PhysicMaterial;

#endregion

namespace Minecraft.Scripts.Entities
{
  [LuaCallCSharp]
  public interface IAABBEntity : ILuaCallCSharp
  {
    bool Initialized { get; }

    float Mass { get; set; }

    AABB BoundingBox { get; set; }

    float GravityMultiplier { get; set; }

    bool UseGravity { get; set; }

    PhysicMaterial PhysicMaterial { get; }

    bool IsGrounded { get; }

    Vector3 Velocity { get; }

    Vector3 Position { get; set; }

    Vector3 LocalPosition { get; set; }

    Vector3 Forward { get; }

    IWorld World { get; }

    event UnityAction<CollisionFlags> OnCollisions;

    void InitializeEntityIfNot();

    void OnRecycle();

    bool GetIsGrounded(out BlockData groundBlock);

    void AddInstantForce(Vector3 force);
  }
}