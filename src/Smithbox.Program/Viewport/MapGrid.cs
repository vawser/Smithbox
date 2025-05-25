using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Framework;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.ViewportNS;

public class MapGrid
{
    private MapEditorScreen Editor;

    private DbgPrimWireGrid WireGridPrimitive;
    private DebugPrimitiveRenderableProxy Grid;
    private MeshRenderables RenderList;

    public MapGrid(MapEditorScreen editor, MeshRenderables renderlist)
    {
        Editor = editor;
        RenderList = renderlist;

        WireGridPrimitive = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

        Grid = new DebugPrimitiveRenderableProxy(RenderList, WireGridPrimitive);
        Grid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
    }

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Regenerate()
    {
        WireGridPrimitive.Dispose();
        Grid.Dispose();

        WireGridPrimitive = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.MapEditor_Viewport_Grid_Size, CFG.Current.MapEditor_Viewport_Grid_Square_Size);

        Grid = new DebugPrimitiveRenderableProxy(RenderList, WireGridPrimitive);
        Grid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
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
            Grid.BaseColor = GetViewGridColor(CFG.Current.MapEditor_Viewport_Grid_Color);
            Grid.Visible = true;
            Grid.World = new Transform(0, CFG.Current.MapEditor_Viewport_Grid_Height, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            Grid.Visible = false;
        }
    }
}
