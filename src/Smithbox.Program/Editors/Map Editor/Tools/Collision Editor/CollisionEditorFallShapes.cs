using Havok.hk2018;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>Which outer edges of a mesh get fall protection walls, by the world direction they face.</summary>
public enum FallProtectionDirection
{
    [Display(Name = "MAP_CollisionEditor_Fall_Direction_All")]
    All,
    [Display(Name = "MAP_CollisionEditor_Fall_Direction_North")]
    North,
    [Display(Name = "MAP_CollisionEditor_Fall_Direction_East")]
    East,
    [Display(Name = "MAP_CollisionEditor_Fall_Direction_South")]
    South,
    [Display(Name = "MAP_CollisionEditor_Fall_Direction_West")]
    West
}

/// <summary>
/// The ground the drop filter looks for past an edge: world triangles bucketed on a flat grid, so the highest one
/// under a point is found from the few in its cell.
/// </summary>
public sealed class FallGround
{
    private const float CellSize = 2f;

    private readonly List<Vector3> Corners = new();
    private readonly Dictionary<(int, int), List<int>> Cells = new();

    public int TriangleCount => Corners.Count / 3;

    /// <summary>Adds triangles (corners in threes), placed in the world by the matrix.</summary>
    public void Add(IEnumerable<List<Vector3>> triangleLists, Matrix4x4 world)
    {
        foreach (var corners in triangleLists)
        {
            for (var t = 0; t + 2 < corners.Count; t += 3)
            {
                var a = Vector3.Transform(corners[t], world);
                var b = Vector3.Transform(corners[t + 1], world);
                var c = Vector3.Transform(corners[t + 2], world);

                // Walls and other upright faces can't be stood on, so they aren't ground
                var normal = Vector3.Cross(b - a, c - a);

                if (normal.LengthSquared() < 1e-12f || MathF.Abs(Vector3.Normalize(normal).Y) < 0.1f)
                    continue;

                var index = Corners.Count / 3;
                Corners.Add(a);
                Corners.Add(b);
                Corners.Add(c);

                var min = Vector3.Min(a, Vector3.Min(b, c));
                var max = Vector3.Max(a, Vector3.Max(b, c));

                for (var x = Cell(min.X); x <= Cell(max.X); x++)
                {
                    for (var z = Cell(min.Z); z <= Cell(max.Z); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var list))
                            Cells[(x, z)] = list = new List<int>();

                        list.Add(index);
                    }
                }
            }
        }
    }

    private static int Cell(float v) => (int)MathF.Floor(v / CellSize);

    /// <summary>The height of the highest ground straight below a point (or at it), or null if there is none.</summary>
    public float? HighestBelow(Vector3 point)
    {
        if (!Cells.TryGetValue((Cell(point.X), Cell(point.Z)), out var list))
            return null;

        float? best = null;

        foreach (var t in list)
        {
            var a = Corners[t * 3];
            var b = Corners[t * 3 + 1];
            var c = Corners[t * 3 + 2];

            // Where the point lies over the triangle, flat
            var d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);

            if (MathF.Abs(d) < 1e-10f)
                continue;

            var u = ((b.Z - c.Z) * (point.X - c.X) + (c.X - b.X) * (point.Z - c.Z)) / d;
            var v = ((c.Z - a.Z) * (point.X - c.X) + (a.X - c.X) * (point.Z - c.Z)) / d;
            var w = 1 - u - v;

            if (u < -1e-5f || v < -1e-5f || w < -1e-5f)
                continue;

            var y = u * a.Y + v * b.Y + w * c.Y;

            if (y <= point.Y && (best == null || y > best.Value))
                best = y;
        }

        return best;
    }
}

/// <summary>
/// Fall protection walls for the Collision Editor: vertical walls along the outer edges of a collision mesh, built as
/// ER and NR ship them (Tools/CollisionSurvey fall, falldetail, fallrebuild and fallcompare check this against the
/// game's f files). Each run of connected edges is a body of its own, whose hknpExternMeshShape is a strip of quads:
/// a bottom vertex on the edge and a top vertex the wall height above it, under a compact simd tree.
/// </summary>
public static class CollisionEditorFallShapes
{
    /// <summary>The wall height of every wall in ER's and NR's f files.</summary>
    public const float GameWallHeight = 1.8f;

