﻿using Helion.Util.Configs.Values;

namespace Helion.Util.Configs.Components
{
    [ConfigInfo("Developer options for debugging the engine.")]
    public class ConfigDeveloper
    {
        [ConfigInfo("If the internal BSP builder should be used.")]
        public readonly ConfigValueBoolean InternalBSPBuilder = new();

        [ConfigInfo("If rendering should have debugging information drawn.")]
        public readonly ConfigValueBoolean RenderDebug = new();
    }
}