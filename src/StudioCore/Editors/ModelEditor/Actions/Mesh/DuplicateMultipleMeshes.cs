using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class DuplicateMultipleMeshes : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private Multiselection Multiselect;
    private List<FLVER2.Mesh> DupedObjects;

    public DuplicateMultipleMeshes(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.Mesh>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedMesh = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Meshes[idx] != null)
                DupedObjects.Add(CurrentFLVER.Meshes[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Meshes.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Meshes.Remove(DupedObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}