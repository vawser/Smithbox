using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Renames map objects. When updating references, the [MSBReference] name fields of the other objects in the
/// same map that point to a renamed object are rewritten to its new name, so the MSB still resolves on save.
/// </summary>
public class EntRenameAction : ViewportAction, IMapHistoryItem
{
    public string Describe()
    {
        return Entities.Count == 1 && NewNames.Count == 1
            ? LOC.Get("MAP_History_Rename_One", OldNames[0], NewNames[0])
            : LOC.Get("MAP_History_Rename", MapHistory.NameObjects(Entities));
    }

    private readonly MapEditorView View;
    private readonly List<MsbEntity> Entities;
    private readonly List<string> NewNames;
    private readonly List<string> OldNames;
    private readonly bool UpdateReferences;

    // The reference fields rewritten by the last execute, with the name each is given; undo puts back the name
    // the slot held when it was found
    private readonly List<(ReferenceSlot Slot, string NewValue)> ReferenceEdits = new();

    public EntRenameAction(MapEditorView view, List<MsbEntity> entities, List<string> newNames, bool reference)
    {
        View = view;
        Entities = entities;
        OldNames = entities.Select(e => e.Name).ToList();
        NewNames = newNames;
        UpdateReferences = reference;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        ReferenceEdits.Clear();

        // Gathered before anything is renamed, so references are matched against the original names
        if (UpdateReferences)
        {
            CollectReferenceEdits();
        }

        foreach (var (entity, name) in Entities.Zip(NewNames))
        {
            Rename(entity, name);
        }

        foreach (var (slot, newValue) in ReferenceEdits)
        {
            slot.Set(newValue);
        }

        MarkDirty();
        RebuildReferenceMaps();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var (slot, _) in ReferenceEdits)
        {
            slot.Set(slot.Value);
        }

        foreach (var (entity, name) in Entities.Zip(OldNames))
        {
            Rename(entity, name);
        }

        MarkDirty();
        RebuildReferenceMaps();

        return ActionEvent.ObjectAddedRemoved;
    }

    private void Rename(MsbEntity entity, string name)
    {
        var oldName = entity.Name;
        entity.Name = name;
        entity.CachedAliasName = null;

        MoveCommunityName(entity.MapID, oldName, name);

        if (CFG.Current.MapEditor_MapContentGroup_AutoAdjustName)
        {
            View.SelectionGroupsTool.UpdateMapGroupEntry(oldName, name);
        }

        (entity.Container as MapContainer)?.LightAtlasResolver.AutomaticAdjustment(entity, oldName, name);
    }

    /// <summary>
    /// Community names are kept by map and object name, so the alias goes with the object to its new name
    /// (and back on undo), rather than staying on the old name for whatever takes it next
    /// </summary>
    private void MoveCommunityName(string mapID, string oldName, string newName)
    {
        if (string.IsNullOrEmpty(mapID) || string.IsNullOrEmpty(oldName) || oldName == newName || newName == null)
            return;

        var mapData = View.Project.Handler.MapData;
        var alias = mapData.GetMapObjectName(mapID, oldName);

        if (string.IsNullOrEmpty(alias))
            return;

        mapData.UpdateMapObjectName(mapID, newName, alias);
        mapData.UpdateMapObjectName(mapID, oldName, "");
    }

    /// <summary>
    /// Finds every reference field in the renamed objects' maps that holds the current name of a renamed object
    /// of the type the field refers to
    /// </summary>
    private void CollectReferenceEdits()
    {
        var newNameOf = new Dictionary<Entity, string>();

        foreach (var ((entity, newName), oldName) in Entities.Zip(NewNames).Zip(OldNames))
        {
            if (entity.Container != null && !string.IsNullOrEmpty(oldName) && oldName != newName)
            {
                newNameOf[entity] = newName;
            }
        }

        foreach (var group in newNameOf.Keys.GroupBy(e => e.Container))
        {
            foreach (var (slot, target) in MsbReferences.FindReferencesTo(group.Key, group))
            {
                ReferenceEdits.Add((slot, newNameOf[target]));
            }
        }
    }

    private void MarkDirty()
    {
        MapContainer.MarkEntitiesDirty(Entities);

        foreach (var (slot, _) in ReferenceEdits)
        {
            MapContainer.MarkEntityDirty(slot.Referrer);
        }
    }

    /// <summary>
    /// The reference maps are built from names, so they are stale after a rename
    /// </summary>
    private void RebuildReferenceMaps()
    {
        foreach (var container in Entities.Select(e => e.Container).Where(c => c != null).Distinct())
        {
            Entity.BuildReferenceMaps(container.Objects);
        }
    }
}
