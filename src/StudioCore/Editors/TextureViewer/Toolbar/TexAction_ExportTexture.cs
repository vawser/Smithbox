using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Numerics;
using StudioCore.TextureViewer;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using Org.BouncyCastle.Utilities;

namespace StudioCore.Editors.TextureViewer.Toolbar;

public static class TexAction_ExportTexture
{
    private static string[] exportTypes = new[]{ "DDS", "PNG", "BMP", "TGA", "TIFF", "JPEG", "WEBP" };

    public static void Select()
    {
        if (ImGui.RadioButton("Export Texture##tool_ExportTexture", TextureToolbar.SelectedAction == TextureViewerAction.ExportTexture))
        {
            TextureToolbar.SelectedAction = TextureViewerAction.ExportTexture;
        }
        ImguiUtils.ShowHoverTooltip("Use this to export the currently viewed texture.");
    }

    public static void Configure()
    {
        if (TextureToolbar.SelectedAction == TextureViewerAction.ExportTexture)
        {
            ImGui.Text("Export the viewed texture.");
            ImGui.Text("");

            var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

            ImGui.Text("Export File Type:");
            if (ImGui.Combo("##ExportType", ref index, exportTypes, exportTypes.Length))
            {
                CFG.Current.TextureViewerToolbar_ExportTextureType = index;
            }
            ImguiUtils.ShowHoverTooltip("The file type the exported texture will be saved as.");
            ImGui.Text("");

            ImGui.Text("Export Destination:");
            ImGui.InputText("##exportDestination", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation, 255);
            if(ImGui.Button("Select"))
            {
                string path;
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
                if(result)
                {
                    CFG.Current.TextureViewerToolbar_ExportTextureLocation = path;
                }
            }
            ImguiUtils.ShowHoverTooltip("The folder destination to export the texture to.");
            ImGui.Text("");

            ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
            ImguiUtils.ShowHoverTooltip("Place the exported texture in a folder with the title of the texture container.");

            ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
            ImguiUtils.ShowHoverTooltip("Display the confirmation message box after each export.");
            ImGui.Text("");
        }
    }

    public static void Act()
    {
        if (TextureToolbar.SelectedAction == TextureViewerAction.ExportTexture)
        {
            if (ImGui.Button("Apply##action_Selection_ExportTexture", new Vector2(200, 32)))
            {
                if (CFG.Current.Interface_TextureViewer_PromptUser)
                {
                    var result = PlatformUtils.Instance.MessageBox($"You are about to use the Export Texture action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        ExportTexture();
                    }
                }
                else
                {
                    ExportTexture();
                }
            }
        }
    }

    public static void ExportTexture()
    {
        if(TextureViewerScreen.CurrentTextureInView != null)
        {
            var filename = TextureViewerScreen.CurrentTextureName;

            if (CFG.Current.TextureViewerToolbar_ExportTextureLocation != "")
            {
                var exportFilePath = $@"{CFG.Current.TextureViewerToolbar_ExportTextureLocation}\{filename}";
                var write = true;

                if (File.Exists(exportFilePath))
                {
                    var result = PlatformUtils.Instance.MessageBox($"Overwrite existing file at {exportFilePath}?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result == DialogResult.No)
                    {
                        write = false;
                    }
                }

                if (write)
                {
                    if(CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder)
                    {
                        var folder = TextureViewerScreen.CurrentTextureContainerName;
                        var newFolderPath = $@"{CFG.Current.TextureViewerToolbar_ExportTextureLocation}\{folder}";

                        if(!Directory.Exists(newFolderPath))
                        {
                            Directory.CreateDirectory(newFolderPath);
                        }

                        exportFilePath = $@"{CFG.Current.TextureViewerToolbar_ExportTextureLocation}\{folder}\{filename}";
                    }

                    ExportTextureFile(exportFilePath);

                    if (CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm)
                    {
                        PlatformUtils.Instance.MessageBox($"{filename} exported.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"Export Destination is not set.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox($"No texture is currently being viewed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private static void ExportTextureFile(string exportFilePath)
    {
        var tex = TextureViewerScreen.CurrentTextureInView.GPUTexture.TpfTexture;
        var bytes = tex.Bytes.ToArray();

        // DDS
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 0)
        {
            File.WriteAllBytes($"{exportFilePath}.dds", bytes);
        }

        // PNG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 1)
        {
            ExportPNGImage(exportFilePath, bytes);
        }

        // BMP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 2)
        {
            ExportBMPImage(exportFilePath, bytes);
        }

        // TGA
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 3)
        {
            ExportTGAImage(exportFilePath, bytes);
        }

        // TIFF
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 4)
        {
            ExportTIFFImage(exportFilePath, bytes);
        }

        // JPEG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 5)
        {
            ExportJPEGImage(exportFilePath, bytes);
        }

        // WEBP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 6)
        {
            ExportWEBPImage(exportFilePath, bytes);
        }
    }

    private static void ExportPNGImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static void ExportBMPImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static void ExportTGAImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static void ExportTIFFImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static void ExportJPEGImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static void ExportWEBPImage(string exportFilePath, byte[] bytes)
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
        else
        {
            throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }
    }

    private static Pfim.IImage GetPfimImage(byte[] bytes)
    {
        Pfim.IImage _image = Pfim.Dds.Create(bytes, new Pfim.PfimConfig());

        if (_image.Compressed)
        {
            _image.Decompress();
        }

        return _image;
    }
}
