using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Converts a FLVER2 mesh list into static, model-space geometry (positions/normals/UV0/triangle indices),
/// baking in whichever per-mesh or per-vertex rigid-node transform the mesh needs (mirroring the same rules
/// the live renderer uses in <c>FlverResource.ProcessMesh</c>), and resolves each mesh's albedo/normal
/// texture virtual paths.
/// </summary>
public static class FlverStaticGeometryBaker
{
    public class BakedPrimitive
    {
        public string MaterialName;
        public List<Vector3> Positions;
        public List<Vector3> Normals;
        public List<Vector2> UVs;
        public List<int> Indices;
        public string AlbedoVirtualPath;
        public string NormalVirtualPath;
        public string SpecularVirtualPath;
        public string AlphaMode;
    }

    public static List<BakedPrimitive> Bake(ProjectEntry project, FLVER2 flver, string flverVirtPath)
    {
        var result = new List<BakedPrimitive>();

        if (flver?.Meshes == null)
            return result;

        var nodes = flver.Nodes;

        // Per-node normal transform (inverse-transpose of the node's world matrix), cached across meshes/vertices.
        // Positions can be transformed directly by a node's world matrix, but normals cannot - From's node
        // transforms routinely carry non-uniform or mirrored (negative) scale (e.g. reused/flipped decorative
        // props), and transforming a normal by a matrix with non-uniform scale the same way as a position
        // rotates it off-perpendicular from the surface, which reads as flipped/broken shading (black faces)
        // once lit. The correct transform for normals is the inverse-transpose of the position transform.
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

        // A node transform with a negative determinant (mirrored/negative scale - common for reused,
        // flipped decorative props) flips the triangles' apparent winding once baked into world space.
        // Left uncorrected, the winding no longer agrees with the (now correctly transformed) normal
        // direction, which single-sided renderers see as backwards/culled faces.
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
            if (!mesh.UseBoneWeights && nodes is { Count: > 0 })
            {
                var elements = mesh.VertexBuffers.SelectMany(b => flver.BufferLayouts[b.LayoutIndex]);
                useNormalWTransform = elements.Any(e =>
                    e.Semantic == FLVER.LayoutSemantic.Normal &&
                    (e.Type == FLVER.LayoutType.UByte4 || e.Type == FLVER.LayoutType.Byte4E));
            }

            Matrix4x4? rigidMeshTransform = null;
            Matrix4x4? rigidMeshNormalTransform = null;
            if (!mesh.UseBoneWeights && !useNormalWTransform && nodes is { Count: > 0 } &&
                mesh.NodeIndex >= 0 && mesh.NodeIndex < nodes.Count)
            {
                rigidMeshTransform = Utils.GetBoneWorldMatrix(nodes[mesh.NodeIndex], nodes);
                rigidMeshNormalTransform = GetNormalTransform(mesh.NodeIndex);
            }

            var positions = new List<Vector3>(mesh.Vertices.Count);
            var normals = new List<Vector3>(mesh.Vertices.Count);
            var uvs = new List<Vector2>(mesh.Vertices.Count);

            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                var normal = vertex.Normal;

                if (useNormalWTransform && nodes is { Count: > 0 } && vertex.NormalW >= 0 && vertex.NormalW < nodes.Count)
                {
                    var t = Utils.GetBoneWorldMatrix(nodes[vertex.NormalW], nodes);
                    pos = Vector3.Transform(pos, t);
                    normal = Vector3.TransformNormal(normal, GetNormalTransform(vertex.NormalW));
                }
                else if (rigidMeshTransform.HasValue)
                {
                    pos = Vector3.Transform(pos, rigidMeshTransform.Value);
                    normal = Vector3.TransformNormal(normal, rigidMeshNormalTransform!.Value);
                }

                positions.Add(pos);
                normals.Add(normal.LengthSquared() > 0.0001f ? Vector3.Normalize(normal) : normal);
                uvs.Add(vertex.UVs is { Count: > 0 } ? new Vector2(vertex.UVs[0].X, vertex.UVs[0].Y) : Vector2.Zero);
            }

            if (useNormalWTransform && nodes is { Count: > 0 })
            {
                // Rigid nodes can vary per vertex here, so decide per triangle (using its first vertex's node).
                for (var i = 0; i + 2 < indices.Count; i += 3)
                {
                    var nodeIndex = mesh.Vertices[indices[i]].NormalW;
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

            result.Add(new BakedPrimitive
            {
                MaterialName = string.IsNullOrEmpty(material.Name) ? material.MTD : material.Name,
                Positions = positions,
                Normals = normals,
                UVs = uvs,
                Indices = indices,
                AlbedoVirtualPath = resolvedTextures.AlbedoVirtualPath,
                NormalVirtualPath = resolvedTextures.NormalVirtualPath,
                SpecularVirtualPath = resolvedTextures.SpecularVirtualPath,
                AlphaMode = resolvedTextures.AlphaMode
            });
        }

        return result;
    }
}
