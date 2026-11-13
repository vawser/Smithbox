using StudioCore.Editors.Common;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Sets (or clears) the unsaved replacement of one sound. Before/after are whole <see cref="SoundMediaEdit"/>s (null =
/// no replacement), so undo restores exactly what was there, including an earlier unsaved replacement.
/// </summary>
public class ReplaceSoundAction : EditorAction
{
    private readonly SoundEdits Edits;
    private readonly SoundMediaKey Key;
    private readonly SoundMediaEdit Before, After;

    public ReplaceSoundAction(SoundEdits edits, SoundMediaKey key, SoundMediaEdit after)
    {
        Edits = edits;
        Key = key;
        Before = edits.Get(key);
        After = after;
    }

    public override ActionEvent Execute()
    {
        Edits.Set(Key, After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Edits.Set(Key, Before);
        return ActionEvent.NoEvent;
    }
}
