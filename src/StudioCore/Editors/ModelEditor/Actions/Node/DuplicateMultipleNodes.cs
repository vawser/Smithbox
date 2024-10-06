using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Node;

public class DuplicateMultipleNodes : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER.Node> DupedObjects;

    public DuplicateMultipleNodes(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER.Node>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedNode = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Nodes[idx] != null)
                DupedObjects.Add(CurrentFLVER.Nodes[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Nodes.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Nodes.Remove(DupedObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}