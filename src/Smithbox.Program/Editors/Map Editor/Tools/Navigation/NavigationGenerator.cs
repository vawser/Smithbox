using DotRecast.Core.Numerics;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Generating a navmesh from collision (Navigation Editor Plan, Phase 4; Navmesh Generation Plan): Recast (DotRecast)
// voxelises the collision's walkable surfaces into convex polygons, which become an hkaiNavMesh laid out as the game's
// are, with a face tree for its query mediator. Kept free of the rest of Smithbox; NavSurvey `navgen` runs it on the
// game's collision and compares the result with the game's own navmeshes.

/// <summary>The agent and voxel settings a navmesh is generated with (Recast's, in metres and degrees).</summary>
public sealed record NavGenParameters
{
    /// <summary>The horizontal size of a voxel.</summary>
    public float CellSize { get; init; } = 0.2f;

    /// <summary>The vertical size of a voxel.</summary>
    public float CellHeight { get; init; } = 0.1f;

    /// <summary>The steepest slope a character walks up.</summary>
    public float MaxSlope { get; init; } = 60f;

    /// <summary>The headroom a character needs.</summary>
    public float AgentHeight { get; init; } = 1.8f;

    /// <summary>How far the navmesh is kept from walls and ledges.</summary>
    public float AgentRadius { get; init; } = 0.1f;

    /// <summary>The highest step a character climbs.</summary>
    public float MaxClimb { get; init; } = 0.8f;

    /// <summary>Islands smaller than this many square metres are dropped.</summary>
    public float MinRegionArea { get; init; } = 2f;

    /// <summary>Regions smaller than this many square metres are merged into their neighbours.</summary>
    public float MergeRegionArea { get; init; } = 20f;

    /// <summary>The longest a boundary edge may be; 0 for no limit.</summary>
    public float MaxEdgeLength { get; init; } = 8f;

    /// <summary>How far a simplified boundary may stray from the voxels' outline.</summary>
    public float MaxEdgeError { get; init; } = 1.0f;

    /// <summary>The most corners a face may have (3 to 6).</summary>
    public int MaxVerticesPerFace { get; init; } = 6;

    /// <summary>Move each vertex onto the collision surface below or just above it, as the game's sit.</summary>
    public bool SnapToCollision { get; init; } = true;

    /// <summary>
    /// The size of the world grid every face is split along, in metres, for faces of an even size (links and face data
    /// are set per face); 0 keeps Recast's faces, which can be large.
    /// </summary>
    public float FaceSize { get; init; } = 2f;

    /// <summary>Split every face into triangles, after the grid.</summary>
    public bool TrianglesOnly { get; init; }

    /// <summary>Add jump and fall links between faces across gaps and down drops (NavLinkGenerator).</summary>
    public bool Links { get; init; }

    /// <summary>A link whose ends differ in height by no more than this, either way, is a jump, both ways; 0 for none.</summary>
    public float JumpHeight { get; init; } = 2.5f;

    /// <summary>The deepest drop a fall link goes down; deeper than the jump height (and the climb), it is a fall, one way.</summary>
    public float FallHeight { get; init; } = 15f;

    /// <summary>How far out from a boundary edge a link's landing is looked for, across.</summary>
    public float LinkReach { get; init; } = 5f;

    /// <summary>
    /// Defaults for the n set (normal characters): a fine mesh close to the collision. Tuned with NavSurvey navgen on
    /// m10_01 and m12_01, whose walkable surfaces are all map collision: it covers 87% (median) of the game's n meshes.
    /// </summary>
    public static NavGenParameters DefaultN => new();

    /// <summary>Defaults for the o set (large characters): a coarser mesh. Covers 66% (median) of the game's o meshes.</summary>
    public static NavGenParameters DefaultO => new()
    {
        CellSize = 0.3f,
        AgentHeight = 2.5f,
        AgentRadius = 0.3f,
        MaxClimb = 1.2f,
        MinRegionArea = 6f,
        MaxEdgeLength = 12f,
        // The game's o meshes have falls only, reaching further across (NavSurvey linkgeom)
        JumpHeight = 0f,
        LinkReach = 9f,
    };
}

/// <summary>A generated navmesh, or why there is none.</summary>
public sealed class NavGenResult
{
    public IHavokNavMeshEdit Mesh { get; init; }

    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; init; }

    public int InputTriangles { get; init; }

    public int SnappedVertices { get; init; }

    /// <summary>How many faces a blocker cut.</summary>
    public int ClippedFaces { get; init; }

    /// <summary>Vertices and face centres left inside a blocker's cut: a self-check, which should be 0.</summary>
    public int InsideCut { get; init; }

    /// <summary>Faces dropped as still wound backwards after the clean-up: a last resort, normally 0.</summary>
    public int DroppedFaces { get; init; }

    /// <summary>The jump links (both ways) and fall links (one way) added, when links are on.</summary>
    public int JumpLinks { get; init; }

    public int FallLinks { get; init; }

    public bool Ok => Error == null;
}

public static class NavigationGenerator
{
    /// <summary>Recast's index for a missing vertex or neighbour.</summary>
    private const int Null = 0xFFFF;

