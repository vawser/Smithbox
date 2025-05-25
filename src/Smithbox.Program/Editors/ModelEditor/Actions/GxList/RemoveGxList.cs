using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxList;

public class RemoveGxList : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.GXList RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveGxList(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedGXList;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.GXLists[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedGXList = -1;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.GXLists.Insert(Index, RemovedObject);
        Selection._selectedGXList = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}