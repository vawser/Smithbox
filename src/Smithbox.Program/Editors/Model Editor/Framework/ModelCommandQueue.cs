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

        var filename = commands[1];

        var entry = Project.Handler.ModelData.PrimaryBank.Models
            .FirstOrDefault(e => e.Key.Filename.ToLower() == filename.ToLower());

        if (entry.Value == null)
            return;

        curView.Selection.SelectedModelContainerWrapper = entry.Value;
        entry.Value.PopulateModelList();

        if (curView.Selection.SelectedModelContainerWrapper == null)
            return;

        var firstEntry = curView.Selection.SelectedModelContainerWrapper.Models.FirstOrDefault();

        if (firstEntry == null)
            return;

        if (curView.Selection.SelectedModelWrapper != null)
        {
            curView.Selection.SelectedModelWrapper.Unload();
        }

        curView.Selection.SelectedModelWrapper = firstEntry;

        curView.ActionManager.Clear();

        firstEntry.Load();
    }

    public void HandleSelectCommand(ModelEditorView curView, string[] commands)
    {
        ISelectable target = null;

        if (commands[0] != "select")
            return;

        if (commands.Length <= 1)
            return;

        var type = commands[1];
        var value = commands[2];

        if (curView.Selection.SelectedModelWrapper == null)
            return;

        var container = curView.Selection.SelectedModelWrapper.Container;

        if (container == null)
            return;

        if (type == "dummy")
        {
            var index = int.Parse(value);

            for (int i = 0; i < container.Dummies.Count; i++)
            {
                if (index == i)
                {
                    target = container.Dummies[i];
                }
            }
        }

        if (type == "node")
        {
            var index = int.Parse(value);

            for (int i = 0; i < container.Nodes.Count; i++)
            {
                if (index == i)
                {
                    target = container.Nodes[i];
                }
            }
        }

        if (type == "mesh")
        {
            var index = int.Parse(value);

            for (int i = 0; i < container.Meshes.Count; i++)
            {
                if (index == i)
                {
                    target = container.Meshes[i];
                }
            }
        }

        if (type == "material")
        {
            var index = int.Parse(value);

            for (int i = 0; i < container.Materials.Count; i++)
            {
                if (index == i)
                {
                    target = container.Materials[i];
                }
            }
        }

        if (target != null)
        {
            curView.Universe.Selection.ClearSelection();
            curView.Universe.Selection.AddSelection(target);
            curView.Universe.Selection.GotoTreeTarget = target;
        }
    }
}

