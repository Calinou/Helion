﻿using System;
using System.Drawing;
using Helion.Geometry.Boxes;
using Helion.Render.Common.Renderers;
using OpenTK.Graphics.OpenGL;

namespace Helion.Render.OpenGL.Renderers
{
    public class GLRenderableSurfaceContext : IRenderableSurfaceContext
    {
        public IRenderableSurface Surface { get; }
        private readonly IHudRenderer m_hudRenderer;
        private readonly IWorldRenderer m_worldRenderer;
        private Box2I m_viewport;
        private Box2I m_scissor;

        public GLRenderableSurfaceContext(IRenderableSurface surface, IHudRenderer hud, IWorldRenderer world)
        {
            Surface = surface;
            m_hudRenderer = hud;
            m_worldRenderer = world;
            m_viewport = ((0, 0), surface.Dimension.Vector);
            m_scissor = ((0, 0), surface.Dimension.Vector);
        }
        
        public void Clear(Color color, bool depth, bool stencil)
        {
            GL.ClearColor(color);

            ClearBufferMask mask = ClearBufferMask.ColorBufferBit;
            if (depth)
                mask |= ClearBufferMask.DepthBufferBit;
            if (stencil)
                mask |= ClearBufferMask.StencilBufferBit;
            
            GL.Clear(mask);
        }

        public void Viewport(Box2I area)
        {
            m_viewport = area;
            GL.Viewport(m_viewport.Min.X, m_viewport.Min.Y, m_viewport.Max.X, m_viewport.Max.Y);
        }

        public void Viewport(Box2I area, Action action)
        {
            GL.Viewport(area.Min.X, area.Min.Y, area.Max.X, area.Max.Y);
            action();
            GL.Viewport(m_viewport.Min.X, m_viewport.Min.Y, m_viewport.Max.X, m_viewport.Max.Y);
        }

        public void Scissor(Box2I area)
        {
            m_scissor = area;
            GL.Scissor(m_scissor.Min.X, m_scissor.Min.Y, m_scissor.Max.X, m_scissor.Max.Y);
        }

        public void Scissor(Box2I area, Action action)
        {
            GL.Scissor(area.Min.X, area.Min.Y, area.Max.X, area.Max.Y);
            action();
            GL.Scissor(m_scissor.Min.X, m_scissor.Min.Y, m_scissor.Max.X, m_scissor.Max.Y);
        }

        public void Hud(Action<IHudRenderer> action)
        {
            // TODO
            action(m_hudRenderer);
            // TODO
        }

        public void World(Action<IWorldRenderer> action)
        {
            // TODO
            action(m_worldRenderer);
            // TODO
        }
    }
}