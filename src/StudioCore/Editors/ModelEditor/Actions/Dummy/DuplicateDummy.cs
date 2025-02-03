using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Dummy;

public class DuplicateDummy : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER.Dummy DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateDummy(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedDummy;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Dummies[index].Clone();
        Index = flver.Dummies.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Dummies.Insert(Index, DupedObject);
        Selection._selectedDummy = Index;

        ViewportManager.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedDummy = PreviousSelectionIndex;
        CurrentFLVER.Dummies.RemoveAt(Index);

        ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}