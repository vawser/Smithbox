using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The Navigation tool's NGP mode (DS2, DS2S; Navigation.md (NGP (DS2, DS2S)), Phases G0 and G1): the map's one .ngp file,
// its meshes joined to the MSB navmesh parts by position, the flag field's meaning (Navigation.md (NGP (DS2, DS2S)), section 6) and the
// flag edit. Renderer-free, so Tools/NavSurvey links it and checks it on the game files.

/// <summary>The NGP games and how their files are laid out.</summary>
public sealed class NgpGameProfile
{
    public string Game { get; init; }

    /// <summary>The version the game reads: DS2 1 (32-bit offsets), SOTFS 2 (64-bit; its exe only relocates version 2).</summary>
    public NGP.NGPVersion Version { get; init; }

    public static readonly NgpGameProfile DS2 = new() { Game = "DS2", Version = NGP.NGPVersion.Vanilla };
    public static readonly NgpGameProfile DS2S = new() { Game = "DS2S", Version = NGP.NGPVersion.Scholar };

    /// <summary>The profile of an NGP game, or null for any other game.</summary>
    public static NgpGameProfile For(string game) => game switch
    {
        "DS2" => DS2,
        "DS2S" => DS2S,
        _ => null,
    };

    public string NgpPath(string mapId) => $"/map/{mapId}/{mapId}.ngp";

    public string MsbPath(string mapId) => $"/map/{mapId}/{mapId}.msb";
}

/// <summary>One NGP mesh and the MSB navmesh part of the same position.</summary>
public sealed class NgpMesh
{
    /// <summary>The mesh's position in the NGP, which is its MSB navmesh part's among the map's navmesh parts.</summary>
    public int Index { get; init; }

    public NGP.Mesh Mesh { get; init; }

    /// <summary>The part's name and model; null when the MSB has fewer navmesh parts than the NGP has meshes.</summary>
    public string PartName { get; init; }

    public string ModelName { get; init; }

    /// <summary>The MSB part object, when built from the Map Editor (null in NavSurvey).</summary>
    public object Part { get; init; }

    /// <summary>The part's name, or the mesh index when the MSB has no part for it.</summary>
    public string Label => PartName ?? $"#{Index}";
}

/// <summary>A map's NGP navigation data. Built anew from the document, so it reflects the latest edit.</summary>
public sealed class NgpMap
{
    public string MapId { get; init; }

    public NgpGameProfile Profile { get; init; }

    public NGP Ngp { get; init; }

    public List<NgpMesh> Meshes { get; init; } = new();

    /// <summary>The MSB's navmesh part count, which the game expects to equal the mesh count.</summary>
    public int PartCount { get; init; }

    /// <summary>
    /// Map-local to map space: the MSB's MapOffset translation, which the game adds to every vertex, bounding box, gate
    /// and tree split at load (DarkSoulsII.exe 0x140BAFFF0). Part rotations are not applied.
    /// </summary>
    public Matrix4x4 World { get; init; } = Matrix4x4.Identity;

    public string FileName => $"{MapId}.ngp";

    public Vector3 Place(Vector3 local) => Vector3.Transform(local, World);

    /// <summary>The mesh index of a mesh id, gate mesh field or ref (bits 17 to 23).</summary>
    public static int MeshOf(uint id) => NGP.Ids.MeshOf(id);
}

/// <summary>Reads a map's NGP data straight from the game or project files (the vanilla baseline, NavSurvey).</summary>
public static class NgpReader
{
    public sealed record PartInfo(string Name, string ModelName, object Part);

    private static byte[] ReadBytes(VirtualFileSystem fs, string path)
    {
        var file = fs.ReadFile(path);
        if (file == null)
            return null;

        var data = file.Value.ToArray();
        return DCX.Is(data) ? DCX.Decompress(data, out DCX.Type _).ToArray() : data;
    }

    /// <summary>The map's NGP joined to its MSB's navmesh parts, or null if the map has no NGP.</summary>
    public static NgpMap Read(VirtualFileSystem fs, string mapId, NgpGameProfile profile)
    {
        var ngpBytes = ReadBytes(fs, profile.NgpPath(mapId));
        if (ngpBytes == null)
            return null;

        var parts = new List<PartInfo>();
        var offset = Vector3.Zero;

        if (ReadBytes(fs, profile.MsbPath(mapId)) is byte[] msbBytes)
        {
            var msb = MSB2.Read(msbBytes);
            parts.AddRange(msb.Parts.Navmeshes.Select(p => new PartInfo(p.Name, p.ModelName, p)));
            offset = msb.Events.MapOffsets.FirstOrDefault()?.Translation ?? Vector3.Zero;
        }

        return Build(mapId, profile, NGP.Read(ngpBytes), parts, Matrix4x4.CreateTranslation(offset));
    }

