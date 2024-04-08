#region

using System.Collections.Generic;
using UnityEditor;

#endregion

namespace Minecraft.Scripts.Assets
{
  public static class AssetUtility
  {
#if UNITY_EDITOR
    public static AssetCatalog CreateEditorAssetCatalog()
    {
      var assets = new Dictionary<string, AssetInfo>();
      var assetBundles = new Dictionary<string, AssetBundleInfo>();

      foreach (var assetBundleName in AssetDatabase.GetAllAssetBundleNames())
      {
        var assetsInAssetBundle = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

        foreach (var asset in assetsInAssetBundle)
        {
          var assetInfo = new AssetInfo
          {
            AssetName = asset,
            AssetBundleName = assetBundleName
          };
          assets.Add(asset, assetInfo);
          assets.Add(AssetDatabase.AssetPathToGUID(asset), assetInfo);
        }

        assetBundles.Add(assetBundleName, new AssetBundleInfo
        {
          FileName = assetBundleName,
          Assets = assetsInAssetBundle,
          Dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, false)
        });
      }

      return new AssetCatalog
      {
        Assets = assets,
        AssetBundles = assetBundles
      };
    }
#endif
  }
}