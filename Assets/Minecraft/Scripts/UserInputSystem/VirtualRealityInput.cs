using UnityEngine;
using Valve.VR;

namespace Minecraft.Scripts.UserInputSystem
{
  public class VirtualRealityInput : MonoBehaviour, IUserInput
  {
    public Vector2 AxisVector => SteamVR_Actions.default_TouchPad.axis;
  }
}