#region

using UnityEngine;

#endregion

namespace Minecraft.Scripts
{
  [RequireComponent(typeof(Light))]
  public class DayNightCycle : MonoBehaviour
  {
    [SerializeField] private Vector3 m_EulerRotation;

    private Transform m_Transform;

    private void Start()
    {
      m_Transform = GetComponent<Transform>();
    }

    private void Update()
    {
      m_Transform.Rotate(m_EulerRotation * Time.deltaTime);
    }
  }
}