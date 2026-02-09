using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class DeleteMapObjectsAction : ViewportAction
{
    private MapEditorView View;

    private readonly List<MsbEntity> Deletables = new();
    private readonly List<int> RemoveIndices = new();
    private readonly List<ObjectContainer> RemoveMaps = new();
    private readonly List<MsbEntity> RemoveParent = new();
    private readonly List<int> RemoveParentIndex = new();
    private readonly bool SetSelection;

    private readonly List<DeleteRecord> Records = new();

    public DeleteMapObjectsAction(MapEditorView view, List<MsbEntity> objects, bool setSelection)
    {
        View = view;
        Deletables.AddRange(objects);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Records.Clear();

        foreach (var obj in Deletables)
        {
            var map = View.Selection.GetMapContainerFromMapID(obj.MapID);

            int mapIndex = -1;
            if (map != null)
                mapIndex = map.Objects.IndexOf(obj);

            var parent = (MsbEntity)obj.Parent;
            int parentIndex = -1;

            if (parent != null)
                parentIndex = parent.Children.IndexOf(obj);

            Records.Add(new DeleteRecord
            {
                Map = map,
                Entity = obj,
                MapIndex = mapIndex,
                Parent = parent,
                ParentIndex = parentIndex
            });
        }

        // Remove map objects safely
        Records.Sort((a, b) =>
        {
            int mapCmp = Comparer<MapContainer>.Default.Compare(a.Map, b.Map);
            if (mapCmp != 0)
                return mapCmp;

            return b.MapIndex.CompareTo(a.MapIndex);
        });

        foreach (var r in Records)
        {
            if (r.Map != null && r.MapIndex >= 0)
            {
                r.Map.HasUnsavedChanges = true;
                r.Map.Objects.RemoveAt(r.MapIndex);
            }

            if (r.Entity.RenderSceneMesh != null)
            {
                r.Entity.RenderSceneMesh.AutoRegister = false;
                r.Entity.RenderSceneMesh.UnregisterWithScene();
            }

            if (r.Parent != null)
            {
                r.Parent.Children.Remove(r.Entity);
                r.Entity.Parent = null;
            }
        }

        if (SetSelection)
            View.Universe.View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        // Restore map objects FIRST
        var mapOrdered = Records
            .Where(r => r.Map != null && r.MapIndex >= 0)
            .OrderBy(r => r.Map)
            .ThenBy(r => r.MapIndex);

        foreach (var r in mapOrdered)
        {
            int idx = r.MapIndex;
            if (idx > r.Map.Objects.Count)
                idx = r.Map.Objects.Count;

            r.Map.Objects.Insert(idx, r.Entity);

            if (r.Entity.RenderSceneMesh != null)
            {
                r.Entity.RenderSceneMesh.AutoRegister = true;
                r.Entity.RenderSceneMesh.Register();
            }
        }

        // Restore hierarchy SECOND
        var parentGroups = Records
            .Where(r => r.Parent != null && r.ParentIndex >= 0)
            .GroupBy(r => r.Parent);

        foreach (var group in parentGroups)
        {
            foreach (var r in group.OrderBy(x => x.ParentIndex))
            {
                int idx = r.ParentIndex;
                if (idx > r.Parent.Children.Count)
                    idx = r.Parent.Children.Count;

                if (r.Entity.Parent != null)
                    r.Entity.Parent.Children.Remove(r.Entity);

                r.Entity.Parent = r.Parent;
                r.Parent.Children.Insert(idx, r.Entity);
                r.Entity.UpdateRenderModel();
            }
        }

        if (SetSelection)
        {
            var sel = View.Universe.View.ViewportSelection;
            sel.ClearSelection();

            foreach (var r in Records)
                if (r.Entity != null)
                    sel.AddSelection(r.Entity);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }

    private struct DeleteRecord
    {
        public MapContainer Map;
        public MsbEntity Entity;
        public int MapIndex;

        public MsbEntity Parent;
        public int ParentIndex;
    }
}