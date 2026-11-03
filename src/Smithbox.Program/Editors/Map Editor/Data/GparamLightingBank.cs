using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Reads the lighting of Armored Core VI, Elden Ring and Nightreign maps from their GPARAMs: the sun, hemisphere
/// ambient and exposure (<see cref="GparamLighting" />).
/// The map's GPARAMs only hold what differs from the shared ones, so each field is looked up in the chosen file
/// (mAA_BB_0000 by default), then mapbase, then default. Within a file a field holds values by ID, numbered
/// separately in each group: LightSet's ID 0 is the scene's lighting and 100 the lighting of models, with ID 0 used
/// when a file has no value for the ID asked for. Each ID can hold several values through the day, keyed by
/// TimeOfDay, which are interpolated.
/// See Documentation/Full GI/Gparam Lighting.md.
/// </summary>
public class GparamLightingBank
{
    public ProjectEntry Project;

    /// <summary>The LightSet value ID of the scene's lighting</summary>
    public const int SceneLightSetId = 0;

    /// <summary>The LightSet value ID of the lighting of models, which adds the fill lights</summary>
    public const int ModelLightSetId = 100;

    private const string LightSetKey = "LightSet ParamEditor";
    private const string ToneMapKey = "Tone Map[Yebis]";
    private const string EnvironmentConfigKey = "EnvironmentConfig ParamEditor";
    private const string SettingInfoKey = "Setting Info[Yebis]";

    // The shared GPARAMs a field is looked for in when the map's own file doesn't have it
    private static readonly string[] SharedFiles = { "mapbase", "default" };

    // Each GPARAM read, null if it is missing or couldn't be read
    private readonly Dictionary<string, GPARAM> Files = new(StringComparer.OrdinalIgnoreCase);

    public GparamLightingBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
            return true;

