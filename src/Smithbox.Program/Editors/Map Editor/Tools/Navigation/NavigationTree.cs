using Havok.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The face tree of a navmesh's query mediator (hkaiStaticTreeNavMeshQueryMediator → hkcdStaticAabbTree →
// hkcdStaticTree.Aabb6BytesTree): building one for a generated navmesh, and reading one, to check a built tree against the
// game's (NavSurvey treecheck). The codec is the one the hk2014 builders use (BVH.BuildAxis6Tree):
// - each node's box is stored relative to its parent's: per axis a byte, the high nibble h and the low nibble l, with
//   min = parent min + h² / 226 × extent and max = parent max − l² / 226 × extent. The root is all zeros: the domain;
// - an inner node has bit 0x80 of m_hiData set; its left child is the next node, its right child is
//   ((m_hiData & 0x7F) << 16 | m_loData) × 2 nodes further on;
// - a leaf holds its face index in the same 23 bits.
// The tree is held as NavFaceTreeData, read from and written to a file's tree through Havok.Shared's IHavokNavFaceTree,
// so it works for every class set that has one (hk2018's query mediator, hk2019's cached face iterator).

/// <summary>A decoded node of a navmesh face tree.</summary>
public readonly record struct NavTreeNode(int Index, Vector3 Min, Vector3 Max, bool IsLeaf, int Face, int Left, int Right);

/// <summary>A face tree as stored: its domain (the root's box) and its encoded nodes.</summary>
public sealed record NavFaceTreeData(Vector4 DomainMin, Vector4 DomainMax, IReadOnlyList<HavokNavTreeNode> Nodes)
{
    /// <summary>The tree a file holds.</summary>
    public static NavFaceTreeData Of(IHavokNavFaceTree tree) => new(tree.DomainMin, tree.DomainMax, tree.GetNodes());

    /// <summary>Puts this tree into a file's tree, in place of the nodes and domain it has.</summary>
    public void WriteTo(IHavokNavFaceTree tree) => tree.SetNodes(DomainMin, DomainMax, Nodes);
}

public static class NavigationTree
{
    private const float Scale = 226f;

    private sealed class Node
    {
        public Vector3 Min, Max;
        public Node Left, Right;
        public int Face = -1;
    }

    /// <summary>
    /// The face trees of a file that index one of its meshes, each with nodes to hold: hk2018's query mediators that name
    /// the mesh, and hk2019's cached face iterator of the mesh itself.
    /// </summary>
    public static List<IHavokNavFaceTree> TreesOf(Havok.HavokDocument document, IHavokNavMeshEdit mesh)
    {
        var trees = new List<IHavokNavFaceTree>();

        foreach (var mediator in document.ObjectsOf<IHavokNavMeshQueryMediator>())
            if (ReferenceEquals(mediator.NavMesh, mesh) && mediator.FaceTree is { HasNodes: true } tree)
                trees.Add(tree);

        if (mesh.CachedFaceTree is { HasNodes: true } own && !trees.Contains(own))
            trees.Add(own);

        // hk2014 (DS3, BB): a bare AABB tree next to the file's one mesh
        if (document.Root is IHavokRootContainerEdit root && document.ObjectsOf<IHavokNavMeshEdit>().Take(2).Count() == 1)
            foreach (var loose in root.LooseFaceTrees)
                if (loose.HasNodes && !trees.Contains(loose))
                    trees.Add(loose);

        return trees;
    }

    /// <summary>Whether a file has a face tree over any of its meshes (a query mediator in hk2018).</summary>
    public static bool HasTree(Havok.HavokDocument document)
        => document.ObjectsOf<IHavokNavMeshEdit>().Any(m => TreesOf(document, m).Count > 0);

