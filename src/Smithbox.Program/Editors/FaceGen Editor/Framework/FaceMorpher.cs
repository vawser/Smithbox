#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// Applies a face's shape to the face part models of a character mesh, on the GPU.
/// <para>
/// Each face part (fg_a_*) carries its own EGM (how each FaceGen mode moves each of its TRI vertices) and flver2tri
/// (the TRI vertex of each FLVER vertex, per mesh). A FLVER vertex is moved by its TRI vertex's displacement, with
/// Z negated (FLVER Z = −TRI Z). Working with displacements, not positions, keeps meshes that sit off the TRI surface
/// (eyelash cards, the heads' extra meshes) where they are relative to the skin.
/// </para>
/// <para>
/// The shared head of DS1 and DS1R (<see cref="FaceComposition.SharedHead"/>) carries no FaceGen files: its EGMs
/// and flver2tri are the fgbnd's, one EGM per mesh by the TRI its entry names (face.tri, eye.tri).
/// </para>
/// <para>
/// The new positions are written over the vertices the model was loaded with (kept for face parts by
/// <see cref="FlverResource.RetainVerticesFor"/>). Models are shared by everything that shows them, so the original
/// vertices are put back by <see cref="Restore"/> when the face is no longer shown. Normals keep the base shape's.
/// </para>
/// </summary>
public sealed class FaceMorpher
{
    /// <summary>A part's FaceGen data: per flver2tri entry (so per FLVER mesh) the EGM of its TRI, or null to leave it</summary>
    /// <param name="Indices">Per FLVER mesh, the TRI vertex of each of its vertices (the flver2tri entry's indices)</param>
    private sealed record PartData(EGM?[] EntryEgms, IReadOnlyList<List<short>> Indices, FLVER2? Flver);

    /// <summary>
    /// How far a head's edge is pulled onto the body: an edge vertex within this of the body's surface is moved onto it.
    /// ER's heads meet the body's skull and neck to within about 2 mm (FaceGenSurvey seamcheck), and the face's modes
    /// never move their edge (their EGM deltas there are 0), so the seam is closed once and stays closed.
    /// </summary>
    private const float SeamReach = 0.003f;

    /// <summary>The head parts, whose edges meet the body (face_partsId's family)</summary>
    private const string HeadPrefix = "fg_a_01";

    /// <summary>The body models (CharacterComposition's)</summary>
    private const string BodyPrefix = "fc_";

    // The body's head and neck triangles, by body model (null: none read)
    private readonly Dictionary<string, List<(Vector3 A, Vector3 B, Vector3 C)>?> BodyTriangles = new();

    // Per head and body: per mesh, the edge vertices and the offset that puts each on the body
    private readonly Dictionary<(string Head, string Body), Dictionary<int, Dictionary<int, Vector3>>> Seams = new();

    private readonly ProjectEntry Project;

    // The FaceGen files of each face part, read once (null: the part has none)
    private readonly Dictionary<string, PartData?> Parts = new();

    // The submeshes that have been written, so they can be put back
    private readonly HashSet<FlverResource.FlverSubmesh> Morphed = new();

    /// <summary>Makes face parts keep their vertices when they load. Call before any face part is loaded.</summary>
    public static void RetainFaceParts()
    {
        FlverResource.RetainVerticesFor = name => name.StartsWith("fg_a_", StringComparison.OrdinalIgnoreCase) ||
                                                  name.Equals(FaceComposition.BundleHead, StringComparison.OrdinalIgnoreCase);
    }

