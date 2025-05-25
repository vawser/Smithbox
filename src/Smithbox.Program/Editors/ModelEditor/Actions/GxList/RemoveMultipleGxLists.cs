using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.GxList;

public class RemoveMultipleGxLists : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER2.GXList> RemovedObjects;
    private List<FLVER2.GXList> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMultipleGxLists(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;

        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.GXList>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.GXList>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedGXList = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.GXLists[idx] != null)
                RemovedObjects.Add(CurrentFLVER.GXLists[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.GXLists.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.GXLists.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}