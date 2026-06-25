using Hexa.NET.ImGui;
using Microsoft.Extensions.FileSystemGlobbing;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TextureExport
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    private TextureExportModal ExportModal;

    public TextureExport(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ExportModal = new TextureExportModal("Texture Export", this);
    }

    public string[] exportTypes = new[] { "DDS", "PNG", "BMP", "TGA", "TIFF", "JPEG", "WEBP" };

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Data Export"))
        {
            ImGui.BeginChild("TextureExportToolSection", ImGuiChildFlags.Borders);

            UIHelper.WrappedText("Export the currently viewed texture.");
            UIHelper.WrappedText("");

            var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Export", "");

            UIHelper.SinglelineTextInput("exportDestinationInput", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation);

            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Export File Type", "");

            DPI.ApplyInputWidth(windowWidth * 0.5f);
            if (ImGui.Combo("##ExportType", ref index, exportTypes, exportTypes.Length))
            {
                CFG.Current.TextureViewerToolbar_ExportTextureType = index;
            }
            UIHelper.Tooltip("The file type the exported texture will be saved as. Affects the context menu export functions too.");

            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Options", "");
            ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
            UIHelper.Tooltip("Place the exported texture in a folder with the title of the texture container.");

            ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
            UIHelper.Tooltip("Display the confirmation message box after each export.");

            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Actions", "");
            UIHelper.MultiButtonInput("exportDestinationActions",
                "selectExportDest", "Select Export Folder", "", SelectExportDestination,
                "openExportDest", "Open Export Folder", "", OpenExportFolder,
                "export", "Export Texture", "", ExportTextureHandler);

            ImGui.EndChild();
        }

        ExportModal.Draw();
    }

    public void SelectExportDestination()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
        if (result)
        {
            CFG.Current.TextureViewerToolbar_ExportTextureLocation = path;
        }
    }

    public void OpenExportFolder()
    {
        Process.Start("explorer.exe", CFG.Current.TextureViewerToolbar_ExportTextureLocation);
    }

    public void ExportTextureHandler()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var currentTexture = activeView.Selection.ViewerTextureResource;
        var currentTextureName = activeView.Selection.SelectedTextureKey;

        if (currentTexture != null)
        {
            var filename = currentTextureName;
            var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

            if (exportPath != "")
            {
                var exportFilePath = Path.Join(exportPath, filename);
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
                        var newFolderPath = Path.Join(exportPath, folder);

                        if (!Directory.Exists(newFolderPath))
                        {
                            Directory.CreateDirectory(newFolderPath);
                        }

                        exportFilePath = Path.Join(exportPath, folder, filename);
                    }

                    ExportTextureFileFromTextureResource(currentTexture, exportFilePath);

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

    private void ExportTextureFileFromTextureResource(TextureResource texResource, string exportFilePath)
    {
        var tex = texResource.GPUTexture.TpfTexture;
        var bytes = tex.Bytes.ToArray();

        ExportTextureFile(exportFilePath, bytes);
    }

    private void ExportTextureFile(string exportFilePath, byte[] bytes)
    {
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

    private BinderContents TargetBinderContents;
    private TPF TargetTPF;
    private string TargetTpfName;
    private TPF.Texture TargetTex;
    private bool ProgressTextures = true;

    public async Task ExportTPFsFromContainerAsync(BinderContents binderContents)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            TargetBinderContents = binderContents;
            await Task.Run(ExportTPFsFromContainer);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Texture export failed", ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTPFsFromContainer()
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        int processed = 0;
        var total = TargetBinderContents.Files.Count;

        if (Directory.Exists(exportPath))
        {
            foreach (var file in TargetBinderContents.Files)
            {
                processed++;

                ExportModal.ReportProgress?.Invoke(new()
                {
                    PhaseLabel = "Processing TPF",
                    StepLabel = $"{file.Key.Name}",
                    Percent = processed / (float)total
                });

                ExportTPF(file.Value, file.Key.Name);
            }
        }
    }

    public async Task ExportTPFAsync(TPF tpf, string filename)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            TargetTpfName = filename;
            TargetTPF = tpf;
            await Task.Run(ExportTPF);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Texture export failed", ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTPF()
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        if (Directory.Exists(exportPath))
        {
            var filePath = $"{Path.GetFileName(TargetTpfName)}.tpf.dcx";
            var data = TargetTPF.Write();
            var filepath = Path.Join(exportPath, filePath);

            File.WriteAllBytes(filepath, data);
        }
    }

    public void ExportTPF(TPF tpf, string filename)
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        if (Directory.Exists(exportPath))
        {
            var filePath = $"{Path.GetFileName(filename)}.tpf.dcx";
            var data = tpf.Write();
            var filepath = Path.Join(exportPath, filePath);

            File.WriteAllBytes(filepath, data);
        }
    }

    public async Task ExportTexturesFromContainerAsync(BinderContents binderContents)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            TargetBinderContents = binderContents;
            await Task.Run(ExportTexturesFromContainer);

        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Texture export failed", ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexturesFromContainer()
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        int processed = 0;
        var total = TargetBinderContents.Files.Count;

        if (Directory.Exists(exportPath))
        {
            foreach (var file in TargetBinderContents.Files)
            {
                var tpf = file.Value;

                processed++;

                ExportModal.ReportProgress?.Invoke(new()
                {
                    PhaseLabel = "Processing TPF",
                    StepLabel = $"{file.Key.Name}",
                    Percent = processed / (float)total
                });

                TargetTPF = tpf;

                ProgressTextures = false;
                ExportTexturesFromTPF();
            }
        }
    }

    public async Task ExportTexturesFromTPFAsync(TPF tpf)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            TargetTPF = tpf;
            ProgressTextures = true;
            await Task.Run(ExportTexturesFromTPF);

        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Texture export failed", ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexturesFromTPF()
    {
        int processed = 0;
        var total = TargetTPF.Textures.Count;

        foreach (var tex in TargetTPF.Textures)
        {
            processed++;

            if (ProgressTextures)
            {
                ExportModal.ReportProgress?.Invoke(new()
                {
                    PhaseLabel = "Processing Texture",
                    StepLabel = $"{tex.Name}",
                    Percent = processed / (float)total
                });
            }

            ExportTexture(tex);
        }
    }

    public async Task ExportTextureAsync(TPF.Texture tex)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            TargetTex = tex;
            await Task.Run(ExportTexture);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Texture export failed", ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexture()
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        var texFilename = TargetTex.Name;
        var data = TargetTex.Bytes;
        var filepath = Path.Join(exportPath, $"{texFilename}");

        // For DDS, we can write to file directly (makes it quick).
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 0)
        {
            File.WriteAllBytes($"{filepath}.dds", data);
        }
        else
        {
            ExportTextureFile(filepath, data);
        }
    }

    public void ExportTexture(TPF.Texture tex)
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        var texFilename = tex.Name;
        var data = tex.Bytes;
        var filepath = Path.Join(exportPath, $"{texFilename}");

        // For DDS, we can write to file directly (makes it quick).
        if (CFG.Current.TextureViewerToolbar_ExportTextureType == 0)
        {
            File.WriteAllBytes($"{filepath}.dds", data);
        }
        else
        {
            ExportTextureFile(filepath, data);
        }
    }
}
