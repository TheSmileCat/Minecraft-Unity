using System;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace Minecraft.Scripts.UserInputSystem
{
  public class VirtualRealityInput : MonoBehaviour, IUserInput
  {
    public Vector2 AxisVector => SteamVR_Actions.default_TouchPad.axis;
    private readonly TaskCompletionSource<bool> m_StartTask = new();

    public Task<bool> StartTask => m_StartTask.Task;

    private void Start()
    {
      m_StartTask.SetResult(true);
    }
  }
}