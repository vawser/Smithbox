using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Adds an animation to a time act, or removes one when it is told to delete instead
/// </summary>
public class TimeActAddAnimationAction : ViewportAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;

    private readonly bool IsDelete;

    private int Index;

    public TimeActAddAnimationAction(TimeActEditorView view, TimeActEntry entry, TAE.Animation animation, int index,
        bool isDelete = false)
    {
        View = view;
        Entry = entry;
        Animation = animation;
        IsDelete = isDelete;

        Index = isDelete ? entry.TimeAct.Animations.IndexOf(animation) : index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (IsDelete)
        {
            Remove();
        }
        else
        {
            Insert();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (IsDelete)
        {
            Insert();
        }
        else
        {
            Remove();
        }

        return ActionEvent.NoEvent;
    }

    private void Insert()
    {
        var animations = Entry.TimeAct.Animations;

        Index = Math.Clamp(Index, 0, animations.Count);

        animations.Insert(Index, Animation);

        Entry.IsDirty = true;
        Entry.MarkAnimationsChanged();

        if (View.Selection.SelectedTimeAct == Entry)
        {
            View.AnimationSelectionWindow.SelectAnimation(Animation);
        }
    }

    private void Remove()
    {
        var animations = Entry.TimeAct.Animations;

        Index = animations.IndexOf(Animation);

        if (Index < 0)
            return;

        animations.RemoveAt(Index);

        Entry.IsDirty = true;
        Entry.MarkAnimationsChanged();

        if (View.Selection.SelectedAnimation == Animation)
        {
            // The animation that took its place is selected, so the list can be deleted from one after another
            if (animations.Count > 0)
            {
                View.AnimationSelectionWindow.SelectAnimation(animations[Math.Min(Index, animations.Count - 1)]);
            }
            else
            {
                View.AnimationSelectionWindow.ClearSelection();
            }
        }
    }
}

/// <summary>
/// Moves an animation to another position in its time act
/// </summary>
public class TimeActMoveAnimationAction : ViewportAction
{
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;

    private readonly int OldIndex;
    private readonly int NewIndex;

    public TimeActMoveAnimationAction(TimeActEntry entry, TAE.Animation animation, int newIndex)
    {
        Entry = entry;
        Animation = animation;

        OldIndex = entry.TimeAct.Animations.IndexOf(animation);
        NewIndex = newIndex;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Move(NewIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Move(OldIndex);

        return ActionEvent.NoEvent;
    }

    private void Move(int index)
    {
        var animations = Entry.TimeAct.Animations;

        if (!animations.Remove(Animation))
            return;

        animations.Insert(Math.Clamp(index, 0, animations.Count), Animation);

        Entry.IsDirty = true;
        Entry.MarkAnimationsChanged();
    }
}

/// <summary>
/// Puts the animations of a time act in ID order, which is the order the game stores them in
/// </summary>
public class TimeActSortAnimationsAction : ViewportAction
{
    private readonly TimeActEntry Entry;

    private readonly List<TAE.Animation> OldOrder;
    private readonly List<TAE.Animation> NewOrder;

    public TimeActSortAnimationsAction(TimeActEntry entry)
    {
        Entry = entry;

        OldOrder = entry.TimeAct.Animations.ToList();
        NewOrder = OldOrder.OrderBy(e => e.ID).ToList();
    }

    /// <summary>
    /// Whether sorting would change anything
    /// </summary>
    public bool ChangesOrder => !OldOrder.SequenceEqual(NewOrder);

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(NewOrder);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(OldOrder);

        return ActionEvent.NoEvent;
    }

    private void Apply(List<TAE.Animation> order)
    {
        var animations = Entry.TimeAct.Animations;

        animations.Clear();
        animations.AddRange(order);

        Entry.IsDirty = true;
        Entry.MarkAnimationsChanged();
    }
}

/// <summary>
/// Imports a HKX file for an animation. It is held by the source until it is saved.
/// </summary>
public class TimeActImportAnimationFileAction : ViewportAction
{
    private readonly TimeActContainerWrapper Source;
    private readonly TimeActPendingAnimationFile File;

    // An earlier import of the same animation that this one replaces
    private readonly TimeActPendingAnimationFile Previous;

    public TimeActImportAnimationFileAction(TimeActContainerWrapper source, TimeActPendingAnimationFile file)
    {
        Source = source;
        File = file;

        source.PendingAnimationFiles.TryGetValue(file.ID, out Previous);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Source.PendingAnimationFiles[File.ID] = File;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Previous == null)
        {
            Source.PendingAnimationFiles.Remove(File.ID);
        }
        else
        {
            Source.PendingAnimationFiles[File.ID] = Previous;
        }

        return ActionEvent.NoEvent;
    }
}
