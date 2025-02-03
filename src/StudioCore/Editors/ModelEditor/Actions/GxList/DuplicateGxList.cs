using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxList;

public class DuplicateGxList : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.GXList DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateGxList(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedGXList;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.GXLists[index].Clone();
        Index = flver.GXLists.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.GXLists.Insert(Index, DupedObject);
        Selection._selectedGXList = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedGXList = PreviousSelectionIndex;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}