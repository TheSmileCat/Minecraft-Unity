#if !(UNITY_EDITOR || LOAD_ASSET_BUNDLE_FROM_FILE)
#define LOAD_ASSET_BUNDLE_FROM_FILE
#endif

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Minecraft.Scripts.Lua;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#endregion

namespace Minecraft.Scripts.Assets
{
  public sealed class AssetManager : ILuaCallCSharp
  {
    public static AssetManager Instance { get; private set; }

    public static void InitializeIfNeeded(string assetBundleDirectory)
    {
      Instance ??= new AssetManager(assetBundleDirectory);
    }


    private readonly AssetCatalog m_Catalog;
    private readonly Dictionary<string, IAssetBundle> m_CreatedAssetBundles;
    private readonly Dictionary<string, AsyncAsset> m_CreatedAssets;
    private readonly List<IAssetBundle> m_LoadingAssetBundles;
    private readonly List<AsyncAsset> m_LoadingAssets;

    public string AssetBundleDirectory { get; }

    public bool EnableLog { get; set; }


    private AssetManager(string assetBundleDirectory)
    {
      AssetBundleDirectory = assetBundleDirectory;

#if LOAD_ASSET_BUNDLE_FROM_FILE
            string json = File.ReadAllText(Path.Combine(assetBundleDirectory, AssetCatalog.FileName));
            m_Catalog = JsonConvert.DeserializeObject<AssetCatalog>(json);
#else
      m_Catalog = AssetUtility.CreateEditorAssetCatalog();
#endif

      m_CreatedAssetBundles = new Dictionary<string, IAssetBundle>();
      m_CreatedAssets = new Dictionary<string, AsyncAsset>();
      m_LoadingAssetBundles = new List<IAssetBundle>();
      m_LoadingAssets = new List<AsyncAsset>();

#if LOAD_ASSET_BUNDLE_FROM_FILE
            LogMessage("Load asset from AssetBundle files.");
#else
      LogMessage("Load asset from AssetDatabase.");
#endif
    }

    public void Update()
    {
      for (var i = m_LoadingAssetBundles.Count - 1; i >= 0; --i)
      {
        var assetBundle = m_LoadingAssetBundles[i];

        if (!m_CreatedAssetBundles.ContainsKey(assetBundle.Name)) // unloaded
        {
          m_LoadingAssetBundles.RemoveAt(i);
        }
        else if (assetBundle.UpdateLoadingState())
        {
          LogMessage("AssetBundle: ", assetBundle.Name, " is loaded.");
          m_LoadingAssetBundles.RemoveAt(i);
        }
      }

      for (var i = m_LoadingAssets.Count - 1; i >= 0; --i)
      {
        var asset = m_LoadingAssets[i];

        if (!m_CreatedAssets.ContainsKey(asset.AssetName)) // unloaded
        {
          m_LoadingAssets.RemoveAt(i);
        }
        else if (asset.UpdateLoadingState())
        {
          LogMessage("Asset: ", asset.AssetName, " is loaded.");
          m_LoadingAssets.RemoveAt(i);
        }
      }
    }

    public void LogAssetCatalog()
    {
      LogMessage(JsonConvert.SerializeObject(m_Catalog, Formatting.None));
    }

    public IAssetBundle LoadAssetBundle(string name)
    {
      if (m_CreatedAssetBundles.TryGetValue(name, out var ab))
      {
        LogMessage("Load AssetBundle: '", name, "' from cache.");
        return ab;
      }

      if (!m_Catalog.AssetBundles.TryGetValue(name, out var info))
      {
        throw new FileNotFoundException("Can not find AssetBundle: " + name);
      }

#if LOAD_ASSET_BUNDLE_FROM_FILE
            ab = LoadAssetBundleFromFile(name, info);
#else
      ab = LoadAssetBundleInEditor(name, info);
#endif

      m_CreatedAssetBundles.Add(name, ab);
      m_LoadingAssetBundles.Add(ab);
      return ab;
    }

#if UNITY_EDITOR
    private IAssetBundle LoadAssetBundleInEditor(string name, AssetBundleInfo info)
    {
      var dependencies = LoadAssetBundleDependencies(info.Dependencies);
      IAssetBundle ab = new EditorAssetBundle(name, dependencies);
      LogMessage("Create Editor AssetBundle: '", name, "'.");
      return ab;
    }
#endif

