using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Material;

public class AddMaterial : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.Material NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddMaterial(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedMaterial;

        CurrentFLVER = flver;

        NewObject = new FLVER2.Material();

        var emptyMaterialTexture = new FLVER2.Texture();
        NewObject.Textures.Add(emptyMaterialTexture);
        Index = flver.Materials.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Materials.Insert(Index, NewObject);
        Selection._selectedMaterial = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedMaterial = PreviousSelectionIndex;
        CurrentFLVER.Materials.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}