    /// <summary>
    /// The edges of a body's strip, so a long run of edges doesn't make one large body (the game's longest are around
    /// 64 triangles).
    /// </summary>
    public const int MaxSegmentsPerBody = 32;

    // Corners closer than this are the same vertex, as decoded meshes store shared vertices per section
    private const float WeldTolerance = 1e-3f;

    /// <summary>One wall body: its vertices (bottom, top, bottom, top...) and triangles, in the space of the part.</summary>
    public sealed record WallBody(List<Vector3> Vertices, List<int> Indices);

    /// <summary>
    /// What a generation found: the outer edges, those facing the direction, those of them with a drop past them
    /// (all of them without the drop filter), and the wall bodies.
    /// </summary>
    public sealed record WallResult(int OuterEdges, int FacingEdges, int KeptEdges, List<WallBody> Bodies)
    {
        public int TriangleCount => Bodies.Sum(b => b.Indices.Count / 3);
    }

    /// <summary>
    /// Keeps only the outer edges that can be fallen off: just past the edge, the highest ground under it (out of the
    /// ground's triangles) is more than the minimum drop below the edge, or there is none. An open seam between two
    /// pieces of floor has ground at the same height past it, so it gets no wall.
    /// </summary>
    public sealed record DropFilter(FallGround Ground, float MinDrop)
    {
        /// <summary>
        /// The least upward facing (the world normal's Y) of the triangle an edge belongs to: only ground that can be
        /// stood on can be fallen off, so the edges of walls, steep slopes and ceilings get no wall. Either side of a
        /// triangle counts as its top when this is negative.
        /// </summary>
        public float MinNormalY { get; init; } = WalkableNormalY;
    }

    /// <summary>The default walkable facing: slopes up to about 60 degrees.</summary>
    public const float WalkableNormalY = 0.5f;

    /// <summary>How far past an edge the ground is looked for.</summary>
    public const float ProbeDistance = 0.25f;

    /// <summary>
    /// How far above an edge the look for ground starts, so ground a little higher (a kerb, a step up) counts as
    /// ground rather than a drop.
    /// </summary>
    public const float ProbeRise = 0.5f;

    /// <summary>
    /// The walls along the outer edges of the triangles (corners in threes, in the space of the part) that face the
    /// direction in the world. Up is the world's up, and the world matrix places the part. With a drop filter, only
    /// the edges with a drop past them are kept.
    /// </summary>
    public static WallResult BuildWalls(IEnumerable<List<Vector3>> triangleLists, Matrix4x4 world,
        FallProtectionDirection direction, float height, DropFilter drop = null)
    {
        // Weld the corners of every body into shared vertices
        var positions = new List<Vector3>();
        var weld = new Dictionary<(long, long, long), int>();

        int Weld(Vector3 p)
        {
            var key = ((long)MathF.Round(p.X / WeldTolerance), (long)MathF.Round(p.Y / WeldTolerance), (long)MathF.Round(p.Z / WeldTolerance));

            if (!weld.TryGetValue(key, out var index))
            {
                index = positions.Count;
                positions.Add(p);
                weld[key] = index;
            }

            return index;
        }

        // Each undirected edge, with how many triangles use it and the first triangle's third corner
        var edges = new Dictionary<(int, int), (int Count, int A, int B, int C)>();

        foreach (var corners in triangleLists)
        {
            for (var t = 0; t + 2 < corners.Count; t += 3)
            {
                var a = Weld(corners[t]);
                var b = Weld(corners[t + 1]);
                var c = Weld(corners[t + 2]);

                if (a == b || b == c || a == c)
                    continue;

                if (Vector3.Cross(positions[b] - positions[a], positions[c] - positions[a]).LengthSquared() < 1e-12f)
                    continue;

                AddEdge(edges, a, b, c);
                AddEdge(edges, b, c, a);
                AddEdge(edges, c, a, b);
            }
        }

        // The outer edges are those of a single triangle
        var outer = edges.Values.Where(e => e.Count == 1).ToList();
        var kept = new List<(int A, int B)>();
        var facing = 0;

        foreach (var edge in outer)
        {
            var a = positions[edge.A];
            var b = positions[edge.B];
            var c = positions[edge.C];

            // A vertical edge has no length to stand a wall along
            var worldEdge = Vector3.TransformNormal(b - a, world);

            if (worldEdge.X * worldEdge.X + worldEdge.Z * worldEdge.Z < 1e-8f)
                continue;

            var outward = GetOutward(a, b, c, world);

            if (direction is not FallProtectionDirection.All && !Faces(outward, direction))
                continue;

            facing++;

            if (drop != null)
            {
                if (!IsWalkable(edge.A, edge.B, edge.C, positions, world, drop.MinNormalY))
                    continue;

                if (!HasDrop(Vector3.Transform((a + b) * 0.5f, world), outward, drop))
                    continue;
            }

            kept.Add((edge.A, edge.B));
        }

        // The world's up, in the space of the part, the wall height long
        var up = Matrix4x4.Invert(world, out var inverse)
            ? Vector3.TransformNormal(new Vector3(0, height, 0), inverse)
            : new Vector3(0, height, 0);

        var bodies = new List<WallBody>();

        foreach (var chain in GetChains(kept))
        {
            for (var start = 0; start < chain.Count - 1; start += MaxSegmentsPerBody)
            {
                var end = Math.Min(start + MaxSegmentsPerBody, chain.Count - 1);
                bodies.Add(MakeStrip(chain.GetRange(start, end - start + 1).Select(i => positions[i]).ToList(), up));
            }
        }

        return new WallResult(outer.Count, facing, kept.Count, bodies);
    }

