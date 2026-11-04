using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// A grid in the viewport, drawn by the ViewportGrid shader on a single quad.
/// </summary>
public class ViewportGrid
{
    private const float MinSectionSize = 0.001f;

    private DebugPrimitiveRenderableProxy Grid;

    /// <summary>
    /// The same grid again, drawn only where geometry hides it, more faintly. See Viewport_Grid_Hidden_Opacity.
    /// </summary>
    private DebugPrimitiveRenderableProxy HiddenGrid;

    public ViewportGrid(MeshRenderables renderlist)
    {
        Grid = CreateProxy(renderlist, false);
        HiddenGrid = CreateProxy(renderlist, true);
    }

    private static DebugPrimitiveRenderableProxy CreateProxy(MeshRenderables renderlist, bool occludedOnly)
    {
        var proxy = new DebugPrimitiveRenderableProxy(renderlist, new DbgPrimGridPlane());
        proxy.RenderTranslucent = true;
        proxy.RenderOccludedOnly = occludedOnly;
        proxy.Pickable = false;
        proxy.Visible = false;

        return proxy;
    }

    private static Color GetViewGridColor(Vector3 color, float opacity)
    {
        static int ToByte(float value) => (int)(Math.Clamp(value, 0f, 1f) * 255);

        return Color.FromArgb(ToByte(opacity), ToByte(color.X), ToByte(color.Y), ToByte(color.Z));
    }

    public void Update(bool isEditorFocused, ViewportGridSettings config, Vector3 cameraPosition)
    {
        if (config.Display && isEditorFocused)
        {
            var cells = Math.Max(config.Size, 1);
            var sectionSize = Math.Max(config.SectionSize, MinSectionSize);

            // One square larger than the grid, see ViewportGrid.frag
            var planeScale = (cells + 1) * sectionSize;

            var gridWorld = ViewportGridMovement.GetGridTransform(config).WorldMatrix;

            // Following the camera moves the quad under it in whole squares, so the lines don't slide
            var offset = Vector2.Zero;
            if (config.FollowCamera && Matrix4x4.Invert(gridWorld, out var toGrid))
            {
                var local = Vector3.Transform(cameraPosition, toGrid);
                offset = new Vector2(MathF.Round(local.X / sectionSize), MathF.Round(local.Z / sectionSize));
            }

            var world = Matrix4x4.CreateScale(planeScale, 1, planeScale) *
                Matrix4x4.CreateTranslation(offset.X * sectionSize, 0, offset.Y * sectionSize) *
                gridWorld;

            var opacity = Math.Clamp(CFG.Current.Viewport_Grid_Opacity, 0f, 1f);
            var hiddenOpacity = Math.Clamp(CFG.Current.Viewport_Grid_Hidden_Opacity, 0f, 1f);

            Apply(Grid, config, cells, offset, world, opacity);

            if (hiddenOpacity > 0f)
            {
                Apply(HiddenGrid, config, cells, offset, world, opacity * hiddenOpacity);
            }
            else
            {
                HiddenGrid.Visible = false;
            }
        }
        else
        {
            Grid.Visible = false;
            HiddenGrid.Visible = false;
        }
    }

    private static void Apply(DebugPrimitiveRenderableProxy proxy, ViewportGridSettings config,
        int cells, Vector2 offset, Matrix4x4 world, float opacity)
    {
        proxy.BaseColor = GetViewGridColor(config.Color, opacity);
        proxy.GridCells = cells;
        proxy.GridMajorInterval = CFG.Current.Viewport_Grid_Major_Line_Interval;
        proxy.GridFadeDistance = Math.Max(CFG.Current.Viewport_Grid_Fade_Distance, 0f);
        proxy.GridAxisLines = CFG.Current.Viewport_Grid_Display_Axis_Lines;
        proxy.GridLineWidth = Math.Clamp(CFG.Current.Viewport_Grid_Line_Width, 0.25f, 8f);
        proxy.GridOffset = offset;
        proxy.Visible = true;
        proxy.World = world;
    }
}