    /// <summary>
    /// Generates a navmesh from triangles (three points each, in the space the navmesh is to have). The template gives
    /// the per-face and per-mesh values the game's files carry (cluster index, padding, flags, AABB w); faces get no face
    /// data, edges flags 4 (ORIGINAL) and no edge data, as the game's do.
    /// </summary>
    /// <param name="blockers">
    /// Boxes that are solid and never walkable, in the same space. No part of the navmesh is left inside a box widened
    /// by the agent radius at its sides and reaching a climb below its base: a hard cut, made three ways.
    /// - The box is voxelised with the walkable collision, unwalkable, so the floor under it is covered and its top isn't
    ///   walkable;
    /// - its outline seen from above, widened, is marked unwalkable from a climb below its base to its top, as Recast
    ///   would otherwise let a character step onto anything less than a climb high;
    /// - every face is then clipped against the widened box, as Recast's simplified outlines can reach up to the max
    ///   edge error back into the cut. The pieces outside are kept, joined to their neighbours.
    /// </param>
    /// <param name="createMesh">
    /// Makes the empty mesh to build into when there is no template mesh, in the class set of the files it will go into
    /// (the template file's). With a template, the mesh is made in the template's class set.
    /// </param>
    public static NavGenResult Generate(IReadOnlyList<Vector3> triangles, NavGenParameters p, IHavokNavMeshEdit template = null, IReadOnlyList<NavBlocker> blockers = null,
        Func<IHavokNavMeshEdit> createMesh = null)
    {
        if (triangles.Count < 3)
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_No_Collision" };

        blockers ??= Array.Empty<NavBlocker>();
        var cuts = blockers.Select(b => b.Widened(p.AgentRadius, p.MaxClimb)).ToList();
        var blockerTriangles = blockers.SelectMany(b => b.Triangles()).ToList();
        int walkableCount = triangles.Count / 3, blockerCount = blockerTriangles.Count / 3;
        var vertices = new float[(walkableCount + blockerCount) * 9];
        var faces = new int[(walkableCount + blockerCount) * 3];
        for (int i = 0; i < (walkableCount + blockerCount) * 3; i++)
        {
            var point = i < walkableCount * 3 ? triangles[i] : blockerTriangles[i - walkableCount * 3];
            vertices[i * 3] = point.X;
            vertices[i * 3 + 1] = point.Y;
            vertices[i * 3 + 2] = point.Z;
            faces[i] = i;
        }

        // The walkable collision alone, for the bounds and the input geometry Recast's later steps read
        var walkableVertices = vertices.AsSpan(0, walkableCount * 9).ToArray();
        var geometry = new RcSampleInputGeomProvider(walkableVertices, faces.AsSpan(0, walkableCount * 3).ToArray());

        foreach (var cut in cuts)
        {
            var corners = cut.Corners();
            var outline = Outline(corners, p.CellSize);
            if (outline.Count < 3)
                continue;

            var volume = new float[outline.Count * 3];
            for (int i = 0; i < outline.Count; i++)
            {
                volume[i * 3] = outline[i].X;
                volume[i * 3 + 2] = outline[i].Y;
            }

            geometry.AddConvexVolume(volume, corners.Min(v => v.Y) - p.CellHeight, corners.Max(v => v.Y) + p.CellHeight,
                new RcAreaModification(RcRecast.RC_NULL_AREA));
        }

        // The Recast settings in voxels; the area settings are in squared voxels
        int regionMin = Math.Max(1, (int)MathF.Round(MathF.Sqrt(p.MinRegionArea) / p.CellSize));
        int regionMerge = Math.Max(1, (int)MathF.Round(MathF.Sqrt(p.MergeRegionArea) / p.CellSize));
        var config = new RcConfig(RcPartition.WATERSHED, p.CellSize, p.CellHeight, p.MaxSlope, p.AgentHeight, p.AgentRadius, p.MaxClimb,
            regionMin, regionMerge, p.MaxEdgeLength, p.MaxEdgeError, Math.Clamp(p.MaxVerticesPerFace, 3, 6), 6f, 1f,
            true, true, true, new RcAreaModification(RcRecast.RC_WALKABLE_AREA), false);

        RcBuilderResult built;
        try
        {
            // The heightfield over the walkable collision, with room above and below for blockers
            var bmin = geometry.GetMeshBoundsMin();
            var bmax = geometry.GetMeshBoundsMax();
            foreach (var b in blockerTriangles)
            {
                if (b.X < bmin.X || b.X > bmax.X || b.Z < bmin.Z || b.Z > bmax.Z)
                    continue;
                bmin.Y = MathF.Min(bmin.Y, b.Y);
                bmax.Y = MathF.Max(bmax.Y, b.Y);
            }

            var context = new DotRecast.Core.RcContext();
            var builderConfig = new RcBuilderConfig(config, bmin, bmax);
            var solid = new RcHeightfield(builderConfig.width, builderConfig.height, builderConfig.bmin, builderConfig.bmax, config.Cs, config.Ch, config.BorderSize);

            // Walkable by slope, as Recast marks them; blockers never
            var areas = RcRecast.MarkWalkableTriangles(context, config.WalkableSlopeAngle, vertices, faces, walkableCount + blockerCount, config.WalkableAreaMod);
            for (int t = walkableCount; t < walkableCount + blockerCount; t++)
                areas[t] = RcRecast.RC_NULL_AREA;

            RcRasterizations.RasterizeTriangles(context, vertices, faces, areas, walkableCount + blockerCount, solid, config.WalkableClimb);
            built = new RcBuilder().Build(context, 0, 0, geometry, config, solid, false);
        }
        catch (Exception)
        {
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_Recast", InputTriangles = triangles.Count / 3 };
        }

        var poly = built?.Mesh;
        if (poly == null || poly.npolys == 0)
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_Empty", InputTriangles = triangles.Count / 3 };

        // Vertices, from voxel coordinates
        var points = new Vector3[poly.nverts];
        for (int v = 0; v < poly.nverts; v++)
            points[v] = new Vector3(poly.bmin.X + poly.verts[v * 3] * poly.cs, poly.bmin.Y + poly.verts[v * 3 + 1] * poly.ch, poly.bmin.Z + poly.verts[v * 3 + 2] * poly.cs);

        int snapped = 0;
        var surface = p.SnapToCollision ? new SurfaceGrid(triangles, 4f) : null;
        if (surface != null)
        {
            for (int v = 0; v < points.Length; v++)
            {
                if (surface.Nearest(points[v], p.MaxClimb + 2 * p.CellHeight) is float y)
                {
                    points[v].Y = y;
                    snapped++;
                }
            }
        }

        var polygons = Polygons(poly, points);

        // Even faces: split along a world grid; the new corners go onto the collision too
        if (p.FaceSize > 0)
        {
            polygons = SplitByGrid(polygons, MathF.Max(0.25f, p.FaceSize));

            if (surface != null)
            {
                foreach (var polygon in polygons)
                    for (int i = 0; i < polygon.Count; i++)
                        if (surface.Nearest(polygon[i], p.MaxClimb + 2 * p.CellHeight) is float y)
                            polygon[i] = polygon[i] with { Y = y };
            }
        }

        // The hard cut, last, so nothing after it moves a corner back into a box: every face clipped against every
        // widened box
        int clipped = 0;
        foreach (var cut in cuts)
        {
            var next = new List<List<Vector3>>(polygons.Count);
            foreach (var polygon in polygons)
            {
                var pieces = cut.Outside(polygon);
                if (pieces.Count != 1 || !ReferenceEquals(pieces[0], polygon))
                    clipped++;
                foreach (var piece in pieces.Where(q => q.Count >= 3 && AreaXZ(q) > 1e-4f))
                {
                    // A cut through a face that isn't flat can leave a piece that bends in, seen from above: it goes in as triangles
                    if (ReferenceEquals(piece, polygon) || ConvexXZ(piece))
                        next.Add(piece);
                    else
                        next.AddRange(EarClip(piece));
                }
            }
            polygons = next;
        }

        if (polygons.Count == 0)
            return new NavGenResult { Error = "MAP_Navigation_Generate_Error_Empty", InputTriangles = triangles.Count / 3 };

        var mesh = BuildMesh(polygons, template, createMesh, p.TrianglesOnly, out int dropped);

        // Self-check: vertices and face centres still inside a cut (should be none)
        int insideCut = 0;
        foreach (var v in mesh.m_vertices)
            if (cuts.Any(c => c.Contains(new Vector3(v.X, v.Y, v.Z), -0.01f)))
                insideCut++;
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var centre = NavigationOverlayBuilder.FaceCentre(mesh, f);
            if (cuts.Any(c => c.Contains(centre, -0.01f)))
                insideCut++;
        }

