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
/// viewport's light radius, and within each light's own EnableDist. Selected lights shine wherever they are. The
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
        var intensityScale = GetIntensityScale();

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
        }

        found.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        InRangeCount = found.Count;

        var count = Math.Min(found.Count, Math.Clamp(CFG.Current.Viewport_BTL_Light_Max_Count, 0, SceneLightTable.Capacity));

        for (var i = 0; i < count; i++)
            Entries[i] = found[i].Data;

        ShadedCount = count;

        table.Upload(Entries, count);
    }

    private static bool IsNearCamera(BTL.Light light, float distance, float radius)
    {
        if (!CFG.Current.Viewport_BTL_Lights_Near_Camera || distance > radius)
            return false;

        // The game turns a light on within its EnableDist of the camera; 0 is taken as no limit
        if (CFG.Current.Viewport_BTL_Light_Use_Enable_Distance && light.EnableDist > 0.0f && distance > light.EnableDist)
            return false;

        return true;
    }

    /// <summary>
    /// BTL power is in the game's units, like the GPARAM's lights, so with map lighting it takes the same exposure;
    /// otherwise the viewport's light intensity scale
    /// </summary>
    private static float GetIntensityScale()
    {
        var mapLighting = SceneRenderer.MapLighting;

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

    private static SceneLight CreateLight(BTL.Light btlLight, Matrix4x4 world, float intensityScale)
    {
        var outerAngle = btlLight.ConeAngle / 2.0f;

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
            OuterConeAngleDegrees = outerAngle
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
