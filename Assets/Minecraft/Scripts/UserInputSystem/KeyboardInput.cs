﻿#region

using System;
using System.Threading.Tasks;
using Minecraft.Scripts.Configurations;
using Minecraft.Scripts.Entities;
using Minecraft.Scripts.PlayerControls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#endregion

namespace Minecraft.Scripts.UserInputSystem
{
  [DisallowMultipleComponent]
  [RequireComponent(typeof(BlockInteraction))]
  [RequireComponent(typeof(FluidInteractor))]
  public class KeyboardInput : MonoBehaviour, IUserInput
  {
    // ...fields of entity class

    [Space] [Header("Input")] [SerializeField]
    private InputActionAsset m_InputActions;

    [Space] [SerializeField] private FirstPersonLook m_FirstPersonLook;
    [SerializeField] private FOVKick m_FOVKick;
    [SerializeField] private CurveControlledBob m_HeadBob;
    [SerializeField] private LerpControlledBob m_JumpBob;

    [Space] [Header("Events")] [SerializeField]
    private UnityEvent<BlockData> m_OnStepOnBlock;


    [NonSerialized] private InputAction m_MoveAction;

    // [NonSerialized] private InputAction m_RunAction;
    [NonSerialized] private InputAction m_LookAction;
    [NonSerialized] private InputAction m_JumpAction;
    [NonSerialized] private InputAction m_FlyAction;
    [NonSerialized] private InputAction m_FlyDownAction;
    [NonSerialized] private InputAction m_CursorStateAction;

    private float WalkSpeed => m_PlayerEntity.WalkSpeed;
    private float RunSpeed => m_PlayerEntity.RunSpeed;
    private float FlyUpSpeed => m_PlayerEntity.FlyUpSpeed;
    private float JumpHeight => m_PlayerEntity.JumpHeight;
    private float StepInterval => m_PlayerEntity.StepInterval;
    private float RunstepLengthen => m_PlayerEntity.RunstepLengthen;

    private Camera m_Camera;
    private Transform m_CameraTransform;
    private FluidInteractor m_FluidInteractor;

    private PlayerEntity m_PlayerEntity;
    private Vector3 m_OriginalCameraPosition;
    private bool m_Jump;
    private bool m_FlyDown;
    private bool m_IsRunning;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private Transform m_Transform;

    private float m_LastTimePressW;

    private readonly TaskCompletionSource<bool> m_StartTask = new();

    public Task<bool> StartTask => m_StartTask.Task;

    private void Start()
    {
      m_Transform = transform;

      m_InputActions.Enable();
      m_MoveAction = m_InputActions["Player/Move"];
      // m_RunAction = m_InputActions["Player/Run"];
      m_LookAction = m_InputActions["Player/Look"];
      m_JumpAction = m_InputActions["Player/Jump"];
      m_FlyAction = m_InputActions["Player/Fly"];
      m_FlyDownAction = m_InputActions["Player/Fly Down"];
      m_CursorStateAction = m_InputActions["Player/Cursor State"];

      m_Camera = GetComponentInChildren<Camera>();
      m_CameraTransform = m_Camera.GetComponent<Transform>();
      m_FluidInteractor = GetComponent<FluidInteractor>();
      m_PlayerEntity = GetComponent<PlayerEntity>();

      m_FirstPersonLook.Initialize(m_Transform, m_CameraTransform, true);
      m_HeadBob.Initialize(m_CameraTransform);
      m_JumpBob.Initialize();

      m_OriginalCameraPosition = m_CameraTransform.localPosition;
      m_StepCycle = 0f;
      m_NextStep = m_StepCycle * 0.5f;

      m_LastTimePressW = 0f;

      // m_RunAction.performed += SwitchRunMode;
      m_JumpAction.performed += SwitchJumpMode;
      m_FlyAction.performed += SwitchFlyMode;
      m_FlyDownAction.performed += SwitchFlyDownMode;
      m_CursorStateAction.performed += SwitchCursorState;
      m_StartTask.SetResult(true);
    }

    private void SwitchJumpMode(InputAction.CallbackContext context)
    {
      m_Jump = context.ReadValueAsButton();
    }

    // private void SwitchRunMode(InputAction.CallbackContext context)
    // {
    //     m_IsRunning = true;
    // }

    private void SwitchFlyMode(InputAction.CallbackContext context)
    {
      m_PlayerEntity.UseGravity = !m_PlayerEntity.UseGravity;
    }

    private void SwitchFlyDownMode(InputAction.CallbackContext context)
    {
      m_FlyDown = context.ReadValueAsButton();
    }

    private void SwitchCursorState(InputAction.CallbackContext context)
    {
      var value = context.ReadValueAsButton();
      m_FirstPersonLook.SetCursorLockMode(!value);
    }

