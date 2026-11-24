#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Renderer.FlverResource;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The baked vertex animations an actor plays in one cut (cloth, hair, flags). Two kinds of event, on timelines under the
/// actor's:
/// <list type="bullet">
/// <item>Type 189: while it runs, a material's slot <c>paramName</c> shows the texture <c>dstTexName</c>: the
/// offsets (<c>[CS]VertexAnimationTransfer</c>) or normals (<c>[CS]VertexAnimationNormal</c>) of the animation.</item>
/// <item>Type 190: while it runs, the material is <c>playFrameValue</c> (0 to 1, keyed) of the way through it.</item>
/// </list>
/// An empty <c>materialName</c> means every vertex-animated material of the model. The textures are in the cutscene's own
/// texture binders, which <see cref="CutsceneModelPool.LoadVertexAnimationTextures" /> loads. Parameters are read from the
/// MQB every frame, so edits show at once. See Claude/Documentation/Rendering.md (Per-game material and model fixes)
/// </summary>
public sealed class CutsceneVertexAnimation
{
    public const int TextureType = 189;
    public const int PlayType = 190;

    public const string PositionSlot = "[CS]VertexAnimationTransfer";
    public const string NormalSlot = "[CS]VertexAnimationNormal";

    /// <summary>The type 189 events: the textures</summary>
    public List<MQB.Event> Textures = new();

    /// <summary>The type 190 events: how far through</summary>
    public List<MQB.Event> Plays = new();

    /// <summary>The frame last applied, null if it never was (the model or its rig hasn't loaded, or it isn't shown)</summary>
    public float? LastFrame { get; private set; }

    /// <summary>
    /// The vertex animation events of a cut, by the resource index of the model actor they belong to: the first
    /// timeline up their timeline's parents with a model (type 1) event.
    /// </summary>
    public static Dictionary<int, CutsceneVertexAnimation> Read(MQB mqb, MQB.Cut cut)
    {
        var byActor = new Dictionary<int, CutsceneVertexAnimation>();
        var resources = mqb.Resources;

        int TypeOf(MQB.Event e) => e.ResourceIndex >= 0 && e.ResourceIndex < resources.Count ? resources[e.ResourceIndex].Type : -1;

        int OwnerOf(MQB.Timeline timeline)
        {
            var t = timeline;
            for (var guard = 0; guard < 16; guard++)
            {
                if (t.ParentIndex < 0 || t.ParentIndex >= cut.Timelines.Count)
                    return -1;

                t = cut.Timelines[t.ParentIndex];
                var model = t.Events.FirstOrDefault(e => TypeOf(e) == CutsceneResourceTypes.Model);
                if (model != null)
                    return model.ResourceIndex;
            }

            return -1;
        }

        foreach (var timeline in cut.Timelines)
        {
            var owner = -2;
            foreach (var e in timeline.Events)
            {
                var type = TypeOf(e);
                if (type is not (TextureType or PlayType))
                    continue;

                if (owner == -2)
                    owner = OwnerOf(timeline);
                if (owner < 0)
                    continue;

                if (!byActor.TryGetValue(owner, out var animation))
                    byActor[owner] = animation = new CutsceneVertexAnimation();

                (type == TextureType ? animation.Textures : animation.Plays).Add(e);
            }
        }

        return byActor;
    }

    /// <summary>Every vertex animation texture any cut of the cutscene names</summary>
    public static HashSet<string> TextureNames(MQB mqb)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resources = mqb.Resources;

        foreach (var cut in mqb.Cuts)
        foreach (var timeline in cut.Timelines)
        foreach (var e in timeline.Events)
        {
            if (e.ResourceIndex < 0 || e.ResourceIndex >= resources.Count || resources[e.ResourceIndex].Type != TextureType)
                continue;

            if (IsVertexAnimationSlot(Text(e, "paramName")) && Text(e, "dstTexName") is { Length: > 0 } name)
                names.Add(name);
        }

