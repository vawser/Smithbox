using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextureViewer;
using StudioCore.Renderer;
using StudioCore.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Veldrid.Utilities;
using static StudioCore.Renderer.FlverResource.FlverMaterial;

namespace StudioCore.Editors.MapEditor;

public enum MapExtractionTextureFormat
{
    PNG,
    DDS
}

/// <summary>
/// The texture slots written to the MTL, with the MTL statement each one is bound with
/// </summary>
public enum MapExtractionTextureSlot
{
    Albedo,
    Normal,
    Specular,
    Emissive
}

public sealed record MapExtractionOptions(
    string OutputDirectory,
    string FileName,
    bool IncludeTextures,
    bool IncludeMaterialMaps,
    MapExtractionTextureFormat TextureFormat,
    bool FlipUVs,
    bool MirrorX,
    bool CentreAtOrigin);

/// <summary>
/// A texture as the viewport bound it: its name in the TPF and the container it was loaded from
/// </summary>
public sealed record MapExtractionTextureRef(string Name, string ContainerPath);

/// <summary>
/// One mesh of a placed model, with the material it is drawn with in the viewport
/// </summary>
public sealed class MapExtractionMeshInfo
{
    public int MaterialIndex = -1;
    public string MaterialName = "";
    public Dictionary<MapExtractionTextureSlot, MapExtractionTextureRef> Textures = new();
}

/// <summary>
/// One selected map object with a model, read from the viewport on the main thread
/// </summary>
public sealed class MapExtractionPart
{
    public string EntityName;
    public string ModelName;
    public string ContainerPath;
    public string EntryPath;

    /// <summary>Where the model is drawn: its attach transform and world matrix, as the renderer combines them</summary>
    public Matrix4x4 World;

    public List<MapExtractionMeshInfo> Meshes = new();
}

/// <summary>
/// Writes the selected map objects to one OBJ, placed as the viewport draws them, with an MTL that binds the
/// textures the viewport used. The FLVERs and textures are read again from their containers, as the
/// renderer keeps neither once they are on the GPU.
/// </summary>
public class MapObjExporter
{
    private const string TextureFolderName = "Textures";

    private readonly ProjectEntry Project;
    private readonly MapExtractionOptions Options;
    private readonly List<MapExtractionPart> Parts;
    private readonly Vector3 Centre;

    // Normal W is read as a node index for Color normals only by the fast FLVER loader, which these games don't use
    private readonly bool ColorNormalIsNodeIndex;
    private readonly bool LegacyBinders;

    private readonly Dictionary<string, FLVER2> FlverCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, TPF.Texture>> TextureCache = new(StringComparer.OrdinalIgnoreCase);

    // Texture name to the file written for it (relative to the OBJ), or null if it couldn't be written
    private readonly Dictionary<string, string> WrittenTextures = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> TexturesWithAlpha = new(StringComparer.OrdinalIgnoreCase);

    public int ProcessedParts;
    public int TotalParts => Parts.Count;
    public int WrittenPartCount;
    public int WrittenTextureCount;
    public List<string> Warnings = new();

    public MapObjExporter(ProjectEntry project, MapExtractionOptions options, List<MapExtractionPart> parts, Vector3 centre)
    {
        Project = project;
        Options = options;
        Parts = parts;
        Centre = centre;

        ColorNormalIsNodeIndex = !(project.Descriptor.ProjectType is ProjectType.NR or ProjectType.DS1 or ProjectType.DS1R);
        LegacyBinders = project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }

    #region Snapshot

    /// <summary>
    /// Collects the selected objects that draw a model, and the objects under any selected one that doesn't.
    /// Must run on the main thread, as it reads the viewport's render state.
    /// </summary>
    public static List<MapExtractionPart> CollectParts(IEnumerable<Entity> selection, List<string> skipped, out BoundingBox? bounds)
    {
        var parts = new List<MapExtractionPart>();
        var visited = new HashSet<Entity>();
        bounds = null;

        var pending = new Stack<Entity>(selection.Reverse());

        while (pending.Count > 0)
        {
            var entity = pending.Pop();

            if (entity == null || !visited.Add(entity))
                continue;

            if (entity.RenderSceneMesh is not MeshRenderableProxy proxy)
            {
                // A map or folder in the content list stands for the objects under it
                for (var i = entity.Children.Count - 1; i >= 0; i--)
                {
                    pending.Push(entity.Children[i]);
                }

                continue;
            }

            var part = CollectPart(entity, proxy, out var reason);

            if (part == null)
            {
                skipped.Add($"{entity.Name}: {reason}");
                continue;
            }

            parts.Add(part);

            var partBounds = proxy.GetBounds();
            bounds = bounds == null ? partBounds : BoundingBox.Combine(bounds.Value, partBounds);
        }

        return parts;
    }

