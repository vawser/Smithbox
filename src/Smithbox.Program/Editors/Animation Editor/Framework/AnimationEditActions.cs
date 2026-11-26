#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Gives a clip a new working version (an operation's result, or null to go back to its file). Undo puts back the
/// version it had, so each operation is one step.
/// </summary>
public sealed class AnimationClipEditAction : EditorAction
{
    private readonly AnimationEditorBank Bank;
    private readonly AnimationClipSource Source;
    private readonly EditableClip? Before;
    private readonly EditableClip? After;
    private readonly System.Action? Changed;

    public AnimationClipEditAction(AnimationEditorBank bank, AnimationClipSource source, EditableClip? after,
        string displayName, System.Action? changed = null)
    {
        Bank = bank;
        Source = source;
        Before = bank.IsEdited(source) ? bank.Get(source) : null;
        After = after;
        Changed = changed;
        DisplayName = displayName;
    }

    public override ActionEvent Execute()
    {
        Bank.SetEdited(Source, After);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.SetEdited(Source, Before);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}

/// <summary>Adds a clip under a new ID (Save as New ID before saving), with its first version</summary>
public sealed class AnimationAddClipAction : EditorAction
{
    private readonly AnimationEditorBank Bank;
    private readonly AnimationClipSource Source;
    private readonly EditableClip Clip;
    private readonly System.Action? Changed;

    public AnimationAddClipAction(AnimationEditorBank bank, AnimationClipSource source, EditableClip clip,
        string displayName, System.Action? changed = null)
    {
        Bank = bank;
        Source = source;
        Clip = clip;
        Changed = changed;
        DisplayName = displayName;
    }

    public override ActionEvent Execute()
    {
        Bank.SetClipListed(Source, true);
        Bank.SetEdited(Source, Clip);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.SetClipListed(Source, false);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Deletes a clip: a saved clip's file is taken out of its anibnd on save, a new clip is dropped. Undo lists it again,
/// with the edit it had.
/// </summary>
public sealed class AnimationDeleteClipAction : EditorAction
{
    private readonly AnimationEditorBank Bank;
    private readonly AnimationClipSource Source;
    private readonly EditableClip? Edit;
    private readonly System.Action? Changed;

    public AnimationDeleteClipAction(AnimationEditorBank bank, AnimationClipSource source, string displayName,
        System.Action? changed = null)
    {
        Bank = bank;
        Source = source;
        Edit = bank.IsEdited(source) ? bank.Get(source) : null;
        Changed = changed;
        DisplayName = displayName;
    }

    public override ActionEvent Execute()
    {
        Bank.SetClipDeleted(Source, true);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.SetClipDeleted(Source, false);
        Bank.SetEdited(Source, Edit);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}
