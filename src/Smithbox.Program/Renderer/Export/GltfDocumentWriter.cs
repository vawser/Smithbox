using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Renderer.Export;

/// <summary>
/// A minimal, self-contained glTF 2.0 (.gltf + .bin) writer.
/// Only supports what the model/map viewport exporter needs:
/// indexed triangle meshes with POSITION/NORMAL/TEXCOORD_0, a node hierarchy with
/// explicit 4x4 matrices, and materials with a base color + normal texture referencing
/// external PNG files.
/// </summary>
public class GltfDocumentWriter
{
    private const int GL_ARRAY_BUFFER = 34962;
    private const int GL_ELEMENT_ARRAY_BUFFER = 34963;

    private const int GL_UNSIGNED_SHORT = 5123;
    private const int GL_UNSIGNED_INT = 5125;
    private const int GL_FLOAT = 5126;

    private readonly List<byte> _binaryBuffer = new();

    private readonly List<(int buffer, int byteOffset, int byteLength, int? target)> _bufferViews = new();

    private readonly List<(int bufferView, int byteOffset, int componentType, int count, string type, float[] min, float[] max)> _accessors = new();

    private readonly List<GltfMesh> _meshes = new();
    private readonly List<GltfSkin> _skins = new();
    private readonly List<GltfMaterial> _materials = new();
    private readonly Dictionary<string, int> _materialLookup = new();

    private readonly List<string> _images = new();
    private readonly Dictionary<string, int> _imageLookup = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<int> _textures = new();
    private readonly Dictionary<int, int> _textureLookup = new();

    private readonly List<GltfNode> _nodes = new();
    private readonly List<int> _sceneRootNodes = new();

    private class GltfMesh
    {
        public string Name;
        public readonly List<(int posAccessor, int? normAccessor, int uvAccessor, int idxAccessor, int material,
            int? jointsAccessor, int? weightsAccessor)> Primitives = new();
    }

    private class GltfSkin
    {
        public string Name;
        public List<int> Joints;
        public int InverseBindMatricesAccessor;
        public int? Skeleton;
    }

    private class GltfMaterial
    {
        public string Name;
        public int? BaseColorTexture;
        public int? NormalTexture;
        public int? MetallicRoughnessTexture;
        public string AlphaMode;
    }

    private class GltfNode
    {
        public string Name;
        public Matrix4x4? Matrix;
        public int? Mesh;
        public int? Skin;
        public readonly List<int> Children = new();
    }

    /// <summary>
    /// Registers (or reuses) an image entry pointing at a PNG file, given as a path relative to the .gltf file.
    /// </summary>
    public int AddOrGetImage(string relativeUri)
    {
        var key = relativeUri.Replace('\\', '/');
        if (_imageLookup.TryGetValue(key, out var existing))
            return existing;

        var index = _images.Count;
        _images.Add(key);
        _imageLookup[key] = index;
        return index;
    }

    public int AddOrGetTexture(int imageIndex)
    {
        if (_textureLookup.TryGetValue(imageIndex, out var existing))
            return existing;

        var index = _textures.Count;
        _textures.Add(imageIndex);
        _textureLookup[imageIndex] = index;
        return index;
    }

    /// <summary>
    /// Registers (or reuses) a material. <paramref name="dedupeKey"/> should uniquely identify the
    /// combination of textures/name so identical materials across models are shared.
    /// </summary>
    /// <param name="metallicRoughnessTexture">
    /// Wired into the material's roughness (G) channel. From's engine doesn't have a metallic-roughness map
    /// of its own, so whatever specular/shininess/reflectance texture the source material had is used here
    /// as a best-effort stand-in rather than an exact conversion.
    /// </param>
    /// <param name="alphaMode">One of "OPAQUE", "MASK" or "BLEND".</param>
    public int AddOrGetMaterial(string dedupeKey, string name, int? baseColorTexture, int? normalTexture,
        int? metallicRoughnessTexture = null, string alphaMode = "OPAQUE")
    {
        if (_materialLookup.TryGetValue(dedupeKey, out var existing))
            return existing;

        var index = _materials.Count;
        _materials.Add(new GltfMaterial
        {
            Name = name,
            BaseColorTexture = baseColorTexture,
            NormalTexture = normalTexture,
            MetallicRoughnessTexture = metallicRoughnessTexture,
            AlphaMode = string.IsNullOrEmpty(alphaMode) ? "OPAQUE" : alphaMode
        });
        _materialLookup[dedupeKey] = index;
        return index;
    }