    private static MapExtractionPart CollectPart(Entity entity, MeshRenderableProxy proxy, out string reason)
    {
        reason = null;

        if (proxy.ResourceHandle is not ResourceHandle<FlverResource> handle)
        {
            reason = LOC.Get("MAP_MapExtraction_Skip_Not_Flver");
            return null;
        }

        if (!handle.IsLoaded)
        {
            reason = LOC.Get("MAP_MapExtraction_Skip_Not_Loaded");
            return null;
        }

        var resource = handle.Get();
        var context = resource?.Context;

        if (context == null || string.IsNullOrEmpty(context.ContainerPath))
        {
            reason = LOC.Get("MAP_MapExtraction_Skip_No_Source");
            return null;
        }

        if (resource.FlverDeS != null)
        {
            reason = LOC.Get("MAP_MapExtraction_Skip_Flver0");
            return null;
        }

        var part = new MapExtractionPart
        {
            EntityName = entity.Name ?? context.Name,
            ModelName = context.Name,
            ContainerPath = context.ContainerPath,
            EntryPath = context.EntryPath,
            World = proxy.AttachTransform * proxy.World
        };

        var gpuMeshes = resource.GPUMeshes ?? Array.Empty<FlverResource.FlverSubmesh>();
        var gpuMaterials = resource.GPUMaterials ?? Array.Empty<FlverResource.FlverMaterial>();

        foreach (var gpuMesh in gpuMeshes)
        {
            var info = new MapExtractionMeshInfo();
            var material = gpuMesh?.Material;

            if (material != null)
            {
                info.MaterialIndex = Array.IndexOf(gpuMaterials, material);
                info.MaterialName = material.MaterialName ?? "";

                AddTexture(info, material, MapExtractionTextureSlot.Albedo, TextureType.AlbedoTextureResource);
                AddTexture(info, material, MapExtractionTextureSlot.Normal, TextureType.NormalTextureResource);
                AddTexture(info, material, MapExtractionTextureSlot.Specular, TextureType.SpecularTextureResource);
                AddTexture(info, material, MapExtractionTextureSlot.Emissive, TextureType.EmissiveTextureResource);
            }

            part.Meshes.Add(info);
        }

        return part;
    }

    private static void AddTexture(MapExtractionMeshInfo info, FlverResource.FlverMaterial material,
        MapExtractionTextureSlot slot, TextureType type)
    {
        var handle = material.TextureResources[(int)type];

        // A slot can point at a texture that was never found, which the viewport draws with a default
        if (handle == null || !handle.IsLoaded || handle.Source == null || string.IsNullOrEmpty(handle.Source.ContainerPath))
            return;

        info.Textures[slot] = new MapExtractionTextureRef(handle.Name, handle.Source.ContainerPath);
    }

    #endregion

    #region Export

