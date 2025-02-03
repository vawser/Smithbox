using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class DuplicateMesh : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateMesh(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedMesh;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Meshes[index].Clone();
        Index = flver.Meshes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes.Insert(Index, DupedObject);
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
    public override string GetEditMessage()
    {
        return "";
    }
}