    public int AddMesh(string name)
    {
        var index = _meshes.Count;
        _meshes.Add(new GltfMesh { Name = name });
        return index;
    }

    /// <summary>
    /// Adds a primitive (triangle list) to the given mesh. Arrays must all be the same length as <paramref name="positions"/>,
    /// except <paramref name="indices"/> which indexes into them.
    /// </summary>
    /// <param name="includeNormals">
    /// When false, no NORMAL attribute is written for this primitive at all. Per the glTF spec, viewers must
    /// then compute their own normals from the geometry - e.g. Blender falls back to its own face/vertex
    /// normals instead of using (potentially wrong) baked-in custom split normals.
    /// </param>
    /// <param name="joints">
    /// Per-vertex glTF joint indices (into whichever skin's <c>joints</c> array is bound to this primitive's
    /// node - see <see cref="AddSkin"/>). Must be the same length as <paramref name="positions"/> when provided;
    /// omit (together with <paramref name="weights"/>) for an unskinned/static primitive.
    /// </param>
    /// <param name="weights">Per-vertex joint weights, matching <paramref name="joints"/> one-for-one.</param>
    public void AddPrimitive(int meshIndex, IReadOnlyList<Vector3> positions, IReadOnlyList<Vector3> normals,
        IReadOnlyList<Vector2> uvs, IReadOnlyList<int> indices, int materialIndex, bool includeNormals = true,
        IReadOnlyList<(ushort A, ushort B, ushort C, ushort D)> joints = null, IReadOnlyList<Vector4> weights = null)
    {
        if (positions.Count == 0 || indices.Count == 0)
            return;

        var posAccessor = WriteVec3Accessor(positions, GL_ARRAY_BUFFER, computeMinMax: true);
        int? normAccessor = includeNormals ? WriteVec3Accessor(normals, GL_ARRAY_BUFFER, computeMinMax: false) : null;
        var uvAccessor = WriteVec2Accessor(uvs);
        var idxAccessor = WriteIndexAccessor(indices);

        int? jointsAccessor = joints != null ? WriteJointsAccessor(joints) : null;
        int? weightsAccessor = weights != null ? WriteWeightsAccessor(weights) : null;

        _meshes[meshIndex].Primitives.Add((posAccessor, normAccessor, uvAccessor, idxAccessor, materialIndex,
            jointsAccessor, weightsAccessor));
    }

    public int AddNode(string name, Matrix4x4? matrix, int? meshIndex, IEnumerable<int> children = null, int? skin = null)
    {
        var node = new GltfNode
        {
            Name = name,
            Matrix = matrix,
            Mesh = meshIndex,
            Skin = skin
        };

        if (children != null)
            node.Children.AddRange(children);

        var index = _nodes.Count;
        _nodes.Add(node);
        return index;
    }

    public void AddChild(int parentNode, int childNode)
    {
        _nodes[parentNode].Children.Add(childNode);
    }

    /// <summary>
    /// Sets (or replaces) which skin a previously-added node is bound to - useful when the mesh node has to be
    /// created before the skin's joint node tree exists yet.
    /// </summary>
    public void SetNodeSkin(int nodeIndex, int skinIndex)
    {
        _nodes[nodeIndex].Skin = skinIndex;
    }

    public void AddSceneRoot(int nodeIndex)
    {
        _sceneRootNodes.Add(nodeIndex);
    }

    /// <summary>
    /// Registers a skin: <paramref name="jointNodeIndices"/> are the glTF node indices (already added via
    /// <see cref="AddNode"/>) making up the joint hierarchy, in the same order as <paramref name="inverseBindMatrices"/>.
    /// Bind the returned skin index to a mesh's node via <see cref="AddNode"/>'s <c>skin</c> parameter or
    /// <see cref="SetNodeSkin"/>.
    /// </summary>
    public int AddSkin(string name, IReadOnlyList<int> jointNodeIndices, IReadOnlyList<Matrix4x4> inverseBindMatrices,
        int? skeletonRootNode = null)
    {
        var ibmAccessor = WriteMat4Accessor(inverseBindMatrices);

        var index = _skins.Count;
        _skins.Add(new GltfSkin
        {
            Name = name,
            Joints = jointNodeIndices.ToList(),
            InverseBindMatricesAccessor = ibmAccessor,
            Skeleton = skeletonRootNode
        });
        return index;
    }