    /// <summary>
    /// Whether the triangle of an outer edge (its corners in winding order a, b, c) faces up enough to stand on.
    /// </summary>
    private static bool IsWalkable(int a, int b, int c, List<Vector3> positions, Matrix4x4 world, float minNormalY)
    {
        var wa = Vector3.Transform(positions[a], world);
        var normal = Vector3.Cross(Vector3.Transform(positions[b], world) - wa, Vector3.Transform(positions[c], world) - wa);

        if (normal.LengthSquared() < 1e-12f)
            return false;

        var y = Vector3.Normalize(normal).Y;

        return minNormalY < 0 ? MathF.Abs(y) >= -minNormalY : y >= minNormalY;
    }

    /// <summary>
    /// Whether the ground just past an edge's middle (in the world) is more than the minimum drop below it, or
    /// missing. An edge whose facing can't be told has nowhere to look, so it keeps its wall.
    /// </summary>
    private static bool HasDrop(Vector3 middle, Vector2 outward, DropFilter drop)
    {
        if (outward.LengthSquared() < 1e-12f)
            return true;

        var step = Vector2.Normalize(outward) * ProbeDistance;
        var probe = new Vector3(middle.X + step.X, middle.Y + ProbeRise, middle.Z + step.Y);
        var ground = drop.Ground.HighestBelow(probe);

        return ground == null || ground.Value < middle.Y - drop.MinDrop;
    }

    private static void AddEdge(Dictionary<(int, int), (int Count, int A, int B, int C)> edges, int a, int b, int c)
    {
        var key = a < b ? (a, b) : (b, a);

        edges[key] = edges.TryGetValue(key, out var e) ? e with { Count = e.Count + 1 } : (1, a, b, c);
    }

    /// <summary>
    /// The world direction an outer edge faces, flat: away from its triangle. A triangle standing straight up above
    /// its edge has no inside to face away from, so its own facing is used.
    /// </summary>
    private static Vector2 GetOutward(Vector3 a, Vector3 b, Vector3 c, Matrix4x4 world)
    {
        var wa = Vector3.Transform(a, world);
        var wb = Vector3.Transform(b, world);
        var wc = Vector3.Transform(c, world);

        var edge = new Vector2(wb.X - wa.X, wb.Z - wa.Z);
        var toInside = new Vector2(wc.X - wa.X, wc.Z - wa.Z);

        // The part of the way to the third corner across the edge
        var across = toInside - edge * (Vector2.Dot(toInside, edge) / edge.LengthSquared());

        if (across.LengthSquared() > 1e-8f)
            return -across;

        var normal = Vector3.Cross(wb - wa, wc - wa);
        return new Vector2(normal.X, normal.Z);
    }

