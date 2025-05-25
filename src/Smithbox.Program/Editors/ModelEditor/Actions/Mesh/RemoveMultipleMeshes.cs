using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class RemoveMultipleMeshes : ViewportAction
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER2.Mesh> RemovedObjects;
    private List<FLVER2.Mesh> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMultipleMeshes(ModelEditorScreen editor, FLVER2 flver, Multiselection multiselect)
    {
        Editor = editor;
        Selection = editor.Selection;
        ViewportManager = editor.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.Mesh>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.Mesh>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedMesh = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Meshes[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Meshes[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Meshes.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Meshes.Insert(StoredIndices[i], StoredObjects[i]);
        }

        Editor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}