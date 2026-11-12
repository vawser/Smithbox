using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Reverting to the game's version, shared by the lists, the graph and the properties. Each revert is one undoable
/// step, and only offered for what the game has and has been changed.
/// </summary>
public static class TimeActRevert
{
    public static bool CanRevertTimeAct(TimeActContainerWrapper source, TimeActEntry entry)
    {
        return source.Vanilla.GetEntryDiff(entry).Status == TimeActVanillaStatus.Modified;
    }

    /// <summary>
    /// The whole time act goes back to the game's, read again from its bytes
    /// </summary>
    public static void RevertTimeAct(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry)
    {
        var diff = source.Vanilla.GetEntryDiff(entry);

        if (diff.Status != TimeActVanillaStatus.Modified || diff.Vanilla == null)
            return;

        entry.Load();

        if (entry.TimeAct == null)
            return;

        TAE vanilla;

        try
        {
            vanilla = TAE.Read(diff.Vanilla.GetBytes());
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(TimeActRevert), LOC.Get("TAE_Vanilla_Revert_Failed", entry.Name), e);
            return;
        }

        view.ViewportActionManager.ExecuteAction(new TimeActRevertTimeActAction(view, entry, vanilla));

        Smithbox.Log(typeof(TimeActRevert), LOC.Get("TAE_Vanilla_Reverted_TimeAct", entry.Name));
    }

    /// <summary>
    /// The game's animations that the time act no longer has
    /// </summary>
    public static List<TAE.Animation> GetRemovedAnimations(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (entry.TimeAct == null)
            return [];

        var diff = source.Vanilla.GetEntryDiff(entry, true);

        return diff.Status == TimeActVanillaStatus.Modified ? diff.RemovedAnimations : [];
    }

    /// <summary>
    /// Adds copies of the game's animations the time act no longer has, in ID order, as one step
    /// </summary>
    public static void RestoreRemovedAnimations(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry)
    {
        var removed = GetRemovedAnimations(source, entry).OrderBy(e => e.ID).ToList();

        if (removed.Count == 0)
            return;

        var bigEndian = entry.TimeAct.BigEndian;
        var ids = entry.TimeAct.Animations.Select(e => e.ID).OrderBy(e => e).ToList();

        var actions = new List<ViewportAction>();

        for (int i = 0; i < removed.Count; i++)
        {
            var vanilla = removed[i];

            var copy = TimeActEntry.CloneAnimation(vanilla, vanilla.ID, vanilla.AnimFileName, bigEndian, false);

            // The ones restored before this have lower IDs, so they are all in front of it
            var index = ids.Count(e => e < vanilla.ID) + i;

            actions.Add(new TimeActAddAnimationAction(view, entry, copy, index));
        }

        view.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(actions,
            LOC.Get("TAE_History_Restore_Animations", removed.Count)));

        Smithbox.Log(typeof(TimeActRevert), LOC.Get("TAE_Vanilla_Restored_Animations", removed.Count, entry.Name));
    }

    public static bool CanRevertAnimation(TimeActContainerWrapper source, TimeActEntry entry, TAE.Animation animation)
    {
        return source.Vanilla.GetAnimationDiff(entry, animation).Status == TimeActVanillaStatus.Modified;
    }

    public static void RevertAnimation(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry,
        TAE.Animation animation)
    {
        var diff = source.Vanilla.GetAnimationDiff(entry, animation);

        if (diff.Status != TimeActVanillaStatus.Modified || diff.Vanilla == null)
            return;

        view.ViewportActionManager.ExecuteAction(new TimeActRevertAnimationAction(view, entry, animation, diff.Vanilla));

        Smithbox.Log(typeof(TimeActRevert), LOC.Get("TAE_Vanilla_Reverted_Animation",
            source.GetAnimationName(entry, animation)));
    }

    /// <summary>
    /// The events among those given that the game has in a changed form, each with the game's version
    /// </summary>
    public static List<(TAE.Event Current, TAE.Event Vanilla)> GetRevertibleEvents(TimeActContainerWrapper source,
        TimeActEntry entry, TAE.Animation animation, IEnumerable<TAE.Event> events)
    {
        if (source == null || entry?.TimeAct == null || animation == null)
            return [];

        var match = source.Vanilla.GetEventMatch(entry, animation);

        if (match == null)
            return [];

        var result = new List<(TAE.Event, TAE.Event)>();

        foreach (var ev in events)
        {
            if (match.GetStatus(ev) == TimeActVanillaStatus.Modified)
            {
                result.Add((ev, match.Matches[ev]));
            }
        }

        return result;
    }

    public static void RevertEvents(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry,
        TAE.Animation animation, IEnumerable<TAE.Event> events)
    {
        var revertible = GetRevertibleEvents(source, entry, animation, events);

        if (revertible.Count == 0)
            return;

        var template = view.Project.Handler.TimeActData.GetTemplate(source);

        view.ViewportActionManager.ExecuteAction(new TimeActRevertEventsAction(entry, animation, revertible, template));

        Smithbox.Log(typeof(TimeActRevert), LOC.Get("TAE_Vanilla_Reverted_Events", revertible.Count,
            source.GetAnimationName(entry, animation)));
    }
}
