using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>A model's meshes as the Create tab's Selected FLVERs shape uses them: triangles in the model's space.</summary>
public sealed class CollisionFlverModel
{
    public readonly List<CollisionFlverMesh> Meshes = new();
}

/// <summary>One mesh: its material's name (hkm_Name_Property gives a collision material) and its triangles' corners.</summary>
public sealed class CollisionFlverMesh
{
    public string Material = "";

    /// <summary>Three corners a triangle, placed by the mesh's or vertex's node where the mesh is static.</summary>
    public readonly List<Vector3> Corners = new();
}

/// <summary>
/// Reads the model of a map piece or object for the Create tab, from the container the model locator gives, in every
/// game's layout: a loose FLVER (DES, DS1, DS1R and BB map pieces, .dcx or not), a BND3 (DES, DS1, DS1R objects), a
/// BND4 (BB, DS3 and later), or a split binder (DS2S's model/map/m10_02_00_00.mapbhd); FLVER0 (DES) or FLVER2.
/// Static meshes are placed as the viewport draws them: FLVER2 by the mesh's node, or per vertex by the node index in
/// the normal's W; FLVER0 (DES) rigid meshes by the node of each vertex's first bone. Depends only on SoulsFormats,
/// so the survey tools link it.
/// </summary>
public static class CollisionFlverReader
{
    /// <summary>
    /// The model of an asset name (m2000B1A10, m24_01_00_00_000100, o0100) in a container, or null if it isn't there
    /// or can't be read. colorNormalIsNode: whether a Color normal's W is a node index (not in DS1, DS1R).
    /// </summary>
    public static CollisionFlverModel Read(Func<string, byte[]> readFile, string containerPath, string assetName,
        bool colorNormalIsNode)
    {
        var bytes = FindModelBytes(readFile, containerPath, assetName);

        return bytes == null ? null : FromBytes(bytes, colorNormalIsNode);
    }

    /// <summary>The model file's bytes, out of its container.</summary>
    public static byte[] FindModelBytes(Func<string, byte[]> readFile, string containerPath, string assetName)
    {
        if (string.IsNullOrEmpty(containerPath))
            return null;

        var data = readFile(containerPath);

        if (data == null)
            return null;

        // A split binder: the header beside its data (.mapbhd and .mapbdt)
        var bhd = containerPath.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);

        if (bhd >= 0 && bhd == containerPath.Length - 3)
        {
            var bdt = readFile(string.Concat(containerPath.AsSpan(0, bhd), "bdt"));

            if (bdt == null)
                return null;

            IBinder split = Magic(data, "BHF3") ? BXF3.Read(data, bdt) : BXF4.Read(data, bdt);
            return Pick(split.Files.Select(f => (f.Name, (Func<byte[]>)(() => f.Bytes.ToArray()))).ToList(), assetName);
        }

        data = Unpack(data);

        if (Magic(data, "BND3"))
        {
            using var binder = new BND3Reader(data);
            return Pick(binder.Files.Select(f => (f.Name, (Func<byte[]>)(() => binder.ReadFile(f).ToArray()))).ToList(), assetName);
        }

        if (Magic(data, "BND4"))
        {
            using var binder = new BND4Reader(data);
            return Pick(binder.Files.Select(f => (f.Name, (Func<byte[]>)(() => binder.ReadFile(f).ToArray()))).ToList(), assetName);
        }

