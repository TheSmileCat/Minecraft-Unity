#region

using System.Threading.Tasks;

#endregion

namespace Minecraft.Scripts.UserInputSystem
{
  public interface IUserInput
  {
    //public bool JumpKey { get; set; }
    //public bool SneakKey { get; set; }
    /// <summary>
    /// 用户移动输入
    /// </summary>
    // public Vector2 AxisVector { get; }

    public Task<bool> StartTask { get; }
  }
}