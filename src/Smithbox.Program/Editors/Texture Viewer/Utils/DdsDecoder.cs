using Hexa.NET.DirectXTex;
using SixLabors.ImageSharp;
using DxImage = Hexa.NET.DirectXTex.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Decodes a DDS file to 8-bit RGBA through DirectXTex, which handles every block format the games use
/// (BC1 to BC7, including the BC7 and BC5 textures Pfim can't read).
/// </summary>
public static class DdsDecoder
{
    private const int R8G8B8A8_UNORM = 28;

    /// <summary>
    /// Decodes the top mip of the first image in a DDS file. Returns null if DirectXTex can't read it.
    /// The pixels stay in the colour space they are stored in, so an sRGB texture is written out as sRGB.
    /// </summary>
    public static unsafe Image<Rgba32> DecodeToImage(byte[] dds, out string error)
    {
        error = null;

        var source = DirectXTex.CreateScratchImage();
        var converted = DirectXTex.CreateScratchImage();

        try
        {
            TexMetadata metadata;
            HexaGen.Runtime.HResult hr;

            fixed (byte* data = dds)
            {
                hr = DirectXTex.LoadFromDDSMemory(data, (nuint)dds.Length, DDSFlags.None, &metadata, &source);
            }

            if (hr.IsFailure)
            {
                error = $"Failed to read DDS (0x{hr.Value:X8})";
                return null;
            }

            DxImage* image = source.GetImage(0, 0, 0);

            if (image == null)
            {
                error = "DDS has no image";
                return null;
            }

            // Keeping the sRGB flag stops DirectXTex from converting the colours to linear
            var target = DirectXTex.IsSRGB(image->Format) ? DirectXTex.MakeSRGB(R8G8B8A8_UNORM) : R8G8B8A8_UNORM;

            if (image->Format != target)
            {
                hr = DirectXTex.IsCompressed(image->Format)
                    ? DirectXTex.Decompress(image, target, &converted)
                    : DirectXTex.Convert(image, target, TexFilterFlags.Default, 0.5f, &converted);

                if (hr.IsFailure)
                {
                    error = $"Failed to decode DDS format {image->Format} (0x{hr.Value:X8})";
                    return null;
                }

                image = converted.GetImage(0, 0, 0);
            }

            var width = (int)image->Width;
            var height = (int)image->Height;
            var rowPitch = (int)image->RowPitch;

            // Rows can be padded, so they are copied one at a time
            var pixels = new byte[width * height * 4];

            for (var y = 0; y < height; y++)
            {
                new ReadOnlySpan<byte>(image->Pixels + (long)y * rowPitch, width * 4)
                    .CopyTo(pixels.AsSpan(y * width * 4));
            }

            return ImageSharpImage.LoadPixelData<Rgba32>(pixels, width, height);
        }
        catch (Exception e)
        {
            error = e.Message;
            return null;
        }
        finally
        {
            source.Release();
            converted.Release();
        }
    }
}
