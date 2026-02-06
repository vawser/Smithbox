using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class CloneModelObjectAction : ViewportAction
{
    private ModelEditorView View;
    private ProjectEntry Project;

    private ModelContainer Container;
    private List<ModelEntity> Clonables = new();
    private List<ModelEntity> Clones = new();

    public CloneModelObjectAction(ModelEditorView view, ProjectEntry project, ModelContainer container, List<ModelEntity> objects)
    {
        View = view;
        Project = project;

        Container = container;
        Clonables.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var clonesCached = Clones.Count() > 0;

        for (var i = 0; i < Clonables.Count(); i++)
        {
            var newObj = clonesCached ? Clones[i] : (ModelEntity)Clonables[i].Clone();

            Container.Objects.Insert(Container.Objects.IndexOf(Clonables[i]) + 1, newObj);

            if (Clonables[i].Parent != null)
            {
                var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                Clonables[i].Parent.AddChild(newObj, idx + 1);
            }

            if (!clonesCached)
            {
                Clones.Add(newObj);
                Container.HasUnsavedChanges = true;
            }
            else
            {
                if (Clones[i].RenderSceneMesh != null)
                {
                    Clones[i].RenderSceneMesh.AutoRegister = true;
                    Clones[i].RenderSceneMesh.Register();
                }
            }
        }

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity d in Clonables)
        {
            View.ViewportSelection.AddSelection(d);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Container.Objects.Remove(Clones[i]);

            if (Clones[i] != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity d in Clonables)
        {
            View.ViewportSelection.AddSelection(d);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
