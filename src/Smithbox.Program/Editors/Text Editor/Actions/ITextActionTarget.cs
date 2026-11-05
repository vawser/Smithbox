using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// A text action that can say what it changed, so undo/redo can select the changed
/// entry. Undo history covers every FMG in the view, and undoing an edit in a file
/// you had navigated away from used to change text with no visible sign.
/// </summary>
public interface ITextActionTarget
{
    TextContainerWrapper TargetContainer { get; }
    FMG TargetFmg { get; }

    /// <summary>
    /// The changed entry. May not be in TargetFmg (e.g. after a delete is redone).
    /// </summary>
    FMG.Entry TargetEntry { get; }
}

public static class TextActionTargetUtils
{
    /// <summary>
    /// The first target in an action, looking inside grouped actions.
    /// </summary>
    public static ITextActionTarget GetFirstTarget(EditorAction action)
    {
        if (action is ITextActionTarget target)
            return target;

        if (action is FmgGroupedAction grouped)
        {
            foreach (var child in grouped.GetActions())
            {
                var childTarget = GetFirstTarget(child);

                if (childTarget != null)
                    return childTarget;
            }
        }

        return null;
    }
}
