using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BufferLayout;

public class AddBufferLayout : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.BufferLayout NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddBufferLayout(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBufferLayout;

        CurrentFLVER = flver;
        NewObject = new FLVER2.BufferLayout();
        var emptyLayoutMember = new FLVER.LayoutMember();
        NewObject.Add(emptyLayoutMember);
        Index = flver.BufferLayouts.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.BufferLayouts.Insert(Index, NewObject);
        Selection._selectedBufferLayout = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedBufferLayout = PreviousSelectionIndex;
        CurrentFLVER.BufferLayouts.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}