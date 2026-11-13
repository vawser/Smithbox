using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The values a structural action changed, so that its undo can put them back
/// </summary>
public sealed class IndexChangeRecord
{
    private readonly List<Action> _undo = new();

    public void Record(Action undo)
    {
        _undo.Add(undo);
    }

    public void Revert()
    {
        for (var i = _undo.Count - 1; i >= 0; i--)
        {
            _undo[i]();
        }

        _undo.Clear();
    }
}

/// <summary>
/// The FLVER refers to its materials and nodes by their position in the material and node lists, and those lists
/// are written in the order of the container's objects. Anything that adds, removes or moves a material or node
/// has to rewrite these references, or they end up pointing at a different material or node once saved.
/// </summary>
public static class ModelIndexRemapper
{
    private const int MaxReportedReferences = 10;

    public static List<FLVER2.Material> GetMaterials(ModelContainer container)
    {
        return container.Objects.Select(e => e.WrappedObject).OfType<FLVER2.Material>().ToList();
    }

    public static List<FLVER.Node> GetNodes(ModelContainer container)
    {
        return container.Objects.Select(e => e.WrappedObject).OfType<FLVER.Node>().ToList();
    }

    private static List<FLVER2.Mesh> GetMeshes(ModelContainer container)
    {
        return container.Objects.Select(e => e.WrappedObject).OfType<FLVER2.Mesh>().ToList();
    }

    private static List<FLVER.Dummy> GetDummies(ModelContainer container)
    {
        return container.Objects.Select(e => e.WrappedObject).OfType<FLVER.Dummy>().ToList();
    }

    private static FLVER2.SkeletonSet GetSkeletons(ModelContainer container)
    {
        return container.Objects.Select(e => e.WrappedObject).OfType<FLVER2.SkeletonSet>().FirstOrDefault();
    }

    /// <summary>
    /// The objects of the model that the editing actions may change: the entries of the FLVER's material, node,
    /// dummy and mesh lists. The skeleton set and the other files (CLM2, HKX, ...) are single objects saved from
    /// elsewhere, so removing or copying them either breaks the save or is lost.
    /// </summary>
    public static List<ModelEntity> GetEditableSelection(ModelEditorView view, ModelContainer container)
    {
        return view.ViewportSelection.GetFilteredSelection<ModelEntity>()
            .Where(e => e.WrappedObject != null && IsListEntry(e) && container.Objects.Contains(e))
            .ToList();
    }

    public static bool IsListEntry(ModelEntity entity)
    {
        return entity.WrappedObject is FLVER2.Material or FLVER.Node or FLVER.Dummy or FLVER2.Mesh;
    }

    #region Vertex Layout
    // Vertex bone indices refer to the bone table of the mesh in older versions, and to the node list after that
    private static bool UsesPerMeshBoneTable(FLVER2 flver)
    {
        return flver.Header.Version <= 0x2000D;
    }

    private static IEnumerable<FLVER.LayoutMember> GetLayoutMembers(FLVER2 flver, FLVER2.Mesh mesh)
    {
        return mesh.VertexBuffers
            .Where(b => b.LayoutIndex >= 0 && b.LayoutIndex < flver.BufferLayouts.Count)
            .SelectMany(b => flver.BufferLayouts[b.LayoutIndex]);
    }

    private static bool UsesBoneIndices(FLVER2 flver, FLVER2.Mesh mesh)
    {
        return GetLayoutMembers(flver, mesh).Any(l => l.Semantic == FLVER.LayoutSemantic.BoneIndices);
    }

    // Vertices without bone indices name a single bone through their normal W, when the normal has a W component
    private static bool UsesNormalW(FLVER2 flver, FLVER2.Mesh mesh)
    {
        return !UsesBoneIndices(flver, mesh) && GetLayoutMembers(flver, mesh).Any(l =>
            l.Semantic == FLVER.LayoutSemantic.Normal &&
            (l.Type == FLVER.LayoutType.UByte4 || l.Type == FLVER.LayoutType.Byte4E || l.Type == FLVER.LayoutType.Color));
    }
    #endregion

