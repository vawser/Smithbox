using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.LayoutMember;

public class DuplicateLayoutMember : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.BufferLayout CurrentBufferLayout;
    private FLVER.LayoutMember NewObject;

    public DuplicateLayoutMember(ModelEditorScreen screen, FLVER2 flver, FLVER.LayoutMember curMember)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentBufferLayout = flver.BufferLayouts[Selection._selectedBufferLayout];

        NewObject = curMember.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentBufferLayout.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentBufferLayout.Count > 1)
            Selection._subSelectedBufferLayoutMember = 0;
        else
            Selection._subSelectedBufferLayoutMember = -1;

        CurrentBufferLayout.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}