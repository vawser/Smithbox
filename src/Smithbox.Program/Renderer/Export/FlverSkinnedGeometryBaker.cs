using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Converts a FLVER2 mesh list into geometry suitable for a skinned glTF export: positions/normals/UV0/triangle
/// indices in bind-pose model space (mirroring <see cref="FlverStaticGeometryBaker"/>), plus a joint index and
/// weight per vertex so the result can be exported with a real glTF skin instead of baked-in transforms.
///
/// Every vertex ends up bound to at least one node (the FLVER's bone/dummy hierarchy, exposed as <c>flver.Nodes</c>):
/// genuinely skinned meshes (<see cref="FLVER2.Mesh.UseBoneWeights"/>) keep their own per-vertex bone indices/weights,
/// while rigid meshes (single fixed node, or a per-vertex node via NormalW) are given a single full-weight joint so
/// they still deform correctly if the resulting armature is posed.
/// </summary>
public static class FlverSkinnedGeometryBaker
{
    public class BakedSkinnedPrimitive
    {
        public string MaterialName;
        public List<Vector3> Positions;
        public List<Vector3> Normals;
        public List<Vector2> UVs;
        public List<int> Indices;
        public List<(ushort A, ushort B, ushort C, ushort D)> Joints;
        public List<Vector4> Weights;
        public string AlbedoVirtualPath;
        public string NormalVirtualPath;
        public string SpecularVirtualPath;
        public string MetallicVirtualPath;
        public string AlphaMode;
    }

    public class BakedSkinnedModel
    {
        public List<BakedSkinnedPrimitive> Primitives = new();
        public List<FLVER.Node> Nodes;
    }

