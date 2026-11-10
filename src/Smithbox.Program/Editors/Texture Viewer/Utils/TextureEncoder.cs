using Hexa.NET.DirectXTex;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using DxImage = Hexa.NET.DirectXTex.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// What an imported image is turned into.
/// </summary>
public sealed class TextureEncodeRequest
{
    /// <summary>The DXGI format to write.</summary>
    public int TargetFormat;

    /// <summary>Write a DX10 header even when a legacy (DXT1/DXT5/ATI1/ATI2) header could describe the format.</summary>
    public bool ForceDx10Header;

    /// <summary>
    /// The legacy FourCC to write when the header isn't DX10 (e.g. "ATI1"): DirectXTex writes BC4/BC5 as "BC4U"/"BC5U",
    /// which the games' files (and SoulsFormats' DDS reader) spell "ATI1"/"ATI2". Null keeps DirectXTex's.
    /// </summary>
    public string LegacyFourCC;

    /// <summary>Mip levels to write: 0 = the full chain, 1 = the top level only.</summary>
    public int MipLevels;

    /// <summary>Resize the image to this size first (0 = keep the image's size).</summary>
    public int ResizeWidth;
    public int ResizeHeight;
}

public sealed class TextureEncodeResult
{
    public byte[] Dds;
    public string Error;
    public int Width;
    public int Height;
    public int MipLevels;
    public int Format;
    public bool UsedGpu;

    /// <summary>The source was a DDS already in the requested form, so its bytes were kept as they are.</summary>
    public bool PassedThrough;

    public long Milliseconds;
}

/// <summary>
/// Turns DDS, PNG, TGA, TIFF, BMP, JPEG and WEBP files into game-ready DDS files (Texture Editor plan, T2).
/// BC6H/BC7 go through DirectXTex's GPU compressor on a standalone D3D11 device (§6 Q3: ~1 s for a 4096×2048 BC7
/// sheet, against minutes on the CPU), falling back to the CPU's quick BC7 mode when no device can be made.
/// Everything else is compressed or converted on the CPU, which is instant for those formats.
/// </summary>
public static unsafe class TextureEncoder
{
    // DXGI formats used below
    public const int R32G32B32A32_FLOAT = 2;
    public const int R16G16B16A16_FLOAT = 10;
    public const int R8G8B8A8_UNORM = 28;
    public const int R8G8B8A8_UNORM_SRGB = 29;
    public const int BC6H_UF16 = 95;
    public const int BC6H_SF16 = 96;
    public const int BC7_UNORM = 98;
    public const int BC7_UNORM_SRGB = 99;

    private static readonly object GpuLock = new();
    private static Vortice.Direct3D11.ID3D11Device GpuDevice;
    private static bool GpuTried;

    /// <summary>The adapter the GPU encoder runs on, or null when there is none (CPU fallback).</summary>
    public static string GpuAdapterName { get; private set; }

