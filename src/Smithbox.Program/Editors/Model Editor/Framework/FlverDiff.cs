using SoulsFormats;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

// This file depends on SoulsFormats only, so Claude/Tools/ModelCheckSurvey can compile it and check it against the
// game's models (a model compared with itself must give no differences, and moving or appending objects must not
// make the others look changed).

public enum FlverDiffKind
{
    /// <summary>A plain value: reverted by setting <see cref="FlverFieldDiff.Property" /> on the owner</summary>
    Value,

    /// <summary>A node index, compared by the node's name. Current and Vanilla are the names.</summary>
    NodeReference,

    /// <summary>A material index, compared by the material's name. Current and Vanilla are the names.</summary>
    MaterialReference,

    /// <summary>A mesh's bone table, compared by node names. Current and Vanilla are the name lists.</summary>
    BoneTable,

    /// <summary>A mesh's vertices, faces, vertex buffers and bounds. Current and Vanilla are summaries.</summary>
    Geometry,

    /// <summary>Reported only, not revertable here (buffer layouts, the skeleton set)</summary>
    Info
}

/// <summary>
/// One field that differs between a model object and its vanilla counterpart. <see cref="Owner" /> is the current
/// object holding <see cref="Property" /> (the object itself, a nested object of it, or the header), and
/// <see cref="Index" /> the element of a list field, or -1.
/// </summary>
public sealed record FlverFieldDiff(string Path, FlverDiffKind Kind, object Owner, PropertyInfo Property, int Index,
    object Current, object Vanilla);

/// <summary>
/// Pairs the objects of an edited FLVER with those of its vanilla FLVER and lists what differs. Indices into the
/// node and material lists are compared by the names they point at, so adding, removing or moving objects doesn't
/// make the objects that refer to them look changed. Used by the Model Editor's Vanilla Compare
/// (<c>ModelVanillaCompare</c>). Plan: Documentation/Model Editor Second Review.md, R6.
/// </summary>
public sealed class FlverDiff
{
    public FLVER2 Current { get; }
    public FLVER2 Vanilla { get; }

    /// <summary>Each current object's vanilla counterpart, or null when vanilla has none (added)</summary>
    public Dictionary<object, object> Pairs { get; } = new(ReferenceEqualityComparer.Instance);

    /// <summary>The vanilla materials, nodes, dummies and meshes nothing pairs with</summary>
    public List<object> Removed { get; } = new();

    private readonly string[] CurrentNodeNames;
    private readonly string[] VanillaNodeNames;

    private FlverDiff(FLVER2 current, FLVER2 vanilla)
    {
        Current = current;
        Vanilla = vanilla;

        CurrentNodeNames = current.Nodes.Select(n => n.Name ?? "").ToArray();
        VanillaNodeNames = vanilla.Nodes.Select(n => n.Name ?? "").ToArray();
    }

    /// <summary>Pairs the two models' objects</summary>
    public static FlverDiff Build(FLVER2 current, FLVER2 vanilla)
    {
        var diff = new FlverDiff(current, vanilla);

        diff.Match(current.Materials, vanilla.Materials, m => m.Name ?? "");
        diff.Match(current.Nodes, vanilla.Nodes, n => n.Name ?? "");
        diff.Match(current.Dummies, vanilla.Dummies, d => d.ReferenceID.ToString());
        diff.MatchMeshes();

        return diff;
    }

