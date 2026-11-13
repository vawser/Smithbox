using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

// This file depends on SoulsFormats only, so Claude/Tools/ModelCheckSurvey can compile it as it is and run the same
// rules over the vanilla models. Every rule must give no errors on vanilla data, or it would stop every save.

public enum ModelCheckSeverity
{
    /// <summary>The save fails, or writes a model the game can't use</summary>
    Error,

    /// <summary>Saves, but is probably a mistake</summary>
    Warning,

    /// <summary>Worth knowing; vanilla models do it too</summary>
    Info
}

public enum ModelCheckKind
{
    /// <summary>A mesh's MaterialIndex names no material</summary>
    MaterialOutOfRange,

    /// <summary>A field that indexes the node list names no node</summary>
    NodeOutOfRange,

    /// <summary>A vertex buffer's LayoutIndex names no buffer layout</summary>
    LayoutOutOfRange,

    /// <summary>Vertices name a slot past the end of their mesh's bone table</summary>
    VertexBoneTableOutOfRange,

    /// <summary>Vertices name a node that doesn't exist</summary>
    VertexNodeOutOfRange,

    /// <summary>A face set names a vertex the mesh doesn't have</summary>
    FaceIndexOutOfRange,

    /// <summary>A face set's indices and the vertex count don't fit together (vertices without buffers, etc.)</summary>
    VertexBufferMissing,

    /// <summary>Following a node's parents leads back to the node</summary>
    ParentLoop,

    /// <summary>A child or sibling link disagrees with the parent links</summary>
    SiblingChain,

    /// <summary>The version writes a skeleton set and the model has none</summary>
    MissingSkeletonSet,

    /// <summary>A skeleton set bone's link names no bone of its skeleton</summary>
    SkeletonLinkOutOfRange,

    /// <summary>Demon's Souls models: more than FLVER0 can store</summary>
    Flver0Limit,

    /// <summary>Two dummies share a reference ID</summary>
    DuplicateDummyReferenceID
}

/// <summary>
/// One problem found by <see cref="ModelCheckRules" />. <see cref="Target" /> is the FLVER object at fault (a mesh,
/// node, dummy or the skeleton set), or null for the model, and <see cref="TargetIndex" /> its position in its list.
/// <see cref="Value" /> is the value at fault, <see cref="Limit" /> the count it should be below (or the object it
/// clashes with, for duplicates), and <see cref="Count" /> how many vertices or indices share the problem.
/// </summary>
public sealed record ModelCheckFinding(
    ModelCheckKind Kind,
    ModelCheckSeverity Severity,
    object Target,
    int TargetIndex,
    string Field,
    long Value,
    int Limit,
    int Count = 1);

/// <summary>
/// Checks a FLVER for what would break its save, or the model in game. Used by the Model Editor before saving (see
/// <c>ModelCheck</c>). Plan and survey results: Documentation/Model Editor Second Review.md, R4.
/// </summary>
public static class ModelCheckRules
{
    /// <summary>The first version that writes a skeleton set</summary>
    public const int SkeletonSetVersion = 0x2001A;

    /// <summary>FLVER0 meshes hold this many bones in their bone table</summary>
    public const int Flver0MaxBoneCount = FLVER0.Mesh.MaxBoneCount;

    /// <param name="isFlver0">The model is saved as a FLVER0 (Demon's Souls), so FLVER0's limits apply</param>
    public static List<ModelCheckFinding> Run(FLVER2 flver, bool isFlver0)
    {
        var findings = new List<ModelCheckFinding>();

        if (flver == null)
            return findings;

        CheckSkeletonSet(flver, findings);
        CheckMeshes(flver, isFlver0, findings);
        CheckNodes(flver, findings);
        CheckDummies(flver, findings);

        if (isFlver0)
        {
            CheckFlver0Limits(flver, findings);
        }

        return findings;
    }

