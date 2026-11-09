using StudioCore.Renderer;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Draws the running effect's PointSprite, BillboardEx and MultiTextureBillboardEx particles (FX Editor Plan,
/// phase V3). Each frame every live particle of a billboard appearance is evaluated (<see cref="FxBillboardDef"/>),
/// faded by its view distance, sorted back to front and handed to that appearance's batch
/// (<see cref="FxBillboardRenderableProxy"/>). Batches are made as appearances show up and dropped with the effect.
/// </summary>
public class FxBillboardRenderer
{
    private readonly FxEditorScreen Editor;
    private readonly FxTextureLibrary Textures;

    private sealed class Batch
    {
        public FxBillboardRenderableProxy Proxy;
        public FxBillboardMaterial Material;
        public bool HasMaterial;
        public readonly List<(float Distance, FxBillboardInstance Instance)> Particles = new();
        public int IdleFrames;
    }

    private readonly Dictionary<FxAppearanceDef, Batch> Batches = new();
    private FxEffectDef BatchesFor;
    private readonly List<FxParticleOutput> Scratch = new();

    /// <summary>Billboards drawn last frame, for the Preview tool.</summary>
    public int DrawnBillboards { get; private set; }
    public int BatchCount => Batches.Count;

    /// <summary>A batch unused for this many frames goes.</summary>
    private const int IdleFramesBeforeRelease = 120;

    /// <summary>The scene distance over which a depth-blended particle fades out in front of geometry.</summary>
    private const float DepthFadeDistance = 0.3f;

    public FxBillboardRenderer(FxEditorScreen editor, FxTextureLibrary textures)
    {
        Editor = editor;
        Textures = textures;
    }

    public void Update(FxEffectInstance instance, Vector3 eye)
    {
        Textures.Update();

        if (Editor.RenderScene == null)
            return;

        if (instance == null || !CFG.Current.FxEditor_Viewport_Display_Billboards)
        {
            Clear();
            return;
        }

        // A rebuilt effect has new appearance objects: the old batches can't be reused
        if (instance.Def != BatchesFor)
        {
            Clear();
            BatchesFor = instance.Def;
        }

        foreach (var batch in Batches.Values)
            batch.Particles.Clear();

        Scratch.Clear();
        instance.CollectParticles(Scratch);

        foreach (var p in Scratch)
        {
            var appearances = p.Node.Config?.Appearances;
            if (appearances == null)
                continue;

            foreach (var appearance in appearances)
            {
                var def = appearance.Billboard;
                if (def == null || !def.Evaluate(p.Node, p.Particle, p.Position, out var b))
                    continue;

                float distance = Vector3.Distance(eye, b.Position);
                if (!DistanceFade(def, distance, ref b.Color.W))
                    continue;

                if (!Batches.TryGetValue(appearance, out var batch))
                {
                    batch = new Batch { Proxy = new FxBillboardRenderableProxy(Editor.RenderScene.OpaqueRenderables) };
                    Batches[appearance] = batch;
                }
                batch.Particles.Add((distance, b));
            }
        }

        int drawn = 0;
        var idle = new List<FxAppearanceDef>();
        foreach (var (appearance, batch) in Batches)
        {
            UpdateMaterial(appearance.Billboard, batch);

            if (batch.Particles.Count == 0)
            {
                if (++batch.IdleFrames > IdleFramesBeforeRelease)
                    idle.Add(appearance);
                batch.Proxy.SetParticles(Array.Empty<FxBillboardGpu>(), 0, new BoundingBox(Vector3.Zero, Vector3.Zero));
                continue;
            }
            batch.IdleFrames = 0;

            // Back to front within the batch
            batch.Particles.Sort((a, b) => b.Distance.CompareTo(a.Distance));

            var data = new FxBillboardGpu[batch.Particles.Count];
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            uint material = batch.Proxy.MaterialIndex;
            for (int i = 0; i < data.Length; i++)
            {
                var b = batch.Particles[i].Instance;
                data[i] = Pack(b, material);
                float r = MathF.Max(MathF.Abs(b.Size.X), MathF.Abs(b.Size.Y));
                min = Vector3.Min(min, b.Position - new Vector3(r));
                max = Vector3.Max(max, b.Position + new Vector3(r));
            }

            batch.Proxy.SetParticles(data, data.Length, new BoundingBox(min, max));
            drawn += data.Length;
        }

        foreach (var appearance in idle)
        {
            Batches[appearance].Proxy.Dispose();
            Batches.Remove(appearance);
        }

        DrawnBillboards = drawn;
    }

