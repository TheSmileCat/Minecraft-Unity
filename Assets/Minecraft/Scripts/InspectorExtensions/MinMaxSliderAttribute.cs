#region

using System;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.InspectorExtensions
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public sealed class MinMaxSliderAttribute : PropertyAttribute
  {
    public float Min { get; }

    public float Max { get; }

    private MinMaxSliderAttribute()
    {
    }

    public MinMaxSliderAttribute(float min, float max)
    {
      Min = min;
      Max = max;
    }
  }
}