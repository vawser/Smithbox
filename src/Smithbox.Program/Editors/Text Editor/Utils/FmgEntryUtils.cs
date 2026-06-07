using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public static class FmgEntryUtils
{
    /// <summary>
    /// Used to reorder the title + associated summary/description/effect entries
    /// </summary>
    public static void ReorderFmgEntryGroup(TextEditorView view, TextFmgWrapper wrapper, FMG fmg, List<FMG.Entry> entriesToMove, int dropTargetIndex)
    {
        var actions = new List<EditorAction>();

        FMG titleFmg = null;
        var titleEntries = new List<FMG.Entry>();

        FMG summaryFmg = null;
        var summaryEntries = new List<FMG.Entry>();

        FMG descriptionFmg = null;
        var descriptionEntries = new List<FMG.Entry>();

        FMG effectFmg = null;
        var effectEntries = new List<FMG.Entry>();

        foreach (var entry in entriesToMove)
        {
            var fmgEntryGroup = view.EntryGroupManager.GetEntryGroup(entry);

            // If non-grouped, handle like so.
            if (!fmgEntryGroup.SupportsGrouping)
            {
                titleFmg = fmg;
                titleEntries.Add(entry);
            }
            else
            {
                if (fmgEntryGroup.SupportsTitle)
                {
                    var titleEntry = fmgEntryGroup.Title;
                    if (titleEntry != null)
                    {
                        titleFmg = fmgEntryGroup.Title.Parent;

                        titleEntries.Add(titleEntry);
                    }
                }

                if (fmgEntryGroup.SupportsSummary)
                {
                    var summarEntry = fmgEntryGroup.Summary;
                    if (summarEntry != null)
                    {
                        summaryFmg = fmgEntryGroup.Summary.Parent;

                        summaryEntries.Add(summarEntry);
                    }
                }

                if (fmgEntryGroup.SupportsDescription)
                {
                    var descriptionEntry = fmgEntryGroup.Description;
                    if (descriptionEntry != null)
                    {
                        descriptionFmg = fmgEntryGroup.Description.Parent;

                        descriptionEntries.Add(descriptionEntry);
                    }
                }

                if (fmgEntryGroup.SupportsEffect)
                {
                    var effectEntry = fmgEntryGroup.Effect;
                    if (effectEntry != null)
                    {
                        effectFmg = fmgEntryGroup.Effect.Parent;

                        effectEntries.Add(effectEntry);
                    }
                }
            }
        }

        if (titleFmg != null)
        {
            var action = new ReorderEntryAction(titleFmg, titleEntries, dropTargetIndex);
            actions.Add(action);
        }

        if (summaryFmg != null)
        {
            var action = new ReorderEntryAction(summaryFmg, summaryEntries, dropTargetIndex);
            actions.Add(action);
        }

        if (descriptionFmg != null)
        {
            var action = new ReorderEntryAction(descriptionFmg, descriptionEntries, dropTargetIndex);
            actions.Add(action);
        }

        if (effectFmg != null)
        {
            var action = new ReorderEntryAction(effectFmg, effectEntries, dropTargetIndex);
            actions.Add(action);
        }

        var groupedAction = new FmgGroupedAction(actions);
        view.ActionManager.ExecuteAction(groupedAction);
    }

}
