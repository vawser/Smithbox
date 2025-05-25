using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Dummy;

public class RemoveMultipleDummies : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private Multiselection Multiselect;
    private List<FLVER.Dummy> RemovedObjects;
    private List<FLVER.Dummy> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMultipleDummies(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER.Dummy>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER.Dummy>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedDummy = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Dummies[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Dummies[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Dummies.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Dummies.Insert(StoredIndices[i], StoredObjects[i]);
        }

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}