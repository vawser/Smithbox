using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Dummy;

public class DuplicateMultipleDummies : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private Multiselection Multiselect;
    private List<FLVER.Dummy> DupedObjects;

    public DuplicateMultipleDummies(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER.Dummy>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedDummy = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Dummies[idx] != null)
                DupedObjects.Add(CurrentFLVER.Dummies[idx].Clone());
        }

        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Dummies.Add(DupedObjects[i]);
        }

        Multiselect.StoredIndices = new List<int>();

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Dummies.Remove(DupedObjects[i]);
        }

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}