    #region Matching
    /// <summary>
    /// Pairs by key. Among the objects sharing a key, those exactly equal pair first (so moving one of several
    /// same-named materials doesn't swap them), then the rest in order: the k-th left with the k-th left. Returns the
    /// vanilla objects left over. A current object with no partner pairs with null, if <paramref name="pairUnmatched" />.
    /// </summary>
    private List<TV> Match<TC, TV>(IEnumerable<TC> current, IEnumerable<TV> vanilla, Func<TC, string> keyC,
        Func<TV, string> keyV, bool pairUnmatched = true) where TC : class where TV : class
    {
        var pool = new Dictionary<string, List<TV>>();
        var vanillaList = vanilla.ToList();

        foreach (var v in vanillaList)
        {
            var key = keyV(v);

            if (!pool.TryGetValue(key, out var list))
                pool[key] = list = new List<TV>();

            list.Add(v);
        }

        var matched = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var pending = current.Where(c => !(Pairs.TryGetValue(c, out var existing) && existing != null)).ToList();

        // Exactly equal objects, where a key has more than one candidate (a single candidate pairs anyway)
        foreach (var c in pending.ToList())
        {
            if (!pool.TryGetValue(keyC(c), out var candidates) || candidates.Count < 2)
                continue;

            var equal = candidates.FirstOrDefault(v => !matched.Contains(v) && CompareWith(c, v, 1).Count == 0);

            if (equal != null)
            {
                Pairs[c] = equal;
                matched.Add(equal);
                pending.Remove(c);
            }
        }

        // The rest in order
        foreach (var c in pending)
        {
            var v = pool.TryGetValue(keyC(c), out var candidates)
                ? candidates.FirstOrDefault(x => !matched.Contains(x))
                : null;

            if (v != null)
            {
                Pairs[c] = v;
                matched.Add(v);
            }
            else if (pairUnmatched)
            {
                Pairs[c] = null;
            }
        }

        return vanillaList.Where(v => !matched.Contains(v)).ToList();
    }

    private void Match<T>(IEnumerable<T> current, IEnumerable<T> vanilla, Func<T, string> key) where T : class
    {
        Removed.AddRange(Match(current, vanilla, key, key));
    }

    /// <summary>
    /// Meshes have no names: they pair by material name and vertex count first, which survives moves, reorders and
    /// copies, then by vertex count alone, which survives a material change
    /// </summary>
    private void MatchMeshes()
    {
        string MaterialName(FLVER2 flver, FLVER2.Mesh mesh) =>
            mesh.MaterialIndex >= 0 && mesh.MaterialIndex < flver.Materials.Count ? flver.Materials[mesh.MaterialIndex].Name ?? "" : $"#{mesh.MaterialIndex}";

        var left = Match(Current.Meshes, Vanilla.Meshes,
            m => $"{MaterialName(Current, m)}|{m.Vertices.Count}",
            m => $"{MaterialName(Vanilla, m)}|{m.Vertices.Count}",
            pairUnmatched: false);

        var unpaired = Current.Meshes.Where(m => !Pairs.ContainsKey(m)).ToList();

        Removed.AddRange(Match(unpaired, left, m => m.Vertices.Count.ToString(), m => m.Vertices.Count.ToString()));
    }
    #endregion

    #region Comparing
    /// <summary>Whether the object differs from its counterpart (stops at the first difference)</summary>
    public bool Differs(object current)
    {
        return Compare(current, 1).Count > 0;
    }

    /// <summary>
    /// The fields of a current material, node, dummy or mesh that differ from its counterpart, up to
    /// <paramref name="max" />. Empty for an object with no counterpart.
    /// </summary>
    public List<FlverFieldDiff> Compare(object current, int max = 500)
    {
        if (current == null || !Pairs.TryGetValue(current, out var vanilla) || vanilla == null)
            return new List<FlverFieldDiff>();

        return CompareWith(current, vanilla, max);
    }

    private List<FlverFieldDiff> CompareWith(object current, object vanilla, int max)
    {
        var diffs = new List<FlverFieldDiff>();

        switch (current)
        {
            case FLVER.Node node:
                CompareObject("", node, vanilla, NodeSkipped, diffs, 0, max);
                CompareNodeReference("ParentIndex", node, nameof(FLVER.Node.ParentIndex), node.ParentIndex, ((FLVER.Node)vanilla).ParentIndex, diffs);
                break;

            case FLVER.Dummy dummy:
                var vd = (FLVER.Dummy)vanilla;
                CompareObject("", dummy, vanilla, DummySkipped, diffs, 0, max);
                CompareNodeReference("ParentBoneIndex", dummy, nameof(FLVER.Dummy.ParentBoneIndex), dummy.ParentBoneIndex, vd.ParentBoneIndex, diffs);
                CompareNodeReference("AttachBoneIndex", dummy, nameof(FLVER.Dummy.AttachBoneIndex), dummy.AttachBoneIndex, vd.AttachBoneIndex, diffs);
                break;

            case FLVER2.Mesh mesh:
                CompareMesh(mesh, (FLVER2.Mesh)vanilla, diffs, max);
                break;

            default:
                CompareObject("", current, vanilla, null, diffs, 0, max);
                break;
        }

        return diffs.Count > max ? diffs.Take(max).ToList() : diffs;
    }

