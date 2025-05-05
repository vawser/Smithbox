using StudioCore.Configuration;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Framework;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.Core;

public class MapViewportGrid
{
    private MapEditorScreen Editor;

    private DbgPrimWireGrid WireGrid;

    private DebugPrimitiveRenderableProxy ViewportGrid;

    private MeshRenderables _renderlist;

    public MapViewportGrid(MapEditorScreen editor, MeshRenderables renderlist)
    {
        Editor = editor;
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

        if (CFG.Current.Interface_MapEditor_Viewport_Grid && Editor.Project.FocusedEditor is MapEditorScreen)
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
