using SoulsFormats;
using StudioCore.Editor;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamDuplicateValueRow : EditorAction
{
    private GparamEditor Editor;

    private GPARAM SelectedGPARAM;

    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int NewRowID;

    public GparamDuplicateValueRow(GparamEditor editor)
    {
        Editor = editor;
        SelectedGPARAM = editor.Selection._selectedGparam;
        SelectedField = editor.Selection._selectedParamField;
        SelectedFieldValue = editor.Selection._selectedFieldValue;
        NewRowID = editor.Selection._duplicateValueRowId;
    }

    public override ActionEvent Execute()
    {
        Editor.FieldEntryView.ExtendDisplayTruth(SelectedField);

        Editor.FieldInput.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

        // Update the group index lists to account for the new ID.
        Editor.FieldInput.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Editor.FieldEntryView.ReduceDisplayTruth(SelectedField);

        Editor.FieldInput.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

        Editor.FieldInput.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
