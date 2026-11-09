using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>Queries and fix-ups on an effect's node tree, used by the Effect Tree's actions.</summary>
public static class FxNodeOps
{
    public const short RootType = 2000, ProxyType = 2001, LodType = 2002, BasicType = 2200, EmitterType = 2202;
    public const short StateMapAction = 199, ProxyAction = 132;

    public static FxrNode FindParent(FxrNode root, FxrNode target)
    {
        foreach (var child in root.Children)
        {
            if (ReferenceEquals(child, target))
                return root;
            if (FindParent(child, target) is { } found)
                return found;
        }
        return null;
    }

    /// <summary>Whether <paramref name="node"/> is <paramref name="ancestor"/> or under it.</summary>
    public static bool IsInBranch(FxrNode ancestor, FxrNode node)
    {
        if (ReferenceEquals(ancestor, node))
            return true;
        return ancestor.Children.Any(c => IsInBranch(c, node));
    }

    /// <summary>Whether a tree item (state, node, config or action) is still in the effect under this node.</summary>
    public static bool Contains(FxrNode node, object item)
    {
        if (item is FxrState)
            return true;
        if (ReferenceEquals(node, item))
            return true;
        if (node.Actions.Any(a => ReferenceEquals(a, item)))
            return true;
        foreach (var config in node.Configs)
            if (ReferenceEquals(config, item) || config.Actions.Any(a => ReferenceEquals(a, item)))
                return true;
        return node.Children.Any(c => Contains(c, item));
    }

    /// <summary>The node a tree item belongs to: the node itself, or the node owning the config or action.</summary>
    public static FxrNode OwnerNode(FxrNode root, object item)
    {
        if (item is FxrNode n)
            return n;
        foreach (var node in FXR.Walk(root))
        {
            if (item is FxrConfig c && node.Configs.Contains(c))
                return node;
            if (item is FxrAction a && (node.Actions.Contains(a) || node.Configs.Any(cfg => cfg.Actions.Contains(a))))
                return node;
        }
        return null;
    }

    /// <summary>The list an action sits in (a node's or a config's actions).</summary>
    public static List<FxrAction> OwnerList(FxrNode root, FxrAction action)
    {
        foreach (var node in FXR.Walk(root))
        {
            if (node.Actions.Contains(action))
                return node.Actions;
            foreach (var c in node.Configs)
                if (c.Actions.Contains(action))
                    return c.Actions;
        }
        return null;
    }

    /// <summary>Child indexes from the root, e.g. "0/2/1"; "" for the root, null if the node isn't in the tree.</summary>
    public static string PathOf(FxrNode root, FxrNode node)
    {
        if (ReferenceEquals(root, node))
            return "";
        for (int i = 0; i < root.Children.Count; i++)
        {
            var sub = PathOf(root.Children[i], node);
            if (sub != null)
                return sub.Length == 0 ? i.ToString() : $"{i}/{sub}";
        }
        return null;
    }

    public static FxrNode FromPath(FxrNode root, string path)
    {
        if (string.IsNullOrEmpty(path))
            return root;
        var node = root;
        foreach (var part in path.Split('/'))
        {
            if (!int.TryParse(part, out var i) || i < 0 || i >= node.Children.Count)
                return null;
            node = node.Children[i];
        }
        return node;
    }

    // ---------------------------------------------------------------- state maps

    public static FxrAction StateMap(FxrNode node) => node.Actions.FirstOrDefault(a => a.Type == StateMapAction);

    /// <summary>
    /// Fits every StateConfigMap in the branch to <paramref name="stateCount"/> entries (cut, or padded with the
    /// first entry, which is what a missing entry reads as). Returns how many maps changed.
    /// </summary>
    public static int FitStateMaps(FxrNode branch, int stateCount)
    {
        int changed = 0;
        foreach (var node in FXR.Walk(branch))
        {
            var map = StateMap(node);
            if (map == null || map.Section10s.Count == 0)
                continue;
            var list = map.Section10s[0];
            if (list.Count == stateCount || stateCount <= 0)
                continue;
            int pad = list.Count > 0 ? list[0] : 0;
            if (list.Count > stateCount)
                list.RemoveRange(stateCount, list.Count - stateCount);
            while (list.Count < stateCount)
                list.Add(pad);
            changed++;
        }
        return changed;
    }

    // ---------------------------------------------------------------- labels

