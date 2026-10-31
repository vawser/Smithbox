using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The instances of one grass type on a part.
/// </summary>
public record GrassLayer(string ModelName, Matrix4x4[] Transforms);

/// <summary>
/// Builds the grass displayed on parts. A part names up to six GrassTypeParam rows, and the GRASS file
/// of its model holds a density per row for each vertex. Nothing is read until grass is displayed.
/// </summary>
public class GrassBank
{
    // Applied per frame, as each layer of each part creates a mesh
    private const int MaxAppliedPerFrame = 128;

    private record GrassResult(MsbEntity Entity, string Key, List<GrassLayer> Layers);

    private readonly ProjectEntry Project;

    private readonly ConcurrentDictionary<string, Task<GRASS>> Files = new();
    private readonly ConcurrentQueue<GrassResult> Completed = new();
    private readonly Dictionary<int, GrassType> Types = new();
    private readonly HashSet<MeshProvider> HeldProviders = new();

    private Dictionary<string, string> ModelPaths;

    private bool Visible = false;
    private bool AwaitingParams = false;
    private float Density = 1f;

    public GrassBank(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Picks up changes in what should be displayed and applies finished parts.
    /// Checked here as the filter can be changed from several places (e.g. the presets).
    /// </summary>
    public void Update(bool visible)
    {
        var refresh = visible && !Visible;

        Visible = visible;

        if (visible && Density != CFG.Current.MapEditor_Grass_Density)
        {
            Density = CFG.Current.MapEditor_Grass_Density;
            refresh = true;
        }

        if (visible && AwaitingParams && GetGrassTypeParam() != null)
        {
            refresh = true;
        }

        if (refresh)
        {
            Refresh();
        }

        ApplyCompleted();
    }

    /// <summary>
    /// Requests the grass of a part, if it isn't already what is displayed.
    /// </summary>
    public void Request(MsbEntity entity)
    {
        if (!Visible)
            return;

        // A part that has left the map still gets here when it is deselected, and any grass it
        // asked for would be treated as displayed when the part is put back
        if (entity.Parent == null)
            return;

        var ids = entity.GetGrassParamIds();

        if (ids == null || ids.All(id => id <= 0))
        {
            if (entity.GrassKey != null)
            {
                entity.ClearGrass();
            }

            return;
        }

        var param = GetGrassTypeParam();

        if (param == null)
        {
            AwaitingParams = true;
            return;
        }

        var key = $"{entity.GetPartModelName()}:{string.Join(',', ids)}:{Density}";

        if (key == entity.GrassKey)
            return;

        entity.ClearGrass();

        var path = GetModelPath(entity.GetGrassModelFile());

        if (path == null)
            return;

        var types = ids.Select(id => GetGrassType(param, id)).ToArray();

        entity.GrassKey = key;

        var density = Density;
        var seed = GetSeed(entity.Name);
        var file = Files.GetOrAdd(path, _ => Task.Run(() => ReadGrass(path)));

        Task.Run(async () =>
        {
            var grass = await file;

            if (grass == null)
                return;

            List<GrassLayer> layers = new();

            for (var slot = 0; slot < types.Length; slot++)
            {
                if (types[slot] == null)
                    continue;

                var transforms = GrassPlacement.Scatter(grass, slot, types[slot], density, seed + slot);

                if (transforms.Length > 0)
                {
                    layers.Add(new GrassLayer(types[slot].ModelName, transforms));
                }
            }

            Completed.Enqueue(new GrassResult(entity, key, layers));
        });
    }

    /// <summary>
    /// Keeps a grass model loaded for as long as the project is open. Grass models usually aren't placed in
    /// the map, so grass holds the only references to them. Without this, rebuilding the last grass using a
    /// model releases it, and the queued unload can run after the new mesh has asked for it.
    /// </summary>
    public void HoldModel(IResourceHandle handle)
    {
        var provider = MeshProviderCache.GetProvider(handle);

        if (HeldProviders.Add(provider))
        {
            provider.Acquire();
        }
    }

    /// <summary>
    /// Rebuilds everything from the current data, so changes to the files or params show when grass is turned back on.
    /// </summary>
    private void Refresh()
    {
        AwaitingParams = false;

        Files.Clear();

        lock (Types)
        {
            Types.Clear();
        }

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var ent in entry.Value.MapContainer.Objects)
                {
                    // Entities still being set up get their grass along with the rest of their drawable
                    if (ent is MsbEntity { SetupRenderMesh: true } msbEnt)
                    {
                        msbEnt.ClearGrass();
                        msbEnt.RefreshGrass();
                    }
                }
            }
        }
    }

    private void ApplyCompleted()
    {
        HashSet<string> loadedModels = new();

        for (var i = 0; i < MaxAppliedPerFrame && Completed.TryDequeue(out var result); i++)
        {
            // The part may have changed, or left the map, since this was requested
            if (result.Entity.GrassKey == result.Key && result.Entity.RenderSceneMesh != null)
            {
                result.Entity.ApplyGrass(result.Layers, loadedModels);
            }
        }
    }

    private Param GetGrassTypeParam()
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank?.Params == null)
            return null;

        return bank.Params.TryGetValue("GrassTypeParam", out var param) ? param : null;
    }

    private GrassType GetGrassType(Param param, int id)
    {
        if (id <= 0)
            return null;

        lock (Types)
        {
            if (!Types.TryGetValue(id, out var type))
            {
                type = ReadGrassType(param[id]);
                Types[id] = type;
            }

            return type;
        }
    }

    private static GrassType ReadGrassType(Param.Row row)
    {
        if (row == null)
            return null;

        // Called model0Name in ER, as it has a second model for its lower detail levels
        var modelName = GetText(row, "model0Name") ?? GetText(row, "modelName") ?? GetText(row, "simpleModelName");

        if (string.IsNullOrEmpty(modelName))
            return null;

        return new GrassType(
            modelName,
            GetNumber(row, "baseDensity", 1),
            GetNumber(row, "normalInfluence", 0),
            GetNumber(row, "inclinationMax", 90),
            GetNumber(row, "inclinationJitter", 0),
            GetNumber(row, "scaleBaseMin", 100),
            GetNumber(row, "scaleBaseMax", 100),
            GetNumber(row, "scaleHeightMin", 100),
            GetNumber(row, "scaleHeightMax", 100),
            GetNumber(row, "orientationAngle", -1),
            GetNumber(row, "orientationRange", -1),
            GetNumber(row, "spacing", 0));
    }

    private static string GetText(Param.Row row, string field)
    {
        var text = row[field]?.Value as string;

        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static float GetNumber(Param.Row row, string field, float fallback)
    {
        var value = row[field]?.Value;

        return value == null ? fallback : Convert.ToSingle(value);
    }

    /// <summary>
    /// Gets the path of the binder that holds a map piece or asset model.
    /// </summary>
    private string GetModelPath(string modelFile)
    {
        lock (Files)
        {
            if (ModelPaths == null)
            {
                ModelPaths = new(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in Project.Locator.MapPieceFiles.Entries.Concat(Project.Locator.AssetFiles.Entries))
                {
                    ModelPaths.TryAdd(entry.Filename, entry.Path);
                }
            }

            return ModelPaths.GetValueOrDefault(modelFile);
        }
    }

    private GRASS ReadGrass(string path)
    {
        try
        {
            var data = Project.VFS.FS.ReadFile(path);

            if (data == null)
                return null;

            var binder = BND4.Read(data.Value);
            var file = binder.Files.FirstOrDefault(e => e.Name.EndsWith(".grass", StringComparison.OrdinalIgnoreCase));

            return file == null ? null : GRASS.Read(file.Bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Grass_Failed_To_Read", path), e);
            return null;
        }
    }

    /// <summary>
    /// String.GetHashCode changes between runs, which would move the grass of every part.
    /// </summary>
    private static int GetSeed(string name)
    {
        var hash = 17;

        foreach (var c in name)
        {
            hash = unchecked(hash * 31 + c);
        }

        return hash;
    }
}
