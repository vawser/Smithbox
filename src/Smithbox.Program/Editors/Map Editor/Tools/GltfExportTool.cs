using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer.Export;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Exports every currently loaded map's FLVER-backed parts (map pieces, characters, objects/assets) to a
/// single glTF 2.0 scene, with albedo and normal map textures exported alongside as PNG files.
/// </summary>
public class GltfExportTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private bool _isExporting;
    private string _lastMessage = "";
    private bool _lastSuccess;

    public GltfExportTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        GUI.WrappedText("Export every currently loaded map piece, character and object/asset model to a single glTF 2.0 scene, " +
            "with albedo and normal map textures alongside as PNG files. Parts that share a common name (minus a trailing " +
            "instance/part index, e.g. AEG099_000_0000 and AEG099_000_0001) are grouped under one node.");

        GUI.Spacer();
        GUI.SimpleHeader("Output Folder", "The folder the .gltf, .bin and PNG textures will be written into.");
        GUI.SinglelineTextInput("gltfExportOutputFolder", ref CFG.Current.MapEditor_GltfExport_OutputFolder);

        GUI.Spacer();
        ImGui.Checkbox("Include custom vertex normals##gltfExportIncludeNormals", ref CFG.Current.MapEditor_GltfExport_IncludeNormals);
        GUI.Tooltip("If unchecked, the glTF file won't store any per-vertex normal data at all, so Blender (or any other " +
            "importer) will compute its own normals straight from the mesh geometry instead of using the model's own. " +
            "Use this as a fallback if some meshes still render oddly-shaded.");

        GUI.Spacer();
        GUI.MultiButtonInput("gltfExportActions",
            "selectGltfExportDest", "Select Output Folder", "Choose the folder to export the glTF scene into.", SelectExportDestination,
            "openGltfExportDest", "Open Output Folder", "Open the export folder in Explorer.", OpenExportFolder,
            "exportGltf", "Export glTF", "Export all currently loaded models to a glTF scene.", ExportHandler);

        if (_isExporting)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.2f, 1.0f), "Exporting, this may take a while for large maps...");
        }
        else if (!string.IsNullOrEmpty(_lastMessage))
        {
            var color = _lastSuccess ? new Vector4(0.3f, 0.9f, 0.3f, 1.0f) : new Vector4(0.95f, 0.35f, 0.35f, 1.0f);
            GUI.WrappedTextColored(color, _lastMessage);
        }
    }

    private void SelectExportDestination()
    {
        if (PlatformUtils.Instance.OpenFolderDialog("Select glTF Export Folder", out var path))
        {
            CFG.Current.MapEditor_GltfExport_OutputFolder = path;
        }
    }

    private void OpenExportFolder()
    {
        if (Directory.Exists(CFG.Current.MapEditor_GltfExport_OutputFolder))
        {
            Process.Start("explorer.exe", CFG.Current.MapEditor_GltfExport_OutputFolder);
        }
    }

    private async void ExportHandler()
    {
        if (_isExporting)
            return;

        var outputFolder = CFG.Current.MapEditor_GltfExport_OutputFolder;

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            _lastSuccess = false;
            _lastMessage = "Set an output folder first.";
            return;
        }

        var loadedMaps = Project.Handler.MapData.PrimaryBank.Maps
            .Select(e => e.Value.MapContainer)
            .Where(c => c != null)
            .ToList();

        _isExporting = true;
        _lastMessage = "";

        try
        {
            var includeNormals = CFG.Current.MapEditor_GltfExport_IncludeNormals;
            var result = await Task.Run(() => GltfSceneExportService.ExportMapViewport(Project, loadedMaps, outputFolder, includeNormals));

            _lastSuccess = result.Success;

            if (result.Success)
            {
                _lastMessage = $"Exported {result.ExportedModelCount} model(s) and {result.ExportedTextureCount} texture(s) to '{result.OutputPath}'.";
                Smithbox.Log(this, _lastMessage);
            }
            else
            {
                _lastMessage = result.Message;
                Smithbox.LogError(this, $"glTF export failed: {result.Message}");
            }

            foreach (var warning in result.Warnings.Take(100))
            {
                Smithbox.LogWarning<GltfExportTool>(warning);
            }
        }
        catch (Exception e)
        {
            _lastSuccess = false;
            _lastMessage = $"Export failed: {e.Message}";
            Smithbox.LogError(this, "glTF export failed", e);
        }
        finally
        {
            _isExporting = false;
        }
    }
}
