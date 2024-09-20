using ImGuiNET;
using StudioCore.DebugPrimitives;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
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

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
    }

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Regenerate()
    {
        WireGrid.Dispose();
        ViewportGrid.Dispose();

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
    }

    public void Update(Ray ray)
    {
        if (CFG.Current.MapEditor_Viewport_RegenerateMapGrid)
        {
            CFG.Current.MapEditor_Viewport_RegenerateMapGrid = false;

            Regenerate();
        }

        if (UI.Current.Interface_MapEditor_Viewport_Grid && Smithbox.EditorHandler.FocusedEditor is MapEditorScreen)
        {
            ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
            ViewportGrid.Visible = true;
            ViewportGrid.World = new Transform(0, CFG.Current.MapEditor_Viewport_Grid_Height, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            ViewportGrid.Visible = false;
        }
    }
}
