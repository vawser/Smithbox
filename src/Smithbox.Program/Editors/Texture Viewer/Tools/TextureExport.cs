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
    public TexEditorView View;
    public ProjectEntry Project;

    private TextureExportModal ExportModal;

    public TextureExport(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        ExportModal = new TextureExportModal(this);
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader($"{LOC.Get("TEXVIEW_TexExport_Header_Data_Export")}##dataExportHeader"))
        {
            ImGui.BeginChild("TextureExportToolSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("TEXVIEW_TexExport_Header_Data_Export_Hint"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXVIEW_TexExport_Header_Export_Directory"),
                LOC.Get("TEXVIEW_TexExport_Header_Export_Directory_TT"));

            GUI.SinglelineTextInput("exportDestinationInput", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXVIEW_TexExport_Header_Export_File_Type"),
                LOC.Get("TEXVIEW_TexExport_Header_Export_File_Type_TT"));

            var previewName = LOC.Get(
                CFG.Current.TextureViewerToolbar_ExportTextureType.GetDisplayName());

            DPI.ApplyInputWidth();
            if (ImGui.BeginCombo("##ExportType", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(TextureExportType)))
                {
                    var type = (TextureExportType)entry;

                    var displayName = LOC.Get(type.GetDisplayName());

                    if (ImGui.Selectable(displayName))
                    {
                        CFG.Current.TextureViewerToolbar_ExportTextureType = type;
                    }
                }
                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_TexExport_Export_Type_Select_TT"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXVIEW_TexExport_Header_Options"),
                LOC.Get("TEXVIEW_TexExport_Header_Options_TT"));

            ImGui.Checkbox(
                $"{LOC.Get("TEXVIEW_TexExport_Checkbox_Include_Container_Folder")}##includeContainerFolderToggle", 
                ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
            GUI.Tooltip(LOC.Get("TEXVIEW_TexExport_Checkbox_Include_Container_Folder_TT"));

            ImGui.Checkbox(
                $"{LOC.Get("TEXVIEW_TexExport_Checkbox_Display_Export_Confirm")}##exportConfirmToggle", 
                ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
            GUI.Tooltip(LOC.Get("TEXVIEW_TexExport_Checkbox_Display_Export_Confirm_TT"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXVIEW_TexExport_Header_Actions"),
                LOC.Get("TEXVIEW_TexExport_Header_Actions_TT"));

            GUI.MultiButtonInput("exportDestinationActions",
                "selectExportDest", 
                LOC.Get("TEXVIEW_TexExport_Action_Select_Export_Folder"),
                LOC.Get("TEXVIEW_TexExport_Action_Select_Export_Folder_TT"),
                SelectExportDestination,

                "openExportDest", 
                LOC.Get("TEXVIEW_TexExport_Action_Open_Export_Folder"),
                LOC.Get("TEXVIEW_TexExport_Action_Open_Export_Folder_TT"),
                OpenExportFolder,

                "export", 
                LOC.Get("TEXVIEW_TexExport_Action_Export_Texture"),
                LOC.Get("TEXVIEW_TexExport_Action_Export_Texture_TT"),
                ExportTextureHandler);

            ImGui.EndChild();
        }

        ExportModal.Draw();
    }

    public void SelectExportDestination()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("TEXVIEW_TexExport_Dialog_Select_Export_Dest"), out path);
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
        var currentTexture = View.Selection.ViewerTextureResource;
        var currentTextureName = View.Selection.SelectedTextureKey;

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
                    var result = PlatformUtils.Instance.MessageBox(
                        LOC.Get("TEXVIEW_TexExport_Dialog_Overwrite_File", exportFilePath), 
                        LOC.Get("SYS_Warning_Header"), 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        write = false;
                    }
                }

                if (!Directory.Exists(exportPath))
                {
                    write = false;
                    Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Invalid_Directory"));
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
                        Smithbox.Log(this, LOC.Get("TEXVIEW_TexExport_Log_Exported_Texture", filename));
                    }
                }
            }
            else
            {
                Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Export_Dir_Not_Set"));
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_No_Texture_Viewed"));
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
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.DDS)
        {
            TexUtils.ExportDDSImage(exportFilePath, bytes);
        }

        // PNG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.PNG)
        {
            TexUtils.ExportPNGImage(exportFilePath, bytes);
        }

        // BMP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.BMP)
        {
            TexUtils.ExportBMPImage(exportFilePath, bytes);
        }

        // TGA
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.TGA)
        {
            TexUtils.ExportTGAImage(exportFilePath, bytes);
        }

        // TIFF
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.TIFF)
        {
            TexUtils.ExportTIFFImage(exportFilePath, bytes);
        }

        // JPEG
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.JPEG)
        {
            TexUtils.ExportJPEGImage(exportFilePath, bytes);
        }

        // WEBP
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.WEBP)
        {
            TexUtils.ExportWEBPImage(exportFilePath, bytes);
        }
    }

    public async Task ExportTPFsFromContainerAsync(BinderContents binderContents)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            await Task.Run(() => ExportTPFsFromContainer(binderContents));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTPFsFromContainer(BinderContents binderContents)
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        int processed = 0;
        var total = binderContents.Files.Count;

        if (Directory.Exists(exportPath))
        {
            foreach (var file in binderContents.Files)
            {
                processed++;

                ExportModal.ReportProgress?.Invoke(new()
                {
                    PhaseLabel = LOC.Get("TEXVIEW_TexExport_Phase_Processing_TPF"),
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
            await Task.Run(() => ExportTPF(tpf, filename));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
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
            await Task.Run(() => ExportTexturesFromContainer(binderContents));

        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexturesFromContainer(BinderContents binderContents)
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        int processed = 0;
        var total = binderContents.Files.Count;

        if (Directory.Exists(exportPath))
        {
            foreach (var file in binderContents.Files)
            {
                var tpf = file.Value;

                processed++;

                if(binderContents.Files.Count > 1)
                {
                    ExportModal.ReportProgress?.Invoke(new()
                    {
                        PhaseLabel = LOC.Get("TEXVIEW_TexExport_Phase_Processing_TPF"),
                        StepLabel = $"{file.Key.Name}",
                        Percent = processed / (float)total
                    });

                    ExportTexturesFromTPF(tpf, false);
                }
                else
                {
                    ExportTexturesFromTPF(tpf, true);
                }
            }
        }
    }

    public async Task ExportTexturesFromTPFAsync(TPF tpf)
    {
        ExportModal.DisplayModal = true;
        ExportModal.InitialLayout = false;

        try
        {
            await Task.Run(() => ExportTexturesFromTPF(tpf, true));

        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexturesFromTPF(TPF tpf, bool displayProgress)
    {
        int processed = 0;
        var total = tpf.Textures.Count;

        foreach (var tex in tpf.Textures)
        {
            processed++;

            if (displayProgress)
            {
                ExportModal.ReportProgress?.Invoke(new()
                {
                    PhaseLabel = LOC.Get("TEXVIEW_TexExport_Phase_Processing_Texture"),
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
            await Task.Run(() => ExportTexture(tex));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_TexExport_Log_Texture_Export_Failed"), ex);
        }
        finally
        {
            ExportModal.DisplayModal = false;
        }
    }

    public void ExportTexture(TPF.Texture tex)
    {
        var exportPath = CFG.Current.TextureViewerToolbar_ExportTextureLocation;

        var texFilename = tex.Name;
        var data = tex.Bytes;
        var filepath = Path.Join(exportPath, $"{texFilename}");

        // For DDS, we can write to file directly (makes it quick).
        if (CFG.Current.TextureViewerToolbar_ExportTextureType is TextureExportType.DDS)
        {
            File.WriteAllBytes($"{filepath}.dds", data);
        }
        else
        {
            ExportTextureFile(filepath, data);
        }
    }
}
