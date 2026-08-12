using SoulsFormats;
using System;
using System.Collections.Generic;
using Vortice.Vulkan;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Reconstructs a standard, loadable DDS byte blob from a Bloodborne/PS4-platform TPF texture.
///
/// PS4 TPF textures are stored "swizzled" (tiled into tightly packed blocks for the PS4's GPU memory
/// controller) rather than as a plain top-to-bottom DDS mip chain, so their raw bytes can't be handed to a
/// normal DDS/PNG decoder as-is. The live viewport already reverses this layout when uploading these
/// textures to the GPU (see <see cref="TexturePool.TextureHandle.FillWithPS4TPF"/>); this reuses the exact
/// same (pure byte-shuffling, GPU-independent) deswizzle routines, but writes the result into a plain
/// managed byte buffer and wraps it in a standard DX10 DDS header instead of uploading it to a texture.
/// </summary>
public static class Ps4TextureDecoder
{
    public static byte[] TryDecodeToDds(TPF.Texture tex, out string error)
    {
        error = null;

        if (tex.Header == null)
        {
            error = "Texture has no PS4 header metadata to decode.";
            return null;
        }

        var dxgiFormat = (DDS.DXGI_FORMAT)tex.Header.DXGIFormat;

        // Matches the "BB HACK" skip in TexturePool.TextureHandle.FillWithPS4TPF - the RGBA deswizzle path
        // indexes out of range for this particular format.
        if (dxgiFormat == DDS.DXGI_FORMAT.R16G16B16A16_FLOAT)
        {
            error = "R16G16B16A16_FLOAT PS4 textures aren't supported (matches a known limitation in the viewport's own PS4 texture loader).";
            return null;
        }

        VkFormat vkFormat;
        try
        {
            vkFormat = TexturePool.GetPixelFormatFromDXGI(dxgiFormat);
        }
        catch (Exception e)
        {
            error = $"Unsupported DXGI format {dxgiFormat}: {e.Message}";
            return null;
        }

        var rawWidth = tex.Header.Width;
        var rawHeight = tex.Header.Height;

        var width = (uint)(Math.Ceiling(rawWidth / 4f) * 4f);
        var height = (uint)(Math.Ceiling(rawHeight / 4f) * 4f);

        uint mipCount = tex.Mipmaps;
        if (mipCount == 0)
            mipCount = (uint)(1 + Math.Floor(Math.Log(Math.Max(width, height), 2)));

        var isCubemap = tex.Type == TPF.TexType.Cubemap;
        var arrayCount = (uint)(tex.Header.TextureCount > 0 ? tex.Header.TextureCount : isCubemap ? 6 : 1);

        var blockSize = (uint)TexturePool.GetBlockSize(tex.Format);

        var output = new List<byte>(tex.Bytes.Length);
        var copyOffset = 0;

        try
        {
            for (var slice = 0; slice < arrayCount; slice++)
            {
                var currentWidth = width;
                var currentHeight = height;

                for (uint level = 0; level < mipCount; level++)
                {
                    uint paddedWidth, paddedHeight, paddedSize;

                    if (tex.Format == 105)
                    {
                        paddedWidth = currentWidth;
                        paddedHeight = currentHeight;
                        paddedSize = paddedWidth * paddedHeight * blockSize;
                    }
                    else
                    {
                        paddedWidth = (uint)(Math.Ceiling(currentWidth / 32f) * 32f);
                        paddedHeight = (uint)(Math.Ceiling(currentHeight / 32f) * 32f);
                        paddedSize = (uint)(Math.Ceiling(paddedWidth / 4f) * Math.Ceiling(paddedHeight / 4f) * blockSize);
                    }

                    var mipByteCount = TexturePool.GetMipInfo(vkFormat, rawWidth, rawHeight, (int)level, false);
                    var mipOutput = new byte[mipByteCount];

                    var input = new Span<byte>(tex.Bytes, copyOffset, (int)paddedSize);

                    TexturePool.TextureHandle.DeswizzleDDSBytesPS4((int)currentWidth, (int)currentHeight, tex.Format,
                        (int)blockSize, (int)paddedWidth, input, mipOutput);

                    output.AddRange(mipOutput);

                    copyOffset += (int)paddedSize;

                    if (currentWidth > 1) currentWidth /= 2;
                    if (currentHeight > 1) currentHeight /= 2;
                }
            }
        }
        catch (Exception e)
        {
            error = $"Failed to deswizzle PS4 texture: {e.Message}";
            return null;
        }

        var dds = new DDS
        {
            dwWidth = rawWidth,
            dwHeight = rawHeight,
            dwMipMapCount = (int)mipCount,
            dwFlags = DDS.HEADER_FLAGS_TEXTURE | DDS.DDSD.MIPMAPCOUNT,
            dwCaps = DDS.DDSCAPS.TEXTURE
        };

        if (mipCount > 1)
            dds.dwCaps |= DDS.DDSCAPS.MIPMAP | DDS.DDSCAPS.COMPLEX;

        dds.ddspf.dwFourCC = "DX10";
        dds.header10 = new DDS.HEADER_DXT10
        {
            dxgiFormat = dxgiFormat,
            resourceDimension = DDS.DIMENSION.TEXTURE2D,
            arraySize = isCubemap ? 6u : 1u
        };

        if (isCubemap)
        {
            dds.dwCaps |= DDS.DDSCAPS.COMPLEX;
            dds.dwCaps2 = DDS.CUBEMAP_ALLFACES;
            dds.header10.miscFlag = DDS.RESOURCE_MISC.TEXTURECUBE;
        }

        try
        {
            return dds.Write(output.ToArray());
        }
        catch (Exception e)
        {
            error = $"Failed to build DDS header: {e.Message}";
            return null;
        }
    }
}
