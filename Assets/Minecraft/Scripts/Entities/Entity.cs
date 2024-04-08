#region

using System;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.PhysicSystem;
using Minecraft.XLua.Src;
using UnityEngine;
using UnityEngine.Events;
using PhysicMaterial = Minecraft.Scripts.PhysicSystem.PhysicMaterial;
using Physics = Minecraft.Scripts.PhysicSystem.Physics;

#endregion

namespace Minecraft.Scripts.Entities
{
  [LuaCallCSharp]
  [DisallowMultipleComponent]
  [RequireComponent(typeof(Transform))]
  public abstract class Entity : MonoBehaviour, IAABBEntity
  {
    [SerializeField] [Min(0.1f)] private float m_Mass = 10;
    [SerializeField] private AABB m_BoundingBox = new(Vector3.zero, Vector3.one);
    [SerializeField] private float m_GravityMultiplier = 1;
    [SerializeField] private bool m_UseGravity = true;
    [SerializeField] private PhysicMaterial m_Material;

    [Space] [SerializeField] private UnityEvent<CollisionFlags> m_OnCollisions;


    private Vector3 m_Velocity;
    private Vector3 m_AddedForce;
    [NonSerialized] protected Transform m_Transform;


    [field: NonSerialized] public bool Initialized { get; private set; } = false;

    public float Mass
    {
      get => m_Mass;
      set => m_Mass = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }

    public AABB BoundingBox
    {
      get => m_BoundingBox;
      set => m_BoundingBox = value;
    }

    public float GravityMultiplier
    {
      get => m_GravityMultiplier;
      set => m_GravityMultiplier = value;
    }

    public bool UseGravity
    {
      get => m_UseGravity;
      set => m_UseGravity = value;
    }

    public PhysicMaterial PhysicMaterial
    {
      get => m_Material;
      set => m_Material = value;
    }

    public bool IsGrounded => GetIsGrounded(out _);

    public Vector3 Velocity => m_Velocity;

    public Vector3 Position
    {
      get => m_Transform.position;
      set => m_Transform.position = value;
    }

    public Vector3 LocalPosition
    {
      get => m_Transform.localPosition;
      set => m_Transform.localPosition = value;
    }

    public Vector3 Forward => m_Transform.forward;

    [field: NonSerialized] public IWorld World { get; private set; }

    public event UnityAction<CollisionFlags> OnCollisions
    {
      add => m_OnCollisions.AddListener(value);
      remove => m_OnCollisions.RemoveListener(value);
    }


    protected virtual void Start()
    {
      InitializeEntityIfNot();
    }

    protected virtual void FixedUpdate()
    {
      var time = Time.fixedDeltaTime;

      var acceleration = m_AddedForce / m_Mass;
      m_AddedForce = Vector3.zero;

      if (m_UseGravity)
      {
        acceleration += Physics.Gravity * GravityMultiplier;
      }

      // 用匀速直线运动近似
      var averageVelocity = m_Velocity + time * acceleration;
      Move(averageVelocity, time);
    }

    public void InitializeEntityIfNot()
    {
      if (!Initialized)
      {
        World = Scripts.World.Active;
        m_Velocity = Vector3.zero;
        m_AddedForce = Vector3.zero;
        m_Transform = GetComponent<Transform>();
        m_OnCollisions ??= new UnityEvent<CollisionFlags>();
        OnInitialize();

        Initialized = true;
      }
    }

    protected virtual void OnInitialize()
    {
    }

    public virtual void OnRecycle()
    {
      // m_OnCollisions.RemoveAllListeners();
    }

    private void Move(Vector3 velocity, float time)
    {
      m_Velocity = velocity;

      if (m_Velocity == Vector3.zero || time <= 0)
      {
        return;
      }

      var position = m_Transform.position;
      var flags = Physics.CollideWithTerrainAABB(
        position,
        m_BoundingBox,
        World,
        m_Material,
        time,
        ref m_Velocity,
        out var movement
      );

      m_Transform.position = position + movement;

      if (flags != CollisionFlags.None)
      {
        m_OnCollisions.Invoke(flags);
      }
    }

    public bool GetIsGrounded(out BlockData groundBlock)
    {
      return Physics.CheckGroundedAABB(m_Transform.position, m_BoundingBox, World, out groundBlock);
    }

    public void AddInstantForce(Vector3 force)
    {
      m_AddedForce += force;
    }
  }
}