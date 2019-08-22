﻿using System;
using Helion.Maps.Geometry;
using Helion.Util;

namespace Helion.Maps.Special.Specials
{
    public class LightStrobeSpecial : ISpecial
    {
        public Sector? Sector { get; private set; }

        private byte m_maxBright;
        private byte m_minBright;
        private int m_brightTics;
        private int m_darkTics;
        private int m_delay;

        public LightStrobeSpecial(Sector sector, byte minLightLevel, int brightTics, int darkTics)
        {
            Sector = sector;
            m_brightTics = brightTics;
            m_darkTics = darkTics;
            m_maxBright = sector.LightLevel;
            m_minBright = minLightLevel;
        }

        public SpecialTickStatus Tick(long gametic)
        {
            if (m_delay > 0)
            {
                m_delay--;
                return SpecialTickStatus.Continue;
            }

            if (Sector.LightLevel == m_maxBright)
            {
                Sector.SetLightLevel(m_minBright);
                m_delay = m_brightTics;            
            }
            else if (Sector.LightLevel == m_minBright)
            {
                Sector.SetLightLevel(m_maxBright);
                m_delay = m_darkTics;
            }

            return SpecialTickStatus.Continue;
        }

        public void Use()
        {
        }
    }
}