    /// <summary>
    /// The min/max view distances: the opacity falls to 0 between the fade distance and the limit, and the hard
    /// thresholds hide the particle. -1 turns each off.
    /// </summary>
    private static bool DistanceFade(FxBillboardDef def, float distance, ref float alpha)
    {
        if (def.MinDistanceThreshold > 0 && distance < def.MinDistanceThreshold)
            return false;
        if (def.MaxDistanceThreshold > 0 && distance > def.MaxDistanceThreshold)
            return false;

        if (def.MinDistance >= 0 && def.MinFadeDistance > def.MinDistance)
            alpha *= Math.Clamp((distance - def.MinDistance) / (def.MinFadeDistance - def.MinDistance), 0, 1);
        if (def.MaxDistance > 0 && def.MaxFadeDistance >= 0 && def.MaxFadeDistance < def.MaxDistance)
            alpha *= Math.Clamp((def.MaxDistance - distance) / (def.MaxDistance - def.MaxFadeDistance), 0, 1);

        return alpha > 0;
    }

    private void UpdateMaterial(FxBillboardDef def, Batch batch)
    {
        bool multi = def.Kind == FxBillboardKind.MultiTexture;
        uint flags = 0;
        if (def.InterpolateFrames) flags |= FxBillboardMaterial.FlagInterpolate;
        if (def.DepthBlend) flags |= FxBillboardMaterial.FlagDepthBlend;
        if (def.PremultiplyAlpha) flags |= FxBillboardMaterial.FlagPremultiply;
        if (multi) flags |= FxBillboardMaterial.FlagMultiTexture;
        if (!def.ScaleBeforeRotation) flags |= FxBillboardMaterial.FlagRotateBeforeScale;

        // The texture type is the schema's: albedo ("a") for the colour textures
        var material = new FxBillboardMaterial
        {
            Texture1 = Textures.Get(def.Texture, "a"),
            Texture2 = multi ? Textures.Get(def.Layer2, "a") : 0,
            Texture3 = multi ? Textures.Get(def.Layer3, "a") : 0,
            LayerCount = multi ? 3u : 1u,
            Columns = (uint)Math.Max(def.Columns, 1),
            TotalFrames = (uint)Math.Max(def.TotalFrames, 1),
            Flags = flags,
            Orientation = (uint)Math.Max(def.Kind == FxBillboardKind.PointSprite ? 1 : def.Orientation, 0),
            BlendMode = (uint)Math.Clamp(def.BlendMode, 0, 7),
            Tuning = new Vector4(CFG.Current.FxEditor_Viewport_Billboard_Brightness, DepthFadeDistance, 0, 0),
        };

        if (batch.HasMaterial && material.Equals(batch.Material))
            return;

        batch.Material = material;
        batch.HasMaterial = true;
        batch.Proxy.SetMaterial(material);
    }

    private static FxBillboardGpu Pack(in FxBillboardInstance b, uint material)
    {
        const float rad = MathF.PI / 180f;
        uint thresholds = PackUnorm2x16(b.AlphaThreshold, b.AlphaFadeThreshold);

        return new FxBillboardGpu
        {
            A0 = new Vector4(b.Position, b.Rotation.Z * rad),
            A1 = new Vector4(b.Size.X, b.Size.Y, b.Frame, BitConverter.UInt32BitsToSingle(material)),
            A2 = b.Color,
            A3 = new Vector4(b.Rotation.X * rad, b.Rotation.Y * rad, BitConverter.UInt32BitsToSingle(thresholds), b.DepthOffset),
            A4 = b.Layer1Uv,
            A5 = b.Layer2Uv,
            B0 = b.Layer3Uv,
            B1 = b.Layer1Color,
            B2 = b.Layer2Color,
            B3 = b.Layer3Color,
            B4 = new Vector4(b.NodeRight, 0),
            B5 = new Vector4(b.NodeUp, 0),
        };
    }

    /// <summary>As GLSL's packUnorm2x16: x in the low 16 bits.</summary>
    private static uint PackUnorm2x16(float x, float y)
    {
        uint a = (uint)MathF.Round(Math.Clamp(x, 0, 1) * 65535f);
        uint b = (uint)MathF.Round(Math.Clamp(y, 0, 1) * 65535f);
        return a | b << 16;
    }

    public void Clear()
    {
        foreach (var batch in Batches.Values)
            batch.Proxy.Dispose();
        Batches.Clear();
        BatchesFor = null;
        DrawnBillboards = 0;
    }
}