    public static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".dds" or ".png" or ".tga" or ".tif" or ".tiff" or ".bmp" or ".jpg" or ".jpeg" or ".webp";
    }

    public static readonly string[] FileDialogFilters = ["dds", "png", "tga", "tif", "tiff", "bmp", "jpg", "jpeg", "webp"];

    /// <summary>
    /// Encodes the file's bytes. The extension says how to read them (".dds" or an image format).
    /// Safe to call from a background task.
    /// </summary>
    public static TextureEncodeResult Encode(byte[] source, string extension, TextureEncodeRequest request)
    {
        var timer = Stopwatch.StartNew();
        var result = new TextureEncodeResult();

        var loaded = DirectXTex.CreateScratchImage();
        var working = DirectXTex.CreateScratchImage();
        var resized = DirectXTex.CreateScratchImage();
        var mipped = DirectXTex.CreateScratchImage();
        var encoded = DirectXTex.CreateScratchImage();

        try
        {
            int target = request.TargetFormat;
            bool targetSrgb = DirectXTex.IsSRGB(target);
            bool targetFloat = target is BC6H_UF16 or BC6H_SF16 or R16G16B16A16_FLOAT or R32G32B32A32_FLOAT;

            // The format edits happen in: 8-bit with the target's colour space (so no sRGB conversion happens on the
            // way in or out), or float for HDR targets
            int work = targetFloat ? R32G32B32A32_FLOAT : targetSrgb ? R8G8B8A8_UNORM_SRGB : R8G8B8A8_UNORM;

            HexaGen.Runtime.HResult hr;
            DxImage* image;

            if (extension.Equals(".dds", StringComparison.OrdinalIgnoreCase))
            {
                TexMetadata meta;
                fixed (byte* p = source)
                    hr = DirectXTex.LoadFromDDSMemory(p, (nuint)source.Length, DDSFlags.None, &meta, &loaded);

                if (hr.IsFailure)
                    return Fail(result, $"Failed to read the DDS file (0x{hr.Value:X8})");

                // Already what was asked for: keep the user's file exactly
                bool sameSize = request.ResizeWidth == 0 || (request.ResizeWidth == (int)meta.Width && request.ResizeHeight == (int)meta.Height);
                bool sameMips = request.MipLevels == (int)meta.MipLevels || (request.MipLevels == 0 && (int)meta.MipLevels == FullChain((int)meta.Width, (int)meta.Height));

                if (meta.Format == target && sameSize && sameMips && meta.ArraySize == 1 && meta.Depth == 1)
                {
                    result.Dds = source;
                    result.PassedThrough = true;
                    result.Width = (int)meta.Width;
                    result.Height = (int)meta.Height;
                    result.MipLevels = (int)meta.MipLevels;
                    result.Format = target;
                    return result;
                }

                if (meta.ArraySize != 1 || meta.Depth != 1)
                    return Fail(result, "Cubemaps, arrays and volume textures can only be imported as a DDS file already in the texture's format, size and mip count");

                var top = loaded.GetImage(0, 0, 0);
                hr = DirectXTex.IsCompressed(top->Format)
                    ? DirectXTex.Decompress(top, work, &working)
                    : DirectXTex.Convert(top, work, TexFilterFlags.Default, 0.5f, &working);

                if (hr.IsFailure)
                    return Fail(result, $"Failed to decode the DDS file's format {top->Format} (0x{hr.Value:X8})");
            }
            else
            {
                var error = LoadImage(source, targetSrgb ? R8G8B8A8_UNORM_SRGB : R8G8B8A8_UNORM, ref loaded);
                if (error != null)
                    return Fail(result, error);

                if (targetFloat)
                {
                    hr = DirectXTex.Convert(loaded.GetImage(0, 0, 0), work, TexFilterFlags.Default, 0.5f, &working);
                    if (hr.IsFailure)
                        return Fail(result, $"Failed to convert the image to floating point (0x{hr.Value:X8})");
                }
                else
                {
                    // Swap so 'working' holds the image
                    (loaded, working) = (working, loaded);
                }
            }

            image = working.GetImage(0, 0, 0);

            // Resize
            if (request.ResizeWidth > 0 && request.ResizeHeight > 0 && (request.ResizeWidth != (int)image->Width || request.ResizeHeight != (int)image->Height))
            {
                hr = DirectXTex.Resize(image, (nuint)request.ResizeWidth, (nuint)request.ResizeHeight, TexFilterFlags.Default, &resized);
                if (hr.IsFailure)
                    return Fail(result, $"Failed to resize to {request.ResizeWidth}x{request.ResizeHeight} (0x{hr.Value:X8})");
                image = resized.GetImage(0, 0, 0);
            }

            if (DirectXTex.IsCompressed(target) && (image->Width % 4 != 0 || image->Height % 4 != 0))
                return Fail(result, $"Block compressed textures need a width and height divisible by 4 ({image->Width}x{image->Height})");

            // Mips
            DxImage* images;
            nuint imageCount;
            TexMetadata chainMeta;

            int levels = request.MipLevels <= 0 ? FullChain((int)image->Width, (int)image->Height) : Math.Min(request.MipLevels, FullChain((int)image->Width, (int)image->Height));

            if (levels > 1)
            {
                hr = DirectXTex.GenerateMipMaps(image, TexFilterFlags.Default, (nuint)levels, &mipped, false);
                if (hr.IsFailure)
                    return Fail(result, $"Failed to generate mipmaps (0x{hr.Value:X8})");

                images = mipped.GetImages();
                imageCount = mipped.GetImageCount();
                chainMeta = mipped.GetMetadata();
            }
            else
            {
                images = image;
                imageCount = 1;
                chainMeta = new TexMetadata
                {
                    Width = image->Width,
                    Height = image->Height,
                    Depth = 1,
                    ArraySize = 1,
                    MipLevels = 1,
                    Format = image->Format,
                    Dimension = TexDimension.Texture2D,
                };
            }

            // Encode
            if (DirectXTex.IsCompressed(target))
            {
                bool bc7 = target is BC7_UNORM or BC7_UNORM_SRGB;
                bool gpuFormat = bc7 || target is BC6H_UF16 or BC6H_SF16;

                hr = new HexaGen.Runtime.HResult(-1);

                if (gpuFormat)
                {
                    lock (GpuLock)
                    {
                        var device = GetGpuDevice();
                        if (device != null)
                        {
                            hr = DirectXTex.Compress4((Hexa.NET.DirectXTex.ID3D11Device*)device.NativePointer, images, imageCount, &chainMeta, target,
                                bc7 ? TexCompressFlags.Bc7Use3Subsets : TexCompressFlags.Default, 1.0f, &encoded);
                            result.UsedGpu = !hr.IsFailure;
                        }
                    }
                }

                if (!result.UsedGpu)
                {
                    encoded.Release();
                    encoded = DirectXTex.CreateScratchImage();

                    hr = DirectXTex.Compress2(images, imageCount, &chainMeta, target,
                        TexCompressFlags.Parallel | (bc7 ? TexCompressFlags.Bc7Quick : TexCompressFlags.Default), 0.5f, &encoded);
                }

                if (hr.IsFailure)
                    return Fail(result, $"Failed to compress to DXGI format {target} (0x{hr.Value:X8})");
            }
            else if (chainMeta.Format != target)
            {
                hr = DirectXTex.Convert2(images, imageCount, &chainMeta, target, TexFilterFlags.Default, 0.5f, &encoded);
                if (hr.IsFailure)
                    return Fail(result, $"Failed to convert to DXGI format {target} (0x{hr.Value:X8})");
            }
            // Already in the target format, the chain is written as it is
            bool wasEncoded = DirectXTex.IsCompressed(target) || chainMeta.Format != target;
            var outImages = wasEncoded ? encoded.GetImages() : images;
            var outCount = wasEncoded ? encoded.GetImageCount() : imageCount;
            var outMeta = wasEncoded ? encoded.GetMetadata() : chainMeta;
            var blob = DirectXTex.CreateBlob();

            try
            {
                hr = DirectXTex.SaveToDDSMemory2(outImages, outCount, &outMeta,
                    request.ForceDx10Header ? DDSFlags.ForceDx10Ext : DDSFlags.None, &blob);

                if (hr.IsFailure)
                    return Fail(result, $"Failed to write the DDS file (0x{hr.Value:X8})");

                var size = (int)blob.GetBufferSize();
                result.Dds = new ReadOnlySpan<byte>(blob.GetBufferPointer(), size).ToArray();

                // Same compression, the game's spelling of it (BC4U → ATI1, BC5U → ATI2)
                if (!request.ForceDx10Header && request.LegacyFourCC?.Length == 4 && result.Dds.Length > 0x58)
                {
                    var written = System.Text.Encoding.ASCII.GetString(result.Dds, 0x54, 4);
                    if (written != "DX10" && IsEquivalentFourCC(written, request.LegacyFourCC))
                        System.Text.Encoding.ASCII.GetBytes(request.LegacyFourCC, 0, 4, result.Dds, 0x54);
                }
            }
            finally
            {
                DirectXTex.BlobRelease(blob);
            }

            result.Width = (int)outMeta.Width;
            result.Height = (int)outMeta.Height;
            result.MipLevels = (int)outMeta.MipLevels;
            result.Format = target;
            return result;
        }
        catch (Exception e)
        {
            return Fail(result, e.Message);
        }
        finally
        {
            loaded.Release();
            working.Release();
            resized.Release();
            mipped.Release();
            encoded.Release();
            result.Milliseconds = timer.ElapsedMilliseconds;
        }
    }

    /// <summary>Reads a PNG/TGA/TIFF/BMP/JPEG/WEBP into an 8-bit RGBA scratch image of the given format.</summary>
    private static string LoadImage(byte[] source, int format, ref ScratchImage scratch)
    {
        using var img = ImageSharpImage.Load<Rgba32>(source);

        var hr = scratch.Initialize2D(format, (nuint)img.Width, (nuint)img.Height, 1, 1, CPFlags.None);
        if (hr.IsFailure)
            return $"Failed to allocate a {img.Width}x{img.Height} image (0x{hr.Value:X8})";

        var dst = scratch.GetImage(0, 0, 0);
        var rowBytes = img.Width * 4;
        var pixels = new byte[rowBytes * img.Height];
        img.CopyPixelDataTo(pixels);

        // Rows can be padded, so they are copied one at a time
        for (var y = 0; y < img.Height; y++)
        {
            pixels.AsSpan(y * rowBytes, rowBytes).CopyTo(new Span<byte>(dst->Pixels + (long)y * (long)dst->RowPitch, rowBytes));
        }

        return null;
    }

    private static Vortice.Direct3D11.ID3D11Device GetGpuDevice()
    {
        if (GpuTried)
            return GpuDevice;

        GpuTried = true;

        try
        {
            var hr = Vortice.Direct3D11.D3D11.D3D11CreateDevice(null, Vortice.Direct3D.DriverType.Hardware,
                Vortice.Direct3D11.DeviceCreationFlags.None, [Vortice.Direct3D.FeatureLevel.Level_11_0], out GpuDevice);

            if (hr.Failure)
            {
                GpuDevice = null;
                Smithbox.LogError(typeof(TextureEncoder), LOC.Get("TEXVIEW_Encoder_Log_No_GPU", $"{hr}"));
                return null;
            }

            using var dxgi = GpuDevice.QueryInterface<Vortice.DXGI.IDXGIDevice>();
            using var adapter = dxgi.GetAdapter();
            GpuAdapterName = adapter.Description.Description;
        }
        catch (Exception e)
        {
            GpuDevice = null;
            Smithbox.LogError(typeof(TextureEncoder), LOC.Get("TEXVIEW_Encoder_Log_No_GPU", e.Message), e);
        }

        return GpuDevice;
    }

    /// <summary>Whether two legacy FourCCs name the same compression.</summary>
    private static bool IsEquivalentFourCC(string a, string b)
    {
        static string Canon(string c) => c switch
        {
            "BC4U" or "ATI1" => "BC4",
            "BC5U" or "ATI2" => "BC5",
            _ => c,
        };

        return Canon(a) == Canon(b);
    }

    public static int FullChain(int width, int height)
    {
        int levels = 1;
        while (width > 1 || height > 1)
        {
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
            levels++;
        }
        return levels;
    }

    private static TextureEncodeResult Fail(TextureEncodeResult result, string error)
    {
        result.Dds = null;
        result.Error = error;
        return result;
    }
}
