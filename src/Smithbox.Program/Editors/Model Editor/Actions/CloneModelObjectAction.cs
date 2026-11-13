using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class CloneModelObjectAction : ViewportAction, IModelHistoryItem
{
    private ModelEditorView View;
    private ProjectEntry Project;

    private ModelContainer Container;
    private List<ModelEntity> Clonables = new();
    private List<ModelEntity> Clones = new();

    public string Describe()
    {
        return LOC.Get("MODEL_History_Duplicate", ModelHistory.NameObjects(Clonables.Cast<Entity>().ToList()));
    }

    private IndexChangeRecord NodeLinks;

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

            // Copies go after the last object of their type, as the FLVER refers to materials and nodes by their
            // position: putting a copy between the existing ones would shift what every later index points at
            Container.Objects.Insert(GetInsertIndex(Clonables[i]), newObj);

            if (Clonables[i].Parent != null)
            {
                var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                Clonables[i].Parent.AddChild(newObj, idx + 1);
            }

            if (!clonesCached)
            {
                Clones.Add(newObj);
                Container.HasUnsavedChanges = true;

                // Cloning copies the object but not its drawable, without which the copy can't be seen or picked
                var wrapper = View.Selection.SelectedModelWrapper;

                if (wrapper?.Container == Container)
                {
                    Container.AssignClonedDrawable(Clonables[i], newObj, wrapper);
                }
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

        LinkClonedNodes();

        Container.RefreshTypeLists();

        // The copies are selected, so that the next edit applies to them rather than the originals
        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity d in Clones)
        {
            View.ViewportSelection.AddSelection(d);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        NodeLinks?.Revert();
        NodeLinks = null;

        for (var i = 0; i < Clones.Count(); i++)
        {
            Container.Objects.Remove(Clones[i]);

            if (Clones[i].Parent != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        Container.RefreshTypeLists();

        View.ViewportSelection.ClearSelection();

        foreach (ModelEntity d in Clonables)
        {
            View.ViewportSelection.AddSelection(d);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    /// <summary>
    /// The position after the last object with the same type as the given object
    /// </summary>
    private int GetInsertIndex(ModelEntity source)
    {
        var type = source.WrappedObject?.GetType();

        var lastIndex = Container.Objects.FindLastIndex(e => e.WrappedObject?.GetType() == type);

        if (lastIndex == -1)
        {
            lastIndex = Container.Objects.IndexOf(source);
        }

        return lastIndex + 1;
    }

    /// <summary>
    /// A copied node keeps the parent of its original, so it is added to the sibling chain of that parent
    /// </summary>
    private void LinkClonedNodes()
    {
        NodeLinks = new IndexChangeRecord();

        var nodes = ModelIndexRemapper.GetNodes(Container);

        for (var i = 0; i < Clonables.Count; i++)
        {
            if (Clonables[i].WrappedObject is FLVER.Node original && Clones[i].WrappedObject is FLVER.Node clone)
            {
                ModelIndexRemapper.LinkClonedNode(nodes, original, clone, NodeLinks);
            }
        }
    }
}
