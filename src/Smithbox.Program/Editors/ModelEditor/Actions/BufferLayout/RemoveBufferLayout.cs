using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BufferLayout;

public class RemoveBufferLayout : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.BufferLayout RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveBufferLayout(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBufferLayout;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.BufferLayouts[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedBufferLayout = -1;
        CurrentFLVER.BufferLayouts.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.BufferLayouts.Insert(Index, RemovedObject);
        Selection._selectedBufferLayout = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
