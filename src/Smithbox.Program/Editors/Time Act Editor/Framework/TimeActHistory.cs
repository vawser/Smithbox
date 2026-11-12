using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The undo history of a view, which outlives switching sources. Before a step is undone or redone, the view goes
/// to what it changes (its source, time act and animation), so the change happens in view. Going there first also
/// lets the step's own selection handling work, as it only selects within the animation on show.
/// </summary>
public class TimeActHistory
{
    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    public TimeActHistory(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ViewportActionManager Manager => View.ViewportActionManager;

    public void Undo()
    {
        if (Manager.PeekUndoAction() is not { } action)
            return;

        GoTo(action);

        Manager.UndoAction();
    }

    public void Redo()
    {
        if (Manager.PeekRedoAction() is not { } action)
            return;

        GoTo(action);

        Manager.RedoAction();
    }

    /// <summary>
    /// Undoes everything, going to the oldest step before it is undone
    /// </summary>
    public void UndoAll()
    {
        var undo = Manager.GetUndoActions();

        if (undo.Count == 0)
            return;

        UndoTo(undo[^1]);
    }

    /// <summary>
    /// Undoes the steps after the given one, and then it, so the history is back to before it was made
    /// </summary>
    public void UndoTo(ViewportAction target)
    {
        if (!Manager.GetUndoActions().Contains(target))
            return;

        while (Manager.PeekUndoAction() is { } action && action != target)
        {
            Manager.UndoAction();
        }

        Undo();
    }

    /// <summary>
    /// Redoes the steps up to and including the given one
    /// </summary>
    public void RedoTo(ViewportAction target)
    {
        if (!Manager.GetRedoActions().Contains(target))
            return;

        while (Manager.PeekRedoAction() is { } action && action != target)
        {
            Manager.RedoAction();
        }

        Redo();
    }

    /// <summary>
    /// The source a step changes: the one it names, or else the source whose list holds its time act
    /// </summary>
    public TimeActContainerWrapper FindSource(ITimeActHistoryItem item)
    {
        if (item.TargetSource != null)
            return item.TargetSource;

        if (item.TargetEntry == null)
            return null;

        var bank = Project.Handler.TimeActData?.PrimaryBank;

        return bank?.GetCharacters().Concat(bank.GetAssets()).FirstOrDefault(e => e.TimeActs.Contains(item.TargetEntry));
    }

    /// <summary>
    /// The animation a step changes: the one it names, or else the one holding its event
    /// </summary>
    public static TAE.Animation FindAnimation(ITimeActHistoryItem item)
    {
        if (item.TargetAnimation != null)
            return item.TargetAnimation;

        if (item.TargetEvent == null || item.TargetEntry?.TimeAct == null)
            return null;

        return item.TargetEntry.TimeAct.Animations.FirstOrDefault(e => e.Events.Contains(item.TargetEvent));
    }

    /// <summary>
    /// Opens what a step changes, as far as it still exists: an undo of a delete goes to the time act that held the
    /// deleted animation, as the animation only comes back with the undo
    /// </summary>
    public void GoTo(ViewportAction action)
    {
        if (action is not ITimeActHistoryItem item)
            return;

        var source = FindSource(item);

        if (source == null)
            return;

        var view = View;

        if (view.Selection.SelectedTimeActContainerWrapper != source)
        {
            // The source may be open in another view, which then shows the change
            view = view.SourceWindow.SelectSource(source);
            view.SourceWindow.ScrollTo(source, source.IsCharacter ? ModelListType.Character : ModelListType.Asset);
        }

        var entry = item.TargetEntry;

        if (entry == null || !source.TimeActs.Contains(entry))
            return;

        var selection = view.Selection;

        if (selection.SelectedTimeAct != entry)
        {
            view.SelectionWindow.SelectTimeAct(entry);
            view.SelectionWindow.ScrollTo(entry);
        }

        var animation = FindAnimation(item);

        if (animation == null || entry.TimeAct?.Animations.Contains(animation) != true)
            return;

        if (selection.SelectedAnimation != animation)
        {
            view.AnimationSelectionWindow.SelectAnimation(animation);
        }

        view.AnimationSelectionWindow.ScrollTo(animation);

        // The event the step changes is brought forward in the properties, if it is there to select
        if (item.TargetEvent is { } ev && animation.Events.Contains(ev) && !selection.IsEventSelected(ev))
        {
            selection.SelectedEvent = ev;
        }
    }
}
