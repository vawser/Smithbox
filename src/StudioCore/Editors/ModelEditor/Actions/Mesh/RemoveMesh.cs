using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class RemoveMesh : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveMesh(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedMesh;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Meshes[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedMesh = -1;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes.Insert(Index, RemovedObject);
        Selection._selectedMesh = PreviousSelectionIndex;

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}