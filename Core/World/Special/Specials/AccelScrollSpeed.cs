﻿using Helion.Geometry.Vectors;
using Helion.Maps.Specials.ZDoom;
using Helion.World.Geometry.Sectors;

namespace Helion.World.Special.Specials
{
    public class AccelScrollSpeed
    {
        public Vec2D AccelSpeed;
        public double LastChangeZ;
        public readonly Sector Sector;
        public readonly ZDoomScroll ScrollFlags;

        private Vec2D m_speed;

        public AccelScrollSpeed(Sector changeSector, in Vec2D speed, ZDoomScroll scrollFlags)
        {
            Sector = changeSector;
            m_speed = speed;
            LastChangeZ = Sector.Floor.Z;
            ScrollFlags = scrollFlags;
        }

        public void Tick()
        {
            if (LastChangeZ == Sector.Floor.Z)
            {
                if (ScrollFlags.HasFlag(ZDoomScroll.Displacement))
                    AccelSpeed = Vec2D.Zero;
                return;
            }

            double diff = Sector.Floor.Z - LastChangeZ;
            LastChangeZ = Sector.Floor.Z;
            Vec2D speed = m_speed;
            speed *= diff;

            if (ScrollFlags.HasFlag(ZDoomScroll.Accelerative))
                AccelSpeed += speed;
            else
                AccelSpeed = speed;
        }
    }
}