using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace StudioCore.Editors.TextureViewer;

public static class TexUtils
{
    /// <summary>
    /// The texture as a headered DDS file: PC payloads already are one, console payloads are headerized
    /// (and deswizzled / ASTC-decoded for Switch 2). Writing a console payload as it is gives an unreadable file.
    /// </summary>
    public static byte[] GetDdsBytes(SoulsFormats.TPF tpf, SoulsFormats.TPF.Texture tex)
    {
        var platform = tpf?.Platform ?? tex.Platform;
        return platform == SoulsFormats.TPF.TPFPlatform.PC ? tex.Bytes : tex.Headerize();
    }

    /// <summary>
    /// Writes a DDS file (headered) as the export type. Images are decoded with DirectXTex (DdsDecoder), which reads
    /// every BC format the games use; Pfim, which can't read BC5/BC7, is only the fallback.
    /// </summary>
    public static void ExportImage(string exportFilePath, byte[] bytes, TextureExportType exportType)
    {
        if (exportType != TextureExportType.DDS)
        {
            using var decoded = DdsDecoder.DecodeToImage(bytes, out _);

            if (decoded != null)
            {
                var path = $"{exportFilePath}{GetExportExtension(exportType)}";
                switch (exportType)
                {
                    case TextureExportType.PNG: decoded.SaveAsPng(path); return;
                    case TextureExportType.BMP: decoded.SaveAsBmp(path); return;
                    case TextureExportType.TGA: decoded.SaveAsTga(path); return;
                    case TextureExportType.TIFF: decoded.SaveAsTiff(path); return;
                    case TextureExportType.JPEG: decoded.SaveAsJpeg(path); return;
                    case TextureExportType.WEBP: decoded.SaveAsWebp(path); return;
                }
            }
        }

        switch (exportType)
        {
            case TextureExportType.DDS: ExportDDSImage(exportFilePath, bytes); break;
            case TextureExportType.PNG: ExportPNGImage(exportFilePath, bytes); break;
            case TextureExportType.BMP: ExportBMPImage(exportFilePath, bytes); break;
            case TextureExportType.TGA: ExportTGAImage(exportFilePath, bytes); break;
            case TextureExportType.TIFF: ExportTIFFImage(exportFilePath, bytes); break;
            case TextureExportType.JPEG: ExportJPEGImage(exportFilePath, bytes); break;
            case TextureExportType.WEBP: ExportWEBPImage(exportFilePath, bytes); break;
        }
    }

    /// <summary>
    /// The extension the export functions append, which is the lowercase name of each type
    /// </summary>
    public static string GetExportExtension(TextureExportType exportType)
    {
        return $".{exportType.ToString().ToLowerInvariant()}";
    }

    public static void ExportDDSImage(string exportFilePath, byte[] bytes)
    {
        File.WriteAllBytes($"{exportFilePath}.dds", bytes);
    }

    public static void ExportPNGImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static void ExportBMPImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static void ExportTGAImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static void ExportTIFFImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static void ExportJPEGImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static void ExportWEBPImage(string exportFilePath, byte[] bytes)
    {
        Pfim.IImage _image = GetPfimImage(bytes);

        if (_image.Format == Pfim.ImageFormat.Rgba32)
        {
            Image<Bgra32> image = Image.LoadPixelData<Bgra32>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");

        }
        else if (_image.Format == Pfim.ImageFormat.Rgb24)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");
        }
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<L8> image = Image.LoadPixelData<L8>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");
        }
        else
        {
            throw new Exception(LOC.Get("REND_Unsupported_Pixel_Format", _image.Format));
        }
    }

    public static Pfim.IImage GetPfimImage(byte[] bytes)
    {
        Pfim.IImage _image = Pfim.Dds.Create(bytes, new Pfim.PfimConfig());

        if (_image.Compressed)
        {
            _image.Decompress();
        }

        return _image;
    }
}
