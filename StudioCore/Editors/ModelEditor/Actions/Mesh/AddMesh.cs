using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class AddMesh : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddMesh(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedMesh;

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

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}