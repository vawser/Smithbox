using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Applies a mass edit. Maps loaded in the view are changed through one action, so a single undo takes the edit back.
/// Maps that aren't loaded are read again, changed and written straight to the project, which can't be undone.
/// Every object is checked against the values the plan was made from first; one that has changed since is left alone
/// and counted as stale, so an edit never builds on values it didn't show.
/// </summary>
public static class MsbMassEditApplier
{
    public class Result
    {
        /// <summary>
        /// The objects changed or deleted
        /// </summary>
        public int Applied;

        /// <summary>
        /// The objects left alone, as they had changed since the find, or were gone
        /// </summary>
        public int Stale;

        /// <summary>
        /// The objects left alone, as their map is loaded in another view
        /// </summary>
        public int OtherView;

        /// <summary>
        /// The unloaded maps written to the project
        /// </summary>
        public readonly List<string> Written = new();

        /// <summary>
        /// The maps that couldn't be read or written, with why
        /// </summary>
        public readonly List<(string MapID, string Error)> Failed = new();
    }

    /// <summary>
    /// How a map of a match is to be changed now: through the editor, by writing its file, or not at all
    /// </summary>
    private enum MapState
    {
        LoadedHere,
        LoadedElsewhere,
        Unloaded
    }

    private static MapState GetMapState(MapEditorView view, MsbQueryRunner runner, string mapID, out MapContainer map)
    {
        map = view.Selection.GetMapContainerFromMapID(mapID);

        if (map == null)
            return MapState.Unloaded;

        return runner.IsLoadedHere(map) ? MapState.LoadedHere : MapState.LoadedElsewhere;
    }

    /// <summary>
    /// The maps of these matches that aren't loaded, which an apply writes straight to the project
    /// </summary>
    public static List<string> GetUnloadedMaps(MapEditorView view, MsbQueryRunner runner, IEnumerable<MsbQueryMatch> matches)
    {
        return matches.Select(e => e.MapID).Distinct()
            .Where(id => GetMapState(view, runner, id, out _) is MapState.Unloaded)
            .OrderBy(e => e)
            .ToList();
    }

    #region Edit
    public static Result ApplyEdit(MapEditorView view, ProjectEntry project, MsbQueryRunner runner, MsbMassEditPlan plan)
    {
        var result = new Result();
        var matcher = new MsbQueryMatcher(project);
        var actions = new List<ViewportAction>();

        foreach (var mapGroup in plan.Changes.GroupBy(e => e.Match.MapID))
        {
            var state = GetMapState(view, runner, mapGroup.Key, out var map);
            var byObject = mapGroup.GroupBy(e => e.Match).ToList();

            switch (state)
            {
                case MapState.LoadedElsewhere:
                    result.OtherView += byObject.Count;
                    break;

                case MapState.LoadedHere:
                    var objects = new HashSet<Entity>(map.Objects);

                    foreach (var changes in byObject)
                    {
                        if (!MsbQueryRunner.TryResolve(map, changes.Key.Entry, out var entity, objects) ||
                            !TryGetFields(matcher, entity.WrappedObject, changes, out var fields))
                        {
                            result.Stale++;
                            continue;
                        }

                        actions.AddRange(CreateActions(entity, changes.ToList(), fields));
                        result.Applied++;
                    }

                    break;

                case MapState.Unloaded:
                    ApplyToFile(project, runner, matcher, mapGroup.Key, byObject, result);
                    break;
            }
        }

        if (actions.Count > 0)
        {
            view.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
        }

        return result;
    }

    /// <summary>
    /// The fields of an object for its changes, checked against the values the plan was made from. False if any
    /// field is gone or has a different value now.
    /// </summary>
    private static bool TryGetFields(MsbQueryMatcher matcher, object obj, IEnumerable<MsbPlannedChange> changes,
        out List<MsbQueryField> fields)
    {
        fields = new List<MsbQueryField>();

        var current = matcher.GetFields(obj).GroupBy(e => e.Path).ToDictionary(e => e.Key, e => e.First());

        foreach (var change in changes)
        {
            if (!current.TryGetValue(change.Path, out var field) || !Equals(field.Value, change.OldValue))
                return false;

            fields.Add(field);
        }

        return true;
    }

