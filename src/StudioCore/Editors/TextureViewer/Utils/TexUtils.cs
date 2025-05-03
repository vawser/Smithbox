using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StudioCore.Core;

namespace StudioCore.Editors.TextureViewer.Utils;

public static class TexUtils
{
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
