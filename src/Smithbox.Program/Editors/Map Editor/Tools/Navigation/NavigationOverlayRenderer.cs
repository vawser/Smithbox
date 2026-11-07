using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Shows a <see cref="NavOverlay"/> in a viewport: a translucent mesh per face data value and a line batch per layer,
/// split to fit the 16 bit indices of the debug primitives. None of it can be picked or selected.
/// A new overlay replaces the old one without gaps, as the Collision Editor's preview does: its geometry is uploaded a
/// frame or more after it is made, and a built renderable isn't necessarily drawn yet. So it is hidden until every part
/// is built, then drawn alongside the old one for <see cref="OverlapFrames" /> frames, and only then does the old one
/// go. <see cref="Update" /> must be called each frame for this.
/// </summary>
public sealed class NavigationOverlayRenderer
{
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();

    // The overlay on its way to replacing Shown, and how many frames it has been drawn alongside it (0 while its
    // geometry is still uploading)
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Pending;
    private int PendingShownFrames;

    /// <summary>Frames a new overlay is drawn alongside the old one before the old one goes.</summary>
    public const int OverlapFrames = 2;

    /// <summary>The faces' colour for each face data value, in the order of the legend.</summary>
    public IReadOnlyList<(uint Value, Vector3 Color, int Faces)> FaceLegend { get; private set; } = Array.Empty<(uint, Vector3, int)>();

    public bool IsShown => Shown.Count > 0 || Pending?.Count > 0;

    /// <summary>True while a new overlay is on its way to replacing the drawn one.</summary>
    public bool IsUpdating => Pending != null;

    /// <summary>
    /// True once an overlay shown since the last <see cref="Clear" /> has been drawn and taken over; it stays true while
    /// later ones upload, as the drawn one is kept until then.
    /// </summary>
    public bool HasDrawn { get; private set; }

