﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Helion.Geometry;
using Helion.Geometry.Boxes;
using Helion.Geometry.Vectors;
using Helion.Graphics;
using Helion.Graphics.Fonts;
using Helion.Render.Common.Textures;
using Helion.Render.OpenGL.Textures.Types;
using Helion.Render.OpenGL.Util;
using Helion.Resources;
using NLog;
using OpenTK.Graphics.OpenGL;
using static Helion.Util.Assertion.Assert;

namespace Helion.Render.OpenGL.Textures
{
    public class GLTextureManager : IRendererTextureManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public GLTextureHandle NullHandle { get; }
        public GLFontTexture NullFont { get; }
        private readonly IResources m_resources;
        private readonly List<AtlasGLTexture> m_textures = new() { new AtlasGLTexture("Atlas layer 0") };
        private readonly List<GLTextureHandle> m_handles = new();
        private readonly ResourceTracker<GLTextureHandle> m_handlesTable = new();
        private readonly Dictionary<string, GLFontTexture> m_fontTextures = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<GLFontTexture> m_fontHandles = new();
        private bool m_disposed;

        public GLTextureManager(IResources resources)
        {
            m_resources = resources;

            NullHandle = AddNullTexture();
            NullFont = AddNullFontTexture();
        }

        ~GLTextureManager()
        {
            FailedToDispose(this);
            PerformDispose();
        }

        private GLTextureHandle AddNullTexture()
        {
            GLTextureHandle? handle = AddImage("NULL", Image.NullImage, Mipmap.Generate, Binding.Bind);
            return handle ?? throw new Exception("Should never fail to allocate the null texture");
        }

        private GLTextureHandle? AddImage(string name, Image image, Mipmap mipmap, Binding bind)
        {
            if (image.ImageType == ImageType.Palette)
                throw new Exception($"Image {name} must be converted to ARGB first before uploading to the GPU");
            
            Dimension neededDim = image.Dimension;
            Dimension maxDim = m_textures[0].Dimension;
            if (neededDim.Width > maxDim.Width || neededDim.Height > maxDim.Height)
                return null;

            for (int i = 0; i < m_textures.Count; i++)
            {
                AtlasGLTexture texture = m_textures[i];
                if (texture.TryUpload(image, out Box2I box, mipmap, bind))
                    return CreateHandle(name, i, box, image, texture);
            }

            // Since we know it has to fit, but it didn't fit anywhere, then we
            // will make a new texture and use that, which must fit via precondition.
            AtlasGLTexture newTexture = new($"Atlas layer {m_textures.Count}");
            m_textures.Add(newTexture);

            if (!newTexture.TryUpload(image, out Box2I newBox, mipmap, bind))
            {
                Fail("Should never fail to upload an image when we allocated enough space for it (GL atlas texture)");
                return null;
            }

            return CreateHandle(name, m_textures.Count - 1, newBox, image, newTexture);
        }

        private GLTextureHandle CreateHandle(string name, int layerIndex, Box2I box, Image image, AtlasGLTexture glTexture)
        {
            int index = m_handles.Count;
            Vec2F uvFactor = glTexture.Dimension.Vector.Float;
            Vec2F min = box.Min.Float / uvFactor;
            Vec2F max = box.Max.Float / uvFactor;
            Box2F uvBox = new(min, max);

            GLTextureHandle handle = new(index, layerIndex, box, uvBox, image.Offset, glTexture);
            m_handles.Add(handle);
            m_handlesTable.Insert(name, image.Namespace, handle);

            return handle;
        }

        private GLFontTexture AddNullFontTexture()
        {
            Glyph glyph = new Glyph('?', Box2F.UnitBox, new Box2I((0, 0), Image.NullImage.Dimension.Vector));
            Dictionary<char, Glyph> glyphs = new() { ['?'] = glyph };
            Font font = new("Null font", glyphs, Image.NullImage);

            GLTexture texture = new("Null font", TextureTarget.Texture2D);
            GLFontTexture fontTexture = new(texture, font);
            m_fontTextures["NULL"] = fontTexture;

            return fontTexture;
        }

        public bool TryGet(string name, [NotNullWhen(true)] out IRenderableTextureHandle? handle, 
            ResourceNamespace? specificNamespace = null)
        {
            GLTextureHandle texture = Get(name, specificNamespace ?? ResourceNamespace.Global);
            handle = texture;
            return ReferenceEquals(texture, NullHandle);
        }
        
        /// <summary>
        /// Gets a texture with a name and priority namespace. If it cannot
        /// find one in the priority namespace, it will search others. If none
        /// can be found, the <see cref="NullHandle"/> is returned. If data is
        /// found for some existing texture in the resource texture manager, it
        /// will upload the texture data.
        /// </summary>
        /// <param name="name">The texture name, case insensitive.</param>
        /// <param name="priority">The first namespace to look at.</param>
        /// <returns>The texture handle, or the <see cref="NullHandle"/> if it
        /// cannot be found.</returns>
        public GLTextureHandle Get(string name, ResourceNamespace priority)
        {
            Texture texture = m_resources.Textures.GetTexture(name, priority);
            return Get(texture, priority);
        }

        /// <summary>
        /// Looks up or creates a texture from an existing resource texture.
        /// </summary>
        /// <param name="texture">The texture to look up (or upload).</param>
        /// <param name="priority">The priority namespace to look up, or null
        /// if it does not matter. This is used in caching results, if our
        /// lookup fails and we pull from somewhere else. It is likely the
        /// case that the same call will be made again.</param>
        /// <returns>A texture handle.</returns>
        public GLTextureHandle Get(Texture texture, ResourceNamespace? priority = null)
        {
            if (texture.Image == null)
                return NullHandle;

            Image image = texture.Image;
            GLTextureHandle? handle = AddImage(texture.Name, image, Mipmap.Generate, Binding.Bind);
            if (handle != null)
            {
                // If we grab it from another namespace, also track it in the
                // requested namespace because we know it doesn't exist and
                // instead can return hits quicker by caching the result.
                if (priority != null && priority != image.Namespace)
                    m_handlesTable.Insert(texture.Name, priority.Value, handle);
                
                return handle;
            }
            
            Log.Warn("Unable to allocate space for texture {Name} ({Dimension}, {Namespace})", texture.Name, image.Dimension, image.Namespace);
            return NullHandle;
        }
        
        /// <summary>
        /// Gets a font, or uploads it if it finds one and it has not been
        /// uploaded yet. If none can be found, the <see cref="NullFont"/> is
        /// returned.
        /// </summary>
        /// <param name="name">The font name, case insensitive.</param>
        /// <returns>The font handle, or <see cref="NullFont"/> if no font
        /// resource can be found.</returns>
        public GLFontTexture GetFont(string name)
        {
            if (m_fontTextures.TryGetValue(name, out GLFontTexture? fontTexture))
                return fontTexture;

            // TODO: Try to create it from m_resources.

            return NullFont;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            PerformDispose();
        }

        private void PerformDispose()
        {
            if (m_disposed)
                return;

            m_handles.Clear();
            m_handlesTable.Clear();

            foreach (var texture in m_fontHandles)
                texture.Dispose();
            m_fontHandles.Clear();

            foreach (AtlasGLTexture texture in m_textures)
                texture.Dispose();
            m_textures.Clear();

            m_disposed = true;
        }
    }
}