    private int AppendBufferView(byte[] data, int? target)
    {
        // Pad to 4-byte alignment as required by the glTF spec.
        while (_binaryBuffer.Count % 4 != 0)
            _binaryBuffer.Add(0);

        var byteOffset = _binaryBuffer.Count;
        _binaryBuffer.AddRange(data);

        var index = _bufferViews.Count;
        _bufferViews.Add((0, byteOffset, data.Length, target));
        return index;
    }

    private int WriteVec3Accessor(IReadOnlyList<Vector3> values, int target, bool computeMinMax)
    {
        var bytes = new byte[values.Count * 12];
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];
            BitConverter.GetBytes(v.X).CopyTo(bytes, i * 12);
            BitConverter.GetBytes(v.Y).CopyTo(bytes, i * 12 + 4);
            BitConverter.GetBytes(v.Z).CopyTo(bytes, i * 12 + 8);

            if (computeMinMax)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
        }

        var bufferView = AppendBufferView(bytes, target);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_FLOAT, values.Count, "VEC3",
            computeMinMax ? new[] { min.X, min.Y, min.Z } : null,
            computeMinMax ? new[] { max.X, max.Y, max.Z } : null));
        return accessorIndex;
    }

    private int WriteVec2Accessor(IReadOnlyList<Vector2> values)
    {
        var bytes = new byte[values.Count * 8];
        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];
            BitConverter.GetBytes(v.X).CopyTo(bytes, i * 8);
            BitConverter.GetBytes(v.Y).CopyTo(bytes, i * 8 + 4);
        }

        var bufferView = AppendBufferView(bytes, GL_ARRAY_BUFFER);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_FLOAT, values.Count, "VEC2", null, null));
        return accessorIndex;
    }

    private int WriteIndexAccessor(IReadOnlyList<int> indices)
    {
        var bytes = new byte[indices.Count * 4];
        for (var i = 0; i < indices.Count; i++)
        {
            BitConverter.GetBytes((uint)indices[i]).CopyTo(bytes, i * 4);
        }

        var bufferView = AppendBufferView(bytes, GL_ELEMENT_ARRAY_BUFFER);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_UNSIGNED_INT, indices.Count, "SCALAR", null, null));
        return accessorIndex;
    }

    /// <summary>
    /// Writes a JOINTS_0 accessor: 4 unsigned-short joint indices per vertex, indexing into whichever skin's
    /// <c>joints</c> array the primitive's node is bound to. Per spec these must NOT be normalized.
    /// </summary>
    private int WriteJointsAccessor(IReadOnlyList<(ushort A, ushort B, ushort C, ushort D)> values)
    {
        var bytes = new byte[values.Count * 8];
        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];
            BitConverter.GetBytes(v.A).CopyTo(bytes, i * 8);
            BitConverter.GetBytes(v.B).CopyTo(bytes, i * 8 + 2);
            BitConverter.GetBytes(v.C).CopyTo(bytes, i * 8 + 4);
            BitConverter.GetBytes(v.D).CopyTo(bytes, i * 8 + 6);
        }

        var bufferView = AppendBufferView(bytes, GL_ARRAY_BUFFER);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_UNSIGNED_SHORT, values.Count, "VEC4", null, null));
        return accessorIndex;
    }

    /// <summary>
    /// Writes a WEIGHTS_0 accessor: 4 float joint weights per vertex, matching a JOINTS_0 accessor one-for-one.
    /// </summary>
    private int WriteWeightsAccessor(IReadOnlyList<Vector4> values)
    {
        var bytes = new byte[values.Count * 16];
        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];
            BitConverter.GetBytes(v.X).CopyTo(bytes, i * 16);
            BitConverter.GetBytes(v.Y).CopyTo(bytes, i * 16 + 4);
            BitConverter.GetBytes(v.Z).CopyTo(bytes, i * 16 + 8);
            BitConverter.GetBytes(v.W).CopyTo(bytes, i * 16 + 12);
        }

        var bufferView = AppendBufferView(bytes, GL_ARRAY_BUFFER);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_FLOAT, values.Count, "VEC4", null, null));
        return accessorIndex;
    }

    /// <summary>
    /// Writes a MAT4 accessor (16 floats per matrix, column-major per spec) - used for a skin's
    /// <c>inverseBindMatrices</c>. No buffer view target: this data isn't a vertex attribute or index list.
    /// </summary>
    private int WriteMat4Accessor(IReadOnlyList<Matrix4x4> matrices)
    {
        var bytes = new byte[matrices.Count * 64];
        for (var i = 0; i < matrices.Count; i++)
        {
            var m = matrices[i];
            var offset = i * 64;
            float[] column = { m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44 };
            for (var c = 0; c < 16; c++)
                BitConverter.GetBytes(column[c]).CopyTo(bytes, offset + c * 4);
        }

        var bufferView = AppendBufferView(bytes, null);
        var accessorIndex = _accessors.Count;
        _accessors.Add((bufferView, 0, GL_FLOAT, matrices.Count, "MAT4", null, null));
        return accessorIndex;
    }

    /// <summary>
    /// Writes the accumulated scene to disk as "&lt;name&gt;.gltf" + "&lt;name&gt;.bin" next to each other.
    /// PNG textures referenced via <see cref="AddOrGetImage"/> are expected to already exist on disk
    /// (relative to the .gltf file).
    /// </summary>
    public void WriteToDisk(string gltfFilePath)
    {
        var directory = Path.GetDirectoryName(gltfFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var baseName = Path.GetFileNameWithoutExtension(gltfFilePath);
        var binFileName = $"{baseName}.bin";
        var binFilePath = Path.Combine(directory ?? "", binFileName);

        File.WriteAllBytes(binFilePath, _binaryBuffer.ToArray());

        using var stream = File.Create(gltfFilePath);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();

        // asset
        writer.WriteStartObject("asset");
        writer.WriteString("version", "2.0");
        writer.WriteString("generator", "Smithbox glTF Exporter");
        writer.WriteEndObject();

        // scene
        writer.WriteNumber("scene", 0);
        writer.WriteStartArray("scenes");
        writer.WriteStartObject();
        writer.WriteStartArray("nodes");
        foreach (var n in _sceneRootNodes)
            writer.WriteNumberValue(n);
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndArray();

        // nodes
        writer.WriteStartArray("nodes");
        foreach (var node in _nodes)
        {
            writer.WriteStartObject();

            if (!string.IsNullOrEmpty(node.Name))
                writer.WriteString("name", node.Name);

            if (node.Matrix.HasValue)
            {
                var m = node.Matrix.Value;
                writer.WriteStartArray("matrix");
                writer.WriteNumberValue(m.M11); writer.WriteNumberValue(m.M12); writer.WriteNumberValue(m.M13); writer.WriteNumberValue(m.M14);
                writer.WriteNumberValue(m.M21); writer.WriteNumberValue(m.M22); writer.WriteNumberValue(m.M23); writer.WriteNumberValue(m.M24);
                writer.WriteNumberValue(m.M31); writer.WriteNumberValue(m.M32); writer.WriteNumberValue(m.M33); writer.WriteNumberValue(m.M34);
                writer.WriteNumberValue(m.M41); writer.WriteNumberValue(m.M42); writer.WriteNumberValue(m.M43); writer.WriteNumberValue(m.M44);
                writer.WriteEndArray();
            }

            if (node.Mesh.HasValue)
                writer.WriteNumber("mesh", node.Mesh.Value);

            if (node.Skin.HasValue)
                writer.WriteNumber("skin", node.Skin.Value);

            if (node.Children.Count > 0)
            {
                writer.WriteStartArray("children");
                foreach (var c in node.Children)
                    writer.WriteNumberValue(c);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // meshes
        writer.WriteStartArray("meshes");
        foreach (var mesh in _meshes)
        {
            writer.WriteStartObject();
            if (!string.IsNullOrEmpty(mesh.Name))
                writer.WriteString("name", mesh.Name);

            writer.WriteStartArray("primitives");
            foreach (var prim in mesh.Primitives)
            {
                writer.WriteStartObject();
                writer.WriteStartObject("attributes");
                writer.WriteNumber("POSITION", prim.posAccessor);
                if (prim.normAccessor.HasValue)
                    writer.WriteNumber("NORMAL", prim.normAccessor.Value);
                writer.WriteNumber("TEXCOORD_0", prim.uvAccessor);
                if (prim.jointsAccessor.HasValue)
                    writer.WriteNumber("JOINTS_0", prim.jointsAccessor.Value);
                if (prim.weightsAccessor.HasValue)
                    writer.WriteNumber("WEIGHTS_0", prim.weightsAccessor.Value);
                writer.WriteEndObject();
                writer.WriteNumber("indices", prim.idxAccessor);
                writer.WriteNumber("material", prim.material);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // skins
        if (_skins.Count > 0)
        {
            writer.WriteStartArray("skins");
            foreach (var skin in _skins)
            {
                writer.WriteStartObject();
                if (!string.IsNullOrEmpty(skin.Name))
                    writer.WriteString("name", skin.Name);

                writer.WriteStartArray("joints");
                foreach (var j in skin.Joints)
                    writer.WriteNumberValue(j);
                writer.WriteEndArray();

                writer.WriteNumber("inverseBindMatrices", skin.InverseBindMatricesAccessor);

                if (skin.Skeleton.HasValue)
                    writer.WriteNumber("skeleton", skin.Skeleton.Value);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        // materials
        writer.WriteStartArray("materials");
        foreach (var mat in _materials)
        {
            writer.WriteStartObject();
            if (!string.IsNullOrEmpty(mat.Name))
                writer.WriteString("name", mat.Name);

            writer.WriteStartObject("pbrMetallicRoughness");
            if (mat.BaseColorTexture.HasValue)
            {
                writer.WriteStartObject("baseColorTexture");
                writer.WriteNumber("index", mat.BaseColorTexture.Value);
                writer.WriteEndObject();
            }
            if (mat.MetallicRoughnessTexture.HasValue)
            {
                writer.WriteStartObject("metallicRoughnessTexture");
                writer.WriteNumber("index", mat.MetallicRoughnessTexture.Value);
                writer.WriteEndObject();
            }
            writer.WriteNumber("metallicFactor", 0.0);
            writer.WriteNumber("roughnessFactor", 1.0);
            writer.WriteEndObject();

            if (mat.NormalTexture.HasValue)
            {
                writer.WriteStartObject("normalTexture");
                writer.WriteNumber("index", mat.NormalTexture.Value);
                writer.WriteEndObject();
            }

            if (!string.IsNullOrEmpty(mat.AlphaMode) && mat.AlphaMode != "OPAQUE")
            {
                writer.WriteString("alphaMode", mat.AlphaMode);
                if (mat.AlphaMode == "MASK")
                    writer.WriteNumber("alphaCutoff", 0.5);
            }

            if (mat.AlphaMode is "BLEND" or "MASK")
                writer.WriteBoolean("doubleSided", true);

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // textures
        writer.WriteStartArray("textures");
        foreach (var imgIndex in _textures)
        {
            writer.WriteStartObject();
            writer.WriteNumber("source", imgIndex);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // images
        writer.WriteStartArray("images");
        foreach (var uri in _images)
        {
            writer.WriteStartObject();
            writer.WriteString("uri", Uri.EscapeDataString(uri).Replace("%2F", "/"));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // accessors
        writer.WriteStartArray("accessors");
        foreach (var acc in _accessors)
        {
            writer.WriteStartObject();
            writer.WriteNumber("bufferView", acc.bufferView);
            if (acc.byteOffset != 0)
                writer.WriteNumber("byteOffset", acc.byteOffset);
            writer.WriteNumber("componentType", acc.componentType);
            writer.WriteNumber("count", acc.count);
            writer.WriteString("type", acc.type);

            if (acc.min != null)
            {
                writer.WriteStartArray("min");
                foreach (var f in acc.min) writer.WriteNumberValue(f);
                writer.WriteEndArray();
            }

            if (acc.max != null)
            {
                writer.WriteStartArray("max");
                foreach (var f in acc.max) writer.WriteNumberValue(f);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // bufferViews
        writer.WriteStartArray("bufferViews");
        foreach (var bv in _bufferViews)
        {
            writer.WriteStartObject();
            writer.WriteNumber("buffer", bv.buffer);
            writer.WriteNumber("byteOffset", bv.byteOffset);
            writer.WriteNumber("byteLength", bv.byteLength);
            if (bv.target.HasValue)
                writer.WriteNumber("target", bv.target.Value);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        // buffers
        writer.WriteStartArray("buffers");
        writer.WriteStartObject();
        writer.WriteString("uri", binFileName);
        writer.WriteNumber("byteLength", _binaryBuffer.Count);
        writer.WriteEndObject();
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
