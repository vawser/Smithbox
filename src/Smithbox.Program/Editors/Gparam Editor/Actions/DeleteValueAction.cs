using SoulsFormats;
using StudioCore.Editors.Common;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class DeleteValueAction : EditorAction
{
    private GparamEditorView Parent;

    private GPARAM SelectedGPARAM;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int RemovedRowID;
    private int RemovedRowIndex;

    public DeleteValueAction(GparamEditorView view)
    {
        Parent = view;

        SelectedGPARAM = view.Selection.GetSelectedGparam();
        SelectedField = view.Selection.GetSelectedField();
        SelectedFieldValue = view.Selection.GetSelectedValue();
        RemovedRowID = SelectedFieldValue.ID;
    }

    public override ActionEvent Execute()
    {
        Parent.FieldValueListView.ReduceDisplayTruth(SelectedField);

        RemovedRowIndex = Parent.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.ID);

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
