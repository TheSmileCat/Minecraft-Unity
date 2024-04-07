using System;
using Minecraft.XLua.Src;
using UnityEngine;

namespace Minecraft.Scripts
{
    [Serializable]
    [LuaCallCSharp]
    public class WorldSetting
    {
        public string Name;
        public int Seed;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        public Quaternion CameraRotation;
        public string ResourcePackageName;
    }
}