    /// <summary>The model's own fields: the header, and what is reported but not revertable here</summary>
    public List<FlverFieldDiff> CompareModel()
    {
        var diffs = new List<FlverFieldDiff>();

        CompareObject("Header.", Current.Header, Vanilla.Header, null, diffs, 0, 100);

        if (!LayoutsEqual(Current.BufferLayouts, Vanilla.BufferLayouts))
        {
            diffs.Add(new FlverFieldDiff("BufferLayouts", FlverDiffKind.Info, Current, null, -1,
                Current.BufferLayouts.Count, Vanilla.BufferLayouts.Count));
        }

        if (!SkeletonsEqual())
        {
            diffs.Add(new FlverFieldDiff("Skeletons", FlverDiffKind.Info, Current, null, -1,
                SkeletonCount(Current), SkeletonCount(Vanilla)));
        }

        return diffs;
    }

    // Fields compared another way, or kept by the editor from other fields
    private static readonly HashSet<string> NodeSkipped = new()
    {
        nameof(FLVER.Node.ParentIndex), nameof(FLVER.Node.FirstChildIndex),
        nameof(FLVER.Node.NextSiblingIndex), nameof(FLVER.Node.PreviousSiblingIndex)
    };

    private static readonly HashSet<string> DummySkipped = new()
    {
        nameof(FLVER.Dummy.ParentBoneIndex), nameof(FLVER.Dummy.AttachBoneIndex)
    };

    private static readonly HashSet<string> MeshSkipped = new()
    {
        nameof(FLVER2.Mesh.MaterialIndex), nameof(FLVER2.Mesh.NodeIndex), nameof(FLVER2.Mesh.BoneIndices),
        nameof(FLVER2.Mesh.Vertices), nameof(FLVER2.Mesh.FaceSets), nameof(FLVER2.Mesh.VertexBuffers),
        nameof(FLVER2.Mesh.BoundingBox)
    };

    /// <summary>A node index as a name; out of range keeps the number (rigid meshes store such indices)</summary>
    private static string NodeName(string[] names, int index)
    {
        if (index == -1)
            return null;

        return index >= 0 && index < names.Length ? names[index] : $"#{index}";
    }

    public string CurrentNodeName(int index) => NodeName(CurrentNodeNames, index);
    public string VanillaNodeName(int index) => NodeName(VanillaNodeNames, index);

    private void CompareNodeReference(string path, object owner, string property, int current, int vanilla,
        List<FlverFieldDiff> diffs)
    {
        var cn = CurrentNodeName(current);
        var vn = VanillaNodeName(vanilla);

        // Vanilla's index is past its node list, so it isn't used as a node (rigid ER assets do this): the numbers
        // are compared, as a node appended since can bring the same number into range
        var same = vn != null && vn.StartsWith('#') ? current == vanilla : cn == vn;

        if (!same)
        {
            diffs.Add(new FlverFieldDiff(path, FlverDiffKind.NodeReference, owner, owner.GetType().GetProperty(property), -1, cn, vn));
        }
    }

