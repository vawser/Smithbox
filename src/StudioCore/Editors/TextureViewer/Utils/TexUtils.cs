using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StudioCore.Core.Project;

namespace StudioCore.Editors.TextureViewer.Utils;

public static class TexUtils
{
    /// <summary>
    /// Whether the current project type supports the Texture Viewer
    /// </summary>
    public static bool IsSupportedProjectType()
    {
        // Need to add PS3 deswizzling support for these to work
        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.AC4 or ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
        {
            return false;
        }

        return true;
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsPng($"{exportFilePath}.png");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsBmp($"{exportFilePath}.bmp");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTga($"{exportFilePath}.tga");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsTiff($"{exportFilePath}.tiff");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsJpeg($"{exportFilePath}.jpeg");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
        // Use 24 since 16 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgba16)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");
        }
        // Use 24 since 8 isn't directly supported in ImageSharp
        else if (_image.Format == Pfim.ImageFormat.Rgb8)
        {
            Image<Bgr24> image = Image.LoadPixelData<Bgr24>(_image.Data, _image.Width, _image.Height);
            image.SaveAsWebp($"{exportFilePath}.webp");
        }
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
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
