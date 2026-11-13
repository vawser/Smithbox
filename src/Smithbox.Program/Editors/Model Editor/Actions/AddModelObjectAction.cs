using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Adds a new object (a dummy, node or material) to the end of the model's objects of its type. Appending keeps the
/// position of every existing material and node, so nothing that refers to them by index needs rewriting.
/// </summary>
public class AddModelObjectAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private readonly ModelContainer Container;
    private readonly ModelEntity Added;

    public string Describe()
    {
        return LOC.Get("MODEL_History_Add", ModelHistory.NameObject(Added));
    }

    // The entity the new one sits under: a dummy's parent bone, a node's parent node, or the model root
    private readonly Entity Parent;

    // Changes to the FLVER the new object needs once it is in the lists (node links, node flags), with their undo
    private readonly Action<ModelContainer, IndexChangeRecord> ApplyLinks;
    private IndexChangeRecord Links;

    public AddModelObjectAction(ModelEditorView view, ProjectEntry project, ModelContainer container,
        ModelEntity added, Entity parent, Action<ModelContainer, IndexChangeRecord> applyLinks = null)
    {
        View = view;
        Project = project;

        Container = container;
        Added = added;
        Parent = parent ?? container.RootObject;
        ApplyLinks = applyLinks;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Container.Objects.Add(Added);
        Parent.AddChild(Added);

        Container.RefreshTypeLists();

        Links = new IndexChangeRecord();
        ApplyLinks?.Invoke(Container, Links);

        if (Added.RenderSceneMesh == null)
        {
            AssignDrawable();
        }
        else
        {
            Added.RenderSceneMesh.AutoRegister = true;
            Added.RenderSceneMesh.Register();
        }

        Added.UpdateRenderModel();

        View.ViewportSelection.ClearSelection();
        View.ViewportSelection.AddSelection(Added);
        View.ViewportSelection.GotoTreeTarget = Added;

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        Links?.Revert();
        Links = null;

        Container.Objects.Remove(Added);
        Added.Parent?.RemoveChild(Added);

        if (Added.RenderSceneMesh != null)
        {
            Added.RenderSceneMesh.AutoRegister = false;
            Added.RenderSceneMesh.UnregisterWithScene();
        }

        Container.RefreshTypeLists();

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    /// <summary>
    /// Dummies and nodes are drawn as markers; materials have nothing to draw
    /// </summary>
    private void AssignDrawable()
    {
        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan)
            return;

        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper?.Container != Container)
            return;

        switch (Added.WrappedObject)
        {
            case FLVER.Dummy:
                Container.AssignDummyDrawable(Added, wrapper);
                break;

            case FLVER.Node:
                Container.AssignNodeDrawable(Added, wrapper);
                break;
        }
    }
}
