using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Puts events back as the game has them: their times, Unk04 and parameters. Their group is left alone, as that
/// belongs to the animation. Each event keeps its place, so the selection and history still point at it.
/// </summary>
public class TimeActRevertEventsAction : TimeActAction
{
    private record EventState(float Start, float End, int Unk04, byte[] Bytes, TAE.Template.EventTemplate Template);

    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;
    private readonly TAE.Template.BankTemplate Template;
    private readonly bool BigEndian;

    private readonly List<(TAE.Event Event, EventState Old, EventState New)> Changes = new();

    public TimeActRevertEventsAction(TimeActEntry entry, TAE.Animation animation,
        IEnumerable<(TAE.Event Current, TAE.Event Vanilla)> events, TAE.Template.BankTemplate template)
    {
        Entry = entry;
        Animation = animation;
        Template = template;
        BigEndian = entry.TimeAct.BigEndian;

        foreach (var (current, vanilla) in events)
        {
            var old = new EventState(current.StartTime, current.EndTime, current.Unk04,
                current.GetParameterBytes(BigEndian).ToArray(), current.Template);

            var reverted = new EventState(vanilla.StartTime, vanilla.EndTime, vanilla.Unk04,
                vanilla.GetParameterBytes(BigEndian).ToArray(), null);

            Changes.Add((current, old, reverted));
        }
    }

    public int Count => Changes.Count;

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;
    public override TAE.Event TargetEvent => Changes.Count > 0 ? Changes[0].Event : null;

    public override string Describe() => Changes.Count == 1
        ? LOC.Get("TAE_History_Revert_Event", NameEvent(Changes[0].Event))
        : LOC.Get("TAE_History_Revert_Events", Changes.Count);

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var (ev, _, reverted) in Changes)
        {
            Apply(ev, reverted);

            // The template goes back on only if it fits the game's bytes exactly, as when the time act is read
            if (Template != null && Template.TryGetValue(ev.Type, out var eventTemplate))
            {
                Entry.TryApplyTemplateExactly(ev, eventTemplate);
            }
        }

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (ev, old, _) in Changes)
        {
            Apply(ev, old);

            if (old.Template != null)
            {
                ev.ApplyTemplate(BigEndian, old.Template, true);
            }
        }

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    private static void Apply(TAE.Event ev, EventState state)
    {
        ev.StartTime = state.Start;
        ev.EndTime = state.End;
        ev.Unk04 = state.Unk04;

        ev.RemoveTemplate(state.Bytes.ToArray());
    }
}

/// <summary>
/// Puts an animation back as the game has it: its mini header, file name, events and groups. The animation itself
/// stays in the list, so the selection, graph view and history still point at it.
/// </summary>
public class TimeActRevertAnimationAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;

    private readonly (TAE.Animation.AnimMiniHeader Header, string FileName, List<TAE.Event> Events, List<TAE.EventGroup> Groups) Old;
    private readonly (TAE.Animation.AnimMiniHeader Header, string FileName, List<TAE.Event> Events, List<TAE.EventGroup> Groups) New;

    public TimeActRevertAnimationAction(TimeActEditorView view, TimeActEntry entry, TAE.Animation animation,
        TAE.Animation vanilla)
    {
        View = view;
        Entry = entry;
        Animation = animation;

        Old = (animation.MiniHeader, animation.AnimFileName, animation.Events, animation.EventGroups);

        var copy = TimeActEntry.CloneAnimation(vanilla, animation.ID, vanilla.AnimFileName, entry.TimeAct.BigEndian, false);

        New = (copy.MiniHeader, copy.AnimFileName, copy.Events, copy.EventGroups);
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;

    public override string Describe() => LOC.Get("TAE_History_Revert_Animation");

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(New);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(Old);

        return ActionEvent.NoEvent;
    }

    private void Apply((TAE.Animation.AnimMiniHeader Header, string FileName, List<TAE.Event> Events, List<TAE.EventGroup> Groups) state)
    {
        Animation.MiniHeader = state.Header;
        Animation.AnimFileName = state.FileName;
        Animation.Events = state.Events;
        Animation.EventGroups = state.Groups;

        // The game's events come without templates, which are applied once the animation is selected again
        Entry.ForgetTemplate(Animation);
        Entry.IsDirty = true;

        if (View.Selection.SelectedAnimation == Animation)
        {
            View.AnimationSelectionWindow.SelectAnimation(Animation);
        }
    }
}

/// <summary>
/// Puts a time act back as the game has it, read again from the game's bytes. The entry stays in the list. The
/// animation that was selected stays selected if the game has one of its ID.
/// </summary>
public class TimeActRevertTimeActAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActEntry Entry;

    private readonly TAE OldTimeAct;
    private readonly TAE NewTimeAct;

    public TimeActRevertTimeActAction(TimeActEditorView view, TimeActEntry entry, TAE vanilla)
    {
        View = view;
        Entry = entry;

        OldTimeAct = entry.TimeAct;
        NewTimeAct = vanilla;
    }

    public override TimeActEntry TargetEntry => Entry;

    public override string Describe() => LOC.Get("TAE_History_Revert_TimeAct");

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(NewTimeAct);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(OldTimeAct);

        return ActionEvent.NoEvent;
    }

    private void Apply(TAE timeAct)
    {
        var selected = View.Selection.SelectedTimeAct == Entry ? View.Selection.SelectedAnimation : null;

        Entry.ReplaceTimeAct(timeAct);

        if (View.Selection.SelectedTimeAct != Entry)
            return;

        var animation = selected != null ? timeAct.Animations.FirstOrDefault(e => e.ID == selected.ID) : null;

        if (animation != null)
        {
            View.AnimationSelectionWindow.SelectAnimation(animation);
        }
        else
        {
            View.AnimationSelectionWindow.ClearSelection();
        }
    }
}
