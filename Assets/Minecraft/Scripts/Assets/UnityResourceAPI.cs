#region

using System;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Minecraft.Scripts.Assets
{
  internal class UnityResourceAPI : ResourcesAPI
  {
    // TODO

    protected override Object[] FindObjectsOfTypeAll(Type systemTypeInstance)
    {
      return base.FindObjectsOfTypeAll(systemTypeInstance);
    }

    protected override Shader FindShaderByName(string name)
    {
      return base.FindShaderByName(name);
    }

    protected override Object Load(string path, Type systemTypeInstance)
    {
      return base.Load(path, systemTypeInstance);
    }

    protected override Object[] LoadAll(string path, Type systemTypeInstance)
    {
      return base.LoadAll(path, systemTypeInstance);
    }

    protected override ResourceRequest LoadAsync(string path, Type systemTypeInstance)
    {
      return base.LoadAsync(path, systemTypeInstance);
    }

    protected override void UnloadAsset(Object assetToUnload)
    {
      base.UnloadAsset(assetToUnload);
    }
  }
}