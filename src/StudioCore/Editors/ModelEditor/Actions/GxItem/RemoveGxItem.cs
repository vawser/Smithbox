using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxItem;

public class RemoveGxItem : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.GXList CurrentGXList;
    private FLVER2.GXItem StoredItem;
    private FLVER2.GXItem OldObject;

    public RemoveGxItem(ModelEditorScreen screen, FLVER2 flver, FLVER2.GXItem curItem)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentGXList = flver.GXLists[Selection._selectedGXList];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentGXList.Count > 1)
            Selection._subSelectedGXItemRow = 0;
        else
            Selection._subSelectedGXItemRow = -1;

        CurrentGXList.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentGXList.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
