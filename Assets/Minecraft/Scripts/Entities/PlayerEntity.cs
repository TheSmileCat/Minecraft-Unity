using System;
using Minecraft.Scripts.PlayerControls;
using UnityEngine;
using System.Threading.Tasks;
using Minecraft.Scripts.UserInputSystem;

namespace Minecraft.Scripts.Entities
{
  
  [DisallowMultipleComponent]
  [RequireComponent(typeof(KeyboardInput))]
  [RequireComponent(typeof(VirtualRealityInput))]
  public class PlayerEntity : Entity
  {
    private Camera m_Camera;
    
    [Space]
    [Header("Config")]
    public float WalkSpeed;
    public float RunSpeed;
    public float FlyUpSpeed;
    public float JumpHeight;
    [SerializeField] private float m_StepInterval;
    [SerializeField] [Range(0, 1)] private float m_RunstepLengthen;

    public float StepInterval => m_StepInterval;
    public float RunstepLengthen => m_RunstepLengthen;

    private KeyboardInput m_KeyboardInput;
    private VirtualRealityInput m_VirtualRealityInput;

    protected override void Start()
    {
      base.Start();

      m_Camera = GetComponentInChildren<Camera>();
      m_KeyboardInput = GetComponent<KeyboardInput>();
      m_VirtualRealityInput = GetComponent<VirtualRealityInput>();
      
      InteractionInit();
    }

    private async void InteractionInit()
    {
      Debug.Log("===> m_KeyboardInput.StartTask.Task!");
      
      await Task.WhenAll(m_KeyboardInput.StartTask, m_VirtualRealityInput.StartTask);
      
      Debug.Log("<=== m_KeyboardInput.StartTask.Task!");
      
      var interaction = GetComponent<BlockInteraction>();
      interaction.Initialize(m_Camera, this);
      interaction.enabled = true;
    }
  }
}