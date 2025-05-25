using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BufferLayout;

public class DuplicateBufferLayout : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.BufferLayout DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateBufferLayout(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBufferLayout;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.BufferLayouts[index].Clone();
        Index = flver.BufferLayouts.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.BufferLayouts.Insert(Index, DupedObject);
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
