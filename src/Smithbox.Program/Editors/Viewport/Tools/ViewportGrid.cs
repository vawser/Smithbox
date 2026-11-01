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

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Update(bool isEditorFocused, ViewportGridConfig config)
    {
        if (config.Display && isEditorFocused)
        {
            var cells = Math.Max(config.Size, 1);
            var sectionSize = Math.Max(config.SectionSize, MinSectionSize);

            // One square larger than the grid, see ViewportGrid.frag
            var planeScale = (cells + 1) * sectionSize;

            Grid.BaseColor = GetViewGridColor(config.Color);
            Grid.GridCells = cells;
            Grid.Visible = true;
            Grid.World = Matrix4x4.CreateScale(planeScale, 1, planeScale) * new Transform(
                config.PositionX,
                config.PositionY,
                config.PositionZ,
                Utils.DegToRadians(config.RotationX),
                Utils.DegToRadians(config.RotationY),
                Utils.DegToRadians(config.RotationZ)).WorldMatrix;
        }
        else
        {
            Grid.Visible = false;
        }
    }
}
