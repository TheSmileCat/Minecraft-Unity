using System;
using Minecraft.XLua.Src;

namespace Minecraft.Scripts.Utils
{
    [LuaCallCSharp]
    public static class ArrayUtility
    {
        public static void EnsureArrayReferenceAndSize<T>(ref T[] array, int size, bool restrictSize)
        {
            if (array == null)
            {
                array = new T[size];
            }
            else if ((restrictSize && array.Length != size) || array.Length < size)
            {
                Array.Resize(ref array, size);
            }
        }
    }
}
