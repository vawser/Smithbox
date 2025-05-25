using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Node;

public class RemoveNode : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER.Node RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveNode(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedNode;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Nodes[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedNode = -1;
        CurrentFLVER.Nodes.RemoveAt(Index);

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Nodes.Insert(Index, RemovedObject);
        Selection._selectedNode = PreviousSelectionIndex;

        ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
