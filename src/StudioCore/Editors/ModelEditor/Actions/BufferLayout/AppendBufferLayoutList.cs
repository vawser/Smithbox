using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BufferLayout;

public class AppendBufferLayoutList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.BufferLayout> OldBufferLayouts;
    private List<FLVER2.BufferLayout> NewBufferLayouts;

    public AppendBufferLayoutList(ModelEditorScreen screen, List<FLVER2.BufferLayout> BufferLayouts)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldBufferLayouts = [.. CurrentFLVER.BufferLayouts];
        NewBufferLayouts = BufferLayouts;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var entry in NewBufferLayouts)
        {
            CurrentFLVER.BufferLayouts.Add(entry);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.BufferLayouts = OldBufferLayouts;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}