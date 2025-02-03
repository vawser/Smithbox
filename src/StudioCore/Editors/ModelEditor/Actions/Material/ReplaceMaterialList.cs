using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Material;

public class ReplaceMaterialList : ViewportAction
{
    private ModelEditorScreen Screen;

    private FLVER2 CurrentFLVER;

    private List<FLVER2.Material> OldMaterials;
    private List<FLVER2.Material> NewMaterials;

    public ReplaceMaterialList(ModelEditorScreen screen, List<FLVER2.Material> materials)
    {
        Screen = screen;

        CurrentFLVER = screen.ResManager.GetCurrentFLVER();

        OldMaterials = [.. CurrentFLVER.Materials];
        NewMaterials = materials;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Materials = NewMaterials;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Materials = OldMaterials;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
