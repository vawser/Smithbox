#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Havok.Builders;

/// <summary>
/// Builds the data of a FromSoftware compressed mesh (fsnpCustomParamCompressedMeshShape) from a triangle list,
/// without depending on a Havok version, so the HKX3 2018 and 2019 classes can be filled from the same build.
/// <para>
/// The layout follows what the game files hold (checked against ER map collision with Tools/CollisionSurvey):
/// <list type="bullet">
/// <item>The mesh is split into sections of at most 127 primitives and 255 vertices. Each section has a binary
/// 4-byte AABB tree over its primitives, and a binary 5-byte AABB tree runs over the sections.</item>
/// <item>A vertex used by one section is packed (11/11/10 bits over the section's codec range); a vertex used by
/// several sections is shared (21/21/22 bits over the mesh domain), so section seams match exactly.</item>
/// <item>A shape key is section &lt;&lt; 8 | primitive &lt;&lt; 1 | quad half. The bits per key is the bit length of
/// the largest key.</item>
/// <item>A compact 4-wide SIMD tree with a sentinel at node 0 and the root at node 1 holds the shape keys in its
/// leaves, with unexpanded triangle bounds.</item>
/// <item>The domains of the mesh and its sections are expanded by the convex radius, with W = 1 -/+ radius.</item>
/// <item>The FromSoftware parameters group the triangles by collision material, listing each triangle in input
/// order with group-local vertex indices, and map each input triangle to its shape key.</item>
/// </list>
/// </para>
/// Primitives are always stored as triangles; vanilla files also merge triangle pairs into quads, which is only a
/// size saving.
/// </summary>
public static class HavokCompressedMeshBuilder
{
    /// <summary>
    /// The convex radius most ER map collision uses.
    /// </summary>
    public const float DefaultConvexRadius = 0.015f;

    public const int MaxSectionPrimitives = 127;
    public const int MaxSectionVertices = 255;

    /// <summary>
    /// The largest packed vertex quantization step allowed (vanilla sections go up to about 2 cm). A section that
    /// would need a coarser step stores all of its vertices as shared vertices instead, as the game files do for
    /// large sections, rather than being split further.
    /// </summary>
    public const float MaxPackedStep = 0.02f;

    public const uint ParamVersion = 0x100000;

    public sealed class Section
    {
        /// <summary>Offset XYZ then scale XYZ of the packed vertices</summary>
        public float[] CodecParms = new float[6];
        public uint FirstPackedVertexIndex;
        public uint FirstSharedVertexIndex;
        public uint FirstPrimitiveIndex;
        public uint FirstDataRunIndex;
        public byte NumPackedVertices;
        public byte NumPrimitives;
        public ushort LeafIndex;
        public Vector3 DomainMin;
        public Vector3 DomainMax;
        public List<Node4> Nodes = new();
    }

    public struct Node4
    {
        public byte X, Y, Z;
        public byte Data;
    }

    public struct Node5
    {
        public byte X, Y, Z;
        public byte HiData, LoData;
    }

    public sealed class SimdNode
    {
        public bool IsLeaf;
        public Vector3[] Min = new Vector3[4];
        public Vector3[] Max = new Vector3[4];
        public uint[] Data = new uint[4];

        public SimdNode(bool isLeaf)
        {
            IsLeaf = isLeaf;
            Array.Fill(Min, new Vector3(float.MaxValue));
            Array.Fill(Max, new Vector3(-float.MaxValue));
        }
    }

    public sealed class MaterialGroup
    {
        public uint MaterialId;
        public int VertexCount;
    }

    public sealed class TriangleParam
    {
        public uint MaterialGroupIndex;
        public uint TriangleIndex;
        public uint VertexA, VertexB, VertexC;
    }

    public sealed class Result
    {
        public float ConvexRadius;
        public Vector3 DomainMin;
        public Vector3 DomainMax;

        public List<Section> Sections = new();
        /// <summary>Four vertex indices per primitive; a triangle repeats its third index</summary>
        public List<byte[]> Primitives = new();
        public List<uint> PackedVertices = new();
        public List<ulong> SharedVertices = new();
        public List<ushort> SharedVerticesIndex = new();
        public List<Node5> TreeNodes = new();
        public List<SimdNode> SimdNodes = new();

        public int NumPrimitiveKeys;
        public int BitsPerKey;
        public uint MaxKeyValue;

