using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Import a group: it replaces the file's group with the same key, or is added at the end.
/// With a replace target, it takes that group's place instead, whatever its key.
/// Edits the GPARAM in place, so the objects older undo steps hold stay the live ones.
/// </summary>
public class ImportParamAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM Data { get; set; }

    private GPARAM.Param NewGroup { get; set; }

    // The group to replace in place, whatever its key; null to match by key
    private GPARAM.Param ReplaceTarget { get; set; }

    // The group it replaced, if any
    private GPARAM.Param ReplacedGroup { get; set; }

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public ImportParamAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM gparam, GPARAM.Param param, GPARAM.Param newGroup,
        GPARAM.Param replaceTarget = null)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        Data = gparam;
        NewGroup = newGroup;
        ReplaceTarget = replaceTarget;

        // Lists the writer needs, which the JSON may leave out
        NewGroup.Fields ??= new List<GPARAM.IField>();
        NewGroup.Comments ??= new List<string>();

        foreach (var field in NewGroup.Fields)
        {
            field?.SyncCapacity();
        }
    }

    public override ActionEvent Execute()
    {
        var index = ReplaceTarget != null
            ? Data.Params.IndexOf(ReplaceTarget)
            : Data.Params.FindIndex(e => e != null && e.Key == NewGroup.Key);

        // The group to replace is gone (e.g. deleted since): nothing to do
        if (ReplaceTarget != null && index < 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);
        var previousParams = new List<GPARAM.Param>(Data.Params);

        if (index >= 0)
        {
            ReplacedGroup = Data.Params[index];
            Data.Params[index] = NewGroup;

            // The replaced group's extra entry carries over to the group that takes its place
            previousParams[index] = NewGroup;
        }
        else
        {
            ReplacedGroup = null;
            Data.Params.Add(NewGroup);
        }

        GparamUtils.RebuildParamExtras(Data, previousParams);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var index = Data.Params.IndexOf(NewGroup);

        // Execute did nothing (its replace target was gone)
        if (index < 0)
            return ActionEvent.NoEvent;

        if (ReplacedGroup != null)
        {
            Data.Params[index] = ReplacedGroup;
        }
        else
        {
            Data.Params.RemoveAt(index);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
