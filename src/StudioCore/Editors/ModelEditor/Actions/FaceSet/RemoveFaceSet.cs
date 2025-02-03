using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.FaceSet;

public class RemoveFaceSet : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.FaceSet StoredItem;
    private FLVER2.FaceSet OldObject;

    public RemoveFaceSet(ModelEditorScreen screen, FLVER2 flver, FLVER2.FaceSet curItem)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Selection._selectedMesh];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentMesh.FaceSets.Count > 1)
            Selection._subSelectedFaceSetRow = 0;
        else
            Selection._subSelectedFaceSetRow = -1;

        CurrentMesh.FaceSets.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.FaceSets.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}