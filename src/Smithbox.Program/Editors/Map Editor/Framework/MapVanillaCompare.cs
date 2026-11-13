using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public enum VanillaStatus
{
    /// <summary>Not an MSB part, region or event, or not worked out</summary>
    None,

    /// <summary>The map has no vanilla file (the mod added it), or it couldn't be read</summary>
    NoVanilla,

    Unchanged,
    Modified,

    /// <summary>Vanilla has no object of this list and name</summary>
    Added
}

/// <summary>
/// Compares loaded maps with their vanilla MSBs (MapData.VanillaBank, over VanillaFS), for the Map Contents
/// highlighting, Properties › Compare with Vanilla and the Vanilla Compare tool. Shared by every view.
/// Plan: Documentation/Map Editor Vanilla Compare.md. The diff itself is MsbDiff.
/// </summary>
public class MapVanillaCompare
{
    private readonly ProjectEntry Project;

    private sealed class MapState
    {
        public bool HasVanilla;
        public List<IMsbEntry> VanillaEntries = new();

        // Pairs and removed entries, rebuilt when the map's history version changes (renames, adds and deletes
        // change which objects pair up)
        public int PairVersion = int.MinValue;
        public Dictionary<Entity, IMsbEntry> Pairs = new();
        public List<IMsbEntry> Removed = new();

        public readonly Dictionary<Entity, (int Version, VanillaStatus Status)> Status = new();
        public readonly Dictionary<Entity, (int Version, List<MsbFieldDiff> Diffs)> Diffs = new();
    }

    private readonly Dictionary<MapContainer, MapState> States = new();

    public MapVanillaCompare(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Drops the map's vanilla copy, to be read again on next use. Called after a revert or restore, which hands
    /// vanilla objects to the map, so they mustn't be compared against any more.
    /// </summary>
    public void Invalidate(MapContainer map)
    {
        if (map != null)
        {
            States.Remove(map);
        }
    }

    public bool HasVanilla(MapContainer map) => GetState(map)?.HasVanilla == true;

    public VanillaStatus GetStatus(Entity ent)
    {
        if (ent?.WrappedObject == null || MsbDiff.ListOf(ent.WrappedObject) == null)
            return VanillaStatus.None;

        if (ent.Container is not MapContainer map)
            return VanillaStatus.None;

        var state = GetPairedState(map, out var version);

        if (state == null)
            return VanillaStatus.None;

        if (!state.HasVanilla)
            return VanillaStatus.NoVanilla;

        if (state.Status.TryGetValue(ent, out var cached) && cached.Version == version)
            return cached.Status;

        VanillaStatus status;

        if (!state.Pairs.TryGetValue(ent, out var vanilla) || vanilla == null)
            status = VanillaStatus.Added;
        else
            status = MsbDiff.Differs(ent.WrappedObject, vanilla) ? VanillaStatus.Modified : VanillaStatus.Unchanged;

        state.Status[ent] = (version, status);

        return status;
    }

    /// <summary>The vanilla counterpart of the object, or null</summary>
    public IMsbEntry GetVanilla(Entity ent)
    {
        if (ent?.Container is not MapContainer map)
            return null;

        var state = GetPairedState(map, out _);

        return state != null && state.Pairs.TryGetValue(ent, out var vanilla) ? vanilla : null;
    }

    /// <summary>The fields that differ from vanilla (empty if none, or no counterpart)</summary>
    public List<MsbFieldDiff> GetDiffs(Entity ent)
    {
        if (ent?.Container is not MapContainer map)
            return new();

        var state = GetPairedState(map, out var version);

        if (state == null || !state.Pairs.TryGetValue(ent, out var vanilla) || vanilla == null)
            return new();

        if (state.Diffs.TryGetValue(ent, out var cached) && cached.Version == version)
            return cached.Diffs;

        var diffs = MsbDiff.Compare(ent.WrappedObject, vanilla);
        state.Diffs[ent] = (version, diffs);

        return diffs;
    }

    /// <summary>The vanilla parts, regions and events that no object of the map pairs with</summary>
    public List<IMsbEntry> GetRemoved(MapContainer map)
    {
        var state = GetPairedState(map, out _);
        return state?.Removed ?? new();
    }

    private static int GetVersion(MapContainer map) => map.OwnerView?.ViewportActionManager.Version ?? 0;

    private MapState GetPairedState(MapContainer map, out int version)
    {
        version = GetVersion(map);

        var state = GetState(map);

        if (state == null || !state.HasVanilla)
            return state;

        if (state.PairVersion != version)
        {
            var (pairs, removed) = MsbDiff.Match(
                map.Objects.Where(o => MsbDiff.ListOf(o.WrappedObject) != null),
                o => o.WrappedObject as IMsbEntry,
                state.VanillaEntries);

            state.Pairs = pairs.ToDictionary(p => p.Item1, p => p.Item2);
            state.Removed = removed;
            state.PairVersion = version;
        }

        return state;
    }

    private MapState GetState(MapContainer map)
    {
        if (map == null || map.LoadState is not MapContentLoadState.Loaded)
            return null;

        if (States.TryGetValue(map, out var state))
            return state;

        // Maps unloaded since are let go
        foreach (var stale in States.Keys.Where(m => m.LoadState is not MapContentLoadState.Loaded).ToList())
        {
            States.Remove(stale);
        }

        state = new MapState { HasVanilla = ReadVanilla(map, out var entries), VanillaEntries = entries };
        States[map] = state;

        return state;
    }

    private bool ReadVanilla(MapContainer map, out List<IMsbEntry> entries)
    {
        entries = new();

        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);
        var bank = Project.Handler.MapData?.VanillaBank;

        if (adapter == null || bank == null)
            return false;

        var wrapper = bank.Maps.FirstOrDefault(e => e.Key.Filename == map.Name).Value;

        if (wrapper == null || !wrapper.TargetFS.FileExists(wrapper.Path))
            return false;

        try
        {
            var data = wrapper.TargetFS.ReadFile(wrapper.Path);

            if (data == null)
                return false;

            var msb = adapter.Read(data.Value);

            entries = msb.Parts.GetEntries().Cast<IMsbEntry>()
                .Concat(msb.Regions.GetEntries())
                .Concat(msb.Events.GetEntries())
                .ToList();

            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Vanilla_Read_Failed", map.Name), e);
            return false;
        }
    }

