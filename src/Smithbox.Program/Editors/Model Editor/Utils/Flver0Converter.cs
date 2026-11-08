using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Model and Time Act Editors hold a model as a FLVER2. Demon's Souls models are FLVER0, so they are converted on
/// read and converted back on save (<see cref="WriteEditorFlver"/>); the Model Editor saves DES models, the Time Act
/// Editor does not. The viewport draws them from the file itself (FlverResource reads FLVER0).
/// See Documentation/Claude/Older Games/DES Model Display.md and DES Model Save.md.
/// </summary>
public static class Flver0Converter
{
    /// <summary>
    /// What a FLVER2 doesn't hold of the FLVER0 it was converted from, kept for writing it back
    /// </summary>
    private class Flver0Origin
    {
        public FLVER0.FLVERHeader Header;
        public DCX.Type Compression;

        /// <summary>The FLVER2 buffer layout index of each layout of each material, in the material's order</summary>
        public List<List<int>> MaterialLayouts = new();
        public int BufferLayoutCount;
    }

    private static readonly ConditionalWeakTable<FLVER2, Flver0Origin> Origins = new();

    /// <summary>
    /// Reads a model for the editors: a FLVER0 for DES, converted, otherwise a FLVER2
    /// </summary>
    public static FLVER2 ReadEditorFlver(ProjectEntry project, Memory<byte> data)
    {
        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            return ToFlver2(FLVER0.Read(data));
        }

