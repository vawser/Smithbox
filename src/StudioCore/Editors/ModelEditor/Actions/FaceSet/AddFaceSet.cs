using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.FaceSet;

public class AddFaceSet : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.FaceSet NewObject;

    public AddFaceSet(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Selection._selectedMesh];

        NewObject = new FLVER2.FaceSet();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMesh.FaceSets.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMesh.FaceSets.Count > 1)
            Selection._subSelectedFaceSetRow = 0;
        else
            Selection._subSelectedFaceSetRow = -1;

        CurrentMesh.FaceSets.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}