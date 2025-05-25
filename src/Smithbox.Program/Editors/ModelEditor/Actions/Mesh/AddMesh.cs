using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class AddMesh : ViewportAction
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddMesh(ModelEditorScreen editor, FLVER2 flver)
    {
        Editor = editor;
        Selection = editor.Selection;
        ViewportManager = editor.ViewportManager;

        PreviousSelectionIndex = editor.Selection._selectedMesh;

        CurrentFLVER = flver;
        NewObject = new FLVER2.Mesh();

        var emptyBoundingBoxes = new FLVER2.Mesh.BoundingBoxes();
        var emptyFaceset = new FLVER2.FaceSet();
        var emptyVertexBuffer = new FLVER2.VertexBuffer(0);
        NewObject.BoundingBox = emptyBoundingBoxes;
        NewObject.FaceSets.Add(emptyFaceset);
        NewObject.VertexBuffers.Add(emptyVertexBuffer);

        Index = flver.Meshes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes.Insert(Index, NewObject);
        Selection._selectedMesh = Index;

        Editor.ViewportManager.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Editor.ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}