    private static bool InRange(long index, int count) => index >= 0 && index < count;

    // -1 means "none" for these
    private static bool InRangeOrNone(long index, int count) => index == -1 || InRange(index, count);

    #region Skeleton Set
    private static void CheckSkeletonSet(FLVER2 flver, List<ModelCheckFinding> findings)
    {
        var nodeCount = flver.Nodes.Count;
        var skeletons = flver.Skeletons;

        if (skeletons == null)
        {
            // FLVER2.Write calls Skeletons.Write from this version on
            if (flver.Header.Version >= SkeletonSetVersion)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.MissingSkeletonSet, ModelCheckSeverity.Error,
                    null, -1, "Skeletons", flver.Header.Version, SkeletonSetVersion));
            }

            return;
        }

        CheckSkeleton(skeletons, skeletons.BaseSkeleton, "BaseSkeleton", nodeCount, findings);
        CheckSkeleton(skeletons, skeletons.AllSkeletons, "AllSkeletons", nodeCount, findings);
    }

    private static void CheckSkeleton(FLVER2.SkeletonSet set, List<FLVER2.SkeletonSet.Bone> bones, string listName,
        int nodeCount, List<ModelCheckFinding> findings)
    {
        if (bones == null)
            return;

        for (var i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];

            if (!InRange(bone.NodeIndex, nodeCount))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Error,
                    set, i, $"{listName}[{i}].NodeIndex", bone.NodeIndex, nodeCount));
            }

            void Link(string field, short value)
            {
                if (!InRangeOrNone(value, bones.Count))
                {
                    findings.Add(new ModelCheckFinding(ModelCheckKind.SkeletonLinkOutOfRange, ModelCheckSeverity.Error,
                        set, i, $"{listName}[{i}].{field}", value, bones.Count));
                }
            }

            Link("ParentIndex", bone.ParentIndex);
            Link("FirstChildIndex", bone.FirstChildIndex);
            Link("NextSiblingIndex", bone.NextSiblingIndex);
            Link("PreviousSiblingIndex", bone.PreviousSiblingIndex);
        }
    }
    #endregion

    #region Meshes
    /// <summary>Vertex bone indices name a slot of the mesh's bone table in these versions, and a node after them</summary>
    public static bool UsesPerMeshBoneTable(FLVER2 flver) => flver.Header.Version <= 0x2000D;

    private static void CheckMeshes(FLVER2 flver, bool isFlver0, List<ModelCheckFinding> findings)
    {
        var materialCount = flver.Materials.Count;
        var nodeCount = flver.Nodes.Count;
        var layoutCount = flver.BufferLayouts.Count;

        for (var m = 0; m < flver.Meshes.Count; m++)
        {
            var mesh = flver.Meshes[m];

            if (!InRange(mesh.MaterialIndex, materialCount))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.MaterialOutOfRange, ModelCheckSeverity.Error,
                    mesh, m, "MaterialIndex", mesh.MaterialIndex, materialCount));
            }

            // The bone table: the first entry at fault, and how many there are
            var badBones = mesh.BoneIndices.Where(b => !InRange(b, nodeCount)).ToList();

            if (badBones.Count > 0)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Error,
                    mesh, m, "BoneIndices", badBones[0], nodeCount, badBones.Count));
            }

            var layoutsValid = true;

            for (var b = 0; b < mesh.VertexBuffers.Count; b++)
            {
                var layoutIndex = mesh.VertexBuffers[b].LayoutIndex;

                if (!InRange(layoutIndex, layoutCount))
                {
                    layoutsValid = false;
                    findings.Add(new ModelCheckFinding(ModelCheckKind.LayoutOutOfRange, ModelCheckSeverity.Error,
                        mesh, m, $"VertexBuffers[{b}].LayoutIndex", layoutIndex, layoutCount));
                }
            }

            // Vertices with nothing to write them through
            if (mesh.VertexBuffers.Count == 0 && mesh.Vertices.Count > 0)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.VertexBufferMissing, ModelCheckSeverity.Error,
                    mesh, m, "VertexBuffers", 0, 0, mesh.Vertices.Count));
            }

            CheckFaceSets(mesh, m, findings);

            var vertexBones = layoutsValid && CheckVertexBones(flver, mesh, m, findings);

            // NodeIndex places a mesh whose vertices don't name their bones. Rigid meshes in every FLVER2 game keep
            // a NodeIndex past the node list (634 in 148 of 166 surveyed ER assets) while their vertices name the
            // bone through the normal's W, so it is only checked where it is used. Even there vanilla has some
            // (56 DS3 map pieces with one node, 6 ER asset meshes), so it is only a note.
            if (!vertexBones && !InRangeOrNone(mesh.NodeIndex, nodeCount))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Info,
                    mesh, m, "NodeIndex", mesh.NodeIndex, nodeCount));
            }
        }
    }

    private static void CheckFaceSets(FLVER2.Mesh mesh, int meshIndex, List<ModelCheckFinding> findings)
    {
        var vertexCount = mesh.Vertices.Count;

        // 0xFFFF restarts a triangle strip while the mesh has fewer vertices than that
        var allowRestart = vertexCount < 0xFFFF;

        for (var f = 0; f < mesh.FaceSets.Count; f++)
        {
            var indices = mesh.FaceSets[f].Indices;
            var bad = 0;
            var first = 0L;

            for (var i = 0; i < indices.Count; i++)
            {
                var index = indices[i];

                if (index >= 0 && index < vertexCount)
                    continue;

                if (allowRestart && (index == 0xFFFF || index == -1))
                    continue;

                if (bad == 0)
                    first = index;

                bad++;
            }

            if (bad > 0)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.FaceIndexOutOfRange, ModelCheckSeverity.Error,
                    mesh, meshIndex, $"FaceSets[{f}].Indices", first, vertexCount, bad));
            }
        }
    }

    private static IEnumerable<FLVER.LayoutMember> GetLayoutMembers(FLVER2 flver, FLVER2.Mesh mesh)
    {
        return mesh.VertexBuffers
            .Where(b => b.LayoutIndex >= 0 && b.LayoutIndex < flver.BufferLayouts.Count)
            .SelectMany(b => flver.BufferLayouts[b.LayoutIndex]);
    }

    /// <summary>
    /// The bones the vertices name: through the mesh's bone table in the older versions, directly in the node list
    /// after them. Only weighted slots count (all four when the layout has no weights). Vertices without bone
    /// indices name one bone through their normal's W, on rigid meshes, as the viewport reads them. Returns whether
    /// the vertices name their bones.
    /// </summary>
    private static bool CheckVertexBones(FLVER2 flver, FLVER2.Mesh mesh, int meshIndex, List<ModelCheckFinding> findings)
    {
        if (mesh.Vertices.Count == 0)
            return false;

        var members = GetLayoutMembers(flver, mesh).ToList();

        var hasBoneIndices = members.Any(l => l.Semantic == FLVER.LayoutSemantic.BoneIndices);
        var hasBoneWeights = members.Any(l => l.Semantic == FLVER.LayoutSemantic.BoneWeights);
        var hasNormalW = !hasBoneIndices && mesh.Dynamic == 0 && members.Any(l =>
            l.Semantic == FLVER.LayoutSemantic.Normal &&
            (l.Type == FLVER.LayoutType.UByte4 || l.Type == FLVER.LayoutType.Byte4E));

        if (!hasBoneIndices && !hasNormalW)
            return false;

        // An empty bone table in the older versions means the vertices aren't bound through it
        var perMeshTable = UsesPerMeshBoneTable(flver);

        if (perMeshTable && mesh.BoneIndices.Count == 0)
            return false;

        var limit = perMeshTable ? mesh.BoneIndices.Count : flver.Nodes.Count;
        var kind = perMeshTable ? ModelCheckKind.VertexBoneTableOutOfRange : ModelCheckKind.VertexNodeOutOfRange;

        var bad = 0;
        var first = 0L;

        foreach (var vertex in mesh.Vertices)
        {
            if (hasBoneIndices)
            {
                for (var k = 0; k < 4; k++)
                {
                    if (hasBoneWeights && vertex.BoneWeights[k] == 0)
                        continue;

                    var index = vertex.BoneIndices[k];

                    if (InRange(index, limit))
                        continue;

                    if (bad == 0)
                        first = index;

                    bad++;
                    break;
                }
            }
            else
            {
                if (InRange(vertex.NormalW, limit))
                    continue;

                if (bad == 0)
                    first = vertex.NormalW;

                bad++;
            }
        }

        if (bad > 0)
        {
            findings.Add(new ModelCheckFinding(kind, ModelCheckSeverity.Error,
                mesh, meshIndex, hasBoneIndices ? "Vertices.BoneIndices" : "Vertices.NormalW", first, limit, bad));
        }

        return true;
    }
    #endregion

    #region Nodes
    private static void CheckNodes(FLVER2 flver, List<ModelCheckFinding> findings)
    {
        var nodes = flver.Nodes;
        var count = nodes.Count;

        var linksValid = true;

        for (var i = 0; i < count; i++)
        {
            var node = nodes[i];

            void Link(string field, short value)
            {
                if (InRangeOrNone(value, count))
                    return;

                linksValid = false;
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Error,
                    node, i, field, value, count));
            }

            Link("ParentIndex", node.ParentIndex);
            Link("FirstChildIndex", node.FirstChildIndex);
            Link("NextSiblingIndex", node.NextSiblingIndex);
            Link("PreviousSiblingIndex", node.PreviousSiblingIndex);
        }

        // The rest follows the links, so needs them in range
        if (!linksValid)
            return;

        var looped = CheckParentLoops(nodes, findings);

        if (!looped)
        {
            CheckSiblingChains(nodes, findings);
        }
    }

    /// <summary>Following a node's parents leads back to it. Returns whether any did.</summary>
    private static bool CheckParentLoops(List<FLVER.Node> nodes, List<ModelCheckFinding> findings)
    {
        var looped = false;

        for (var i = 0; i < nodes.Count; i++)
        {
            var steps = 0;

            for (int p = nodes[i].ParentIndex; p != -1; p = nodes[p].ParentIndex)
            {
                if (p == i || ++steps > nodes.Count)
                {
                    looped = true;
                    findings.Add(new ModelCheckFinding(ModelCheckKind.ParentLoop, ModelCheckSeverity.Error,
                        nodes[i], i, "ParentIndex", nodes[i].ParentIndex, nodes.Count));
                    break;
                }
            }
        }

        return looped;
    }

    /// <summary>
    /// The child and sibling links must agree with the parent links: a node's first child has it as parent and no
    /// previous sibling, siblings share a parent and link back to each other, and every child can be reached from
    /// its parent's first child. One finding per node, for the first link that is wrong. Vanilla models have none,
    /// but nothing shows the game needs these links (ParentIndex places the nodes), so they are warnings.
    /// </summary>
    private static void CheckSiblingChains(List<FLVER.Node> nodes, List<ModelCheckFinding> findings)
    {
        var count = nodes.Count;
        var flagged = new HashSet<int>();

        void Flag(int i, string field, short value)
        {
            if (flagged.Add(i))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.SiblingChain, ModelCheckSeverity.Warning,
                    nodes[i], i, field, value, count));
            }
        }

        for (var i = 0; i < count; i++)
        {
            var node = nodes[i];

            if (node.FirstChildIndex != -1)
            {
                var child = nodes[node.FirstChildIndex];

                if (child.ParentIndex != i || child.PreviousSiblingIndex != -1)
                    Flag(i, "FirstChildIndex", node.FirstChildIndex);
            }

            if (node.NextSiblingIndex != -1)
            {
                var next = nodes[node.NextSiblingIndex];

                if (next.ParentIndex != node.ParentIndex || next.PreviousSiblingIndex != i)
                    Flag(i, "NextSiblingIndex", node.NextSiblingIndex);
            }

            if (node.PreviousSiblingIndex != -1)
            {
                var previous = nodes[node.PreviousSiblingIndex];

                if (previous.ParentIndex != node.ParentIndex || previous.NextSiblingIndex != i)
                    Flag(i, "PreviousSiblingIndex", node.PreviousSiblingIndex);
            }
        }

        // Every child reachable from its parent's first child
        var reached = new bool[count];

        for (var p = 0; p < count; p++)
        {
            var steps = 0;

            for (int c = nodes[p].FirstChildIndex; c != -1 && steps <= count; c = nodes[c].NextSiblingIndex, steps++)
            {
                if (nodes[c].ParentIndex == p)
                    reached[c] = true;
            }
        }

        for (var i = 0; i < count; i++)
        {
            if (nodes[i].ParentIndex != -1 && !reached[i])
                Flag(i, "ParentIndex", nodes[i].ParentIndex);
        }
    }
    #endregion

    #region Dummies
    private static void CheckDummies(FLVER2 flver, List<ModelCheckFinding> findings)
    {
        var nodeCount = flver.Nodes.Count;
        var firstWithID = new Dictionary<int, int>();

        for (var d = 0; d < flver.Dummies.Count; d++)
        {
            var dummy = flver.Dummies[d];

            if (!InRangeOrNone(dummy.ParentBoneIndex, nodeCount))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Error,
                    dummy, d, "ParentBoneIndex", dummy.ParentBoneIndex, nodeCount));
            }

            if (!InRangeOrNone(dummy.AttachBoneIndex, nodeCount))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.NodeOutOfRange, ModelCheckSeverity.Error,
                    dummy, d, "AttachBoneIndex", dummy.AttachBoneIndex, nodeCount));
            }

            // Several dummies sharing an ID is how the games spread an effect or hitbox over points, so this is
            // only for information. Limit holds the first dummy with the ID.
            if (firstWithID.TryGetValue(dummy.ReferenceID, out var first))
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.DuplicateDummyReferenceID, ModelCheckSeverity.Info,
                    dummy, d, "ReferenceID", dummy.ReferenceID, first));
            }
            else
            {
                firstWithID[dummy.ReferenceID] = d;
            }
        }
    }
    #endregion

    #region FLVER0
    /// <summary>What the FLVER0 writer (Flver0Converter.ToFlver0) refuses</summary>
    private static void CheckFlver0Limits(FLVER2 flver, List<ModelCheckFinding> findings)
    {
        if (flver.Materials.Count > byte.MaxValue + 1)
        {
            findings.Add(new ModelCheckFinding(ModelCheckKind.Flver0Limit, ModelCheckSeverity.Error,
                null, -1, "Materials", flver.Materials.Count, byte.MaxValue + 1));
        }

        for (var m = 0; m < flver.Meshes.Count; m++)
        {
            var mesh = flver.Meshes[m];

            if (mesh.BoneIndices.Count > Flver0MaxBoneCount)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.Flver0Limit, ModelCheckSeverity.Error,
                    mesh, m, "BoneIndices", mesh.BoneIndices.Count, Flver0MaxBoneCount));
            }

            if (mesh.VertexBuffers.Count == 0 && mesh.Vertices.Count == 0)
            {
                findings.Add(new ModelCheckFinding(ModelCheckKind.Flver0Limit, ModelCheckSeverity.Error,
                    mesh, m, "VertexBuffers", 0, 1));
            }
        }
    }
    #endregion
}
