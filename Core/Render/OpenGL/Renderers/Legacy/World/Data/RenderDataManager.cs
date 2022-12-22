﻿using Helion.Render.OpenGL.Shader;
using Helion.Render.OpenGL.Texture.Legacy;
using Helion.Util.Container;
using OpenTK.Graphics.OpenGL;
using System;

namespace Helion.Render.OpenGL.Renderers.Legacy.World.Data;

public class RenderDataManager<TVertex> : IDisposable where TVertex : struct
{
    private readonly DynamicArray<RenderData<TVertex>?> m_allRenderData = new();
    private readonly DynamicArray<RenderData<TVertex>> m_dataToRender = new();
    private readonly RenderProgram m_program;
    private int m_renderCount;
    private bool m_disposed;

    public RenderDataManager(RenderProgram program)
    {
        m_program = program;
    }

    ~RenderDataManager()
    {
        Dispose(false);
    }

    public void Clear()
    {
        m_dataToRender.Clear();
        m_renderCount++;
    }

    public RenderData<TVertex> Get(GLLegacyTexture texture)
    {
        if (texture.TextureId >= m_allRenderData.Length)
            ResizeToSupportIndex(texture.TextureId);

        RenderData<TVertex>? data = m_allRenderData[texture.TextureId];
        
        if (data == null)
        {
            data = new(texture, m_program) { RenderCount = m_renderCount - 1 };
            m_allRenderData[texture.TextureId] = data;
        }

        if (data.RenderCount != m_renderCount)
        {
            m_dataToRender.Add(data);
            data.RenderCount = m_renderCount;
        }

        return data;
    }

    private void ResizeToSupportIndex(int index)
    {
        const int GrowthSize = 1024;

        int nextLargestSize = ((index / GrowthSize) + 1) * GrowthSize;
        m_allRenderData.Resize(nextLargestSize);
    }

    public void Render()
    {
        for (int i = 0; i < m_dataToRender.Length; i++)
        {
            RenderData<TVertex> data = m_dataToRender[i];
            
            data.Texture.Bind();
            data.Vao.Bind();
            data.Vbo.Bind();
            data.Vbo.Upload();
            
            GL.DrawArrays(PrimitiveType.Points, 0, data.Vbo.Count);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        for (int i = 0; i < m_allRenderData.Length; i++)
            m_allRenderData[i].Dispose();
        m_allRenderData.Clear();

        m_disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}