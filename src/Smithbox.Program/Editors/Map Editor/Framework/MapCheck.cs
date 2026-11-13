using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public enum MapCheckSeverity
{
    /// <summary>The save will fail, or write data the game can't use</summary>
    Error,

    /// <summary>Saves, but is probably a mistake</summary>
    Warning
}

public enum MapCheckKind
{
    BrokenReference,
    DS2DuplicateID,
    DuplicateName,
    DuplicateEntityID,
    MissingParamRow
}

/// <summary>
/// One problem found by <see cref="MapCheck" />. <see cref="Value" /> is the name or ID at fault, and
/// <see cref="Detail" /> the field or the object it clashes with. <see cref="Slot" /> is set for a broken reference.
/// </summary>
public sealed record MapCheckIssue(
    MapContainer Map,
    Entity Entity,
    MapCheckKind Kind,
    MapCheckSeverity Severity,
    string Value,
    string Detail,
    ReferenceSlot? Slot = null)
{
    /// <summary>One line for the report</summary>
    public string Describe()
    {
        return Kind switch
        {
            MapCheckKind.BrokenReference => LOC.Get("MAP_SaveCheck_Issue_Broken_Reference", Detail, Value),
            MapCheckKind.DS2DuplicateID => LOC.Get("MAP_SaveCheck_Issue_DS2_Duplicate_ID", Detail, Value),
            MapCheckKind.DuplicateName => LOC.Get("MAP_SaveCheck_Issue_Duplicate_Name", Value, Detail),
            MapCheckKind.DuplicateEntityID => LOC.Get("MAP_SaveCheck_Issue_Duplicate_Entity_ID", Value, Detail),
            MapCheckKind.MissingParamRow => LOC.Get("MAP_SaveCheck_Issue_Missing_Param_Row", Detail, Value),
            _ => Value
        };
    }
}

/// <summary>
/// Checks loaded maps for what would make a save fail (broken references, DS2 duplicate IDs) or is probably a
/// mistake (duplicate names and entity IDs, missing param rows). Used before a save (the Save Check window) and by
/// the Map Validator tool. Plan: Documentation/Map Editor Save Check.md.
/// </summary>
public static class MapCheck
{
    // Map object fields that refer to a param row, with the param they refer to
    private static readonly (string Field, string Param)[] ParamFields =
    {
        ("ThinkParamID", "NpcThinkParam"),
        ("NPCParamID", "NpcParam"),
        ("CharaInitID", "CharaInitParam"),
        ("ChameleonParamID", "MapMimicryEstablishmentParam")
    };

    public static List<MapCheckIssue> Run(ProjectEntry project, IEnumerable<MapContainer> maps)
    {
        var issues = new List<MapCheckIssue>();

        foreach (var map in maps)
        {
            if (map == null || map.LoadState is not MapContentLoadState.Loaded)
                continue;

            CheckReferences(map, issues);
            CheckDuplicateNames(map, issues);
            CheckDuplicateEntityIDs(map, issues);
            CheckParamRows(project, map, issues);

            if (project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                CheckDS2IDs(map, issues);
            }
        }

        // Errors first, then by map and kind, keeping each map's object order
        return issues
            .Select((issue, index) => (issue, index))
            .OrderBy(e => e.issue.Severity)
            .ThenBy(e => e.issue.Map.Name, StringComparer.Ordinal)
            .ThenBy(e => e.issue.Kind)
            .ThenBy(e => e.index)
            .Select(e => e.issue)
            .ToList();
    }

    /// <summary>Whether the issues should stop a save</summary>
    public static bool StopsSave(IEnumerable<MapCheckIssue> issues, bool stopOnWarnings)
    {
        return issues.Any(i => i.Severity is MapCheckSeverity.Error || stopOnWarnings);
    }

    private static bool IsMsbEntry(Entity ent)
    {
        return ent?.WrappedObject is IMsbPart or IMsbRegion or IMsbEvent;
    }

    /// <summary>
    /// A reference field naming no object of the type it refers to. The save resolves names to indices within
    /// the map and throws MissingReferenceException on the first that fails, so these are all errors.
    /// </summary>
    private static void CheckReferences(MapContainer map, List<MapCheckIssue> issues)
    {
        var byName = new Dictionary<string, List<Entity>>();

        foreach (var ent in map.Objects)
        {
            if (ent.Name == null)
                continue;

            if (!byName.TryGetValue(ent.Name, out var list))
            {
                list = new List<Entity>();
                byName[ent.Name] = list;
            }

            list.Add(ent);
        }

        foreach (var ent in map.Objects)
        {
            if (!IsMsbEntry(ent))
                continue;

            foreach (var slot in MsbReferences.GetSlots(ent))
            {
                var resolved = byName.TryGetValue(slot.Value, out var candidates) && candidates.Any(slot.CanReferTo);

                if (!resolved)
                {
                    issues.Add(new MapCheckIssue(map, ent, MapCheckKind.BrokenReference, MapCheckSeverity.Error,
                        slot.Value, slot.FieldName, slot));
                }
            }
        }
    }

