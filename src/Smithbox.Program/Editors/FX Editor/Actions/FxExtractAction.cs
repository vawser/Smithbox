using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Extract to New Effect: the node is replaced by a Proxy node starting a new effect that holds the node. The new
/// effect joins the bank (it's written into the host's binders on save). Undo puts the node back and, while the new
/// effect hasn't been saved, takes it out of the bank again; once saved its file stays.
/// </summary>
public class FxExtractAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Host;
    private readonly FxEntry Created;
    private readonly FxrNode Parent;
    private readonly FxrNode Node;
    private readonly FxrNode Proxy;

    public FxExtractAction(FxBank bank, FxEntry host, FxEntry created, FxrNode parent, FxrNode node, FxrNode proxy)
    {
        Bank = bank;
        Host = host;
        Created = created;
        Parent = parent;
        Node = node;
        Proxy = proxy;
    }

    public override ActionEvent Execute()
    {
        int index = Parent.Children.IndexOf(Node);
        if (index >= 0)
            Parent.Children[index] = Proxy;
        Bank.AddNew(Created);
        Bank.MarkDirty(Host);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        int index = Parent.Children.IndexOf(Proxy);
        if (index >= 0)
            Parent.Children[index] = Node;
        if (Created.IsNew)
            Bank.RemoveNew(Created);
        Bank.MarkDirty(Host);
        return ActionEvent.NoEvent;
    }
}