    private void CompareMesh(FLVER2.Mesh mesh, FLVER2.Mesh vanilla, List<FlverFieldDiff> diffs, int max)
    {
        CompareObject("", mesh, vanilla, MeshSkipped, diffs, 0, max);

        // Material, by name
        string MaterialName(FLVER2 flver, int index) =>
            index >= 0 && index < flver.Materials.Count ? flver.Materials[index].Name : $"#{index}";

        var cm = MaterialName(Current, mesh.MaterialIndex);
        var vm = MaterialName(Vanilla, vanilla.MaterialIndex);

        if (cm != vm)
        {
            diffs.Add(new FlverFieldDiff("MaterialIndex", FlverDiffKind.MaterialReference, mesh,
                typeof(FLVER2.Mesh).GetProperty(nameof(FLVER2.Mesh.MaterialIndex)), -1, cm, vm));
        }

        CompareNodeReference("NodeIndex", mesh, nameof(FLVER2.Mesh.NodeIndex), mesh.NodeIndex, vanilla.NodeIndex, diffs);

        // Bone table, by names
        var ct = mesh.BoneIndices.Select(CurrentNodeName).ToList();
        var vt = vanilla.BoneIndices.Select(VanillaNodeName).ToList();

        if (!ct.SequenceEqual(vt))
        {
            diffs.Add(new FlverFieldDiff("BoneIndices", FlverDiffKind.BoneTable, mesh,
                typeof(FLVER2.Mesh).GetProperty(nameof(FLVER2.Mesh.BoneIndices)), -1, ct, vt));
        }

        // Geometry, as one difference
        var geometry = DescribeGeometryDifference(mesh, vanilla);

        if (geometry != null)
        {
            diffs.Add(new FlverFieldDiff("Geometry", FlverDiffKind.Geometry, mesh, null, -1, geometry.Value.Current, geometry.Value.Vanilla));
        }
    }

