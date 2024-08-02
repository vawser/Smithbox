using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActExampleAction : EditorAction
{
    private GPARAM SelectedGPARAM;
    private GparamEditorScreen Screen;
    private IField SelectedField;
    private IFieldValue SelectedFieldValue;
    private int NewRowID;

    public TimeActExampleAction(GparamEditorScreen screen, GPARAM selectedGparam, IField selectedField, IFieldValue fieldValue, int dupeRowId)
    {
        SelectedGPARAM = selectedGparam;
        Screen = screen;
        SelectedField = selectedField;
        SelectedFieldValue = fieldValue;
        NewRowID = dupeRowId;
    }

    public override ActionEvent Execute()
    {
        Screen.ExtendDisplayTruth(SelectedField);

        Screen.PropertyEditor.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

        // Update the group index lists to account for the new ID.
        Screen._selectedGparamInfo.WasModified = true;
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ReduceDisplayTruth(SelectedField);

        Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

        Screen._selectedGparamInfo.WasModified = false;
        Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

        return ActionEvent.NoEvent;
    }
}