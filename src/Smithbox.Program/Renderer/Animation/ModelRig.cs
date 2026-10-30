#nullable enable
using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
///     How the vertices of a mesh follow the skeleton.
/// </summary>
public enum MeshSkinMode
{
    // Weighted vertices stored in model space
    Skinned,

    // Each vertex is stored in the space of the single bone named by its normal W
    RigidNormalW,

    // The whole mesh is stored in the space of the default bone of the mesh
    RigidMesh
}

/// <summary>
///     Per vertex skin data, packed into the same storage buffer as the bone matrices so that
///     the vertex shader can fetch it with gl_VertexIndex. Static geometry stays unaffected:
///     no vertex layout changes, and only models that are being animated pay for it.
/// </summary>
public struct SkinVertex
{
    // Bone indices 0 and 1, then 2 and 3, 16 bits each
    public uint Indices01;
    public uint Indices23;

    // Four unorm8 weights
    public uint Weights;
    public uint Padding;
}

public sealed class MeshRig
{
    public MeshSkinMode Mode;
    public int NodeIndex = -1;
    public SkinVertex[]? Vertices;
}

/// <summary>
///     The bones and skin of one FLVER, read separately from the renderer: the renderer drops bone weights
///     when it fills its vertex buffers, and keeps nothing but the GPU copy of a loaded model.
/// </summary>
public sealed class ModelRig
{
    public string[] NodeNames = Array.Empty<string>();
    public int[] NodeParents = Array.Empty<int>();
    public Matrix4x4[] NodeLocal = Array.Empty<Matrix4x4>();
    public Matrix4x4[] NodeReferenceWorld = Array.Empty<Matrix4x4>();
    public Matrix4x4[] NodeInverseReferenceWorld = Array.Empty<Matrix4x4>();

    // Parents always come before their children
    public int[] NodeOrder = Array.Empty<int>();

    public MeshRig[] Meshes = Array.Empty<MeshRig>();

    /// <summary>
    ///     Reads the rig of the named model from its binder. Returns null if the binder has no such model.
    /// </summary>
    public static ModelRig? Read(AnimationPackage package, Memory<byte> containerData, string modelName)
    {
        using BinderReader binder = AnimationPackage.OpenBinder(package.Game, containerData);

        BinderFileHeader? match = null;
        foreach (BinderFileHeader file in binder.Files)
        {
            var name = Path.GetFileName(file.Name.Replace('\\', '/'));
            if (!name.Contains(".flver", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (name.Split('.')[0].Equals(modelName, StringComparison.OrdinalIgnoreCase))
            {
                match = file;
                break;
            }

            match ??= file;
        }

        if (match == null)
        {
            return null;
        }

        byte[] data = binder.ReadFile(match).ToArray();
        if (DCX.Is(data))
        {
            data = DCX.Decompress(data).ToArray();
        }

        return FLVER2.Is(data) ? FromFlver(FLVER2.Read(data)) : FromFlver(FLVER0.Read(data));
    }

    public static ModelRig FromFlver(FLVER2 flver)
    {
        ModelRig rig = new();
        rig.ReadNodes(flver.Nodes);

        // Vertex bone indices refer to the bone table of the mesh in older versions
        var perMeshBoneTable = flver.Header.Version <= 0x2000D;

        rig.Meshes = new MeshRig[flver.Meshes.Count];
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            FLVER2.Mesh mesh = flver.Meshes[i];

            var usesBoneIndices = mesh.VertexBuffers
                .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
                .Any(l => l.Semantic == FLVER.LayoutSemantic.BoneIndices);

            var usesNormalW = mesh.VertexBuffers
                .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
                .Any(l => l.Semantic == FLVER.LayoutSemantic.Normal && (l.Type == FLVER.LayoutType.UByte4 ||
                    l.Type == FLVER.LayoutType.Byte4E || l.Type == FLVER.LayoutType.Color));

            rig.Meshes[i] = rig.BuildMesh(mesh.Dynamic != 0, mesh.NodeIndex, mesh.Vertices, mesh.BoneIndices,
                perMeshBoneTable, usesBoneIndices, usesNormalW);
        }

        return rig;
    }

    public static ModelRig FromFlver(FLVER0 flver)
    {
        ModelRig rig = new();
        rig.ReadNodes(flver.Nodes);

        rig.Meshes = new MeshRig[flver.Meshes.Count];
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            FLVER0.Mesh mesh = flver.Meshes[i];
            List<int> boneTable = mesh.BoneIndices.Select(b => (int)b).ToList();

            rig.Meshes[i] = rig.BuildMesh(mesh.Dynamic != 0, mesh.NodeIndex, mesh.Vertices, boneTable, true,
                mesh.UseBoneWeights, false);
        }

        return rig;
    }