    /// <summary>
    /// One property change action for each object that holds a changed field: the map object itself, or a class
    /// nested in it. They refresh the entity as an edit in the Properties window does.
    /// </summary>
    private static List<ViewportAction> CreateActions(MsbEntity entity, List<MsbPlannedChange> changes, List<MsbQueryField> fields)
    {
        var actions = new List<ViewportAction>();
        var byOwner = new Dictionary<object, PropChangeAction>(ReferenceEqualityComparer.Instance);

        for (int i = 0; i < changes.Count; i++)
        {
            var field = fields[i];

            if (!byOwner.TryGetValue(field.Owner, out var action))
            {
                action = new PropChangeAction(entity, field.Owner);

                // References to other objects are worked out from the fields, so they are worked out again
                action.SetPostExecutionAction(_ => entity.BuildReferenceMap());

                byOwner[field.Owner] = action;
                actions.Add(action);
            }

            action.AddPropertyChange(field.Property, changes[i].NewValue, field.Index);
        }

        return actions;
    }

    /// <summary>
    /// Reads an unloaded map again, makes the changes to it and writes it to the project
    /// </summary>
    private static void ApplyToFile(ProjectEntry project, MsbQueryRunner runner, MsbQueryMatcher matcher, string mapID,
        List<IGrouping<MsbQueryMatch, MsbPlannedChange>> byObject, Result result)
    {
        try
        {
            var (fileEntry, msb) = ReadMap(project, runner, mapID);

            var applied = 0;

            foreach (var changes in byObject)
            {
                var obj = MsbQueryRunner.TryResolve(msb, changes.Key.Entry);

                if (obj == null || !TryGetFields(matcher, obj, changes, out var fields))
                {
                    result.Stale++;
                    continue;
                }

                var list = changes.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    SetValue(fields[i], list[i].NewValue);
                }

                applied++;
            }

            if (applied == 0)
                return;

            MsbDirectWriter.Write(project, fileEntry, msb, null);

            result.Applied += applied;
            result.Written.Add(mapID);
        }
        catch (Exception e)
        {
            result.Failed.Add((mapID, e.Message));
            Smithbox.LogError(typeof(MsbMassEditApplier), LOC.Get("MAP_MassEditTool_Write_Failed", mapID), e);
        }
    }

    private static (FileDictionaryEntry, IMsb) ReadMap(ProjectEntry project, MsbQueryRunner runner, string mapID)
    {
        var fileEntry = runner.View.Selection.GetFileEntryFromMapID(mapID)
            ?? throw new InvalidOperationException(LOC.Get("MAP_MassEditTool_Map_Not_Found", mapID));

        var msb = MsbQueryRunner.ReadMsb(MsbGameAdapter.For(project.Descriptor.ProjectType),
            project.Handler.MapData.PrimaryBank.TargetFS, fileEntry)
            ?? throw new InvalidOperationException(LOC.Get("MAP_MassEditTool_Map_Not_Read", mapID));

        return (fileEntry, msb);
    }

    /// <summary>
    /// Sets a field's value on the object that holds it, in its array or list if it is in one
    /// </summary>
    private static void SetValue(MsbQueryField field, object value)
    {
        if (field.Index >= 0)
        {
            var container = field.Property.GetValue(field.Owner);

            if (container is Array array)
            {
                array.SetValue(value, field.Index);
                return;
            }

            if (container is IList list)
            {
                list[field.Index] = value;
                return;
            }
        }

        field.Property.SetValue(field.Owner, value);
    }
    #endregion

    #region Delete
    public static Result ApplyDelete(MapEditorView view, ProjectEntry project, MsbQueryRunner runner, List<MsbQueryMatch> matches)
    {
        var result = new Result();
        var deleted = new List<MsbEntity>();

        foreach (var mapGroup in matches.GroupBy(e => e.MapID))
        {
            var state = GetMapState(view, runner, mapGroup.Key, out var map);
            var group = mapGroup.ToList();

            switch (state)
            {
                case MapState.LoadedElsewhere:
                    result.OtherView += group.Count;
                    break;

                case MapState.LoadedHere:
                    var objects = new HashSet<Entity>(map.Objects);
                    var taken = new HashSet<MsbEntity>();

                    foreach (var match in group)
                    {
                        if (MsbQueryRunner.TryResolve(map, match.Entry, out var entity, objects) && taken.Add(entity))
                        {
                            deleted.Add(entity);
                            result.Applied++;
                        }
                        else
                        {
                            result.Stale++;
                        }
                    }

                    break;

                case MapState.Unloaded:
                    DeleteFromFile(project, runner, mapGroup.Key, group, result);
                    break;
            }
        }

        if (deleted.Count > 0)
        {
            view.ViewportActionManager.ExecuteAction(new EntDeleteAction(view, deleted, true));
        }

        return result;
    }

    private static void DeleteFromFile(ProjectEntry project, MsbQueryRunner runner, string mapID, List<MsbQueryMatch> matches,
        Result result)
    {
        try
        {
            var (fileEntry, msb) = ReadMap(project, runner, mapID);

            var removed = new HashSet<object>(ReferenceEqualityComparer.Instance);

            foreach (var match in matches)
            {
                var obj = MsbQueryRunner.TryResolve(msb, match.Entry);

                if (obj == null || !removed.Add(obj))
                {
                    result.Stale++;
                }
            }

            if (removed.Count == 0)
                return;

            MsbDirectWriter.Write(project, fileEntry, msb, removed);

            result.Applied += removed.Count;
            result.Written.Add(mapID);
        }
        catch (Exception e)
        {
            result.Failed.Add((mapID, e.Message));
            Smithbox.LogError(typeof(MsbMassEditApplier), LOC.Get("MAP_MassEditTool_Write_Failed", mapID), e);
        }
    }
    #endregion
}

