using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace StudioCore.Editors.TextureViewer;

public static class TexUtils
{
    public static void ExportImage(string exportFilePath, byte[] bytes, TextureExportType exportType)
    {
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
