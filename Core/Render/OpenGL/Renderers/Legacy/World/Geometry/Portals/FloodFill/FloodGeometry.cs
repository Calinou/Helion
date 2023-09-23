﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helion.Render.OpenGL.Renderers.Legacy.World.Geometry.Portals.FloodFill;

public readonly struct FloodGeometry
{
    public readonly int Key;
    public readonly int TextureHandle;
    public readonly int VboOffset;

    public FloodGeometry(int key, int textureHandle, int vboOffset)
    {
        Key = key;
        TextureHandle = textureHandle;
        VboOffset = vboOffset;
    }
}