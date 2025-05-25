using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class RemoveMesh : ViewportAction
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveMesh(ModelEditorScreen editor, FLVER2 flver, int index)
    {
        Editor = editor;
        Selection = editor.Selection;
        ViewportManager = editor.ViewportManager;

        PreviousSelectionIndex = editor.Selection._selectedMesh;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Meshes[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedMesh = -1;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes.Insert(Index, RemovedObject);
        Selection._selectedMesh = PreviousSelectionIndex;

        Editor.ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}