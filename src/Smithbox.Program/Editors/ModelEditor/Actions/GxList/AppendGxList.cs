using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxList;

public class AppendGxList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.GXList> OldGXLists;
    private List<FLVER2.GXList> NewGXLists;

    public AppendGxList(ModelEditorScreen screen, List<FLVER2.GXList> GXLists)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldGXLists = [.. CurrentFLVER.GXLists];
        NewGXLists = GXLists;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var entry in NewGXLists)
        {
            CurrentFLVER.GXLists.Add(entry);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.GXLists = OldGXLists;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
