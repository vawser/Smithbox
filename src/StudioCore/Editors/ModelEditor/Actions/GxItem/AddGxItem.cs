using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxItem;

public class AddGxItem : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.GXList CurrentGXList;
    private FLVER2.GXItem NewObject;

    public AddGxItem(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentGXList = flver.GXLists[Selection._selectedGXList];

        NewObject = new FLVER2.GXItem();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentGXList.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentGXList.Count > 1)
            Selection._subSelectedGXItemRow = 0;
        else
            Selection._subSelectedGXItemRow = -1;

        CurrentGXList.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}