    /// <summary>
    /// Shows the pending overlay once it has been built, and lets it replace the drawn one after it has been drawn
    /// alongside it for <see cref="OverlapFrames" /> frames.
    /// </summary>
    public void Update()
    {
        if (Pending == null)
            return;

        if (PendingShownFrames >= OverlapFrames)
        {
            // Drawn alongside the old overlay for long enough: it takes over
            Release(Shown);
            Shown = Pending;
            Pending = null;
            PendingShownFrames = 0;
            HasDrawn = true;
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

    /// <summary>Distinct, fairly light colours for the face data values; the first is for 0 (no flags).</summary>
    private static readonly Vector3[] FacePalette =
    {
        new(0.35f, 0.75f, 0.45f), new(0.95f, 0.55f, 0.20f), new(0.30f, 0.55f, 0.95f), new(0.90f, 0.30f, 0.60f),
        new(0.95f, 0.85f, 0.25f), new(0.55f, 0.35f, 0.90f), new(0.25f, 0.85f, 0.85f), new(0.95f, 0.35f, 0.30f),
        new(0.60f, 0.85f, 0.25f), new(0.85f, 0.60f, 0.95f), new(0.55f, 0.55f, 0.55f), new(0.95f, 0.70f, 0.60f),
    };

    /// <summary>The colour of each line layer.</summary>
    /// <summary>The opacity of a gate marker's fill (0 to 100).</summary>
    public const float GateFillOpacity = 35;

    public static Vector3 LayerColor(NavOverlayLayer layer) => layer switch
    {
        NavOverlayLayer.InnerEdges => new(0.20f, 0.20f, 0.20f),
        NavOverlayLayer.OpenEdges => new(1.00f, 0.25f, 0.25f),
        NavOverlayLayer.ConnectedInMap => new(0.25f, 0.90f, 1.00f),
        NavOverlayLayer.ConnectedCrossMap => new(1.00f, 0.30f, 1.00f),
        NavOverlayLayer.UserEdges => new(1.00f, 0.90f, 0.20f),
        NavOverlayLayer.BlockedUserEdges => new(1.00f, 0.55f, 0.10f),
        NavOverlayLayer.GateNodes => new(1.00f, 1.00f, 1.00f),
        NavOverlayLayer.GateNodesUnpaired => new(0.55f, 1.00f, 0.55f),
        NavOverlayLayer.GateNodesBroken => new(1.00f, 0.00f, 0.45f),
        NavOverlayLayer.GateCosts => new(1.00f, 0.80f, 0.60f),
        NavOverlayLayer.GateSelected => new(1.00f, 1.00f, 1.00f),
        NavOverlayLayer.OnavAreas => new(0.60f, 0.60f, 1.00f),
        NavOverlayLayer.OnavLinks => new(0.40f, 0.40f, 1.00f),
        NavOverlayLayer.OnavCells => new(0.45f, 0.45f, 0.75f),
        NavOverlayLayer.Highlight => new(1.00f, 1.00f, 1.00f),
        NavOverlayLayer.Blockers => new(1.00f, 0.15f, 0.15f),
        _ => new(1, 1, 1),
    };

    private static Color ToColor(Vector3 c, int alpha = 255)
        => Color.FromArgb(alpha, (int)(c.X * 255), (int)(c.Y * 255), (int)(c.Z * 255));

    /// <summary>
    /// Replaces what is shown with the overlay. Face fills use the given opacity (0 to 100). With a legend (another
    /// renderer's), face data values it lists take its colours, so a part drawn on its own matches the rest.
    /// The drawn overlay stays until the new one has been drawn (see <see cref="Update" />). A new overlay shown while
    /// another is still pending replaces the pending one.
    /// </summary>
    public void Show(RenderScene scene, NavOverlay overlay, float faceOpacity, IReadOnlyList<(uint Value, Vector3 Color, int Faces)> legendColors = null)
    {
        if (scene == null || overlay == null)
        {
            Clear();
            return;
        }

        if (Pending != null)
            Release(Pending);

        // Hidden until its geometry has been uploaded
        var built = new List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>();
        void Add(DebugPrimitiveRenderableProxy proxy, Action release)
        {
            proxy.Visible = false;
            built.Add((proxy, release));
        }

        var faceBatches = overlay.Batches.Values.Where(b => b.Layer == NavOverlayLayer.Faces).OrderBy(b => b.Key).ToList();
        var legend = new List<(uint, Vector3, int)>();

        for (int i = 0; i < faceBatches.Count; i++)
        {
            var batch = faceBatches[i];
            var color = FacePalette[((legendColors?.Count ?? 0) + i) % FacePalette.Length];

            if (legendColors != null)
            {
                foreach (var (value, legendColor, _) in legendColors)
                {
                    if (value == batch.Key)
                    {
                        color = legendColor;
                        break;
                    }
                }
            }

            legend.Add((batch.Key, color, batch.FaceCount));

            var fill = ColorHelper.GetTransparencyColor(color, faceOpacity);

            foreach (var chunk in batch.Triangles.Chunk(DbgPrimWallCells.MaxTriangles))
            {
                var primitive = new DbgPrimWallCells(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    RenderTranslucent = true,
                    BaseColor = fill,
                    HighlightedColor = fill,
                };
                Add(proxy, primitive.Release);
            }
        }

        FaceLegend = legend;

        // Other filled layers in their colour: blockers a little more opaque than the faces, so they read over the
        // collision; gate markers at a fixed opacity, so their wireframe shows through
        foreach (var batch in overlay.Batches.Values.Where(b => b.Layer != NavOverlayLayer.Faces && b.Triangles.Count > 0))
        {
            float opacity = batch.Layer == NavOverlayLayer.Blockers ? Math.Max(faceOpacity, 40) : GateFillOpacity;
            var fill = ColorHelper.GetTransparencyColor(LayerColor(batch.Layer), opacity);

            foreach (var chunk in batch.Triangles.Chunk(DbgPrimWallCells.MaxTriangles))
            {
                var primitive = new DbgPrimWallCells(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    RenderTranslucent = true,
                    BaseColor = fill,
                    HighlightedColor = fill,
                };
                Add(proxy, primitive.Release);
            }
        }

        foreach (var batch in overlay.Batches.Values.Where(b => b.Layer != NavOverlayLayer.Faces && b.Lines.Count > 0))
        {
            var color = ToColor(LayerColor(batch.Layer));

            foreach (var chunk in batch.Lines.Chunk(DbgPrimLineBatch.MaxLines))
            {
                var primitive = new DbgPrimLineBatch(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    BaseColor = color,
                    HighlightedColor = color,
                    // The picked face shows through what is in front of it
                    RenderOverlay = batch.Layer is NavOverlayLayer.Highlight or NavOverlayLayer.GateSelected,
                };
                Add(proxy, primitive.Release);
            }
        }

        Pending = built;
        PendingShownFrames = 0;
    }

    /// <summary>Removes what is shown at once, including an overlay still on its way.</summary>
    public void Clear()
    {
        Release(Shown);

        if (Pending != null)
            Release(Pending);

        Pending = null;
        PendingShownFrames = 0;
        HasDrawn = false;
        FaceLegend = Array.Empty<(uint, Vector3, int)>();
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
