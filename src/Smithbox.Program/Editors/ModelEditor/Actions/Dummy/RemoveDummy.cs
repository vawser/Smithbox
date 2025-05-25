using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Dummy;

public class RemoveDummy : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER.Dummy RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveDummy(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedDummy;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Dummies[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedDummy = -1;
        CurrentFLVER.Dummies.RemoveAt(Index);

        ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Dummies.Insert(Index, RemovedObject);
        Selection._selectedDummy = PreviousSelectionIndex;

        ViewportManager.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
