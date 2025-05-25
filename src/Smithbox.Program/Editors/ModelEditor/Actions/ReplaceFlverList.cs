using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ReplaceFLVERList : ViewportAction
{
    private ModelEditorScreen Screen;
    private List<FLVER2> OldFlvers;
    private List<FLVER2> NewFlvers;

    public ReplaceFLVERList(ModelEditorScreen screen, List<FLVER2> flvers)
    {
        Screen = screen;
        OldFlvers = [.. flvers];
        NewFlvers = flvers;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ResManager.LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER = NewFlvers[0];

        // TODO: should really update the viewport representation

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ResManager.LoadedFlverContainer.CurrentInternalFlver.CurrentFLVER = OldFlvers[0];

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