        return false;
    }

    /// <summary>
    /// The map's own GPARAM, mAA_BB_0000 for map mAA_BB_CC_DD
    /// </summary>
    public static string GetDefaultFile(string mapId)
    {
        if (mapId == null || mapId.Length < 6)
            return null;

        return $"{mapId.Substring(0, 6)}_0000";
    }

    /// <summary>
    /// The GPARAM to look a map's lighting up in first: the chosen file if it belongs to the map's area, else the
    /// map's own
    /// </summary>
    public static string ChooseFile(string mapId, string chosen)
    {
        var own = GetDefaultFile(mapId);

        if (own == null || string.IsNullOrEmpty(chosen))
            return own;

        return chosen.StartsWith(own.Substring(0, 7), StringComparison.OrdinalIgnoreCase) ? chosen : own;
    }

    /// <summary>
    /// The GPARAMs of a map's area, mAA_BB_*, the plain ones before those for events and weather
    /// </summary>
    public List<string> ListFiles(string mapId)
    {
        if (mapId == null || mapId.Length < 6)
            return new List<string>();

        var prefix = $"{mapId.Substring(0, 6)}_";

        return Project.Locator.GparamFiles.Entries
            .Select(e => e.Filename)
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n.Length)
            .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Forget the GPARAMs read, so edits to them are picked up the next time the lighting is resolved
    /// </summary>
    public void ClearCache()
    {
        lock (Files)
        {
            Files.Clear();
        }
    }

    private GPARAM GetFile(string name)
    {
        lock (Files)
        {
            if (Files.TryGetValue(name, out var cached))
                return cached;
        }

        GPARAM gparam = null;

        var entry = Project.Locator.GparamFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

            try
            {
                if (fs.FileExists(entry.Path))
                {
                    gparam = GPARAM.Read(fs.ReadFile(entry.Path).Value);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Lighting_Failed_Read_GPARAM", entry.Path), e);
            }
        }

        lock (Files)
        {
            Files[name] = gparam;
        }

        return gparam;
    }

    /// <summary>
    /// The lighting of a map, looked up in <paramref name="file" /> (the map's own GPARAM if null or empty), then
    /// mapbase, then default, with <paramref name="lightSetId" /> for the LightSet group and ID 0 for the others,
    /// at <paramref name="hours" /> on the GPARAMs' TimeOfDay scale
    /// </summary>
    public GparamLighting Resolve(string mapId, string file, int lightSetId, float hours)
    {
        if (string.IsNullOrEmpty(file))
            file = GetDefaultFile(mapId);

        var lighting = new GparamLighting
        {
            File = file ?? "",
            LightSetId = lightSetId,
            Hours = hours
        };

        if (!CanUse())
            return lighting;

        var chain = new List<(string Name, GPARAM Gparam)>();

        foreach (var name in new[] { file }.Concat(SharedFiles))
        {
            if (string.IsNullOrEmpty(name) || chain.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                continue;

            var gparam = GetFile(name);

            if (gparam != null)
                chain.Add((name, gparam));
        }

        var reader = new FieldReader(chain, hours, lighting.Sources);

        // LightSet
        ReadLight(reader, lightSetId, lighting.Sun, "Directional Light Angle0", "Directional Light DiffColor0",
            "Directional Light SpecColor0", null, null);
        // Light1 has no switch; an intensity of 0 leaves it out
        ReadLight(reader, lightSetId, lighting.Light1, "Directional Light Angle1", "Directional Light DiffColor1",
            "Directional Light SpecColor1", null, "Directional LightLight1 FollowedCamera");
        ReadLight(reader, lightSetId, lighting.LocalLight1, "Directional Light LocalLight Angle1",
            "Directional Light LocalLight DiffColor1", "Directional Light LocalLight SpecColor1",
            "Directional Enable LocalLight1", null);
        ReadLight(reader, lightSetId, lighting.LocalLight2, "Directional Light LocalLight Angle2",
            "Directional Light LocalLight DiffColor2", "Directional Light LocalLight SpecColor2",
            "Directional Enable LocalLight2", null);

        lighting.HemiUp = reader.Get(LightSetKey, "Directional Light Hemi Color Up", lightSetId, lighting.HemiUp);
        lighting.HemiDown = reader.Get(LightSetKey, "Directional Light Hemi Color Down", lightSetId, lighting.HemiDown);

        // Tone Map
        lighting.Exposure = reader.Get(ToneMapKey, "GS Presents - ToneMap-Exposure", 0, lighting.Exposure);
        lighting.AutoExposure = reader.Get(ToneMapKey, "GS Presents - ToneMap-Enable AutoExposure", 0, lighting.AutoExposure);
        lighting.MiddleGray = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure Middle Gray", 0, lighting.MiddleGray);

        // EnvironmentConfig
        lighting.EnvMapHue = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Hue", 0, lighting.EnvMapHue);
        lighting.EnvMapSaturate = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Saturate", 0, lighting.EnvMapSaturate);
        lighting.EnvMapShift = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Shift", 0, lighting.EnvMapShift);
        lighting.EnvMapScale = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Scale", 0, lighting.EnvMapScale);

        // Setting Info
        lighting.BackgroundColor = reader.Get(SettingInfoKey, "GS - BG Color-Data", 0, lighting.BackgroundColor);

        return lighting;
    }

    private static void ReadLight(FieldReader reader, int id, GparamDirectionalLight light,
        string angleKey, string diffuseKey, string specularKey, string enableKey, string followKey)
    {
        light.Angle = reader.Get(LightSetKey, angleKey, id, light.Angle, isAngle: true);
        light.Diffuse = reader.Get(LightSetKey, diffuseKey, id, light.Diffuse);
        light.Specular = reader.Get(LightSetKey, specularKey, id, light.Specular);

        if (enableKey != null)
            light.Enabled = reader.Get(LightSetKey, enableKey, id, light.Enabled);

        if (followKey != null)
            light.FollowCamera = reader.Get(LightSetKey, followKey, id, light.FollowCamera);
    }

    /// <summary>
    /// Temporary diagnostics, see Gparam Lighting.md Phase 1: the files a map's lighting comes from and the scene's
    /// lighting at the viewport's time of day
    /// </summary>
    public void LogMap(MapContainer map)
    {
        if (!CanUse() || map == null)
            return;

        var file = ChooseFile(map.Name, CFG.Current.Viewport_Map_Lighting_File);

        var l = Resolve(map.Name, file, SceneLightSetId, CFG.Current.Viewport_Map_Lighting_Hours);

        string Source(string key) => l.Sources.TryGetValue(key, out var s) ? s : "-";

        Smithbox.Log(this, LOC.Get("MAP_Lighting_Load_Summary",
            map.Name,
            string.Join(" ", ListFiles(map.Name)),
            l.File,
            l.Hours,
            FormatColor(l.Sun.Diffuse), FormatAngle(l.Sun.Angle), Source("Directional Light DiffColor0"),
            FormatColor(l.HemiUp), FormatColor(l.HemiDown),
            l.Exposure, l.AutoExposure, l.MiddleGray, Source("GS Presents - ToneMap-Exposure")));
    }

    public static string FormatColor(Vector4 c) => $"({c.X:0.###}, {c.Y:0.###}, {c.Z:0.###}) x {c.W:0.###}";

    public static string FormatAngle(Vector2 a) => $"({a.X:0.###}, {a.Y:0.###})";

    /// <summary>
    /// Looks fields up along the chain of GPARAMs, and interpolates their values through the day
    /// </summary>
    private class FieldReader
    {
        private readonly List<(string Name, GPARAM Gparam)> Chain;
        private readonly float Hours;
        private readonly Dictionary<string, string> Sources;

        public FieldReader(List<(string Name, GPARAM Gparam)> chain, float hours, Dictionary<string, string> sources)
        {
            Chain = chain;
            Hours = hours;
            Sources = sources;
        }

        public T Get<T>(string groupKey, string fieldKey, int id, T fallback, bool isAngle = false)
        {
            foreach (var (name, gparam) in Chain)
            {
                var field = gparam.Params.FirstOrDefault(p => p.Key == groupKey)?.Fields
                    .FirstOrDefault(f => f.Key == fieldKey);

                if (field == null)
                    continue;

                var usedId = id;
                var values = field.Values.Where(v => v.ID == id).ToList();

                if (values.Count == 0 && id != 0)
                {
                    usedId = 0;
                    values = field.Values.Where(v => v.ID == 0).ToList();
                }

                if (values.Count == 0)
                    continue;

                var value = Interpolate(values, Hours, isAngle);

                if (!TryConvert(value, fallback, out T result))
                    continue;

                Sources[fieldKey] = $"{name} #{usedId}";
                return result;
            }

            return fallback;
        }

        /// <summary>
        /// The value at a time of day: between the keys either side of it, wrapping from the last key of the day
        /// round to the first. Numbers and vectors are blended; anything else takes the earlier key.
        /// </summary>
        private static object Interpolate(List<GPARAM.IFieldValue> values, float hours, bool isAngle)
        {
            if (values.Count == 1)
                return values[0].Value;

            var keys = values.OrderBy(v => v.TimeOfDay).ToList();

            hours = ((hours % 24.0f) + 24.0f) % 24.0f;

            var previous = keys.LastOrDefault(k => k.TimeOfDay <= hours) ?? keys[^1];
            var next = keys.FirstOrDefault(k => k.TimeOfDay > hours) ?? keys[0];

            var span = ((next.TimeOfDay - previous.TimeOfDay) % 24.0f + 24.0f) % 24.0f;

            if (span <= 0.0f || ReferenceEquals(previous, next))
                return previous.Value;

            var t = (((hours - previous.TimeOfDay) % 24.0f + 24.0f) % 24.0f) / span;

            return Lerp(previous.Value, next.Value, t, isAngle);
        }

        private static object Lerp(object a, object b, float t, bool isAngle)
        {
            switch (a)
            {
                case float fa when b is float fb:
                    return fa + (fb - fa) * t;
                case Vector2 va when b is Vector2 vb:
                    if (isAngle)
                    {
                        // The yaw takes the short way round
                        var yaw = vb.Y - va.Y;
                        yaw -= MathF.Round(yaw / MathF.Tau) * MathF.Tau;
                        return new Vector2(va.X + (vb.X - va.X) * t, va.Y + yaw * t);
                    }
                    return Vector2.Lerp(va, vb, t);
                case Vector3 va when b is Vector3 vb:
                    return Vector3.Lerp(va, vb, t);
                case Vector4 va when b is Vector4 vb:
                    return Vector4.Lerp(va, vb, t);
                case Color ca when b is Color cb:
                    return Vector4.Lerp(ToVector(ca), ToVector(cb), t);
                default:
                    return a;
            }
        }

        private static Vector4 ToVector(Color c) => new(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);

        private static bool TryConvert<T>(object value, T fallback, out T result)
        {
            result = fallback;

            if (value is T t)
            {
                result = t;
                return true;
            }

            if (value is Color c && typeof(T) == typeof(Vector4))
            {
                result = (T)(object)ToVector(c);
                return true;
            }

            // Switches are stored as bools, or as ints in some groups (e.g. Enable AutoExposure)
            if (typeof(T) == typeof(bool) && value is IConvertible)
            {
                try
                {
                    result = (T)(object)(Convert.ToDouble(value) != 0.0);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (value is IConvertible && fallback is IConvertible)
            {
                try
                {
                    result = (T)Convert.ChangeType(value, typeof(T));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
