using SoulsFormats;
using StudioCore.Editor;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsParamEditorNS;

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
        Screen.FieldValueList.ExtendDisplayTruth(SelectedField);

        Screen.PropertyEditor.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

        // Update the group index lists to account for the new ID.
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.FieldValueList.ReduceDisplayTruth(SelectedField);

        Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