    /// <summary>
    /// Writes the OBJ, MTL and textures. Runs on a background task.
    /// </summary>
    public bool Run(CancellationToken token, out string objPath)
    {
        Directory.CreateDirectory(Options.OutputDirectory);

        objPath = Path.Combine(Options.OutputDirectory, $"{Options.FileName}.obj");
        var mtlFileName = $"{Options.FileName}.mtl";
        var mtlPath = Path.Combine(Options.OutputDirectory, mtlFileName);

        // Material name to its textures, in the order the materials are first used
        var materials = new List<(string Name, MapExtractionMeshInfo Info)>();
        var materialNames = new HashSet<string>(StringComparer.Ordinal);

        var exportTransform = Matrix4x4.CreateTranslation(-Centre)
            * Matrix4x4.CreateScale(Options.MirrorX ? -1 : 1, 1, 1);

        using (var obj = new StreamWriter(objPath, false, new UTF8Encoding(false)))
        {
            obj.WriteLine("# Exported from the Smithbox Map Editor");
            obj.WriteLine($"mtllib {mtlFileName}");
            obj.WriteLine();

            // OBJ indices are 1-based and count every vertex written before them in the file
            var vertexBase = 1;
            var uvBase = 1;

            var usedObjectNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var part in Parts)
            {
                token.ThrowIfCancellationRequested();

                var flver = GetFlver(part);
                ProcessedParts++;

                if (flver == null)
                    continue;

                var objectName = UniqueName(Sanitize(part.EntityName), usedObjectNames);
                obj.WriteLine($"o {objectName}");

                var nodeMatrices = flver.Nodes.Select(n => Utils.GetBoneWorldMatrix(n, flver.Nodes)).ToArray();
                var partTransform = part.World * exportTransform;

                for (var meshIndex = 0; meshIndex < flver.Meshes.Count; meshIndex++)
                {
                    var mesh = flver.Meshes[meshIndex];

                    if (mesh.Vertices == null || mesh.Vertices.Count == 0)
                        continue;

                    var faceSet = mesh.FaceSets.Find(fs => fs.Flags == FLVER2.FaceSet.FSFlags.None)
                        ?? (mesh.FaceSets.Count > 0 ? mesh.FaceSets[0] : null);

                    if (faceSet == null)
                        continue;

                    var info = meshIndex < part.Meshes.Count ? part.Meshes[meshIndex] : new MapExtractionMeshInfo();
                    var materialName = GetMaterialName(part, flver, mesh, info);

                    if (materialNames.Add(materialName))
                    {
                        materials.Add((materialName, info));
                    }

                    obj.WriteLine($"g {objectName}_{meshIndex}");
                    obj.WriteLine($"usemtl {materialName}");

                    WriteMesh(obj, flver, mesh, faceSet, nodeMatrices, partTransform, vertexBase, uvBase, out var hasUVs);

                    vertexBase += mesh.Vertices.Count;
                    if (hasUVs)
                        uvBase += mesh.Vertices.Count;
                }

                obj.WriteLine();
                WrittenPartCount++;
            }
        }

        token.ThrowIfCancellationRequested();

        File.WriteAllText(mtlPath, BuildMtl(materials, token), new UTF8Encoding(false));

