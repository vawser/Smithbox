using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.MapEditor;

public class PrefabAttributes
{
    public string PrefabName { get; set; } = "";
    public string PrefixSeparator { get; set; } = "[]";
    public ProjectType Type { get; set; }

    public List<string> TagList { get; set; }

    [JsonConstructor]
    public PrefabAttributes() { }

    public PrefabAttributes(MapEditorView view)
    {
        Type = view.Project.Descriptor.ProjectType;
    }
}

public abstract class Prefab : PrefabAttributes
{
    protected Prefab() : base() { }

    protected Prefab(MapEditorView view) : base(view)
    {
    }

    public abstract bool ImportJson(string path);
    public abstract List<MsbEntity> GenerateMapEntities(MapContainer targetMap);
    public abstract void ExportSelection(string filepath, string name, string tags, ViewportSelection _selection);

    protected abstract IMsb Map();

    public static Prefab New(MapEditorView view)
    {
        return view.Project.Descriptor.ProjectType switch
        {
            ProjectType.NR => new Prefab<MSB_NR>(view),
            ProjectType.ER => new Prefab<MSBE>(view),
            ProjectType.SDT => new Prefab<MSBS>(view),
            ProjectType.DS1 or ProjectType.DS1R => new Prefab<MSB1>(view),
            ProjectType.DS2 or ProjectType.DS2S => new Prefab<MSB2>(view),
            ProjectType.DS3 => new Prefab<MSB3>(view),
            ProjectType.AC6 => new Prefab<MSB_AC6>(view),
            ProjectType.BB => new Prefab<MSBB>(view),
            _ => null,
        };
    }

    public void ImportToMap(MapEditorView view, MapContainer targetMap, RenderScene _scene, ViewportActionManager _actionManager, string prefixName = null)
    {
        if (targetMap is null)
        {
            Smithbox.LogError(this, $"Failed to create prefab {PrefabName}: Target map is null.");
            return;
        }

        prefixName ??= PrefabName;
        var parent = targetMap.RootObject;
        List<MsbEntity> ents = GenerateMapEntities(targetMap);

        if (CFG.Current.Prefab_PlaceAtPlacementOrb)
        {
            var placementOrigin = view.ViewportWindow.GetPlacementPosition();

            Vector3 currentCenter = Vector3.Zero;
            foreach (var entity in ents)
            {
                if (EntityHelper.IsPart(entity) || EntityHelper.IsRegion(entity))
                {
                    var position = entity.GetPropertyValue<Vector3>("Position");
                    currentCenter += position;
                }
            }

            currentCenter /= ents.Count;

            Vector3 offset = placementOrigin - currentCenter;

            foreach (var entity in ents)
            {
                var position = entity.GetPropertyValue<Vector3>("Position");
                var newPos = position += offset;
                entity.SetPropertyValue("Position", newPos);
            }
        }

        var entries = ents.Select((entity) => entity.WrappedObject as IMsbEntry);

        foreach (var entry in entries)
        {
            MsbUtils.RenameWithRefs(entries, entry, prefixName + entry.Name);
        }

        AddMapObjectsAction act = new(view, targetMap, ents, true, parent);
        _actionManager.ExecuteAction(act);
    }

    public List<string> GetSelectedPrefabObjects()
    {
        List<string> entNames = new();
        foreach (var entry in PrefabUtils.GetMapMsbEntries(Map()))
        {
            var typeName = entry.GetType().Name;
            entNames.Add($"{typeName} - {entry.Name}");
        }
        return entNames;
    }
}