    private IAssetBundle LoadAssetBundleFromFile(string name, AssetBundleInfo info)
    {
      var request = CreateAssetBundleRequest(info.FileName);
      var dependencies = LoadAssetBundleDependencies(info.Dependencies);
      IAssetBundle ab = new RuntimeAssetBundle(name, request, dependencies);
      LogMessage("Create Runtime AssetBundle: '", name, "'.");
      return ab;
    }

    private IAssetBundle[] LoadAssetBundleDependencies(string[] dependencyNames)
    {
      var dependencies = new IAssetBundle[dependencyNames.Length];

      for (var i = 0; i < dependencies.Length; i++)
      {
        dependencies[i] = LoadAssetBundle(dependencyNames[i]);
        dependencies[i].IncreaseRef();
      }

      return dependencies;
    }

    private AssetBundleCreateRequest CreateAssetBundleRequest(string fileName)
    {
      var filePath = Path.Combine(AssetBundleDirectory, fileName);
      return AssetBundle.LoadFromFileAsync(filePath);
    }


    public void UnloadAssetBundle(IAssetBundle assetBundle)
    {
      UnloadAssetBundlePrivate(assetBundle, assetBundle.RefCount <= 0);
    }

    private void UnloadAssetBundlePrivate(IAssetBundle assetBundle, bool unloadAllLoadedObjects)
    {
      if (!m_CreatedAssetBundles.Remove(assetBundle.Name))
      {
        Debug.LogWarning($"AssetBundle '{assetBundle.Name}' is not loaded, you can not unload it.");
        return;
      }

      assetBundle.Unload(unloadAllLoadedObjects);
      LogMessage("Unload AssetBundle: '", assetBundle.Name, "', unloadAllLoadedObjects: ", unloadAllLoadedObjects);

      for (var i = 0; i < assetBundle.Dependencies.Length; i++)
      {
        var dep = assetBundle.Dependencies[i];
        dep.DecreaseRef();

        if (dep.RefCount <= 0)
        {
          UnloadAssetBundlePrivate(dep, unloadAllLoadedObjects);
        }
      }
    }


    private AsyncAsset CreateAssetFromAssetBundle(string name, Type type, IAssetBundle assetBundle)
    {
      var asset = new AsyncAsset();
      asset.Initialize(name, type, assetBundle);
      assetBundle.IncreaseRef();

      m_CreatedAssets.Add(name, asset);
      m_LoadingAssets.Add(asset);

      LogMessage("Create Asset: '", name, "'.");
      return asset;
    }

    public AsyncAsset LoadAsset(string name, Type type)
    {
      if (!m_Catalog.Assets.TryGetValue(name, out var assetInfo))
      {
        throw new FileNotFoundException("Can not find Asset: " + name);
      }

      name = assetInfo.AssetName;

      if (m_CreatedAssets.TryGetValue(name, out var asset))
      {
        LogMessage("Load Asset: '", name, "' from cache.");
      }
      else
      {
        var assetBundle = LoadAssetBundle(assetInfo.AssetBundleName);
        asset = CreateAssetFromAssetBundle(name, type, assetBundle);
      }

      return asset;
    }

    public AsyncAsset LoadAsset<T>(string name) where T : Object
    {
      return LoadAsset(name, typeof(T));
    }

    public AsyncAsset LoadAsset(AssetPtr ptr, Type type)
    {
      return LoadAsset(ptr.AssetGUID, type);
    }

    public AsyncAsset LoadAsset<T>(AssetPtr ptr) where T : Object
    {
      return LoadAsset<T>(ptr.AssetGUID);
    }

