using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Import a field: it replaces the group's field with the same key, or is added at the end.
/// Edits the group in place, so the objects older undo steps hold stay the live ones.
/// </summary>
public class ImportFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM Data { get; set; }

    private GPARAM.Param Param { get; set; }

    private GPARAM.IField NewField { get; set; }

    // The field it replaced, if any
    private GPARAM.IField ReplacedField { get; set; }

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public ImportFieldAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM gparam, GPARAM.Param param, GPARAM.IField field, GPARAM.IField newField)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        Data = gparam;
        Param = param;
        NewField = newField;

        // The JSON's capacity may not match its values, and the writer trusts it
        NewField.SyncCapacity();
    }

    public override ActionEvent Execute()
    {
        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);

        var index = Param.Fields.FindIndex(e => e != null && e.Key == NewField.Key);

        if (index >= 0)
        {
            ReplacedField = Param.Fields[index];
            Param.Fields[index] = NewField;
        }
        else
        {
            ReplacedField = null;
            Param.Fields.Add(NewField);
        }

        // The imported values' IDs may differ from the replaced field's
        GparamUtils.RebuildParamExtras(Data);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var index = Param.Fields.IndexOf(NewField);

        if (index >= 0)
        {
            if (ReplacedField != null)
            {
                Param.Fields[index] = ReplacedField;
            }
            else
            {
                Param.Fields.RemoveAt(index);
            }
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
