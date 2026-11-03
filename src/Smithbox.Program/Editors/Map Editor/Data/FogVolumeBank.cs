using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Holds the FVB files of Armored Core VI, Elden Ring and Nightreign maps, which place the boxes of volumetric fog.
/// A map can have several, named after its MSB with a suffix (e.g. m31_06_00_00_0000 and _0002). A file is read
/// when its map is loaded, and its volumes become entities of the map.
/// The settings a volume is drawn with are the row of its FogID in the "FogVolume ParamEditor" group of the GPARAMs
/// the map lighting uses (<see cref="GparamLightingBank" />): the file, weather and time of day chosen in
/// Viewport Rendering > Map Lighting, then the area's own (mAA_BB_0000, after GparamRefSettings), mapbase (ER, NR),
/// default, and the area's other GPARAMs, such as m41_00_0100 in AC6. Each field is looked up in each file with the
/// row in turn, so a weather file only needs the fields it changes, and is interpolated through the day.
/// </summary>
public class FogVolumeBank
{
    public ProjectEntry Project;

    private const string FogGroupKey = "FogVolume ParamEditor";

    // The version and compression of each file read, so it is written back the same way
    private readonly Dictionary<string, (int Version, DCX.Type Compression)> FileInfo = new(StringComparer.OrdinalIgnoreCase);

    // The fog group of each GPARAM read, null if the GPARAM or its group is missing
    private readonly Dictionary<string, GPARAM.Param> FogGroups = new(StringComparer.OrdinalIgnoreCase);

    public FogVolumeBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
            return true;

