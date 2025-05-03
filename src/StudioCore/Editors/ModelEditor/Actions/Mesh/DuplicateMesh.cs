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
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Mesh DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateMesh(ModelEditorScreen editor, FLVER2 flver, int index)
    {
        Editor = editor;
        Selection = editor.Selection;
        ViewportManager = editor.ViewportManager;

        PreviousSelectionIndex = editor.Selection._selectedMesh;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Meshes[index].Clone();
        Index = flver.Meshes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes.Insert(Index, DupedObject);
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