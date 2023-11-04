﻿using System;
using GlmSharp;
using Helion.Geometry.Vectors;
using Helion.Render.OpenGL.Buffer.Array.Vertex;
using Helion.Render.OpenGL.Shared;
using Helion.Render.OpenGL.Vertex;
using Helion.Util;
using OpenTK.Graphics.OpenGL;

namespace Helion.Render.OpenGL.Renderers.Legacy.World.SkyNew.Sphere;

// A sphere of triangles that can be used when rendering skies.
public class SkySphere : IDisposable
{
    private const int HorizontalSpherePoints = 32;
    private const int VerticalSpherePoints = 32;
    private static readonly vec3 UpOpenGL = new(0, 1, 0);

    private readonly StaticVertexBuffer<SkySphereVertex> m_vbo;
    private readonly VertexArrayObject m_vao;
    private readonly SkySphereProgram m_program;
    private bool m_disposed;

    public SkySphere()
    {
        m_vbo = new("Sky sphere", HorizontalSpherePoints * VerticalSpherePoints);
        m_vao = new("Sky sphere");
        Attributes.BindAndApply(m_vbo, m_vao, m_program.Attributes);

        GenerateAndUploadSphereVertices();
    }

    ~SkySphere()
    {
        PerformDispose();
    }

    private static mat4 CalculateMvp(RenderInfo renderInfo)
    {
        // Note that this means we've hard coded the sky to always render
        // the same regardless of the field of view.
        float w = renderInfo.Viewport.Width;
        float h = renderInfo.Viewport.Height;
        float aspectRatio = w / h;
        float fovY = OldCamera.FieldOfViewXToY((float)MathHelper.HalfPi, aspectRatio);

        // We want the sky sphere to not be touching the NDC edges because
        // we'll be doing some translating which could push it outside of
        // the clipping box. Therefore we shrink the unit sphere from r = 1
        // down to r = 0.5 around the origin.
        mat4 model = mat4.Scale(0.5f);

        // Our world system is in the form <X, Z, -Y> with respect to
        // the OpenGL coordinate transformation system. We will also move
        // our body upwards by 20% (so 0.1 units since r = 0.5) so prevent
        // the horizon from appearing.
        Vec3F direction = renderInfo.Camera.Direction;
        vec3 pos = new(0.0f, 0.1f, 0.0f);
        vec3 eye = new(direction.X, direction.Z, -direction.Y);
        mat4 view = mat4.LookAt(pos, pos + eye, UpOpenGL);

        // Our projection far plane only goes as far as the scaled sphere
        // radius.
        mat4 projection = mat4.PerspectiveFov(fovY, w, h, 0.0f, 0.5f);

        return projection * view * model;
    }

    private void GenerateAndUploadSphereVertices()
    {
        SphereTable sphereTable = new(HorizontalSpherePoints, VerticalSpherePoints);

        for (int row = 0; row < VerticalSpherePoints; row++)
        {
            for (int col = 0; col < HorizontalSpherePoints; col++)
            {
                // Note that this works fine with the +1, it will not go
                // out of range because we specifically made sure that the
                // code adds in one extra vertex for us on both the top row
                // and the right column.
                SkySphereVertex bottomLeft = sphereTable.MercatorRectangle[row, col];
                SkySphereVertex bottomRight = sphereTable.MercatorRectangle[row, col + 1];
                SkySphereVertex topLeft = sphereTable.MercatorRectangle[row + 1, col];
                SkySphereVertex topRight = sphereTable.MercatorRectangle[row + 1, col + 1];

                m_vbo.Add(topLeft);
                m_vbo.Add(bottomLeft);
                m_vbo.Add(topRight);

                m_vbo.Add(topRight);
                m_vbo.Add(bottomLeft);
                m_vbo.Add(bottomRight);
            }
        }

        m_vbo.UploadIfNeeded();
    }

    public void Render(RenderInfo renderInfo, SkyTexture texture, bool flipped)
    {
        mat4 mvp = CalculateMvp(renderInfo);

        GL.ActiveTexture(TextureUnit.Texture0);
        texture.Texture.Bind();
        
        m_program.Bind();
        m_program.BoundTexture(TextureUnit.Texture0);
        m_program.HasInvulnerability(renderInfo.ViewerEntity.IsInvulnerable);
        m_program.Mvp(mvp);
        m_program.ScaleU(texture.ScaleU);
        m_program.FlipU(flipped);
        
        m_vao.Bind();
        m_vbo.DrawArrays();
        m_vao.Unbind();
        
        m_program.Unbind();
        
        texture.Texture.Unbind();
    }
    
    public void Dispose()
    {
        PerformDispose();
        GC.SuppressFinalize(this);
    }

    private void PerformDispose()
    {
        if (m_disposed)
            return;
        
        m_vao.Dispose();
        m_vbo.Dispose();

        m_disposed = true;
    }
}