    #region References
    /// <summary>
    /// Lists what still refers to the materials and nodes about to be deleted. The deletion is refused while there is
    /// anything listed, as removing them would leave those references pointing at a missing material or node.
    /// </summary>
    public static List<string> FindBlockingReferences(ModelContainer container, IReadOnlyCollection<Entity> targets)
    {
        var references = new List<string>();

        var deleted = new HashSet<object>(targets.Select(e => e.WrappedObject), ReferenceEqualityComparer.Instance);

        var materials = GetMaterials(container);
        var nodes = GetNodes(container);

        var deletedMaterials = new HashSet<int>(Enumerable.Range(0, materials.Count).Where(i => deleted.Contains(materials[i])));
        var deletedNodes = new HashSet<int>(Enumerable.Range(0, nodes.Count).Where(i => deleted.Contains(nodes[i])));

        if (deletedMaterials.Count == 0 && deletedNodes.Count == 0)
            return references;

        string MaterialName(int index) => $"material {index} ({materials[index].Name})";
        string NodeName(int index) => $"node {index} ({nodes[index].Name})";

        var flver = container.Flver;
        var perMeshBoneTable = flver != null && UsesPerMeshBoneTable(flver);

        var meshes = GetMeshes(container);

        for (var m = 0; m < meshes.Count; m++)
        {
            var mesh = meshes[m];

            if (deleted.Contains(mesh))
                continue;

            if (deletedMaterials.Contains(mesh.MaterialIndex))
                references.Add($"Mesh {m} uses {MaterialName(mesh.MaterialIndex)}");

            if (deletedNodes.Contains(mesh.NodeIndex))
                references.Add($"Mesh {m} is attached to {NodeName(mesh.NodeIndex)}");

            foreach (var boneIndex in mesh.BoneIndices.Distinct().Where(deletedNodes.Contains))
                references.Add($"Mesh {m} has {NodeName(boneIndex)} in its bone table");

            if (flver == null || perMeshBoneTable || deletedNodes.Count == 0)
                continue;

            // The vertices name the nodes directly
            var weightedNodes = new HashSet<int>();

            if (UsesBoneIndices(flver, mesh))
            {
                foreach (var vertex in mesh.Vertices)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        if (vertex.BoneWeights[k] != 0 && deletedNodes.Contains(vertex.BoneIndices[k]))
                            weightedNodes.Add(vertex.BoneIndices[k]);
                    }
                }
            }
            else if (UsesNormalW(flver, mesh))
            {
                foreach (var vertex in mesh.Vertices)
                {
                    if (deletedNodes.Contains(vertex.NormalW))
                        weightedNodes.Add(vertex.NormalW);
                }
            }

