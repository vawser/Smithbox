// The flying navigation volume of Armored Core VI (hkaiNavVolume): building its cells, edges and cell tree from
// collision, in place of Havok's generator, which Smithbox doesn't have. The rules come from the game's 20 volumes and
// the calibration in Claude/Documentation/AC6 Flying Navigation Regeneration Analysis.md (sections 2, 4 and 6):
// - the volume is a grid of ~10 m voxels; a voxel is solid when a collision triangle overlaps its box enlarged by a
//   margin (0.1 voxel), and free otherwise; nothing is filled inside closed objects;
// - the cells are axis-aligned boxes that tile the free voxels exactly, grown greedily (x, then z, then y);
// - there is one edge for every ordered pair of cells sharing a face of positive area, with no flags;
// - the cell iterator holds an Aabb6BytesTree with one leaf per cell, over the volume's AABB.
// Which collision goes in (the caller's job): every placed asset's low collision, and the MSB Collision parts with
// HitFilterID 8 or 10 only.

namespace Havok.hk2019.Builders;

using System.Numerics;

/// <summary>A box of voxels, min inclusive, max exclusive, in the volume's voxel coordinates.</summary>
public readonly record struct VoxelBox(int MinX, int MinY, int MinZ, int MaxX, int MaxY, int MaxZ)
{
    public long Volume => (long)(MaxX - MinX) * (MaxY - MinY) * (MaxZ - MinZ);

    public bool Intersects(VoxelBox o) =>
        MinX < o.MaxX && o.MinX < MaxX && MinY < o.MaxY && o.MinY < MaxY && MinZ < o.MaxZ && o.MinZ < MaxZ;

    public bool Contains(int x, int y, int z) => x >= MinX && x < MaxX && y >= MinY && y < MaxY && z >= MinZ && z < MaxZ;

    public static VoxelBox Of(hkaiNavVolume.Cell c) => new(c.m_min[0], c.m_min[1], c.m_min[2], c.m_max[0], c.m_max[1], c.m_max[2]);
}

/// <summary>
/// A volume's grid: where it sits in the world and how many voxels it has on each axis. A voxel's world box is
/// Offset + v × Scale to Offset + (v + 1) × Scale.
/// </summary>
public sealed class NavVolumeGrid
{
    public Vector3 Offset { get; init; }
    public Vector3 Scale { get; init; }
    public int ResX { get; init; }
    public int ResY { get; init; }
    public int ResZ { get; init; }

    /// <summary>The W of the AABB's min and max: 0.985 / 1.015 in fitted volumes, −10 / 10 in hand-set ones.</summary>
    public float AabbMinW { get; init; } = 0.985f;
    public float AabbMaxW { get; init; } = 1.015f;

    public long VoxelCount => (long)ResX * ResY * ResZ;

    public Vector3 Max => Offset + new Vector3(ResX, ResY, ResZ) * Scale;

    /// <summary>The grid an existing volume has, so cells outside a rebuilt region keep their coordinates.</summary>
    public static NavVolumeGrid Of(hkaiNavVolume volume) => new()
    {
        Offset = new Vector3(volume.m_quantizationOffset.X, volume.m_quantizationOffset.Y, volume.m_quantizationOffset.Z),
        Scale = new Vector3(volume.m_quantizationScale.X, volume.m_quantizationScale.Y, volume.m_quantizationScale.Z),
        ResX = volume.m_res[0],
        ResY = volume.m_res[1],
        ResZ = volume.m_res[2],
        AabbMinW = volume.m_aabb.m_min.W,
        AabbMaxW = volume.m_aabb.m_max.W,
    };

    /// <summary>
    /// A grid fitted to world bounds, as Havok rounds it: the voxel count is the size over the cell width, rounded, and
    /// the scale is the size over that count, so the grid ends exactly on the bounds.
    /// </summary>
    public static NavVolumeGrid Fit(Vector3 min, Vector3 max, float cellWidth = 10f)
    {
        var size = max - min;
        int Res(float s) => Math.Clamp((int)MathF.Round(s / cellWidth), 1, ushort.MaxValue);
        int rx = Res(size.X), ry = Res(size.Y), rz = Res(size.Z);
        return new NavVolumeGrid { Offset = min, Scale = new Vector3(size.X / rx, size.Y / ry, size.Z / rz), ResX = rx, ResY = ry, ResZ = rz };
    }

