using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class InsertFieldValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM CurrentGparam { get; set; }
    private GPARAM StoredGparam { get; set; }

    private GPARAM.Param Param { get; set; }

    private GPARAM.IField Field { get; set; }
    private GPARAM.IField StoredFieldData { get; set; }
    private int FieldIndex { get; set; }

    private GPARAM.IFieldValue CurrentValue { get; set; }
    private GPARAM.IFieldValue NewValue { get; set; }
    private GPARAM.IFieldValue StoredValue { get; set; }
    private int ValueIndex { get; set; }

    private bool Overwrite = false;


    public InsertFieldValueAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry,
        GPARAM gparam, GPARAM.Param param, GPARAM.IField field, GPARAM.IFieldValue value, GPARAM.IFieldValue newValue,
        bool overwrite)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        CurrentGparam = gparam;
        StoredGparam = gparam.Clone();

        Param = param;

        Field = field;
        StoredFieldData = param.CloneField(field);
        FieldIndex = param.Fields.IndexOf(field);

        CurrentValue = value;
        NewValue = newValue;
        StoredValue = field.CloneValue(value);
        ValueIndex = field.IndexOfValue(value);

        Overwrite = overwrite;
    }

    public override ActionEvent Execute()
    {
        if (Overwrite)
        {
            Field.SetValue(ValueIndex, NewValue);
        }
        else
        {
            Field.AddValue(ValueIndex, NewValue);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGparam = StoredGparam;

        var target = Project.Handler.GparamData.PrimaryBank.Entries.GetValueOrDefault(FileEntry);

        if (target != null)
        {
            Project.Handler.GparamData.PrimaryBank.Entries[FileEntry] = StoredGparam;
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }
}
