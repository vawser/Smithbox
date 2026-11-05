using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelCommandQueue
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;
    public ModelCommandQueue(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;

        HandleLoadCommand(activeView, commands);
        HandleSelectCommand(activeView, commands);
    }

    public void HandleLoadCommand(ModelEditorView curView, string[] commands)
    {
        if (commands[0] != "load")
            return;

        if (commands.Length <= 1)
            return;

        var filename = commands[1];
        var modelType = commands.Length > 2 ? commands[2] : "";
        var modelFilename = commands.Length > 4 ? commands[4] : filename;
        var modelBank = modelType switch
        {
            "Character" => Project.Handler.ModelData.PrimaryBank.Characters,
            "Asset" => Project.Handler.ModelData.PrimaryBank.Assets,
            "Part" => Project.Handler.ModelData.PrimaryBank.Parts,
            "MapPiece" => Project.Handler.ModelData.PrimaryBank.MapPieces,
            _ => Project.Handler.ModelData.PrimaryBank.Models
        };

        var entry = modelBank
            .FirstOrDefault(e => e.Key.Filename.Equals(filename, System.StringComparison.OrdinalIgnoreCase));

        // DS2 stores all map-piece FLVERs for a map inside one <map ID>.mapbhd/.mapbdt container.
        if (entry.Value == null && modelType == "MapPiece" && commands.Length > 3)
        {
            var mapID = commands[3];
            entry = modelBank.FirstOrDefault(e =>
                e.Key.Filename.Equals(mapID, System.StringComparison.OrdinalIgnoreCase));
        }

        if (entry.Value == null)
            return;

        curView.Selection.SelectedModelContainerWrapper = entry.Value;
        entry.Value.PopulateModelList();

        if (curView.Selection.SelectedModelContainerWrapper == null)
            return;

        // Use filename for non-DS2 projects
        var modelEntry = curView.Selection.SelectedModelContainerWrapper.Models.FirstOrDefault(e =>
            e.Name.Equals(filename, System.StringComparison.OrdinalIgnoreCase));

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            modelEntry = curView.Selection.SelectedModelContainerWrapper.Models.FirstOrDefault(e =>
                e.Name.Equals(modelFilename, System.StringComparison.OrdinalIgnoreCase));
        }

        if (commands.Length <= 4)
            modelEntry ??= curView.Selection.SelectedModelContainerWrapper.Models.FirstOrDefault();

        if (modelEntry == null)
            return;

        curView.LoadModel(modelEntry);
    }

    public void HandleSelectCommand(ModelEditorView curView, string[] commands)
    {
        ISelectable target = null;

        if (commands[0] != "select")
            return;

        // select/<type>/<index>
        if (commands.Length <= 2)
            return;

        var type = commands[1];

        if (!int.TryParse(commands[2], out var index))
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Command_Invalid_Select_Index", commands[2]));
            return;
        }

        if (curView.Selection.SelectedModelWrapper == null)
            return;

        var container = curView.Selection.SelectedModelWrapper.Container;

        if (container == null)
            return;

        var entries = type switch
        {
            "dummy" => container.Dummies,
            "node" => container.Nodes,
            "mesh" => container.Meshes,
            "material" => container.Materials,
            _ => null
        };

        if (entries != null && index >= 0 && index < entries.Count)
        {
            target = entries[index];
        }

        if (target != null)
        {
            curView.Universe.Selection.ClearSelection();
            curView.Universe.Selection.AddSelection(target);
            curView.Universe.Selection.GotoTreeTarget = target;
        }
    }
}