    public AsyncAsset[] LoadAssets(Type type, params string[] names)
    {
      var results = new AsyncAsset[names.Length];

      for (var i = 0; i < results.Length; i++)
      {
        results[i] = LoadAsset(names[i], type);
      }

      return results;
    }

    public AsyncAsset[] LoadAssets(Type type, params AssetPtr[] ptrs)
    {
      var results = new AsyncAsset[ptrs.Length];

      for (var i = 0; i < results.Length; i++)
      {
        results[i] = LoadAsset(ptrs[i], type);
      }

      return results;
    }

    public AsyncAsset[] LoadAssets<T>(params string[] names) where T : Object
    {
      return LoadAssets(typeof(T), names);
    }

    public AsyncAsset[] LoadAssets<T>(params AssetPtr[] ptrs) where T : Object
    {
      return LoadAssets(typeof(T), ptrs);
    }

    public AsyncAsset[] LoadAllAssets(string assetBundleName)
    {
      var assetBundle = LoadAssetBundle(assetBundleName);
      var assetNames = m_Catalog.AssetBundles[assetBundleName].Assets;
      var assets = new AsyncAsset[assetNames.Length];

      for (var i = 0; i < assetNames.Length; i++)
      {
        var assetName = assetNames[i];

        if (m_CreatedAssets.TryGetValue(assetName, out var asset))
        {
          LogMessage("Load Asset: '", assetName, "' from cache.");
        }
        else
        {
          asset = CreateAssetFromAssetBundle(assetName, typeof(Object), assetBundle);
        }

        assets[i] = asset;
      }

      return assets;
    }

    public void UnloadAsset(string name)
    {
      if (!m_Catalog.Assets.TryGetValue(name, out var assetInfo))
      {
        throw new FileNotFoundException("Can not find Asset: " + name);
      }

      if (m_CreatedAssets.TryGetValue(assetInfo.AssetName, out var asset))
      {
        UnloadAsset(asset);
      }
      else
      {
        Debug.LogWarning($"Asset '{assetInfo.AssetName}' is not loaded, you can not unload it.");
      }
    }

    public void UnloadAsset(AssetPtr ptr)
    {
      UnloadAsset(ptr.AssetGUID);
    }

    public void UnloadAsset(AsyncAsset asset)
    {
      if (!m_CreatedAssets.Remove(asset.AssetName))
      {
        Debug.LogWarning($"Asset '{asset.AssetName}' is not loaded, you can not unload it.");
        return;
      }

      asset.Unload();
      asset.AssetBundle.DecreaseRef();
      LogMessage("Unload Asset: ", asset.AssetName);

      // TODO: 延时卸载 AssetBundle
      if (asset.AssetBundle.RefCount == 0)
      {
        UnloadAssetBundle(asset.AssetBundle);
      }
    }

    public void UnloadAssets(params string[] names)
    {
      for (var i = 0; i < names.Length; i++)
      {
        UnloadAsset(names[i]);
      }
    }

    public void UnloadAssets(params AssetPtr[] ptrs)
    {
      for (var i = 0; i < ptrs.Length; i++)
      {
        UnloadAsset(ptrs[i]);
      }
    }

    public void UnloadAssets(params AsyncAsset[] assets)
    {
      for (var i = 0; i < assets.Length; i++)
      {
        UnloadAsset(assets[i]);
      }
    }

    public void UnloadAll()
    {
      foreach (var asset in m_CreatedAssets.Values)
      {
        asset.Unload();
        LogMessage("Unload Asset: ", asset.AssetName);
      }

      foreach (var assetBundle in m_CreatedAssetBundles.Values)
      {
        assetBundle.Unload(true);
        LogMessage("Force unload AssetBundle: ", assetBundle.Name);
      }

      m_CreatedAssetBundles.Clear();
      m_LoadingAssetBundles.Clear();
      m_CreatedAssets.Clear();
      m_LoadingAssets.Clear();
    }


    [Conditional("UNITY_EDITOR")]
    private void LogMessage(params object[] args)
    {
      if (EnableLog)
      {
        Debug.Log("[AssetManager] " + string.Join(string.Empty, args));
      }
    }
  }
}