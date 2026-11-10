// The editable navmesh views of Havok.Shared/NavMeshEdit.cs, for every class set with navmeshes: one source compiled into
// Havok.hk2014 (HK2014), Havok.hk2016 (HK2016), Havok.hk2018 (HK2018) and Havok.hk2019 (HK2019). The four hkaiNavMesh,
// Face, Edge, hkAabb, hkRootLevelContainer and hkaiUserEdgeSetupArray classes have the same fields; they differ only where
// marked:
// - the clearance cache and its seeding: hk2016 and later (hk2014 has none);
// - the face tree: hkcdStaticAabbTree over an Aabb6BytesTree, held by hk2018's query mediator and by hk2019's cached
//   face iterator (hk2014's tree storage differs and has no view yet; hk2016's SDT files have no tree).
// The classes are generated partial for this (the generator's partial list, with nested classes as Outer.Name).
#if HK2014
namespace Havok.hk2014;
#elif HK2016
namespace Havok.hk2016;
#elif HK2018
namespace Havok.hk2018;
#elif HK2019
namespace Havok.hk2019;
#endif

using System.Numerics;
using Havok.Shared;

public partial class hkaiNavMesh : IHavokNavMeshEdit
{
    private HavokListView<Face, IHavokNavFace>? _facesView;

    private HavokListView<Edge, IHavokNavEdge>? _edgesView;

    IHavokList<IHavokNavFace> IHavokNavMeshEdit.m_faces => _facesView ??= new HavokListView<Face, IHavokNavFace>(() => m_faces);

    IHavokList<IHavokNavEdge> IHavokNavMeshEdit.m_edges => _edgesView ??= new HavokListView<Edge, IHavokNavEdge>(() => m_edges);

    List<Vector4> IHavokNavMeshEdit.m_vertices { get => m_vertices; set => m_vertices = value; }

    List<int> IHavokNavMeshEdit.m_faceData { get => m_faceData; set => m_faceData = value; }

    List<int> IHavokNavMeshEdit.m_edgeData { get => m_edgeData; set => m_edgeData = value; }

    int IHavokNavMeshEdit.m_faceDataStriding { get => m_faceDataStriding; set => m_faceDataStriding = value; }

    int IHavokNavMeshEdit.m_edgeDataStriding { get => m_edgeDataStriding; set => m_edgeDataStriding = value; }

    byte IHavokNavMeshEdit.m_flags { get => m_flags; set => m_flags = value; }

    float IHavokNavMeshEdit.m_erosionRadius { get => m_erosionRadius; set => m_erosionRadius = value; }

    ulong IHavokNavMeshEdit.m_userData { get => m_userData; set => m_userData = value; }

    IHavokAabb IHavokNavMeshEdit.m_aabb => m_aabb ??= new hkAabb();

    void IHavokNavMeshEdit.ClearStreamingSets() => m_streamingSets.Clear();

#if HK2014
    bool IHavokNavMeshEdit.HasClearanceSeeding => false;

    IReadOnlyList<IHavokNavClearanceCache> IHavokNavMeshEdit.ClearanceCaches => Array.Empty<IHavokNavClearanceCache>();

    void IHavokNavMeshEdit.DropClearanceSeeding() { }
#else
    bool IHavokNavMeshEdit.HasClearanceSeeding => m_clearanceCacheSeedingDataSet != null;

    IReadOnlyList<IHavokNavClearanceCache> IHavokNavMeshEdit.ClearanceCaches =>
        m_clearanceCacheSeedingDataSet?.m_cacheDatas.Select(d => d.m_initialCache).Where(c => c != null).Distinct()
            .Cast<IHavokNavClearanceCache>().ToList()
        ?? new List<IHavokNavClearanceCache>();

    void IHavokNavMeshEdit.DropClearanceSeeding() => m_clearanceCacheSeedingDataSet = null;
#endif

#if HK2019
    IHavokNavFaceTree? IHavokNavMeshEdit.CachedFaceTree => m_cachedFaceIterator?.m_tree;
#else
    IHavokNavFaceTree? IHavokNavMeshEdit.CachedFaceTree => null;
#endif

    IHavokNavMeshEdit IHavokNavMeshEdit.CreateEmpty() => new hkaiNavMesh();

    IHavokNavMeshEdit IHavokNavMeshEdit.ShallowWithVertices(List<Vector4> vertices) => new hkaiNavMesh
    {
        m_faces = m_faces,
        m_edges = m_edges,
        m_vertices = vertices,
        m_faceData = m_faceData,
        m_edgeData = m_edgeData,
        m_faceDataStriding = m_faceDataStriding,
        m_edgeDataStriding = m_edgeDataStriding,
    };