    // Property lists per type: public, readable, not indexers
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && p.GetGetMethod() != null)
            .ToArray());
    }

    private static bool IsLeaf(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type.IsValueType;
    }

    private const int MaxDepth = 6;

    private static void CompareObject(string path, object current, object vanilla, HashSet<string> skipped,
        List<FlverFieldDiff> diffs, int depth, int max)
    {
        foreach (var prop in GetProperties(current.GetType()))
        {
            if (diffs.Count >= max)
                return;

            if (skipped != null && skipped.Contains(prop.Name))
                continue;

            object cv, vv;

            try
            {
                cv = prop.GetValue(current);
                vv = prop.GetValue(vanilla);
            }
            catch
            {
                continue;
            }

            CompareValue(path + prop.Name, current, prop, -1, cv, vv, diffs, depth, max);
        }
    }

    private static void CompareValue(string path, object owner, PropertyInfo prop, int index, object cv, object vv,
        List<FlverFieldDiff> diffs, int depth, int max)
    {
        if (cv == null && vv == null)
            return;

        if (cv == null || vv == null || cv.GetType() != vv.GetType())
        {
            diffs.Add(new FlverFieldDiff(path, FlverDiffKind.Value, owner, prop, index, cv, vv));
            return;
        }

        var type = cv.GetType();

        if (IsLeaf(type) || depth >= MaxDepth)
        {
            if (!Equals(cv, vv))
                diffs.Add(new FlverFieldDiff(path, FlverDiffKind.Value, owner, prop, index, cv, vv));

            return;
        }

        if (cv is IList cl && vv is IList vl)
        {
            // A list of another length is replaced as a whole
            if (cl.Count != vl.Count)
            {
                diffs.Add(new FlverFieldDiff(path, FlverDiffKind.Value, owner, prop, index, cv, vv));
                return;
            }

            for (var i = 0; i < cl.Count && diffs.Count < max; i++)
            {
                var ce = cl[i];
                var ve = vl[i];

                if (ce != null && ve != null && ce.GetType() == ve.GetType() && !IsLeaf(ce.GetType()))
                {
                    CompareObject($"{path}[{i}].", ce, ve, null, diffs, depth + 1, max);
                }
                else if (index < 0)
                {
                    CompareValue($"{path}[{i}]", owner, prop, i, ce, ve, diffs, depth + 1, max);
                }
                else if (!Equals(ce, ve))
                {
                    diffs.Add(new FlverFieldDiff(path, FlverDiffKind.Value, owner, prop, index, cv, vv));
                    return;
                }
            }

            return;
        }

        CompareObject(path + ".", cv, vv, null, diffs, depth + 1, max);
    }
    #endregion

    #region Geometry
    /// <summary>Vertex bone indices name nodes directly after this version, and a slot of the mesh's table before</summary>
    private static bool UsesPerMeshBoneTable(FLVER2 flver) => flver.Header.Version <= 0x2000D;

    private static List<FLVER.LayoutMember> GetLayoutMembers(FLVER2 flver, FLVER2.Mesh mesh)
    {
        return mesh.VertexBuffers
            .Where(b => b.LayoutIndex >= 0 && b.LayoutIndex < flver.BufferLayouts.Count)
            .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
            .ToList();
    }

    /// <summary>Which vertex fields name nodes: the bone indices, and the normal's W on rigid meshes</summary>
    private static (bool BoneIndices, bool NormalW) NodeFields(FLVER2 flver, FLVER2.Mesh mesh)
    {
        if (UsesPerMeshBoneTable(flver))
            return (false, false);

        var members = GetLayoutMembers(flver, mesh);
        var boneIndices = members.Any(l => l.Semantic == FLVER.LayoutSemantic.BoneIndices);
        var normalW = !boneIndices && members.Any(l => l.Semantic == FLVER.LayoutSemantic.Normal &&
            (l.Type == FLVER.LayoutType.UByte4 || l.Type == FLVER.LayoutType.Byte4E || l.Type == FLVER.LayoutType.Color));

        return (boneIndices, normalW);
    }

    /// <summary>
    /// Null when the geometry is the same; otherwise short summaries of each side. Node indices in the vertices are
    /// compared by name, so appending or moving nodes doesn't change a mesh.
    /// </summary>
    public (string Current, string Vanilla)? DescribeGeometryDifference(FLVER2.Mesh mesh, FLVER2.Mesh vanilla)
    {
        var parts = new List<string>();

        if (mesh.Vertices.Count != vanilla.Vertices.Count)
        {
            parts.Add("vertex count");
        }
        else
        {
            var changed = CountChangedVertices(mesh, vanilla);

            if (changed > 0)
                parts.Add($"{changed} vertices");
        }

        if (!FaceSetsEqual(mesh.FaceSets, vanilla.FaceSets))
            parts.Add("faces");

        if (!VertexBuffersEqual(mesh, vanilla))
            parts.Add("vertex buffers");

        if (!BoundsEqual(mesh.BoundingBox, vanilla.BoundingBox))
            parts.Add("bounds");

        if (parts.Count == 0)
            return null;

        var faces = (FLVER2.Mesh m) => m.FaceSets.Sum(f => f.Indices.Count) / 3;

        return ($"{mesh.Vertices.Count} vertices, {faces(mesh)} faces ({string.Join(", ", parts)} differ)",
            $"{vanilla.Vertices.Count} vertices, {faces(vanilla)} faces");
    }

    private int CountChangedVertices(FLVER2.Mesh mesh, FLVER2.Mesh vanilla)
    {
        var (currentBones, currentNormalW) = NodeFields(Current, mesh);
        var (vanillaBones, vanillaNormalW) = NodeFields(Vanilla, vanilla);

        var changed = 0;

        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var c = mesh.Vertices[i];
            var v = vanilla.Vertices[i];

            if (!VertexEqual(c, v, currentBones && vanillaBones, currentNormalW && vanillaNormalW))
                changed++;
        }

        return changed;
    }

    private bool VertexEqual(FLVER.Vertex c, FLVER.Vertex v, bool bonesAreNodes, bool normalWIsNode)
    {
        if (c.Position != v.Position || c.Normal != v.Normal || c.Bitangent != v.Bitangent)
            return false;

        if (!c.UVs.SequenceEqual(v.UVs) || !c.Tangents.SequenceEqual(v.Tangents) || !c.Colors.SequenceEqual(v.Colors))
            return false;

        for (var k = 0; k < 4; k++)
        {
            if (c.BoneWeights[k] != v.BoneWeights[k])
                return false;

            if (bonesAreNodes
                    ? CurrentNodeName(c.BoneIndices[k]) != VanillaNodeName(v.BoneIndices[k])
                    : c.BoneIndices[k] != v.BoneIndices[k])
                return false;
        }

        return normalWIsNode
            ? CurrentNodeName(c.NormalW) == VanillaNodeName(v.NormalW)
            : c.NormalW == v.NormalW;
    }

    private static bool FaceSetsEqual(List<FLVER2.FaceSet> a, List<FLVER2.FaceSet> b)
    {
        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].Flags != b[i].Flags || a[i].TriangleStrip != b[i].TriangleStrip ||
                a[i].CullBackfaces != b[i].CullBackfaces || a[i].Unk06 != b[i].Unk06 ||
                !a[i].Indices.SequenceEqual(b[i].Indices))
                return false;
        }

        return true;
    }

    /// <summary>The vertex buffers, compared by the layouts they use rather than the layouts' positions</summary>
    private bool VertexBuffersEqual(FLVER2.Mesh mesh, FLVER2.Mesh vanilla)
    {
        if (mesh.VertexBuffers.Count != vanilla.VertexBuffers.Count)
            return false;

        for (var i = 0; i < mesh.VertexBuffers.Count; i++)
        {
            var c = mesh.VertexBuffers[i];
            var v = vanilla.VertexBuffers[i];

            if (c.EdgeCompressed != v.EdgeCompressed)
                return false;

            if (!LayoutEqual(GetLayout(Current, c.LayoutIndex), GetLayout(Vanilla, v.LayoutIndex)))
                return false;
        }

        return true;
    }

    private static FLVER2.BufferLayout GetLayout(FLVER2 flver, int index)
    {
        return index >= 0 && index < flver.BufferLayouts.Count ? flver.BufferLayouts[index] : null;
    }

    public static bool LayoutEqual(FLVER2.BufferLayout a, FLVER2.BufferLayout b)
    {
        if (a == null || b == null)
            return a == b;

        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].Stream != b[i].Stream || a[i].SpecialModifier != b[i].SpecialModifier || a[i].Type != b[i].Type ||
                a[i].Semantic != b[i].Semantic || a[i].Index != b[i].Index)
                return false;
        }

        return true;
    }

    private static bool LayoutsEqual(List<FLVER2.BufferLayout> a, List<FLVER2.BufferLayout> b)
    {
        return a.Count == b.Count && a.Zip(b).All(p => LayoutEqual(p.First, p.Second));
    }

    private static bool BoundsEqual(FLVER2.Mesh.BoundingBoxes a, FLVER2.Mesh.BoundingBoxes b)
    {
        if (a == null || b == null)
            return a == b;

        return a.Min == b.Min && a.Max == b.Max && a.Unk == b.Unk;
    }

    private static int SkeletonCount(FLVER2 flver)
    {
        return (flver.Skeletons?.BaseSkeleton?.Count ?? 0) + (flver.Skeletons?.AllSkeletons?.Count ?? 0);
    }

    /// <summary>The skeleton set, with each bone's node compared by name</summary>
    private bool SkeletonsEqual()
    {
        static IEnumerable<FLVER2.SkeletonSet.Bone> Bones(FLVER2 flver) =>
            (flver.Skeletons?.BaseSkeleton ?? new()).Concat(flver.Skeletons?.AllSkeletons ?? new());

        var a = Bones(Current).ToList();
        var b = Bones(Vanilla).ToList();

        if (a.Count != b.Count || (Current.Skeletons == null) != (Vanilla.Skeletons == null))
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].ParentIndex != b[i].ParentIndex || a[i].FirstChildIndex != b[i].FirstChildIndex ||
                a[i].NextSiblingIndex != b[i].NextSiblingIndex || a[i].PreviousSiblingIndex != b[i].PreviousSiblingIndex ||
                CurrentNodeName(a[i].NodeIndex) != VanillaNodeName(b[i].NodeIndex))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Copies of the vanilla mesh's geometry with its node indices (bone table, vertex bones) and layout indices
    /// mapped to the current model. Null, with the reason, when it can't be: a node or layout the current model lacks.
    /// </summary>
    public MeshGeometry CopyVanillaGeometry(FLVER2.Mesh mesh, out string missing)
    {
        missing = null;

        if (!Pairs.TryGetValue(mesh, out var paired) || paired is not FLVER2.Mesh vanilla)
        {
            missing = "no vanilla mesh";
            return null;
        }

        var nodeIndex = new Dictionary<string, int>();

        for (var i = 0; i < CurrentNodeNames.Length; i++)
            nodeIndex.TryAdd(CurrentNodeNames[i], i);

        string lacking = null;

        int MapNode(int vanillaIndex)
        {
            var name = VanillaNodeName(vanillaIndex);

            if (name == null)
                return -1;

            // An index past vanilla's node list is kept as it is
            if (name.StartsWith('#'))
                return vanillaIndex;

            if (nodeIndex.TryGetValue(name, out var index))
                return index;

            lacking ??= $"node {name}";
            return vanillaIndex;
        }

        var geometry = new MeshGeometry
        {
            BoneIndices = vanilla.BoneIndices.Select(MapNode).ToList()
        };

        foreach (var buffer in vanilla.VertexBuffers)
        {
            var layout = GetLayout(Vanilla, buffer.LayoutIndex);
            var index = Current.BufferLayouts.FindIndex(l => LayoutEqual(l, layout));

            if (index < 0)
            {
                missing = "a buffer layout the model doesn't have";
                return null;
            }

            geometry.VertexBuffers.Add(new FLVER2.VertexBuffer(index) { EdgeCompressed = buffer.EdgeCompressed });
        }

        var (bonesAreNodes, normalWIsNode) = NodeFields(Vanilla, vanilla);

        foreach (var v in vanilla.Vertices)
        {
            var copy = new FLVER.Vertex(v) { NormalW = v.NormalW };

            // All four slots, as ModelIndexRemapper maps them, so unweighted slots compare equal afterwards too
            if (bonesAreNodes)
            {
                var indices = copy.BoneIndices;

                for (var k = 0; k < 4; k++)
                    indices[k] = MapNode(v.BoneIndices[k]);

                copy.BoneIndices = indices;
            }

            if (normalWIsNode)
                copy.NormalW = MapNode(v.NormalW);

            geometry.Vertices.Add(copy);
        }

        foreach (var f in vanilla.FaceSets)
        {
            geometry.FaceSets.Add(new FLVER2.FaceSet(f.Flags, f.TriangleStrip, f.CullBackfaces, f.Unk06, new List<int>(f.Indices)));
        }

        if (vanilla.BoundingBox != null)
        {
            geometry.BoundingBox = new FLVER2.Mesh.BoundingBoxes
            {
                Min = vanilla.BoundingBox.Min,
                Max = vanilla.BoundingBox.Max,
                Unk = vanilla.BoundingBox.Unk
            };
        }

        if (lacking != null)
        {
            missing = lacking;
            return null;
        }

        return geometry;
    }
    #endregion
}