    /// <summary>
    /// Two parts, regions or events with the same name that a reference field in the map holds, so the reference
    /// can resolve to either. Names nothing refers to are left out: vanilla maps share many event names (DES has
    /// over a thousand), which only matter once something refers to them.
    /// </summary>
    private static void CheckDuplicateNames(MapContainer map, List<MapCheckIssue> issues)
    {
        // Keyed by the list the name is looked up in: an SFX region and its event often share a name, and a
        // reference to the region doesn't make two events of that name ambiguous
        var referenced = new HashSet<(Type, string)>();

        foreach (var ent in map.Objects)
        {
            if (!IsMsbEntry(ent))
                continue;

            foreach (var slot in MsbReferences.GetSlots(ent))
            {
                if (slot.TargetList != null)
                {
                    referenced.Add((slot.TargetList, slot.Value));
                }
                else
                {
                    referenced.Add((typeof(IMsbPart), slot.Value));
                    referenced.Add((typeof(IMsbRegion), slot.Value));
                    referenced.Add((typeof(IMsbEvent), slot.Value));
                }
            }
        }

        var first = new Dictionary<(Type, string), Entity>();

        foreach (var ent in map.Objects)
        {
            if (!IsMsbEntry(ent) || string.IsNullOrEmpty(ent.Name))
                continue;

            var key = (MsbNaming.GetNameScope(ent.WrappedObject), ent.Name);

            if (!referenced.Contains(key))
                continue;

            if (first.TryGetValue(key, out var original))
            {
                issues.Add(new MapCheckIssue(map, ent, MapCheckKind.DuplicateName, MapCheckSeverity.Warning,
                    ent.Name, original.PrettyName));
            }
            else
            {
                first[key] = ent;
            }
        }
    }

    /// <summary>
    /// Two objects with the same entity ID. Vanilla maps have some, and the game treats them as a group for some
    /// commands, so these are warnings. Region duplicates are known to stop later regions working in DS1.
    /// </summary>
    private static void CheckDuplicateEntityIDs(MapContainer map, List<MapCheckIssue> issues)
    {
        var first = new Dictionary<long, Entity>();

        foreach (var ent in map.Objects)
        {
            if (!IsMsbEntry(ent))
                continue;

            var prop = ent.WrappedObject.GetType().GetProperty("EntityID");

            if (prop == null)
                continue;

            long id = prop.GetValue(ent.WrappedObject) switch
            {
                int i => i,
                uint u => u,
                _ => 0
            };

            if (id <= 0)
                continue;

            if (first.TryGetValue(id, out var original))
            {
                issues.Add(new MapCheckIssue(map, ent, MapCheckKind.DuplicateEntityID, MapCheckSeverity.Warning,
                    id.ToString(), original.PrettyName));
            }
            else
            {
                first[id] = ent;
            }
        }
    }

    /// <summary>A param ID field naming a row its param doesn't have. Needs the params loaded.</summary>
    private static void CheckParamRows(ProjectEntry project, MapContainer map, List<MapCheckIssue> issues)
    {
        if (project.Handler.ParamEditor == null)
            return;

        var paramBank = project.Handler.ParamData?.PrimaryBank;

        if (paramBank == null)
            return;

        foreach (var ent in map.Objects)
        {
            if (!IsMsbEntry(ent))
                continue;

            foreach (var (field, paramName) in ParamFields)
            {
                var prop = PropFinderUtil.FindProperty(field, ent.WrappedObject);

                if (prop == null || !paramBank.Params.TryGetValue(paramName, out var param))
                    continue;

                var value = PropFinderUtil.FindPropertyValue(prop, ent.WrappedObject);

                if (value == null || !int.TryParse(value.ToString(), out var id) || id == -1)
                    continue;

                if (!param.ContainsRow(id))
                {
                    issues.Add(new MapCheckIssue(map, ent, MapCheckKind.MissingParamRow, MapCheckSeverity.Warning,
                        id.ToString(), $"{field} ({paramName})"));
                }
            }
        }
    }

    /// <summary>DS2 per-map param rows sharing an ID within their param, which the save refuses</summary>
    private static void CheckDS2IDs(MapContainer map, List<MapCheckIssue> issues)
    {
        var first = new Dictionary<(MsbEntityType, long), Entity>();

        foreach (var ent in map.Objects)
        {
            if (ent is not MsbEntity msbEnt)
                continue;

            long? id = msbEnt.Type switch
            {
                MsbEntityType.DS2Generator when ent.WrappedObject is MergedParamRow merged => merged.ID,
                MsbEntityType.DS2GeneratorRegist or MsbEntityType.DS2Event or MsbEntityType.DS2EventLocation
                    or MsbEntityType.DS2ObjectInstance when ent.WrappedObject is Param.Row row => row.ID,
                _ => null
            };

            if (id == null)
                continue;

            var key = (msbEnt.Type, id.Value);

            if (first.ContainsKey(key))
            {
                issues.Add(new MapCheckIssue(map, ent, MapCheckKind.DS2DuplicateID, MapCheckSeverity.Error,
                    id.Value.ToString(), msbEnt.Type.ToString()));
            }
            else
            {
                first[key] = ent;
            }
        }
    }
}