        // Jump and fall links, last, as user edges after all the original edges; their paths go round the collision and
        // the blockers' boxes
        int jumps = 0, falls = 0;
        if (p.Links)
        {
            var solids = new List<Vector3>(triangles.Count + blockerTriangles.Count);
            solids.AddRange(triangles);
            solids.AddRange(blockerTriangles);

            var found = NavLinkGenerator.Find(mesh, solids, p);
            if (found.Links.Count > 0 && NavUserEdges.AddAll(mesh, found.Links).Ok)
            {
                jumps = found.Jumps;
                falls = found.Falls;
            }
        }

        return new NavGenResult { Mesh = mesh, InputTriangles = triangles.Count / 3, SnappedVertices = snapped, ClippedFaces = clipped, InsideCut = insideCut, DroppedFaces = dropped, JumpLinks = jumps, FallLinks = falls };
    }

    /// <summary>
    /// The convex outline of a set of points seen from above (X, Z), widened by a margin: the hull of the points and of
    /// eight points around each hull corner at the margin's distance. Counter-clockwise.
    /// </summary>
    private static List<Vector2> Outline(IEnumerable<Vector3> points, float margin)
    {
        var hull = Hull(points.Select(v => new Vector2(v.X, v.Z)));
        if (margin <= 0)
            return hull;

        var widened = new List<Vector2>();
        foreach (var corner in hull)
            for (int k = 0; k < 8; k++)
                widened.Add(corner + new Vector2(MathF.Cos(k * MathF.PI / 4), MathF.Sin(k * MathF.PI / 4)) * margin / MathF.Cos(MathF.PI / 8));
        return Hull(widened);
    }

    /// <summary>The convex hull of points (Andrew's monotone chain), counter-clockwise.</summary>
    private static List<Vector2> Hull(IEnumerable<Vector2> input)
    {
        var pts = input.Distinct().OrderBy(q => q.X).ThenBy(q => q.Y).ToList();
        if (pts.Count < 3)
            return pts;

        static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        var hull = new List<Vector2>();
        for (int pass = 0; pass < 2; pass++)
        {
            int start = hull.Count;
            foreach (var q in pass == 0 ? pts : Enumerable.Reverse(pts))
            {
                while (hull.Count >= start + 2 && Cross(hull[^2], hull[^1], q) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(q);
            }
            hull.RemoveAt(hull.Count - 1);
        }
        return hull;
    }

    /// <summary>
    /// Convex faces split along the lines x = k·size and z = k·size: each piece is convex, and two faces that share an
    /// edge split it at the same points, so the mesh builder joins them again. Slivers are dropped.
    /// </summary>
    private static List<List<Vector3>> SplitByGrid(List<List<Vector3>> polygons, float size)
    {
        var result = new List<List<Vector3>>();

        // Corners within 2 cm of a line go onto it: decided by the corner alone, so every face with it agrees, and no
        // cut leaves a sliver beside a corner
        Vector3 Snap(Vector3 q)
        {
            float x = MathF.Round(q.X / size) * size, z = MathF.Round(q.Z / size) * size;
            return new Vector3(MathF.Abs(q.X - x) < 0.02f ? x : q.X, q.Y, MathF.Abs(q.Z - z) < 0.02f ? z : q.Z);
        }

        foreach (var original in polygons)
        {
            var snapped = original.Select(Snap).ToList();
            var convex = ConvexXZ(snapped) ? new List<List<Vector3>> { snapped } : EarClip(snapped);

            foreach (var polygon in convex)
            foreach (var column in SplitAxis(polygon, size, q => q.X))
                foreach (var piece in SplitAxis(column, size, q => q.Z))
                    if (piece.Count >= 3 && AreaXZ(piece) > 1e-4f)
                        result.Add(piece);
        }

        return result;
    }

    /// <summary>
    /// A convex polygon cut by every grid line of one axis that crosses it, low side first. A corner on a line is kept in
    /// both pieces; a crossing point depends only on its edge's two ends, so faces sharing the edge cut it at the same point.
    /// </summary>
    private static List<List<Vector3>> SplitAxis(List<Vector3> polygon, float size, Func<Vector3, float> axis)
    {
        var pieces = new List<List<Vector3>>();
        float lo = polygon.Min(axis), hi = polygon.Max(axis);
        var rest = polygon;

        for (float line = (MathF.Floor(lo / size) + 1) * size; line < hi - 1e-4f && rest.Count >= 3; line += size)
        {
            float at = line;
            if (line <= lo + 1e-4f)
                continue;

            var (below, above) = NavBlocker.Split(rest, q => at - axis(q));
            if (below.Count >= 3)
                pieces.Add(below);
            rest = above;
        }

        if (rest.Count >= 3)
            pieces.Add(rest);
        return pieces;
    }

    /// <summary>Whether a polygon, seen from above, turns the same way (counter-clockwise) at every corner.</summary>
    private static bool ConvexXZ(List<Vector3> polygon)
    {
        for (int k = 0; k < polygon.Count; k++)
        {
            var a = polygon[k];
            var b = polygon[(k + 1) % polygon.Count];
            var c = polygon[(k + 2) % polygon.Count];
            // As the sine of the turn, so a long edge's float error isn't taken for a bend
            if (TurnSine(a, b, c) > 1e-3f)
                return false;
        }
        return true;
    }

    /// <summary>The turn at b as the sine of its angle (negative for counter-clockwise), 0 for a zero-length side.</summary>
    private static float TurnSine(Vector3 a, Vector3 b, Vector3 c)
    {
        // In double: far from the origin a float's step is a tenth of a millimetre, enough to tip a near-straight corner
        double abx = (double)b.X - a.X, abz = (double)b.Z - a.Z, bcx = (double)c.X - b.X, bcz = (double)c.Z - b.Z;
        double lengths = Math.Sqrt((abx * abx + abz * abz) * (bcx * bcx + bcz * bcz));
        return lengths > 1e-12 ? (float)((abx * bcz - abz * bcx) / lengths) : 0;
    }

    /// <summary>The turn at b, seen from above (X right, Z down the page): negative for counter-clockwise, as the faces wind.</summary>
    private static float TurnXZ(Vector3 a, Vector3 b, Vector3 c)
        => (float)(((double)b.X - a.X) * ((double)c.Z - b.Z) - ((double)b.Z - a.Z) * ((double)c.X - b.X));

    /// <summary>
    /// A simple polygon (counter-clockwise from above) as triangles, by ear clipping in XZ: each step cuts off a corner
    /// that turns the right way and has no other corner inside it. Corners on a straight line are kept until last.
    /// </summary>
    private static List<List<Vector3>> EarClip(List<Vector3> polygon)
    {
        var result = new List<List<Vector3>>();
        var rest = new List<Vector3>(polygon);
        int guard = rest.Count * rest.Count;

        while (rest.Count > 3 && guard-- > 0)
        {
            bool cut = false;
            for (int i = 0; i < rest.Count && !cut; i++)
            {
                var a = rest[(i + rest.Count - 1) % rest.Count];
                var b = rest[i];
                var c = rest[(i + 1) % rest.Count];

                // Only an ear that turns by a real angle: a near-straight corner would make a sliver whose winding is noise
                if (TurnSine(a, b, c) > -1e-3f)
                    continue;

                bool empty = true;
                foreach (var q in rest)
                {
                    if (q == a || q == b || q == c)
                        continue;
                    if (TurnXZ(a, b, q) <= 0 && TurnXZ(b, c, q) <= 0 && TurnXZ(c, a, q) <= 0)
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty)
                    continue;

                result.Add(new List<Vector3> { a, b, c });
                rest.RemoveAt(i);
                cut = true;
            }

            if (!cut)
                break;
        }

        if (rest.Count >= 3 && AreaXZ(rest) > 1e-6f)
            result.Add(rest);
        return result;
    }

    private static float AreaXZ(List<Vector3> polygon)
    {
        float area = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            area += a.X * b.Z - b.X * a.Z;
        }
        return MathF.Abs(area) / 2;
    }

    /// <summary>The faces of Recast's polygon mesh as corner lists, wound counter-clockwise seen from above.</summary>
    private static List<List<Vector3>> Polygons(RcPolyMesh poly, Vector3[] points)
    {
        var polygons = new List<List<Vector3>>(poly.npolys);

        for (int i = 0; i < poly.npolys; i++)
        {
            int start = i * 2 * poly.nvp;
            var corners = new List<Vector3>();
            for (int k = 0; k < poly.nvp && poly.polys[start + k] != Null; k++)
                corners.Add(points[poly.polys[start + k]]);

            if (corners.Count < 3)
                continue;

            // Counter-clockwise from above: the normal of the corners points up
            var normal = Vector3.Zero;
            for (int k = 1; k + 1 < corners.Count; k++)
                normal += Vector3.Cross(corners[k] - corners[0], corners[k + 1] - corners[0]);
            if (normal.Y < 0)
                corners.Reverse();

            polygons.Add(corners);
        }

        return polygons;
    }

    /// <summary>
    /// Faces (corner lists, counter-clockwise from above) as an hkaiNavMesh: corners at the same place (within a
    /// millimetre) are one vertex; a vertex lying on another face's edge is inserted into that edge, so faces that
    /// share a border share its edges; each edge runs from its corner to the next, and inner edges are named by the
    /// edge and face on their other side.
    /// </summary>
    private static IHavokNavMeshEdit BuildMesh(List<List<Vector3>> polygons, IHavokNavMeshEdit template, Func<IHavokNavMeshEdit> createMesh,
        bool trianglesOnly, out int dropped)
    {
        var templateFace = template?.m_faces.FirstOrDefault();
        float w = template?.m_vertices.FirstOrDefault().W ?? 1f;

        // A mesh of the template's class set, so its files can take the mesh's faces and edges as they are
        var mesh = template?.CreateEmpty() ?? createMesh?.Invoke()
            ?? throw new InvalidOperationException("A generated navmesh needs a template mesh, or a way to make one, for its class set.");
        mesh.m_faceDataStriding = 1;
        mesh.m_edgeDataStriding = 1;
        mesh.m_flags = template?.m_flags ?? 0;
        mesh.m_erosionRadius = 0;
        mesh.m_userData = template?.m_userData ?? 0;

        // Welded vertices: a corner within 1 mm of a vertex is that vertex (the voxels are far coarser, so no real corners
        // meet), found among the neighbouring cells of a 1 mm hash, so two points either side of a cell border still meet
        const float weld = 0.001f;
        var points = new List<Vector3>();
        var byCell = new Dictionary<(long, long, long), List<int>>();
        (long, long, long) CellOf(Vector3 q) => ((long)MathF.Floor(q.X / weld), (long)MathF.Floor(q.Y / weld), (long)MathF.Floor(q.Z / weld));
        int Weld(Vector3 q)
        {
            var (cx, cy, cz) = CellOf(q);
            for (long x = cx - 1; x <= cx + 1; x++)
                for (long y = cy - 1; y <= cy + 1; y++)
                    for (long z = cz - 1; z <= cz + 1; z++)
                        if (byCell.TryGetValue((x, y, z), out var near))
                            foreach (var index in near)
                                if (Vector3.DistanceSquared(points[index], q) <= weld * weld)
                                    return index;

            int added = points.Count;
            points.Add(q);
            if (!byCell.TryGetValue((cx, cy, cz), out var list))
                byCell[(cx, cy, cz)] = list = new List<int>();
            list.Add(added);
            return added;
        }

        var faces = new List<List<int>>();
        foreach (var polygon in polygons)
        {
            var corners = new List<int>();
            foreach (var q in polygon)
            {
                int index = Weld(q);
                if (corners.Count == 0 || corners[^1] != index)
                    corners.Add(index);
            }
            if (corners.Count > 1 && corners[0] == corners[^1])
                corners.RemoveAt(corners.Count - 1);
            if (corners.Count >= 3)
                faces.Add(corners);
        }

        // T-junctions: a vertex on another face's edge goes into that edge
        var grid = new Dictionary<(int, int), List<int>>();
        const float cell = 2f;
        for (int v = 0; v < points.Count; v++)
        {
            var key = ((int)MathF.Floor(points[v].X / cell), (int)MathF.Floor(points[v].Z / cell));
            if (!grid.TryGetValue(key, out var list))
                grid[key] = list = new List<int>();
            list.Add(v);
        }

        foreach (var face in faces)
        {
            for (int k = 0; k < face.Count; k++)
            {
                int ia = face[k], ib = face[(k + 1) % face.Count];
                var a = points[ia];
                var b = points[ib];
                var ab = b - a;
                float length2 = ab.LengthSquared();
                if (length2 < 1e-8f)
                    continue;

                var between = new List<(float T, int V)>();
                for (int x = (int)MathF.Floor(MathF.Min(a.X, b.X) / cell); x <= (int)MathF.Floor(MathF.Max(a.X, b.X) / cell); x++)
                    for (int z = (int)MathF.Floor(MathF.Min(a.Z, b.Z) / cell); z <= (int)MathF.Floor(MathF.Max(a.Z, b.Z) / cell); z++)
                    {
                        if (!grid.TryGetValue((x, z), out var list))
                            continue;
                        foreach (var v in list)
                        {
                            if (v == ia || v == ib)
                                continue;
                            float t = Vector3.Dot(points[v] - a, ab) / length2;
                            if (t <= 1e-4f || t >= 1 - 1e-4f)
                                continue;
                            if (Vector3.DistanceSquared(a + ab * t, points[v]) < 4e-6f)
                                between.Add((t, v));
                        }
                    }

                if (between.Count == 0)
                    continue;

                var inserted = between.OrderBy(x => x.T).Select(x => x.V).Distinct().ToList();
                face.InsertRange(k + 1, inserted);
                k += inserted.Count;
            }
        }

        // Edges shorter than 9 mm are collapsed: their two vertices become one, everywhere at once, so the faces either
        // side stay joined and the slivers cuts leave beside corners vanish
        var parent = Enumerable.Range(0, points.Count).ToArray();
        int Find(int v)
        {
            while (parent[v] != v)
            {
                parent[v] = parent[parent[v]];
                v = parent[v];
            }
            return v;
        }

        foreach (var face in faces)
            for (int k = 0; k < face.Count; k++)
            {
                int a = Find(face[k]), b = Find(face[(k + 1) % face.Count]);
                if (a != b && Vector3.DistanceSquared(points[a], points[b]) < 8.1e-5f)
                    parent[Math.Max(a, b)] = Math.Min(a, b);
            }

        foreach (var face in faces)
            for (int k = 0; k < face.Count; k++)
                face[k] = Find(face[k]);

        // Tidied: no corner twice in a row, and no spike (a corner that goes out and straight back)
        foreach (var face in faces)
        {
            bool changed = true;
            while (changed && face.Count >= 3)
            {
                changed = false;
                for (int k = 0; k < face.Count && face.Count >= 3; k++)
                {
                    int next = (k + 1) % face.Count, after = (k + 2) % face.Count;
                    if (face[k] == face[next])
                    {
                        face.RemoveAt(next);
                        changed = true;
                    }
                    else if (face[k] == face[after])
                    {
                        face.RemoveAt(Math.Max(next, after));
                        face.RemoveAt(Math.Min(next, after));
                        changed = true;
                    }
                }
            }
        }
        faces.RemoveAll(face => face.Count < 3);

        // Slivers are dissolved and bent faces triangulated, in rounds, as either step can leave work for the other
        for (int round = 0; round < 3; round++)
        {
        // Slivers: a triangle that is nearly flat (its far corner within 0.2% of its longest edge's length of that edge's
        // line) or wound backwards is dissolved. Its far corner goes into the face across its longest edge, which then
        // shares the sliver's other two edges, so the faces around it stay joined. Repeated until none is left.
        for (int pass = 0; pass < 8; pass++)
        {
            var owner = new Dictionary<(int, int), int>();
            for (int i = 0; i < faces.Count; i++)
                for (int k = 0; k < faces[i].Count; k++)
                    owner[(faces[i][k], faces[i][(k + 1) % faces[i].Count])] = i;

            var removed = new HashSet<int>();
            bool any = false;

            for (int i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                if (face.Count != 3 || removed.Contains(i))
                    continue;

                // The longest edge a -> b, and the far corner c
                int longest = 0;
                float best = -1;
                for (int k = 0; k < 3; k++)
                {
                    float length = Vector2.DistanceSquared(new Vector2(points[face[k]].X, points[face[k]].Z), new Vector2(points[face[(k + 1) % 3]].X, points[face[(k + 1) % 3]].Z));
                    if (length > best)
                    {
                        best = length;
                        longest = k;
                    }
                }

                int a = face[longest], b = face[(longest + 1) % 3], c = face[(longest + 2) % 3];
                float turn = TurnXZ(points[a], points[b], points[c]);
                bool backwards = turn > 0;
                bool flat = MathF.Abs(turn) < 2e-3f * best;
                if (!backwards && !flat)
                    continue;

                if (owner.TryGetValue((b, a), out int across) && across != i && !removed.Contains(across))
                {
                    var neighbour = faces[across];
                    int at = -1;
                    for (int k = 0; k < neighbour.Count; k++)
                        if (neighbour[k] == b && neighbour[(k + 1) % neighbour.Count] == a)
                            at = k;
                    if (at < 0)
                        continue;
                    neighbour.Insert(at + 1, c);
                }

                removed.Add(i);
                any = true;
            }

            if (!any)
                break;

            faces = faces.Where((_, i) => !removed.Contains(i)).ToList();
        }

        // A face that still bends in (a few millimetres moved by the steps above), or any face when only triangles are
        // wanted, is split into triangles of its own corners by ear clipping: its edges stay as they are, so its neighbours
        // stay joined, and a corner on a straight edge (shared with a neighbour) is kept rather than cut off flat
        for (int i = faces.Count - 1; i >= 0; i--)
        {
            var corners = faces[i].Select(v => points[v]).ToList();
            if (ConvexXZ(corners) && (!trianglesOnly || corners.Count == 3))
                continue;

            var byPoint = new Dictionary<Vector3, int>();
            for (int k = 0; k < faces[i].Count; k++)
                byPoint.TryAdd(corners[k], faces[i][k]);

            var triangles = EarClip(corners).Select(t => t.Select(q => byPoint[q]).ToList()).Where(t => t.Distinct().Count() == t.Count).ToList();
            faces.RemoveAt(i);
            faces.InsertRange(i, triangles);
        }
        }

        // Last resort: a face still wound backwards (seen from above) after the rounds is dropped, as an inverted face is
        // worse than a gap of its width; its neighbours' edges stay on the boundary
        dropped = faces.RemoveAll(face =>
        {
            double area = 0;
            for (int k = 0; k < face.Count; k++)
            {
                var a = points[face[k]];
                var b = points[face[(k + 1) % face.Count]];
                area += (double)a.X * b.Z - (double)b.X * a.Z;
            }
            // The faces wind counter-clockwise from above, which is a negative shoelace sum here (as TurnXZ is)
            return area >= 0;
        });

        foreach (var point in points)
            mesh.m_vertices.Add(new Vector4(point, w));

        var byVertices = new Dictionary<(int, int), int>();
        var edgeFace = new List<int>();

        foreach (var corners in faces)
        {
            var face = mesh.NewFace(mesh.m_edges.Count, (short)corners.Count, -1, 0,
                templateFace?.m_clusterIndex ?? -1, templateFace?.m_padding ?? 0);

            int faceIndex = mesh.m_faces.Count;
            mesh.m_faces.Add(face);
            mesh.m_faceData.Add(0);

            for (int k = 0; k < corners.Count; k++)
            {
                int a = corners[k], b = corners[(k + 1) % corners.Count];
                // An edge two faces share the same way round can't pair: it stays on the boundary (-1)
                byVertices[(a, b)] = byVertices.ContainsKey((a, b)) ? -1 : mesh.m_edges.Count;
                mesh.m_edges.Add(mesh.NewEdge(a, b, 0xFFFFFFFF, 0xFFFFFFFF, HavokNavEdgeFlags.EDGE_ORIGINAL, 0));
                mesh.m_edgeData.Add(0);
                edgeFace.Add(faceIndex);
            }
        }

        // Inner edges: the same two vertices the other way round, when both ways are unique
        for (int e = 0; e < mesh.m_edges.Count; e++)
        {
            var edge = mesh.m_edges[e];
            if (byVertices.TryGetValue((edge.m_b, edge.m_a), out int other) && other >= 0 && other != e
                && byVertices.GetValueOrDefault((edge.m_a, edge.m_b), -1) == e)
            {
                edge.m_oppositeEdge = (uint)other;
                edge.m_oppositeFace = (uint)edgeFace[other];
            }
        }

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        mesh.m_aabb.Set(new Vector4(min, template?.m_aabb.m_min.W ?? 1f), new Vector4(max, template?.m_aabb.m_max.W ?? 1f));

        return mesh;
    }

    /// <summary>Triangles (three points each) bucketed on an XZ grid, for the surface height under a point.</summary>
    private sealed class SurfaceGrid
    {
        private readonly IReadOnlyList<Vector3> T;
        private readonly float Cell;
        private readonly Dictionary<(int, int), List<int>> Cells = new();

        public SurfaceGrid(IReadOnlyList<Vector3> triangles, float cell)
        {
            T = triangles;
            Cell = cell;

            for (int i = 0; i + 2 < T.Count; i += 3)
            {
                // Walkable-facing triangles only: a wall's height says nothing about the floor
                var n = Vector3.Cross(T[i + 1] - T[i], T[i + 2] - T[i]);
                if (n.LengthSquared() < 1e-12f || MathF.Abs(n.Y) / n.Length() < 0.1f)
                    continue;

                float x0 = MathF.Min(T[i].X, MathF.Min(T[i + 1].X, T[i + 2].X)), x1 = MathF.Max(T[i].X, MathF.Max(T[i + 1].X, T[i + 2].X));
                float z0 = MathF.Min(T[i].Z, MathF.Min(T[i + 1].Z, T[i + 2].Z)), z1 = MathF.Max(T[i].Z, MathF.Max(T[i + 1].Z, T[i + 2].Z));

                for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                    for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var list))
                            Cells[(x, z)] = list = new List<int>();
                        list.Add(i);
                    }
            }
        }

        /// <summary>The height of the surface nearest a point straight up or down, within a range, or null.</summary>
        public float? Nearest(Vector3 p, float range)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list))
                return null;

            float? best = null;
            foreach (var i in list)
            {
                if (HeightAt(T[i], T[i + 1], T[i + 2], p.X, p.Z) is not float y || MathF.Abs(y - p.Y) > range)
                    continue;
                if (best == null || MathF.Abs(y - p.Y) < MathF.Abs(best.Value - p.Y))
                    best = y;
            }
            return best;
        }

        private static float? HeightAt(Vector3 a, Vector3 b, Vector3 c, float x, float z)
        {
            float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
            if (MathF.Abs(d) < 1e-9f)
                return null;

            float u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / d;
            float v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / d;
            float w = 1 - u - v;

            // A vertex on a triangle's edge counts for both triangles
            if (u < -1e-3f || v < -1e-3f || w < -1e-3f)
                return null;

            return u * a.Y + v * b.Y + w * c.Y;
        }
    }
}

