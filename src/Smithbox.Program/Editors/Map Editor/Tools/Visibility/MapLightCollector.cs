using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lights the viewports with the loaded maps' BTL lights near the camera, as the game turns them on: within the
/// viewport's light radius, and within each light's own ViewDistance. Selected lights shine wherever they are. The
/// nearest are kept, up to the viewport's limit, in <see cref="SceneRenderer.MapLights" />. Rebuilt every frame,
/// so they follow the camera, gizmo drags and property edits.
/// </summary>
public class MapLightCollector
{
    /// <summary>
    /// BTL has no soft edge to a spot light's cone, so this is the part of it that is at full strength.
    /// </summary>
    private const float InnerConeRatio = 0.75f;

    private const float ColorGamma = 2.2f;

    private readonly MapEditorView View;

    // The light entities of each loaded map, found again when the map's objects change in number
    private readonly Dictionary<MapContainer, (int ObjectCount, List<MsbEntity> Lights)> MapLights = new();

    private readonly MapLightData[] Entries = new MapLightData[SceneLightTable.Capacity];

    /// <summary>How many lights were in range last frame, before the limit</summary>
    public int InRangeCount { get; private set; }

    /// <summary>How many lights light the viewports</summary>
    public int ShadedCount { get; private set; }

    public MapLightCollector(MapEditorView view)
    {
        View = view;
    }

    public void Update()
    {
        var table = SceneRenderer.MapLights;

        if (table == null)
            return;

        var eye = View.GetCurrentViewport()?.ViewPipeline.Eye;

        var maps = View.Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .ToHashSet();

        foreach (var stale in MapLights.Keys.Where(m => !maps.Contains(m)).ToList())
        {
            MapLights.Remove(stale);
        }

        var selected = View.ViewportSelection.GetSelection().OfType<MsbEntity>().ToHashSet();
        var found = new List<(float Distance, MapLightData Data)>();

        var radius = Math.Max(CFG.Current.Viewport_BTL_Light_Radius, 0.0f);
        var intensityScale = GetIntensityScale(SceneRenderer.MapLighting);

        foreach (var map in maps)
        {
            foreach (var entity in GetLights(map))
            {
                if (!entity.EditorVisible || entity.WrappedObject is not BTL.Light btlLight)
                    continue;

                var world = entity.GetWorldMatrix();
                var isSelected = selected.Contains(entity);
                var distance = eye.HasValue ? Vector3.Distance(world.Translation, eye.Value) : float.MaxValue;

                if (!isSelected && !IsNearCamera(btlLight, distance, radius))
                    continue;

                // Selected lights come first, then the nearest
                found.Add((isSelected ? -1.0f : distance, CreateLight(btlLight, world, intensityScale).ToMapGpu()));
            }

            AddDrawParamPointLights(map, eye, radius, intensityScale, selected, found);
        }

        found.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        InRangeCount = found.Count;

        var count = Math.Min(found.Count, Math.Clamp(CFG.Current.Viewport_BTL_Light_Max_Count, 0, SceneLightTable.Capacity));

        for (var i = 0; i < count; i++)
            Entries[i] = found[i].Data;

        ShadedCount = count;

        table.Upload(Entries, count);
    }

    /// <summary>
    /// Demon's Souls' point lights, its MSB's Light events: the light is at the map's region whose index is the
    /// event's PointLightID (595 of the 598 whose name matches a region's are at that index, and every ID is in range),
    /// and shines as the row of the area's PointLightBank its PointLightBankID names (the rows' names match the events': candles,
    /// torches), -1 for none. The row's dwindleBegin is how far it is at full strength, dwindleEnd where it is gone,
    /// colR/G/B its colour and colA its intensity in percent. Lit near the camera as BTL lights are; a selected event
    /// or region shines wherever it is. See the DES DrawParam lighting plan, Phase 5b.
    /// </summary>
    private void AddDrawParamPointLights(MapContainer map, Vector3? eye, float radius, float intensityScale,
        HashSet<MsbEntity> selected, List<(float Distance, MapLightData Data)> found)
    {
        var bank = View.DrawParamLightingBank;

        if (!CFG.Current.Viewport_Map_DrawParam_Point_Lights || bank == null || !bank.CanUse())
            return;

        var pointLights = bank.GetBank(map.Name, DrawParamLightingBank.PointLightBank);

        if (pointLights == null)
            return;

        // The parts by name, as the events hold their part: the MSB stores the part's index, read back as its name
        Dictionary<string, Entity> partsByName = null;

        foreach (var entity in map.Events)
        {
            if (entity is not MsbEntity eventEntity || eventEntity.WrappedObject is not MSBD.Event.Light light)
                continue;

            if (light.PointLightID < 0 || light.PointLightID >= map.Regions.Count || light.PointLightBankID < 0)
                continue;

            if (map.Regions[light.PointLightID] is not MsbEntity region || !region.EditorVisible)
                continue;

            // A light goes with its candlestick or torch: hidden while its part is hidden in the editor
            if (!string.IsNullOrEmpty(light.PartName))
            {
                partsByName ??= map.Parts
                    .Where(p => p.WrappedObject is MSBD.Part)
                    .GroupBy(p => ((MSBD.Part)p.WrappedObject).Name)
                    .ToDictionary(g => g.Key, g => g.First());

                if (partsByName.TryGetValue(light.PartName, out var part) && !part.EditorVisible)
                    continue;
            }

            var row = pointLights[light.PointLightBankID];

            if (row == null)
                continue;

            var position = region.GetWorldMatrix().Translation;
            var isSelected = selected.Contains(eventEntity) || selected.Contains(region);
            var distance = eye.HasValue ? Vector3.Distance(position, eye.Value) : float.MaxValue;

            if (!isSelected && (!CFG.Current.Viewport_BTL_Lights_Near_Camera || distance > radius))
                continue;

            var color = new Vector3(
                MathF.Pow(DrawParamLightingBank.Get(row, "colR", 255.0f) / 255.0f, ColorGamma),
                MathF.Pow(DrawParamLightingBank.Get(row, "colG", 255.0f) / 255.0f, ColorGamma),
                MathF.Pow(DrawParamLightingBank.Get(row, "colB", 255.0f) / 255.0f, ColorGamma));

            var sceneLight = new SceneLight
            {
                Type = SceneLightType.Point,
                Position = position,
                Color = color,
                Intensity = DrawParamLightingBank.Get(row, "colA", 100.0f) / 100.0f * intensityScale,
                Range = DrawParamLightingBank.Get(row, "dwindleEnd", 2.0f),
                FullStrengthRange = DrawParamLightingBank.Get(row, "dwindleBegin", 0.5f),
                FadeAcrossRange = true,
                // A straight fade between the two, the user's choice: the curve isn't in the data
                LinearFade = true
            };

            found.Add((isSelected ? -1.0f : distance, sceneLight.ToMapGpu()));
        }
    }