        return names;
    }

    private static bool IsVertexAnimationSlot(string? slot) =>
        slot != null && (slot.Equals(PositionSlot, StringComparison.OrdinalIgnoreCase) || slot.Equals(NormalSlot, StringComparison.OrdinalIgnoreCase));

    private static string? Text(MQB.Event e, string name) => e.Parameters.Find(p => p.Name == name)?.Value as string;

    private static bool Covers(MQB.Event e, float frame) => frame >= e.StartFrame && frame < e.StartFrame + Math.Max(1, e.Duration);

    private static bool Names(MQB.Event e, FlverMaterial material)
    {
        var name = Text(e, "materialName");
        return string.IsNullOrEmpty(name) || string.Equals(name, material.FlverMaterialName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A 190 event's <c>playFrameValue</c> at a frame of the cut. Its keys are at frames from the event's start, held
    /// before the first and after the last, and taken as linear (the only kind seen, point type 1).
    /// </summary>
    public static float PlayValue(MQB.Event e, float frame)
    {
        var parameter = e.Parameters.Find(p => p.Name == "playFrameValue");
        if (parameter == null)
            return 0;

        var constant = ToFloat(parameter.Value);
        var points = parameter.Sequences.FirstOrDefault(s => s.Points.Count > 0)?.Points;
        if (points == null)
            return constant;

        var local = frame - e.StartFrame;
        if (local <= points[0].Unk08)
            return ToFloat(points[0].Value);

        for (var i = 1; i < points.Count; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            if (local > b.Unk08)
                continue;

            var span = b.Unk08 - a.Unk08;
            var t = span > 0 ? (local - a.Unk08) / span : 1;
            return ToFloat(a.Value) + (ToFloat(b.Value) - ToFloat(a.Value)) * t;
        }

        return ToFloat(points[^1].Value);
    }

    private static float ToFloat(object? value) => value switch
    {
        float f => f,
        null => 0,
        IConvertible c => Convert.ToSingle(c),
        _ => 0
    };

    /// <summary>
    /// Shows the animation at a frame on the model's vertex-animated materials. Call every frame while the actor is
    /// shown: materials only upload when something changed.
    /// </summary>
    public void Apply(float frame, FlverResource flver, ResourcePool pool)
    {
        if (flver.GPUMaterials == null)
            return;

        LastFrame = frame;

        foreach (var material in flver.GPUMaterials)
        {
            if (material?.Definition?.Deformer != MaterialDeformer.VatFrame)
                continue;

            var (position, normal, value, _) = Resolve(material, frame, pool);
            material.SetVertexAnimation(position, normal, value);
        }
    }

    /// <summary>What a material shows at a frame: its textures (null if not named or not in the pool) and how far through</summary>
    private (ResourceHandle<TextureResource>? Position, ResourceHandle<TextureResource>? Normal, float Value, MQB.Event? Play)
        Resolve(FlverMaterial material, float frame, ResourcePool pool)
    {
        var play = Plays.LastOrDefault(e => Covers(e, frame) && Names(e, material));
        if (play == null)
            return (null, null, 0, null);

        string? positionName = null, normalName = null;
        foreach (var e in Textures)
        {
            if (!Covers(e, frame) || !Names(e, material))
                continue;

            var slot = Text(e, "paramName");
            if (string.Equals(slot, PositionSlot, StringComparison.OrdinalIgnoreCase))
                positionName = Text(e, "dstTexName");
            else if (string.Equals(slot, NormalSlot, StringComparison.OrdinalIgnoreCase))
                normalName = Text(e, "dstTexName");
        }

        ResourceHandle<TextureResource>? position = null, normal = null;
        if (!string.IsNullOrEmpty(positionName))
            pool.TryFind(positionName, out position);
        if (!string.IsNullOrEmpty(normalName))
            pool.TryFind(normalName, out normal);

        return (position, normal, Math.Clamp(PlayValue(play, frame), 0, 1), play);
    }

    /// <summary>Puts the model's vertex-animated materials back in their bind pose</summary>
    public static void Stop(FlverResource flver)
    {
        if (flver.GPUMaterials == null)
            return;

        foreach (var material in flver.GPUMaterials)
            material?.SetVertexAnimation(null, null, 0);
    }

    /// <summary>
    /// For the log: the events, and what each of the model's materials shows at the frame last applied: its shader and
    /// deformer (only VatFrame ones play), and for those the textures and how far through.
    /// </summary>
    public string Describe(FlverResource? flver, string indent)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{indent}vertex animation: {Textures.Count} texture events, {Plays.Count} play events, ");
        sb.AppendLine(LastFrame is { } f ? $"last applied at frame {f:0.#}" : "never applied (model, rig or visibility)");

        if (flver?.GPUMaterials == null)
        {
            sb.AppendLine($"{indent}  model not loaded");
            return sb.ToString();
        }

        var frame = LastFrame ?? 0;
        foreach (var material in flver.GPUMaterials)
        {
            if (material == null)
                continue;

            sb.Append($"{indent}  material \"{material.FlverMaterialName}\" matbin={material.MaterialName} " +
                      $"shader={material.Definition?.ShaderName ?? "no definition"} deformer={material.Definition?.Deformer.ToString() ?? "-"}");

            if (material.Definition?.Deformer == MaterialDeformer.VatFrame)
            {
                var (position, normal, value, play) = Resolve(material, frame, ResourceManager.DefaultPool);

                string Texture(ResourceHandle<TextureResource>? h) => h == null ? "not in pool" : $"{h.Name} loaded={h.IsLoaded}";
                sb.Append(play == null
                    ? " no play event at this frame"
                    : $" play={value:0.###} (event {play.StartFrame}+{play.Duration}) position=[{Texture(position)}] normal=[{Texture(normal)}]");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
