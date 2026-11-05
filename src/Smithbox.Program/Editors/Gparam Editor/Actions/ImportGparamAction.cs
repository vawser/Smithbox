using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Replace a GPARAM's contents with an imported one's.
/// The contents are copied into the existing GPARAM rather than the file's GPARAM being swapped for the imported one:
/// every other action on the undo stack holds the existing instance, and would edit a detached object otherwise.
/// </summary>
public class ImportGparamAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM Target { get; set; }

    // The target's contents before and after the import. Both hold the groups themselves, not copies, so undo and
    // redo put back the very objects later actions refer to.
    private GPARAM StoredState { get; set; } = new();
    private GPARAM ImportedState { get; set; }

    public ImportGparamAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM data, GPARAM newData)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        Target = data;
        ImportedState = newData;

        // Lists the writer needs, which the JSON may leave out, and capacities it trusts to match the values
        ImportedState.Params ??= new System.Collections.Generic.List<GPARAM.Param>();

        foreach (var group in ImportedState.Params)
        {
            if (group == null)
                continue;

            group.Fields ??= new System.Collections.Generic.List<GPARAM.IField>();
            group.Comments ??= new System.Collections.Generic.List<string>();

            foreach (var field in group.Fields)
            {
                field?.SyncCapacity();
            }
        }
    }

    public override ActionEvent Execute()
    {
        GparamUtils.CopyGparamState(StoredState, Target);
        GparamUtils.CopyGparamState(Target, ImportedState);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Target);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        GparamUtils.CopyGparamState(Target, StoredState);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Target);

        return ActionEvent.ObjectAddedRemoved;
    }
}
