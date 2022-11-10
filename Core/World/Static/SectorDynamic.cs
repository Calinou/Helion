﻿using System;

namespace Helion.World.Static;

[Flags]
public enum SectorDynamic
{
    Movement = 1,
    Light = 2,
    TransferHeights = 4,
    TransferHeightStatic = 8,
    Scroll = 16,
    ChangeFloorTexture = 32,
}