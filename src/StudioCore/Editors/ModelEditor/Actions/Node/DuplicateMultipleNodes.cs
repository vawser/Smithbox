using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Node;

public class DuplicateMultipleNodes : ViewportAction
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER.Node> DupedObjects;

    public DuplicateMultipleNodes(ModelEditorScreen editor, FLVER2 flver, Multiselection multiselect)
    {
        Editor = editor;
        Selection = editor.Selection;
        ViewportManager = editor.ViewportManager;

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

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Nodes.Remove(DupedObjects[i]);
        }

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}