    public partial class Face : IHavokNavFace
    {
        int IHavokNavFace.m_startEdgeIndex { get => m_startEdgeIndex; set => m_startEdgeIndex = value; }

        int IHavokNavFace.m_startUserEdgeIndex { get => m_startUserEdgeIndex; set => m_startUserEdgeIndex = value; }

        short IHavokNavFace.m_numEdges { get => m_numEdges; set => m_numEdges = value; }

        short IHavokNavFace.m_numUserEdges { get => m_numUserEdges; set => m_numUserEdges = value; }

        short IHavokNavFace.m_clusterIndex { get => m_clusterIndex; set => m_clusterIndex = value; }

        ushort IHavokNavFace.m_padding { get => m_padding; set => m_padding = value; }
    }

    public partial class Edge : IHavokNavEdge
    {
        int IHavokNavEdge.m_a { get => m_a; set => m_a = value; }

        int IHavokNavEdge.m_b { get => m_b; set => m_b = value; }

        uint IHavokNavEdge.m_oppositeEdge { get => m_oppositeEdge; set => m_oppositeEdge = value; }

        uint IHavokNavEdge.m_oppositeFace { get => m_oppositeFace; set => m_oppositeFace = value; }

        HavokNavEdgeFlags IHavokNavEdge.m_flags { get => (HavokNavEdgeFlags)(byte)m_flags; set => m_flags = (EdgeFlagBits)(byte)value; }

        float IHavokNavEdge.m_userEdgeCost { get => m_userEdgeCost; set => m_userEdgeCost = value; }
    }
}

public partial class hkAabb : IHavokAabb
{
    Vector4 IHavokAabb.m_min { get => m_min; set => m_min = value; }

    Vector4 IHavokAabb.m_max { get => m_max; set => m_max = value; }
}

public partial class hkRootLevelContainer : IHavokRootContainerEdit
{
    IEnumerable<object> IHavokRootContainerEdit.Variants => m_namedVariants.Select(v => (object?)v.m_variant).Where(v => v != null)!;

    int IHavokRootContainerEdit.KeepOnly(Func<object?, bool> keep) => m_namedVariants.RemoveAll(v => !keep(v.m_variant));

    IReadOnlyList<(string? Name, string? ClassName, object? Variant)> IHavokRootContainerEdit.NamedVariants
        => m_namedVariants.Select(v => (v.m_name, v.m_className, (object?)v.m_variant)).ToList();

    IHavokNavUserEdgeSetups IHavokRootContainerEdit.AddUserEdgeSetups(string name, string className)
    {
        var setups = new hkaiUserEdgeSetupArray();
        m_namedVariants.Add(new NamedVariant { m_name = name, m_className = className, m_variant = setups });
        return setups;
    }

#if HK2014
    IReadOnlyList<IHavokNavFaceTree> IHavokRootContainerEdit.LooseFaceTrees
        => m_namedVariants.Select(v => v.m_variant).OfType<hkcdStaticAabbTree>().Select(t => (IHavokNavFaceTree)new LooseAabbTree(t)).ToList();
#else
    IReadOnlyList<IHavokNavFaceTree> IHavokRootContainerEdit.LooseFaceTrees => Array.Empty<IHavokNavFaceTree>();
#endif
}

#if HK2014
/// <summary>
/// hk2014's hkcdStaticAabbTree as a face tree. Its storage (DefaultTreeStorage6) holds the domain and the nodes itself,
/// in the 6-byte codec hk2018's Aabb6BytesTree uses. Not partial in this SDK, so it is wrapped rather than implemented.
/// </summary>
internal sealed class LooseAabbTree : IHavokNavFaceTree
{
    private readonly hkcdStaticAabbTree _tree;

    public LooseAabbTree(hkcdStaticAabbTree tree) => _tree = tree;

    public bool HasNodes => _tree.m_treePtr != null;

    public Vector4 DomainMin
    {
        get => _tree.m_treePtr?.m_domain.m_min ?? default;
        set { if (_tree.m_treePtr != null) _tree.m_treePtr.m_domain.m_min = value; }
    }

    public Vector4 DomainMax
    {
        get => _tree.m_treePtr?.m_domain.m_max ?? default;
        set { if (_tree.m_treePtr != null) _tree.m_treePtr.m_domain.m_max = value; }
    }

    public IReadOnlyList<HavokNavTreeNode> GetNodes() =>
        _tree.m_treePtr?.m_nodes.Select(n => new HavokNavTreeNode(n.m_xyz[0], n.m_xyz[1], n.m_xyz[2], n.m_hiData, n.m_loData)).ToList()
        ?? new List<HavokNavTreeNode>();

