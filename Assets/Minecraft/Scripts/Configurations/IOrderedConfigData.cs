#region

using Minecraft.Scripts.Lua;

#endregion

namespace Minecraft.Scripts.Configurations
{
  public interface IOrderedConfigData : ILuaCallCSharp
  {
    int ID { get; set; }

    string InternalName { get; set; }
  }
}