    public FaceMorpher(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Shapes every face part of <paramref name="mesh"/> (the parts are <paramref name="appearance"/>'s, in order) by
    /// the shape coefficients. False while a part isn't loaded yet, so it is to be called again.
    /// </summary>
    public bool Apply(MeshRenderableProxy mesh, CharacterAppearance appearance, float[] coefficients)
    {
        var done = true;
        var body = appearance.Parts.FirstOrDefault(p => p.Name.StartsWith(BodyPrefix, StringComparison.OrdinalIgnoreCase))?.Name;

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            var name = appearance.Parts[i].Name;

            if (!name.StartsWith("fg_a_", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals(FaceComposition.BundleHead, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (GetPartData(name) is not { } data)
                continue;

            // A head's edge, pulled onto the body it sits on
            var seam = body != null && name.StartsWith(HeadPrefix, StringComparison.OrdinalIgnoreCase)
                ? GetSeam(name, data, body)
                : null;

            if (mesh.Submeshes[i]._meshProvider.ResourceHandle is not ResourceHandle<FlverResource> { IsLoaded: true } handle ||
                handle.Get()?.GPUMeshes is not { } gpuMeshes)
            {
                done = false;
                continue;
            }

            // Each TRI's displacement, once (the shared head's meshes are on face.tri and eye.tri)
            var displacements = new Dictionary<EGM, Vector3[]>();

            for (var m = 0; m < gpuMeshes.Length && m < data.Indices.Count; m++)
            {
                var submesh = gpuMeshes[m];

                if (data.EntryEgms[m] is not { } egm || submesh.RetainedVertices is not { } original || submesh.GeomBuffer is not { } buffer)
                    continue;

                if (!displacements.TryGetValue(egm, out var displacement))
                {
                    displacements[egm] = displacement = new Vector3[egm.VertexCount];
                    egm.Apply(displacement, coefficients);
                }

                if (buffer.AllocStatus != VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
                {
                    done = false;
                    continue;
                }

                buffer.UpdateResidentVertices(Displace(original, submesh.VertexStride, submesh.VertexCount,
                    data.Indices[m], displacement, seam != null && seam.TryGetValue(m, out var snaps) ? snaps : null));
                Morphed.Add(submesh);
            }
        }

        return done;
    }

    private static byte[] Displace(byte[] original, uint stride, int vertexCount, List<short> indices, Vector3[] displacement,
        Dictionary<int, Vector3>? seam)
    {
        var data = (byte[])original.Clone();
        var count = Math.Min(Math.Min(vertexCount, indices.Count), (int)(data.Length / Math.Max(stride, 1)));

        for (var j = 0; j < count; j++)
        {
            var tri = indices[j];
            var move = tri >= 0 && tri < displacement.Length
                ? new Vector3(displacement[tri].X, displacement[tri].Y, -displacement[tri].Z)
                : Vector3.Zero;

            // An edge vertex goes onto the body (its modes leave it where it is)
            if (seam != null && seam.TryGetValue(j, out var pull))
                move += pull;

            if (move == Vector3.Zero)
                continue;

            var span = data.AsSpan((int)(j * stride), 12);
            var position = MemoryMarshal.Read<Vector3>(span) + move;
            MemoryMarshal.Write(span, in position);
        }

        return data;
    }

    /// <summary>
    /// The edge vertices of a head's meshes that come within <see cref="SeamReach"/> of the body's surface, each with
    /// the offset that puts it on that surface. Edges are those of one triangle in the full-detail face set, by
    /// position (so a UV seam inside the mesh is not an edge). Worked out once per head and body.
    /// </summary>
    private Dictionary<int, Dictionary<int, Vector3>> GetSeam(string head, PartData data, string body)
    {
        if (Seams.TryGetValue((head, body), out var known))
            return known;

        var result = new Dictionary<int, Dictionary<int, Vector3>>();
        Seams[(head, body)] = result;

        if (data.Flver is not { } flver || GetBodyTriangles(body) is not { Count: > 0 } triangles)
            return result;

        for (var m = 0; m < flver.Meshes.Count; m++)
        {
            var mesh = flver.Meshes[m];
            var faceSet = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None) ?? mesh.FaceSets.FirstOrDefault();

            if (faceSet == null)
                continue;

            var tris = faceSet.Triangulate(mesh.Vertices.Count < ushort.MaxValue);
            var keys = mesh.Vertices.Select(v => Key(v.Position)).ToList();
            var edges = new Dictionary<(Vector3, Vector3), int>();

            for (var t = 0; t + 2 < tris.Count; t += 3)
            {
                for (var e = 0; e < 3; e++)
                {
                    var a = keys[tris[t + e]];
                    var b = keys[tris[t + (e + 1) % 3]];
                    var edge = Less(a, b) ? (a, b) : (b, a);
                    edges[edge] = edges.GetValueOrDefault(edge) + 1;
                }
            }

            var boundary = edges.Where(e => e.Value == 1).SelectMany(e => new[] { e.Key.Item1, e.Key.Item2 }).ToHashSet();
            var snaps = new Dictionary<int, Vector3>();

            for (var v = 0; v < mesh.Vertices.Count; v++)
            {
                if (!boundary.Contains(keys[v]))
                    continue;

                var p = mesh.Vertices[v].Position;
                var closest = p;
                var best = SeamReach;

                foreach (var (a, b, c) in triangles)
                {
                    var q = ClosestOnTriangle(p, a, b, c);
                    var d = Vector3.Distance(p, q);

                    if (d < best)
                    {
                        best = d;
                        closest = q;
                    }
                }

                if (closest != p)
                    snaps[v] = closest - p;
            }

            if (snaps.Count > 0)
                result[m] = snaps;
        }

        return result;

        static Vector3 Key(Vector3 p) => new(MathF.Round(p.X, 4), MathF.Round(p.Y, 4), MathF.Round(p.Z, 4));

        static bool Less(Vector3 a, Vector3 b) => a.X != b.X ? a.X < b.X : a.Y != b.Y ? a.Y < b.Y : a.Z < b.Z;
    }

    /// <summary>
    /// The triangles of a body model's plain meshes (#NN#, not its own default face, hair or variants), from its
    /// partsbnd, in the rest pose both it and the head are drawn in
    /// </summary>
    private List<(Vector3 A, Vector3 B, Vector3 C)>? GetBodyTriangles(string body)
    {
        if (BodyTriangles.TryGetValue(body, out var known))
            return known;

        List<(Vector3, Vector3, Vector3)>? result = null;

        try
        {
            if (ReadPartFile(body, ".flver") is { } bytes)
            {
                var flver = FLVER2.Read(bytes);
                result = new();

                foreach (var mesh in flver.Meshes)
                {
                    var material = flver.Materials[mesh.MaterialIndex];

                    if (!PlainMask.IsMatch(material.Name) ||
                        material.MTD?.Contains("[FG_", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        continue;
                    }

                    var faceSet = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None) ?? mesh.FaceSets.FirstOrDefault();

                    if (faceSet == null)
                        continue;

                    var tris = faceSet.Triangulate(mesh.Vertices.Count < ushort.MaxValue);

                    for (var t = 0; t + 2 < tris.Count; t += 3)
                    {
                        result.Add((mesh.Vertices[tris[t]].Position, mesh.Vertices[tris[t + 1]].Position,
                            mesh.Vertices[tris[t + 2]].Position));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Part_Read_Failed", body), e);
            result = null;
        }

        BodyTriangles[body] = result;
        return result;
    }

    private static readonly System.Text.RegularExpressions.Regex PlainMask = new(@"^#\d+#$");

    /// <summary>The closest point to p on triangle abc (Ericson, Real-Time Collision Detection 5.1.5)</summary>
    private static Vector3 ClosestOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a, ac = c - a, ap = p - a;
        float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0) return a;
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3) return b;
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0) return a + d1 / (d1 - d3) * ab;
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6) return c;
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0) return a + d2 / (d2 - d6) * ac;
        float va = d3 * d6 - d5 * d4;
        if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0) return b + (d4 - d3) / ((d4 - d3) + (d5 - d6)) * (c - b);
        float denom = 1 / (va + vb + vc);
        return a + ab * (vb * denom) + ac * (vc * denom);
    }

