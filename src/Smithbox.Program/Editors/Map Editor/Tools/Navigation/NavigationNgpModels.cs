using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's mesh edits (Models tab, Phase G4): adding, replacing, duplicating and removing navmeshes. Mesh i is
/// the map's i-th MSB navmesh part, and Smithbox writes parts in map order with a new part after the others, so a new
/// mesh goes at the end and a removed part's mesh comes out at its place. IDs embed the mesh index, so every edit goes
/// through NgpRebuild, which numbers them again. The rules come from the game files (NgpSurvey groups, names):
/// - models are nAA_XXYY (n01_0100), parts the model + _0000;
/// - each navmesh part sets one NavmeshGroup bit, which 1 to 3 collision parts share in their NvmGroups (1,317 of
///   1,345 parts): the navmesh of those collisions;
/// - faces wind clockwise seen from above (cross(b - a, c - a).Y &lt; 0, 97.7 % of vanilla faces).
/// Renderer-free, so NavSurvey checks them on the game files (`nvm GAME ngp-models`).
/// </summary>
public static class NgpModelEdit
{
    /// <summary>The most meshes a map can hold: mesh IDs keep the index in 7 bits.</summary>
    public const int MaxMeshes = 128;

    /// <summary>The most faces, vertices or edges a mesh can hold (15-bit indices).</summary>
    public const int MaxElements = 32767;

    /// <summary>
    /// The generator settings for an NGP mesh: a 4 m face grid (vanilla ground faces average 6.4 m², about a 3.6 m
    /// triangle), no links (NGP has none; its drops are face types).
    /// </summary>
    public static NavGenParameters DefaultParameters => NavGenParameters.DefaultN with { FaceSize = 4f, Links = false, MaxEdgeLength = 12f };

    private static readonly Regex ModelName = new(@"^n(\d\d)_(\d\d)(\d\d)$", RegexOptions.IgnoreCase);

    /// <summary>
    /// A new model and part name: the map's most used nAA prefix, the next free XX after its highest, YY 00
    /// (n01_3100), and the part the model + _0000; made unique among the given names. Null when nothing is free.
    /// </summary>
    public static (string Model, string Part) NewNames(NgpMap data, ICollection<string> takenNames)
    {
        var parsed = data.Meshes.Select(m => m.ModelName == null ? null : ModelName.Match(m.ModelName)).Where(m => m != null && m.Success).ToList();
        var aa = parsed.GroupBy(m => m.Groups[1].Value).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "01";
        var models = data.Meshes.Select(m => m.ModelName).Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        int next = parsed.Count == 0 ? 1 : parsed.Max(m => int.Parse(m.Groups[2].Value)) + 1;

        for (int xx = next; xx <= 99 + next; xx++)
        {
            for (int yy = 0; yy <= 99; yy++)
            {
                var model = $"n{aa}_{xx % 100:D2}{yy:D2}";
                var part = $"{model}_0000";
                if (!models.Contains(model) && !takenNames.Contains(part))
                    return (model, part);
            }
        }

        return (null, null);
    }

    /// <summary>
    /// A mesh from polygons in map-local space (shared corners as the same points, as the generator makes them): corners
    /// welded, each polygon fanned into faces wound clockwise from above, every face level 7, type ground. Its tables are
    /// built by NgpRebuild when it is added. Null when it would pass the limits.
    /// </summary>
    public static NGP.Mesh FromPolygons(IReadOnlyList<IReadOnlyList<Vector3>> polygons, int runtimeSlot = 0)
    {
        var mesh = new NGP.Mesh { RuntimeSlot = runtimeSlot };
        var index = new Dictionary<Vector3, int>();

        int Vertex(Vector3 p)
        {
            if (!index.TryGetValue(p, out var i))
            {
                i = mesh.Vertices.Count;
                index[p] = i;
                mesh.Vertices.Add(p);
            }
            return i;
        }

        foreach (var polygon in polygons)
        {
            if (polygon.Count < 3)
                continue;

            var corners = polygon.Select(Vertex).ToList();

            for (int k = 1; k + 1 < corners.Count; k++)
            {
                int a = corners[0], b = corners[k], c = corners[k + 1];
                if (a == b || b == c || a == c)
                    continue;

                // Clockwise from above, as the game's faces
                var normal = Vector3.Cross(mesh.Vertices[b] - mesh.Vertices[a], mesh.Vertices[c] - mesh.Vertices[a]);
                if (normal.Y > 0)
                    (b, c) = (c, b);

                mesh.Faces.Add(new NGP.Face { V1 = (short)a, V2 = (short)b, V3 = (short)c });
                mesh.FaceFlags.Add(NgpFlags.DefaultLevel);
            }
        }

        if (mesh.Faces.Count == 0 || !WithinLimits(mesh))
            return null;

        return mesh;
    }

    /// <summary>A copy of a mesh moved by an offset (map-local), keeping its faces and flags; its tables are rebuilt when added.</summary>
    public static NGP.Mesh Moved(NGP.Mesh mesh, Vector3 offset)
    {
        var copy = new NGP.Mesh { RuntimeSlot = mesh.RuntimeSlot };
        copy.Vertices.AddRange(mesh.Vertices.Select(v => v + offset));
        copy.Faces.AddRange(mesh.Faces.Select(f => new NGP.Face { V1 = f.V1, V2 = f.V2, V3 = f.V3 }));
        copy.FaceFlags.AddRange(mesh.FaceFlags);
        return copy;
    }

