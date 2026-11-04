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

    public ViewportGrid(MeshRenderables renderlist)
    {
        Grid = new DebugPrimitiveRenderableProxy(renderlist, new DbgPrimGridPlane());
        Grid.RenderTranslucent = true;
        Grid.Pickable = false;
        Grid.Visible = false;
    }

    private static Color GetViewGridColor(Vector3 color, float opacity)
    {
        static int ToByte(float value) => (int)(Math.Clamp(value, 0f, 1f) * 255);

        return Color.FromArgb(ToByte(opacity), ToByte(color.X), ToByte(color.Y), ToByte(color.Z));
    }

    public void Update(bool isEditorFocused, ViewportGridSettings config)
    {
        if (config.Display && isEditorFocused)
        {
            var cells = Math.Max(config.Size, 1);
            var sectionSize = Math.Max(config.SectionSize, MinSectionSize);

            // One square larger than the grid, see ViewportGrid.frag
            var planeScale = (cells + 1) * sectionSize;

            Grid.BaseColor = GetViewGridColor(config.Color, CFG.Current.Viewport_Grid_Opacity);
            Grid.GridCells = cells;
            Grid.GridMajorInterval = CFG.Current.Viewport_Grid_Major_Line_Interval;
            Grid.GridFadeDistance = Math.Max(CFG.Current.Viewport_Grid_Fade_Distance, 0f);
            Grid.GridAxisLines = CFG.Current.Viewport_Grid_Display_Axis_Lines;
            Grid.Visible = true;
            Grid.World = Matrix4x4.CreateScale(planeScale, 1, planeScale) *
                ViewportGridMovement.GetGridTransform(config).WorldMatrix;
        }
        else
        {
            Grid.Visible = false;
        }
    }
}