/// <summary>A mesh's geometry, as <see cref="FlverDiff.CopyVanillaGeometry" /> and the revert action carry it</summary>
public sealed class MeshGeometry
{
    public List<int> BoneIndices = new();
    public List<FLVER.Vertex> Vertices = new();
    public List<FLVER2.FaceSet> FaceSets = new();
    public List<FLVER2.VertexBuffer> VertexBuffers = new();
    public FLVER2.Mesh.BoundingBoxes BoundingBox;

    /// <summary>The mesh's current geometry, by reference (the lists are replaced, not changed, by the revert)</summary>
    public static MeshGeometry Of(FLVER2.Mesh mesh)
    {
        return new MeshGeometry
        {
            BoneIndices = mesh.BoneIndices.ToList(),
            Vertices = mesh.Vertices.ToList(),
            FaceSets = mesh.FaceSets.ToList(),
            VertexBuffers = mesh.VertexBuffers.ToList(),
            BoundingBox = mesh.BoundingBox
        };
    }

    /// <summary>Puts this geometry into the mesh, keeping the mesh's own list objects</summary>
    public void ApplyTo(FLVER2.Mesh mesh)
    {
        mesh.BoneIndices.Clear();
        mesh.BoneIndices.AddRange(BoneIndices);

        mesh.Vertices.Clear();
        mesh.Vertices.AddRange(Vertices);

        mesh.FaceSets.Clear();
        mesh.FaceSets.AddRange(FaceSets);

        mesh.VertexBuffers.Clear();
        mesh.VertexBuffers.AddRange(VertexBuffers);

        mesh.BoundingBox = BoundingBox;
    }
}