    public static bool IsNearCamera(BTL.Light light, float distance, float radius)
    {
        if (!CFG.Current.Viewport_BTL_Lights_Near_Camera || distance > radius)
            return false;

        // The game turns a light on within its ViewDistance of the camera; 0 is taken as no limit
        if (CFG.Current.Viewport_BTL_Light_Use_Enable_Distance && light.ViewDistance > 0.0f && distance > light.ViewDistance)
            return false;

        return true;
    }

    /// <summary>
    /// BTL power is in the game's units, like the GPARAM's lights, so with map lighting it takes the same exposure;
    /// otherwise the viewport's light intensity scale. <paramref name="mapLighting" /> is the lighting the lights are
    /// drawn with.
    /// </summary>
    public static float GetIntensityScale(GparamLighting mapLighting)
    {

        return mapLighting != null
            ? mapLighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale
            : CFG.Current.Viewport_Light_Intensity_Scale;
    }

    private List<MsbEntity> GetLights(MapContainer map)
    {
        var objectCount = map.Objects.Count;

        if (MapLights.TryGetValue(map, out var cached) && cached.ObjectCount == objectCount)
            return cached.Lights;

        var lights = map.Objects
            .OfType<MsbEntity>()
            .Where(e => e.Type == MsbEntityType.Light)
            .ToList();

        MapLights[map] = (objectCount, lights);

        return lights;
    }

    public static SceneLight CreateLight(BTL.Light btlLight, Matrix4x4 world, float intensityScale)
    {
        // A spot light's FoVs are the cone's full widths, each the same as the other when 0. The vertical one is
        // measured along the light's local Y, the horizontal one along its local X (to check in app: SDT m10's
        // 059_本家_窓光_000 is V 45 / H 90, so its patch should be twice as wide as it is tall).
        var verticalFov = btlLight.VerticalFov != 0.0f ? btlLight.VerticalFov : btlLight.HorizontalFov;
        var horizontalFov = btlLight.HorizontalFov != 0.0f ? btlLight.HorizontalFov : verticalFov;
        var outerAngle = verticalFov / 2.0f;

        return new SceneLight
        {
            Type = btlLight.Type switch
            {
                BTL.LightType.Directional => SceneLightType.Directional,
                BTL.LightType.Spot => SceneLightType.Spot,
                _ => SceneLightType.Point
            },
            Position = world.Translation,
            // Lights shine along their local Z axis
            Direction = Vector3.TransformNormal(Vector3.UnitZ, world),
            Color = ToLinearColor(btlLight.DiffuseColor),
            Intensity = btlLight.DiffusePower * intensityScale,
            // BTL gives the highlights their own color and power, on the same scale as the diffuse light
            SpecularColor = ToLinearColor(btlLight.SpecularColor),
            SpecularIntensity = btlLight.SpecularPower * intensityScale,
            Range = btlLight.Radius,
            FadeAcrossRange = true,
            InnerConeAngleDegrees = outerAngle * InnerConeRatio,
            OuterConeAngleDegrees = outerAngle,
            HorizontalAxis = Vector3.TransformNormal(Vector3.UnitX, world),
            HorizontalOuterConeAngleDegrees = horizontalFov / 2.0f
        };
    }

    private static Vector3 ToLinearColor(System.Drawing.Color color)
    {
        return new Vector3(
            MathF.Pow(color.R / 255.0f, ColorGamma),
            MathF.Pow(color.G / 255.0f, ColorGamma),
            MathF.Pow(color.B / 255.0f, ColorGamma));
    }
}
