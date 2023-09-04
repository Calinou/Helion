using System;
using Helion.Geometry;
using Helion.Geometry.Vectors;
using Helion.Graphics.Palettes;
using Helion.Resources;
using Helion.Util.Extensions;
using static Helion.Util.Assertion.Assert;

namespace Helion.Graphics;

/// <summary>
/// An image, that can either contain ARGB data or palette indices.
/// </summary>
/// <remarks>
/// The bitmap, if of ImageType Palette, will have only the alpha and red
/// channels set. The alpha channel will be either 255, or 0.
/// </remarks>
public class Image
{
    public const ushort TransparentIndex = 0xFF00;
    public static readonly Image NullImage = CreateNullImage();
    public static readonly Image WhiteImage = CreateWhiteImage();
    public static readonly Image TransparentImage = CreateTransparentImage();

    public Dimension Dimension;
    public readonly ImageType ImageType;
    public readonly Vec2I Offset;
    public readonly ResourceNamespace Namespace;
    private readonly uint[] m_pixels; // Stored as argb with a = high byte, b = low byte

    public int Width => Dimension.Width;
    public int Height => Dimension.Height;
    public Span<uint> Pixels => m_pixels;

    public Image(Dimension dimension, ImageType imageType, Vec2I offset = default, ResourceNamespace ns = ResourceNamespace.Global) :
        this(new uint[dimension.Area], dimension, imageType, offset, ns)
    {
    }

    public Image(int w, int h, ImageType imageType, Vec2I offset = default, ResourceNamespace ns = ResourceNamespace.Global) :
        this((w, h), imageType, offset, ns)
    {
        Precondition(w >= 0, "Tried providing a negative width for an image");
        Precondition(h >= 0, "Tried providing a negative height for an image");
    }

    public Image(uint[] pixels, Dimension dimension, ImageType imageType, Vec2I offset, ResourceNamespace ns)
    {
        Precondition(pixels.Length == dimension.Area, "Image size mismatch");

        Dimension = dimension;
        ImageType = imageType;
        Offset = offset;
        Namespace = ns;
        m_pixels = pixels;
    }

    public static Image? FromPaletteIndices(Dimension dimension, uint[] indices, Vec2I offset = default, ResourceNamespace ns = ResourceNamespace.Global)
    {
        if (dimension.Area != indices.Length)
            return null;

        return new(indices, dimension, ImageType.Palette, offset, ns);
    }

    public static Image? FromArgbBytes(Dimension dimension, byte[] argbData, Vec2I offset = default, ResourceNamespace ns = ResourceNamespace.Global)
    {
        if (dimension.Area * 4 != argbData.Length)
            return null;

        int numPixels = argbData.Length / 4;
        uint[] pixels = new uint[numPixels];

        int argbByteOffset = 0;
        for (int i = 0; i < numPixels; i++)
        {
            uint a = argbData[argbByteOffset];
            uint r = argbData[argbByteOffset + 1];
            uint g = argbData[argbByteOffset + 2];
            uint b = argbData[argbByteOffset + 3];
            pixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
            argbByteOffset += 4;
        }

        return new(pixels, dimension, ImageType.Argb, offset, ns);
    }

    public Image PaletteToArgb(Palette palette)
    {
        uint[] pixels = new uint[m_pixels.Length];
        Color[] layer = palette.DefaultLayer;

        for (int i = 0; i < m_pixels.Length; i++)
        {
            uint argb = m_pixels[i];
            pixels[i] = (argb == Image.TransparentIndex ? Color.Transparent.Uint : layer[argb].Uint);
        }

        return new(pixels, Dimension, ImageType.Argb, Offset, Namespace);
    }

