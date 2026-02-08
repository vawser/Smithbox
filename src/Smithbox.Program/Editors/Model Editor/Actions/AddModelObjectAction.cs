using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

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

        Added.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        for (var i = 0; i < Added.Count(); i++)
        {
            Container.Objects.Add(Added[i]);
            Container.RootObject.AddChild(Added[i]);
            Added[i].UpdateRenderModel();

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.SetSelectable(Added[i]);

                Added[i].RenderSceneMesh.AutoRegister = true;
                Added[i].RenderSceneMesh.Register();
            }
        }

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity c in Added)
        {
            View.ViewportSelection.AddSelection(c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Added.Count(); i++)
        {
            Container.Objects.Remove(Added[i]);
            if (Added[i] != null)
            {
                Added[i].Parent.RemoveChild(Added[i]);
            }

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.AutoRegister = false;
                Added[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }

}