        return FLVER2.Read(data);
    }

    /// <summary>
    /// Writes a model held by the editors: as a FLVER0 for DES, otherwise as the FLVER2 it is
    /// </summary>
    public static byte[] WriteEditorFlver(ProjectEntry project, FLVER2 flver)
    {
        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            return ToFlver0(flver).Write();
        }

        return flver.Write();
    }

    /// <summary>
    /// The model of a binder that holds exactly one, whatever it is called; null otherwise or if it can't be read.
    /// For binders whose model isn't named after them.
    /// </summary>
    public static FLVER2 ReadOnlyFlver(ProjectEntry project, IBinder binder)
    {
        var model = GetOnlyFlverFile(binder);

        if (model == null)
            return null;

        try
        {
            return ReadEditorFlver(project, model.Bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(Flver0Converter), $"Failed to read {model.Name}", Logger.LogPriority.High, e);
            return null;
        }
    }

    /// <summary>
    /// The model file of a binder that holds exactly one, whatever it is called; null otherwise
    /// </summary>
    public static BinderFile GetOnlyFlverFile(IBinder binder)
    {
        var models = binder.Files
            .Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase) ||
                        f.Name.EndsWith(".flv", StringComparison.OrdinalIgnoreCase) ||
                        f.Name.EndsWith(".flver.dcx", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return models.Count == 1 ? models[0] : null;
    }

    public static FLVER2 ToFlver2(FLVER0 source)
    {
        var flver = new FLVER2();

        // A version whose vertex bone indices index the mesh's own bone table, as FLVER0's do
        flver.Header.BigEndian = source.Header.BigEndian;
        flver.Header.Version = 0x20005;
        flver.Header.Unicode = source.Header.Unicode;
        flver.Header.BoundingBoxMin = source.Header.BoundingBoxMin;
        flver.Header.BoundingBoxMax = source.Header.BoundingBoxMax;

        flver.Dummies.AddRange(source.Dummies);
        flver.Nodes.AddRange(source.Nodes);
        flver.Skeletons = new FLVER2.SkeletonSet();

        var origin = new Flver0Origin
        {
            Header = new FLVER0.FLVERHeader(source.Header),
            Compression = source.Compression
        };

        // FLVER0 keeps its buffer layouts in the materials; FLVER2 in one list the meshes index
        var layoutIndices = new Dictionary<(int Material, int Layout), int>();

        for (var m = 0; m < source.Materials.Count; m++)
        {
            FLVER0.Material material = source.Materials[m];

            var converted = new FLVER2.Material
            {
                Name = material.Name,
                MTD = material.MTD,
                GXIndex = -1,
                Index = m
            };

            foreach (FLVER0.Texture texture in material.Textures)
            {
                converted.Textures.Add(new FLVER2.Texture { ParamName = texture.ParamName, Path = texture.Path });
            }

            flver.Materials.Add(converted);

            var materialLayouts = new List<int>();
            for (var l = 0; l < material.Layouts.Count; l++)
            {
                var layout = new FLVER2.BufferLayout();
                layout.AddRange(material.Layouts[l]);

                layoutIndices[(m, l)] = flver.BufferLayouts.Count;
                materialLayouts.Add(flver.BufferLayouts.Count);
                flver.BufferLayouts.Add(layout);
            }
            origin.MaterialLayouts.Add(materialLayouts);
        }
        origin.BufferLayoutCount = flver.BufferLayouts.Count;

        foreach (FLVER0.Mesh mesh in source.Meshes)
        {
            var converted = new FLVER2.Mesh
            {
                Dynamic = mesh.Dynamic,
                MaterialIndex = mesh.MaterialIndex,
                NodeIndex = mesh.NodeIndex
            };

            converted.BoneIndices.AddRange(mesh.BoneIndices.Take(mesh.UsedBoneCount).Select(b => (int)b));
            converted.Vertices.AddRange(mesh.Vertices);

            converted.FaceSets.Add(new FLVER2.FaceSet(FLVER2.FaceSet.FSFlags.None, mesh.TriangleStrip, mesh.CullBackfaces, 0,
                new List<int>(mesh.Indices)));

            if (layoutIndices.TryGetValue((mesh.MaterialIndex, mesh.LayoutIndex), out var layoutIndex))
            {
                converted.VertexBuffers.Add(new FLVER2.VertexBuffer(layoutIndex));
            }

            flver.Meshes.Add(converted);
        }

        Origins.AddOrUpdate(flver, origin);

        return flver;
    }

    /// <summary>
    /// Converts an editor model back to a FLVER0. The header values a FLVER2 doesn't hold (version, index size, unknowns)
    /// and the compression come from the FLVER0 it was read from; a model that wasn't read from one gets the values most
    /// Demon's Souls models have. Throws if the model holds something FLVER0 can't store.
    /// </summary>
    public static FLVER0 ToFlver0(FLVER2 source)
    {
        Origins.TryGetValue(source, out var origin);

        var flver = new FLVER0();

        if (origin != null)
        {
            flver.Header = new FLVER0.FLVERHeader(origin.Header);
            flver.Compression = origin.Compression;
        }
        else
        {
            // As in 0x15 DES models: big endian, UTF-16 names, 16-bit indices restarting at 0xFFFF
            flver.Header = new FLVER0.FLVERHeader
            {
                BigEndian = source.Header.BigEndian,
                Version = 0x15,
                Unicode = source.Header.Unicode,
                VertexIndexSize = 16,
                Unk4A = 1,
                Unk4C = 0xFFFF
            };
        }

        flver.Header.BoundingBoxMin = source.Header.BoundingBoxMin;
        flver.Header.BoundingBoxMax = source.Header.BoundingBoxMax;

        flver.Dummies.AddRange(source.Dummies);
        flver.Nodes.AddRange(source.Nodes);

        // The original per-material layout lists still apply while the material and layout lists keep their shape
        var useOriginLayouts = origin != null
            && origin.MaterialLayouts.Count == source.Materials.Count
            && origin.BufferLayoutCount == source.BufferLayouts.Count;

        var materialLayouts = new List<List<int>>();
        for (var m = 0; m < source.Materials.Count; m++)
        {
            materialLayouts.Add(useOriginLayouts ? new List<int>(origin.MaterialLayouts[m]) : new List<int>());
        }

        if (source.Materials.Count > byte.MaxValue + 1)
            throw new InvalidOperationException($"FLVER0 holds at most {byte.MaxValue + 1} materials; the model has {source.Materials.Count}.");

        // Meshes first: each one's layout must be in its material's layout list
        for (var i = 0; i < source.Meshes.Count; i++)
        {
            FLVER2.Mesh mesh = source.Meshes[i];

            if (mesh.MaterialIndex < 0 || mesh.MaterialIndex >= source.Materials.Count)
                throw new InvalidOperationException($"Mesh {i} uses material {mesh.MaterialIndex}, which doesn't exist.");

            if (mesh.BoneIndices.Count > FLVER0.Mesh.MaxBoneCount)
                throw new InvalidOperationException($"Mesh {i} uses {mesh.BoneIndices.Count} bones; FLVER0 meshes hold at most {FLVER0.Mesh.MaxBoneCount}.");

            if (mesh.VertexBuffers.Count == 0)
                throw new InvalidOperationException($"Mesh {i} has no vertex buffer.");

            // FLVER0 has one face set per mesh: the full detail one
            FLVER2.FaceSet faceSet = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None)
                ?? mesh.FaceSets.FirstOrDefault();

            var bufferLayout = mesh.VertexBuffers[0].LayoutIndex;
            if (bufferLayout < 0 || bufferLayout >= source.BufferLayouts.Count)
                throw new InvalidOperationException($"Mesh {i} uses buffer layout {bufferLayout}, which doesn't exist.");

            var layouts = materialLayouts[mesh.MaterialIndex];
            var localLayout = layouts.IndexOf(bufferLayout);
            if (localLayout < 0)
            {
                localLayout = layouts.Count;
                layouts.Add(bufferLayout);
            }

            var converted = new FLVER0.Mesh
            {
                Dynamic = mesh.Dynamic,
                MaterialIndex = (byte)mesh.MaterialIndex,
                NodeIndex = (short)mesh.NodeIndex,
                LayoutIndex = localLayout,
                TriangleStrip = faceSet?.TriangleStrip ?? false,
                CullBackfaces = faceSet?.CullBackfaces ?? true,
                Indices = faceSet != null ? new List<int>(faceSet.Indices) : new List<int>(),
                Vertices = mesh.Vertices
            };

            for (var b = 0; b < mesh.BoneIndices.Count; b++)
            {
                converted.BoneIndices[b] = (short)mesh.BoneIndices[b];
            }

            flver.Meshes.Add(converted);
        }

        for (var m = 0; m < source.Materials.Count; m++)
        {
            FLVER2.Material material = source.Materials[m];

            var converted = new FLVER0.Material
            {
                Name = material.Name,
                MTD = material.MTD
            };

            foreach (FLVER2.Texture texture in material.Textures)
            {
                converted.Textures.Add(new FLVER0.Texture(texture.ParamName, texture.Path));
            }

            foreach (var layoutIndex in materialLayouts[m])
            {
                var layout = new FLVER0.BufferLayout();
                layout.AddRange(source.BufferLayouts[layoutIndex]);
                converted.Layouts.Add(layout);
            }

            flver.Materials.Add(converted);
        }

        return flver;
    }
}