        /// <summary>The shape key of each input triangle, not shifted</summary>
        public uint[] TriangleKeys;

        public List<MaterialGroup> MaterialGroups = new();
        public List<TriangleParam> TriangleParams = new();

        /// <summary>The shape key of each input triangle as the FromSoftware parameters store it: top-aligned, with
        /// the unused low bits set</summary>
        public uint StoredKey(int triangle)
        {
            int shift = 32 - BitsPerKey;
            return (TriangleKeys[triangle] << shift) | ((1u << shift) - 1);
        }
    }

    private sealed class BuildNode
    {
        public BuildNode Left, Right;
        public List<int> Triangles;
        public Vector3 Min, Max;
        public bool IsLeaf => Left == null;
    }

    /// <summary>
    /// Builds the compressed mesh. <paramref name="triangleMaterials"/> holds a collision material ID per triangle,
    /// or is null to give every triangle <paramref name="defaultMaterial"/>.
    /// Throws if the mesh has no triangles or more shared vertices than a compressed mesh can index.
    /// </summary>
    public static Result Build(
        IReadOnlyList<Vector3> vertices,
        IReadOnlyList<int> indices,
        IReadOnlyList<uint> triangleMaterials = null,
        uint defaultMaterial = 0,
        float convexRadius = DefaultConvexRadius)
    {
        if (indices.Count % 3 != 0)
            throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(indices));

        int triangleCount = indices.Count / 3;
        if (triangleCount == 0)
            throw new ArgumentException("A compressed mesh needs at least one triangle.", nameof(indices));

        var result = new Result { ConvexRadius = convexRadius };
        var radius = new Vector3(convexRadius);

        // The mesh domain, which the shared vertices are quantized over
        var rawMin = new Vector3(float.MaxValue);
        var rawMax = new Vector3(-float.MaxValue);
        for (int i = 0; i < indices.Count; i++)
        {
            rawMin = Vector3.Min(rawMin, vertices[indices[i]]);
            rawMax = Vector3.Max(rawMax, vertices[indices[i]]);
        }
        result.DomainMin = rawMin - radius;
        result.DomainMax = rawMax + radius;

        // Split the triangles into sections, keeping the splits as the tree over the sections
        var rawBounds = new (Vector3 Min, Vector3 Max)[triangleCount];
        var centroids = new Vector3[triangleCount];
        for (int t = 0; t < triangleCount; t++)
        {
            var (a, b, c) = Corners(vertices, indices, t);
            rawBounds[t] = (Vector3.Min(Vector3.Min(a, b), c), Vector3.Max(Vector3.Max(a, b), c));
            centroids[t] = (a + b + c) / 3f;
        }

        var sectionRoot = SplitSections(Enumerable.Range(0, triangleCount).ToList(), indices, rawBounds, centroids);

        var sectionLeaves = new List<BuildNode>();
        CollectLeaves(sectionRoot, sectionLeaves);

        // A vertex used by more than one section is shared, as is every vertex of a section too large to pack precisely
        var sectionsOfVertex = new Dictionary<int, int>();
        var sectionVertices = new List<SortedSet<int>>();
        var sectionPackable = new List<bool>();
        foreach (var leaf in sectionLeaves)
        {
            var used = new SortedSet<int>();
            foreach (var t in leaf.Triangles)
            {
                used.Add(indices[t * 3]);
                used.Add(indices[t * 3 + 1]);
                used.Add(indices[t * 3 + 2]);
            }
            sectionVertices.Add(used);

            foreach (var v in used)
                sectionsOfVertex[v] = sectionsOfVertex.GetValueOrDefault(v) + 1;

            var extent = leaf.Max - leaf.Min;
            sectionPackable.Add(extent.X / 2047f <= MaxPackedStep
                && extent.Y / 2047f <= MaxPackedStep
                && extent.Z / 1023f <= MaxPackedStep);
        }

        var sharedSlot = new Dictionary<int, ushort>();
        for (int s = 0; s < sectionLeaves.Count; s++)
        {
            foreach (var v in sectionVertices[s])
            {
                if ((sectionsOfVertex[v] > 1 || !sectionPackable[s]) && !sharedSlot.ContainsKey(v))
                {
                    if (result.SharedVertices.Count > ushort.MaxValue)
                        throw new InvalidOperationException("The mesh has too many vertices shared between sections for a compressed mesh.");

                    sharedSlot[v] = (ushort)result.SharedVertices.Count;
                    result.SharedVertices.Add(CompressSharedVertex(vertices[v], result.DomainMin, result.DomainMax));
                }
            }
        }

