#region

using System;
using Minecraft.Scripts.Lua;
using Minecraft.Scripts.PhysicSystem;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.Configurations
{
  [CreateAssetMenu(menuName = "Minecraft/Configurations/BlockMesh")]
  public class BlockMesh : ScriptableObject, ILuaCallCSharp
  {
    [Serializable]
    public struct FaceData
    {
      public BlockFace Face;
      public bool NeverClip;
      public BlockVertexData[] Vertices;
      public int[] Indices;
    }

    public Vector3 Pivot;
    public AABB BoundingBox;
    public FaceData[] Faces;
  }
}