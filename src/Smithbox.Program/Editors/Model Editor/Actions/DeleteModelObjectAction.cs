using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class DeleteModelObjectAction : ViewportAction
{
    private ModelEditorView View;
    private ProjectEntry Project;

    private ModelContainer Container;
    private List<ModelEntity> Deletables = new();

    private List<int> RemoveIndices = new();
    private List<ObjectContainer> RemoveContainers = new();
    private List<ModelEntity> RemoveParent = new();
    private List<int> RemoveParentIndex = new();

    public DeleteModelObjectAction(ModelEditorView view, ProjectEntry project, ModelContainer container, List<ModelEntity> objects)
    {
        View = view;
        Project = project;

        Container = container;

        Deletables.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (ModelEntity obj in Deletables)
        {
            if (Container != null)
            {
                RemoveContainers.Add(Container);
                Container.HasUnsavedChanges = true;

                RemoveIndices.Add(Container.Objects.IndexOf(obj));
                Container.Objects.RemoveAt(RemoveIndices.Last());

                if (obj.RenderSceneMesh != null)
                {
                    obj.RenderSceneMesh.AutoRegister = false;
                    obj.RenderSceneMesh.UnregisterWithScene();
                }

                RemoveParent.Add((ModelEntity)obj.Parent);
                if (obj.Parent != null)
                {
                    RemoveParentIndex.Add(obj.Parent.RemoveChild(obj));
                }
                else
                {
                    RemoveParentIndex.Add(-1);
                }
            }
            else
            {
                RemoveContainers.Add(null);
                RemoveIndices.Add(-1);
                RemoveParent.Add(null);
                RemoveParentIndex.Add(-1);
            }
        }

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Deletables.Count(); i++)
        {
            if (RemoveContainers[i] == null || RemoveIndices[i] == -1)
            {
                continue;
            }

            RemoveContainers[i].Objects.Insert(RemoveIndices[i], Deletables[i]);
            if (Deletables[i].RenderSceneMesh != null)
            {
                Deletables[i].RenderSceneMesh.AutoRegister = true;
                Deletables[i].RenderSceneMesh.Register();
            }

            if (RemoveParent[i] != null)
            {
                RemoveParent[i].AddChild(Deletables[i], RemoveParentIndex[i]);
            }
        }

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity d in Deletables)
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
