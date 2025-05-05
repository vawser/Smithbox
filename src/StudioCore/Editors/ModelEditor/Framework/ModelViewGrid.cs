using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor.Framework;

public class ModelViewGrid
{
    private ModelEditorScreen Editor;

    private DbgPrimWireGrid WireGrid;

    private DebugPrimitiveRenderableProxy ViewportGrid;

    private MeshRenderables _renderlist;

    public ModelViewGrid(ModelEditorScreen editor, MeshRenderables renderlist)
    {
        Editor = editor;
        _renderlist = renderlist;

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.ModelEditor_Viewport_Grid_Size, CFG.Current.ModelEditor_Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.ModelEditor_Viewport_Grid_Color);
    }

    private Color GetViewGridColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public void Regenerate()
    {
        WireGrid.Dispose();
        ViewportGrid.Dispose();

        WireGrid = new DbgPrimWireGrid(Color.Red, Color.Red, CFG.Current.ModelEditor_Viewport_Grid_Size, CFG.Current.ModelEditor_Viewport_Grid_Square_Size);

        ViewportGrid = new DebugPrimitiveRenderableProxy(_renderlist, WireGrid);
        ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.ModelEditor_Viewport_Grid_Color);
    }

    public void Update(Ray ray)
    {
        if (CFG.Current.ModelEditor_Viewport_RegenerateMapGrid)
        {
            CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = false;

            Regenerate();
        }

        if (CFG.Current.Interface_ModelEditor_Viewport_Grid && Editor.Project.FocusedEditor is ModelEditorScreen)
        {
            ViewportGrid.BaseColor = GetViewGridColor(CFG.Current.ModelEditor_Viewport_Grid_Color);
            ViewportGrid.Visible = true;
            ViewportGrid.World = new Transform(0, CFG.Current.ModelEditor_Viewport_Grid_Height, 0, 0, 0, 0).WorldMatrix;
        }
        else
        {
            ViewportGrid.Visible = false;
        }
    }
}