    /// <summary>Joins the NGP's meshes to the MSB navmesh parts (in MSB order) by position.</summary>
    public static NgpMap Build(string mapId, NgpGameProfile profile, NGP ngp, IReadOnlyList<PartInfo> parts, Matrix4x4 world)
    {
        var map = new NgpMap { MapId = mapId, Profile = profile, Ngp = ngp, PartCount = parts.Count, World = world };

        for (int i = 0; i < ngp.Meshes.Count; i++)
        {
            var part = i < parts.Count ? parts[i] : null;
            map.Meshes.Add(new NgpMesh { Index = i, Mesh = ngp.Meshes[i], PartName = part?.Name, ModelName = part?.ModelName, Part = part?.Part });
        }

        return map;
    }
}

/// <summary>What the overlay colours faces by: one of the flag field's parts, or the whole field.</summary>
public enum NgpColorBy { Flags, Type, Level, Attributes }

/// <summary>
/// The face flag field (Navigation.md (NGP (DS2, DS2S)), sections 6.4 to 6.6): bits 0-2 the level, a body-size gate; bits 3-6 the type;
/// bits 7-10 the attributes; nothing above. Names are LOC keys, MAP_Navigation_Ngp_Type_{Name} and so on.
/// </summary>
public static class NgpFlags
{
    public const uint LevelMask = 0x7;
    public const uint TypeMask = 0x78;
    public const uint AttributeMask = 0x780;
    public const uint KnownMask = LevelMask | TypeMask | AttributeMask;

    /// <summary>The level of a face every enemy may use; what new faces get.</summary>
    public const uint DefaultLevel = 7;

    public static uint Level(uint flags) => flags & LevelMask;
    public static uint Type(uint flags) => flags & TypeMask;
    public static uint Attributes(uint flags) => flags & AttributeMask;

    /// <summary>The flags with one part replaced.</summary>
    public static uint WithLevel(uint flags, uint level) => flags & ~LevelMask | level & LevelMask;
    public static uint WithType(uint flags, uint type) => flags & ~TypeMask | type & TypeMask;

    /// <summary>The value a face's colour is keyed by.</summary>
    public static uint Key(uint flags, NgpColorBy by) => by switch
    {
        NgpColorBy.Type => Type(flags),
        NgpColorBy.Level => Level(flags),
        NgpColorBy.Attributes => Attributes(flags),
        _ => flags,
    };

    /// <summary>
    /// The 16 type values and their names: those the game handles (door threshold, ladder, rope, one-way drop, event
    /// area) from the exe, the others from where their faces lie (Navigation.md (NGP (DS2, DS2S)) 6.5).
    /// </summary>
    public static readonly (uint Value, string Name)[] Types =
    {
        (0x00, "Ground"), (0x08, "DoorApproach"), (0x10, "DoorThreshold"), (0x18, "LadderLanding"),
        (0x20, "Ladder"), (0x28, "RopeEnd"), (0x30, "Rope"), (0x38, "Type38"),
        (0x40, "Drop"), (0x48, "EventArea"), (0x50, "Type50"), (0x58, "Water"),
        (0x60, "Type60"), (0x68, "Type68"), (0x70, "Type70"), (0x78, "Type78"),
    };

    /// <summary>The attribute bits: 0x100 blocks, 0x200 a closed door, 0x400 a closed door enemies may pass, 0x80 unknown.</summary>
    public static readonly (uint Bit, string Name)[] AttributeBits =
    {
        (0x80, "Attribute80"), (0x100, "Blocked"), (0x200, "DoorClosed"), (0x400, "DoorPassable"),
    };

    public static string TypeName(uint flags)
    {
        uint t = Type(flags);
        foreach (var (value, name) in Types)
            if (value == t)
                return name;
        return "Type38";
    }

    public static IEnumerable<string> AttributeNames(uint flags)
        => AttributeBits.Where(a => (flags & a.Bit) != 0).Select(a => a.Name);

    /// <summary>
    /// The CHR_WEIGHT_TYPE values (0 None … 7 Very Enormous) a level admits: an enemy's query level is its weight type
    /// minus one (None and Minimum both 0), and a face accepts it if its level is greater (DarkSoulsII.exe 0x14042ED50,
    /// 0x140BAF0D0). That weightType is the source is likely, not proven (Navigation.md (NGP (DS2, DS2S)) 6.4).
    /// </summary>
    public static IEnumerable<int> WeightTypesAdmitted(uint level)
        => Enumerable.Range(0, 8).Where(w => Math.Max(0, w - 1) < (int)level);
}

/// <summary>The NGP mode's edits, as copies: each returns a new NGP and leaves the one given as it was (undo).</summary>
public static class NgpEdit
{
    public static NGP Copy(NGP ngp) => NGP.Read(ngp.Write());

    /// <summary>A copy with faces' flags changed. Flags change no geometry, so no other table changes.</summary>
    public static NGP WithFlags(NGP ngp, IReadOnlyDictionary<(int Mesh, int Face), uint> flags)
    {
        var copy = Copy(ngp);

        foreach (var ((mesh, face), value) in flags)
        {
            if (mesh >= 0 && mesh < copy.Meshes.Count && face >= 0 && face < copy.Meshes[mesh].FaceFlags.Count)
                copy.Meshes[mesh].FaceFlags[face] = value;
        }

        return copy;
    }

