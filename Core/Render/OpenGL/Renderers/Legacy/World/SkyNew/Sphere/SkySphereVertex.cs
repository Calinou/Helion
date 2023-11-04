using System.Runtime.InteropServices;
using Helion.Render.OpenGL.Vertex;

namespace Helion.Render.OpenGL.Renderers.Legacy.World.SkyNew.Sphere;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct SkySphereVertex
{
    [VertexAttribute("pos", size: 3)]
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    [VertexAttribute("uv", size: 2)]
    public readonly float U;
    public readonly float V;

    public SkySphereVertex(float x, float y, float z, float u, float v)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
    }
}
