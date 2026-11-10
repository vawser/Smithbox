using System.Collections;
using System.Numerics;

namespace Havok.Shared;

// Editable cross-version views of navmesh files, for the Map Editor's Navigation tool. Every SDK with navmeshes
// (hk2014 DS3/BB, hk2016 SDT, hk2018 ER/NR/DB, hk2019 AC6) implements them on its own classes, from one shared source
// (Classes/Shared/NavMeshEdit.cs), so the tool works on whichever class set a file was read into.
//
// The members keep the Havok field names (m_faces, m_startEdgeIndex, ...): each view is a window onto those fields,
// implemented explicitly on the class that has them, so the view and the field are the same data under the same name.
// The objects a document holds are the views; nothing is copied or wrapped, so an edit is an edit of the document.
// See Documentation/Claude/Navigation Editor Other Games Plan.md, Phase N1.

/// <summary>hkaiNavMesh::EdgeFlagBits, the same values in every SDK (hk2014 lacks the last two names).</summary>
[Flags]
public enum HavokNavEdgeFlags : byte
{
    EDGE_SILHOUETTE = 1,
    EDGE_RETRIANGULATED = 2,
    EDGE_ORIGINAL = 4,
    EDGE_ORPHANED_USER_EDGE = 8,
    EDGE_USER = 16,
    EDGE_BLOCKED = 32,
    EDGE_EXTERNAL_OPPOSITE = 64,
    EDGE_GARBAGE = 128,
}

/// <summary>
/// A list of a class set's objects seen as one of their views. Adding needs an object of the same class set, which
/// <see cref="New"/> makes.
/// </summary>
public interface IHavokList<T> : IList<T>
{
    /// <summary>A new object of the list's element class, not yet added.</summary>
    T New();

    void AddRange(IEnumerable<T> items);

    void InsertRange(int index, IEnumerable<T> items);

    void RemoveRange(int index, int count);

    int RemoveAll(Predicate<T> match);
}

/// <summary>
/// An <see cref="IHavokList{T}"/> over a class set's list field. It reads the field each time, so it follows the field
/// when the list is replaced.
/// </summary>
public sealed class HavokListView<TImpl, T> : IHavokList<T> where TImpl : class, T, new()
{
    private readonly Func<List<TImpl>> _list;

    public HavokListView(Func<List<TImpl>> list)
    {
        _list = list;
    }

    private List<TImpl> List => _list();

    private static TImpl Cast(T item) => item as TImpl
        ?? throw new ArgumentException($"Only {typeof(TImpl).FullName} objects can be added to this list.", nameof(item));

    public T this[int index]
    {
        get => List[index];
        set => List[index] = Cast(value);
    }

    public int Count => List.Count;

    public bool IsReadOnly => false;

    public T New() => new TImpl();

    public void Add(T item) => List.Add(Cast(item));

    public void AddRange(IEnumerable<T> items) => List.AddRange(items.Select(Cast).ToList());

    public void Insert(int index, T item) => List.Insert(index, Cast(item));

    public void InsertRange(int index, IEnumerable<T> items) => List.InsertRange(index, items.Select(Cast).ToList());

    public void RemoveAt(int index) => List.RemoveAt(index);

    public void RemoveRange(int index, int count) => List.RemoveRange(index, count);

    public int RemoveAll(Predicate<T> match) => List.RemoveAll(x => match(x));

    public bool Remove(T item) => item is TImpl impl && List.Remove(impl);

    public void Clear() => List.Clear();

    public bool Contains(T item) => item is TImpl impl && List.Contains(impl);

    public int IndexOf(T item) => item is TImpl impl ? List.IndexOf(impl) : -1;

    public void CopyTo(T[] array, int arrayIndex)
    {
        var list = List;
        for (int i = 0; i < list.Count; i++)
            array[arrayIndex + i] = list[i];
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in List)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>hkaiNavMesh::Face.</summary>
public interface IHavokNavFace
{
    int m_startEdgeIndex { get; set; }

    int m_startUserEdgeIndex { get; set; }

    short m_numEdges { get; set; }

    short m_numUserEdges { get; set; }

    short m_clusterIndex { get; set; }

    ushort m_padding { get; set; }
}

/// <summary>hkaiNavMesh::Edge.</summary>
public interface IHavokNavEdge
{
    int m_a { get; set; }

    int m_b { get; set; }

    uint m_oppositeEdge { get; set; }

    uint m_oppositeFace { get; set; }

    HavokNavEdgeFlags m_flags { get; set; }

    float m_userEdgeCost { get; set; }
}

/// <summary>hkAabb.</summary>
public interface IHavokAabb
{
    Vector4 m_min { get; set; }

    Vector4 m_max { get; set; }
}

/// <summary>hkaiNavMeshClearanceCache::McpDataInteger.</summary>
public interface IHavokNavMcp
{
    byte m_interpolant { get; set; }

