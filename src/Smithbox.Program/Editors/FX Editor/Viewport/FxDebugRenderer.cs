using StudioCore.Renderer;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Shows <see cref="FxDebugShapes"/> in the FX Editor's scene: one line batch per layer (split to fit the debug
/// primitives' 16 bit indices), none of them pickable. A new set replaces the old one without a gap, as the Map
/// Editor's navigation overlay does: its geometry uploads a frame or more after it is made, so it stays hidden until
/// built, is drawn beside the old set for <see cref="OverlapFrames"/> frames, and only then does the old one go.
/// </summary>
public sealed class FxDebugRenderer
{
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Pending;
    private int PendingShownFrames;

    private const int OverlapFrames = 2;

    public static Vector3 LayerColor(FxDebugLayer layer) => layer switch
    {
        FxDebugLayer.Shapes => new(0.30f, 0.80f, 1.00f),
        FxDebugLayer.Selected => new(1.00f, 0.90f, 0.20f),
        FxDebugLayer.AxisX => new(0.95f, 0.25f, 0.25f),
        FxDebugLayer.AxisY => new(0.30f, 0.90f, 0.30f),
        FxDebugLayer.AxisZ => new(0.30f, 0.45f, 1.00f),
        FxDebugLayer.Directions => new(1.00f, 0.55f, 0.15f),
        FxDebugLayer.Proxies => new(1.00f, 0.35f, 1.00f),
        _ => new(1, 1, 1),
    };

    public void Show(RenderScene scene, FxDebugShapes.Result shapes)
    {
        // A set still on its way is dropped for the newer one
        if (Pending != null)
            Release(Pending);

        var built = new List<(DebugPrimitiveRenderableProxy, Action)>();

        foreach (var (layer, lines) in shapes.Lines)
        {
            if (lines.Count == 0)
                continue;

            var c = LayerColor(layer);
            var color = Color.FromArgb(255, (int)(c.X * 255), (int)(c.Y * 255), (int)(c.Z * 255));

            foreach (var chunk in lines.Chunk(DbgPrimLineBatch.MaxLines))
            {
                var primitive = new DbgPrimLineBatch(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    BaseColor = color,
                    HighlightedColor = color,
                    // The selected node's shapes show through anything in front of them
                    RenderOverlay = layer == FxDebugLayer.Selected,
                    Visible = false,
                };
                built.Add((proxy, primitive.Release));
            }
        }

        Pending = built;
        PendingShownFrames = 0;
    }

    /// <summary>Called every frame: lets a built set take over from the drawn one.</summary>
    public void Update()
    {
        if (Pending == null)
            return;

        if (PendingShownFrames >= OverlapFrames || Pending.Count == 0)
        {
            Release(Shown);
            Shown = Pending;
            Pending = null;
            PendingShownFrames = 0;
        }
        else if (PendingShownFrames > 0 || Pending.All(p => p.Proxy.IsRenderableConstructed))
        {
            foreach (var (proxy, _) in Pending)
            {
                if (!proxy.Visible)
                    proxy.Visible = true;
            }
            PendingShownFrames++;
        }
    }

    public void Clear()
    {
        Release(Shown);
        if (Pending != null)
            Release(Pending);
        Pending = null;
        PendingShownFrames = 0;
    }

    private static void Release(List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> parts)
    {
        foreach (var (proxy, release) in parts)
        {
            proxy.Dispose();
            release();
        }
        parts.Clear();
    }
}
