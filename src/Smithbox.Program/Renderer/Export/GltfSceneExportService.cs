using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Top level orchestrator for exporting the models currently present in a viewport (Map Editor or Model
/// Editor) to a single glTF 2.0 scene, with albedo/normal textures exported alongside as PNG files.
///
/// Scope: FLVER2 games only (DS1 and later - FLVER0/Demon's Souls models are skipped with a warning).
/// Only visually meaningful, textured parts are exported (map pieces, characters, objects/assets);
/// collisions, navmeshes, regions, events and lights are skipped since they carry no material/texture data.
/// </summary>
public static class GltfSceneExportService
{
    public class ExportResult
    {
        public bool Success;
        public string Message;
        public string OutputPath;
        public int ExportedModelCount;
        public int ExportedTextureCount;
        public List<string> Warnings = new();
    }

    private class ExportCaches
    {
        // Model cache key -> gltf mesh index (or null if that model failed to export)
        public readonly Dictionary<string, int?> Meshes = new();

        // Texture virtual path -> gltf image index (or null if that texture failed to export)
        public readonly Dictionary<string, int?> Images = new();
    }

    #region Map Editor

    public static ExportResult ExportMapViewport(ProjectEntry project, IEnumerable<MapContainer> maps, string outputFolder,
        bool includeNormals = true, Action<string> onStep = null)
    {
        var result = new ExportResult();
        var mapList = maps.Where(m => m != null).ToList();

        if (mapList.Count == 0)
        {
            result.Message = "No maps are currently loaded.";
            return result;
        }

        Directory.CreateDirectory(outputFolder);

        var doc = new GltfDocumentWriter();
        var caches = new ExportCaches();

        foreach (var map in mapList)
        {
            onStep?.Invoke($"Map: {map.Name}");

            var exportableParts = map.Parts
                .Where(IsExportableFlverPart)
                .Select(p => (Entity: p, Name: p.Name ?? "Part", GroupKey: GetGroupKey(p.Name ?? "Part")))
                .ToList();

            if (exportableParts.Count == 0)
                continue;

            var groupCounts = exportableParts
                .GroupBy(p => p.GroupKey)
                .ToDictionary(g => g.Key, g => g.Count());

            var mapNode = doc.AddNode($"Map_{map.Name}", null, null);
            var groupNodeCache = new Dictionary<string, int>();
            var addedAnyPart = false;

            foreach (var entry in exportableParts)
            {
                var meshIndex = GetOrBuildMapPartMesh(project, entry.Entity, doc, outputFolder, includeNormals, caches, result.Warnings);
                if (meshIndex == null)
                    continue;

                Matrix4x4 world;
                try
                {
                    world = entry.Entity.GetWorldMatrix();
                }
                catch
                {
                    world = Matrix4x4.Identity;
                }

                var partNode = doc.AddNode(entry.Name, world, meshIndex);
                addedAnyPart = true;

                if (groupCounts[entry.GroupKey] > 1)
                {
                    if (!groupNodeCache.TryGetValue(entry.GroupKey, out var groupNode))
                    {
                        groupNode = doc.AddNode(entry.GroupKey, null, null);
                        doc.AddChild(mapNode, groupNode);
                        groupNodeCache[entry.GroupKey] = groupNode;
                    }

                    doc.AddChild(groupNode, partNode);
                }
                else
                {
                    doc.AddChild(mapNode, partNode);
                }
            }

            if (addedAnyPart)
                doc.AddSceneRoot(mapNode);
        }

        if (caches.Meshes.Values.All(v => v == null))
        {
            result.Message = "No exportable models were found (nothing textured/FLVER-backed is currently loaded).";
            return result;
        }

        var fileName = mapList.Count == 1 ? $"{mapList[0].Name}.gltf" : "map_export.gltf";
        var gltfPath = Path.Combine(outputFolder, fileName);
        doc.WriteToDisk(gltfPath);

        result.Success = true;
        result.OutputPath = gltfPath;
        result.ExportedModelCount = caches.Meshes.Values.Count(v => v != null);
        result.ExportedTextureCount = caches.Images.Values.Count(v => v != null);
        return result;
    }

    private static bool IsExportableFlverPart(Entity ent)
    {
        return EntityHelper.IsPartMapPiece(ent)
            || EntityHelper.IsPartEnemy(ent)
            || EntityHelper.IsPartDummyEnemy(ent)
            || EntityHelper.IsPartAsset(ent)
            || EntityHelper.IsPartDummyAsset(ent);
    }

    private static int? GetOrBuildMapPartMesh(ProjectEntry project, Entity part, GltfDocumentWriter doc,
        string outputFolder, bool includeNormals, ExportCaches caches, List<string> warnings)
    {
        var modelName = part.GetProperty("ModelName")?.GetValue(part.WrappedObject) as string;
        if (string.IsNullOrEmpty(modelName))
            return null;

        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            warnings.Add($"Skipped '{part.Name}' ({modelName}): Demon's Souls (FLVER0) models are not supported by the glTF exporter.");
            return null;
        }

        var mapContainer = (MapContainer)part.Container;
        var amapid = PathBuilder.GetAssetMapID(project, mapContainer.Name);

        ResourceDescriptor asset;

        if (EntityHelper.IsPartMapPiece(part))
        {
            var name = ModelLocator.MapModelNameToAssetName(project, amapid, modelName);
            asset = ModelLocator.GetMapModel(project, amapid, name, name);
        }
        else if (EntityHelper.IsPartEnemy(part) || EntityHelper.IsPartDummyEnemy(part))
        {
            asset = ModelLocator.GetChrModel(project, modelName, modelName);
        }
        else
        {
            asset = ModelLocator.GetObjModel(project, modelName, modelName);
        }

        if (!asset.IsValid())
            return null;

        var cacheKey = asset.AssetVirtualPath ?? $"{asset.AssetArchiveVirtualPath}::{asset.AssetName}";

        if (caches.Meshes.TryGetValue(cacheKey, out var cached))
            return cached;

        var bytes = SoulsContainerIO.ReadFlverBytes(project, asset, out var error);
        if (bytes == null)
        {
            warnings.Add($"Could not read model '{modelName}': {error}");
            caches.Meshes[cacheKey] = null;
            return null;
        }

        FLVER2 flver;
        try
        {
            flver = FLVER2.Read(bytes);
        }
        catch (Exception e)
        {
            warnings.Add($"Could not parse model '{modelName}': {e.Message}");
            caches.Meshes[cacheKey] = null;
            return null;
        }

        var meshIndex = BuildMeshFromFlver(project, flver, asset.AssetVirtualPath, modelName, doc, outputFolder, includeNormals, caches, warnings);
        caches.Meshes[cacheKey] = meshIndex;
        return meshIndex;
    }

    /// <summary>
    /// Derives a glTF grouping key from a part name by stripping a trailing "_NNNN" instance/part index,
    /// e.g. "AEG099_000_0000" and "AEG099_000_0001" both group under "AEG099_000".
    /// </summary>
    private static string GetGroupKey(string partName)
    {
        var match = Regex.Match(partName, @"^(.*)_\d+$");
        return match.Success ? match.Groups[1].Value : partName;
    }

    #endregion

    #region Model Editor

    /// <param name="includeSkeleton">
    /// When true, exports a real glTF skin (joint hierarchy + inverse bind matrices + JOINTS_0/WEIGHTS_0) in
    /// bind pose instead of baked-in static geometry, so the result comes into Blender already rigged to an
    /// armature. No animation data is exported - the user poses/animates it themselves. Falls back to the
    /// static export automatically if the model has no bone/dummy hierarchy to skin against.
    /// </param>
    public static ExportResult ExportModelViewport(ProjectEntry project, ModelWrapper modelWrapper, string outputFolder,
        bool includeNormals = true, bool includeSkeleton = true)
    {
        var result = new ExportResult();

        if (modelWrapper?.FLVER == null)
        {
            result.Message = "No model is currently loaded in the viewport.";
            return result;
        }

        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            result.Message = "Demon's Souls (FLVER0) models are not supported by the glTF exporter.";
            return result;
        }

        Directory.CreateDirectory(outputFolder);

        var doc = new GltfDocumentWriter();
        var caches = new ExportCaches();

        var flverVirtPath = ResolveModelEditorVirtualPath(project, modelWrapper)?.AssetVirtualPath;

        int rootNode;

        var skinned = includeSkeleton
            ? BuildSkinnedMeshFromFlver(project, modelWrapper.FLVER, flverVirtPath, modelWrapper.Name, doc, outputFolder, includeNormals, caches, result.Warnings)
            : null;

        if (skinned.HasValue)
        {
            rootNode = doc.AddNode(modelWrapper.Name, Matrix4x4.Identity, null);
            doc.AddChild(rootNode, skinned.Value.SkeletonRootNode);

            var meshNode = doc.AddNode($"{modelWrapper.Name}_Mesh", Matrix4x4.Identity, skinned.Value.MeshIndex, skin: skinned.Value.SkinIndex);
            doc.AddChild(rootNode, meshNode);
        }
        else
        {
            if (includeSkeleton)
                result.Warnings.Add($"'{modelWrapper.Name}' has no bone hierarchy to skin against - exported as static geometry instead.");

            var meshIndex = BuildMeshFromFlver(project, modelWrapper.FLVER, flverVirtPath, modelWrapper.Name, doc, outputFolder, includeNormals, caches, result.Warnings);

            if (meshIndex == null)
            {
                result.Message = "The current model has no exportable geometry.";
                return result;
            }

            rootNode = doc.AddNode(modelWrapper.Name, Matrix4x4.Identity, meshIndex);
        }

        doc.AddSceneRoot(rootNode);

        var gltfPath = Path.Combine(outputFolder, $"{SanitizeFileName(modelWrapper.Name)}.gltf");
        doc.WriteToDisk(gltfPath);

        result.Success = true;
        result.OutputPath = gltfPath;
        result.ExportedModelCount = 1;
        result.ExportedTextureCount = caches.Images.Values.Count(v => v != null);
        return result;
    }

    private static ResourceDescriptor ResolveModelEditorVirtualPath(ProjectEntry project, ModelWrapper wrapper)
    {
        var name = wrapper.Name;
        if (string.IsNullOrEmpty(name))
            return null;

        var lower = name.ToLowerInvariant();
        var parent = wrapper.Parent;

        if (lower.StartsWith("m") && parent?.MapID != null)
        {
            var assetName = ModelLocator.GetMapModelName(project, parent.MapID, name);
            return ModelLocator.GetMapModel(project, parent.MapID, assetName, assetName);
        }

        if (lower.StartsWith("c"))
            return ModelLocator.GetChrModel(project, name, name);

        if (lower.StartsWith("o") || lower.StartsWith("aeg"))
            return ModelLocator.GetObjModel(project, name, name);

        if (lower.StartsWith("am") || lower.StartsWith("lg") || lower.StartsWith("bd") ||
            lower.StartsWith("hd") || lower.StartsWith("wp"))
            return ModelLocator.GetPartsModel(project, name, name);

        return null;
    }

    #endregion

    #region Shared

    private static int? BuildMeshFromFlver(ProjectEntry project, FLVER2 flver, string flverVirtPath, string meshName,
        GltfDocumentWriter doc, string outputFolder, bool includeNormals, ExportCaches caches, List<string> warnings)
    {
        List<FlverStaticGeometryBaker.BakedPrimitive> baked;
        try
        {
            baked = FlverStaticGeometryBaker.Bake(project, flver, flverVirtPath ?? "");
        }
        catch (Exception e)
        {
            warnings.Add($"Could not process geometry for '{meshName}': {e.Message}");
            return null;
        }

        if (baked.Count == 0)
            return null;

        var meshIndex = doc.AddMesh(meshName);

        foreach (var prim in baked)
        {
            var albedoImage = GetOrCreateTextureImage(project, doc, prim.AlbedoVirtualPath, outputFolder, caches, warnings);
            var normalImage = GetOrCreateTextureImage(project, doc, prim.NormalVirtualPath, outputFolder, caches, warnings);
            var specularImage = GetOrCreateTextureImage(project, doc, prim.SpecularVirtualPath, outputFolder, caches, warnings);

            int? albedoTexIndex = albedoImage.HasValue ? doc.AddOrGetTexture(albedoImage.Value) : null;
            int? normalTexIndex = normalImage.HasValue ? doc.AddOrGetTexture(normalImage.Value) : null;
            int? specularTexIndex = specularImage.HasValue ? doc.AddOrGetTexture(specularImage.Value) : null;

            var materialKey = $"{prim.AlbedoVirtualPath}|{prim.NormalVirtualPath}|{prim.SpecularVirtualPath}|{prim.AlphaMode}|{prim.MaterialName}";
            var materialIndex = doc.AddOrGetMaterial(materialKey, prim.MaterialName, albedoTexIndex, normalTexIndex,
                specularTexIndex, prim.AlphaMode);

            doc.AddPrimitive(meshIndex, prim.Positions, prim.Normals, prim.UVs, prim.Indices, materialIndex, includeNormals);
        }

        return meshIndex;
    }

    /// <summary>
    /// Same job as <see cref="BuildMeshFromFlver"/> but exports a real glTF skin instead of baked-in static
    /// geometry: one glTF joint node per FLVER node (index-for-index, so vertex JOINTS_0 values line up with
    /// the skin's joints array with no further remap needed) plus per-vertex bone weights. Returns null when
    /// the model has no bone/dummy hierarchy or no exportable geometry, so the caller can fall back to
    /// <see cref="BuildMeshFromFlver"/>.
    /// </summary>
    private static (int MeshIndex, int SkinIndex, int SkeletonRootNode)? BuildSkinnedMeshFromFlver(
        ProjectEntry project, FLVER2 flver, string flverVirtPath, string meshName, GltfDocumentWriter doc,
        string outputFolder, bool includeNormals, ExportCaches caches, List<string> warnings)
    {
        FlverSkinnedGeometryBaker.BakedSkinnedModel baked;
        try
        {
            baked = FlverSkinnedGeometryBaker.Bake(project, flver, flverVirtPath ?? "");
        }
        catch (Exception e)
        {
            warnings.Add($"Could not process skinned geometry for '{meshName}': {e.Message}");
            return null;
        }

        if (baked == null || baked.Primitives.Count == 0)
            return null;

        var nodes = baked.Nodes;

        // One glTF joint node per FLVER node, in the same order/index, wired up via each node's own
        // parent-relative local transform.
        var jointNodeIndices = new int[nodes.Count];
        for (var i = 0; i < nodes.Count; i++)
            jointNodeIndices[i] = doc.AddNode(nodes[i].Name, nodes[i].ComputeLocalTransform(), null);

        var topLevelNodes = new List<int>();
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].ParentIndex >= 0 && nodes[i].ParentIndex < nodes.Count)
                doc.AddChild(jointNodeIndices[nodes[i].ParentIndex], jointNodeIndices[i]);
            else
                topLevelNodes.Add(jointNodeIndices[i]);
        }

        // A skin's "skeleton" hint and the scene graph both want a single root - wrap in a synthetic one if
        // the FLVER's node list has more than one top-level entry (e.g. separate root dummies).
        var skeletonRootNode = topLevelNodes.Count == 1
            ? topLevelNodes[0]
            : doc.AddNode($"{meshName}_Skeleton", Matrix4x4.Identity, null, topLevelNodes);

        var inverseBindMatrices = new Matrix4x4[nodes.Count];
        for (var i = 0; i < nodes.Count; i++)
        {
            var worldMatrix = Utils.GetBoneWorldMatrix(nodes[i], nodes);
            inverseBindMatrices[i] = Matrix4x4.Invert(worldMatrix, out var inverted) ? inverted : Matrix4x4.Identity;
        }

        var skinIndex = doc.AddSkin(meshName, jointNodeIndices, inverseBindMatrices, skeletonRootNode);

        var meshIndex = doc.AddMesh(meshName);

        foreach (var prim in baked.Primitives)
        {
            var albedoImage = GetOrCreateTextureImage(project, doc, prim.AlbedoVirtualPath, outputFolder, caches, warnings);
            var normalImage = GetOrCreateTextureImage(project, doc, prim.NormalVirtualPath, outputFolder, caches, warnings);
            var specularImage = GetOrCreateTextureImage(project, doc, prim.SpecularVirtualPath, outputFolder, caches, warnings);

            int? albedoTexIndex = albedoImage.HasValue ? doc.AddOrGetTexture(albedoImage.Value) : null;
            int? normalTexIndex = normalImage.HasValue ? doc.AddOrGetTexture(normalImage.Value) : null;
            int? specularTexIndex = specularImage.HasValue ? doc.AddOrGetTexture(specularImage.Value) : null;

            var materialKey = $"{prim.AlbedoVirtualPath}|{prim.NormalVirtualPath}|{prim.SpecularVirtualPath}|{prim.AlphaMode}|{prim.MaterialName}";
            var materialIndex = doc.AddOrGetMaterial(materialKey, prim.MaterialName, albedoTexIndex, normalTexIndex,
                specularTexIndex, prim.AlphaMode);

            doc.AddPrimitive(meshIndex, prim.Positions, prim.Normals, prim.UVs, prim.Indices, materialIndex, includeNormals,
                prim.Joints, prim.Weights);
        }

        return (meshIndex, skinIndex, skeletonRootNode);
    }

    private static int? GetOrCreateTextureImage(ProjectEntry project, GltfDocumentWriter doc, string virtualTexPath,
        string outputFolder, ExportCaches caches, List<string> warnings)
    {
        if (string.IsNullOrEmpty(virtualTexPath))
            return null;

        if (caches.Images.TryGetValue(virtualTexPath, out var cached))
            return cached;

        try
        {
            var bytes = SoulsContainerIO.ReadTextureBytes(project, virtualTexPath, out var error);
            if (bytes == null || bytes.Length == 0)
            {
                warnings.Add($"Texture not found: {virtualTexPath}{(error != null ? $" ({error})" : "")}");
                caches.Images[virtualTexPath] = null;
                return null;
            }

            var fileNameNoExt = SanitizeFileName(Path.GetFileNameWithoutExtension(virtualTexPath));
            var texturesDir = Path.Combine(outputFolder, "textures");
            Directory.CreateDirectory(texturesDir);

            var pngPathNoExt = Path.Combine(texturesDir, fileNameNoExt);
            TexUtils.ExportPNGImage(pngPathNoExt, bytes);

            var imageIndex = doc.AddOrGetImage($"textures/{fileNameNoExt}.png");
            caches.Images[virtualTexPath] = imageIndex;
            return imageIndex;
        }
        catch (Exception e)
        {
            warnings.Add($"Failed to convert texture '{virtualTexPath}' to PNG: {e.Message}");
            caches.Images[virtualTexPath] = null;
            return null;
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "unnamed";

        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }

    #endregion
}
