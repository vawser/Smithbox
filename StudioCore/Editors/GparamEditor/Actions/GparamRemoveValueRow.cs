using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor.Actions;

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
        Screen.FieldValueList.ReduceDisplayTruth(SelectedField);

        RemovedRowIndex = Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.Id);

        Screen.Selection.ToggleSelectedFileModifiedState(true);
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.FieldValueList.ExtendDisplayTruth(SelectedField);

        Screen.PropertyEditor.AddPropertyValueRowAtIndex(SelectedField, SelectedFieldValue, RemovedRowID, RemovedRowIndex);

        Screen.Selection.ToggleSelectedFileModifiedState(false);
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}
