#region

using System;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.InspectorExtensions
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public sealed class ConditionalDisplayAttribute : PropertyAttribute
  {
    public string ConditionField { get; }

    private ConditionalDisplayAttribute()
    {
    }

    public ConditionalDisplayAttribute(string conditionField)
    {
      ConditionField = conditionField;
    }
  }
}