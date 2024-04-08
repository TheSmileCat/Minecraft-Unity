#region

using Minecraft.XLua.Src;
using UnityEngine;
using UnityEngine.Rendering;

#endregion

namespace Minecraft.Scripts.Entities
{
  [LuaCallCSharp]
  public interface IRenderableEntity : IAABBEntity
  {
    bool EnableRendering { get; set; }

    Mesh SharedMesh { get; }

    Material SharedMaterial { get; }

    MaterialPropertyBlock MaterialProperty { get; }

    void Render(int layer, Camera camera, ShadowCastingMode castShadows, bool receiveShadows);
  }
}