        return false;
    }

    private IEnumerable<FileDictionaryEntry> GetEntries(string mapId)
    {
        return Project.Locator.FogVolumeFiles.Entries
            .Where(e => e.Filename.StartsWith($"{mapId}_", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Filename, StringComparer.OrdinalIgnoreCase);
    }

    private FVB ReadFile(FileDictionaryEntry entry, out DCX.Type compression)
    {
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        compression = DCX.Type.None;

        if (!fs.FileExists(entry.Path))
            return null;

        var data = DCX.Decompress(fs.ReadFile(entry.Path).Value, out compression);

        return FVB.Read(data);
    }

    /// <summary>
    /// The textures the fog volumes of a map use, so they can be loaded with the map's other textures
    /// </summary>
    public HashSet<string> GetTextureNames(string mapId)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!CanUse())
            return names;

        foreach (var entry in GetEntries(mapId))
        {
            try
            {
                var fvb = ReadFile(entry, out _);

                if (fvb == null)
                    continue;

                foreach (var volume in fvb.Volumes)
                {
                    foreach (var texture in new[] { volume.HeightMapName, volume.ColorMapName, volume.TextureName3 })
                    {
                        if (!string.IsNullOrEmpty(texture))
                            names.Add(texture);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_FVB", entry.Path), e);
            }
        }

        return names;
    }

    public void LoadFVB(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in GetEntries(map.Name))
        {
            try
            {
                var fvb = ReadFile(entry, out var compression);

                if (fvb == null)
                    continue;

                lock (FileInfo)
                {
                    FileInfo[entry.Filename] = (fvb.Version, compression);
                }

                map.LoadFVB(entry.Filename, fvb);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_FVB", entry.Path), e);
            }
        }
    }

    public bool SaveFVB(MapContainer map)
    {
        if (!CanUse())
            return true;

        var success = true;
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        foreach (var parent in map.FogVolumeParents)
        {
            var fileName = parent.WrappedObject.ToString();
            var entry = GetEntries(map.Name).FirstOrDefault(e => e.Filename.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                continue;

            try
            {
                (int Version, DCX.Type Compression) info;

                lock (FileInfo)
                {
                    info = FileInfo.GetValueOrDefault(fileName, (Project.Descriptor.ProjectType is ProjectType.NR ? 2 : 1, DCX.Type.DCX_KRAK));
                }

                var fvb = new FVB { Version = info.Version };

                foreach (var child in parent.Children)
                {
                    if (child.WrappedObject is FVB.Volume volume)
                    {
                        fvb.Volumes.Add(volume);
                    }
                }

                var newBytes = fvb.Write();

                // Only write the file if the volumes have changed, so an unedited map isn't added to the project
                if (fs.FileExists(entry.Path))
                {
                    var oldBytes = DCX.Decompress(fs.ReadFile(entry.Path).Value, out _);

                    // The game's files keep old data in the unused space of their strings, which is written as zeroes,
                    // so an unedited file is compared by what it holds rather than by its bytes
                    var oldWritten = FVB.Read(oldBytes).Write();

                    if (BytePerfectHelper.Md5Equal(oldWritten, newBytes))
                        continue;
                }

                Project.VFS.ProjectFS.WriteFile(entry.Path, DCX.Compress(newBytes, info.Compression));

                Smithbox.Log(this, LOC.Get("MAP_Data_Save_FVB_PASS", fileName));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_FVB", entry.Path), e);
                success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// Goes up whenever the settings of a volume may have changed: the file, weather or time of day chosen, or the
    /// GPARAMs read again. Volumes drawn with an older one look their settings up again.
    /// </summary>
    public int SettingsVersion { get; private set; }

    // What the settings were last looked up with, and the version volumes were last refreshed to
    private (bool Lighting, string File, int Weather, float Hours)? SettingsInputs;
    private int RefreshedVersion = -1;

    /// <summary>
    /// The settings of a fog ID for a map, from the GPARAMs the map lighting uses, see the class
    /// </summary>
    public FogVolumeSettings GetSettings(string mapId, int fogId)
    {
        if (!CanUse() || mapId == null || mapId.Length < 6)
            return FogVolumeSettings.Default;

        var lighting = Project.Handler.MapData.GparamLightingBank;

        var own = lighting.GetDefaultFile(mapId);
        var chosen = CFG.Current.Viewport_Map_Lighting ? lighting.ChooseFile(mapId, CFG.Current.Viewport_Map_Lighting_File) : own;

        // The area's other GPARAMs come in ListFiles' order: the plain ones (mAA_BB_0100) before those for events
        // and weather
        var names = new[] { chosen, own, "mapbase", "default" }
            .Concat(lighting.ListFiles(mapId))
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var rows = new List<(string Name, GPARAM.Param Group)>();

        foreach (var name in names)
        {
            var group = GetFogGroup(name);

            if (group != null && group.Fields.Any(f => f.Values.Any(v => v.ID == fogId)))
                rows.Add((name, group));
        }

        if (rows.Count == 0)
            return FogVolumeSettings.Default;

        return ReadSettings(rows, fogId, CFG.Current.Viewport_Map_Lighting_Hours);
    }

    /// <summary>
    /// Looks the settings of the loaded maps' fog volumes up again when the Map Lighting menu's file, weather or
    /// time of day changes, or the GPARAMs have been read again. Called every frame by the Map Editor.
    /// </summary>
    public void Update(IEnumerable<MapContainer> maps)
    {
        if (!CanUse())
            return;

        var inputs = (CFG.Current.Viewport_Map_Lighting, CFG.Current.Viewport_Map_Lighting_File ?? "",
            CFG.Current.Viewport_Map_Lighting_Weather, CFG.Current.Viewport_Map_Lighting_Hours);

        if (SettingsInputs != inputs)
        {
            SettingsInputs = inputs;
            SettingsVersion++;
        }

        if (RefreshedVersion == SettingsVersion)
            return;

        RefreshedVersion = SettingsVersion;

        foreach (var map in maps)
        {
            if (map.FogVolumeParents == null)
                continue;

            foreach (var parent in map.FogVolumeParents)
            {
                foreach (var child in parent.Children)
                {
                    if (child is MsbEntity entity)
                        entity.RefreshFogVolume();
                }
            }
        }
    }

    private GPARAM.Param GetFogGroup(string gparamName)
    {
        lock (FogGroups)
        {
            if (FogGroups.TryGetValue(gparamName, out var cached))
                return cached;
        }

        GPARAM.Param group = null;

        var entry = Project.Locator.GparamFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals(gparamName, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

            try
            {
                if (fs.FileExists(entry.Path))
                {
                    var gparam = GPARAM.Read(fs.ReadFile(entry.Path).Value);
                    group = gparam.Params.FirstOrDefault(p => p.Key == FogGroupKey);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_Fog_GPARAM", entry.Path), e);
            }
        }

        lock (FogGroups)
        {
            FogGroups[gparamName] = group;
        }

        return group;
    }

    /// <summary>
    /// Forget the GPARAMs read, so edits to them are picked up the next time the volumes are drawn
    /// </summary>
    public void ClearSettingsCache()
    {
        lock (FogGroups)
        {
            FogGroups.Clear();
        }

        SettingsVersion++;
    }

    /// <summary>
    /// The settings of a fog ID from the files that have its row, in order, at a time of day
    /// </summary>
    private static FogVolumeSettings ReadSettings(List<(string Name, GPARAM.Param Group)> rows, int fogId, float hours)
    {
        var d = FogVolumeSettings.Default;

        // Each field is looked for in the row of each file in turn. A row that leaves a field out of every file
        // takes the value of row 0 in the first file, as the game's tools fill new rows from it.
        T Get<T>(string name, T fallback, bool isAngle = false)
        {
            var key = $"FogVolume {name}";

            foreach (var (_, group) in rows)
            {
                var field = group.Fields.FirstOrDefault(f => f.Key == key);

                // Some files store a field as another number type, e.g. HeightFallOff as an int in AC6, which
                // TryGetValue converts
                if (field != null && GparamLightingBank.TryGetValue(field, fogId, hours, fallback, out T value, isAngle))
                    return value;
            }

            var first = rows[0].Group.Fields.FirstOrDefault(f => f.Key == key);

            if (first != null && GparamLightingBank.TryGetValue(first, 0, hours, fallback, out T rowZero, isAngle))
                return rowZero;

            return fallback;
        }

        return new FogVolumeSettings
        {
            NearFadeMinDistance = Get("DrawArea NearFadeMinDistance", d.NearFadeMinDistance),
            NearFadeMaxDistance = Get("DrawArea NearFadeMaxDistance", d.NearFadeMaxDistance),
            FarFadeMinDistance = Get("DrawArea FarFadeMinDistance", d.FarFadeMinDistance),
            FarFadeMaxDistance = Get("DrawArea FarFadeMaxDistance", d.FarFadeMaxDistance),
            EdgeFadeDistance = Get("DrawArea EdgeFadeDistance", d.EdgeFadeDistance),
            EdgeFadeExponent = Get("DrawArea EdgeFadeExponent", d.EdgeFadeExponent),
            HeightFallOff = Get("DrawArea HeightFallOff", d.HeightFallOff),
            FadeDistanceForHeightMap = Get("DrawArea FadeDistanceForHeightMap", d.FadeDistanceForHeightMap),
            MaxInjectingDepth = Get("DrawArea MaxInjectingDepth", d.MaxInjectingDepth),
            MaxSampleCount = Get("RayMarching MaxSampleCount", d.MaxSampleCount),
            SteppingDistance = Get("RayMarching SteppingDistance", d.SteppingDistance),
            SuspendedTransmittance = Get("RayMarching SuspendedTransmittance", d.SuspendedTransmittance),
            LightProcessingInterval = Get("RayMarching LightProcessingInterval", d.LightProcessingInterval),
            LightProcessingIntervalThreshold = Get("RayMarching LightProcessingIntervalThreshold", d.LightProcessingIntervalThreshold),
            ShadowingIntensity = Get("Light ShadowingIntensity", d.ShadowingIntensity),
            DirectionalLightAngle = Get("Light DirectionalLightAngle", d.DirectionalLightAngle, isAngle: true),
            DirectionalLightColor = Get("Light DirectionalLightColor", d.DirectionalLightColor),
            DirectionalLightOppositeColor = Get("Light DirectionalLightOppositeColor", d.DirectionalLightOppositeColor),
            AmbientColor = Get("Light AmbientColor", d.AmbientColor),
            LightOverwrite = Get("Light Overwrite", d.LightOverwrite),
            AttenuationIntensity = Get("Light AttenuationIntensity", d.AttenuationIntensity),
            AttenuationOffset = Get("Light AttenuationOffset", d.AttenuationOffset),
            OctaveNumPerlinNoise = Get("Light OctaveNumPerlinNoise", d.OctaveNumPerlinNoise),
            ColorMapTextureIntensity = Get("Light ColorMapTextureIntensity", d.ColorMapTextureIntensity),
            ColorMapTextureUVTiling = new Vector2(
                Get("Light ColorMapTextureUVTiling-x", d.ColorMapTextureUVTiling.X),
                Get("Light ColorMapTextureUVTiling-y", d.ColorMapTextureUVTiling.Y)),
            DensityIntensity = Get("Density Intensity", d.DensityIntensity),
            PerlinNoiseFrequency = Get("PerlinNoise Frequency", d.PerlinNoiseFrequency),
            PerlinNoiseLacunarity = Get("PerlinNoise Lacunarity", d.PerlinNoiseLacunarity),
            PerlinNoiseGain = Get("PerlinNoise Gain", d.PerlinNoiseGain),
            PerlinNoiseOffset = Get("PerlinNoise Offset", d.PerlinNoiseOffset),
            ScrollSpeed = new Vector3(
                Get("Scroll Speed-x", d.ScrollSpeed.X),
                Get("Scroll Speed-y", d.ScrollSpeed.Y),
                Get("Scroll Speed-z", d.ScrollSpeed.Z)),
            ScrollDetailScale = Get("Scroll DetailScale", d.ScrollDetailScale),
            Source = rows[0].Name
        };
    }
}
