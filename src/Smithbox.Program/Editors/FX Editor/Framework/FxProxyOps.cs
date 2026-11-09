using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Turning a Proxy node into the nodes of the effect it starts (Inline), and a branch into an effect of its own
/// started by a Proxy node (Extract). Only SoulsFormats is used, so the check tool compiles this file on its own.
///
/// What decides whether the result plays the same (from the vanilla survey, FX Effect Tree Actions Plan §E):
/// - A Proxy node holds only its SFXReference (action 132): no configs, no transform, so its effect runs at its
///   parent's place. Its children can move up into the parent unless the parent emits its children (NodeEmitter)
///   or picks one by distance (LevelsOfDetail).
/// - The started effect runs its own states; ~95% of vanilla targets have one state whose only condition ends the
///   effect on External Value 0 (Terminate), as their hosts do.
/// - The root's actions (termination mode, view distance, initial simulation time) are effect-wide settings.
/// </summary>
public static class FxProxyOps
{
    public const short ProxyType = 2001, LodType = 2002, EmitterType = 2202;
    public const short ProxyAction = 132;

    /// <summary>The effect id a Proxy node starts, or -1.</summary>
    public static int Target(FxrNode node) =>
        node.Type == ProxyType && node.Actions.FirstOrDefault(a => a.Type == ProxyAction) is { Fields1.Count: > 0 } a
            ? (int)a.Fields1[0] : -1;

    /// <summary>A Proxy node starting effect <paramref name="id"/>, laid out as vanilla stores them.</summary>
    public static FxrNode MakeProxy(int id) => new()
    {
        Type = ProxyType,
        Actions = new List<FxrAction>
        {
            new() { Type = ProxyAction, Unk02 = 1, Unk03 = 0, Unk04 = 0, Fields1 = new List<uint> { (uint)id } },
        },
    };

    /// <summary>Why a node can't have its children moved into <paramref name="parent"/>, or null when it can.</summary>
    public static string ParentRefusal(FxrNode parent) => parent?.Type switch
    {
        null => "root",
        EmitterType => "emitter",
        LodType => "lod",
        _ => null,
    };

    // ---------------------------------------------------------------- comparisons

    public static bool SameConditions(FxrState a, FxrState b) =>
        a.Conditions.Count == b.Conditions.Count && a.Conditions.Zip(b.Conditions).All(p =>
            p.First.Operator == p.Second.Operator && p.First.TargetState == p.Second.TargetState
            && p.First.Left.Type == p.Second.Left.Type && p.First.Left.RawValue == p.Second.Left.RawValue
            && p.First.Right.Type == p.Second.Right.Type && p.First.Right.RawValue == p.Second.Right.RawValue);

    public static bool SameActions(List<FxrAction> a, List<FxrAction> b) =>
        a.Count == b.Count && a.Zip(b).All(p => p.First.Type == p.Second.Type
            && p.First.Fields1.SequenceEqual(p.Second.Fields1) && p.First.Fields2.SequenceEqual(p.Second.Fields2)
            && p.First.Properties1.Count == p.Second.Properties1.Count && p.First.Properties2.Count == p.Second.Properties2.Count);

    // ---------------------------------------------------------------- inline

    /// <summary>What inlining would change in how the effect plays, as short reason codes (empty: plays the same).</summary>
    public static List<string> InlineDifferences(FXR host, FXR target)
    {
        var list = new List<string>();
        if (target.States.Count > 1)
            list.Add("target-states");
        if (target.States.Count > 0 && host.States.Count > 0 && !SameConditions(target.States[0], host.States[0]))
            list.Add("target-conditions");
        if (!SameActions(target.Root.Actions, host.Root.Actions))
            list.Add("root-actions");
        return list;
    }

    /// <summary>
    /// Copies of the target effect's root children, with every state map set so each of the host's states uses the
    /// config the node runs in the target's state 0 (the target's own state changes don't carry over).
    /// </summary>
    public static List<FxrNode> InlineNodes(FXR target, int hostStates)
    {
        var nodes = target.Root.Children.Select(FxClone.Node).ToList();
        foreach (var node in nodes)
            foreach (var n in FXR.Walk(node))
                foreach (var a in n.Actions)
                    if (a.Type == 199 && a.Section10s.Count > 0)
                    {
                        var map = a.Section10s[0];
                        int first = map.Count > 0 ? map[0] : 0;
                        map.Clear();
                        for (int i = 0; i < Math.Max(hostStates, 1); i++)
                            map.Add(first);
                    }
        return nodes;
    }

    // ---------------------------------------------------------------- extract

    /// <summary>
    /// A new effect holding a copy of <paramref name="node"/> as its only root child, with the host's states (so
    /// the node's state maps still mean the same states) and the host's root actions (the same effect-wide settings).
    /// </summary>
    public static FXR ExtractEffect(FXR host, FxrGame game, int id, FxrNode node)
    {
        var fxr = FXR.Create(game, id);
        fxr.States = host.States.Select(FxClone.State).ToList();
        if (fxr.States.Count == 0)
            fxr.States.Add(new FxrState());
        fxr.Root.Actions = host.Root.Actions.Select(FxClone.Action).ToList();
        fxr.Root.Children.Add(FxClone.Node(node));
        return fxr;
    }

    /// <summary>What extracting would change in how the effect plays, as reason codes (empty: plays the same).</summary>
    public static List<string> ExtractDifferences(FXR host, FxrNode parent)
    {
        // One node becomes one Proxy node, so any parent can hold it (vanilla has Proxy nodes under emitters too)
        var list = new List<string>();
        // The new effect's state time starts when the Proxy node first runs, which is at once only under the root
        if (host.States.Count > 1 && parent != host.Root)
            list.Add("state-timing");
        return list;
    }
}
