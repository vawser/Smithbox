using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Gparam Merge into one file: runs the merge engine's Apply in place, undone by restoring a snapshot taken just
/// before (Claude/Documentation/Gparam Merge Plan.md §4, P3). The chosen items are fixed when the action is made, so
/// a redo applies the same ones whatever the review shows by then. Items whose entry changed since the analysis are
/// skipped as stale, on every Execute.
/// </summary>
public class GparamMergeAction : EditorAction
{
    private readonly ProjectEntry Project;
    private readonly GPARAM Data;
    private readonly List<GparamMergeItem> Chosen;

    private GparamMergeSnapshot Before;

    public FileDictionaryEntry FileEntry { get; }

    /// <summary>What the last Execute did</summary>
    public GparamMergeApplyResult LastResult { get; private set; }

    public GparamMergeAction(ProjectEntry project, FileDictionaryEntry fileEntry, GPARAM gparam, List<GparamMergeItem> chosen)
    {
        Project = project;
        FileEntry = fileEntry;
        Data = gparam;
        Chosen = chosen;
    }

    public override ActionEvent Execute()
    {
        Before = GparamMergeSnapshot.Take(Data);
        LastResult = GparamMerge.Apply(Data, Chosen);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return LastResult.Applied > 0 ? ActionEvent.ObjectAddedRemoved : ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Before == null)
            return ActionEvent.NoEvent;

        Before.Restore();
        Before = null;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
