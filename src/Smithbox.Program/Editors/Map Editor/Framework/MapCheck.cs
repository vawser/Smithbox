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
    MissingParamRow,
    ConnectCollision,
    SessionPlacement
}

/// <summary>
/// One problem found by <see cref="MapCheck" />. <see cref="Value" /> is the name or ID at fault, and
/// <see cref="Detail" /> the field or the object it clashes with. <see cref="Slot" /> is set for a broken reference.
/// <see cref="Message" /> is the finished line for kinds with several messages (connect collisions).
/// </summary>
public sealed record MapCheckIssue(
    MapContainer Map,
    Entity Entity,
    MapCheckKind Kind,
    MapCheckSeverity Severity,
    string Value,
    string Detail,
    ReferenceSlot? Slot = null,
    string Message = null)
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
            MapCheckKind.ConnectCollision => Message ?? Value,
            MapCheckKind.SessionPlacement => Message ?? Value,
            _ => Value
        };
    }
}

/// <summary>
/// Checks loaded maps for what would make a save fail (broken references, DS2 duplicate IDs) or is probably a
/// mistake (duplicate names and entity IDs, missing param rows). Used before a save (the Save Check window) and by
/// the Map Validator tool. Plan: Claude/Documentation/Map Editor.md (Save Check).
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
            CheckConnectCollisions(project, map, issues);
            CheckSessionPlacement(project, map, issues);

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

            // Wherever the format keeps it (NR parts in EntityData, NR regions in Common_EntityID)
            if (!MsbEntityIds.TryGet(ent.WrappedObject, out var id) || id <= 0)
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

    /// <summary>
    /// Per-session placement fields (<see cref="NrPlacementFilter"/>, by field presence) that make a part never
    /// placed, or that the game ignores. Checked against the 300 retail NR MSBs (Claude/Logs/nr_placement_survey.txt):
    /// Variation ≥ 1 in a fixed block (m20_10, 6 parts), RandomAppear rows with no slots (36 parts on
    /// 61001/61006/61008/61009) and two partial scenario triples (m10) occur in vanilla; VariationAlsoPlus10 outside
    /// m16_00/m19_00 assets doesn't.
    /// </summary>
    private static void CheckSessionPlacement(ProjectEntry project, MapContainer map, List<MapCheckIssue> issues)
    {
        RandomAppearTable table = null;
        var tableRead = false;
        var fixedBlock = !NrPlacementFilter.HasSessionVariation(map.Name);

        void Add(Entity ent, string message) =>
            issues.Add(new MapCheckIssue(map, ent, MapCheckKind.SessionPlacement, MapCheckSeverity.Warning,
                ent.Name, null, null, message));

        foreach (var ent in map.Objects)
        {
            if (ent.WrappedObject is not IMsbPart)
                continue;

            var part = ent.WrappedObject;

            // Read as stored, for every part type: the filter reads it for assets only, which is what this checks
            if (MsbEntryColumns.TryGetValue(part, NrPlacementFilter.AlsoPlus10Field, out var plus) &&
                plus is IConvertible p && p.ToInt64(null) != 0 &&
                (!EntityHelper.IsPartAsset(part) || !NrPlacementFilter.AllowsAlsoPlus10(map.Name)))
            {
                Add(ent, LOC.Get("MAP_SaveCheck_Issue_Placement_Plus10_Ignored"));
            }

            if (NrPlacementFilter.TryGetScenarioKeys(part, out var k0, out var k1, out var step))
            {
                var unset = (k0 == -1 ? 1 : 0) + (k1 == -1 ? 1 : 0) + (step == -1 ? 1 : 0);

                if (unset is 1 or 2)
                    Add(ent, LOC.Get("MAP_SaveCheck_Issue_Placement_Scenario_Partial", k0, k1, step));
            }

            if (!NrPlacementFilter.TryGetFields(part, out var fields))
                continue;

            if (fixedBlock && fields.Variation > 0)
                Add(ent, LOC.Get("MAP_SaveCheck_Issue_Placement_Variation_Fixed", fields.Variation));

            if (fields.RandomAppearParamID != 0)
            {
                if (!tableRead)
                {
                    table = RandomAppearTable.Build(project);
                    tableRead = true;
                }

                var slots = table?.SlotCountOf(fields.RandomAppearParamID);

                if (slots == 0)
                    Add(ent, LOC.Get("MAP_SaveCheck_Issue_Placement_Random_No_Slots", fields.RandomAppearParamID));
                else if (slots < 0)
                    Add(ent, LOC.Get("MAP_SaveCheck_Issue_Placement_Random_Missing", fields.RandomAppearParamID));
            }
        }
    }

    /// <summary>DS2 per-map param rows sharing an ID within their param, which the save refuses</summary>
    /// <summary>
    /// ER connect collisions: the map load copies the distinct target maps into 16 slots without a bounds check and
    /// asserts on more than 4 distinct other areas (errors); a connect without a collision, with another model than its
    /// collision, an unsupported load level or world state, or a target map that doesn't exist (warnings).
    /// </summary>
    private static void CheckConnectCollisions(ProjectEntry project, MapContainer map, List<MapCheckIssue> issues)
    {
        var connects = map.Objects.Where(e => e.WrappedObject is MSBE.Part.ConnectCollision).ToList();

        if (connects.Count == 0)
            return;

        var collisions = map.Objects
            .Where(e => e.WrappedObject is MSBE.Part.Collision && e.Name != null)
            .GroupBy(e => e.Name)
            .ToDictionary(g => g.Key, g => (MSBE.Part.Collision)g.First().WrappedObject);

        var knownMaps = project.Locator?.MapFiles?.Entries?.Select(e => e.Filename).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ownArea = ConnectCollisions.AreaOf(map.Name);

        // The map's collision file names, if its collision was loaded (otherwise nothing is listed, so no check)
        // (h files only: with only low collision loaded, the h names aren't listed)
        var collisionBank = map.OwnerView?.HavokCollisionBank;
        if (collisionBank != null && !collisionBank.GetMapCollisionNames(map.Name).Any(n => n.StartsWith("h", StringComparison.OrdinalIgnoreCase)))
            collisionBank = null;

        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var areas = new SortedSet<int>();
        Entity targetLimitAt = null, areaLimitAt = null;

        foreach (var ent in connects)
        {
            var connect = (MSBE.Part.ConnectCollision)ent.WrappedObject;
            var target = ConnectCollisions.TargetName(connect.MapID);

            // The game skips a negative area byte; everything else counts as a target
            if (target != null && !string.Equals(target, map.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (targets.Add(target) && targets.Count > ConnectCollisions.MaxTargets && targetLimitAt == null)
                    targetLimitAt = ent;

                if (connect.MapID[0] != ownArea && areas.Add(connect.MapID[0]) && areas.Count > ConnectCollisions.MaxForeignAreas && areaLimitAt == null)
                    areaLimitAt = ent;

                if (knownMaps is { Count: > 0 } && !knownMaps.Contains(target))
                    AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, target,
                        LOC.Get("MAP_SaveCheck_Issue_Connect_Missing_Target", target));
            }

            // The model must be one of the map's own collision files (h binder); only known while collision is loaded
            if (collisionBank != null && !string.IsNullOrEmpty(connect.ModelName) && connect.ModelName.Length > 1
                && !collisionBank.HasMapCollision(map.Name, $"h{map.Name.Substring(1)}_{connect.ModelName.Substring(1)}.hkx.dcx"))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, connect.ModelName,
                    LOC.Get("MAP_SaveCheck_Issue_Connect_No_Collision_File", connect.ModelName, $"h{map.Name.Substring(1)}"));
            }

            if (string.IsNullOrEmpty(connect.CollisionName))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, "",
                    LOC.Get("MAP_SaveCheck_Issue_Connect_No_Collision"));
            }
            else if (collisions.TryGetValue(connect.CollisionName, out var collision)
                && !string.Equals(collision.ModelName, connect.ModelName, StringComparison.OrdinalIgnoreCase))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, connect.ModelName,
                    LOC.Get("MAP_SaveCheck_Issue_Connect_Model_Mismatch", connect.ModelName, connect.CollisionName, collision.ModelName));
            }

            // The load level only applies to open-world targets (areas 50-88)
            if (ConnectCollisions.IsOpenWorldArea(connect.MapID[0]) && !ConnectCollisions.LoadLevels.Contains(connect.UnkT08))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, connect.UnkT08.ToString(),
                    LOC.Get("MAP_SaveCheck_Issue_Connect_Load_Level", connect.UnkT08));
            }

            if (!ConnectCollisions.WorldStates.Contains(connect.UnkT0A))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, connect.UnkT0A.ToString(),
                    LOC.Get("MAP_SaveCheck_Issue_Connect_World_State", connect.UnkT0A));
            }
            else if (!ConnectCollisions.CanBeActive(connect))
            {
                AddConnectIssue(map, ent, issues, MapCheckSeverity.Warning, connect.UnkT0A.ToString(),
                    LOC.Get("MAP_SaveCheck_Issue_Connect_Never_Active", connect.UnkT0A));
            }
        }

        if (targetLimitAt != null)
        {
            AddConnectIssue(map, targetLimitAt, issues, MapCheckSeverity.Error, targets.Count.ToString(),
                LOC.Get("MAP_SaveCheck_Issue_Connect_Too_Many_Targets", targets.Count, ConnectCollisions.MaxTargets));
        }

        if (areaLimitAt != null)
        {
            AddConnectIssue(map, areaLimitAt, issues, MapCheckSeverity.Error, areas.Count.ToString(),
                LOC.Get("MAP_SaveCheck_Issue_Connect_Too_Many_Areas", areas.Count, ConnectCollisions.MaxForeignAreas,
                    string.Join(", ", areas.Select(x => $"m{x:D2}"))));
        }
    }

    private static void AddConnectIssue(MapContainer map, Entity ent, List<MapCheckIssue> issues,
        MapCheckSeverity severity, string value, string message)
    {
        issues.Add(new MapCheckIssue(map, ent, MapCheckKind.ConnectCollision, severity, value, "", Message: message));
    }

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