    /// <summary>
    /// Whether a flat world direction is nearest the given one: north is +Z and east is +X, as the open world's map
    /// numbers go up.
    /// </summary>
    private static bool Faces(Vector2 outward, FallProtectionDirection direction)
    {
        var alongZ = MathF.Abs(outward.Y) >= MathF.Abs(outward.X);

        return direction switch
        {
            FallProtectionDirection.North => alongZ && outward.Y > 0,
            FallProtectionDirection.South => alongZ && outward.Y < 0,
            FallProtectionDirection.East => !alongZ && outward.X > 0,
            FallProtectionDirection.West => !alongZ && outward.X < 0,
            _ => true
        };
    }

    /// <summary>
    /// Joins the edges into runs of vertices: a run carries on through a vertex with two edges, and stops at one with
    /// one edge or more than two. Loops are closed by repeating their first vertex.
    /// </summary>
    private static List<List<int>> GetChains(List<(int A, int B)> edges)
    {
        var links = new Dictionary<int, List<int>>();

        for (var e = 0; e < edges.Count; e++)
        {
            foreach (var v in new[] { edges[e].A, edges[e].B })
            {
                if (!links.TryGetValue(v, out var list))
                    links[v] = list = new List<int>();

                list.Add(e);
            }
        }

        var used = new bool[edges.Count];
        var chains = new List<List<int>>();

        void Walk(int start, int firstEdge)
        {
            var chain = new List<int> { start };
            var vertex = start;
            var edge = firstEdge;

            while (edge >= 0 && !used[edge])
            {
                used[edge] = true;
                vertex = edges[edge].A == vertex ? edges[edge].B : edges[edge].A;
                chain.Add(vertex);

                // Carries on only through a vertex that joins exactly two edges
                var next = links[vertex];
                edge = next.Count == 2 ? (next[0] == edge ? next[1] : next[0]) : -1;
            }

            chains.Add(chain);
        }

        // Open runs first, from their ends, then the loops left
        foreach (var (vertex, list) in links)
        {
            if (list.Count == 2)
                continue;

            foreach (var e in list)
            {
                if (!used[e])
                    Walk(vertex, e);
            }
        }

        for (var e = 0; e < edges.Count; e++)
        {
            if (!used[e])
                Walk(edges[e].A, e);
        }

        return chains;
    }

    /// <summary>
    /// A strip of wall quads along a run of points: vertex 2i on the point, 2i+1 above it, each quad's triangles
    /// ordered as the game's are ([2,0,1], [1,3,2] for the first).
    /// </summary>
    private static WallBody MakeStrip(List<Vector3> points, Vector3 up)
    {
        var vertices = new List<Vector3>(points.Count * 2);
        var indices = new List<int>((points.Count - 1) * 6);

        foreach (var p in points)
        {
            vertices.Add(p);
            vertices.Add(p + up);
        }

        for (var i = 0; i < points.Count - 1; i++)
        {
            int b0 = i * 2, t0 = i * 2 + 1, b1 = i * 2 + 2, t1 = i * 2 + 3;

            indices.AddRange([b1, b0, t0, t0, t1, b1]);
        }

        return new WallBody(vertices, indices);
    }

    #region Havok

    /// <summary>The motion properties ID of a body with none, as ER's and NR's fall protection bodies have.</summary>
    public const ushort NoMotionProperties = 0xFFFF;

    /// <summary>
    /// A fall protection body as the game's: its extern mesh, at the origin, unnamed, with the collision filter given
    /// (the game's are 0 or 1; ER's legacy dungeons only use 1) and motion properties (ER and NR use none, 0xFFFF;
    /// DB_NT 0).
    /// </summary>
    public static hknpPhysicsSystemData.bodyCinfoWithAttachment MakeBody(WallBody wall, uint collisionFilter,
        ushort motionPropertiesId = NoMotionProperties)
    {
        return new hknpPhysicsSystemData.bodyCinfoWithAttachment
        {
            m_shape = MakeShape(wall.Vertices, wall.Indices),
            m_flags = 0,
            m_collisionCntrl = 0,
            m_collisionFilterInfo = collisionFilter,
            m_materialId = 0,
            m_qualityId = 0,
            m_name = null,
            m_userData = 0,
            m_motionType = 0,
            m_position = Vector4.Zero,
            m_orientation = Quaternion.Identity,
            m_linearVelocity = Vector4.Zero,
            m_angularVelocity = Vector4.Zero,
            m_mass = -1f,
            m_massDistribution = null,
            m_dragProperties = null,
            m_motionPropertiesId = motionPropertiesId,
            m_desiredBodyId = new hknpBodyId { m_serialAndIndex = 0x00FFFFFF },
            m_motionId = 0x7FFFFFFF,
            m_collisionLookAheadDistance = 0f,
            m_localFrame = null,
            m_activationPriority = 0,
            m_attachedBody = -1
        };
    }