    /// <summary>The NGP with a mesh added at the end (its MSB part goes after the others), and its gates found.</summary>
    public static NGP WithMeshAdded(NGP ngp, NGP.Mesh mesh)
    {
        var copy = NgpEdit.Copy(ngp);
        copy.Meshes.Add(mesh);
        return NgpRebuild.Rebuild(copy, new[] { copy.Meshes.Count - 1 });
    }

    /// <summary>The NGP with a mesh's faces replaced (its part kept), and its gates found again.</summary>
    public static NGP WithMeshReplaced(NGP ngp, int index, NGP.Mesh mesh)
    {
        var copy = NgpEdit.Copy(ngp);
        mesh.RuntimeSlot = copy.Meshes[index].RuntimeSlot;
        copy.Meshes[index] = mesh;
        return NgpRebuild.Rebuild(copy, new[] { index });
    }

    /// <summary>
    /// The NGP without a mesh: its gates go, later meshes move down one (their IDs, and the gates' and edge refs' mesh
    /// fields, numbered again); the other meshes and gates are kept as they are.
    /// </summary>
    public static NGP WithMeshRemoved(NGP ngp, int index)
    {
        var copy = NgpEdit.Copy(ngp);
        byte map = copy.MapIndex;

        int Shift(int m) => m > index ? m - 1 : m;
        uint Ref(uint r) => NGP.Ids.EdgeRef(map, Shift(NGP.Ids.MeshOf(r)), NGP.Ids.IndexOf(r));

        var kept = new List<NGP.Gate>();
        foreach (var gate in copy.Gates)
        {
            int ma = NGP.Ids.MeshOf(gate.MeshAId), mb = NGP.Ids.MeshOf(gate.MeshBId);
            if (ma == index || mb == index)
                continue;

            gate.MeshAId = NGP.Ids.Mesh(map, Shift(ma));
            gate.MeshBId = NGP.Ids.Mesh(map, Shift(mb));

            for (int k = 0; k < gate.EdgeCount; k++)
            {
                int a = gate.SideAEdgeStart + k, b = gate.SideBEdgeStart + k;
                if (a >= 0 && a < copy.GateEdgeRefs.Count) copy.GateEdgeRefs[a] = Ref(copy.GateEdgeRefs[a]);
                if (b >= 0 && b < copy.GateEdgeRefs.Count) copy.GateEdgeRefs[b] = Ref(copy.GateEdgeRefs[b]);
            }

            kept.Add(gate);
        }

        copy.Gates = kept;
        copy.Meshes.RemoveAt(index);

        // Nothing changed geometry: the kept gates stay as they are, and every table and ID is numbered again
        return NgpRebuild.Rebuild(copy, Array.Empty<int>());
    }

    /// <summary>The edges a mesh's faces make (each vertex pair once), which the 15-bit limit holds as faces and vertices.</summary>
    public static int EdgeCount(NGP.Mesh mesh)
    {
        var edges = new HashSet<(int, int)>();
        foreach (var f in mesh.Faces)
        {
            edges.Add((Math.Min(f.V1, f.V2), Math.Max(f.V1, f.V2)));
            edges.Add((Math.Min(f.V2, f.V3), Math.Max(f.V2, f.V3)));
            edges.Add((Math.Min(f.V3, f.V1), Math.Max(f.V3, f.V1)));
        }
        return edges.Count;
    }

    /// <summary>Whether a mesh's faces, vertices and edges all fit the 15-bit indices.</summary>
    public static bool WithinLimits(NGP.Mesh mesh)
        => mesh.Faces.Count <= MaxElements && mesh.Vertices.Count <= MaxElements && EdgeCount(mesh) <= MaxElements;

    /// <summary>The index of the single NavmeshGroup bit a part sets (word * 32 + bit), or -1.</summary>
    public static int GroupBit(uint[] groups)
    {
        for (int w = 0; w < groups.Length; w++)
            if (groups[w] != 0)
                return w * 32 + System.Numerics.BitOperations.TrailingZeroCount(groups[w]);
        return -1;
    }

    /// <summary>The four group words with one bit set.</summary>
    public static uint[] GroupOf(int bit)
    {
        var groups = new uint[4];
        if (bit >= 0 && bit < 128)
            groups[bit / 32] = 1u << (bit % 32);
        return groups;
    }

    /// <summary>
    /// The group bit for a new navmesh made from collisions: the lowest bit the collisions' NvmGroups share (their
    /// navmesh's), else the lowest bit no navmesh part or collision of the map uses (-1 if none is free).
    /// </summary>
    public static int NewGroupBit(IReadOnlyList<uint[]> sourceGroups, IEnumerable<uint[]> navmeshGroups, IEnumerable<uint[]> collisionGroups, out bool fromSources)
    {
        fromSources = false;

        foreach (var groups in sourceGroups)
        {
            int bit = GroupBit(groups);
            if (bit >= 0)
            {
                fromSources = true;
                return bit;
            }
        }

        var used = new HashSet<int>();
        foreach (var groups in navmeshGroups.Concat(collisionGroups))
            for (int b = 0; b < 128; b++)
                if ((groups[b / 32] & (1u << (b % 32))) != 0)
                    used.Add(b);

        for (int b = 1; b < 128; b++)
            if (!used.Contains(b))
                return b;

        return -1;
    }
}
