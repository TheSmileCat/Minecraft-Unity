#region

using System;
using System.Collections;
using System.Collections.Generic;
using Minecraft.Scripts.Assets;
using Minecraft.Scripts.Lua;
using Newtonsoft.Json;
using UnityEngine;

#endregion

namespace Minecraft.Scripts.Configurations
{
  [CreateAssetMenu(menuName = "Minecraft/Configurations/BiomeTable")]
  public class BiomeTable : ScriptableObject, ILuaCallCSharp
  {
    [SerializeField] [EnsureAssetType(typeof(TextAsset))]
    private AssetPtr m_BiomeTableJson;

    [NonSerialized] private BiomeData[] m_Biomes;
    [NonSerialized] private Dictionary<string, BiomeData> m_BiomeMap;

    public IEnumerator Initialize()
    {
      yield return InitBiomes();
    }

    private IEnumerator InitBiomes()
    {
      var json = AssetManager.Instance.LoadAsset<TextAsset>(m_BiomeTableJson);
      yield return json;

      m_Biomes = JsonConvert.DeserializeObject<BiomeData[]>(json.GetAssetAs<TextAsset>().text);
      AssetManager.Instance.UnloadAsset(json);

      m_BiomeMap = new Dictionary<string, BiomeData>(m_Biomes.Length);

      for (var i = 0; i < m_Biomes.Length; i++)
      {
        var biome = m_Biomes[i];
        m_BiomeMap.Add(biome.InternalName, biome);
      }
    }


    public BiomeData GetBiome(int id)
    {
      return m_Biomes[id];
    }

    public BiomeData GetBiome(string name)
    {
      return m_BiomeMap[name];
    }
  }
}