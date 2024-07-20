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
using StudioCore.Resource;

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
            ImguiUtils.WrappedText("Export the viewed texture.");
            ImguiUtils.WrappedText("");

            var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

            ImguiUtils.WrappedText("Export File Type:");
            if (ImGui.Combo("##ExportType", ref index, exportTypes, exportTypes.Length))
            {
                CFG.Current.TextureViewerToolbar_ExportTextureType = index;
            }
            ImguiUtils.ShowHoverTooltip("The file type the exported texture will be saved as.");
            ImguiUtils.WrappedText("");

            ImguiUtils.WrappedText("Export Destination:");
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
            ImguiUtils.WrappedText("");

            ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
            ImguiUtils.ShowHoverTooltip("Place the exported texture in a folder with the title of the texture container.");

            ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
            ImguiUtils.ShowHoverTooltip("Display the confirmation message box after each export.");
            ImguiUtils.WrappedText("");
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
                        ExportTexture(TextureViewerScreen.CurrentTextureInView, TextureViewerScreen.CurrentTextureName);
                    }
                }
                else
                {
                    ExportTexture(TextureViewerScreen.CurrentTextureInView, TextureViewerScreen.CurrentTextureName);
                }
            }
        }
    }

    public static void ExportTexture(TextureResource texResource, string texName)
    {
        if(texResource != null)
        {
            var filename = texName;
            var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

            if (exportPath != "")
            {
                var exportFilePath = $@"{exportPath}\{filename}";
                var write = true;

                if (File.Exists(exportFilePath))
                {
                    var result = PlatformUtils.Instance.MessageBox($"Overwrite existing file at {exportFilePath}?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result == DialogResult.No)
                    {
                        write = false;
                    }
                }

                if(!Directory.Exists(exportPath))
                {
                    write = false;
                    PlatformUtils.Instance.MessageBox($"Directory is not valid.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (write)
                {
                    if(CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder)
                    {
                        var folder = texName;
                        var newFolderPath = $@"{exportPath}\{folder}";

                        if(!Directory.Exists(newFolderPath))
                        {
                            Directory.CreateDirectory(newFolderPath);
                        }

                        exportFilePath = $@"{exportPath}\{folder}\{filename}";
                    }

                    ExportTextureFile(texResource, exportFilePath);

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

    private static void ExportTextureFile(TextureResource texResource, string exportFilePath)
    {
        var tex = texResource.GPUTexture.TpfTexture;
        var bytes = tex.Bytes.ToArray();

        // DDS
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 0)
        {
            TexUtils.ExportDDSImage(exportFilePath, bytes);
        }

        // PNG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 1)
        {
            TexUtils.ExportPNGImage(exportFilePath, bytes);
        }

        // BMP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 2)
        {
            TexUtils.ExportBMPImage(exportFilePath, bytes);
        }

        // TGA
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 3)
        {
            TexUtils.ExportTGAImage(exportFilePath, bytes);
        }

        // TIFF
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 4)
        {
            TexUtils.ExportTIFFImage(exportFilePath, bytes);
        }

        // JPEG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 5)
        {
            TexUtils.ExportJPEGImage(exportFilePath, bytes);
        }

        // WEBP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 6)
        {
            TexUtils.ExportWEBPImage(exportFilePath, bytes);
        }
    }
}