/// <summary>
/// Writes an MSB that isn't loaded straight to the project, the way a save of a loaded map does
/// (<see cref="MapUniverse"/>.SaveMSB): a new MSB made from the file, keeping the sections the editor doesn't manage,
/// with the parts, regions, events and part poses added back, and the models rebuilt from the model names in use.
/// </summary>
public static class MsbDirectWriter
{
    public static void Write(ProjectEntry project, FileDictionaryEntry fileEntry, IMsb edited, HashSet<object> excluded)
    {
        var adapter = MsbGameAdapter.For(project.Descriptor.ProjectType)
            ?? throw new NotSupportedException(LOC.Get("MAP_Universe_Save_Map_Unsupported", project.Descriptor.ProjectType));

        var original = project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(fileEntry.Path)
            ?? throw new InvalidOperationException(LOC.Get("MAP_MassEditTool_Map_Not_Read", fileEntry.Filename));

        var msb = adapter.CreateForWrite(original);

        bool Keep(object obj) => excluded == null || !excluded.Contains(obj);

        // The models that existed are reused as they are, and the ones the parts name but the MSB lacks are made
        var models = new Dictionary<string, IMsbModel>();

        foreach (var model in edited.Models.GetEntries())
        {
            if (model?.Name != null)
                models.TryAdd(model.Name, model);
        }

        foreach (var part in edited.Parts.GetEntries().Where(Keep))
        {
            msb.Parts.Add(part);

            if (part.ModelName != null)
                models.TryAdd(part.ModelName, null);
        }

        foreach (var region in edited.Regions.GetEntries().Where(Keep))
        {
            msb.Regions.Add(region);
        }

        foreach (var ev in edited.Events.GetEntries().Where(Keep))
        {
            msb.Events.Add(ev);
        }

        // The editor keeps the part poses of these games with the map objects, so they are added back as a save does
        if (edited is MSB2 editedMsb2 && msb is MSB2 msb2)
        {
            msb2.PartPoses.AddRange(editedMsb2.PartPoses);
        }
        else if (edited is MSB3 editedMsb3 && msb is MSB3 msb3)
        {
            msb3.PartsPoses.AddRange(editedMsb3.PartsPoses);
        }

        adapter.AddModels(msb, fileEntry.Filename, models);

        var data = msb.Write(adapter.Compression);
        project.VFS.ProjectFS.WriteFile(fileEntry.Path, data);

        // Maps loaded after this place the saved contents of a prefab
        project.Handler.MapData.PrefabAssetBank?.Invalidate(fileEntry.Filename);

        Smithbox.Log(typeof(MsbDirectWriter), LOC.Get("MAP_MassEditTool_Written", fileEntry.Filename));
    }
}
