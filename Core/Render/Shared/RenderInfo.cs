using System.Drawing;
using Helion.Geometry.Vectors;
using Helion.World.Entities;
using static Helion.Util.Assertion.Assert;

namespace Helion.Render.OpenGL.Shared;

/// <summary>
/// A simple container for render-related information.
/// </summary>
public class RenderInfo
{
    public static Vec2F LastAutomapOffset;

    public OldCamera Camera;
    public float TickFraction;
    public Rectangle Viewport;
    public Entity ViewerEntity;
    public bool DrawAutomap;
    public Vec2I AutomapOffset;
    public double AutomapScale;

    public RenderInfo()
    {

    }

    public void Set(OldCamera camera, float tickFraction, Rectangle viewport, Entity viewerEntity, bool drawAutomap,
        Vec2I automapOffset, double automapScale)
    {
        Precondition(tickFraction >= 0.0 && tickFraction <= 1.0, "Tick fraction should be in the unit range");

        Camera = camera;
        TickFraction = tickFraction;
        Viewport = viewport;
        ViewerEntity = viewerEntity;
        DrawAutomap = drawAutomap;
        AutomapOffset = automapOffset;
        AutomapScale = automapScale;

        if (!DrawAutomap)
            LastAutomapOffset = Vec2F.Zero;
    }
}