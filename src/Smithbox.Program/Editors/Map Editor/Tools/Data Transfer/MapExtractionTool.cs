using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Exports the selected map objects as one OBJ, placed as they appear in the viewport, with their textures
/// </summary>
public class MapExtractionTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private MapObjExporter ActiveExport;
    private CancellationTokenSource ExportCancel;
    private volatile bool ExportRunning;
    private string StatusText = "";

    public MapExtractionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAP_MapExtraction_Header")}##mapExtractionToolHeader"))
            return;

        ImGui.BeginChild("MapExtractionToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_MapExtraction_Description"));

        var selection = View.ViewportSelection.GetSelection().OfType<Entity>().ToList();

        // Selection
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Selection_Header"), LOC.Get("MAP_MapExtraction_Selection_Header_TT"));

        if (selection.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MapExtraction_Selection_None"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_MapExtraction_Selection_Count", selection.Count));
        }

        // Output Directory
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Output_Header"), LOC.Get("MAP_MapExtraction_Output_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##mapExtractionOutputDir", ref CFG.Current.MapEditor_MapExtraction_OutputDirectory, 1024);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_Output_Input_TT"));

        GUI.MultiButtonInput("mapExtractionOutputActions",
            "mapExtractionSetDir", LOC.Get("MAP_MapExtraction_Set_Output"), LOC.Get("MAP_MapExtraction_Set_Output_TT"), SetOutputDirectory,
            "mapExtractionOpenDir", LOC.Get("MAP_MapExtraction_Open_Output"), LOC.Get("MAP_MapExtraction_Open_Output_TT"), OpenOutputDirectory);

        // File Name
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_File_Name_Header"), LOC.Get("MAP_MapExtraction_File_Name_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##mapExtractionFileName", ref CFG.Current.MapEditor_MapExtraction_FileName, 255);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_File_Name_Input_TT"));

        // Textures
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Textures_Header"), LOC.Get("MAP_MapExtraction_Textures_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapExtraction_Include_Textures")}##mapExtractionIncludeTextures", ref CFG.Current.MapEditor_MapExtraction_IncludeTextures);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_Include_Textures_TT"));

        if (CFG.Current.MapEditor_MapExtraction_IncludeTextures)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_MapExtraction_Include_Material_Maps")}##mapExtractionIncludeMaps", ref CFG.Current.MapEditor_MapExtraction_IncludeMaterialMaps);
            GUI.Tooltip(LOC.Get("MAP_MapExtraction_Include_Material_Maps_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##mapExtractionTextureFormat", LOC.Get($"MAP_MapExtraction_Format_{CFG.Current.MapEditor_MapExtraction_TextureFormat}")))
            {
                foreach (var format in Enum.GetValues<MapExtractionTextureFormat>())
                {
                    if (ImGui.Selectable($"{LOC.Get($"MAP_MapExtraction_Format_{format}")}##mapExtractionFormat{format}",
                        format == CFG.Current.MapEditor_MapExtraction_TextureFormat))
                    {
                        CFG.Current.MapEditor_MapExtraction_TextureFormat = format;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MAP_MapExtraction_Format_TT"));
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Options_Header"), LOC.Get("MAP_MapExtraction_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapExtraction_Centre")}##mapExtractionCentre", ref CFG.Current.MapEditor_MapExtraction_CentreAtOrigin);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_Centre_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapExtraction_Flip_UVs")}##mapExtractionFlipUVs", ref CFG.Current.MapEditor_MapExtraction_FlipUVs);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_Flip_UVs_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapExtraction_Mirror_X")}##mapExtractionMirrorX", ref CFG.Current.MapEditor_MapExtraction_MirrorX);
        GUI.Tooltip(LOC.Get("MAP_MapExtraction_Mirror_X_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Actions_Header"), LOC.Get("MAP_MapExtraction_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("mapExtractionActions",
            "mapExtractionExport", LOC.Get("MAP_MapExtraction_Export"), LOC.Get("MAP_MapExtraction_Export_TT"),
            () => StartExport(selection), selection.Count > 0 && !ExportRunning);

        DisplayProgress();
    }

    private void DisplayProgress()
    {
        var export = ActiveExport;

        if (!ExportRunning && string.IsNullOrEmpty(StatusText))
            return;

        // Progress
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapExtraction_Progress_Header"), "");

        if (ExportRunning && export != null)
        {
            var total = Math.Max(export.TotalParts, 1);
            var fraction = Math.Min(export.ProcessedParts, total) / (float)total;

            ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MAP_MapExtraction_Progress", export.ProcessedParts, export.TotalParts));

            GUI.ConditionalMultiButtonInput("mapExtractionCancelActions",
                "mapExtractionCancel", LOC.Get("MAP_MapExtraction_Cancel"), LOC.Get("MAP_MapExtraction_Cancel_TT"),
                () => ExportCancel?.Cancel(), true);
        }

        if (!string.IsNullOrEmpty(StatusText))
        {
            GUI.WrappedText(StatusText);
        }
    }

    private void StartExport(List<Entity> selection)
    {
        var outputDirectory = CFG.Current.MapEditor_MapExtraction_OutputDirectory;

        if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
        {
            StatusText = LOC.Get("MAP_MapExtraction_Error_Output");
            Smithbox.LogError(this, StatusText);
            return;
        }

        var fileName = MapObjExporter.Sanitize(Path.GetFileNameWithoutExtension(CFG.Current.MapEditor_MapExtraction_FileName ?? ""));

        // Everything the export needs from the viewport is read here, on the main thread
        var skipped = new List<string>();
        var parts = MapObjExporter.CollectParts(selection, skipped, out var bounds);

        foreach (var reason in skipped)
        {
            Smithbox.Log(this, LOC.Get("MAP_MapExtraction_Skipped", reason));
        }

        if (parts.Count == 0)
        {
            StatusText = LOC.Get("MAP_MapExtraction_Error_No_Models");
            Smithbox.LogError(this, StatusText);
            return;
        }

        var centre = CFG.Current.MapEditor_MapExtraction_CentreAtOrigin && bounds != null
            ? (bounds.Value.Min + bounds.Value.Max) / 2
            : Vector3.Zero;

        var options = new MapExtractionOptions(
            outputDirectory,
            fileName,
            CFG.Current.MapEditor_MapExtraction_IncludeTextures,
            CFG.Current.MapEditor_MapExtraction_IncludeMaterialMaps,
            CFG.Current.MapEditor_MapExtraction_TextureFormat,
            CFG.Current.MapEditor_MapExtraction_FlipUVs,
            CFG.Current.MapEditor_MapExtraction_MirrorX,
            CFG.Current.MapEditor_MapExtraction_CentreAtOrigin);

        var export = new MapObjExporter(Project, options, parts, centre);
        var cancel = new CancellationTokenSource();

        ExportCancel?.Cancel();
        ExportCancel = cancel;
        ActiveExport = export;
        ExportRunning = true;
        StatusText = "";

        Task.Run(() =>
        {
            string status;

            try
            {
                if (export.Run(cancel.Token, out var objPath))
                {
                    status = LOC.Get("MAP_MapExtraction_Complete", export.WrittenPartCount, export.WrittenTextureCount, objPath);
                    Smithbox.Log(this, status);
                }
                else
                {
                    status = LOC.Get("MAP_MapExtraction_Error_None_Written");
                    Smithbox.LogError(this, status);
                }

                foreach (var warning in export.Warnings)
                {
                    Smithbox.Log(this, warning);
                }

                if (export.Warnings.Count > 0)
                {
                    status += "\n" + LOC.Get("MAP_MapExtraction_Warnings", export.Warnings.Count);
                }

                if (skipped.Count > 0)
                {
                    status += "\n" + LOC.Get("MAP_MapExtraction_Skipped_Count", skipped.Count);
                }
            }
            catch (OperationCanceledException)
            {
                status = LOC.Get("MAP_MapExtraction_Cancelled");
            }
            catch (Exception e)
            {
                status = LOC.Get("MAP_MapExtraction_Error_Failed", e.Message);
                Smithbox.LogError(this, status, e);
            }

            StatusText = status;
            ExportRunning = false;
        });
    }

    private void SetOutputDirectory()
    {
        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("MAP_MapExtraction_Set_Output"), out var path))
        {
            CFG.Current.MapEditor_MapExtraction_OutputDirectory = path;
        }
    }

    private void OpenOutputDirectory()
    {
        var path = CFG.Current.MapEditor_MapExtraction_OutputDirectory;

        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            Process.Start("explorer.exe", path);
        }
    }
}
