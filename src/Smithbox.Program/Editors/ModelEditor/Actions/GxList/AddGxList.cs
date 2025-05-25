using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxList;

public class AddGxList : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.GXList NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddGxList(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedGXList;

        CurrentFLVER = flver;
        NewObject = new FLVER2.GXList();
        var emptyGXItem = new FLVER2.GXItem();
        NewObject.Add(emptyGXItem);
        Index = flver.GXLists.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.GXLists.Insert(Index, NewObject);
        Screen.Selection._selectedGXList = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.Selection._selectedGXList = PreviousSelectionIndex;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
