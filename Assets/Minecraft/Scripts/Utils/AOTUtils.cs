#region

using Newtonsoft.Json.Utilities;
using UnityEngine;
using UnityEngine.Scripting;

#endregion

namespace Minecraft.Scripts.Utils
{
  [Preserve]
  public class AOTUtils
  {
    public void EnsureAOT()
    {
      AotHelper.EnsureList<int?>();
      AotHelper.EnsureDictionary<Vector3Int, int>(); // used in lua
    }
  }
}