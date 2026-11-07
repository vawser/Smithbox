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
/// </summary>
public sealed class NavigationOverlayRenderer
{
    private readonly List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();

    /// <summary>The faces' colour for each face data value, in the order of the legend.</summary>
    public IReadOnlyList<(uint Value, Vector3 Color, int Faces)> FaceLegend { get; private set; } = Array.Empty<(uint, Vector3, int)>();

    public bool IsShown => Shown.Count > 0;

    /// <summary>Distinct, fairly light colours for the face data values; the first is for 0 (no flags).</summary>
    private static readonly Vector3[] FacePalette =
    {
        new(0.35f, 0.75f, 0.45f), new(0.95f, 0.55f, 0.20f), new(0.30f, 0.55f, 0.95f), new(0.90f, 0.30f, 0.60f),
        new(0.95f, 0.85f, 0.25f), new(0.55f, 0.35f, 0.90f), new(0.25f, 0.85f, 0.85f), new(0.95f, 0.35f, 0.30f),
        new(0.60f, 0.85f, 0.25f), new(0.85f, 0.60f, 0.95f), new(0.55f, 0.55f, 0.55f), new(0.95f, 0.70f, 0.60f),
    };

    /// <summary>The colour of each line layer.</summary>
    public static Vector3 LayerColor(NavOverlayLayer layer) => layer switch
    {
        NavOverlayLayer.InnerEdges => new(0.20f, 0.20f, 0.20f),
        NavOverlayLayer.OpenEdges => new(1.00f, 0.25f, 0.25f),
        NavOverlayLayer.ConnectedInMap => new(0.25f, 0.90f, 1.00f),
        NavOverlayLayer.ConnectedCrossMap => new(1.00f, 0.30f, 1.00f),
        NavOverlayLayer.UserEdges => new(1.00f, 0.90f, 0.20f),
        NavOverlayLayer.BlockedUserEdges => new(1.00f, 0.55f, 0.10f),
        NavOverlayLayer.GateNodes => new(1.00f, 1.00f, 1.00f),
        NavOverlayLayer.OnavAreas => new(0.60f, 0.60f, 1.00f),
        NavOverlayLayer.OnavLinks => new(0.40f, 0.40f, 1.00f),
        NavOverlayLayer.OnavCells => new(0.45f, 0.45f, 0.75f),
        NavOverlayLayer.Highlight => new(1.00f, 1.00f, 1.00f),
        _ => new(1, 1, 1),
    };

    private static Color ToColor(Vector3 c, int alpha = 255)
        => Color.FromArgb(alpha, (int)(c.X * 255), (int)(c.Y * 255), (int)(c.Z * 255));

    /// <summary>
    /// Replaces what is shown with the overlay. Face fills use the given opacity (0 to 100).
    /// </summary>
    public void Show(RenderScene scene, NavOverlay overlay, float faceOpacity)
    {
        Clear();

        if (scene == null || overlay == null)
            return;

        var faceBatches = overlay.Batches.Values.Where(b => b.Layer == NavOverlayLayer.Faces).OrderBy(b => b.Key).ToList();
        var legend = new List<(uint, Vector3, int)>();

        for (int i = 0; i < faceBatches.Count; i++)
        {
            var batch = faceBatches[i];
            var color = FacePalette[i % FacePalette.Length];
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
                Shown.Add((proxy, primitive.Release));
            }
        }

        FaceLegend = legend;

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
                    RenderOverlay = batch.Layer == NavOverlayLayer.Highlight,
                };
                Shown.Add((proxy, primitive.Release));
            }
        }
    }

    public void Clear()
    {
        foreach (var (proxy, release) in Shown)
        {
            proxy.Dispose();
            release();
        }

        Shown.Clear();
        FaceLegend = Array.Empty<(uint, Vector3, int)>();
    }
}
