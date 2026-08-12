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

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Exports the model currently open in the Model Editor viewport to a glTF 2.0 scene, with albedo and
/// normal map textures exported alongside as PNG files. Every mesh in the FLVER becomes a primitive within
/// a single glTF mesh, so parts that already belong to one model stay grouped together.
/// </summary>
public class GltfExportTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private bool _isExporting;
    private string _lastMessage = "";
    private bool _lastSuccess;

    public GltfExportTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        GUI.WrappedText("Export the model currently open in the viewport to a glTF 2.0 scene, with albedo and normal map " +
            "textures alongside as PNG files. All meshes/parts of the model are kept together under one node.");

        GUI.Spacer();
        GUI.SimpleHeader("Output Folder", "The folder the .gltf, .bin and PNG textures will be written into.");
        GUI.SinglelineTextInput("modelGltfExportOutputFolder", ref CFG.Current.ModelEditor_GltfExport_OutputFolder);

        GUI.Spacer();
        ImGui.Checkbox("Include custom vertex normals##modelGltfExportIncludeNormals", ref CFG.Current.ModelEditor_GltfExport_IncludeNormals);
        GUI.Tooltip("If unchecked, the glTF file won't store any per-vertex normal data at all, so Blender (or any other " +
            "importer) will compute its own normals straight from the mesh geometry instead of using the model's own. " +
            "Use this as a fallback if some meshes still render oddly-shaded.");

        GUI.Spacer();
        GUI.MultiButtonInput("modelGltfExportActions",
            "selectModelGltfExportDest", "Select Output Folder", "Choose the folder to export the glTF scene into.", SelectExportDestination,
            "openModelGltfExportDest", "Open Output Folder", "Open the export folder in Explorer.", OpenExportFolder,
            "exportModelGltf", "Export glTF", "Export the currently loaded model to a glTF scene.", ExportHandler);

        if (_isExporting)
        {
            ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.2f, 1.0f), "Exporting...");
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
            CFG.Current.ModelEditor_GltfExport_OutputFolder = path;
        }
    }

    private void OpenExportFolder()
    {
        if (Directory.Exists(CFG.Current.ModelEditor_GltfExport_OutputFolder))
        {
            Process.Start("explorer.exe", CFG.Current.ModelEditor_GltfExport_OutputFolder);
        }
    }

    private async void ExportHandler()
    {
        if (_isExporting)
            return;

        var outputFolder = CFG.Current.ModelEditor_GltfExport_OutputFolder;

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            _lastSuccess = false;
            _lastMessage = "Set an output folder first.";
            return;
        }

        var modelWrapper = View.Selection.SelectedModelWrapper;

        if (modelWrapper == null)
        {
            _lastSuccess = false;
            _lastMessage = "No model is currently loaded in the viewport.";
            return;
        }

        _isExporting = true;
        _lastMessage = "";

        try
        {
            var includeNormals = CFG.Current.ModelEditor_GltfExport_IncludeNormals;
            var result = await Task.Run(() => GltfSceneExportService.ExportModelViewport(Project, modelWrapper, outputFolder, includeNormals));

            _lastSuccess = result.Success;

            if (result.Success)
            {
                _lastMessage = $"Exported {result.ExportedTextureCount} texture(s) to '{result.OutputPath}'.";
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
