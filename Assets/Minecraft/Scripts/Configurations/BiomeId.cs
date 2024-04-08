﻿#region

using Minecraft.XLua.Src;

#endregion

namespace Minecraft.Scripts.Configurations
{
  [GCOptimize]
  [LuaCallCSharp]
  public enum BiomeId : byte
  {
    Ocean = 0,
    Plains = 1,
    Desert = 2,
    Mountains = 3,
    Forest = 4,
    Taiga = 5,
    Swamp = 6,
    River = 7,
    Nether = 8,
    TheEnd = 9,
    FrozenOcean = 10,
    FrozenRiver = 11,
    SnowyTundra = 12,
    SnowyMountains = 13,
    MushroomFields = 14,
    MushroomFieldShore = 15,
    Beach = 16,
    DesertHills = 17,
    WoodedHills = 18,
    TaigaHills = 19,
    MountainEdge = 20,
    Jungle = 21,
    JungleHills = 22,
    JungleEdge = 23,
    DeepOcean = 24,
    StoneShore = 25,
    SnowyBeach = 26,
    BirchForest = 27,
    BirchForestHills = 28,
    DarkForest = 29,
    SnowyTaiga = 30,
    SnowyTaigaHills = 31,
    GiantTreeTaiga = 32,
    GiantTreeTaigaHills = 33,
    WoodedMountains = 34,
    Savanna = 35,
    SavannaPlateau = 36,
    Badlands = 37,
    WoodedBadlandsPlateau = 38,
    BadlandsPlateau = 39,
    SmallEndIslands = 40,
    EndMidlands = 41,
    EndHighlands = 42,
    EndBarrens = 43,
    WarmOcean = 44,
    LukewarmOcean = 45,
    ColdOcean = 46,
    DeepWarmOcean = 47,
    DeepLukewarmOcean = 48,
    DeepColdOcean = 49,
    DeepFrozenOcean = 50
  }
}