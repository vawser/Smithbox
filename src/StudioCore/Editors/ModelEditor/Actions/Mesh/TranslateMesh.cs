using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class TranslateMesh : ViewportAction
{
    private ModelEditorScreen Editor;
    private FLVER2 CurrentFLVER;
    private List<FLVER.Vertex> OriginalVertices;
    private FLVER2.Mesh CurrentMesh;
    private Vector3 ChangeVector;

    public TranslateMesh(ModelEditorScreen editor, FLVER2 curFlver, FLVER2.Mesh curMesh, Vector3 changeVector)
    {
        Editor = editor;
        CurrentFLVER = curFlver;

        OriginalVertices = new List<FLVER.Vertex>();
        foreach(var entry in curMesh.Vertices)
        {
            OriginalVertices.Add(entry.Clone());
        }
         
        CurrentMesh = curMesh;
        ChangeVector = changeVector;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        VertexUtils.TranslateMesh(CurrentMesh, ChangeVector);

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.Vertices = OriginalVertices;

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}