        // Fill the sections. Everything after quantization uses the decoded vertices, so the trees bound what the
        // game will read.
        var triangleKeys = new uint[triangleCount];
        var decodedBounds = new (Vector3 Min, Vector3 Max)[triangleCount];

        for (int s = 0; s < sectionLeaves.Count; s++)
        {
            var leaf = sectionLeaves[s];
            var section = new Section
            {
                FirstPackedVertexIndex = (uint)result.PackedVertices.Count,
                FirstSharedVertexIndex = (uint)result.SharedVerticesIndex.Count,
                FirstPrimitiveIndex = (uint)result.Primitives.Count,
                FirstDataRunIndex = (uint)s
            };

            var packed = sectionVertices[s].Where(v => !sharedSlot.ContainsKey(v)).ToList();
            var shared = sectionVertices[s].Where(v => sharedSlot.ContainsKey(v)).ToList();

            // The codec covers the packed vertices only
            var packMin = new Vector3(float.MaxValue);
            var packMax = new Vector3(-float.MaxValue);
            foreach (var v in packed)
            {
                packMin = Vector3.Min(packMin, vertices[v]);
                packMax = Vector3.Max(packMax, vertices[v]);
            }
            if (packed.Count == 0)
            {
                packMin = leaf.Min;
                packMax = leaf.Max;
            }

            var packExtent = Vector3.Max(packMax - packMin, new Vector3(1e-4f));
            var scale = packExtent / new Vector3(2047f, 2047f, 1023f);
            section.CodecParms = [packMin.X, packMin.Y, packMin.Z, scale.X, scale.Y, scale.Z];

            var localIndex = new Dictionary<int, byte>();
            var decoded = new Dictionary<int, Vector3>();

            foreach (var v in packed)
            {
                localIndex[v] = (byte)localIndex.Count;
                var compressed = CompressPackedVertex(vertices[v], scale, packMin);
                result.PackedVertices.Add(compressed);
                decoded[v] = DecompressPackedVertex(compressed, scale, packMin);
            }
            foreach (var v in shared)
            {
                localIndex[v] = (byte)localIndex.Count;
                result.SharedVerticesIndex.Add(sharedSlot[v]);
                decoded[v] = DecompressSharedVertex(result.SharedVertices[sharedSlot[v]], result.DomainMin, result.DomainMax);
            }

            section.NumPackedVertices = (byte)packed.Count;

            foreach (var t in leaf.Triangles)
            {
                var a = decoded[indices[t * 3]];
                var b = decoded[indices[t * 3 + 1]];
                var c = decoded[indices[t * 3 + 2]];
                decodedBounds[t] = (Vector3.Min(Vector3.Min(a, b), c), Vector3.Max(Vector3.Max(a, b), c));
            }

            // The section's own tree, over single primitives, ordered so the primitives are listed in tree order
            var sectionTreeRoot = SplitPrimitives(leaf.Triangles, decodedBounds, centroids, radius);

            var orderedTriangles = new List<int>();
            CollectTriangles(sectionTreeRoot, orderedTriangles);

            for (int p = 0; p < orderedTriangles.Count; p++)
            {
                var t = orderedTriangles[p];
                var i0 = localIndex[indices[t * 3]];
                var i1 = localIndex[indices[t * 3 + 1]];
                var i2 = localIndex[indices[t * 3 + 2]];
                result.Primitives.Add([i0, i1, i2, i2]);
                triangleKeys[t] = ((uint)s << 8) | ((uint)p << 1);
            }

            section.NumPrimitives = (byte)orderedTriangles.Count;

            // Bound the section in the mesh domain, so it is inside the root of the tree over the sections
            section.DomainMin = Vector3.Max(sectionTreeRoot.Min, result.DomainMin);
            section.DomainMax = Vector3.Min(sectionTreeRoot.Max, result.DomainMax);
            leaf.Min = section.DomainMin;
            leaf.Max = section.DomainMax;

            var primitiveOfTriangle = new Dictionary<int, int>();
            for (int p = 0; p < orderedTriangles.Count; p++)
                primitiveOfTriangle[orderedTriangles[p]] = p;

            BuildTree4(sectionTreeRoot, section.DomainMin, section.DomainMax, section.Nodes, primitiveOfTriangle);

            result.Sections.Add(section);
        }