    private void ReadNodes(List<FLVER.Node> nodes)
    {
        var count = nodes.Count;

        NodeNames = nodes.Select(n => n.Name).ToArray();
        NodeParents = nodes.Select(n => (int)n.ParentIndex).ToArray();
        NodeLocal = nodes.Select(n => n.ComputeLocalTransform()).ToArray();
        NodeReferenceWorld = new Matrix4x4[count];
        NodeInverseReferenceWorld = new Matrix4x4[count];

        for (var i = 0; i < count; i++)
        {
            NodeReferenceWorld[i] = Utils.GetBoneWorldMatrix(nodes[i], nodes);
            Matrix4x4.Invert(NodeReferenceWorld[i], out NodeInverseReferenceWorld[i]);
        }

        NodeOrder = SortParentsFirst(NodeParents);
    }

    private MeshRig BuildMesh(bool dynamic, int nodeIndex, List<FLVER.Vertex> vertices, List<int> boneTable,
        bool perMeshBoneTable, bool usesBoneIndices, bool usesNormalW)
    {
        if (!dynamic)
        {
            return new MeshRig
            {
                Mode = usesNormalW ? MeshSkinMode.RigidNormalW : MeshSkinMode.RigidMesh,
                NodeIndex = nodeIndex
            };
        }

        var nodeCount = NodeNames.Length;
        var skin = new SkinVertex[vertices.Count];

        Span<int> indices = stackalloc int[4];
        Span<float> weights = stackalloc float[4];

        for (var v = 0; v < skin.Length; v++)
        {
            FLVER.Vertex vert = vertices[v];

            for (var j = 0; j < 4; j++)
            {
                weights[j] = vert.BoneWeights[j];
                indices[j] = vert.BoneIndices[j];
            }

            // Vertices without bone indices name a single bone through their normal W
            if (!usesBoneIndices)
            {
                indices[0] = vert.NormalW;
                weights[0] = 1;
                weights[1] = weights[2] = weights[3] = 0;
            }
            else if (weights[0] == 0 && weights[1] == 0 && weights[2] == 0 && weights[3] == 0)
            {
                weights[0] = 1;
            }

            for (var j = 0; j < 4; j++)
            {
                if (perMeshBoneTable)
                {
                    indices[j] = indices[j] >= 0 && indices[j] < boneTable.Count ? boneTable[indices[j]] : -1;
                }

                if (indices[j] < 0 || indices[j] >= nodeCount)
                {
                    indices[j] = 0;
                    weights[j] = 0;
                }
            }

            skin[v] = PackVertex(indices, weights);
        }

        return new MeshRig { Mode = MeshSkinMode.Skinned, NodeIndex = nodeIndex, Vertices = skin };
    }

    private static SkinVertex PackVertex(ReadOnlySpan<int> indices, ReadOnlySpan<float> weights)
    {
        var total = weights[0] + weights[1] + weights[2] + weights[3];
        var scale = total > 0 ? 255f / total : 0;

        uint packed = 0;
        for (var i = 0; i < 4; i++)
        {
            packed |= (uint)Math.Clamp((int)MathF.Round(weights[i] * scale), 0, 255) << (i * 8);
        }

        return new SkinVertex
        {
            Indices01 = (uint)indices[0] | (uint)indices[1] << 16,
            Indices23 = (uint)indices[2] | (uint)indices[3] << 16,
            Weights = packed
        };
    }

    public static int[] SortParentsFirst(IReadOnlyList<int> parents)
    {
        var order = new List<int>(parents.Count);
        var visited = new bool[parents.Count];

        void Visit(int i)
        {
            if (visited[i])
            {
                return;
            }

            visited[i] = true;
            if (parents[i] >= 0 && parents[i] < parents.Count)
            {
                Visit(parents[i]);
            }

            order.Add(i);
        }

        for (var i = 0; i < parents.Count; i++)
        {
            Visit(i);
        }

        return order.ToArray();
    }
}