        return Magic(data, "FLVER") ? data : null;
    }

    /// <summary>
    /// The model entry of a binder: the one named as the asset, else one whose name holds it, else the only model.
    /// </summary>
    private static byte[] Pick(List<(string Name, Func<byte[]> Read)> files, string assetName)
    {
        static bool IsModel(string name) => name != null && (name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".flv", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".flver.dcx", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".flv.dcx", StringComparison.OrdinalIgnoreCase));

        static string Stem(string name)
        {
            var file = name.Replace('\\', '/');
            file = file.Substring(file.LastIndexOf('/') + 1);
            var dot = file.IndexOf('.');
            return dot < 0 ? file : file.Substring(0, dot);
        }

        var models = files.Where(f => IsModel(f.Name)).ToList();
        var name = assetName ?? "";

        var match = models.FirstOrDefault(f => string.Equals(Stem(f.Name), name, StringComparison.OrdinalIgnoreCase));

        if (match.Name == null && name.Length > 0)
            match = models.FirstOrDefault(f => f.Name.Contains(name.TrimStart('m', 'M'), StringComparison.OrdinalIgnoreCase));

        if (match.Name == null && models.Count == 1)
            match = models[0];

        return match.Name == null ? null : Unpack(match.Read());
    }

    /// <summary>A FLVER0 or FLVER2 file's meshes, or null if it is neither.</summary>
    public static CollisionFlverModel FromBytes(byte[] bytes, bool colorNormalIsNode)
    {
        bytes = Unpack(bytes);

        if (!Magic(bytes, "FLVER") || bytes.Length < 12)
            return null;

        var bigEndian = bytes[6] == 'B';
        var version = bigEndian
            ? bytes[8] << 24 | bytes[9] << 16 | bytes[10] << 8 | bytes[11]
            : bytes[11] << 24 | bytes[10] << 16 | bytes[9] << 8 | bytes[8];

        return version >= 0x20000
            ? FromFlver2(FLVER2.Read(bytes), colorNormalIsNode)
            : FromFlver0(FLVER0.Read(bytes));
    }

    private static CollisionFlverModel FromFlver2(FLVER2 flver, bool colorNormalIsNode)
    {
        var model = new CollisionFlverModel();
        var nodes = NodeWorlds(flver.Nodes);

        foreach (var mesh in flver.Meshes)
        {
            var result = new CollisionFlverMesh
            {
                Material = mesh.MaterialIndex >= 0 && mesh.MaterialIndex < flver.Materials.Count ? flver.Materials[mesh.MaterialIndex].Name ?? "" : ""
            };

            // Static meshes: per vertex by the node in the normal's W where the layout stores one there (as the
            // viewport's vertex shader does); per vertex through the mesh's bone table where it lists several nodes
            // (DS1R's m2200B1A10 mesh 18: bones 1, 2, each vertex at its own node, which puts it where DS1's same
            // mesh is); else at the mesh's own node
            var perVertexNormalW = false;
            var perVertexBone = false;
            var meshMatrix = Matrix4x4.Identity;

            if (mesh.Dynamic == 0)
            {
                var members = mesh.VertexBuffers
                    .Where(b => b.LayoutIndex >= 0 && b.LayoutIndex < flver.BufferLayouts.Count)
                    .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
                    .ToList();

                perVertexNormalW = members.Any(m => m.Semantic == FLVER.LayoutSemantic.Normal &&
                    (m.Type is FLVER.LayoutType.UByte4 or FLVER.LayoutType.Byte4E ||
                     (colorNormalIsNode && m.Type == FLVER.LayoutType.Color)));

                perVertexBone = !perVertexNormalW && mesh.BoneIndices.Count > 1
                    && members.Any(m => m.Semantic == FLVER.LayoutSemantic.BoneIndices);

                if (mesh.NodeIndex >= 0 && mesh.NodeIndex < nodes.Length)
                    meshMatrix = nodes[mesh.NodeIndex];
            }

            foreach (var face in mesh.GetFaces())
            {
                foreach (var vertex in face)
                {
                    var matrix = perVertexNormalW ? Matrix4x4.Identity : meshMatrix;

                    if (perVertexNormalW && (vertex.NormalW & 0xFF) is var node && node < nodes.Length)
                    {
                        matrix = nodes[node];
                    }
                    else if (perVertexBone)
                    {
                        var slot = vertex.BoneIndices[0];

                        if (slot >= 0 && slot < mesh.BoneIndices.Count && mesh.BoneIndices[slot] is var boneNode && boneNode >= 0 && boneNode < nodes.Length)
                            matrix = nodes[boneNode];
                    }

                    result.Corners.Add(Vector3.Transform(vertex.Position, matrix));
                }
            }

            model.Meshes.Add(result);
        }

        return model;
    }

    private static CollisionFlverModel FromFlver0(FLVER0 flver)
    {
        var model = new CollisionFlverModel();
        var nodes = NodeWorlds(flver.Nodes);

        foreach (var mesh in flver.Meshes)
        {
            var result = new CollisionFlverMesh
            {
                Material = mesh.MaterialIndex < flver.Materials.Count ? flver.Materials[mesh.MaterialIndex].Name ?? "" : ""
            };

            var indices = mesh.Triangulate(flver.Header.Version, false, false);

            // As the viewport's FlverResource.GetRigidVertexNode: the first bone through the mesh's used bones, or the
            // mesh's node when the layout has no bone indices
            var layouts = mesh.MaterialIndex < flver.Materials.Count ? flver.Materials[mesh.MaterialIndex].Layouts : null;
            var hasBoneIndices = layouts != null && mesh.LayoutIndex < layouts.Count
                && layouts[mesh.LayoutIndex].Any(m => m.Semantic == FLVER.LayoutSemantic.BoneIndices);
            var usedBones = mesh.UsedBoneCount;

            for (var i = 0; i + 2 < indices.Count; i += 3)
            {
                for (var j = 0; j < 3; j++)
                {
                    var index = indices[i + j];

                    if (index < 0 || index >= mesh.Vertices.Count)
                        break;

                    var vertex = mesh.Vertices[index];
                    var matrix = Matrix4x4.Identity;

                    // A rigid mesh binds each vertex to the node of its first bone, through the mesh's bone table
                    if (mesh.Dynamic == 0)
                    {
                        var slot = vertex.BoneIndices[0];
                        var node = !hasBoneIndices ? mesh.NodeIndex
                            : slot >= 0 && slot < usedBones && slot < mesh.BoneIndices.Length ? mesh.BoneIndices[slot] : -1;

                        if (node >= 0 && node < nodes.Length)
                            matrix = nodes[node];
                    }

                    result.Corners.Add(Vector3.Transform(vertex.Position, matrix));
                }

                // A corner past the vertices drops the triangle
                if (result.Corners.Count % 3 != 0)
                    result.Corners.RemoveRange(result.Corners.Count - result.Corners.Count % 3, result.Corners.Count % 3);
            }

            model.Meshes.Add(result);
        }

        return model;
    }

    /// <summary>Each node's world matrix: its local transform, then its parents'.</summary>
    private static Matrix4x4[] NodeWorlds(List<FLVER.Node> nodes)
    {
        var worlds = new Matrix4x4[nodes.Count];

        for (var i = 0; i < nodes.Count; i++)
        {
            var matrix = nodes[i].ComputeLocalTransform();
            var parent = nodes[i].ParentIndex;
            var guard = 0;

            while (parent >= 0 && parent < nodes.Count && guard++ < nodes.Count)
            {
                matrix *= nodes[parent].ComputeLocalTransform();
                parent = nodes[parent].ParentIndex;
            }

            worlds[i] = matrix;
        }

        return worlds;
    }

    private static byte[] Unpack(byte[] data) => DCX.Is(data) ? DCX.Decompress(data).ToArray() : data;

    private static bool Magic(byte[] data, string magic)
    {
        if (data.Length < magic.Length)
            return false;

        for (var i = 0; i < magic.Length; i++)
        {
            if (data[i] != magic[i])
                return false;
        }

        return true;
    }
}