    public void SetNodes(Vector4 domainMin, Vector4 domainMax, IReadOnlyList<HavokNavTreeNode> nodes)
    {
        if (_tree.m_treePtr == null)
            throw new InvalidOperationException("The tree has no storage (m_treePtr) to hold nodes.");

        _tree.m_treePtr.m_domain = new hkAabb { m_min = domainMin, m_max = domainMax };
        _tree.m_treePtr.m_nodes = nodes.Select(node =>
        {
            var codec = new hkcdStaticTreeCodec3Axis6 { m_hiData = node.HiData, m_loData = node.LoData };
            codec.m_xyz[0] = node.X;
            codec.m_xyz[1] = node.Y;
            codec.m_xyz[2] = node.Z;
            return codec;
        }).ToList();
    }

    public override bool Equals(object? obj) => obj is LooseAabbTree other && ReferenceEquals(other._tree, _tree);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_tree);
}
#endif

public partial class hkaiUserEdgeSetupArray : IHavokNavUserEdgeSetups
{
    void IHavokNavUserEdgeSetups.TranslateWorldSpace(Vector3 offset)
    {
        foreach (var setup in m_edgeSetups)
        {
            if (setup.m_space != hkaiUserEdgeUtils.UserEdgeSetupSpace.USER_EDGE_SPACE_WORLD)
                continue;

            setup.m_obbA.m_transform = Translate(setup.m_obbA.m_transform, offset);
            setup.m_obbB.m_transform = Translate(setup.m_obbB.m_transform, offset);
        }
    }

    IReadOnlyList<HavokUserEdgeSetup> IHavokNavUserEdgeSetups.Setups => m_edgeSetups.Select(s => new HavokUserEdgeSetup(
        s.m_obbA.m_transform, s.m_obbA.m_halfExtents, s.m_obbB.m_transform, s.m_obbB.m_halfExtents,
        s.m_userDataA, s.m_userDataB, s.m_costAtoB, s.m_costBtoA, s.m_worldUpA, s.m_worldUpB,
        (HavokUserEdgeDirection)(int)s.m_direction, s.m_space == hkaiUserEdgeUtils.UserEdgeSetupSpace.USER_EDGE_SPACE_WORLD, s.m_forceAlign)).ToList();

    void IHavokNavUserEdgeSetups.Add(HavokUserEdgeSetup setup) => m_edgeSetups.Add(new hkaiUserEdgeUtils.UserEdgeSetup
    {
        m_obbA = new hkaiUserEdgeUtils.Obb { m_transform = setup.TransformA, m_halfExtents = setup.HalfExtentsA },
        m_obbB = new hkaiUserEdgeUtils.Obb { m_transform = setup.TransformB, m_halfExtents = setup.HalfExtentsB },
        m_userDataA = setup.UserDataA,
        m_userDataB = setup.UserDataB,
        m_costAtoB = setup.CostAtoB,
        m_costBtoA = setup.CostBtoA,
        m_worldUpA = setup.WorldUpA,
        m_worldUpB = setup.WorldUpB,
        m_direction = (hkaiUserEdgeUtils.UserEdgeDirection)(int)setup.Direction,
        m_space = setup.WorldSpace ? hkaiUserEdgeUtils.UserEdgeSetupSpace.USER_EDGE_SPACE_WORLD : hkaiUserEdgeUtils.UserEdgeSetupSpace.USER_EDGE_SPACE_LOCAL,
        m_forceAlign = setup.ForceAlign,
    });

    void IHavokNavUserEdgeSetups.RemoveAt(int index) => m_edgeSetups.RemoveAt(index);

    private static Matrix4x4 Translate(Matrix4x4 m, Vector3 offset)
    {
        m.M41 += offset.X;
        m.M42 += offset.Y;
        m.M43 += offset.Z;
        return m;
    }
}

#if HK2014 || HK2016
public partial class hkaiDirectedGraphExplicitCost : IHavokDirectedGraphEdit
{
    List<Vector4> IHavokDirectedGraphEdit.m_positions => m_positions;

    int IHavokDirectedGraphEdit.EdgeCount => m_edges.Count;

    float IHavokDirectedGraphEdit.GetEdgeCost(int edge) => m_edges[edge].m_cost;

    void IHavokDirectedGraphEdit.SetEdgeCost(int edge, float cost) => m_edges[edge].m_cost = cost;

    void IHavokDirectedGraphEdit.Replace(IReadOnlyList<Vector4> positions, IReadOnlyList<IReadOnlyList<(int Target, float Cost)>> edgesOfNode)
    {
        m_positions = positions.ToList();
        m_nodes = new List<Node>(edgesOfNode.Count);
        m_edges = new List<Edge>();

        foreach (var list in edgesOfNode)
        {
            m_nodes.Add(new Node { m_startEdgeIndex = m_edges.Count, m_numEdges = list.Count });
            foreach (var (target, cost) in list)
                m_edges.Add(new Edge { m_target = (uint)target, m_cost = cost, m_flags = 0 });
        }
    }
}
#endif