/// <summary>
///     Evaluates a clip into bone matrices for a <see cref="ModelRig" />.
///     FLVER bones follow the Havok bone of the same name, and any others hang off their parent.
/// </summary>
public sealed class ModelPose
{
    private readonly ModelRig _rig;
    private readonly HavokSkeletonData _skeleton;

    private readonly int[] _boneOrder;
    private readonly int[] _nodeToBone;

    private readonly NewBlendableTransform[] _reference;
    private readonly Matrix4x4[] _boneRotationTranslation;
    private readonly Vector3[] _boneScale;
    private readonly Matrix4x4[] _boneWorld;

    /// <summary>
    ///     Current model space matrix of every FLVER node.
    /// </summary>
    public Matrix4x4[] NodeWorld { get; }

    /// <summary>
    ///     Bones that the FLVER shares with the Havok skeleton by name.
    /// </summary>
    public int MappedNodeCount { get; }

    public ModelPose(ModelRig rig, HavokSkeletonData skeleton)
    {
        _rig = rig;
        _skeleton = skeleton;

        var boneCount = skeleton.BoneCount;
        _boneOrder = ModelRig.SortParentsFirst(skeleton.Parents.Select(p => (int)p).ToArray());
        _reference = skeleton.ReferencePose.Select(t => t.ToBlendable()).ToArray();
        _boneRotationTranslation = new Matrix4x4[boneCount];
        _boneScale = new Vector3[boneCount];
        _boneWorld = new Matrix4x4[boneCount];

        var bonesByName = new Dictionary<string, int>();
        for (var i = 0; i < boneCount; i++)
        {
            bonesByName.TryAdd(skeleton.Names[i], i);
        }

        _nodeToBone = new int[rig.NodeNames.Length];
        for (var i = 0; i < _nodeToBone.Length; i++)
        {
            _nodeToBone[i] = bonesByName.TryGetValue(rig.NodeNames[i], out var bone) ? bone : -1;
            MappedNodeCount += _nodeToBone[i] >= 0 ? 1 : 0;
        }

        NodeWorld = new Matrix4x4[rig.NodeNames.Length];
        Array.Copy(rig.NodeReferenceWorld, NodeWorld, NodeWorld.Length);
    }

    public void Apply(HavokClip? clip, float frame)
    {
        foreach (var i in _boneOrder)
        {
            NewBlendableTransform local = SampleLocal(clip, i, frame);
            Matrix4x4 rotationTranslation = local.GetMatrix();
            Vector3 scale = local.Scale;

            int parent = _skeleton.Parents[i];
            if (parent >= 0 && parent < _boneWorld.Length)
            {
                rotationTranslation *= _boneRotationTranslation[parent];
                scale *= _boneScale[parent];
            }

            // Scale accumulates apart from the hierarchy, so that it does not shear the children
            _boneRotationTranslation[i] = rotationTranslation;
            _boneScale[i] = scale;
            _boneWorld[i] = Matrix4x4.CreateScale(scale) * rotationTranslation;
        }

        foreach (var i in _rig.NodeOrder)
        {
            var bone = _nodeToBone[i];
            if (bone >= 0)
            {
                NodeWorld[i] = _boneWorld[bone];
                continue;
            }

            var parent = _rig.NodeParents[i];
            NodeWorld[i] = parent >= 0 ? _rig.NodeLocal[i] * NodeWorld[parent] : _rig.NodeLocal[i];
        }
    }

    private NewBlendableTransform SampleLocal(HavokClip? clip, int bone, float frame)
    {
        NewBlendableTransform reference = _reference[bone];

        var track = clip != null && bone < clip.BoneToTrack.Length ? clip.BoneToTrack[bone] : -1;
        if (track < 0)
        {
            return reference;
        }

        NewBlendableTransform sampled = clip!.Sample(track, frame);

        return clip.BlendMode switch
        {
            HavokBlendMode.AdditiveChildSpace => new NewBlendableTransform
            {
                Translation = reference.Translation + sampled.Translation,
                Rotation = reference.Rotation * sampled.Rotation,
                Scale = reference.Scale * sampled.Scale
            },
            HavokBlendMode.AdditiveParentSpace => new NewBlendableTransform
            {
                Translation = reference.Translation + sampled.Translation,
                Rotation = sampled.Rotation * reference.Rotation,
                Scale = reference.Scale * sampled.Scale
            },
            _ => sampled
        };
    }
}
