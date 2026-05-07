using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class ImportFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM CurrentGparam { get; set; }
    private GPARAM StoredGparam { get; set; }

    private GPARAM.Param Param { get; set; }

    private GPARAM.IField CurrentField { get; set; }
    private GPARAM.IField NewField { get; set; }
    private GPARAM.IField StoredField { get; set; }
    private int FieldIndex { get; set; }

    private bool Overwrite = false;

    public ImportFieldAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM gparam, GPARAM.Param param, GPARAM.IField field, GPARAM.IField newField)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        CurrentGparam = gparam;
        StoredGparam = gparam.Clone();

        Param = param;

        CurrentField = field;
        NewField = newField;
        StoredField = param.CloneField(field);

        FieldIndex = param.Fields.IndexOf(field);

    }

    public override ActionEvent Execute()
    {
        if (NewField != null)
        {
            // Overwrite existing field if imported field is a match
            if (Param.Fields.Any(e => e.Key == NewField.Key))
            {
                Overwrite = true;
                var target = Param.Fields.First(e => e.Key == NewField.Key);
                FieldIndex = Param.Fields.IndexOf(target);
            }
        }

        if (Overwrite)
        {
            Param.Fields[FieldIndex] = NewField;
        }
        else
        {
            Param.Fields.Add(NewField);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGparam = StoredGparam;

        var target = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key == FileEntry);
        if (target.Key != null)
        {
            Project.Handler.GparamData.PrimaryBank.Entries[FileEntry] = StoredGparam;
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }
}
