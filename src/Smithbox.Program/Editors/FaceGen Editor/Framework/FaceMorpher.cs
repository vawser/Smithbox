#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// Applies a face's shape to the face part models of a character mesh, on the GPU.
/// <para>
/// Each face part (fg_a_*) carries its own EGM (how each FaceGen mode moves each of its TRI vertices) and flver2tri
/// (the TRI vertex of each FLVER vertex, per mesh). A FLVER vertex is moved by its TRI vertex's displacement, with
/// Z negated (FLVER Z = −TRI Z). Working with displacements, not positions, keeps meshes that sit off the TRI surface
/// (eyelash cards, the heads' extra meshes) where they are relative to the skin.
/// </para>
/// <para>
/// The new positions are written over the vertices the model was loaded with (kept for face parts by
/// <see cref="FlverResource.RetainVerticesFor"/>). Models are shared by everything that shows them, so the original
/// vertices are put back by <see cref="Restore"/> when the face is no longer shown. Normals keep the base shape's.
/// </para>
/// </summary>
public sealed class FaceMorpher
{
    private sealed record PartData(EGM Egm, F2TR Map);

    private readonly ProjectEntry Project;

    // The FaceGen files of each face part, read once (null: the part has none)
    private readonly Dictionary<string, PartData?> Parts = new();

    // The submeshes that have been written, so they can be put back
    private readonly HashSet<FlverResource.FlverSubmesh> Morphed = new();

    /// <summary>Makes face parts keep their vertices when they load. Call before any face part is loaded.</summary>
    public static void RetainFaceParts()
    {
        FlverResource.RetainVerticesFor = name => name.StartsWith("fg_a_", StringComparison.OrdinalIgnoreCase);
    }

    public FaceMorpher(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>TEMPORARY (FaceGen Phase 4 check): what the last <see cref="Apply"/> did, part by part</summary>
    public string LastReport { get; private set; } = "";

    /// <summary>
    /// Shapes every face part of <paramref name="mesh"/> (the parts are <paramref name="appearance"/>'s, in order) by
    /// the shape coefficients. False while a part isn't loaded yet, so it is to be called again.
    /// </summary>
    public bool Apply(MeshRenderableProxy mesh, CharacterAppearance appearance, float[] coefficients)
    {
        var done = true;
        var report = new List<string>();

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            var name = appearance.Parts[i].Name;

            if (!name.StartsWith("fg_a_", StringComparison.OrdinalIgnoreCase))
                continue;

            if (GetPartData(name) is not { } data)
            {
                report.Add($"{name}: no egm/flver2tri");
                continue;
            }

            if (mesh.Submeshes[i]._meshProvider.ResourceHandle is not ResourceHandle<FlverResource> { IsLoaded: true } handle ||
                handle.Get()?.GPUMeshes is not { } gpuMeshes)
            {
                done = false;
                report.Add($"{name}: not loaded");
                continue;
            }

            var displacement = new Vector3[data.Egm.VertexCount];
            data.Egm.Apply(displacement, coefficients);

            int written = 0, noCopy = 0, notResident = 0;

            for (var m = 0; m < gpuMeshes.Length && m < data.Map.Entries.Count; m++)
            {
                var submesh = gpuMeshes[m];

                if (submesh.RetainedVertices is not { } original || submesh.GeomBuffer is not { } buffer)
                {
                    noCopy++;
                    continue;
                }

                if (buffer.AllocStatus != VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
                {
                    done = false;
                    notResident++;
                    continue;
                }

                buffer.UpdateResidentVertices(Displace(original, submesh.VertexStride, submesh.VertexCount,
                    data.Map.Entries[m].Indices, displacement));
                Morphed.Add(submesh);
                written++;
            }

            var largest = displacement.Length > 0 ? displacement.Max(d => d.Length()) : 0;
            report.Add($"{name}: meshes {gpuMeshes.Length}/map {data.Map.Entries.Count}, written {written}, " +
                       $"no vertex copy {noCopy}, not resident {notResident}, largest move {largest:0.0000}, " +
                       $"stride {gpuMeshes.FirstOrDefault()?.VertexStride}");
        }

        // TEMPORARY (FaceGen Phase 4 check)
        LastReport = $"coefficients {coefficients.Min():0.00}..{coefficients.Max():0.00}; " + string.Join("; ", report);

        return done;
    }

    private static byte[] Displace(byte[] original, uint stride, int vertexCount, List<short> indices, Vector3[] displacement)
    {
        var data = (byte[])original.Clone();
        var count = Math.Min(Math.Min(vertexCount, indices.Count), (int)(data.Length / Math.Max(stride, 1)));

        for (var j = 0; j < count; j++)
        {
            var tri = indices[j];

            if (tri < 0 || tri >= displacement.Length)
                continue;

            var d = displacement[tri];
            var span = data.AsSpan((int)(j * stride), 12);
            var position = MemoryMarshal.Read<Vector3>(span) + new Vector3(d.X, d.Y, -d.Z);
            MemoryMarshal.Write(span, in position);
        }

        return data;
    }

    /// <summary>Puts back the vertices of everything that was shaped</summary>
    public void Restore()
    {
        foreach (var submesh in Morphed)
        {
            if (submesh.RetainedVertices is { } original && submesh.GeomBuffer is { } buffer &&
                buffer.AllocStatus == VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
            {
                buffer.UpdateResidentVertices(original);
            }
        }

        Morphed.Clear();
    }

    /// <summary>The EGM and flver2tri of a face part, from its partsbnd</summary>
    private PartData? GetPartData(string model)
    {
        if (Parts.TryGetValue(model, out var known))
            return known;

        PartData? data = null;

        try
        {
            var entry = Project.Locator.PartFiles.Entries.FirstOrDefault(e =>
                e.Filename.Equals(model, StringComparison.OrdinalIgnoreCase));

            if (entry != null && Project.VFS.FS.ReadFile(entry.Path) is { } raw)
            {
                var bytes = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
                var isBnd4 = bytes.Length >= 4 && bytes.Span[..4].SequenceEqual("BND4"u8);
                IBinder binder = isBnd4 ? BND4.Read(bytes) : BND3.Read(bytes);

                var egm = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(".egm", StringComparison.OrdinalIgnoreCase) == true);
                var map = binder.Files.FirstOrDefault(f => f.Name?.EndsWith(".flver2tri", StringComparison.OrdinalIgnoreCase) == true);

                if (egm != null && map != null)
                    data = new PartData(EGM.Read(egm.Bytes), F2TR.Read(map.Bytes));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Part_Read_Failed", model), e);
        }

        Parts[model] = data;
        return data;
    }
}
