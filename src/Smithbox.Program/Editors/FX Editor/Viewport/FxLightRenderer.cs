using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The running effect's PointLight and SpotLight particles as scene lights (FX Editor Plan, phase V4). The scene
/// draws at most <see cref="SceneLightList.MaxLights"/> lights, the camera light included, so the brightest ones are
/// kept. They light the models in the viewport (model particles, and in time the preview character or map).
/// </summary>
public class FxLightRenderer
{
    private readonly FxEditorScreen Editor;
    private readonly List<SceneLight> Lights = new();
    private readonly List<FxParticleOutput> Scratch = new();
    private readonly List<FxLightInstance> Found = new();

    /// <summary>Lights the effect gave last frame (drawn or not), and how many were drawn.</summary>
    public int LightCount { get; private set; }
    public int DrawnLights { get; private set; }

    /// <summary>The drawn lights, for the debug markers.</summary>
    public IReadOnlyList<FxLightInstance> Current => Found;

    public FxLightRenderer(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public void Update(FxEffectInstance instance)
    {
        var scene = Editor.RenderScene;
        if (scene == null)
            return;

        Found.Clear();
        if (instance != null && CFG.Current.FxEditor_Viewport_Display_Lights)
        {
            Scratch.Clear();
            instance.CollectParticles(Scratch);
            foreach (var p in Scratch)
            {
                var appearances = p.Node.Config?.Appearances;
                if (appearances == null)
                    continue;
                foreach (var appearance in appearances)
                    if (appearance.Light is { } def && def.Evaluate(p.Node, p.Particle, p.Position, out var light))
                        Found.Add(light);
            }
        }

        LightCount = Found.Count;

        // The brightest, leaving room for the camera light
        int room = SceneLightList.MaxLights - 1;
        if (Found.Count > room)
        {
            Found.Sort((a, b) => (b.Intensity * b.Range).CompareTo(a.Intensity * a.Range));
            Found.RemoveRange(room, Found.Count - room);
        }

        while (Lights.Count < Found.Count)
        {
            var light = new SceneLight();
            Lights.Add(light);
            scene.Lights.Add(light);
        }

        for (int i = 0; i < Lights.Count; i++)
        {
            var light = Lights[i];
            if (i >= Found.Count)
            {
                light.Enabled = false;
                continue;
            }

            var f = Found[i];
            light.Enabled = true;
            light.Type = f.Spot ? SceneLightType.Spot : SceneLightType.Point;
            light.Position = f.Position;
            light.Direction = f.Spot ? f.Direction : -Vector3.UnitY;
            light.Color = f.Color;
            light.Intensity = f.Intensity * CFG.Current.FxEditor_Viewport_Light_Brightness;
            light.Range = f.Range;
            light.FadeAcrossRange = true;
            if (f.Spot)
            {
                light.OuterConeAngleDegrees = f.ConeDegrees;
                light.InnerConeAngleDegrees = Math.Max(f.ConeDegrees * 0.8f, f.ConeDegrees - 10);
            }
        }

        DrawnLights = Found.Count;
    }

    public void Clear()
    {
        var scene = Editor.RenderScene;
        foreach (var light in Lights)
            scene?.Lights.Remove(light);
        Lights.Clear();
        Found.Clear();
        LightCount = DrawnLights = 0;
    }
}