    /// <summary>
    /// A copy with meshes' vertices moved (topology kept), their bounding boxes and trees and the map's gates rebuilt
    /// (NgpRebuild). Gates whose edges no longer meet go; edges that come to meet get a gate.
    /// </summary>
    public static NGP WithVertices(NGP ngp, IReadOnlyDictionary<int, IReadOnlyList<Vector3>> vertices)
    {
        var copy = Copy(ngp);

        foreach (var (mesh, positions) in vertices)
        {
            var target = copy.Meshes[mesh].Vertices;
            for (int i = 0; i < target.Count && i < positions.Count; i++)
                target[i] = positions[i];
        }

        return NgpRebuild.Rebuild(copy, vertices.Keys.ToList(), keepTopology: true);
    }
}

/// <summary>Geometry of NGP faces and edges in map-local space.</summary>
public static class NgpGeometry
{
    public static bool InRange(NGP.Face f, int vertices)
        => f.V1 >= 0 && f.V1 < vertices && f.V2 >= 0 && f.V2 < vertices && f.V3 >= 0 && f.V3 < vertices;

    public static (Vector3 A, Vector3 B, Vector3 C) Corners(NGP.Mesh mesh, int face)
    {
        var f = mesh.Faces[face];
        return (mesh.Vertices[f.V1], mesh.Vertices[f.V2], mesh.Vertices[f.V3]);
    }

    /// <summary>A face's slope in degrees (0 flat, 90 vertical), or -1 for a degenerate face.</summary>
    public static float Slope(NGP.Mesh mesh, int face)
    {
        var (a, b, c) = Corners(mesh, face);
        var normal = Vector3.Cross(b - a, c - a);
        float length = normal.Length();
        return length < 1e-6f ? -1 : MathF.Acos(Math.Clamp(MathF.Abs(normal.Y) / length, 0f, 1f)) * 180f / MathF.PI;
    }

    /// <summary>
    /// The faces whose centre (map space) lies inside any of the boxes, each a model-space box placed by a world
    /// matrix (an object's or region's bounds, as the viewport's selection box shows them), grown by a margin in metres.
    /// What the game does for a door: it collects the door threshold faces under the door object's box (0x140451460).
    /// </summary>
    public static List<(int Mesh, int Face)> FacesInBoxes(NgpMap map, IEnumerable<(Matrix4x4 World, Vector3 Min, Vector3 Max)> boxes, float margin)
    {
        var placed = new List<(Matrix4x4 Inverse, Vector3 Min, Vector3 Max, Vector3 Margin)>();
        foreach (var (world, min, max) in boxes)
        {
            if (!Matrix4x4.Invert(world, out var inverse))
                continue;

            // The margin is in metres: in model space it is divided by the box's scale on each axis
            var scale = new Vector3(new Vector3(world.M11, world.M12, world.M13).Length(), new Vector3(world.M21, world.M22, world.M23).Length(),
                new Vector3(world.M31, world.M32, world.M33).Length());
            var m = new Vector3(margin / Math.Max(scale.X, 1e-6f), margin / Math.Max(scale.Y, 1e-6f), margin / Math.Max(scale.Z, 1e-6f));
            placed.Add((inverse, min, max, m));
        }

        var result = new List<(int, int)>();
        if (placed.Count == 0)
            return result;

        foreach (var nm in map.Meshes)
        {
            var mesh = nm.Mesh;
            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                if (!InRange(mesh.Faces[f], mesh.Vertices.Count))
                    continue;

                var (a, b, c) = Corners(mesh, f);
                var centre = map.Place((a + b + c) / 3);

                foreach (var (inverse, min, max, m) in placed)
                {
                    var local = Vector3.Transform(centre, inverse);
                    if (local.X >= min.X - m.X && local.X <= max.X + m.X && local.Y >= min.Y - m.Y && local.Y <= max.Y + m.Y
                        && local.Z >= min.Z - m.Z && local.Z <= max.Z + m.Z)
                    {
                        result.Add((nm.Index, f));
                        break;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// The faces of a type (ground by default) at least as steep as a slope: the one-way drops a generated mesh needs
    /// marked (vanilla drop faces: 95 % at 38° or more; only 756 of 103,207 ground faces are over 50°).
    /// </summary>
    public static List<(int Mesh, int Face)> SteepFaces(NgpMap map, float minSlope, uint ofType = 0x00, Func<NgpMesh, bool> include = null)
    {
        var result = new List<(int, int)>();
        foreach (var nm in map.Meshes)
        {
            if (include != null && !include(nm))
                continue;

            var mesh = nm.Mesh;
            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                if (!InRange(mesh.Faces[f], mesh.Vertices.Count) || f >= mesh.FaceFlags.Count || NgpFlags.Type(mesh.FaceFlags[f]) != ofType)
                    continue;

                if (Slope(mesh, f) >= minSlope)
                    result.Add((nm.Index, f));
            }
        }

        return result;
    }

    /// <summary>The edge index of a half-edge (edge &lt;&lt; 1 | side).</summary>
    public static int EdgeOf(ushort halfEdge) => halfEdge >> 1;

    public static int SideOf(ushort halfEdge) => halfEdge & 1;
}