    /// <summary>
    /// An extern mesh of the triangles as the game's f files hold them: vertices with w 1, triangles without a
    /// material, a shape key bit more than the triangle count's bit length, and a compact simd tree.
    /// </summary>
    public static hknpExternMeshShape MakeShape(List<Vector3> vertices, List<int> indices)
    {
        var geometry = new hkGeometry();
        geometry.m_vertices.AddRange(vertices.Select(v => new Vector4(v, 1f)));

        for (var i = 0; i + 2 < indices.Count; i += 3)
        {
            geometry.m_triangles.Add(new hkGeometry.Triangle
            {
                m_a = indices[i],
                m_b = indices[i + 1],
                m_c = indices[i + 2],
                m_material = -1
            });
        }

        var triangleCount = geometry.m_triangles.Count;

        return new hknpExternMeshShape
        {
            m_geometry = new hknpDefaultExternMeshShapeGeometry
            {
                m_geometry = geometry,
                m_useTriangleMaterialAsShapeTag = false
            },
            m_boundingVolumeData = new hknpExternMeshShapeData { m_simdTree = BuildTree(vertices, indices) },
            m_flags = hknpShape.FlagsEnum.IS_COMPOSITE_SHAPE,
            m_type = hknpShapeType.Enum.EXTERN_MESH,
            m_dispatchType = hknpCollisionDispatchType.Enum.COMPOSITE,
            m_numShapeKeyBits = (byte)(32 - System.Numerics.BitOperations.LeadingZeroCount((uint)triangleCount) + 1),
            m_convexRadius = 0f,
            m_userData = 0,
            m_properties = null,
            m_shapeTagCodecInfo = 0xFFFFFFFF,
            m_materialTable = null
        };
    }

    /// <summary>
    /// A compact four-wide tree over the triangles, laid out as the game's: node 0 is an empty sentinel, node 1 the
    /// root (never a leaf), and the rest follow breadth first. An inner node's lanes hold its children's node indices
    /// (0 for none), a leaf's the triangle indices (0xFFFFFFFF for none), and each lane the box of what it holds.
    /// </summary>
    public static hkcdSimdTree BuildTree(List<Vector3> vertices, List<int> indices)
    {
        var triangleCount = indices.Count / 3;
        var boxes = new (Vector3 Min, Vector3 Max)[triangleCount];
        var centers = new Vector3[triangleCount];

        for (var t = 0; t < triangleCount; t++)
        {
            var a = vertices[indices[t * 3]];
            var b = vertices[indices[t * 3 + 1]];
            var c = vertices[indices[t * 3 + 2]];

            boxes[t] = (Vector3.Min(a, Vector3.Min(b, c)), Vector3.Max(a, Vector3.Max(b, c)));
            centers[t] = (boxes[t].Min + boxes[t].Max) * 0.5f;
        }

        var tree = new hkcdSimdTree { m_isCompact = true };
        tree.m_nodes.Add(EmptyNode(leaf: false));

        var pending = new Queue<(int Node, List<int> Triangles, bool Leaf)>();
        tree.m_nodes.Add(null);
        pending.Enqueue((1, Enumerable.Range(0, triangleCount).ToList(), false));

        while (pending.Count > 0)
        {
            var (index, triangles, leaf) = pending.Dequeue();
            var node = EmptyNode(leaf);

            if (leaf)
            {
                for (var lane = 0; lane < triangles.Count; lane++)
                {
                    node.m_data[lane] = (uint)triangles[lane];
                    SetLane(node, lane, boxes[triangles[lane]]);
                }
            }
            else
            {
                var groups = Split(triangles, centers);

                for (var lane = 0; lane < groups.Count; lane++)
                {
                    var child = tree.m_nodes.Count;
                    tree.m_nodes.Add(null);

                    node.m_data[lane] = (uint)child;
                    SetLane(node, lane, Union(groups[lane], boxes));
                    pending.Enqueue((child, groups[lane], groups[lane].Count <= 4));
                }
            }

            tree.m_nodes[index] = node;
        }

        return tree;
    }

