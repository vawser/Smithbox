using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Texture;

public class RemoveTexture : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.Material CurrentMaterial;
    private FLVER2.Texture StoredTexture;
    private FLVER2.Texture OldObject;

    public RemoveTexture(ModelEditorScreen screen, FLVER2 flver, FLVER2.Texture curTexture)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentMaterial = flver.Materials[Selection._selectedMaterial];

        StoredTexture = curTexture.Clone();
        OldObject = curTexture;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentMaterial.Textures.Count > 1)
            Selection._subSelectedTextureRow = 0;
        else
            Selection._subSelectedTextureRow = -1;

        CurrentMaterial.Textures.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMaterial.Textures.Add(StoredTexture);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}