    public Vector3 ToVoxel(Vector3 world) => (world - Offset) / Scale;

    public Vector3 ToWorld(int x, int y, int z) => Offset + new Vector3(x, y, z) * Scale;
}

/// <summary>
/// One bit per voxel over a box of the grid (its origin and size), with long indices: the largest AC6 grids have over
/// 2³¹ voxels. Y is the fastest axis, then Z, then X.
/// </summary>
public sealed class VoxelBits
{
    public VoxelBox Box { get; }
    private readonly ulong[] bits;
    private readonly int ny, nz;

    public VoxelBits(VoxelBox box, bool initial = false)
    {
        Box = box;
        ny = box.MaxY - box.MinY;
        nz = box.MaxZ - box.MinZ;
        bits = new ulong[(box.Volume + 63) / 64];
        if (initial)
            Array.Fill(bits, ulong.MaxValue);
    }

    private VoxelBits(VoxelBits other)
    {
        Box = other.Box; ny = other.ny; nz = other.nz;
        bits = (ulong[])other.bits.Clone();
    }

    public VoxelBits Clone() => new(this);

    private long Index(int x, int y, int z) => ((long)(x - Box.MinX) * nz + (z - Box.MinZ)) * ny + (y - Box.MinY);

    public bool this[int x, int y, int z]
    {
        get => Box.Contains(x, y, z) && (bits[Index(x, y, z) >> 6] & (1UL << (int)(Index(x, y, z) & 63))) != 0;
        set
        {
            long i = Index(x, y, z);
            if (value) bits[i >> 6] |= 1UL << (int)(i & 63);
            else bits[i >> 6] &= ~(1UL << (int)(i & 63));
        }
    }

    /// <summary>Sets a bit from any thread.</summary>
    public void SetAtomic(int x, int y, int z)
    {
        long i = Index(x, y, z);
        Interlocked.Or(ref bits[i >> 6], 1UL << (int)(i & 63));
    }

    /// <summary>Clears every bit set in another mask over the same box (this AND NOT other).</summary>
    public void Subtract(VoxelBits other)
    {
        if (other.Box != Box) throw new ArgumentException("The masks cover different boxes", nameof(other));
        for (int i = 0; i < bits.Length; i++) bits[i] &= ~other.bits[i];
    }

    public long Count()
    {
        long n = 0;
        foreach (var w in bits) n += BitOperations.PopCount(w);
        // The padding bits of the last word are never set by indexing, but are by `initial`
        long pad = (long)bits.Length * 64 - Box.Volume;
        if (pad > 0 && bits.Length > 0) n -= BitOperations.PopCount(bits[^1] >> (int)(64 - pad));
        return n;
    }
}

/// <summary>The options of a build.</summary>
public sealed class NavVolumeBuildSettings
{
    /// <summary>A voxel is solid when a triangle comes within this many voxels of it (calibrated: 0.1).</summary>
    public float TouchMargin { get; set; } = 0.1f;

    /// <summary>The longest a cell may be on any axis, in voxels (the game's largest is 1,536).</summary>
    public int MaxCellExtent { get; set; } = 1024;
}

public static class NavVolumeBuilder
{
    // ---- Solid voxels -----------------------------------------------------------------------------------------------

