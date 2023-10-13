﻿using Helion;
using Helion.Util.Container;
using Helion.Util.RandomGenerators;
using Helion.World;
using Helion.World;
using Helion.World.Entities;
using Helion.World.Entities.Definition.States;
using Helion.World.Physics.Blockmap;
using System.Collections.Generic;

namespace Helion.World;

public static class WorldStatic
{
    public static DynamicArray<BlockmapIntersect> Intersections = new(1024);
    public static IRandom Random;
    public static bool SlowTickEnabled;
    public static int SlowTickChaseFailureSkipCount;
    public static int SlowTickDistance;
    public static int SlowTickChaseMultiplier;
    public static int SlowTickLookMultiplier;
    public static int SlowTickTracerMultiplier;
    public static bool IsFastMonsters;
    public static bool IsSlowMonsters;
    public static bool InfinitelyTallThings;
    public static bool MissileClip;
    public static bool AllowItemDropoff;
    public static bool NoTossDrops;
    public static int RespawnTimeSeconds;
    public static int ClosetLookFrameIndex;
    public static int ClosetChaseFrameIndex;
    public static EntityManager EntityManager;
    public static List<EntityFrame> Frames;

    public static void FlushIntersectionReferences()
    {
        for (int i = 0; i < Intersections.Capacity; i++)
        {
            Intersections.Data[i].Entity = null;
            Intersections.Data[i].Line = null;
        }
    }
}