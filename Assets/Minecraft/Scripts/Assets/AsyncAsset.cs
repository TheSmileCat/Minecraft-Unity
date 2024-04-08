#region

using System;
using System.Collections;
using System.Collections.Generic;
using Minecraft.Scripts.Lua;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Minecraft.Scripts.Assets
{
  public class AsyncAsset : ILuaCallCSharp, IEnumerator // CustomYieldInstruction
  {
    private Type m_AssetType;
    private AssetBundleRequest m_Request;
    private Object m_Asset;


    public bool IsDone { get; private set; }

    public float Progress
    {
      get
      {
        if (IsDone)
        {
          return 1;
        }

        if (!AssetBundle.IsLoadingDone)
        {
          return 0;
        }

        return (1 + m_Request.progress) * 0.5f;
      }
    }

    public string AssetName { get; private set; }

    public IAssetBundle AssetBundle { get; private set; }

    public Object Asset => IsDone ? m_Asset : throw new InvalidOperationException();

    public void Initialize(string name, Type type, IAssetBundle assetBundle)
    {
      AssetName = name;
      m_AssetType = type;
      AssetBundle = assetBundle;
      m_Request = null;
      m_Asset = null;
      IsDone = false;
    }

    public void Initialize(string name, Object asset, IAssetBundle assetBundle)
    {
      AssetName = name;
      m_AssetType = null;
      AssetBundle = assetBundle;
      m_Request = null;
      m_Asset = asset;
      IsDone = true;
    }

    public T GetAssetAs<T>() where T : Object
    {
      return Asset as T;
    }

    public bool UpdateLoadingState()
    {
      if (IsDone)
      {
        return true;
      }

      if (!AssetBundle.IsLoadingDone)
      {
        return false;
      }

      m_Request ??= AssetBundle.LoadAsset(AssetName, m_AssetType);

#if UNITY_EDITOR
      if (!(m_Request is EditorAssetBundle.EditorAssetBundleRequest) && !m_Request.isDone)
      {
        return false;
      }
#else
            if (!m_Request.isDone)
            {
                return false;
            }
#endif

      m_Asset = m_Request.asset;
      m_AssetType = null;
      m_Request = null;
      IsDone = true;
      return true;
    }

    public void Unload()
    {
      m_Asset = null;
    }


    object IEnumerator.Current => default;

    bool IEnumerator.MoveNext() => !IsDone;

    void IEnumerator.Reset()
    {
    }


    public static IEnumerator WaitAll(params AsyncAsset[] assets)
    {
      return WaitAll((IReadOnlyList<AsyncAsset>)assets);
    }

    public static IEnumerator WaitAll(IReadOnlyList<AsyncAsset> assets)
    {
      for (var i = 0; i < assets.Count; i++)
      {
        do
        {
          yield return null;
        } while (!assets[i].IsDone);
      }
    }

    public static IEnumerator WaitAll<T>(T[] assetReferences, IReadOnlyList<AsyncAsset> assets) where T : Object
    {
      for (var i = 0; i < assets.Count; i++)
      {
        do
        {
          yield return null;
        } while (!assets[i].IsDone);

        assetReferences[i] = assets[i].GetAssetAs<T>();
      }
    }
  }
}