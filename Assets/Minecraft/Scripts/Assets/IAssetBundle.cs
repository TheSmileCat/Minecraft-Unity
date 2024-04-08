﻿#region

using System;
using Minecraft.Scripts.Lua;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Minecraft.Scripts.Assets
{
  public interface IAssetBundle : ILuaCallCSharp
  {
    string Name { get; }

    bool IsStreamedSceneAssetBundle { get; }

    IAssetBundle[] Dependencies { get; }

    int RefCount { get; }

    bool IsLoadingDone { get; }

    void IncreaseRef();

    void DecreaseRef();

    bool UpdateLoadingState();

    AssetBundleRequest LoadAsset<T>(string name) where T : Object;

    AssetBundleRequest LoadAsset(string name, Type type);

    void Unload(bool unloadAllLoadedObjects);
  }
}