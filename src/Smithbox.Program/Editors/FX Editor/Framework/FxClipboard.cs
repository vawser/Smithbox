using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The Effect Tree's clipboard: a node (with its branch) or an action, copied when taken so later edits to the
/// source don't change it. It remembers the game it came from, since action layouts differ per game, and the
/// state count of its effect, so state maps can be fitted when it's pasted into an effect with other states.
/// </summary>
public class FxClipboard
{
    public FxrNode Node { get; private set; }
    public FxrAction Action { get; private set; }

    public FxrGame Game { get; private set; }
    public int StateCount { get; private set; }

    /// <summary>Where the copy came from, for the menu ("f000123456").</summary>
    public string Source { get; private set; }

    public bool HasNode => Node != null;
    public bool HasAction => Action != null;

    public void CopyNode(FxrNode node, FxrGame game, int stateCount, string source)
    {
        Node = FxClone.Node(node);
        Action = null;
        Game = game;
        StateCount = stateCount;
        Source = source;
    }

    public void CopyAction(FxrAction action, FxrGame game, string source)
    {
        Action = FxClone.Action(action);
        Node = null;
        Game = game;
        Source = source;
    }

    /// <summary>A fresh copy of the clipboard node, to paste.</summary>
    public FxrNode TakeNode() => Node == null ? null : FxClone.Node(Node);
}
