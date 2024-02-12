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

namespace StudioCore.Editors.MsbEditor;

public class MapViewGrid
{
    private DbgPrimWireGrid WireGrid;

    private DebugPrimitiveRenderableProxy ViewportGrid;

    private MeshRenderables _renderlist;

    public MapViewGrid(MeshRenderables renderlist)
    {
        _renderlist = renderlist;

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.Viewport_Grid_Size, CFG.Current.Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.Viewport_Grid_Color);
    }

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Regenerate()
    {
        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.Viewport_Grid_Size, CFG.Current.Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.Viewport_Grid_Color);
    }

    public void Update(Ray ray)
    {
        if (CFG.Current.Viewport_EnableGrid)
        {
            ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.Viewport_Grid_Color);
            ViewportGrid.Visible = true;
            ViewportGrid.World = new Transform(0, CFG.Current.Viewport_Grid_Height, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            ViewportGrid.Visible = false;
        }

        if (CFG.Current.Viewport_RegenerateMapGrid)
        {
            CFG.Current.Viewport_RegenerateMapGrid = false;

            Regenerate();
            Regenerate();
        }
    }
}