    /// <summary>
    /// Returns null if the model has no more than one node to skin against, or every vertex ends up bound to
    /// that single node anyway (e.g. a MapPiece/architecture FLVER, or a simple static asset) - a skin in that
    /// case couldn't move anything differently than a plain rigid mesh, so it's not worth the extra glTF
    /// complexity. Callers should fall back to <see cref="FlverStaticGeometryBaker"/> in that case.
    /// </summary>
    public static BakedSkinnedModel Bake(ProjectEntry project, FLVER2 flver, string flverVirtPath)
    {
        // Cheap early-out before doing any per-vertex work: with 0 or 1 nodes there's nothing to pose regardless
        // of what the meshes reference.
        if (flver?.Meshes == null || flver.Nodes is not { Count: > 1 })
            return null;

        var nodes = flver.Nodes;
        var result = new BakedSkinnedModel { Nodes = nodes };

        // Per-node normal transform (inverse-transpose of the node's world matrix), cached across meshes/vertices.
        // See FlverStaticGeometryBaker for why this is needed instead of transforming normals like positions.
        var normalTransformCache = new Dictionary<int, Matrix4x4>();
        var mirroredCache = new Dictionary<int, bool>();

        Matrix4x4 GetNormalTransform(int nodeIndex)
        {
            if (normalTransformCache.TryGetValue(nodeIndex, out var cached))
                return cached;

            var worldMatrix = Utils.GetBoneWorldMatrix(nodes[nodeIndex], nodes);
            var normalMatrix = Matrix4x4.Invert(worldMatrix, out var inverted)
                ? Matrix4x4.Transpose(inverted)
                : worldMatrix;

            normalTransformCache[nodeIndex] = normalMatrix;
            return normalMatrix;
        }

        // A node transform with a negative determinant (mirrored/negative scale) flips triangle winding once
        // baked into world space - same correction as the static baker.
        bool IsNodeMirrored(int nodeIndex)
        {
            if (mirroredCache.TryGetValue(nodeIndex, out var cached))
                return cached;

            var mirrored = Utils.GetBoneWorldMatrix(nodes[nodeIndex], nodes).GetDeterminant() < 0;
            mirroredCache[nodeIndex] = mirrored;
            return mirrored;
        }

        foreach (var mesh in flver.Meshes)
        {
            if (mesh.Vertices == null || mesh.Vertices.Count == 0)
                continue;

            if (mesh.MaterialIndex < 0 || mesh.MaterialIndex >= flver.Materials.Count)
                continue;

            var material = flver.Materials[mesh.MaterialIndex];

            var indices = new List<int>();
            foreach (var faceSet in mesh.FaceSets)
            {
                if (faceSet.Indices.Count == 0)
                    continue;

                // Skip LOD/motion-blur facesets; only export the base mesh.
                if (faceSet.Flags != FLVER2.FaceSet.FSFlags.None && faceSet.Flags != FLVER2.FaceSet.FSFlags.EdgeCompressed)
                    continue;

                indices.AddRange(faceSet.Triangulate(true));
            }

            if (indices.Count == 0)
                continue;

            var useNormalWTransform = false;
            if (!mesh.UseBoneWeights)
            {
                var elements = mesh.VertexBuffers.SelectMany(b => flver.BufferLayouts[b.LayoutIndex]);
                useNormalWTransform = elements.Any(e =>
                    e.Semantic == FLVER.LayoutSemantic.Normal &&
                    (e.Type == FLVER.LayoutType.UByte4 || e.Type == FLVER.LayoutType.Byte4E));
            }

            // For a rigid (non-bone-weighted) mesh bound to a single node, both position and normal are
            // transformed the same way for every vertex - cache it once per mesh instead of per vertex.
            Matrix4x4? rigidMeshTransform = null;
            Matrix4x4? rigidMeshNormalTransform = null;
            var rigidNodeValid = mesh.NodeIndex >= 0 && mesh.NodeIndex < nodes.Count;
            if (!mesh.UseBoneWeights && !useNormalWTransform && rigidNodeValid)
            {
                rigidMeshTransform = Utils.GetBoneWorldMatrix(nodes[mesh.NodeIndex], nodes);
                rigidMeshNormalTransform = GetNormalTransform(mesh.NodeIndex);
            }

            // Both FLVER.Vertex.BoneIndices AND NormalW (when used as a rigid single-bone bind) are indices into
            // this mesh's own local bone table (FLVER2.Mesh.BoneIndices), not global flver.Nodes indices - they
            // must be remapped through it. This mirrors the "HACK: Move normalW index from local to global bone
            // index" remap the live renderer applies for NormalW (FlverResource.cs) and the equivalent BoneIndices
            // remap it applies for real skinning - skipping it here previously left the joint pointing at an
            // unrelated node, which is why posing a bone wouldn't move geometry actually bound to a different one.
            int RemapLocalBoneIndex(int localIndex)
            {
                if (mesh.BoneIndices == null || mesh.BoneIndices.Count == 0)
                    return localIndex;

                if (localIndex < 0 || localIndex >= mesh.BoneIndices.Count)
                    return 0;

                return mesh.BoneIndices[localIndex];
            }

            ushort RemapLocalBoneIndexClamped(int localIndex)
                => (ushort)System.Math.Clamp(RemapLocalBoneIndex(localIndex), 0, nodes.Count - 1);

            var positions = new List<Vector3>(mesh.Vertices.Count);
            var normals = new List<Vector3>(mesh.Vertices.Count);
            var uvs = new List<Vector2>(mesh.Vertices.Count);
            var joints = new List<(ushort, ushort, ushort, ushort)>(mesh.Vertices.Count);
            var weights = new List<Vector4>(mesh.Vertices.Count);

            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                var normal = vertex.Normal;

                if (mesh.UseBoneWeights)
                {
                    // Already stored in bind-pose model space - no transform needed.
                    joints.Add((RemapLocalBoneIndexClamped(vertex.BoneIndices[0]), RemapLocalBoneIndexClamped(vertex.BoneIndices[1]),
                        RemapLocalBoneIndexClamped(vertex.BoneIndices[2]), RemapLocalBoneIndexClamped(vertex.BoneIndices[3])));
                    weights.Add(new Vector4(vertex.BoneWeights[0], vertex.BoneWeights[1],
                        vertex.BoneWeights[2], vertex.BoneWeights[3]));
                }
                else if (useNormalWTransform)
                {
                    var globalNode = RemapLocalBoneIndex(vertex.NormalW);
                    if (globalNode < 0 || globalNode >= nodes.Count)
                        globalNode = 0;

                    var t = Utils.GetBoneWorldMatrix(nodes[globalNode], nodes);
                    pos = Vector3.Transform(pos, t);
                    normal = Vector3.TransformNormal(normal, GetNormalTransform(globalNode));

                    joints.Add(((ushort)globalNode, 0, 0, 0));
                    weights.Add(new Vector4(1f, 0f, 0f, 0f));
                }
                else if (rigidMeshTransform.HasValue)
                {
                    pos = Vector3.Transform(pos, rigidMeshTransform.Value);
                    normal = Vector3.TransformNormal(normal, rigidMeshNormalTransform!.Value);

                    var jointIndex = (ushort)mesh.NodeIndex;
                    joints.Add((jointIndex, 0, 0, 0));
                    weights.Add(new Vector4(1f, 0f, 0f, 0f));
                }
                else
                {
                    // No valid node to bind to at all (shouldn't normally happen) - leave unskinned/unmoved
                    // rather than dropping the vertex.
                    joints.Add((0, 0, 0, 0));
                    weights.Add(new Vector4(1f, 0f, 0f, 0f));
                }

                positions.Add(pos);
                normals.Add(normal.LengthSquared() > 0.0001f ? Vector3.Normalize(normal) : normal);
                uvs.Add(vertex.UVs is { Count: > 0 } ? new Vector2(vertex.UVs[0].X, vertex.UVs[0].Y) : Vector2.Zero);
            }

            // Only relevant for the two rigid paths above - bone-weighted vertices are already in model space
            // and never get flipped here.
            if (useNormalWTransform)
            {
                for (var i = 0; i + 2 < indices.Count; i += 3)
                {
                    var nodeIndex = RemapLocalBoneIndex(mesh.Vertices[indices[i]].NormalW);
                    if (nodeIndex >= 0 && nodeIndex < nodes.Count && IsNodeMirrored(nodeIndex))
                    {
                        (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
                    }
                }
            }
            else if (rigidMeshTransform.HasValue && mesh.NodeIndex >= 0 && IsNodeMirrored(mesh.NodeIndex))
            {
                for (var i = 0; i + 2 < indices.Count; i += 3)
                {
                    (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);
                }
            }

            var resolvedTextures = FlverTextureResolver.Resolve(project, material, flverVirtPath);

            result.Primitives.Add(new BakedSkinnedPrimitive
            {
                MaterialName = string.IsNullOrEmpty(material.Name) ? material.MTD : material.Name,
                Positions = positions,
                Normals = normals,
                UVs = uvs,
                Indices = indices,
                Joints = joints,
                Weights = weights,
                AlbedoVirtualPath = resolvedTextures.AlbedoVirtualPath,
                NormalVirtualPath = resolvedTextures.NormalVirtualPath,
                SpecularVirtualPath = resolvedTextures.SpecularVirtualPath,
                MetallicVirtualPath = resolvedTextures.MetallicVirtualPath,
                AlphaMode = resolvedTextures.AlphaMode
            });
        }

        // Thorough check for the rarer case that slips past the node-count early-out: multiple nodes exist,
        // but every vertex that actually carries weight still resolves to the same single one (e.g. unused
        // dummy/attach nodes alongside one real rigid root). Skinning that wouldn't deform any differently
        // than a plain rigid mesh, so it's not worth exporting a skin for.
        if (result.Primitives.Count == 0 || !HasMultipleDistinctWeightedJoints(result.Primitives))
            return null;

        return result;
    }

    private static bool HasMultipleDistinctWeightedJoints(List<BakedSkinnedPrimitive> primitives)
    {
        var usedJoints = new HashSet<int>();

        foreach (var prim in primitives)
        {
            for (var i = 0; i < prim.Joints.Count; i++)
            {
                var joint = prim.Joints[i];
                var weight = prim.Weights[i];

                if (weight.X > 0f) usedJoints.Add(joint.A);
                if (weight.Y > 0f) usedJoints.Add(joint.B);
                if (weight.Z > 0f) usedJoints.Add(joint.C);
                if (weight.W > 0f) usedJoints.Add(joint.D);

                if (usedJoints.Count > 1)
                    return true;
            }
        }

        return false;
    }
}
