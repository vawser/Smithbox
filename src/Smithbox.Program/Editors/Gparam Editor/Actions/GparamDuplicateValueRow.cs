using SoulsFormats;
using StudioCore.Editors.Common;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamDuplicateValueRow : EditorAction
{
    private GPARAM SelectedGPARAM;
    private GparamEditorScreen Screen;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int NewRowID;

    public GparamDuplicateValueRow(GparamEditorScreen screen)
    {
        Screen = screen;
        SelectedGPARAM = screen.Selection._selectedGparam;
        SelectedField = screen.Selection._selectedParamField;
        SelectedFieldValue = screen.Selection._selectedFieldValue;
        NewRowID = screen.Selection._duplicateValueRowId;
    }

    public override ActionEvent Execute()
    {
        Screen.FieldValueListView.ExtendDisplayTruth(SelectedField);

        Screen.PropertyEditor.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

        // Update the group index lists to account for the new ID.
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.FieldValueListView.ReduceDisplayTruth(SelectedField);

        Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