internal class Prefab<T> : Prefab
    where T : SoulsFile<T>, IMsb, new()
{
    private MapEditorView View;

    public byte[] AssetContainerBytes { get; set; }

    [JsonIgnore]
    public T pseudoMap;

    [JsonConstructor]
    public Prefab() : base() { }

    public Prefab(MapEditorView view) : base(view)
    {
        View = view;
    }

    protected override IMsb Map()
    {
        return pseudoMap;
    }

    public PrefabAttributes Attributes() => this;

    void Load(HashSet<MsbEntity> entities)
    {
        pseudoMap = new();
        MapContainer map = new(View, null);
        var entries = entities.Select(ent => ent.WrappedObject as IMsbEntry);
        foreach (var ent in entities)
        {
            var copy = ent.Clone();
            var entry = ent.WrappedObject as IMsbEntry;
            if (!CFG.Current.Prefab_IncludeEntityID)
            {
                ent.GetType().GetProperty("EntityID")?.SetValue(ent, 0);
            }

            if (!CFG.Current.Prefab_IncludeEntityGroupIDs)
            {
                ent.GetType().GetProperty("EntityGroupIDs")?.SetValue(ent, new uint[] { });
            }

            MsbUtils.StripMsbReference(entries, copy.WrappedObject as IMsbEntry);

            map.AddObject(copy);
        }
        map.SerializeToMSB(pseudoMap, Type);
    }

    public override List<MsbEntity> GenerateMapEntities(MapContainer targetMap)
    {
        IEnumerable<MsbEntity> Entities<Category>(IMsbParam<Category> category, MsbEntityType type, Func<Category, Category> copy)
            where Category : IMsbEntry
        {
            foreach (var part in category.GetEntries())
            {
                var entity = new MsbEntity(View.Universe, targetMap, copy(part)) { Type = type };
                yield return entity;
            }
        }

        return new IEnumerable<MsbEntity>[] {
            Entities(pseudoMap.Parts, MsbEntityType.Part, p => p.DeepCopy()),
            Entities(pseudoMap.Events, MsbEntityType.Event, p => p.DeepCopy()),
            Entities(pseudoMap.Regions, MsbEntityType.Region, p => p.DeepCopy()),
        }
        .SelectMany(e => e)
        .ToList();
    }

    /// <summary>
    /// Exports AssetPrefab to json file.
    /// </summary>
    public bool Write(string path)
    {
        try
        {
            AssetContainerBytes = pseudoMap.Write();

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox(
                $"Unable to export Prefab due to the following error:\n\n{e.Message}\n{e.StackTrace}",
                "Prefab export error",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
    }

    /// <summary>
    /// Imports AssetPrefab info from json file.
    /// </summary>
    public override bool ImportJson(string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                Converters = { new PrefabAttributesConverter(View) }
            };

            var loaded = JsonSerializer.Deserialize<Prefab<T>>(File.ReadAllText(path), options);

            if (loaded != null)
            {
                PrefabName = loaded.PrefabName;
                PrefixSeparator = loaded.PrefixSeparator;
                Type = loaded.Type;
                TagList = loaded.TagList;
                AssetContainerBytes = loaded.AssetContainerBytes;
            }

            pseudoMap = SoulsFile<T>.Read(AssetContainerBytes);
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError<Prefab>("Unable to import Prefab due to the following error:", e);

            return false;
        }
    }

    /// <summary>
    /// Export
    /// </summary>
    public override void ExportSelection(string filepath, string name, string tags, ViewportSelection _selection)
    {
        Load(_selection.GetFilteredSelection<MsbEntity>());
        if (!PrefabUtils.GetMapMsbEntries(pseudoMap).Any())
        {
            Smithbox.LogError<Prefab>("Export failed, nothing in selection could be exported.");
            return;
        }
        PrefabName = name;
        TagList = tags.Split(",").ToList();
        Write(filepath);
    }
}

public class PrefabAttributesConverter : JsonConverter<PrefabAttributes>
{
    private readonly MapEditorView _view;

    public PrefabAttributesConverter(MapEditorView view)
    {
        _view = view;
    }

    public override PrefabAttributes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var prefab = new PrefabAttributes(_view);

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("PrefabName", out var prefabName))
            prefab.PrefabName = prefabName.GetString() ?? "";

        if (root.TryGetProperty("PrefixSeparator", out var prefixSeparator))
            prefab.PrefixSeparator = prefixSeparator.GetString() ?? "[]";

        if (root.TryGetProperty("Type", out var type))
            prefab.Type = JsonSerializer.Deserialize<ProjectType>(type.GetRawText(), options);

        if (root.TryGetProperty("TagList", out var tagList))
            prefab.TagList = JsonSerializer.Deserialize<List<string>>(tagList.GetRawText(), options);

        return prefab;
    }

    public override void Write(Utf8JsonWriter writer, PrefabAttributes value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}