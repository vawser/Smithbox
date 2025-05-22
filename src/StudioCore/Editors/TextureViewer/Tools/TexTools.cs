using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextureViewer.Utils;
using StudioCore.Platform;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Tools;

public class TexTools
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexTools(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public string[] exportTypes = new[] { "DDS", "PNG", "BMP", "TGA", "TIFF", "JPEG", "WEBP" };

    public void ExportTextureHandler()
    {
        var currentTexture = Editor.Selection.PreviewTextureResource;
        var currentTextureName = Editor.Selection.SelectedTextureKey;

        if (currentTexture != null)
        {
            var filename = currentTextureName;
            var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

            if (exportPath != "")
            {
                var exportFilePath = $@"{exportPath}\{filename}";
                var write = true;

                if (File.Exists(exportFilePath))
                {
                    var result = PlatformUtils.Instance.MessageBox($"Overwrite existing file at {exportFilePath}?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        write = false;
                    }
                }

                if (!Directory.Exists(exportPath))
                {
                    write = false;
                    PlatformUtils.Instance.MessageBox($"Directory is not valid.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (write)
                {
                    if (CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder)
                    {
                        var folder = currentTextureName;
                        var newFolderPath = $@"{exportPath}\{folder}";

                        if (!Directory.Exists(newFolderPath))
                        {
                            Directory.CreateDirectory(newFolderPath);
                        }

                        exportFilePath = $@"{exportPath}\{folder}\{filename}";
                    }

                    ExportTextureFile(currentTexture, exportFilePath);

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

    private void ExportTextureFile(TextureResource texResource, string exportFilePath)
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