            foreach (var node in weightedNodes)
                references.Add($"Mesh {m} has vertices weighted to {NodeName(node)}");
        }

        var dummies = GetDummies(container);

        for (var d = 0; d < dummies.Count; d++)
        {
            var dummy = dummies[d];

            if (deleted.Contains(dummy))
                continue;

            if (deletedNodes.Contains(dummy.ParentBoneIndex))
                references.Add($"Dummy {d} [{dummy.ReferenceID}] has {NodeName(dummy.ParentBoneIndex)} as its parent bone");

            if (deletedNodes.Contains(dummy.AttachBoneIndex))
                references.Add($"Dummy {d} [{dummy.ReferenceID}] is attached to {NodeName(dummy.AttachBoneIndex)}");
        }

        for (var n = 0; n < nodes.Count; n++)
        {
            if (deletedNodes.Contains(n))
                continue;

            if (deletedNodes.Contains(nodes[n].ParentIndex))
                references.Add($"{NodeName(n)} is a child of {NodeName(nodes[n].ParentIndex)}");
        }

        var skeletons = GetSkeletons(container);

        if (skeletons != null && deletedNodes.Count > 0)
        {
            foreach (var bone in skeletons.BaseSkeleton.Concat(skeletons.AllSkeletons))
            {
                if (deletedNodes.Contains(bone.NodeIndex))
                    references.Add($"The skeleton set has a bone for {NodeName(bone.NodeIndex)}");
            }
        }

        return references.Distinct().ToList();
    }

    /// <summary>
    /// Logs the references that stopped a deletion, keeping the list short for heavily referenced nodes
    /// </summary>
    public static string DescribeReferences(List<string> references)
    {
        var lines = references.Take(MaxReportedReferences).ToList();

        if (references.Count > MaxReportedReferences)
        {
            lines.Add($"...and {references.Count - MaxReportedReferences} more");
        }

        return string.Join("\n", lines);
    }
    #endregion

    #region Node Links
    /// <summary>
    /// Takes a node out of the sibling chain of its parent, so no other node links to it once it is deleted.
    /// The indices are those of the node list before the deletion.
    /// </summary>
    public static void UnlinkNode(List<FLVER.Node> nodes, int index, IndexChangeRecord record)
    {
        var node = nodes[index];

        var previous = node.PreviousSiblingIndex;
        var next = node.NextSiblingIndex;
        var parent = node.ParentIndex;

        if (IsValid(previous, nodes.Count) && nodes[previous].NextSiblingIndex == index)
        {
            SetNextSibling(nodes[previous], next, record);
        }

        if (IsValid(next, nodes.Count) && nodes[next].PreviousSiblingIndex == index)
        {
            SetPreviousSibling(nodes[next], previous, record);
        }

        if (IsValid(parent, nodes.Count) && nodes[parent].FirstChildIndex == index)
        {
            SetFirstChild(nodes[parent], next, record);
        }
    }

    /// <summary>
    /// Adds a copy of a node to the end of the sibling chain of the node it was copied from.
    /// The copy has none of the children of the original.
    /// </summary>
    public static void LinkClonedNode(List<FLVER.Node> nodes, FLVER.Node original, FLVER.Node clone, IndexChangeRecord record)
    {
        var cloneIndex = (short)nodes.IndexOf(clone);
        var last = nodes.IndexOf(original);

        if (cloneIndex < 0 || last < 0)
            return;

        SetFirstChild(clone, -1, record);
        SetNextSibling(clone, -1, record);

        // Find the last sibling, guarding against a broken chain looping back on itself
        var visited = new HashSet<int>();

        while (IsValid(nodes[last].NextSiblingIndex, nodes.Count) && nodes[last].NextSiblingIndex != cloneIndex && visited.Add(last))
        {
            last = nodes[last].NextSiblingIndex;
        }

        SetNextSibling(nodes[last], cloneIndex, record);
        SetPreviousSibling(clone, (short)last, record);
    }

    /// <summary>
    /// Moves a node from the sibling chain under its old parent to the end of the chain under its new parent, after
    /// its ParentIndex has been changed. Root nodes (-1) form a chain of their own. Returns false, changing nothing,
    /// if the new parent is out of range or is the node itself or one of its descendants.
    /// </summary>
    public static bool RelinkNode(List<FLVER.Node> nodes, int index, int oldParent, int newParent, IndexChangeRecord record)
    {
        if (!IsValid(index, nodes.Count) || oldParent == newParent)
            return false;

        if (newParent != -1 && (!IsValid(newParent, nodes.Count) || IsSelfOrDescendant(nodes, index, newParent)))
            return false;

        var node = nodes[index];

        // Out of the old chain
        var previous = node.PreviousSiblingIndex;
        var next = node.NextSiblingIndex;

        if (IsValid(previous, nodes.Count) && nodes[previous].NextSiblingIndex == index)
            SetNextSibling(nodes[previous], next, record);

        if (IsValid(next, nodes.Count) && nodes[next].PreviousSiblingIndex == index)
            SetPreviousSibling(nodes[next], previous, record);

        if (IsValid(oldParent, nodes.Count) && nodes[oldParent].FirstChildIndex == index)
            SetFirstChild(nodes[oldParent], next, record);

        SetNextSibling(node, -1, record);
        SetPreviousSibling(node, -1, record);

        // Onto the end of the new chain
        int first;

        if (newParent == -1)
        {
            first = nodes.FindIndex(n => n != node && n.ParentIndex == -1 && n.PreviousSiblingIndex == -1);
        }
        else
        {
            first = nodes[newParent].FirstChildIndex;

            if (!IsValid(first, nodes.Count) || first == index)
            {
                SetFirstChild(nodes[newParent], (short)index, record);
                return true;
            }
        }

        if (first == -1)
            return true;

        var last = first;
        var visited = new HashSet<int>();

        while (IsValid(nodes[last].NextSiblingIndex, nodes.Count) && nodes[last].NextSiblingIndex != index && visited.Add(last))
        {
            last = nodes[last].NextSiblingIndex;
        }

        SetNextSibling(nodes[last], (short)index, record);
        SetPreviousSibling(node, (short)last, record);

        return true;
    }

    /// <summary>
    /// Adds a new node, already given its ParentIndex, to the end of the sibling chain under its parent (or of the
    /// root nodes for -1). The node must have no links of its own yet.
    /// </summary>
    public static void LinkNewNode(List<FLVER.Node> nodes, int index, IndexChangeRecord record)
    {
        if (!IsValid(index, nodes.Count))
            return;

        var node = nodes[index];
        var parent = node.ParentIndex;

        SetFirstChild(node, -1, record);
        SetNextSibling(node, -1, record);
        SetPreviousSibling(node, -1, record);

        int first;

        if (IsValid(parent, nodes.Count))
        {
            first = nodes[parent].FirstChildIndex;

            if (!IsValid(first, nodes.Count) || first == index)
            {
                SetFirstChild(nodes[parent], (short)index, record);
                return;
            }
        }
        else
        {
            first = nodes.FindIndex(n => n != node && n.ParentIndex == -1 && n.PreviousSiblingIndex == -1);

            if (first == -1)
                return;
        }

        var last = first;
        var visited = new HashSet<int>();

        while (IsValid(nodes[last].NextSiblingIndex, nodes.Count) && nodes[last].NextSiblingIndex != index && visited.Add(last))
        {
            last = nodes[last].NextSiblingIndex;
        }

        SetNextSibling(nodes[last], (short)index, record);
        SetPreviousSibling(node, (short)last, record);
    }

    /// <summary>
    /// Marks a node as one that dummies refer to. In the games every node a dummy names as its parent or attach bone
    /// has this flag, and no other node does. A node marked as unused (Disabled) loses that mark, as it is never
    /// combined with the others.
    /// </summary>
    public static void MarkDummyOwner(FLVER.Node node, IndexChangeRecord record)
    {
        var old = node.Flags;
        var flags = (old & ~FLVER.Node.NodeFlags.Disabled) | FLVER.Node.NodeFlags.DummyOwner;

        if (flags == old)
            return;

        node.Flags = flags;
        record.Record(() => node.Flags = old);
    }

    private static bool IsSelfOrDescendant(List<FLVER.Node> nodes, int index, int candidate)
    {
        var visited = new HashSet<int>();

        for (var i = candidate; IsValid(i, nodes.Count) && visited.Add(i); i = nodes[i].ParentIndex)
        {
            if (i == index)
                return true;
        }

        return false;
    }

    private static void SetNextSibling(FLVER.Node node, short value, IndexChangeRecord record)
    {
        var old = node.NextSiblingIndex;
        node.NextSiblingIndex = value;
        record.Record(() => node.NextSiblingIndex = old);
    }

    private static void SetPreviousSibling(FLVER.Node node, short value, IndexChangeRecord record)
    {
        var old = node.PreviousSiblingIndex;
        node.PreviousSiblingIndex = value;
        record.Record(() => node.PreviousSiblingIndex = old);
    }

    private static void SetFirstChild(FLVER.Node node, short value, IndexChangeRecord record)
    {
        var old = node.FirstChildIndex;
        node.FirstChildIndex = value;
        record.Record(() => node.FirstChildIndex = old);
    }
    #endregion

    #region Remap
    /// <summary>
    /// Rewrites every material and node reference in the model so it points at the same material or node as it did
    /// in the old lists. References to deleted entries are left as they are, which is why deletion of referenced
    /// entries is refused beforehand.
    /// </summary>
    public static IndexChangeRecord Remap(ModelContainer container,
        IReadOnlyList<FLVER2.Material> oldMaterials, IReadOnlyList<FLVER.Node> oldNodes)
    {
        var record = new IndexChangeRecord();

        var materialMap = BuildMap(oldMaterials, GetMaterials(container));
        var nodeMap = BuildMap(oldNodes, GetNodes(container));

        if (materialMap == null && nodeMap == null)
            return record;

        var flver = container.Flver;

        foreach (var mesh in GetMeshes(container))
        {
            if (materialMap != null)
            {
                var old = mesh.MaterialIndex;
                var mapped = Map(materialMap, old);

                if (mapped != old)
                {
                    mesh.MaterialIndex = mapped;
                    record.Record(() => mesh.MaterialIndex = old);
                }
            }

            if (nodeMap == null)
                continue;

            var oldNode = mesh.NodeIndex;
            var mappedNode = Map(nodeMap, oldNode);

            if (mappedNode != oldNode)
            {
                mesh.NodeIndex = mappedNode;
                record.Record(() => mesh.NodeIndex = oldNode);
            }

            var oldBoneIndices = mesh.BoneIndices.ToList();
            var newBoneIndices = oldBoneIndices.Select(b => Map(nodeMap, b)).ToList();

            if (!oldBoneIndices.SequenceEqual(newBoneIndices))
            {
                var boneIndices = mesh.BoneIndices;

                boneIndices.Clear();
                boneIndices.AddRange(newBoneIndices);

                record.Record(() =>
                {
                    boneIndices.Clear();
                    boneIndices.AddRange(oldBoneIndices);
                });
            }

            if (flver != null && !UsesPerMeshBoneTable(flver))
            {
                RemapVertices(flver, mesh, nodeMap, record);
            }
        }

        if (nodeMap == null)
            return record;

        foreach (var dummy in GetDummies(container))
        {
            var oldParent = dummy.ParentBoneIndex;
            var oldAttach = dummy.AttachBoneIndex;

            var newParent = (short)Map(nodeMap, oldParent);
            var newAttach = (short)Map(nodeMap, oldAttach);

            if (newParent != oldParent || newAttach != oldAttach)
            {
                dummy.ParentBoneIndex = newParent;
                dummy.AttachBoneIndex = newAttach;

                record.Record(() =>
                {
                    dummy.ParentBoneIndex = oldParent;
                    dummy.AttachBoneIndex = oldAttach;
                });
            }
        }

        foreach (var node in GetNodes(container))
        {
            var oldParent = node.ParentIndex;
            var oldChild = node.FirstChildIndex;
            var oldNext = node.NextSiblingIndex;
            var oldPrevious = node.PreviousSiblingIndex;

            var newParent = (short)Map(nodeMap, oldParent);
            var newChild = (short)Map(nodeMap, oldChild);
            var newNext = (short)Map(nodeMap, oldNext);
            var newPrevious = (short)Map(nodeMap, oldPrevious);

            if (newParent != oldParent || newChild != oldChild || newNext != oldNext || newPrevious != oldPrevious)
            {
                node.ParentIndex = newParent;
                node.FirstChildIndex = newChild;
                node.NextSiblingIndex = newNext;
                node.PreviousSiblingIndex = newPrevious;

                record.Record(() =>
                {
                    node.ParentIndex = oldParent;
                    node.FirstChildIndex = oldChild;
                    node.NextSiblingIndex = oldNext;
                    node.PreviousSiblingIndex = oldPrevious;
                });
            }
        }

        var skeletons = GetSkeletons(container);

        if (skeletons != null)
        {
            foreach (var bone in skeletons.BaseSkeleton.Concat(skeletons.AllSkeletons))
            {
                var old = bone.NodeIndex;
                var mapped = Map(nodeMap, old);

                if (mapped != old)
                {
                    bone.NodeIndex = mapped;
                    record.Record(() => bone.NodeIndex = old);
                }
            }
        }

        return record;
    }

    private static void RemapVertices(FLVER2 flver, FLVER2.Mesh mesh, int[] nodeMap, IndexChangeRecord record)
    {
        var vertices = mesh.Vertices;

        if (UsesBoneIndices(flver, mesh))
        {
            FLVER.VertexBoneIndices[] before = null;

            for (var v = 0; v < vertices.Count; v++)
            {
                var indices = vertices[v].BoneIndices;
                var changed = false;

                for (var k = 0; k < 4; k++)
                {
                    var mapped = Map(nodeMap, indices[k]);

                    if (mapped != indices[k])
                    {
                        indices[k] = mapped;
                        changed = true;
                    }
                }

                if (!changed)
                    continue;

                before ??= vertices.Select(e => e.BoneIndices).ToArray();
                vertices[v].BoneIndices = indices;
            }

            if (before != null)
            {
                record.Record(() =>
                {
                    for (var v = 0; v < before.Length && v < vertices.Count; v++)
                    {
                        vertices[v].BoneIndices = before[v];
                    }
                });
            }
        }
        else if (UsesNormalW(flver, mesh))
        {
            int[] before = null;

            for (var v = 0; v < vertices.Count; v++)
            {
                var mapped = Map(nodeMap, vertices[v].NormalW);

                if (mapped == vertices[v].NormalW)
                    continue;

                before ??= vertices.Select(e => e.NormalW).ToArray();
                vertices[v].NormalW = mapped;
            }

            if (before != null)
            {
                record.Record(() =>
                {
                    for (var v = 0; v < before.Length && v < vertices.Count; v++)
                    {
                        vertices[v].NormalW = before[v];
                    }
                });
            }
        }
    }

    /// <summary>
    /// Maps each old position to the new position of the same object, or -1 if it was removed.
    /// Returns null when nothing moved.
    /// </summary>
    private static int[] BuildMap<T>(IReadOnlyList<T> oldList, IReadOnlyList<T> newList) where T : class
    {
        var newPositions = new Dictionary<T, int>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < newList.Count; i++)
        {
            newPositions.TryAdd(newList[i], i);
        }

        var map = new int[oldList.Count];
        var isIdentity = true;

        for (var i = 0; i < oldList.Count; i++)
        {
            map[i] = newPositions.TryGetValue(oldList[i], out var position) ? position : -1;

            if (map[i] != i)
                isIdentity = false;
        }

        return isIdentity ? null : map;
    }

    private static int Map(int[] map, int index)
    {
        if (index < 0 || index >= map.Length)
            return index;

        return map[index] < 0 ? index : map[index];
    }

    private static bool IsValid(int index, int count)
    {
        return index >= 0 && index < count;
    }
    #endregion
}