    byte m_clearance { get; set; }
}

/// <summary>hkaiNavMeshClearanceCache (hk2016, hk2018, hk2019: the same fields).</summary>
public interface IHavokNavClearanceCache
{
    float m_clearanceCeiling { get; set; }

    float m_clearanceIntToRealMultiplier { get; set; }

    float m_clearanceRealToIntMultiplier { get; set; }

    List<uint> m_faceOffsets { get; set; }

    List<byte> m_edgePairClearances { get; set; }

    int m_unusedEdgePairElements { get; set; }

    IHavokList<IHavokNavMcp> m_mcpData { get; }

    List<byte> m_vertexClearances { get; set; }

    int m_uncalculatedFacesLowerBound { get; set; }
}

/// <summary>
/// A node of a face tree (hkcdCompressedAabbCodecs::Aabb6BytesCodec): its box as three bytes against its parent's,
/// and 23 bits of data with an inner-node bit (see the Navigation tool's NavigationTree).
/// </summary>
public readonly record struct HavokNavTreeNode(byte X, byte Y, byte Z, byte HiData, ushort LoData);

/// <summary>
/// hkcdStaticAabbTree: the face tree a navmesh is queried through, held by an hk2018 query mediator or an hk2019
/// navmesh's cached face iterator.
/// </summary>
public interface IHavokNavFaceTree
{
    /// <summary>Whether the tree has its implementation (m_treePtr) to hold nodes.</summary>
    bool HasNodes { get; }

    Vector4 DomainMin { get; set; }

    Vector4 DomainMax { get; set; }

    IReadOnlyList<HavokNavTreeNode> GetNodes();

    /// <summary>Replaces the nodes and the domain (the root's box).</summary>
    void SetNodes(Vector4 domainMin, Vector4 domainMax, IReadOnlyList<HavokNavTreeNode> nodes);
}

/// <summary>hkaiStaticTreeNavMeshQueryMediator (hk2018): a navmesh and the face tree over it.</summary>
public interface IHavokNavMeshQueryMediator
{
    IHavokNavMeshEdit? NavMesh { get; set; }

    IHavokNavFaceTree? FaceTree { get; }
}

/// <summary>hkRootLevelContainer: the objects a file names at its root.</summary>
public interface IHavokRootContainerEdit
{
    IEnumerable<object> Variants { get; }

    /// <summary>Takes out the named variants whose object doesn't pass, and returns how many went.</summary>
    int KeepOnly(Func<object?, bool> keep);
}

/// <summary>hkaiUserEdgeSetupArray: the user edges a navmesh was set up with.</summary>
public interface IHavokNavUserEdgeSetups
{
    /// <summary>Moves the boxes of the setups placed in world space by an offset (local ones follow their mesh).</summary>
    void TranslateWorldSpace(Vector3 offset);
}

/// <summary>hkaiNavMesh, editable.</summary>
public interface IHavokNavMeshEdit : IHavokNavMesh
{
    IHavokList<IHavokNavFace> m_faces { get; }

    IHavokList<IHavokNavEdge> m_edges { get; }

    List<Vector4> m_vertices { get; set; }

    List<int> m_faceData { get; set; }

    List<int> m_edgeData { get; set; }

    int m_faceDataStriding { get; set; }

    int m_edgeDataStriding { get; set; }

    byte m_flags { get; set; }

    float m_erosionRadius { get; set; }

    ulong m_userData { get; set; }

    IHavokAabb m_aabb { get; }

    /// <summary>Clears m_streamingSets, whose element class differs between SDKs.</summary>
    void ClearStreamingSets();

    /// <summary>Whether the mesh has a clearance cache seeding (none in hk2014).</summary>
    bool HasClearanceSeeding { get; }

    /// <summary>
    /// The clearance caches of the mesh: the initial caches of its clearance cache seeding, each once (two in most game
    /// files, mostly identical). Empty in hk2014, which has none.
    /// </summary>
    IReadOnlyList<IHavokNavClearanceCache> ClearanceCaches { get; }

    /// <summary>Drops the clearance cache seeding (m_clearanceCacheSeedingDataSet = null).</summary>
    void DropClearanceSeeding();

    /// <summary>The face tree the mesh holds itself: hk2019's m_cachedFaceIterator. Null in the other SDKs.</summary>
    IHavokNavFaceTree? CachedFaceTree { get; }

    /// <summary>A new, empty navmesh of the same class set.</summary>
    IHavokNavMeshEdit CreateEmpty();

    /// <summary>
    /// A navmesh of the same class set that shares this one's faces, edges and data lists, with other vertices: for
    /// drawing an edit in progress. It must only be read.
    /// </summary>
    IHavokNavMeshEdit ShallowWithVertices(List<Vector4> vertices);
}
