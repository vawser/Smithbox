using StudioCore.Renderer;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Draws the running effect's quad particles through <see cref="FxBillboardRenderableProxy"/> batches, one per
/// appearance action:
/// - PointSprite, BillboardEx and MultiTextureBillboardEx particles (phase V3), as billboards;
/// - Tracer and LegacyTracer trails, and Line and QuadLine particles (phase V4), as segments: quads with a free
///   corner at each end (the shader's segment mode).
/// Each frame every live particle is evaluated, sorted back to front within its batch and handed over. Batches are
/// made as appearances show up and dropped with the effect.
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
        public readonly List<(float Distance, FxBillboardGpu Gpu)> Items = new();
        public Vector3 Min, Max;
        public int IdleFrames;
    }

    private readonly Dictionary<FxAppearanceDef, Batch> Batches = new();
    private FxEffectDef BatchesFor;
    private readonly List<FxParticleOutput> Scratch = new();
    private readonly List<FxSegment> Segments = new();

    /// <summary>Billboards and segments drawn last frame, for the Preview tool.</summary>
    public int DrawnBillboards { get; private set; }
    public int DrawnSegments { get; private set; }
    public int BatchCount => Batches.Count;

    /// <summary>A batch unused for this many frames goes.</summary>
    private const int IdleFramesBeforeRelease = 120;

    /// <summary>The scene distance over which a depth-blended particle fades out in front of geometry.</summary>
    private const float DepthFadeDistance = 0.3f;

    /// <summary>The shader's orientation mode for segments.</summary>
    private const uint SegmentOrientation = 100;

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
        {
            batch.Items.Clear();
            batch.Min = new Vector3(float.MaxValue);
            batch.Max = new Vector3(float.MinValue);
        }

        Scratch.Clear();
        instance.CollectParticles(Scratch);

        int billboards = 0, segments = 0;
        foreach (var p in Scratch)
        {
            var appearances = p.Node.Config?.Appearances;
            if (appearances == null)
                continue;

            foreach (var appearance in appearances)
            {
                if (appearance.Billboard is { } def)
                {
                    if (!def.Evaluate(p.Node, p.Particle, p.Position, out var b))
                        continue;
                    float distance = Vector3.Distance(eye, b.Position);
                    if (!DistanceFade(def, distance, ref b.Color.W))
                        continue;

                    var batch = BatchFor(appearance);
                    batch.Items.Add((distance, Pack(b, batch.Proxy.MaterialIndex)));
                    float r = MathF.Max(MathF.Abs(b.Size.X), MathF.Abs(b.Size.Y));
                    Grow(batch, b.Position - new Vector3(r), b.Position + new Vector3(r));
                    billboards++;
                }
                else if (appearance.Tracer != null || appearance.Line != null)
                {
                    Segments.Clear();
                    if (appearance.Tracer != null)
                        appearance.Tracer.Segments(p.Node, p.Particle, p.Position, eye, Segments);
                    else if (appearance.Line.Evaluate(p.Node, p.Particle, p.Position, eye, out var s))
                        Segments.Add(s);
                    if (Segments.Count == 0)
                        continue;

                    var batch = BatchFor(appearance);
                    foreach (var s in Segments)
                    {
                        var mid = (s.A + s.B) * 0.5f;
                        batch.Items.Add((Vector3.Distance(eye, mid), Pack(s, batch.Proxy.MaterialIndex)));
                        var half = Vector3.Max(Vector3.Abs(s.HalfA), Vector3.Abs(s.HalfB));
                        Grow(batch, Vector3.Min(s.A, s.B) - half, Vector3.Max(s.A, s.B) + half);
                    }
                    segments += Segments.Count;
                }
            }
        }

        var idle = new List<FxAppearanceDef>();
        foreach (var (appearance, batch) in Batches)
        {
            UpdateMaterial(appearance, batch);

            if (batch.Items.Count == 0)
            {
                if (++batch.IdleFrames > IdleFramesBeforeRelease)
                    idle.Add(appearance);
                batch.Proxy.SetParticles(Array.Empty<FxBillboardGpu>(), 0, new BoundingBox(Vector3.Zero, Vector3.Zero));
                continue;
            }
            batch.IdleFrames = 0;

            // Back to front within the batch
            batch.Items.Sort((a, b) => b.Distance.CompareTo(a.Distance));
            var data = new FxBillboardGpu[batch.Items.Count];
            for (int i = 0; i < data.Length; i++)
                data[i] = batch.Items[i].Gpu;

            batch.Proxy.SetParticles(data, data.Length, new BoundingBox(batch.Min, batch.Max));
        }

        foreach (var appearance in idle)
        {
            Batches[appearance].Proxy.Dispose();
            Batches.Remove(appearance);
        }

        DrawnBillboards = billboards;
        DrawnSegments = segments;
    }

    private Batch BatchFor(FxAppearanceDef appearance)
    {
        if (!Batches.TryGetValue(appearance, out var batch))
        {
            batch = new Batch
            {
                Proxy = new FxBillboardRenderableProxy(Editor.RenderScene.OpaqueRenderables),
                Min = new Vector3(float.MaxValue),
                Max = new Vector3(float.MinValue),
            };
            Batches[appearance] = batch;
        }
        return batch;
    }

    private static void Grow(Batch batch, Vector3 min, Vector3 max)
    {
        batch.Min = Vector3.Min(batch.Min, min);
        batch.Max = Vector3.Max(batch.Max, max);
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

    private void UpdateMaterial(FxAppearanceDef appearance, Batch batch)
    {
        var brightness = new Vector4(CFG.Current.FxEditor_Viewport_Billboard_Brightness, DepthFadeDistance, 0, 0);
        FxBillboardMaterial material;

        if (appearance.Billboard is { } def)
        {
            bool multi = def.Kind == FxBillboardKind.MultiTexture;
            uint flags = 0;
            if (def.InterpolateFrames) flags |= FxBillboardMaterial.FlagInterpolate;
            if (def.DepthBlend) flags |= FxBillboardMaterial.FlagDepthBlend;
            if (def.PremultiplyAlpha) flags |= FxBillboardMaterial.FlagPremultiply;
            if (multi) flags |= FxBillboardMaterial.FlagMultiTexture;
            if (!def.ScaleBeforeRotation) flags |= FxBillboardMaterial.FlagRotateBeforeScale;

            // The texture type is the schema's: albedo ("a") for the colour textures
            material = new FxBillboardMaterial
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
                Tuning = brightness,
            };
        }
        else if (appearance.Tracer is { } tracer)
        {
            uint flags = FxBillboardMaterial.FlagInterpolate | FxBillboardMaterial.FlagDepthBlend;
            if (tracer.PremultiplyAlpha) flags |= FxBillboardMaterial.FlagPremultiply;
            material = new FxBillboardMaterial
            {
                Texture1 = Textures.Get(tracer.Texture, "a"),
                LayerCount = 1,
                Columns = (uint)tracer.Columns,
                TotalFrames = (uint)tracer.TotalFrames,
                Flags = flags,
                Orientation = SegmentOrientation,
                BlendMode = (uint)Math.Clamp(tracer.BlendMode, 0, 7),
                Tuning = brightness,
            };
        }
        else
        {
            // Lines have no texture: solid
            var line = appearance.Line;
            material = new FxBillboardMaterial
            {
                LayerCount = 1,
                Columns = 1,
                TotalFrames = 1,
                Flags = FxBillboardMaterial.FlagSolid | FxBillboardMaterial.FlagDepthBlend,
                Orientation = SegmentOrientation,
                BlendMode = (uint)Math.Clamp(line.BlendMode, 0, 7),
                Tuning = brightness,
            };
        }

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

    /// <summary>
    /// A segment in the shader's segment mode: A0 end A, A2 its colour, A4 the uv (u0, v0, u1 - u0, 1), A5 the
    /// half-width at A; B0 the half-width at B, B1 B's colour, B4 end B.
    /// </summary>
    private static FxBillboardGpu Pack(in FxSegment s, uint material)
    {
        uint thresholds = PackUnorm2x16(s.AlphaThreshold, s.AlphaFadeThreshold);
        return new FxBillboardGpu
        {
            A0 = new Vector4(s.A, 0),
            A1 = new Vector4(1, 1, s.Frame, BitConverter.UInt32BitsToSingle(material)),
            A2 = s.ColorA,
            A3 = new Vector4(0, 0, BitConverter.UInt32BitsToSingle(thresholds), 0),
            A4 = new Vector4(s.U0, s.V0, s.U1 - s.U0, 1),
            A5 = new Vector4(s.HalfA, 0),
            B0 = new Vector4(s.HalfB, 0),
            B1 = s.ColorB,
            B4 = new Vector4(s.B, 0),
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
        DrawnSegments = 0;
    }
}