        return WrittenPartCount > 0;
    }

    private void WriteMesh(StreamWriter obj, FLVER2 flver, FLVER2.Mesh mesh, FLVER2.FaceSet faceSet,
        Matrix4x4[] nodeMatrices, Matrix4x4 partTransform, int vertexBase, int uvBase, out bool hasUVs)
    {
        // Static meshes are placed by their node, or per vertex by the node index in the normal's W,
        // as the viewport's vertex shader does
        var perVertexNode = false;
        var meshTransform = partTransform;

        if (mesh.Dynamic == 0)
        {
            perVertexNode = mesh.VertexBuffers
                .SelectMany(b => flver.BufferLayouts[b.LayoutIndex])
                .Any(m => m.Semantic == FLVER.LayoutSemantic.Normal &&
                    (m.Type is FLVER.LayoutType.UByte4 or FLVER.LayoutType.Byte4E ||
                     (ColorNormalIsNodeIndex && m.Type == FLVER.LayoutType.Color)));

            if (!perVertexNode && mesh.NodeIndex >= 0 && mesh.NodeIndex < nodeMatrices.Length)
            {
                meshTransform = nodeMatrices[mesh.NodeIndex] * partTransform;
            }
        }

        hasUVs = mesh.Vertices[0].UVs != null && mesh.Vertices[0].UVs.Count > 0;

        var vertexTransforms = new Matrix4x4[mesh.Vertices.Count];
        var normalTransforms = new Matrix4x4[mesh.Vertices.Count];

        // Each distinct transform's normal matrix is worked out once
        var normalCache = new Dictionary<int, Matrix4x4>();

        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var key = -1;
            var transform = meshTransform;

            if (perVertexNode)
            {
                var node = mesh.Vertices[i].NormalW & 0xFF;
                if (node < nodeMatrices.Length)
                {
                    key = node;
                    transform = nodeMatrices[node] * partTransform;
                }
            }

            if (!normalCache.TryGetValue(key, out var normalTransform))
            {
                normalTransform = GetNormalMatrix(transform);
                normalCache[key] = normalTransform;
            }

            vertexTransforms[i] = transform;
            normalTransforms[i] = normalTransform;
        }

        foreach (var (vert, i) in mesh.Vertices.Select((v, i) => (v, i)))
        {
            var p = Vector3.Transform(vert.Position, vertexTransforms[i]);
            obj.WriteLine(Invariant($"v {p.X} {p.Y} {p.Z}"));
        }

        if (hasUVs)
        {
            foreach (var vert in mesh.Vertices)
            {
                var uv = vert.UVs[0];
                var v = Options.FlipUVs ? 1f - uv.Y : uv.Y;
                obj.WriteLine(Invariant($"vt {uv.X} {v}"));
            }
        }

        foreach (var (vert, i) in mesh.Vertices.Select((v, i) => (v, i)))
        {
            var n = vert.Normal == Vector3.Zero ? Vector3.UnitY : vert.Normal;
            n = Vector3.TransformNormal(n, normalTransforms[i]);
            n = n == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(n);

            obj.WriteLine(Invariant($"vn {n.X} {n.Y} {n.Z}"));
        }

        var triangles = faceSet.Triangulate(mesh.Vertices.Count < ushort.MaxValue);

        for (var i = 0; i + 2 < triangles.Count; i += 3)
        {
            var a = triangles[i];
            var b = triangles[i + 1];
            var c = triangles[i + 2];

            if (a >= mesh.Vertices.Count || b >= mesh.Vertices.Count || c >= mesh.Vertices.Count)
                continue;

            // A mirroring transform turns the faces inside out unless their winding is reversed with it
            if (vertexTransforms[a].GetDeterminant() < 0)
                (b, c) = (c, b);

            obj.WriteLine($"f {FaceIndex(a, vertexBase, uvBase, hasUVs)} {FaceIndex(b, vertexBase, uvBase, hasUVs)} {FaceIndex(c, vertexBase, uvBase, hasUVs)}");
        }
    }

    private static string FaceIndex(int index, int vertexBase, int uvBase, bool hasUVs)
    {
        var v = index + vertexBase;
        return hasUVs ? $"{v}/{index + uvBase}/{v}" : $"{v}//{v}";
    }

    /// <summary>
    /// The inverse transpose, which keeps normals perpendicular to their faces under a non-uniform scale
    /// </summary>
    private static Matrix4x4 GetNormalMatrix(Matrix4x4 transform)
    {
        transform.Translation = Vector3.Zero;

        return Matrix4x4.Invert(transform, out var inverse)
            ? Matrix4x4.Transpose(inverse)
            : transform;
    }

    private string GetMaterialName(MapExtractionPart part, FLVER2 flver, FLVER2.Mesh mesh, MapExtractionMeshInfo info)
    {
        var index = mesh.MaterialIndex;
        var name = index >= 0 && index < flver.Materials.Count ? flver.Materials[index].Name : info.MaterialName;

        // Materials of different models can share a name but not their textures
        return Sanitize($"{part.ModelName}_{index}_{name}");
    }

    #endregion

    #region Materials

    private string BuildMtl(List<(string Name, MapExtractionMeshInfo Info)> materials, CancellationToken token)
    {
        var mtl = new StringBuilder();
        mtl.AppendLine("# Exported from the Smithbox Map Editor");
        mtl.AppendLine();

        foreach (var (name, info) in materials)
        {
            token.ThrowIfCancellationRequested();

            mtl.AppendLine($"newmtl {name}");

            var albedo = Options.IncludeTextures ? WriteTexture(info, MapExtractionTextureSlot.Albedo) : null;

            // A texture is multiplied by the colour, so a textured material is left white
            mtl.AppendLine(albedo != null ? "Kd 1.0 1.0 1.0" : "Kd 0.8 0.8 0.8");
            mtl.AppendLine("Ks 0.0 0.0 0.0");
            mtl.AppendLine("illum 2");

            if (albedo != null)
            {
                mtl.AppendLine($"map_Kd {albedo}");

                if (TexturesWithAlpha.Contains(albedo))
                {
                    mtl.AppendLine($"map_d {albedo}");
                }
            }

            if (Options.IncludeTextures && Options.IncludeMaterialMaps)
            {
                var normal = WriteTexture(info, MapExtractionTextureSlot.Normal);
                if (normal != null)
                {
                    mtl.AppendLine($"map_Bump {normal}");
                    mtl.AppendLine($"norm {normal}");
                }

                var specular = WriteTexture(info, MapExtractionTextureSlot.Specular);
                if (specular != null)
                {
                    mtl.AppendLine($"map_Ks {specular}");
                }

                var emissive = WriteTexture(info, MapExtractionTextureSlot.Emissive);
                if (emissive != null)
                {
                    mtl.AppendLine("Ke 1.0 1.0 1.0");
                    mtl.AppendLine($"map_Ke {emissive}");
                }
            }

            mtl.AppendLine();
        }

        return mtl.ToString();
    }

    /// <summary>
    /// Writes a texture once, and returns its path relative to the OBJ, or null if it isn't bound or can't be read
    /// </summary>
    private string WriteTexture(MapExtractionMeshInfo info, MapExtractionTextureSlot slot)
    {
        if (!info.Textures.TryGetValue(slot, out var texRef))
            return null;

        if (WrittenTextures.TryGetValue(texRef.Name, out var written))
            return written;

        WrittenTextures[texRef.Name] = null;

        var texture = GetTexture(texRef);
        if (texture == null)
        {
            Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Texture_Missing", texRef.Name, texRef.ContainerPath));
            return null;
        }

        var dds = texture.HeaderizeExt(out var extension);
        var folder = Path.Combine(Options.OutputDirectory, TextureFolderName);
        Directory.CreateDirectory(folder);

        var baseName = Sanitize(texRef.Name);
        string fileName;

        try
        {
            if (Options.TextureFormat is MapExtractionTextureFormat.PNG && extension == ".dds")
            {
                using var image = DdsDecoder.DecodeToImage(dds, out var error);

                if (image == null)
                {
                    Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Texture_Decode", texRef.Name, error));
                    return null;
                }

                fileName = $"{baseName}.png";
                image.SaveAsPng(Path.Combine(folder, fileName));

                if (slot is MapExtractionTextureSlot.Albedo && HasTransparency(image))
                {
                    TexturesWithAlpha.Add($"{TextureFolderName}/{fileName}");
                }
            }
            else
            {
                fileName = $"{baseName}{extension}";
                File.WriteAllBytes(Path.Combine(folder, fileName), dds);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Texture_Write", texRef.Name, e.Message));
            return null;
        }

        written = $"{TextureFolderName}/{fileName}";
        WrittenTextures[texRef.Name] = written;
        WrittenTextureCount++;

        return written;
    }

    private static bool HasTransparency(Image<Rgba32> image)
    {
        var transparent = false;

        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height && !transparent; y++)
            {
                foreach (var pixel in rows.GetRowSpan(y))
                {
                    if (pixel.A < 250)
                    {
                        transparent = true;
                        break;
                    }
                }
            }
        });

        return transparent;
    }

    #endregion

    #region Reading

    private FLVER2 GetFlver(MapExtractionPart part)
    {
        var key = $"{part.ContainerPath}|{part.EntryPath}";

        if (FlverCache.TryGetValue(key, out var cached))
            return cached;

        FLVER2 flver = null;

        try
        {
            var data = ReadFile(part.ContainerPath);

            if (data == null)
            {
                Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Container_Missing", part.ContainerPath));
            }
            else
            {
                var bytes = ResourceClassifier.DetectContainer(part.ContainerPath) switch
                {
                    ContainerFormat.Binder => FindBinderEntry(OpenBinder(data.Value), ResourceName.FileName(part.EntryPath), 0),
                    ContainerFormat.None => data,
                    _ => null
                };

                if (bytes == null)
                {
                    Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Model_Missing", part.ModelName, part.ContainerPath));
                }
                else
                {
                    flver = FLVER2.Read(bytes.Value);
                }
            }
        }
        catch (Exception e)
        {
            Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Model_Read", part.ModelName, e.Message));
        }

        FlverCache[key] = flver;
        return flver;
    }

    private Memory<byte>? FindBinderEntry(BinderReader binder, string fileName, int depth)
    {
        using (binder)
        {
            foreach (var header in binder.Files)
            {
                var entryName = ResourceName.FileName(header.Name ?? "");

                if (entryName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    return binder.ReadFile(header);
            }

            // The resource pool also loads models out of binders nested in the container
            if (depth < 4)
            {
                foreach (var header in binder.Files)
                {
                    if (ResourceClassifier.DetectContainer(ResourceName.FileName(header.Name ?? "")) is not ContainerFormat.Binder)
                        continue;

                    var nested = FindBinderEntry(OpenBinder(binder.ReadFile(header)), fileName, depth + 1);
                    if (nested != null)
                        return nested;
                }
            }
        }

        return null;
    }

    private TPF.Texture GetTexture(MapExtractionTextureRef texRef)
    {
        if (!TextureCache.TryGetValue(texRef.ContainerPath, out var textures))
        {
            textures = new Dictionary<string, TPF.Texture>(StringComparer.OrdinalIgnoreCase);

            try
            {
                LoadTextures(texRef.ContainerPath, textures);
            }
            catch (Exception e)
            {
                Warnings.Add(LOC.Get("MAP_MapExtraction_Warning_Texture_Container_Read", texRef.ContainerPath, e.Message));
            }

            TextureCache[texRef.ContainerPath] = textures;
        }

        return textures.GetValueOrDefault(texRef.Name);
    }

    /// <summary>
    /// Reads every texture of a container the way the resource pool loaded them: the TPFs in it, at any depth
    /// </summary>
    private void LoadTextures(string containerPath, Dictionary<string, TPF.Texture> textures)
    {
        switch (ResourceClassifier.DetectContainer(containerPath))
        {
            case ContainerFormat.Tpf:
            {
                var data = ReadFile(containerPath);
                if (data != null)
                    AddTpf(TPF.Read(data.Value), textures);
                break;
            }
            case ContainerFormat.Binder:
            {
                var data = ReadFile(containerPath);
                if (data != null)
                    AddBinderTextures(OpenBinder(data.Value), textures, 0);
                break;
            }
            case ContainerFormat.BinderSplit:
            {
                var i = containerPath.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);
                var dataPath = string.Concat(containerPath.AsSpan(0, i), "bdt", containerPath.AsSpan(i + 3));

                var bhd = ReadFile(containerPath);
                var bdt = ReadFile(dataPath);

                if (bhd != null && bdt != null)
                {
                    BinderReader binder = LegacyBinders ? new BXF3Reader(bhd.Value, bdt.Value) : new BXF4Reader(bhd.Value, bdt.Value);
                    AddBinderTextures(binder, textures, 0);
                }
                break;
            }
        }
    }

    private void AddBinderTextures(BinderReader binder, Dictionary<string, TPF.Texture> textures, int depth)
    {
        using (binder)
        {
            foreach (var header in binder.Files)
            {
                var entryName = ResourceName.FileName(header.Name ?? "");

                switch (ResourceClassifier.DetectContainer(entryName))
                {
                    case ContainerFormat.Tpf:
                        AddTpf(TPF.Read(binder.ReadFile(header)), textures);
                        break;
                    case ContainerFormat.Binder when depth < 4:
                        AddBinderTextures(OpenBinder(binder.ReadFile(header)), textures, depth + 1);
                        break;
                }
            }
        }
    }

    private static void AddTpf(TPF tpf, Dictionary<string, TPF.Texture> textures)
    {
        foreach (var texture in tpf.Textures)
        {
            textures.TryAdd(ResourceName.Normalize(texture.Name), texture);
        }
    }

    private BinderReader OpenBinder(Memory<byte> bytes)
    {
        return LegacyBinders ? new BND3Reader(bytes) : new BND4Reader(bytes);
    }

    /// <summary>
    /// Reads a project file, or a file on disk for models loaded from outside the project
    /// </summary>
    private Memory<byte>? ReadFile(string path)
    {
        // Project paths start with '/', which Windows also counts as rooted, so the project is tried first
        if (Project.VFS.FS.FileExists(path))
        {
            return Project.VFS.FS.ReadFile(path);
        }

        // Only a path with a drive is a file outside the project
        if (Path.IsPathFullyQualified(path) && File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }

        return null;
    }

    #endregion

    #region Names

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// OBJ and MTL names end at whitespace, and file names can't hold path characters
    /// </summary>
    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "unnamed";

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var c in name)
        {
            sb.Append(char.IsWhiteSpace(c) || c == '#' || Array.IndexOf(invalid, c) >= 0 ? '_' : c);
        }

        return sb.ToString();
    }

    private static string UniqueName(string name, HashSet<string> used)
    {
        var unique = name;

        for (var i = 1; !used.Add(unique); i++)
        {
            unique = $"{name}_{i}";
        }

        return unique;
    }

    #endregion
}
