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
public class DeleteModelObjectAction : ViewportAction
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    private List<Entity> Targets;
    private readonly List<EntityRemovalRecord> Records = new();

    private bool Captured = false;

    public DeleteModelObjectAction(
        ModelEditorView view,
        ProjectEntry project,
        IEnumerable<Entity> targets)
    {
        View = view;
        Project = project;

        Targets = targets
            .Where(t => !targets.Any(o => t != o && IsDescendantOf(t, o)))
            .ToList();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (!Captured)
        {
            CaptureState();
            Captured = true;
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

        View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
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

        View.ViewportSelection.ClearSelection();
        foreach (var r in Records)
        {
            View.ViewportSelection.AddSelection(r.Entity);
        }

        return ActionEvent.ObjectAddedRemoved;
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
                ParentIndex = e.Parent != null ? e.Parent.Children.IndexOf(e) : -1,
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
            r.Parent.Children.Remove(e);
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
            r.Parent.Children.Insert(r.ParentIndex, e);
            e.Parent = r.Parent;
        }

        if (r.HadRenderMesh)
        {
            e.RenderSceneMesh.AutoRegister = true;
            e.RenderSceneMesh.Register();
        }

        e.BuildReferenceMap();
        e.UpdateRenderModel();
    }

    private bool IsDescendantOf(Entity child, Entity potentialParent)
    {
        var p = child.Parent;
        while (p != null)
        {
            if (p == potentialParent)
                return true;
            p = p.Parent;
        }
        return false;
    }

    public override string GetEditMessage()
    {
        return $"Deleted {Records.Count} object(s)";
    }
}
