using SoulsFormats;
using StudioCore.Editors.Common;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamRemoveValueRow : EditorAction
{
    private GPARAM SelectedGPARAM;
    private GparamEditorScreen Screen;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int RemovedRowID;
    private int RemovedRowIndex;

    public GparamRemoveValueRow(GparamEditorScreen screen)
    {
        Screen = screen;
        SelectedGPARAM = screen.Selection._selectedGparam;
        SelectedField = screen.Selection._selectedParamField;
        SelectedFieldValue = screen.Selection._selectedFieldValue;
        RemovedRowID = screen.Selection._selectedFieldValue.Id;
    }

    public override ActionEvent Execute()
    {
        Screen.FieldValueListView.ReduceDisplayTruth(SelectedField);

        RemovedRowIndex = Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.Id);

        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.FieldValueListView.ExtendDisplayTruth(SelectedField);

        Screen.PropertyEditor.AddPropertyValueRowAtIndex(SelectedField, SelectedFieldValue, RemovedRowID, RemovedRowIndex);

        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
