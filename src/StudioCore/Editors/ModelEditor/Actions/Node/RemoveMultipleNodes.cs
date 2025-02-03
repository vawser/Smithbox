using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Node;

public class RemoveMultipleNodes : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER.Node> RemovedObjects;
    private List<FLVER.Node> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMultipleNodes(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;

        Multiselect = multiselect;
        RemovedObjects = new List<FLVER.Node>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER.Node>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedNode = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Nodes[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Nodes[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Nodes.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Nodes.Insert(StoredIndices[i], StoredObjects[i]);
        }

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
