using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Adds a config to a node, or removes one, keeping the node's StateConfigMap pointing at the same configs: a
/// removed config's states go to config 0 and later indexes move down by one. The map before is stored for undo.
/// </summary>
public class FxConfigListAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrNode Node;
    private readonly FxrConfig Config;
    private readonly int Index;
    private readonly bool Insert;
    private readonly List<int> OldMap;

    private FxConfigListAction(FxBank bank, FxEntry entry, FxrNode node, FxrConfig config, int index, bool insert)
    {
        Bank = bank;
        Entry = entry;
        Node = node;
        Config = config;
        Index = index;
        Insert = insert;
        OldMap = FxNames.StateConfigMap(node)?.ToList();
    }

    /// <summary>Adds the config at the end of the node's configs; no state uses it until one is assigned.</summary>
    public static FxConfigListAction Add(FxBank bank, FxEntry entry, FxrNode node, FxrConfig config)
        => new(bank, entry, node, config, node.Configs.Count, true);

    public static FxConfigListAction Remove(FxBank bank, FxEntry entry, FxrNode node, FxrConfig config)
        => new(bank, entry, node, config, node.Configs.IndexOf(config), false);

    public override ActionEvent Execute()
    {
        if (Insert)
        {
            Node.Configs.Insert(Math.Clamp(Index, 0, Node.Configs.Count), Config);
        }
        else
        {
            Node.Configs.Remove(Config);
            var map = FxNames.StateConfigMap(Node);
            if (map != null)
                for (int i = 0; i < map.Count; i++)
                    map[i] = map[i] == Index ? 0 : map[i] > Index ? map[i] - 1 : map[i];
        }
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Insert)
            Node.Configs.Remove(Config);
        else
            Node.Configs.Insert(Math.Clamp(Index, 0, Node.Configs.Count), Config);

        var map = FxNames.StateConfigMap(Node);
        if (map != null && OldMap != null)
        {
            map.Clear();
            map.AddRange(OldMap);
        }
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }
}
