using SoulsFormats;
using StudioCore.Editors.Common;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamDuplicateValueRow : EditorAction
{
    private GparamEditorView Parent;

    private GPARAM SelectedGPARAM;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int NewRowID;

    public GparamDuplicateValueRow(GparamEditorView view)
    {
        Parent = view;
        SelectedGPARAM = view.Selection._selectedGparam;
        SelectedField = view.Selection._selectedParamField;
        SelectedFieldValue = view.Selection._selectedFieldValue;
        NewRowID = view.Selection._duplicateValueRowId;
    }

    public override ActionEvent Execute()
    {
        Parent.FieldValueListView.ExtendDisplayTruth(SelectedField);

        Parent.PropertyEditor.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

        // Update the group index lists to account for the new ID.
        Parent.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Parent.FieldValueListView.ReduceDisplayTruth(SelectedField);

        Parent.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

        Parent.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
