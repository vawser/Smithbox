using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class EntityRemovalRecord
{
    public Entity Entity;

    public ObjectContainer Container;
    public int ContainerIndex;

    public Entity Parent;
    public int ParentIndex;

    public bool HadRenderMesh;
}
public class DeleteModelObjectAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private List<Entity> Targets;

    public string Describe()
    {
        return LOC.Get("MODEL_History_Delete", ModelHistory.NameObjects(Targets));
    }
    private readonly List<EntityRemovalRecord> Records = new();

    private bool Captured = false;

    // The node links and FLVER indices rewritten by the last execution, in the order they were changed
    private readonly List<IndexChangeRecord> IndexChanges = new();

    public DeleteModelObjectAction(
        ModelEditorView view,
        ProjectEntry project,
        IEnumerable<Entity> targets)
    {
        View = view;
        Project = project;

        // Every target is removed from the objects, children included: a child left among the objects would still
        // be saved, referring to the deleted parent node
        Targets = targets.Distinct().ToList();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (!Captured)
        {
            CaptureState();
            Captured = true;
        }

        var containers = GetModelContainers();

        // The materials and nodes as they were, for rewriting the references to those left after the removal
        var materialsBefore = containers.ToDictionary(c => c, ModelIndexRemapper.GetMaterials);
        var nodesBefore = containers.ToDictionary(c => c, ModelIndexRemapper.GetNodes);

        IndexChanges.Clear();

        // Deleted nodes are taken out of their sibling chains first, while the chains still use the old indices
        var nodeLinks = new IndexChangeRecord();
        IndexChanges.Add(nodeLinks);

        foreach (var r in Records)
        {
            if (r.Container is ModelContainer container && r.Entity.WrappedObject is FLVER.Node node)
            {
                var nodes = nodesBefore[container];
                ModelIndexRemapper.UnlinkNode(nodes, nodes.IndexOf(node), nodeLinks);
            }
        }

        foreach (var group in Records
            .Where(r => r.Container != null)
            .GroupBy(r => r.Container))
        {
            foreach (var r in group.OrderByDescending(x => x.ContainerIndex))
            {
                Remove(r);
            }
        }

        foreach (var r in Records.Where(r => r.Container == null))
        {
            Remove(r);
        }

        foreach (var container in containers)
        {
            IndexChanges.Add(ModelIndexRemapper.Remap(container, materialsBefore[container], nodesBefore[container]));

            container.RefreshTypeLists();
        }

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = IndexChanges.Count - 1; i >= 0; i--)
        {
            IndexChanges[i].Revert();
        }

        IndexChanges.Clear();

        foreach (var group in Records
            .Where(r => r.Container != null)
            .GroupBy(r => r.Container))
        {
            foreach (var r in group.OrderBy(x => x.ContainerIndex))
            {
                Restore(r);
            }
        }

        foreach (var r in Records.Where(r => r.Container == null))
        {
            Restore(r);
        }

        foreach (var container in GetModelContainers())
        {
            container.RefreshTypeLists();
        }

        View.ViewportSelection.ClearSelection();
        foreach (var r in Records)
        {
            View.ViewportSelection.AddSelection(r.Entity);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private List<ModelContainer> GetModelContainers()
    {
        return Records.Select(r => r.Container).OfType<ModelContainer>().Distinct().ToList();
    }

    private void CaptureState()
    {
        foreach (var e in Targets)
        {
            var record = new EntityRemovalRecord
            {
                Entity = e,
                Container = e.Container,
                ContainerIndex = e.Container?.Objects.IndexOf(e) ?? -1,
                Parent = e.Parent,
                ParentIndex = e.Parent != null ? e.Parent.ChildIndex(e) : -1,
                HadRenderMesh = e.RenderSceneMesh != null
            };

            Records.Add(record);
        }
    }

    private void Remove(EntityRemovalRecord r)
    {
        var e = r.Entity;

        if (r.Parent != null)
        {
            r.Parent.RemoveChild(e);
            e.Parent = null;
        }

        if (r.Container != null && r.ContainerIndex >= 0)
        {
            r.Container.HasUnsavedChanges = true;
            r.Container.Objects.RemoveAt(r.ContainerIndex);
        }

        if (r.HadRenderMesh)
        {
            e.RenderSceneMesh.AutoRegister = false;
            e.RenderSceneMesh.UnregisterWithScene();
        }

        e.InvalidateReferencingObjectsCache();
    }

    private void Restore(EntityRemovalRecord r)
    {
        var e = r.Entity;

        if (r.Container != null && r.ContainerIndex >= 0)
        {
            r.Container.Objects.Insert(r.ContainerIndex, e);
        }

        if (r.Parent != null)
        {
            r.Parent.AddChild(e, r.ParentIndex);
            e.Parent = r.Parent;
        }

        if (r.HadRenderMesh)
        {
            e.RenderSceneMesh.AutoRegister = true;
            e.RenderSceneMesh.Register();
        }

        e.BuildReferenceMap();

        if (e is MsbEntity msbEnt)
        {
            msbEnt.AssignDrawable();
        }
        e.UpdateRenderModel();

        if (e.RenderSceneMesh != null)
        {
            e.RenderSceneMesh.RenderSelectionOutline = true;
        }
    }
}
