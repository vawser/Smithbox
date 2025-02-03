using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Material;

public class RemoveMultipleMaterials : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER2.Material> RemovedObjects;
    private List<FLVER2.Material> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMultipleMaterials(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;

        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.Material>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.Material>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedMaterial = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Materials[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Materials[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Materials.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Materials.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}