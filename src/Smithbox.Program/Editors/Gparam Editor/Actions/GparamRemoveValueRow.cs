using SoulsFormats;
using StudioCore.Editors.Common;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamRemoveValueRow : EditorAction
{
    private GparamEditorView Parent;

    private GPARAM SelectedGPARAM;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int RemovedRowID;
    private int RemovedRowIndex;

    public GparamRemoveValueRow(GparamEditorView view)
    {
        Parent = view;

        SelectedGPARAM = view.Selection._selectedGparam;
        SelectedField = view.Selection._selectedParamField;
        SelectedFieldValue = view.Selection._selectedFieldValue;
        RemovedRowID = view.Selection._selectedFieldValue.Id;
    }

    public override ActionEvent Execute()
    {
        Parent.FieldValueListView.ReduceDisplayTruth(SelectedField);

        RemovedRowIndex = Parent.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.Id);

        Parent.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Parent.FieldValueListView.ExtendDisplayTruth(SelectedField);

        Parent.PropertyEditor.AddPropertyValueRowAtIndex(SelectedField, SelectedFieldValue, RemovedRowID, RemovedRowIndex);

        Parent.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
