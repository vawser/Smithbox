using ImGuiNET;
using StudioCore.DebugPrimitives;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.MsbEditor;

public class ViewGrid
{
    private DbgPrimWireGrid WireGrid;

    private DebugPrimitiveRenderableProxy ViewportGrid;

    public ViewGrid(MeshRenderables renderlist)
    {
        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.Map_ViewportGrid_TotalSize, CFG.Current.Map_ViewportGrid_IncrementSize);

        ViewportGrid = new DebugPrimitiveRenderableProxy(renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.GFX_Viewport_Grid_Color);
    }

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Update(Ray ray)
    {
        if (CFG.Current.Map_EnableViewportGrid)
        {
            ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.GFX_Viewport_Grid_Color);
            ViewportGrid.Visible = true;
            ViewportGrid.World = new Transform(0, CFG.Current.Map_ViewportGrid_Offset, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            ViewportGrid.Visible = false;
        }
    }
}
