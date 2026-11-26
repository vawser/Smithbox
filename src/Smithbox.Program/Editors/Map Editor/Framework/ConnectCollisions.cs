using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// ER ConnectCollision rules as the game applies them (Claude/Documentation/MSB Research.md §2.6a): how a MapID names a
/// map, the per-map limits, and the values of the load level and world-state gate. Used by the properties panel, Create
/// Connect and the Save Check. Plan: Claude/Documentation/Connect Collision Tooling.md.
/// </summary>
public static class ConnectCollisions
{
    /// <summary>The map load copies the distinct target maps into this many slots, without a bounds check</summary>
    public const int MaxTargets = 16;

    /// <summary>More distinct other areas than this trips an assert in the map load</summary>
    public const int MaxForeignAreas = 4;

    /// <summary>The Open World Load Level values the game looks up; 0 means 20</summary>
    public static readonly byte[] LoadLevels = { 0, 5, 10, 20, 30 };

    /// <summary>The World State Condition values the game tests (255 = none)</summary>
    public static readonly byte[] WorldStates = { 0, 10, 30, 11, 12, 255 };

    public static bool IsOpenWorldArea(int area) => area is >= 50 and <= 88;

    /// <summary>The map a MapID names as the game reads it (255 counts as 0), or null for no target</summary>
    public static string TargetName(byte[] mapId)
    {
        if (mapId == null || mapId.Length < 4 || mapId[0] >= 0x80)
            return null;

        static int Part(byte b) => b >= 0x80 ? 0 : b;

        return $"m{mapId[0]:D2}_{Part(mapId[1]):D2}_{Part(mapId[2]):D2}_{Part(mapId[3]):D2}";
    }