/// <summary>
/// Navmesh files for generated meshes, built from one of the map's own files so they carry the same types.
/// </summary>
public static class NavGenFile
{
    /// <summary>
    /// A navmesh file holding a generated mesh: a copy of a template file (one of the map's navmesh files with a query
    /// mediator) with its mesh replaced, a new face tree for the mediator, and no clearance seeding or user edge setups.
    /// Null when the template has no mediator.
    /// </summary>
    public static Havok.HavokDocument Build(Havok.HavokDocument template, IHavokNavMeshEdit generated)
    {
        var copy = NavModelEdit.Clone(template);
        var mesh = copy.ObjectsOf<IHavokNavMeshEdit>().FirstOrDefault();
        // hk2018 holds the face tree in a query mediator, hk2019 in the mesh's own cached face iterator, hk2014 (DS3,
        // BB) next to the mesh; SDT has none, but a cost graph, as DS3 and BB do
        var mediator = copy.ObjectsOf<IHavokNavMeshQueryMediator>().FirstOrDefault();
        var trees = mesh == null ? new List<IHavokNavFaceTree>() : NavigationTree.TreesOf(copy, mesh);
        var graph = NavCostGraph.Of(copy);

        if (mesh == null || (trees.Count == 0 && graph == null))
            return null;

        mesh.m_faces.Clear();
        mesh.m_faces.AddRange(generated.m_faces);
        mesh.m_edges.Clear();
        mesh.m_edges.AddRange(generated.m_edges);
        mesh.m_vertices.Clear();
        mesh.m_vertices.AddRange(generated.m_vertices);
        mesh.m_faceData.Clear();
        mesh.m_faceData.AddRange(generated.m_faceData);
        mesh.m_edgeData.Clear();
        mesh.m_edgeData.AddRange(generated.m_edgeData);
        mesh.ClearStreamingSets();
        mesh.m_faceDataStriding = generated.m_faceDataStriding;
        mesh.m_edgeDataStriding = generated.m_edgeDataStriding;
        mesh.m_aabb.Set(generated.m_aabb.m_min, generated.m_aabb.m_max);
        mesh.m_erosionRadius = generated.m_erosionRadius;
        mesh.DropClearanceSeeding();

        if (mediator != null)
            mediator.NavMesh = mesh;
        foreach (var tree in trees)
            NavigationTree.Build(mesh).WriteTo(tree);

        // The cost graph's clusters and nodes, over the new faces
        if (graph != null)
            NavCostGraph.Rebuild(graph, mesh);

        // The mesh, its tree (the mediator, or hk2014's loose tree) and its cost graph stay; user edge setups go
        if (copy.Root is IHavokRootContainerEdit root)
            root.KeepOnly(v => ReferenceEquals(v, mesh) || ReferenceEquals(v, mediator) || ReferenceEquals(v, graph)
                || (trees.Count > 0 && v?.GetType().Name == "hkcdStaticAabbTree"));

        return NavModelEdit.Clone(copy);
    }

    /// <summary>
    /// A file of the map to build generated files from: one with faces and a face tree (a query mediator in hk2018,
    /// a cached face iterator in hk2019, the tree next to the mesh in hk2014) or a cost graph (SDT).
    /// </summary>
    public static Havok.HavokDocument FindTemplate(NavigationMap map)
        => map.Files.Values.Where(f => f.FaceCount > 0 && f.Document != null)
            .Select(f => f.Document)
            .FirstOrDefault(d => d.ObjectsOf<IHavokNavMeshQueryMediator>().Any()
                || d.ObjectsOf<IHavokNavMeshEdit>().Any(m => m.CachedFaceTree != null)
                || NavigationTree.HasTree(d) || NavCostGraph.Of(d) != null);
}