    // Reverts

    /// <summary>Whether the diff's field can be set (a list element always can, through its list)</summary>
    public static bool CanRevert(MsbFieldDiff diff)
    {
        return diff.Index >= 0 || diff.Property.GetSetMethod(true) != null;
    }

    /// <summary>
    /// The action that sets the given fields of the object back to their vanilla values, as one undo step. The values
    /// are vanilla's own objects, so the map's vanilla copy has to be dropped (Invalidate) once it has run.
    /// </summary>
    public static ViewportAction BuildRevertAction(Entity ent, IEnumerable<MsbFieldDiff> diffs)
    {
        var actions = new List<ViewportAction>();

        foreach (var group in diffs.Where(CanRevert).GroupBy(d => d.Owner, ReferenceEqualityComparer.Instance))
        {
            var action = new PropChangeAction(ent, group.Key);

            foreach (var diff in group)
            {
                action.AddPropertyChange(diff.Property, diff.Vanilla, diff.Index);
            }

            action.SetPostExecutionAction(_ => ent.UpdateRenderModel());
            actions.Add(action);
        }

        return actions.Count switch
        {
            0 => null,
            1 => actions[0],
            _ => new ViewportCompoundAction(actions)
        };
    }

    /// <summary>
    /// The action that puts a removed vanilla entry back in the map. The entry itself goes in, so the map's vanilla
    /// copy has to be dropped once it has run.
    /// </summary>
    public static ViewportAction BuildRestoreAction(MapEditorView view, MapContainer map, IMsbEntry entry)
    {
        var type = entry switch
        {
            IMsbPart => MsbEntityType.Part,
            IMsbRegion => MsbEntityType.Region,
            IMsbEvent => MsbEntityType.Event,
            _ => (MsbEntityType?)null
        };

        if (type == null)
            return null;

        var ent = new MsbEntity(view.Universe, map, entry, type.Value);

        return new EntAddAction(view, map, new List<MsbEntity> { ent }, map.RootObject);
    }

    // Display

    /// <summary>A short text for a field value, for the compare table</summary>
    public static string Format(object value)
    {
        switch (value)
        {
            case null:
                return "null";
            case string s:
                return $"\"{s}\"";
            case float f:
                return f.ToString("0.####", CultureInfo.InvariantCulture);
            case Vector3 v:
                return $"{Format(v.X)}, {Format(v.Y)}, {Format(v.Z)}";
            case Vector2 v2:
                return $"{Format(v2.X)}, {Format(v2.Y)}";
            case IList list:
                var shown = list.Cast<object>().Take(8).Select(Format);
                return $"[{string.Join(", ", shown)}{(list.Count > 8 ? $", … ({list.Count})" : "")}]";
            default:
                var type = value.GetType();

                // A nested object whose type differs: its type is what tells them apart
                return type.IsClass ? type.Name : Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
