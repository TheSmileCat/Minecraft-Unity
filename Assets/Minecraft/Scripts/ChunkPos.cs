﻿#region

using System;
using Minecraft.Scripts.Lua;
using Minecraft.XLua.Src;
using UnityEngine;
using static Minecraft.Scripts.WorldConsts;

#endregion

namespace Minecraft.Scripts
{
  [GCOptimize]
  public readonly struct ChunkPos : IEquatable<ChunkPos>, ILuaCallCSharp
  {
    public int X { get; }

    public int Z { get; }


    public Vector2Int XZ => new(X, Z);

    public Vector3Int XOZ => new(X, 0, Z);


    private ChunkPos(int x, int z)
    {
      X = x;
      Z = z;
    }


    public ChunkPos AddOffset(int xCount, int zCount)
    {
      return new ChunkPos(X + (xCount * ChunkWidth), Z + (zCount * ChunkWidth));
    }

    public Vector3Int XYZ(int y)
    {
      return new Vector3Int(X, y, Z);
    }


    public static ChunkPos Get(int x, int z)
    {
      return new ChunkPos(x, z);
    }

    public static ChunkPos GetFromAny(float x, float z)
    {
      var chunkX = Mathf.FloorToInt(x / ChunkWidth) * ChunkWidth;
      var chunkZ = Mathf.FloorToInt(z / ChunkWidth) * ChunkWidth;
      return new ChunkPos(chunkX, chunkZ);
    }


    public bool Equals(ChunkPos other) => X == other.X && Z == other.Z;

    public override bool Equals(object obj) => (obj is ChunkPos pos) && Equals(pos);

    public override int GetHashCode() => X.GetHashCode() ^ (Z.GetHashCode() << 2);

    public override string ToString() => $"Chunk({X}, {Z})";


    public static explicit operator Vector2Int(ChunkPos pos) => new(pos.X, pos.Z);

    public static implicit operator ChunkPos(Vector2Int pos) => new(pos.x, pos.y);

    public static bool operator ==(ChunkPos left, ChunkPos right) => left.Equals(right);

    public static bool operator !=(ChunkPos left, ChunkPos right) => !left.Equals(right);
  }
}