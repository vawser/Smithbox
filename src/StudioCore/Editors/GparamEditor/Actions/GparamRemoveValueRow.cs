using SoulsFormats;
using StudioCore.Editor;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamRemoveValueRow : EditorAction
{
    private GparamEditor Editor;

    private GPARAM SelectedGPARAM;

    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int RemovedRowID;
    private int RemovedRowIndex;

    public GparamRemoveValueRow(GparamEditor editor)
    {
        Editor = editor;
        SelectedGPARAM = editor.Selection._selectedGparam;
        SelectedField = editor.Selection._selectedParamField;
        SelectedFieldValue = editor.Selection._selectedFieldValue;
        RemovedRowID = editor.Selection._selectedFieldValue.Id;
    }

    public override ActionEvent Execute()
    {
        Editor.FieldEntryView.ReduceDisplayTruth(SelectedField);

        RemovedRowIndex = Editor.FieldInput.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.Id);

        Editor.FieldInput.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Editor.FieldEntryView.ExtendDisplayTruth(SelectedField);

        Editor.FieldInput.AddPropertyValueRowAtIndex(SelectedField, SelectedFieldValue, RemovedRowID, RemovedRowIndex);

        Editor.FieldInput.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
