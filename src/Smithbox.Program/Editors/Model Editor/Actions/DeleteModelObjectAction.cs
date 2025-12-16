using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class DeleteModelObjectAction : ViewportAction
{
    private readonly ModelEditorScreen Editor;
    private readonly ProjectEntry Project;

    private readonly ModelContainer Container;
    private readonly List<ModelEntity> Deletables = new();

    private readonly List<int> RemoveIndices = new();
    private readonly List<ObjectContainer> RemoveContainers = new();
    private readonly List<ModelEntity> RemoveParent = new();
    private readonly List<int> RemoveParentIndex = new();

    public DeleteModelObjectAction(ModelEditorScreen editor, ProjectEntry project, ModelContainer container, List<ModelEntity> objects)
    {
        Editor = editor;
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

        Editor.ViewportSelection.ClearSelection(Editor);

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

        Editor.ViewportSelection.ClearSelection(Editor);

        foreach (ModelEntity d in Deletables)
        {
            Editor.ViewportSelection.AddSelection(Editor, d);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