    private uint BlendPixels(uint back, uint front)
    {
        uint frontA = (front >> 24) & 0xFF;
        if (frontA == 0)
            return back;

        float alpha = frontA / 255.0f;
        uint frontR = (front >> 16) & 0xFF;
        uint frontG = (front >> 8) & 0xFF;
        uint frontB = front & 0xFF;
        uint backA = (back >> 24) & 0xFF;
        uint backR = (back >> 16) & 0xFF;
        uint backG = (back >> 8) & 0xFF;
        uint backB = back & 0xFF;

        float oneMinusAlpha = 1.0f - alpha;
        float newA = (alpha * frontA) + (oneMinusAlpha * backA);
        float newR = (alpha * frontR) + (oneMinusAlpha * backR);
        float newG = (alpha * frontG) + (oneMinusAlpha * backG);
        float newB = (alpha * frontB) + (oneMinusAlpha * backB);

        uint a = (uint)newA;
        uint r = (uint)newR;
        uint g = (uint)newG;
        uint b = (uint)newB;

        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    public void DrawOnTopOf(Image image, Vec2I offset)
    {
        for (int thisY = 0; thisY < Height; thisY++)
        {
            int targetY = thisY + offset.Y;

            if (targetY < 0)
                continue;
            if (targetY >= image.Height)
                break;

            for (int thisX = 0; thisX < Width; thisX++)
            {
                int targetX = thisX + offset.X;
                if (targetX < 0)
                    continue;
                if (targetX >= image.Width)
                    break;

                int thisOffset = (thisY * Width) + thisX;
                int targetOffset = (targetY * image.Width) + targetX;

                uint pixel = m_pixels[thisOffset];
                uint alpha = (pixel >> 24) & 0xFF;
                if (alpha > 0)
                {
                    if (alpha == 255)
                        image.m_pixels[targetOffset] = pixel;
                    else
                        image.m_pixels[targetOffset] = BlendPixels(image.m_pixels[targetOffset], pixel);
                }
            }
        }
    }

    public void Fill(Color color)
    {
        m_pixels.Fill(color.Uint);
    }

    public void FillRows(Color color, int startY, int endY)
    {
        int offsetStart = startY * Width;
        int offsetEnd = endY * Width;
        uint argb = color.Uint;
        for (int i = offsetStart; i < offsetEnd; i++)
            m_pixels[i] = argb;
    }

    public int TransparentPixelCount()
    {
        int count = 0;
        for (int i = 0; i < m_pixels.Length ; i++)
        {
            if ((m_pixels[i] & 0xFF000000) == 0)
                count++;
        }
        return count;
    }

    public Color GetPixel(int x, int y)
    {
        int offset = (y * Width) + x;
        uint argb = m_pixels[offset];
        return new(argb);
    }

    public void SetPixel(int x, int y, Color color)
    {
        int offset = (y * Width) + x;
        if (offset >= 0 && offset < m_pixels.Length)
            m_pixels[offset] = color.Uint;
    }

    public Image FlipY()
    {
        uint[] flippedPixels = new uint[Dimension.Area];

        for (int srcRow = 0; srcRow < Dimension.Height; srcRow++)
        {
            int destRow = Dimension.Height - 1 - srcRow;
            int srcOffset = srcRow * Width;
            int destOffset = destRow * Width;

            for (int col = 0; col < Dimension.Width; col++)
            {
                flippedPixels[destOffset] = m_pixels[srcOffset];
                srcOffset++;
                destOffset++;
            }
        }

        return new(flippedPixels, Dimension, ImageType, Offset, Namespace);
    }



    private static Image CreateNullImage()
    {
        const int Dimension = 8;
        const int HalfDimension = Dimension / 2;

        Image image = new((Dimension, Dimension), ImageType.Argb);

        image.Fill(Color.Black);

        for (int y = 0; y < HalfDimension; y++)
            for (int x = 0; x < HalfDimension; x++)
                image.SetPixel(x, y, Color.Red);

        for (int y = HalfDimension; y < Dimension; y++)
            for (int x = HalfDimension; x < Dimension; x++)
                image.SetPixel(x, y, Color.Red);

        return image;
    }

    private static Image CreateWhiteImage()
    {
        return new(new[] { Color.White.Uint }, (1, 1), ImageType.Argb, (0, 0), ResourceNamespace.Global);
    }

    private static Image CreateTransparentImage()
    {
        return new(new[] { Color.Transparent.Uint }, (1, 1), ImageType.Argb, (0, 0), ResourceNamespace.Global);
    }
}