    private void Update()
    {
      if (!m_PlayerEntity.enabled) return;
      // 在 Update 里读取输入。
      // 如果在 FixedUpdate 里读输入会出现丢失，
      // 因为 FixedUpdate 不是按实际帧率执行
      SwitchWalkAndRunMode();

      m_FirstPersonLook.LookRotation(m_LookAction.ReadValue<Vector2>(), Time.deltaTime);

      var isGrounded = m_PlayerEntity.GetIsGrounded(out var groundBlock);

      if (!m_PreviouslyGrounded && isGrounded)
      {
        StartCoroutine(m_JumpBob.DoBobCycle());
        PlayBlockStepSound(groundBlock);

        m_NextStep = m_StepCycle + 0.5f;
      }

      m_PreviouslyGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
      if (!m_PlayerEntity.enabled) return;
      var speed = GetInput(out var input);
      // 运动受流体影响。
      m_FluidInteractor.UpdateState(m_PlayerEntity, m_CameraTransform, out var vMultiplier);

      // always move along the camera forward as it is the direction that it being aimed at
      var velocity = m_Transform.forward * input.y + m_Transform.right * input.x;
      velocity = speed * vMultiplier * velocity.normalized;

      if (m_PlayerEntity.UseGravity)
      {
        velocity.y = m_PlayerEntity.Velocity.y; // 不管 y 方向

        var isGrounded = m_PlayerEntity.GetIsGrounded(out var groundBlock);

        if (isGrounded && m_Jump)
        {
          // 向上跳起
          m_PlayerEntity.AddInstantForce(new Vector3(0, JumpHeight * m_PlayerEntity.Mass / Time.fixedDeltaTime, 0));
          PlayBlockStepSound(groundBlock);
          // m_Jump = false;
        }

        ProgressStepCycle(input, speed, isGrounded, groundBlock);
        UpdateCameraPosition(speed, isGrounded);
      }
      else if (m_FlyDown)
      {
        velocity.y = -FlyUpSpeed; // 向下飞
      }
      else if (m_Jump)
      {
        velocity.y = FlyUpSpeed; // 向上飞
      }
      else
      {
        velocity.y = 0; // 停在空中
      }

      // 施加力来调整速度
      m_PlayerEntity.AddInstantForce((velocity - m_PlayerEntity.Velocity) * m_PlayerEntity.Mass / Time.fixedDeltaTime);
    }

    private void ProgressStepCycle(Vector2 input, float speed, bool isGrounded, BlockData blockUnderFeet)
    {
      var velocity = m_PlayerEntity.Velocity;

      if (velocity.sqrMagnitude > 0 && input != Vector2.zero)
      {
        m_StepCycle += (velocity.magnitude + (speed * (m_IsRunning ? RunstepLengthen : 1f))) * Time.fixedDeltaTime;
      }

      if (m_StepCycle <= m_NextStep)
      {
        return;
      }

      m_NextStep = m_StepCycle + StepInterval;

      if (isGrounded)
      {
        PlayBlockStepSound(blockUnderFeet);
      }
    }

    private void UpdateCameraPosition(float speed, bool isGrounded)
    {
      Vector3 newCameraPosition;
      var velocity = m_PlayerEntity.Velocity;

      if (!m_HeadBob.Enabled)
      {
        return;
      }

      if (velocity.sqrMagnitude > 0 && isGrounded)
      {
        m_CameraTransform.localPosition = m_HeadBob.DoHeadBob(
          velocity.magnitude + (speed * (m_IsRunning ? RunstepLengthen : 1f)), StepInterval, Time.fixedDeltaTime);
        newCameraPosition = m_CameraTransform.localPosition;
        newCameraPosition.y = m_CameraTransform.localPosition.y - m_JumpBob.GetOffset();
      }
      else
      {
        newCameraPosition = m_CameraTransform.localPosition;
        newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.GetOffset();
      }

      m_CameraTransform.localPosition = newCameraPosition;
    }

    private void SwitchWalkAndRunMode()
    {
      // TODO: 如何用 Input System 实现这个功能？
      if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
      {
        var currentTime = Time.time;

        if (currentTime - m_LastTimePressW <= 0.2f)
        {
          // 此时认为玩家双击了 W 或者 向上箭头
          m_IsRunning = true;
        }

        m_LastTimePressW = currentTime;
      }
    }

    private float GetInput(out Vector2 input)
    {
      input = m_MoveAction.ReadValue<Vector2>();

      if (input == Vector2.zero)
      {
        // 一旦玩家停止移动，结束跑步状态
        m_IsRunning = false;
      }

      return m_IsRunning ? RunSpeed : WalkSpeed;
    }

    private void PlayBlockStepSound(BlockData block)
    {
      //block.PlayStepAutio(m_AudioSource);
    }
  }
}