        // The tree over the sections, with each section remembering its leaf
        RefitBounds(sectionRoot);
        var sectionOfLeaf = new Dictionary<BuildNode, int>();
        for (int s = 0; s < sectionLeaves.Count; s++)
            sectionOfLeaf[sectionLeaves[s]] = s;

        BuildTree5(sectionRoot, result.DomainMin, result.DomainMax, result.TreeNodes, sectionOfLeaf, result.Sections);

        // Keys
        result.TriangleKeys = triangleKeys;
        result.NumPrimitiveKeys = triangleCount;
        result.MaxKeyValue = triangleKeys.Max();
        result.BitsPerKey = Math.Max(1, 32 - BitOperations.LeadingZeroCount(result.MaxKeyValue));

        BuildSimdTree(result, triangleKeys, decodedBounds, centroids);

        BuildParams(result, indices, triangleMaterials, defaultMaterial);

        return result;
    }

    private static (Vector3 A, Vector3 B, Vector3 C) Corners(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, int t)
    {
        return (vertices[indices[t * 3]], vertices[indices[t * 3 + 1]], vertices[indices[t * 3 + 2]]);
    }

    #region Sections

    private static BuildNode SplitSections(
        List<int> triangles,
        IReadOnlyList<int> indices,
        (Vector3 Min, Vector3 Max)[] bounds,
        Vector3[] centroids)
    {
        var node = new BuildNode { Triangles = triangles };
        (node.Min, node.Max) = Union(triangles, bounds);

        var uniqueVertices = new HashSet<int>();
        foreach (var t in triangles)
        {
            uniqueVertices.Add(indices[t * 3]);
            uniqueVertices.Add(indices[t * 3 + 1]);
            uniqueVertices.Add(indices[t * 3 + 2]);
        }

        if (triangles.Count <= MaxSectionPrimitives && uniqueVertices.Count <= MaxSectionVertices)
            return node;

        var (left, right) = MedianSplit(triangles, centroids);
        node.Left = SplitSections(left, indices, bounds, centroids);
        node.Right = SplitSections(right, indices, bounds, centroids);
        node.Triangles = null;

        return node;
    }

    private static BuildNode SplitPrimitives(
        List<int> triangles,
        (Vector3 Min, Vector3 Max)[] bounds,
        Vector3[] centroids,
        Vector3 radius)
    {
        var node = new BuildNode { Triangles = triangles };
        var (min, max) = Union(triangles, bounds);
        node.Min = min - radius;
        node.Max = max + radius;

        if (triangles.Count == 1)
            return node;

        var (left, right) = MedianSplit(triangles, centroids);
        node.Left = SplitPrimitives(left, bounds, centroids, radius);
        node.Right = SplitPrimitives(right, bounds, centroids, radius);
        node.Triangles = null;

        return node;
    }

    /// <summary>
    /// Splits along the longest axis of the centroids, at the median
    /// </summary>
    private static (List<int> Left, List<int> Right) MedianSplit(List<int> triangles, Vector3[] centroids)
    {
        var cMin = new Vector3(float.MaxValue);
        var cMax = new Vector3(-float.MaxValue);
        foreach (var t in triangles)
        {
            cMin = Vector3.Min(cMin, centroids[t]);
            cMax = Vector3.Max(cMax, centroids[t]);
        }

        var extent = cMax - cMin;
        int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;

        var sorted = triangles.OrderBy(t => axis == 0 ? centroids[t].X : axis == 1 ? centroids[t].Y : centroids[t].Z)
            .ThenBy(t => t)
            .ToList();

        int half = sorted.Count / 2;
        return (sorted.GetRange(0, half), sorted.GetRange(half, sorted.Count - half));
    }

    private static (Vector3 Min, Vector3 Max) Union(List<int> triangles, (Vector3 Min, Vector3 Max)[] bounds)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(-float.MaxValue);
        foreach (var t in triangles)
        {
            min = Vector3.Min(min, bounds[t].Min);
            max = Vector3.Max(max, bounds[t].Max);
        }
        return (min, max);
    }

    private static void CollectLeaves(BuildNode node, List<BuildNode> leaves)
    {
        if (node.IsLeaf)
        {
            leaves.Add(node);
            return;
        }

        CollectLeaves(node.Left, leaves);
        CollectLeaves(node.Right, leaves);
    }

    private static void CollectTriangles(BuildNode node, List<int> triangles)
    {
        if (node.IsLeaf)
        {
            triangles.Add(node.Triangles[0]);
            return;
        }

        CollectTriangles(node.Left, triangles);
        CollectTriangles(node.Right, triangles);
    }

    private static void RefitBounds(BuildNode node)
    {
        if (node.IsLeaf)
            return;

        RefitBounds(node.Left);
        RefitBounds(node.Right);
        node.Min = Vector3.Min(node.Left.Min, node.Right.Min);
        node.Max = Vector3.Max(node.Left.Max, node.Right.Max);
    }

    #endregion

    #region AABB trees

    /// <summary>
    /// Compresses one axis of a child box against its parent's: the distance of each side from the parent's side is
    /// stored as the square root of a fraction of 226, in 4 bits, rounded so the box only grows.
    /// </summary>
    public static byte CompressAxis(float min, float max, float parentMin, float parentMax)
    {
        float extent = parentMax - parentMin;
        if (extent <= 0)
            return 0;

        float s = 226f / extent;
        int lo = (int)MathF.Floor(MathF.Sqrt(MathF.Max((min - parentMin) * s, 0)));
        int hi = (int)MathF.Floor(MathF.Sqrt(MathF.Max((parentMax - max) * s, 0)));
        return (byte)((Math.Min(lo, 15) << 4) | Math.Min(hi, 15));
    }

    public static (float Min, float Max) DecompressAxis(byte value, float parentMin, float parentMax)
    {
        float extent = parentMax - parentMin;
        float lo = value >> 4;
        float hi = value & 0xF;
        return (parentMin + lo * lo * (1f / 226f) * extent, parentMax - hi * hi * (1f / 226f) * extent);
    }

    private static (byte X, byte Y, byte Z, Vector3 Min, Vector3 Max) CompressBox(
        Vector3 min, Vector3 max, Vector3 parentMin, Vector3 parentMax)
    {
        var x = CompressAxis(min.X, max.X, parentMin.X, parentMax.X);
        var y = CompressAxis(min.Y, max.Y, parentMin.Y, parentMax.Y);
        var z = CompressAxis(min.Z, max.Z, parentMin.Z, parentMax.Z);

        var (minX, maxX) = DecompressAxis(x, parentMin.X, parentMax.X);
        var (minY, maxY) = DecompressAxis(y, parentMin.Y, parentMax.Y);
        var (minZ, maxZ) = DecompressAxis(z, parentMin.Z, parentMax.Z);

        return (x, y, z, new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// A section's tree. The left child follows its parent; an internal node stores the offset to its right child
    /// with the low bit set, a leaf stores its primitive index times two. The root stores no box, it is the section
    /// domain.
    /// </summary>
    private static void BuildTree4(BuildNode root, Vector3 domainMin, Vector3 domainMax, List<Node4> nodes,
        Dictionary<int, int> primitiveOfTriangle)
    {
        void Add(BuildNode node, Vector3 parentMin, Vector3 parentMax, bool isRoot)
        {
            int index = nodes.Count;
            nodes.Add(default);

            var entry = new Node4();
            Vector3 min = parentMin, max = parentMax;

            if (!isRoot)
            {
                (entry.X, entry.Y, entry.Z, min, max) = CompressBox(node.Min, node.Max, parentMin, parentMax);
            }

            if (node.IsLeaf)
            {
                entry.Data = (byte)(primitiveOfTriangle[node.Triangles[0]] * 2);
                nodes[index] = entry;
                return;
            }

            Add(node.Left, min, max, false);

            int rightOffset = nodes.Count - index;
            entry.Data = (byte)(rightOffset | 1);
            nodes[index] = entry;

            Add(node.Right, min, max, false);
        }

        Add(root, domainMin, domainMax, true);
    }

    /// <summary>
    /// The tree over the sections. An internal node sets the high bit and stores half the offset to its right child,
    /// a leaf stores its section index.
    /// </summary>
    private static void BuildTree5(BuildNode root, Vector3 domainMin, Vector3 domainMax, List<Node5> nodes,
        Dictionary<BuildNode, int> sectionOfLeaf, List<Section> sections)
    {
        void Add(BuildNode node, Vector3 parentMin, Vector3 parentMax, bool isRoot)
        {
            int index = nodes.Count;
            nodes.Add(default);

            var entry = new Node5();
            Vector3 min = parentMin, max = parentMax;

            if (!isRoot)
            {
                (entry.X, entry.Y, entry.Z, min, max) = CompressBox(node.Min, node.Max, parentMin, parentMax);
            }

            if (node.IsLeaf)
            {
                int section = sectionOfLeaf[node];
                entry.HiData = (byte)((section >> 8) & 0x7F);
                entry.LoData = (byte)(section & 0xFF);
                nodes[index] = entry;
                sections[section].LeafIndex = (ushort)index;
                return;
            }

            Add(node.Left, min, max, false);

            int half = (nodes.Count - index) / 2;
            entry.HiData = (byte)(((half >> 8) & 0x7F) | 0x80);
            entry.LoData = (byte)(half & 0xFF);
            nodes[index] = entry;

            Add(node.Right, min, max, false);
        }

        Add(root, domainMin, domainMax, true);
    }

    #endregion

    #region SIMD tree

    private static void BuildSimdTree(Result result, uint[] keys, (Vector3 Min, Vector3 Max)[] bounds, Vector3[] centroids)
    {
        var allTriangles = Enumerable.Range(0, keys.Length).ToList();
        var root = BuildSimdNode(allTriangles, bounds, centroids);

        // The root is always an internal node, even when one leaf holds every triangle
        if (root.IsLeaf)
        {
            root = new SimdBuildNode
            {
                IsLeaf = false,
                Children = [root],
                Bounds = root.Bounds
            };
        }

        var nodes = result.SimdNodes;
        nodes.Add(new SimdNode(false)); // sentinel
        nodes.Add(null); // root

        var queue = new Queue<(SimdBuildNode Node, int Slot)>();
        queue.Enqueue((root, 1));

        while (queue.Count > 0)
        {
            var (buildNode, slot) = queue.Dequeue();
            var node = new SimdNode(buildNode.IsLeaf);

            if (buildNode.IsLeaf)
            {
                for (int lane = 0; lane < buildNode.TriangleIndices.Count; lane++)
                {
                    int t = buildNode.TriangleIndices[lane];
                    node.Min[lane] = bounds[t].Min;
                    node.Max[lane] = bounds[t].Max;
                    node.Data[lane] = keys[t];
                }
            }
            else
            {
                for (int lane = 0; lane < buildNode.Children.Count; lane++)
                {
                    var child = buildNode.Children[lane];
                    int childSlot = nodes.Count;
                    nodes.Add(null);

                    node.Min[lane] = child.Bounds.Min;
                    node.Max[lane] = child.Bounds.Max;
                    node.Data[lane] = (uint)childSlot;

                    queue.Enqueue((child, childSlot));
                }
            }

            nodes[slot] = node;
        }
    }

    private sealed class SimdBuildNode
    {
        public bool IsLeaf;
        public List<int> TriangleIndices;
        public List<SimdBuildNode> Children;
        public (Vector3 Min, Vector3 Max) Bounds;
    }

    /// <summary>
    /// Splits into up to four children along the longest centroid axis. Each child but the last takes a whole number
    /// of full subtrees (a power of four triangles), so nearly every leaf holds four triangles, as in the game files.
    /// </summary>
    private static SimdBuildNode BuildSimdNode(List<int> triangles, (Vector3 Min, Vector3 Max)[] bounds, Vector3[] centroids)
    {
        var node = new SimdBuildNode { Bounds = Union(triangles, bounds) };

        if (triangles.Count <= 4)
        {
            node.IsLeaf = true;
            node.TriangleIndices = triangles;
            return node;
        }

        // The smallest subtree capacity that fits everything into four children
        long capacity = 4;
        while (capacity * 4 < triangles.Count)
            capacity *= 4;

        var cMin = new Vector3(float.MaxValue);
        var cMax = new Vector3(-float.MaxValue);
        foreach (var t in triangles)
        {
            cMin = Vector3.Min(cMin, centroids[t]);
            cMax = Vector3.Max(cMax, centroids[t]);
        }
        var extent = cMax - cMin;
        int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;

        var sorted = triangles.OrderBy(t => axis == 0 ? centroids[t].X : axis == 1 ? centroids[t].Y : centroids[t].Z)
            .ThenBy(t => t)
            .ToList();

        node.Children = new List<SimdBuildNode>(4);
        for (int start = 0; start < sorted.Count; start += (int)capacity)
        {
            int count = (int)Math.Min(capacity, sorted.Count - start);
            node.Children.Add(BuildSimdNode(sorted.GetRange(start, count), bounds, centroids));
        }

        return node;
    }

    #endregion

    #region FromSoftware parameters

    private static void BuildParams(Result result, IReadOnlyList<int> indices, IReadOnlyList<uint> triangleMaterials, uint defaultMaterial)
    {
        var groupOfMaterial = new Dictionary<uint, int>();
        var localVertices = new List<Dictionary<int, uint>>();
        var groupTriangleCount = new List<uint>();

        int triangleCount = indices.Count / 3;
        for (int t = 0; t < triangleCount; t++)
        {
            uint material = triangleMaterials != null && t < triangleMaterials.Count ? triangleMaterials[t] : defaultMaterial;

            if (!groupOfMaterial.TryGetValue(material, out var group))
            {
                group = result.MaterialGroups.Count;
                groupOfMaterial[material] = group;
                result.MaterialGroups.Add(new MaterialGroup { MaterialId = material });
                localVertices.Add(new Dictionary<int, uint>());
                groupTriangleCount.Add(0);
            }

            // The game files number a triangle's new vertices from its last corner to its first
            var local = localVertices[group];
            uint Local(int vertex)
            {
                if (!local.TryGetValue(vertex, out var index))
                {
                    index = (uint)local.Count;
                    local[vertex] = index;
                }
                return index;
            }

            var c = Local(indices[t * 3 + 2]);
            var b = Local(indices[t * 3 + 1]);
            var a = Local(indices[t * 3]);

            result.TriangleParams.Add(new TriangleParam
            {
                MaterialGroupIndex = (uint)group,
                TriangleIndex = groupTriangleCount[group]++,
                VertexA = a,
                VertexB = b,
                VertexC = c
            });
        }

        for (int g = 0; g < result.MaterialGroups.Count; g++)
            result.MaterialGroups[g].VertexCount = localVertices[g].Count;
    }

    #endregion

    #region Vertex codecs

    public static ulong CompressSharedVertex(Vector3 v, Vector3 min, Vector3 max)
    {
        ulong x = Quantize(v.X, min.X, max.X, (1 << 21) - 1);
        ulong y = Quantize(v.Y, min.Y, max.Y, (1 << 21) - 1);
        ulong z = Quantize(v.Z, min.Z, max.Z, (1 << 22) - 1);
        return x | (y << 21) | (z << 42);
    }

    public static Vector3 DecompressSharedVertex(ulong v, Vector3 min, Vector3 max)
    {
        var scale = (max - min) / new Vector3((1 << 21) - 1, (1 << 21) - 1, (1 << 22) - 1);
        return new Vector3(
            (v & 0x1FFFFF) * scale.X + min.X,
            ((v >> 21) & 0x1FFFFF) * scale.Y + min.Y,
            ((v >> 42) & 0x3FFFFF) * scale.Z + min.Z);
    }

    public static uint CompressPackedVertex(Vector3 v, Vector3 scale, Vector3 offset)
    {
        uint x = (uint)QuantizeStep(v.X, offset.X, scale.X, 0x7FF);
        uint y = (uint)QuantizeStep(v.Y, offset.Y, scale.Y, 0x7FF);
        uint z = (uint)QuantizeStep(v.Z, offset.Z, scale.Z, 0x3FF);
        return x | (y << 11) | (z << 22);
    }

    public static Vector3 DecompressPackedVertex(uint v, Vector3 scale, Vector3 offset)
    {
        return new Vector3(
            (v & 0x7FF) * scale.X + offset.X,
            ((v >> 11) & 0x7FF) * scale.Y + offset.Y,
            ((v >> 22) & 0x3FF) * scale.Z + offset.Z);
    }

    private static ulong Quantize(float value, float min, float max, ulong maxValue)
    {
        float extent = max - min;
        if (extent <= 0)
            return 0;

        return QuantizeStep(value, min, extent / maxValue, maxValue);
    }

    private static ulong QuantizeStep(float value, float offset, float step, ulong maxValue)
    {
        if (step <= 0)
            return 0;

        var q = MathF.Round((value - offset) / step);
        return (ulong)Math.Clamp(q, 0, maxValue);
    }

    #endregion
}