    /// <summary>Puts back the vertices of everything that was shaped</summary>
    public void Restore()
    {
        foreach (var submesh in Morphed)
        {
            if (submesh.RetainedVertices is { } original && submesh.GeomBuffer is { } buffer &&
                buffer.AllocStatus == VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
            {
                buffer.UpdateResidentVertices(original);
            }
        }

        Morphed.Clear();
    }

    /// <summary>
    /// A file of a part's partsbnd whose name ends in <paramref name="suffix"/> (not a _M.flver, the low-detail model),
    /// decompressed, or null
    /// </summary>
    private Memory<byte>? ReadPartFile(string model, string suffix)
    {
        var entry = Project.Locator.PartFiles.Entries.FirstOrDefault(e =>
            e.Filename.Equals(model, StringComparison.OrdinalIgnoreCase));

        if (entry == null || Project.VFS.FS.ReadFile(entry.Path) is not { } raw)
            return null;

        var bytes = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        var isBnd4 = bytes.Length >= 4 && bytes.Span[..4].SequenceEqual("BND4"u8);
        IBinder binder = isBnd4 ? BND4.Read(bytes) : BND3.Read(bytes);

        var file = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true &&
                                                   f.Name?.EndsWith("_M" + suffix, StringComparison.OrdinalIgnoreCase) != true);

        return file == null ? null : DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes) : file.Bytes;
    }

    /// <summary>
    /// The shared head's FaceGen data, from the fgbnd (DS1, DS1R): its face.flver2tri (version 1) maps the part's
    /// FLVER mesh by mesh, the same four meshes as the fgbnd's own face.flver, and each entry names its TRI (face.tri,
    /// eye.tri; DS1R's say face.tribe, Demon's Souls' big-endian name for the same mesh). Each mesh takes the EGM of
    /// its TRI's name (face.egm, eye.egm, or .egmbe). Null if the fgbnd lacks them.
    /// </summary>
    private PartData? GetSharedHeadData()
    {
        var fgbnd = FaceGenModelData.Get(Project);

        if (fgbnd.File("face.flver2tri") is not { } mapBytes)
            return null;

        var map = F2TR.Read(mapBytes);
        var egms = new Dictionary<string, EGM?>(StringComparer.OrdinalIgnoreCase);

        EGM? EgmOf(string triName)
        {
            var stem = System.IO.Path.GetFileNameWithoutExtension(triName.Replace('\\', '/'));

            if (egms.TryGetValue(stem, out var known))
                return known;

            var bytes = fgbnd.File(stem + ".egm") ?? fgbnd.File(stem + ".egmbe");
            return egms[stem] = bytes is { } b ? EGM.Read(b) : null;
        }

        return new PartData(map.Entries.Select(e => EgmOf(e.Name)).ToArray(), map.Entries.Select(e => e.Indices).ToList(), null);
    }

    /// <summary>How close a FLVER vertex must be to a TRI vertex to be it (Demon's Souls' head matches to within 1 µm)</summary>
    private const float MatchReach = 1e-4f;

    /// <summary>
    /// The FaceGen bundle's own head (Demon's Souls: face.flver, FLVER0, with face.tribe and face.egmbe and no
    /// flver2tri). Each FLVER vertex is the TRI vertex at its position, Z negated: FaceGenSurvey deshead finds every
    /// one of the head's 1340 vertices on a TRI vertex to within 1 µm. A vertex with none within
    /// <see cref="MatchReach"/> is left unmoved (index −1). Null if the bundle lacks the files.
    /// </summary>
    private PartData? GetBundleHeadData()
    {
        var bundle = FaceGenModelData.Get(Project);

        if (bundle.File("face.flver") is not { } flverBytes ||
            (bundle.File("face.tri") ?? bundle.File("face.tribe")) is not { } triBytes ||
            (bundle.File("face.egm") ?? bundle.File("face.egmbe")) is not { } egmBytes)
        {
            return null;
        }

        var tri = TRI.Read(triBytes);
        var egm = EGM.Read(egmBytes);
        var count = Math.Min(egm.VertexCount, tri.Vertices.Count);

        // The TRI's vertices in FLVER space, bucketed by position for the lookup
        const float cell = 0.001f;
        var grid = new Dictionary<(int, int, int), List<int>>();

        (int, int, int) Cell(Vector3 p) => ((int)MathF.Floor(p.X / cell), (int)MathF.Floor(p.Y / cell), (int)MathF.Floor(p.Z / cell));

        var points = new Vector3[count];

        for (var t = 0; t < count; t++)
        {
            points[t] = new Vector3(tri.Vertices[t].X, tri.Vertices[t].Y, -tri.Vertices[t].Z);

            var key = Cell(points[t]);
            if (!grid.TryGetValue(key, out var list))
                grid[key] = list = new List<int>();
            list.Add(t);
        }

        short Match(Vector3 p)
        {
            var (cx, cy, cz) = Cell(p);
            var best = MatchReach;
            var found = -1;

            for (var x = cx - 1; x <= cx + 1; x++)
            for (var y = cy - 1; y <= cy + 1; y++)
            for (var z = cz - 1; z <= cz + 1; z++)
            {
                if (!grid.TryGetValue((x, y, z), out var list))
                    continue;

                foreach (var t in list)
                {
                    var d = Vector3.Distance(p, points[t]);
                    if (d < best)
                    {
                        best = d;
                        found = t;
                    }
                }
            }

            return (short)found;
        }

        var bytes = flverBytes.ToArray();
        List<List<short>> indices;

        if (FLVER0.Is(bytes))
            indices = FLVER0.Read(bytes).Meshes.Select(m => m.Vertices.Select(v => Match(v.Position)).ToList()).ToList();
        else if (FLVER2.Is(bytes))
            indices = FLVER2.Read(bytes).Meshes.Select(m => m.Vertices.Select(v => Match(v.Position)).ToList()).ToList();
        else
            return null;

        return new PartData(indices.Select(_ => (EGM?)egm).ToArray(), indices, null);
    }

    /// <summary>The EGM and flver2tri of a face part, from its partsbnd</summary>
    private PartData? GetPartData(string model)
    {
        if (Parts.TryGetValue(model, out var known))
            return known;

        PartData? data = null;

        try
        {
            if (model.Equals(FaceComposition.BundleHead, StringComparison.OrdinalIgnoreCase))
            {
                data = GetBundleHeadData();
                Parts[model] = data;
                return data;
            }

            var entry = Project.Locator.PartFiles.Entries.FirstOrDefault(e =>
                e.Filename.Equals(model, StringComparison.OrdinalIgnoreCase));

            if (entry != null && Project.VFS.FS.ReadFile(entry.Path) is { } raw)
            {
                var bytes = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
                var isBnd4 = bytes.Length >= 4 && bytes.Span[..4].SequenceEqual("BND4"u8);
                IBinder binder = isBnd4 ? BND4.Read(bytes) : BND3.Read(bytes);

                var egm = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(".egm", StringComparison.OrdinalIgnoreCase) == true);
                var map = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(".flver2tri", StringComparison.OrdinalIgnoreCase) == true);
                var flver = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(".flver", StringComparison.OrdinalIgnoreCase) == true);

                if (egm != null && map != null)
                {
                    // The model too, for its edges (GetSeam)
                    var flverBytes = flver == null ? null : DCX.Is(flver.Bytes) ? DCX.Decompress(flver.Bytes) : flver.Bytes;
                    var headModel = flverBytes is { } fb && FLVER2.Is(fb.ToArray()) ? FLVER2.Read(fb) : null;
                    var partEgm = EGM.Read(egm.Bytes);
                    var partMap = F2TR.Read(map.Bytes);
                    data = new PartData(partMap.Entries.Select(_ => (EGM?)partEgm).ToArray(), partMap.Entries.Select(e => e.Indices).ToList(), headModel);
                }
                else if (egm == null && map == null &&
                         model.Equals(FaceComposition.SharedHead, StringComparison.OrdinalIgnoreCase))
                {
                    data = GetSharedHeadData();
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Part_Read_Failed", model), e);
        }

        Parts[model] = data;
        return data;
    }
}
