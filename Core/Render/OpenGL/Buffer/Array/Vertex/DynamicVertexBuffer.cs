using Helion.Render.OpenGL.Context;
using Helion.Render.OpenGL.Context.Types;
using Helion.Render.OpenGL.Vertex;

namespace Helion.Render.OpenGL.Buffer.Array.Vertex;

public class DynamicVertexBuffer<T> : VertexBufferObject<T> where T : struct
{
    public DynamicVertexBuffer(GLCapabilities capabilities, IGLFunctions functions, VertexArrayObject vao, string objectLabel = "") :
        base(capabilities, functions, vao, objectLabel)
    {
    }

    protected override BufferUsageType GetBufferUsageType() => BufferUsageType.DynamicDraw;
}