    /// <summary>The bounds of each face of a mesh, from its corners.</summary>
    public static (Vector3 Min, Vector3 Max)[] FaceBounds(IHavokNavMeshEdit mesh)
    {
        var bounds = new (Vector3, Vector3)[mesh.m_faces.Count];

        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var corners = NavigationOverlayBuilder.FaceCorners(mesh, f);
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var c in corners)
            {
                min = Vector3.Min(min, c);
                max = Vector3.Max(max, c);
            }
            bounds[f] = (min, max);
        }

        return bounds;
    }

    /// <summary>
    /// Builds the face tree of a mesh: a binary tree with one face per leaf, split at the median of the longest axis of
    /// the face centres, encoded with each box rounded outwards so it always holds its faces.
    /// </summary>
    public static NavFaceTreeData Build(IHavokNavMeshEdit mesh)
    {
        var bounds = FaceBounds(mesh);

        // An empty tree keeps the zero domain a new tree has
        if (bounds.Length == 0)
            return new NavFaceTreeData(default, default, Array.Empty<HavokNavTreeNode>());

        var faces = Enumerable.Range(0, bounds.Length).ToArray();
        var root = Split(bounds, faces, 0, faces.Length);

        var codecs = new List<Codec>();
        Encode(codecs, root, root.Min, root.Max, true);
        return new NavFaceTreeData(new Vector4(root.Min, 1), new Vector4(root.Max, 1),
            codecs.Select(c => new HavokNavTreeNode(c.Xyz[0], c.Xyz[1], c.Xyz[2], c.HiData, c.LoData)).ToList());
    }

    /// <summary>A node being encoded: its fields are filled in after its children are placed.</summary>
    private sealed class Codec
    {
        public readonly byte[] Xyz = new byte[3];
        public byte HiData;
        public ushort LoData;
    }

    private static Node Split((Vector3 Min, Vector3 Max)[] bounds, int[] faces, int start, int count)
    {
        var node = new Node { Min = new Vector3(float.MaxValue), Max = new Vector3(float.MinValue) };

        for (int i = start; i < start + count; i++)
        {
            node.Min = Vector3.Min(node.Min, bounds[faces[i]].Min);
            node.Max = Vector3.Max(node.Max, bounds[faces[i]].Max);
        }

        if (count == 1)
        {
            node.Face = faces[start];
            return node;
        }

        // The longest axis of the centres
        var cmin = new Vector3(float.MaxValue);
        var cmax = new Vector3(float.MinValue);
        for (int i = start; i < start + count; i++)
        {
            var c = (bounds[faces[i]].Min + bounds[faces[i]].Max) / 2;
            cmin = Vector3.Min(cmin, c);
            cmax = Vector3.Max(cmax, c);
        }

        var extent = cmax - cmin;
        int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;
        float Centre(int f) => axis switch
        {
            0 => bounds[f].Min.X + bounds[f].Max.X,
            1 => bounds[f].Min.Y + bounds[f].Max.Y,
            _ => bounds[f].Min.Z + bounds[f].Max.Z,
        };

        Array.Sort(faces, start, count, Comparer<int>.Create((a, b) => Centre(a).CompareTo(Centre(b))));

        int half = count / 2;
        node.Left = Split(bounds, faces, start, half);
        node.Right = Split(bounds, faces, start + half, count - half);
        return node;
    }

    /// <summary>One axis of a box within its parent's, rounded outwards: min down, max up.</summary>
    private static byte CompressAxis(float min, float max, float parentMin, float parentMax)
    {
        float extent = parentMax - parentMin;
        if (extent <= 0)
            return 0;

        float norm = Scale / extent;
        int h = (int)MathF.Floor(MathF.Sqrt(MathF.Max((min - parentMin) * norm, 0)));
        int l = (int)MathF.Floor(MathF.Sqrt(MathF.Max((parentMax - max) * norm, 0)));
        return (byte)((Math.Min(15, h) << 4) | Math.Min(15, l));
    }

    private static (Vector3 Min, Vector3 Max) Decompress(byte[] xyz, Vector3 parentMin, Vector3 parentMax)
    {
        var extent = parentMax - parentMin;
        float Lo(int axis) => (xyz[axis] >> 4) * (xyz[axis] >> 4) / Scale;
        float Hi(int axis) => (xyz[axis] & 0xF) * (xyz[axis] & 0xF) / Scale;

        var min = parentMin + new Vector3(Lo(0), Lo(1), Lo(2)) * extent;
        var max = parentMax - new Vector3(Hi(0), Hi(1), Hi(2)) * extent;
        return (min, max);
    }

    private static void Encode(List<Codec> nodes, Node node, Vector3 parentMin, Vector3 parentMax, bool root)
    {
        int index = nodes.Count;
        var codec = new Codec();
        nodes.Add(codec);

        if (!root)
        {
            codec.Xyz[0] = CompressAxis(node.Min.X, node.Max.X, parentMin.X, parentMax.X);
            codec.Xyz[1] = CompressAxis(node.Min.Y, node.Max.Y, parentMin.Y, parentMax.Y);
            codec.Xyz[2] = CompressAxis(node.Min.Z, node.Max.Z, parentMin.Z, parentMax.Z);
        }

        // The children are encoded within the box as stored, not as built
        var (min, max) = Decompress(codec.Xyz, parentMin, parentMax);

        if (node.Face >= 0)
        {
            codec.LoData = (ushort)(node.Face & 0xFFFF);
            codec.HiData = (byte)((node.Face >> 16) & 0x7F);
            return;
        }

        Encode(nodes, node.Left, min, max, false);

        int offset = (nodes.Count - index) / 2;
        codec.LoData = (ushort)(offset & 0xFFFF);
        codec.HiData = (byte)(((offset >> 16) & 0x7F) | 0x80);

        // The offset is stored halved, so the right child must sit an even number of nodes on: it does, as the left
        // subtree, a binary tree with one face per leaf, has an odd number of nodes (2n − 1)
        Encode(nodes, node.Right, min, max, false);
    }

    /// <summary>The nodes of a tree, decoded from the root (its domain).</summary>
    public static List<NavTreeNode> Decode(NavFaceTreeData tree)
    {
        var result = new List<NavTreeNode>();
        if (tree.Nodes.Count == 0)
            return result;

        var min = new Vector3(tree.DomainMin.X, tree.DomainMin.Y, tree.DomainMin.Z);
        var max = new Vector3(tree.DomainMax.X, tree.DomainMax.Y, tree.DomainMax.Z);
        var stack = new Stack<(int Index, Vector3 Min, Vector3 Max)>();
        stack.Push((0, min, max));

        while (stack.Count > 0)
        {
            var (i, pmin, pmax) = stack.Pop();
            if (i < 0 || i >= tree.Nodes.Count)
                continue;

            var codec = tree.Nodes[i];
            var (nmin, nmax) = Decompress(new[] { codec.X, codec.Y, codec.Z }, pmin, pmax);
            int data = ((codec.HiData & 0x7F) << 16) | codec.LoData;

            if ((codec.HiData & 0x80) != 0)
            {
                int right = i + data * 2;
                result.Add(new NavTreeNode(i, nmin, nmax, false, -1, i + 1, right));
                stack.Push((right, nmin, nmax));
                stack.Push((i + 1, nmin, nmax));
            }
            else
            {
                result.Add(new NavTreeNode(i, nmin, nmax, true, data, -1, -1));
            }
        }

        return result;
    }

    /// <summary>
    /// Whether a tree holds every face of a mesh once, each inside its leaf's box (within a tolerance). Returns a
    /// description of the first problem, or null.
    /// </summary>
    public static string Check(NavFaceTreeData tree, IHavokNavMeshEdit mesh, float tolerance = 1e-3f)
    {
        var bounds = FaceBounds(mesh);
        var seen = new int[bounds.Length];
        var nodes = Decode(tree);

        if (nodes.Count != tree.Nodes.Count)
            return $"{nodes.Count} of {tree.Nodes.Count} nodes reached";

        foreach (var node in nodes.Where(n => n.IsLeaf))
        {
            if (node.Face < 0 || node.Face >= bounds.Length)
                return $"leaf {node.Index} names face {node.Face} of {bounds.Length}";

            seen[node.Face]++;
            var (fmin, fmax) = bounds[node.Face];
            var t = new Vector3(tolerance);
            if (Vector3.Min(fmin, node.Min - t) != node.Min - t || Vector3.Max(fmax, node.Max + t) != node.Max + t)
                return $"face {node.Face} sticks out of its leaf {node.Index}";
        }

        int missing = seen.Count(s => s == 0), twice = seen.Count(s => s > 1);
        return missing > 0 || twice > 0 ? $"{missing} faces in no leaf, {twice} in more than one" : null;
    }
}
