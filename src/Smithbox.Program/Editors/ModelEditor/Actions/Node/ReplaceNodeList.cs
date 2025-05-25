using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Node;

public class ReplaceNodeList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER.Node> OldNodes;
    private List<FLVER.Node> NewNodes;

    public ReplaceNodeList(ModelEditorScreen screen, List<FLVER.Node> nodes)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldNodes = [.. CurrentFLVER.Nodes];
        NewNodes = nodes;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Nodes = NewNodes;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Nodes = OldNodes;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}