    /// <summary>
    /// A short summary of what the node does, from its actions: the proxied effect, or the appearance (with its
    /// texture or model id) and emitter of the config shown for the preview state. Null when there's nothing to say.
    /// </summary>
    public static string Summary(FxData data, FxEntry entry, FxrNode node, int previewState)
    {
        if (node.Type == ProxyType)
            return node.Actions.FirstOrDefault(a => a.Type == ProxyAction) is { Fields1.Count: > 0 } proxy
                ? $"f{(int)proxy.Fields1[0]:D9}" : null;

        if (node.Configs.Count == 0)
            return null;

        int index = FxEffectDef.ConfigIndexFor(FxNames.StateConfigMap(node), node.Configs.Count, previewState);
        var config = node.Configs[index < 0 ? 0 : index];

        string appearance = null, emitter = null;
        foreach (var action in config.Actions)
        {
            var view = data.View(entry, action);
            var def = view.Definition;
            if (def == null)
                continue;

            if (def.IsAppearance && appearance == null)
            {
                appearance = def.Name;
                var res = Resource(view);
                if (res != null)
                    appearance = $"{appearance} {res}";
            }
            else if (emitter == null && def.Slot == "Emitter")
            {
                emitter = def.Name;
            }
        }

        var parts = new[] { appearance, emitter }.Where(p => p != null).ToList();
        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    /// <summary>The first texture or model id an action stores, as "s12345" / "model s12345".</summary>
    public static string Resource(FxrActionView view)
    {
        foreach (var r in Resources(view))
            return r.Kind == "model" ? $"model s{r.Id:D5}" : $"s{r.Id:D5}";
        return null;
    }

    /// <summary>Textures and models an action stores directly (as FxrResources.Collect, for one action).</summary>
    public static IEnumerable<(string Kind, int Id, string TextureType)> Resources(FxrActionView view)
    {
        if (!view.IsTyped)
            yield break;
        foreach (var name in view.Names)
        {
            var def = view.Describe(name);
            if (def?.Resource is not ("texture" or "model") || !view.IsStored(name))
                continue;

            int id;
            switch (view.KindOf(name))
            {
                case FxrValueKind.Field:
                    id = view.GetInt(name);
                    break;
                case FxrValueKind.Property:
                    var curve = FxrCurve.Decode(view.GetProperty(name));
                    if (curve.Constant == null)
                        continue;
                    id = (int)Math.Round(curve.Constant[0]);
                    break;
                default:
                    continue;
            }
            if (id > 0)
                yield return (def.Resource, id, def.TextureType);
        }
    }

    /// <summary>Every action of the node and its configs.</summary>
    public static IEnumerable<FxrAction> OwnActions(FxrNode node) => node.Actions.Concat(node.Configs.SelectMany(c => c.Actions));

    // ---------------------------------------------------------------- checks

    /// <summary>What looks wrong with the node on its own, for the tree's warning icon. Empty when nothing does.</summary>
    public static List<string> Check(FxEditorScreen editor, FxEntry entry, FxrNode node)
    {
        var problems = new List<string>();
        var fxr = entry.Fxr;

        if (node.Type == ProxyType)
        {
            if (node.Actions.FirstOrDefault(a => a.Type == ProxyAction) is { Fields1.Count: > 0 } proxy)
            {
                int id = (int)proxy.Fields1[0];
                if (id == entry.Id)
                    problems.Add(LOC.Get("FX_Check_Proxy_Self"));
                else if (editor.Data.Bank.IsIndexed && !editor.Data.Bank.Entries.ContainsKey(id))
                    problems.Add(LOC.Get("FX_Check_Proxy_Missing", $"f{id:D9}"));
            }
            else
            {
                problems.Add(LOC.Get("FX_Check_Proxy_No_Id"));
            }
        }
        else if (node.Type != RootType && node.Configs.Count == 0)
        {
            problems.Add(LOC.Get("FX_Check_No_Configs"));
        }

        var map = FxNames.StateConfigMap(node);
        if (map != null && node.Configs.Count > 0)
        {
            if (map.Any(i => i < 0 || i >= node.Configs.Count))
                problems.Add(LOC.Get("FX_Check_Map_Range", node.Configs.Count));
            if (map.Count != fxr.States.Count)
                problems.Add(LOC.Get("FX_Check_Map_Count", map.Count, fxr.States.Count));
        }

        var missing = editor.ViewportWindow?.Textures?.MissingTextures;
        if (missing is { Count: > 0 })
        {
            foreach (var action in OwnActions(node))
                foreach (var r in Resources(editor.Data.View(entry, action)))
                {
                    var name = $"s{r.Id:D5}";
                    if (r.Kind == "texture" && missing.Any(m => m.Equals(name, StringComparison.OrdinalIgnoreCase)
                        || m.StartsWith(name + "_", StringComparison.OrdinalIgnoreCase) || m.StartsWith(name + ".", StringComparison.OrdinalIgnoreCase)))
                        problems.Add(LOC.Get("FX_Check_Texture_Missing", name));
                }
        }

        return problems.Distinct().ToList();
    }
}