    /// <summary>
    /// The MapID for a map name, written as vanilla does: 255 for a last number of 0. Null if the name isn't mAA_BB_CC_DD.
    /// </summary>
    public static byte[] MapIdFor(string mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName) || !MapConnectionsUtil.TryParseMap(mapName.Trim(), out var parts) || parts.Length != 4)
            return null;

        return new[] { parts[0], parts[1], parts[2], parts[3] == 0 ? (byte)0xFF : parts[3] };
    }

    /// <summary>The area number of a map name (m10_00_00_00 → 10), or -1</summary>
    public static int AreaOf(string mapName)
    {
        return mapName != null && mapName.Length >= 3 && int.TryParse(mapName.Substring(1, 2), out var a) ? a : -1;
    }

    /// <summary>The open-world load level the game uses for a stored value (0 → 20)</summary>
    public static int EffectiveLoadLevel(byte value) => value == 0 ? 20 : value;

    /// <summary>Whether the world-state gate can ever let the connection apply</summary>
    public static bool CanBeActive(MSBE.Part.ConnectCollision connect) => !(connect.UnkT0A is 11 or 12 && !connect.UnkT0B);

    /// <summary>Number of target-map groups the connection shows: the Condition bit and the DispGroups bits</summary>
    public static int GroupCount(MSBE.Part.ConnectCollision connect)
    {
        var groups = connect.DisplayGroupStruct;

        if (groups == null)
            return 0;

        var bits = new HashSet<int>();

        if (groups.Condition is >= 0 and < 256)
            bits.Add(groups.Condition);

        for (var w = 0; w < groups.DispGroups.Length; w++)
        {
            for (var b = 0; b < 32; b++)
            {
                if ((groups.DispGroups[w] & (1u << b)) != 0)
                    bits.Add(w * 32 + b);
            }
        }

        return bits.Count;
    }

    public static IEnumerable<Entity> ConnectsIn(MapContainer map)
    {
        return map.Objects.Where(e => e.WrappedObject is MSBE.Part.ConnectCollision);
    }

    /// <summary>The connects attached to a collision of the map</summary>
    public static List<Entity> ConnectsOf(MapContainer map, string collisionName)
    {
        return ConnectsIn(map)
            .Where(e => string.Equals(((MSBE.Part.ConnectCollision)e.WrappedObject).CollisionName, collisionName, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>The distinct target maps and other areas of a map's connects, as the map load counts them</summary>
    public static (HashSet<string> Targets, SortedSet<int> Areas) Usage(MapContainer map, MSBE.Part.ConnectCollision exclude = null)
    {
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var areas = new SortedSet<int>();
        var ownArea = AreaOf(map.Name);

        foreach (var ent in ConnectsIn(map))
        {
            var connect = (MSBE.Part.ConnectCollision)ent.WrappedObject;

            if (connect == exclude)
                continue;

            var target = TargetName(connect.MapID);

            if (target == null || string.Equals(target, map.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            targets.Add(target);

            if (connect.MapID[0] != ownArea)
                areas.Add(connect.MapID[0]);
        }

        return (targets, areas);
    }

    /// <summary>
    /// Why a new connect from the map to the target can't be made, or null if it can: a bad or own map name, or a target
    /// that would pass the 16-map or 4-area limit.
    /// </summary>
    public static string CannotConnect(MapContainer map, string target)
    {
        var mapId = MapIdFor(target);

        if (mapId == null)
            return LOC.Get("MAP_ConnectCollision_Create_Bad_Map", target);

        var name = TargetName(mapId);

        if (string.Equals(name, map.Name, StringComparison.OrdinalIgnoreCase))
            return LOC.Get("MAP_ConnectCollision_Create_Own_Map");

        var (targets, areas) = Usage(map);

        if (!targets.Contains(name) && targets.Count >= MaxTargets)
            return LOC.Get("MAP_ConnectCollision_Create_Too_Many_Targets", MaxTargets);

        if (mapId[0] != AreaOf(map.Name) && !areas.Contains(mapId[0]) && areas.Count >= MaxForeignAreas)
            return LOC.Get("MAP_ConnectCollision_Create_Too_Many_Areas", MaxForeignAreas);

        return null;
    }

    /// <summary>
    /// A new ConnectCollision for a collision of the map, linked to the target map: the collision's model, transform and
    /// display data, vanilla defaults elsewhere (no world state, default load level, no target groups yet), named
    /// {model}_{instance}. Not added to the map yet.
    /// </summary>
    public static MsbEntity Prepare(MapEditorView view, MapContainer map, Entity collisionEnt, string target)
    {
        var collision = (MSBE.Part.Collision)collisionEnt.WrappedObject;

        var connect = new MSBE.Part.ConnectCollision
        {
            ModelName = collision.ModelName,
            CollisionName = collision.Name,
            MapID = MapIdFor(target),
            Position = collision.Position,
            Rotation = collision.Rotation,
            Scale = collision.Scale,
            EntityID = 0,
            UnkT08 = 0,
            UnkT09 = false,
            UnkT0A = 255,
            UnkT0B = false,
            DisplayDataStruct = collision.DisplayDataStruct.DeepCopy()
        };

        // As every vanilla connect has them: no tile map, no target groups yet
        connect.TileLoadConfig.MapID = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        connect.TileLoadConfig.TileLoadConfig_Unk0C = -1;
        connect.TileLoadConfig.CullingHeightBehavior = -1;
        connect.DisplayGroupStruct.Condition = -1;
        connect.DisplayGroupStruct.Unk26 = -1;

        var names = MsbNaming.GetNames(map, connect);
        new MassCreateTool.InstanceIdTracker(view.Project.Descriptor.ProjectType, map).Assign(connect, connect.ModelName, names);

        return new MsbEntity(view.Universe, map, connect, MsbEntityType.Part);
    }

    /// <summary>
    /// Property changes that give a connect its collision's model and transform (vanilla connects always match), or an
    /// empty list if they already match.
    /// </summary>
    public static List<ViewportAction> MatchCollision(Entity connectEnt, MSBE.Part.Collision collision)
    {
        var connect = (MSBE.Part.ConnectCollision)connectEnt.WrappedObject;
        var actions = new List<ViewportAction>();

        void Set(string property, object current, object wanted)
        {
            if (Equals(current, wanted))
                return;

            var prop = connect.GetType().GetProperty(property);
            actions.Add(new PropChangeAction(connectEnt, prop, connect, wanted));
        }

        Set(nameof(connect.ModelName), connect.ModelName, collision.ModelName);
        Set(nameof(connect.Position), connect.Position, collision.Position);
        Set(nameof(connect.Rotation), connect.Rotation, collision.Rotation);
        Set(nameof(connect.Scale), connect.Scale, collision.Scale);

        return actions;
    }

    /// <summary>
    /// A collision of the target map that could carry the connection back: how far its world bounds are from the source
    /// collision's (0 when touching), how much of the smaller box the overlap covers (0–1), and whether it already has
    /// a connect to the source map.
    /// </summary>
    public sealed record ReciprocalCandidate(Entity Collision, float Gap, float Overlap, bool AlreadyLinked);

    /// <summary>Candidates further than this from the source collision are left out</summary>
    public const float ReciprocalMaxGap = 5.0f;

    /// <summary>
    /// The target map's collisions nearest the source collision in world space (loaded ER maps are placed as in the
    /// game), overlapping first, then by gap; at most <paramref name="max" />. Null when the source collision has no
    /// loaded mesh, since its bounds are then only a point.
    /// </summary>
    public static List<ReciprocalCandidate> ReciprocalCandidates(MapContainer source, Entity sourceCollision, MapContainer target, int max = 6)
    {
        var sourceBox = DisplayGroupNeighbours.BoundsOf(sourceCollision);

        if (sourceBox.Min == sourceBox.Max)
            return null;

        var candidates = new List<ReciprocalCandidate>();

        foreach (var ent in target.Objects)
        {
            if (ent.WrappedObject is not MSBE.Part.Collision collision)
                continue;

            var box = DisplayGroupNeighbours.BoundsOf(ent);

            // Without a mesh only the position is known, which for collisions is usually the map origin
            if (box.Min == box.Max)
                continue;

            var gap = DisplayGroupNeighbours.BoxDistance(sourceBox, box);

            if (gap > ReciprocalMaxGap)
                continue;

            var linked = ConnectsOf(target, collision.Name)
                .Any(c => string.Equals(TargetName(((MSBE.Part.ConnectCollision)c.WrappedObject).MapID), source.Name, StringComparison.OrdinalIgnoreCase));

            candidates.Add(new ReciprocalCandidate(ent, gap, OverlapRatio(sourceBox, box), linked));
        }

        return candidates
            .OrderByDescending(c => c.Overlap)
            .ThenBy(c => c.Gap)
            .Take(max)
            .ToList();
    }

    /// <summary>The overlap's volume over the smaller box's (thin boxes count as 10 cm thick)</summary>
    private static float OverlapRatio(Veldrid.Utilities.BoundingBox a, Veldrid.Utilities.BoundingBox b)
    {
        const float minExtent = 0.1f;

        static float Volume(Vector3 size) =>
            MathF.Max(size.X, minExtent) * MathF.Max(size.Y, minExtent) * MathF.Max(size.Z, minExtent);

        var overlap = Vector3.Min(a.Max, b.Max) - Vector3.Max(a.Min, b.Min);

        if (overlap.X < 0 || overlap.Y < 0 || overlap.Z < 0)
            return 0;

        var smaller = MathF.Min(Volume(a.Max - a.Min), Volume(b.Max - b.Min));

        return smaller <= 0 ? 0 : MathF.Min(1, Volume(overlap) / smaller);
    }

    /// <summary>Whether a connect's model and transform match its collision's (within a millimetre)</summary>
    public static bool MatchesCollision(MSBE.Part.ConnectCollision connect, MSBE.Part.Collision collision)
    {
        return string.Equals(connect.ModelName, collision.ModelName, StringComparison.OrdinalIgnoreCase)
            && Vector3.Distance(connect.Position, collision.Position) < 0.001f
            && Vector3.Distance(connect.Rotation, collision.Rotation) < 0.001f
            && Vector3.Distance(connect.Scale, collision.Scale) < 0.001f;
    }
}