    /// <summary>
    /// The solid voxels of a box of the grid: every voxel whose box, enlarged by the margin, a triangle overlaps.
    /// Triangles are in world space, three points each.
    /// </summary>
    public static VoxelBits Voxelise(NavVolumeGrid grid, IReadOnlyList<Vector3> triangles, VoxelBox region, float margin)
    {
        var solid = new VoxelBits(region);
        int count = triangles.Count / 3;
        var half = new Vector3(0.5f + margin);

        Parallel.For(0, (count + 4095) / 4096, chunk =>
        {
            int end = Math.Min(count, (chunk + 1) * 4096);
            for (int t = chunk * 4096; t < end; t++)
            {
                var a = grid.ToVoxel(triangles[t * 3]);
                var b = grid.ToVoxel(triangles[t * 3 + 1]);
                var c = grid.ToVoxel(triangles[t * 3 + 2]);
                var mn = Vector3.Min(a, Vector3.Min(b, c)) - new Vector3(margin);
                var mx = Vector3.Max(a, Vector3.Max(b, c)) + new Vector3(margin);
                int x0 = Math.Max(region.MinX, (int)MathF.Floor(mn.X)), x1 = Math.Min(region.MaxX - 1, (int)MathF.Floor(mx.X));
                int y0 = Math.Max(region.MinY, (int)MathF.Floor(mn.Y)), y1 = Math.Min(region.MaxY - 1, (int)MathF.Floor(mx.Y));
                int z0 = Math.Max(region.MinZ, (int)MathF.Floor(mn.Z)), z1 = Math.Min(region.MaxZ - 1, (int)MathF.Floor(mx.Z));
                if (x0 > x1 || y0 > y1 || z0 > z1) continue;

                for (int x = x0; x <= x1; x++)
                    for (int z = z0; z <= z1; z++)
                        for (int y = y0; y <= y1; y++)
                            if (TriangleOverlapsBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), half, a, b, c))
                                solid.SetAtomic(x, y, z);
            }
        });

        return solid;
    }

    /// <summary>Akenine-Möller's triangle/box overlap test (separating axes).</summary>
    public static bool TriangleOverlapsBox(Vector3 centre, Vector3 half, Vector3 a, Vector3 b, Vector3 c)
    {
        var v0 = a - centre; var v1 = b - centre; var v2 = c - centre;
        if (MathF.Max(v0.X, MathF.Max(v1.X, v2.X)) < -half.X || MathF.Min(v0.X, MathF.Min(v1.X, v2.X)) > half.X) return false;
        if (MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)) < -half.Y || MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)) > half.Y) return false;
        if (MathF.Max(v0.Z, MathF.Max(v1.Z, v2.Z)) < -half.Z || MathF.Min(v0.Z, MathF.Min(v1.Z, v2.Z)) > half.Z) return false;

        var e0 = v1 - v0; var e1 = v2 - v1; var e2 = v0 - v2;
        if (!Separates(Vector3.Cross(e0, e1), v0, v1, v2, half)) return false;
        foreach (var e in new[] { e0, e1, e2 })
        {
            if (!Separates(new Vector3(0, -e.Z, e.Y), v0, v1, v2, half)) return false;
            if (!Separates(new Vector3(e.Z, 0, -e.X), v0, v1, v2, half)) return false;
            if (!Separates(new Vector3(-e.Y, e.X, 0), v0, v1, v2, half)) return false;
        }
        return true;
    }

    // True when the axis does NOT separate the triangle from the box
    private static bool Separates(Vector3 axis, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 half)
    {
        if (axis.LengthSquared() < 1e-12f) return true;
        float p0 = Vector3.Dot(v0, axis), p1 = Vector3.Dot(v1, axis), p2 = Vector3.Dot(v2, axis);
        float r = half.X * MathF.Abs(axis.X) + half.Y * MathF.Abs(axis.Y) + half.Z * MathF.Abs(axis.Z);
        return !(MathF.Max(p0, MathF.Max(p1, p2)) < -r || MathF.Min(p0, MathF.Min(p1, p2)) > r);
    }

    // ---- Cells ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Merges the set voxels into boxes, which it clears as it goes: from each voxel not yet taken (scanning Y layers,
    /// then Z rows, then X), a box grows along X, then Z, then Y while the next slab is all free, up to the extent cap.
    /// </summary>
    public static List<VoxelBox> Merge(VoxelBits free, int maxExtent)
    {
        var boxes = new List<VoxelBox>();
        var r = free.Box;

        for (int y = r.MinY; y < r.MaxY; y++)
            for (int z = r.MinZ; z < r.MaxZ; z++)
                for (int x = r.MinX; x < r.MaxX; x++)
                {
                    if (!free[x, y, z]) continue;

                    int x1 = x + 1;
                    while (x1 < r.MaxX && x1 - x < maxExtent && free[x1, y, z]) x1++;

                    int z1 = z + 1;
                    while (z1 < r.MaxZ && z1 - z < maxExtent && RowFree(free, x, x1, y, z1)) z1++;

                    int y1 = y + 1;
                    while (y1 < r.MaxY && y1 - y < maxExtent && LayerFree(free, x, x1, y1, z, z1)) y1++;

                    for (int xx = x; xx < x1; xx++)
                        for (int zz = z; zz < z1; zz++)
                            for (int yy = y; yy < y1; yy++)
                                free[xx, yy, zz] = false;

                    boxes.Add(new VoxelBox(x, y, z, x1, y1, z1));
                }

        return boxes;
    }

    private static bool RowFree(VoxelBits free, int x0, int x1, int y, int z)
    {
        for (int x = x0; x < x1; x++) if (!free[x, y, z]) return false;
        return true;
    }

    private static bool LayerFree(VoxelBits free, int x0, int x1, int y, int z0, int z1)
    {
        for (int x = x0; x < x1; x++) for (int z = z0; z < z1; z++) if (!free[x, y, z]) return false;
        return true;
    }

    // ---- Edges ------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Every cell's face neighbours: the cells that share a face of positive area with it, sorted by index. Found per
    /// face plane (boxes ending on a plane against boxes starting on it), with a tile hash on the plane for big planes.
    /// </summary>
    public static List<int>[] Neighbours(IReadOnlyList<VoxelBox> boxes)
    {
        var result = new List<int>[boxes.Count];
        for (int i = 0; i < boxes.Count; i++) result[i] = new List<int>();

        for (int axis = 0; axis < 3; axis++)
        {
            var ending = new Dictionary<int, List<int>>();
            var starting = new Dictionary<int, List<int>>();
            for (int i = 0; i < boxes.Count; i++)
            {
                var (lo, hi) = Range(boxes[i], axis);
                Add(ending, hi, i);
                Add(starting, lo, i);
            }

            foreach (var (plane, enders) in ending)
            {
                if (!starting.TryGetValue(plane, out var starters)) continue;
                foreach (var (a, b) in PlanePairs(boxes, axis, enders, starters))
                {
                    result[a].Add(b);
                    result[b].Add(a);
                }
            }
        }

        foreach (var list in result) list.Sort();
        return result;
    }

    private static void Add(Dictionary<int, List<int>> d, int key, int value)
    {
        if (!d.TryGetValue(key, out var l)) d[key] = l = new List<int>();
        l.Add(value);
    }

    private static (int Lo, int Hi) Range(VoxelBox b, int axis) => axis switch
    {
        0 => (b.MinX, b.MaxX),
        1 => (b.MinY, b.MaxY),
        _ => (b.MinZ, b.MaxZ),
    };

    // The two axes of a plane normal to `axis`
    private static (int U0, int U1, int V0, int V1) Rect(VoxelBox b, int axis) => axis switch
    {
        0 => (b.MinY, b.MaxY, b.MinZ, b.MaxZ),
        1 => (b.MinX, b.MaxX, b.MinZ, b.MaxZ),
        _ => (b.MinX, b.MaxX, b.MinY, b.MaxY),
    };

    private static IEnumerable<(int, int)> PlanePairs(IReadOnlyList<VoxelBox> boxes, int axis, List<int> enders, List<int> starters)
    {
        static bool Overlap((int U0, int U1, int V0, int V1) p, (int U0, int U1, int V0, int V1) q) =>
            p.U0 < q.U1 && q.U0 < p.U1 && p.V0 < q.V1 && q.V0 < p.V1;

        if ((long)enders.Count * starters.Count <= 4096)
        {
            foreach (var a in enders)
            {
                var ra = Rect(boxes[a], axis);
                foreach (var b in starters)
                    if (Overlap(ra, Rect(boxes[b], axis))) yield return (a, b);
            }
            yield break;
        }

        // Tile hash over the plane: starters by the 32×32 tiles they cover, enders look up the tiles they cover
        const int Tile = 32;
        var tiles = new Dictionary<long, List<int>>();
        foreach (var b in starters)
        {
            var r = Rect(boxes[b], axis);
            for (int tu = r.U0 / Tile; tu <= (r.U1 - 1) / Tile; tu++)
                for (int tv = r.V0 / Tile; tv <= (r.V1 - 1) / Tile; tv++)
                    Add(tiles, tu, tv, b);
        }

        var seen = new HashSet<int>();
        foreach (var a in enders)
        {
            var ra = Rect(boxes[a], axis);
            seen.Clear();
            for (int tu = ra.U0 / Tile; tu <= (ra.U1 - 1) / Tile; tu++)
                for (int tv = ra.V0 / Tile; tv <= (ra.V1 - 1) / Tile; tv++)
                {
                    if (!tiles.TryGetValue(((long)tu << 32) | (uint)tv, out var list)) continue;
                    foreach (var b in list)
                        if (seen.Add(b) && Overlap(ra, Rect(boxes[b], axis))) yield return (a, b);
                }
        }

        static void Add(Dictionary<long, List<int>> d, int tu, int tv, int v)
        {
            long k = ((long)tu << 32) | (uint)tv;
            if (!d.TryGetValue(k, out var l)) d[k] = l = new List<int>();
            l.Add(v);
        }
    }

    // ---- The volume -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Puts the cells into a volume: its cells, edges, AABB, quantisation and resolution, and its cell tree (creating
    /// the cell iterator if it has none). User edges and streaming sets are cleared, as the game's volumes have none.
    /// Cell m_data is kept from <paramref name="cellData"/> where given, else 0.
    /// </summary>
    public static void Apply(hkaiNavVolume volume, NavVolumeGrid grid, IReadOnlyList<VoxelBox> boxes, IReadOnlyList<int>? cellData = null)
    {
        var neighbours = Neighbours(boxes);

        volume.m_cells = new List<hkaiNavVolume.Cell>(boxes.Count);
        volume.m_edges = new List<hkaiNavVolume.Edge>();
        for (int i = 0; i < boxes.Count; i++)
        {
            var b = boxes[i];
            if (neighbours[i].Count > short.MaxValue)
                throw new InvalidOperationException($"Cell {i} has {neighbours[i].Count} neighbours, more than a cell can hold");

            var cell = new hkaiNavVolume.Cell
            {
                m_numEdges = (short)neighbours[i].Count,
                m_startEdgeIndex = volume.m_edges.Count,
                m_data = cellData != null && i < cellData.Count ? cellData[i] : 0,
            };
            cell.m_min[0] = (ushort)b.MinX; cell.m_min[1] = (ushort)b.MinY; cell.m_min[2] = (ushort)b.MinZ;
            cell.m_max[0] = (ushort)b.MaxX; cell.m_max[1] = (ushort)b.MaxY; cell.m_max[2] = (ushort)b.MaxZ;
            volume.m_cells.Add(cell);

            foreach (var n in neighbours[i])
                volume.m_edges.Add(new hkaiNavVolume.Edge { m_oppositeCell = (uint)n, m_flags = 0, m_userEdgeInfoIndex = 0xFFFF });
        }

        volume.m_userEdgeInfos = new List<hkaiNavVolume.UserEdgeInfo>();
        volume.m_userEdgeData = new List<int>();
        volume.m_streamingSets = new List<hkaiAnnotatedStreamingSet>();
        volume.m_userEdgeDataStriding = 1;

        var max = grid.Max;
        volume.m_aabb = new hkAabb
        {
            m_min = new Vector4(grid.Offset, grid.AabbMinW),
            m_max = new Vector4(max, grid.AabbMaxW),
        };
        volume.m_quantizationOffset = new Vector4(grid.Offset, 0);
        volume.m_quantizationScale = new Vector4(grid.Scale, 0);
        volume.m_res[0] = grid.ResX; volume.m_res[1] = grid.ResY; volume.m_res[2] = grid.ResZ;

        volume.m_cachedCellIterator ??= new hkaiNavVolumeStaticTreeCellIterator();
        volume.m_cachedCellIterator.m_tree ??= new hkcdStaticAabbTree();
        volume.m_cachedCellIterator.m_tree.m_treePtr ??= new hkcdStaticAabbTree.Impl();
        var tree = volume.m_cachedCellIterator.m_tree.m_treePtr.m_tree;
        BuildTree(tree, grid, boxes);
    }

    // ---- The cell tree ----------------------------------------------------------------------------------------------

    // The 6-byte codec (as the navmesh face trees, Smithbox's NavigationTree): per axis a byte, the high nibble h and
    // the low nibble l, with min = parent min + h² / 226 × extent and max = parent max − l² / 226 × extent. An inner
    // node has bit 0x80 of m_hiData set, its left child next and its right child ((hi & 0x7F) << 16 | lo) × 2 nodes on;
    // a leaf holds its cell index in the same 23 bits. The root is all zeros: the domain, which is the volume's AABB.
    private const float CodecScale = 226f;

    private sealed class Node
    {
        public Vector3 Min, Max;
        public int Cell = -1;
        public Node? Left, Right;
    }

    private static void BuildTree(hkcdStaticTree.Aabb6BytesTree tree, NavVolumeGrid grid, IReadOnlyList<VoxelBox> boxes)
    {
        tree.m_domain = new hkAabb { m_min = new Vector4(grid.Offset, 0), m_max = new Vector4(grid.Max, 0) };
        tree.m_nodes = new List<hkcdCompressedAabbCodecs.Aabb6BytesCodec>();
        if (boxes.Count == 0) return;
        if (boxes.Count >= 1 << 23) throw new InvalidOperationException($"{boxes.Count} cells is more than the cell tree can index");

        var bounds = boxes.Select(b => (grid.ToWorld(b.MinX, b.MinY, b.MinZ), grid.ToWorld(b.MaxX, b.MaxY, b.MaxZ))).ToArray();
        var order = Enumerable.Range(0, boxes.Count).ToArray();
        var root = Split(bounds, order, 0, order.Length);
        Encode(tree.m_nodes, root, grid.Offset, grid.Max, true);
    }

    private static Node Split((Vector3 Min, Vector3 Max)[] bounds, int[] cells, int start, int count)
    {
        var node = new Node { Min = new Vector3(float.MaxValue), Max = new Vector3(float.MinValue) };
        var cmin = new Vector3(float.MaxValue);
        var cmax = new Vector3(float.MinValue);
        for (int i = start; i < start + count; i++)
        {
            var (mn, mx) = bounds[cells[i]];
            node.Min = Vector3.Min(node.Min, mn);
            node.Max = Vector3.Max(node.Max, mx);
            var c = (mn + mx) / 2;
            cmin = Vector3.Min(cmin, c);
            cmax = Vector3.Max(cmax, c);
        }

        if (count == 1)
        {
            node.Cell = cells[start];
            return node;
        }

        // Split at the median of the longest axis of the centres
        var extent = cmax - cmin;
        int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;
        float Centre(int f) => axis switch
        {
            0 => bounds[f].Min.X + bounds[f].Max.X,
            1 => bounds[f].Min.Y + bounds[f].Max.Y,
            _ => bounds[f].Min.Z + bounds[f].Max.Z,
        };
        Array.Sort(cells, start, count, Comparer<int>.Create((a, b) => Centre(a).CompareTo(Centre(b))));

        int half = count / 2;
        node.Left = Split(bounds, cells, start, half);
        node.Right = Split(bounds, cells, start + half, count - half);
        return node;
    }

    private static byte CompressAxis(float min, float max, float parentMin, float parentMax)
    {
        float extent = parentMax - parentMin;
        if (extent <= 0) return 0;
        float norm = CodecScale / extent;
        int h = (int)MathF.Floor(MathF.Sqrt(MathF.Max((min - parentMin) * norm, 0)));
        int l = (int)MathF.Floor(MathF.Sqrt(MathF.Max((parentMax - max) * norm, 0)));
        return (byte)((Math.Min(15, h) << 4) | Math.Min(15, l));
    }

    /// <summary>A node's box from its codec bytes and its parent's box.</summary>
    public static (Vector3 Min, Vector3 Max) DecompressNode(byte[] xyz, Vector3 parentMin, Vector3 parentMax)
    {
        var extent = parentMax - parentMin;
        float Lo(int a) => (xyz[a] >> 4) * (xyz[a] >> 4) / CodecScale;
        float Hi(int a) => (xyz[a] & 0xF) * (xyz[a] & 0xF) / CodecScale;
        return (parentMin + new Vector3(Lo(0), Lo(1), Lo(2)) * extent, parentMax - new Vector3(Hi(0), Hi(1), Hi(2)) * extent);
    }

    private static void Encode(List<hkcdCompressedAabbCodecs.Aabb6BytesCodec> nodes, Node node, Vector3 parentMin, Vector3 parentMax, bool root)
    {
        int index = nodes.Count;
        var codec = new hkcdCompressedAabbCodecs.Aabb6BytesCodec();
        nodes.Add(codec);

        if (!root)
        {
            codec.m_xyz[0] = CompressAxis(node.Min.X, node.Max.X, parentMin.X, parentMax.X);
            codec.m_xyz[1] = CompressAxis(node.Min.Y, node.Max.Y, parentMin.Y, parentMax.Y);
            codec.m_xyz[2] = CompressAxis(node.Min.Z, node.Max.Z, parentMin.Z, parentMax.Z);
        }

        // Children are encoded within the box as stored, so each one still holds its cells after rounding
        var (min, max) = DecompressNode(codec.m_xyz, parentMin, parentMax);

        if (node.Cell >= 0)
        {
            codec.m_loData = (ushort)(node.Cell & 0xFFFF);
            codec.m_hiData = (byte)((node.Cell >> 16) & 0x7F);
            return;
        }

        Encode(nodes, node.Left!, min, max, false);
        int offset = (nodes.Count - index) / 2; // the left subtree has 2n − 1 nodes, so this is exact
        codec.m_loData = (ushort)(offset & 0xFFFF);
        codec.m_hiData = (byte)(((offset >> 16) & 0x7F) | 0x80);
        Encode(nodes, node.Right!, min, max, false);
    }

    // ---- Checks -----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Checks a volume against the rules the game's volumes all keep, and returns the problems found (empty when it
    /// keeps them): cells inside the grid and not overlapping, covering exactly the expected free voxels when given;
    /// every edge range contiguous in cell order; edges that are exactly the face-adjacent pairs, both ways, unflagged;
    /// and a cell tree with 2n − 1 nodes, one leaf per cell, each leaf's box holding its cell.
    /// </summary>
    public static List<string> Check(hkaiNavVolume volume, VoxelBits? expectedFree = null, int maxProblems = 20)
    {
        var problems = new List<string>();
        void Problem(string s) { if (problems.Count < maxProblems) problems.Add(s); }

        var grid = NavVolumeGrid.Of(volume);
        var boxes = volume.m_cells.Select(VoxelBox.Of).ToList();
        var all = new VoxelBox(0, 0, 0, grid.ResX, grid.ResY, grid.ResZ);

        // Cells
        for (int i = 0; i < boxes.Count; i++)
        {
            var b = boxes[i];
            if (b.MinX >= b.MaxX || b.MinY >= b.MaxY || b.MinZ >= b.MaxZ) Problem($"cell {i} is empty: {b}");
            if (b.MinX < 0 || b.MinY < 0 || b.MinZ < 0 || b.MaxX > grid.ResX || b.MaxY > grid.ResY || b.MaxZ > grid.ResZ) Problem($"cell {i} is outside the grid: {b}");
        }

        // Overlaps and coverage, over the cells' joint bounds
        if (boxes.Count > 0)
        {
            var bound = new VoxelBox(boxes.Min(b => b.MinX), boxes.Min(b => b.MinY), boxes.Min(b => b.MinZ), boxes.Max(b => b.MaxX), boxes.Max(b => b.MaxY), boxes.Max(b => b.MaxZ));
            if (expectedFree != null) bound = expectedFree.Box;
            var covered = new VoxelBits(bound);
            long overlaps = 0;
            foreach (var b in boxes)
                for (int x = b.MinX; x < b.MaxX; x++) for (int z = b.MinZ; z < b.MaxZ; z++) for (int y = b.MinY; y < b.MaxY; y++)
                {
                    if (!bound.Contains(x, y, z)) continue;
                    if (covered[x, y, z]) overlaps++; else covered[x, y, z] = true;
                }
            if (overlaps > 0) Problem($"{overlaps} voxels are in more than one cell");

            if (expectedFree != null)
            {
                long missing = 0, extra = 0;
                var r = expectedFree.Box;
                for (int x = r.MinX; x < r.MaxX; x++) for (int z = r.MinZ; z < r.MaxZ; z++) for (int y = r.MinY; y < r.MaxY; y++)
                {
                    bool f = expectedFree[x, y, z], c = covered[x, y, z];
                    if (f && !c) missing++; else if (c && !f) extra++;
                }
                if (missing > 0) Problem($"{missing} free voxels are in no cell");
                if (extra > 0) Problem($"{extra} solid voxels are in a cell");
            }
        }

        // Edges
        int expect = 0;
        for (int i = 0; i < volume.m_cells.Count; i++)
        {
            var c = volume.m_cells[i];
            if (c.m_startEdgeIndex != expect) { Problem($"cell {i}'s edges start at {c.m_startEdgeIndex}, not {expect}"); break; }
            expect = c.m_startEdgeIndex + c.m_numEdges;
        }
        if (expect != volume.m_edges.Count) Problem($"the cells' edge ranges end at {expect}, but there are {volume.m_edges.Count} edges");

        var neighbours = Neighbours(boxes);
        for (int i = 0; i < volume.m_cells.Count && problems.Count < maxProblems; i++)
        {
            var c = volume.m_cells[i];
            var have = new HashSet<int>();
            for (int e = c.m_startEdgeIndex; e < c.m_startEdgeIndex + c.m_numEdges && e < volume.m_edges.Count; e++)
            {
                var edge = volume.m_edges[e];
                if (edge.m_flags != 0) Problem($"edge {e} of cell {i} has flags {edge.m_flags}");
                if (edge.m_userEdgeInfoIndex != 0xFFFF) Problem($"edge {e} of cell {i} has user edge info {edge.m_userEdgeInfoIndex}");
                if (!have.Add((int)edge.m_oppositeCell)) Problem($"cell {i} links cell {edge.m_oppositeCell} twice");
            }
            if (!have.SetEquals(neighbours[i]))
                Problem($"cell {i} links {have.Count} cells, but shares a face with {neighbours[i].Count} (missing {neighbours[i].Except(have).Count()}, extra {have.Except(neighbours[i]).Count()})");
        }

        // Tree
        var tree = volume.m_cachedCellIterator?.m_tree?.m_treePtr?.m_tree;
        if (tree == null) Problem("the volume has no cell tree");
        else
        {
            if (tree.m_nodes.Count != Math.Max(0, 2 * boxes.Count - 1)) Problem($"the cell tree has {tree.m_nodes.Count} nodes, not {2 * boxes.Count - 1}");
            var dmin = new Vector3(tree.m_domain.m_min.X, tree.m_domain.m_min.Y, tree.m_domain.m_min.Z);
            var dmax = new Vector3(tree.m_domain.m_max.X, tree.m_domain.m_max.Y, tree.m_domain.m_max.Z);
            var leafSeen = new bool[boxes.Count];
            var stack = new Stack<(int I, Vector3 Min, Vector3 Max)>();
            if (tree.m_nodes.Count > 0) stack.Push((0, dmin, dmax));
            int leaves = 0, badLeaves = 0;
            while (stack.Count > 0)
            {
                var (i, pmin, pmax) = stack.Pop();
                if (i < 0 || i >= tree.m_nodes.Count) { Problem($"the cell tree points outside its nodes ({i})"); continue; }
                var n = tree.m_nodes[i];
                var (mn, mx) = DecompressNode(n.m_xyz, pmin, pmax);
                int data = ((n.m_hiData & 0x7F) << 16) | n.m_loData;
                if ((n.m_hiData & 0x80) != 0) { stack.Push((i + data * 2, mn, mx)); stack.Push((i + 1, mn, mx)); continue; }
                leaves++;
                if (data >= boxes.Count) { Problem($"a tree leaf names cell {data}, past the last"); continue; }
                if (leafSeen[data]) Problem($"cell {data} has more than one tree leaf");
                leafSeen[data] = true;
                var b = boxes[data];
                var cmin = grid.ToWorld(b.MinX, b.MinY, b.MinZ); var cmax = grid.ToWorld(b.MaxX, b.MaxY, b.MaxZ);
                const float Tol = 1e-2f;
                if (cmin.X < mn.X - Tol || cmin.Y < mn.Y - Tol || cmin.Z < mn.Z - Tol || cmax.X > mx.X + Tol || cmax.Y > mx.Y + Tol || cmax.Z > mx.Z + Tol) badLeaves++;
            }
            if (leaves != boxes.Count) Problem($"the cell tree has {leaves} leaves for {boxes.Count} cells");
            if (badLeaves > 0) Problem($"{badLeaves} tree leaves don't hold their cell");
        }

        return problems;
    }
}