    /// <summary>
    /// The children of an inner node: the triangles sorted along the longest side of their centres, cut into up to
    /// four runs that each hold whole leaves of four, as evenly as they go.
    /// </summary>
    private static List<List<int>> Split(List<int> triangles, Vector3[] centers)
    {
        if (triangles.Count == 0)
            return new List<List<int>>();

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var t in triangles)
        {
            min = Vector3.Min(min, centers[t]);
            max = Vector3.Max(max, centers[t]);
        }

        var size = max - min;
        var axis = size.X >= size.Y && size.X >= size.Z ? 0 : size.Y >= size.Z ? 1 : 2;

        var sorted = triangles.OrderBy(t => axis == 0 ? centers[t].X : axis == 1 ? centers[t].Y : centers[t].Z).ToList();

        var leaves = (sorted.Count + 3) / 4;
        var childCount = Math.Min(4, leaves);
        var groups = new List<List<int>>(childCount);
        var start = 0;

        for (var c = 0; c < childCount; c++)
        {
            var childLeaves = leaves / childCount + (c < leaves % childCount ? 1 : 0);
            var count = Math.Min(childLeaves * 4, sorted.Count - start);

            groups.Add(sorted.GetRange(start, count));
            start += count;
        }

        return groups;
    }

    private static (Vector3 Min, Vector3 Max) Union(List<int> triangles, (Vector3 Min, Vector3 Max)[] boxes)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var t in triangles)
        {
            min = Vector3.Min(min, boxes[t].Min);
            max = Vector3.Max(max, boxes[t].Max);
        }

        return (min, max);
    }

    private static hkcdSimdTree.Node EmptyNode(bool leaf)
    {
        var empty = leaf ? 0xFFFFFFFFu : 0u;
        var node = new hkcdSimdTree.Node
        {
            m_isLeaf = leaf,
            m_isActive = false,
            m_parent = 0,
            m_lx = new Vector4(float.MaxValue),
            m_hx = new Vector4(-float.MaxValue),
            m_ly = new Vector4(float.MaxValue),
            m_hy = new Vector4(-float.MaxValue),
            m_lz = new Vector4(float.MaxValue),
            m_hz = new Vector4(-float.MaxValue)
        };

        for (var i = 0; i < 4; i++)
            node.m_data[i] = empty;

        return node;
    }

    private static void SetLane(hkcdSimdTree.Node node, int lane, (Vector3 Min, Vector3 Max) box)
    {
        node.m_lx = WithLane(node.m_lx, lane, box.Min.X);
        node.m_hx = WithLane(node.m_hx, lane, box.Max.X);
        node.m_ly = WithLane(node.m_ly, lane, box.Min.Y);
        node.m_hy = WithLane(node.m_hy, lane, box.Max.Y);
        node.m_lz = WithLane(node.m_lz, lane, box.Min.Z);
        node.m_hz = WithLane(node.m_hz, lane, box.Max.Z);
    }

    private static Vector4 WithLane(Vector4 v, int lane, float value)
    {
        switch (lane)
        {
            case 0: v.X = value; break;
            case 1: v.Y = value; break;
            case 2: v.Z = value; break;
            default: v.W = value; break;
        }

        return v;
    }

    /// <summary>
    /// The physics system of an f file, whose bodies are the walls: the first of its physics scene. Null if the
    /// document isn't a Havok 2018 physics scene with one.
    /// </summary>
    public static hknpPhysicsSystemData GetSystem(Havok.HavokDocument document)
    {
        return (document?.Root as hkRootLevelContainer)?.m_namedVariants
            .Select(v => v.m_variant)
            .OfType<hknpPhysicsSceneData>()
            .FirstOrDefault()?.m_systemDatas.FirstOrDefault();
    }

    /// <summary>Whether a document can be the base of a new f file: a physics scene with a single system.</summary>
    public static bool IsTemplate(Havok.HavokDocument document)
    {
        var scene = (document?.Root as hkRootLevelContainer)?.m_namedVariants
            .Select(v => v.m_variant)
            .OfType<hknpPhysicsSceneData>()
            .FirstOrDefault();

        return scene != null && scene.m_systemDatas.Count == 1 && scene.m_systemDatas[0] != null;
    }

    #endregion
}
