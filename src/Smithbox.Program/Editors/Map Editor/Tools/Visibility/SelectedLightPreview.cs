using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lights the viewport with the selected BTL lights. These are rebuilt every frame, so they follow
/// gizmo drags and property edits, and go away with the selection.
/// </summary>
public class SelectedLightPreview
{
    /// <summary>
    /// BTL has no soft edge to a spot light's cone, so this is the part of it that is at full strength.
    /// </summary>
    private const float InnerConeRatio = 0.75f;

    private const float ColorGamma = 2.2f;

    private readonly MapEditorView View;
    private readonly List<SceneLight> ActiveLights = new();

    public SelectedLightPreview(MapEditorView view)
    {
        View = view;
    }

    public void Update()
    {
        var scene = View.ViewportHandler.RenderScene;

        foreach (var light in ActiveLights)
        {
            scene.Lights.Remove(light);
        }

        ActiveLights.Clear();

        // The Light render filter hides the markers, but a light selected through the map list still shines
        foreach (var selection in View.ViewportSelection.GetSelection())
        {
            if (selection is MsbEntity { Type: MsbEntityType.Light, EditorVisible: true } entity &&
                entity.WrappedObject is BTL.Light btlLight)
            {
                var light = CreateLight(btlLight, entity.GetWorldMatrix());

                scene.Lights.Add(light);
                ActiveLights.Add(light);
            }
        }
    }

    private static SceneLight CreateLight(BTL.Light btlLight, Matrix4x4 world)
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
            Intensity = btlLight.DiffusePower * CFG.Current.Viewport_Light_Intensity_Scale,
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