#if !HK2014
public partial class hkaiNavMeshClearanceCache : IHavokNavClearanceCache
{
    private HavokListView<McpDataInteger, IHavokNavMcp>? _mcpView;

    float IHavokNavClearanceCache.m_clearanceCeiling { get => m_clearanceCeiling; set => m_clearanceCeiling = value; }

    float IHavokNavClearanceCache.m_clearanceIntToRealMultiplier { get => m_clearanceIntToRealMultiplier; set => m_clearanceIntToRealMultiplier = value; }

    float IHavokNavClearanceCache.m_clearanceRealToIntMultiplier { get => m_clearanceRealToIntMultiplier; set => m_clearanceRealToIntMultiplier = value; }

    List<uint> IHavokNavClearanceCache.m_faceOffsets { get => m_faceOffsets; set => m_faceOffsets = value; }

    List<byte> IHavokNavClearanceCache.m_edgePairClearances { get => m_edgePairClearances; set => m_edgePairClearances = value; }

    int IHavokNavClearanceCache.m_unusedEdgePairElements { get => m_unusedEdgePairElements; set => m_unusedEdgePairElements = value; }

    IHavokList<IHavokNavMcp> IHavokNavClearanceCache.m_mcpData => _mcpView ??= new HavokListView<McpDataInteger, IHavokNavMcp>(() => m_mcpData);

    List<byte> IHavokNavClearanceCache.m_vertexClearances { get => m_vertexClearances; set => m_vertexClearances = value; }

    int IHavokNavClearanceCache.m_uncalculatedFacesLowerBound { get => m_uncalculatedFacesLowerBound; set => m_uncalculatedFacesLowerBound = value; }

    public partial class McpDataInteger : IHavokNavMcp
    {
        byte IHavokNavMcp.m_interpolant { get => m_interpolant; set => m_interpolant = value; }

        byte IHavokNavMcp.m_clearance { get => m_clearance; set => m_clearance = value; }
    }
}
#endif

#if HK2018 || HK2019
public partial class hkcdStaticAabbTree : IHavokNavFaceTree
{
    bool IHavokNavFaceTree.HasNodes => m_treePtr != null;

    Vector4 IHavokNavFaceTree.DomainMin
    {
        get => m_treePtr?.m_tree.m_domain.m_min ?? default;
        set { if (m_treePtr != null) m_treePtr.m_tree.m_domain.m_min = value; }
    }

    Vector4 IHavokNavFaceTree.DomainMax
    {
        get => m_treePtr?.m_tree.m_domain.m_max ?? default;
        set { if (m_treePtr != null) m_treePtr.m_tree.m_domain.m_max = value; }
    }

    IReadOnlyList<HavokNavTreeNode> IHavokNavFaceTree.GetNodes() =>
        m_treePtr?.m_tree.m_nodes.Select(n => new HavokNavTreeNode(n.m_xyz[0], n.m_xyz[1], n.m_xyz[2], n.m_hiData, n.m_loData)).ToList()
        ?? new List<HavokNavTreeNode>();

    void IHavokNavFaceTree.SetNodes(Vector4 domainMin, Vector4 domainMax, IReadOnlyList<HavokNavTreeNode> nodes)
    {
        if (m_treePtr == null)
            throw new InvalidOperationException("The tree has no implementation (m_treePtr) to hold nodes.");

        // A new tree, as building one did, so nothing of the old one is left behind
        var tree = new hkcdStaticTree.Aabb6BytesTree();
        tree.m_domain = new hkAabb { m_min = domainMin, m_max = domainMax };

        foreach (var node in nodes)
        {
            var codec = new hkcdCompressedAabbCodecs.Aabb6BytesCodec { m_hiData = node.HiData, m_loData = node.LoData };
            codec.m_xyz[0] = node.X;
            codec.m_xyz[1] = node.Y;
            codec.m_xyz[2] = node.Z;
            tree.m_nodes.Add(codec);
        }

        m_treePtr.m_tree = tree;
    }
}
#endif

#if HK2018
public partial class hkaiStaticTreeNavMeshQueryMediator : IHavokNavMeshQueryMediator
{
    IHavokNavMeshEdit? IHavokNavMeshQueryMediator.NavMesh
    {
        get => m_navMesh;
        set => m_navMesh = (hkaiNavMesh?)value;
    }

    IHavokNavFaceTree? IHavokNavMeshQueryMediator.FaceTree => m_tree;
}
#endif
