using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Adds new objects to the end of the model's objects. Appending keeps the position of every existing material and
/// node, so nothing that refers to them by index needs rewriting.
/// </summary>
public class AddModelObjectAction : ViewportAction
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private readonly ModelContainer Container;
    private readonly List<ModelEntity> Added = new();

    public AddModelObjectAction(ModelEditorView view, ProjectEntry project, ModelContainer container, List<ModelEntity> objects)
    {
        View = view;
        Project = project;

        Container = container;

        Added.AddRange(objects.Where(e => e != null));
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var added in Added)
        {
            Container.Objects.Add(added);
            Container.RootObject.AddChild(added);
            added.UpdateRenderModel();

            if (added.RenderSceneMesh != null)
            {
                added.RenderSceneMesh.SetSelectable(added);

                added.RenderSceneMesh.AutoRegister = true;
                added.RenderSceneMesh.Register();
            }
        }

        Container.RefreshTypeLists();

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity c in Added)
        {
            View.ViewportSelection.AddSelection(c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var added in Added)
        {
            Container.Objects.Remove(added);

            added.Parent?.RemoveChild(added);

            if (added.RenderSceneMesh != null)
            {
                added.RenderSceneMesh.AutoRegister = false;
                added.RenderSceneMesh.UnregisterWithScene();
            }
        }

        Container.RefreshTypeLists();

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }
}
