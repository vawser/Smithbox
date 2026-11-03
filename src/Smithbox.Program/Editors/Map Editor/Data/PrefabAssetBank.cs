using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A part placed by a prefab asset, relative to the prefab.
/// </summary>
public record PrefabMember(string Name, string ModelName, Matrix4x4 LocalTransform);

/// <summary>
/// The contents of prefab assets (AEP), which Duskbloods places in maps as a single part. Each prefab model
/// points to an MSB of its own (asset/aep/mapstudio/aepXXX_XXX_000.msb) holding the parts it is made of.
/// Each MSB is read once, the first time a map using it is loaded.
/// </summary>
public class PrefabAssetBank
{
    private readonly ProjectEntry Project;

    private readonly ConcurrentDictionary<string, IReadOnlyList<PrefabMember>> Prefabs = new(StringComparer.OrdinalIgnoreCase);

    public PrefabAssetBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        return Project.Descriptor.ProjectType is ProjectType.DB_NT or ProjectType.DB;
    }

    public static bool IsPrefabModel(string modelName)
    {
        return modelName != null && modelName.StartsWith("AEP", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The parts of a prefab model. Empty if the name isn't a prefab, or its MSB can't be read.
    /// </summary>
    public IReadOnlyList<PrefabMember> GetMembers(string modelName)
    {
        if (!CanUse() || !IsPrefabModel(modelName))
            return Array.Empty<PrefabMember>();

        return Prefabs.GetOrAdd(modelName, ReadMembers);
    }

    /// <summary>
    /// The distinct models used by the parts of a prefab model.
    /// </summary>
    public IEnumerable<string> GetMemberModels(string modelName)
    {
        return GetMembers(modelName).Select(e => e.ModelName).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Forgets a prefab, so it is read again the next time it is placed (e.g. after its MSB is saved).
    /// </summary>
    public void Invalidate(string mapId)
    {
        // The MSB is named after the model with a variant suffix, e.g. aep100_001_000 for AEP100_001
        if (mapId == null || mapId.Length < 10 || !IsPrefabModel(mapId))
            return;

        Prefabs.TryRemove(mapId.Substring(0, 10), out _);
    }

    private IReadOnlyList<PrefabMember> ReadMembers(string modelName)
    {
        var fileName = $"{modelName.ToLower()}_000";
        var entry = Project.Locator.MapFiles.Entries.FirstOrDefault(e =>
            e.Filename.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
            return Array.Empty<PrefabMember>();

        try
        {
            var data = Project.VFS.FS.ReadFile(entry.Path);

            if (data == null)
                return Array.Empty<PrefabMember>();

            var msb = MSB_DB_NT.Read(data.Value);

            return msb.Parts.GetEntries()
                .Where(e => !string.IsNullOrEmpty(e.ModelName))
                .Select(e => new PrefabMember(e.Name, e.ModelName, GetTransform(e)))
                .ToList();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Prefab_Failed_To_Read", entry.Path), e);
            return Array.Empty<PrefabMember>();
        }
    }

    /// <summary>
    /// Built the same way as the transform of a part entity, so members sit where they would in their own MSB.
    /// </summary>
    private static Matrix4x4 GetTransform(MSB_DB_NT.Part part)
    {
        var t = Transform.Default;

        t.Position = part.Position;
        t.EulerRotation = new Vector3(
            Utils.DegToRadians(part.Rotation.X),
            Utils.DegToRadians(part.Rotation.Y),
            Utils.DegToRadians(part.Rotation.Z));
        t.Scale = part.Scale;

        return t.WorldMatrix;
    }
}
