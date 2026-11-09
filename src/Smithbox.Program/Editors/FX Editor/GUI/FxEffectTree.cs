using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The selected effect's structure: its states (with their conditions), then the node tree. Each node shows
/// its own actions and its configs (labelled with the states that use them), and each config its actions.
/// </summary>
public class FxEffectTree
{
    private readonly FxEditorScreen Editor;

    public FxEffectTree(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        GUI.TitleHeader(LOC.Get("FX_EffectTree_Header"), LOC.Get("FX_EffectTree_Header_TT"));

        ImGui.BeginChild("FxEffectTreeSection", ImGuiChildFlags.Borders);

        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null)
        {
            GUI.WrappedText(LOC.Get("FX_EffectTree_No_Effect"));
            ImGui.EndChild();
            return;
        }

        var fxr = entry.Fxr;

        // States
        if (ImGui.TreeNodeEx($"{LOC.Get("FX_EffectTree_States", fxr.States.Count)}##fxStates", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < fxr.States.Count; i++)
            {
                var state = fxr.States[i];
                var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow;
                if (state.Conditions.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
                if (Editor.Selection.IsSelected(state)) flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx($"{LOC.Get("FX_EffectTree_State", i)}##fxState_{i}", flags);
                if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                    Editor.Selection.SelectItem(state);
                if (open)
                {
                    foreach (var c in state.Conditions)
                        GUI.WrappedText(FxNames.Condition(c));
                    ImGui.TreePop();
                }
            }
            ImGui.TreePop();
        }

        // Nodes
        int id = 0;
        DisplayNode(entry, fxr.Root, null, ref id, 0, false);

        ImGui.EndChild();

        // Structure changes wait until the tree is drawn, so the lists aren't changed while they're walked
        if (PendingDuplicate != null)
            DuplicateNode(PendingDuplicate);
        if (PendingDelete != null)
            DeleteNode(PendingDelete);
        PendingDuplicate = PendingDelete = null;
    }

    // ---------------------------------------------------------------- node actions

    private FxrNode PendingDuplicate, PendingDelete;

    /// <summary>The selected node, when it can be duplicated or deleted (any node but the root).</summary>
    public FxrNode EditableNode
    {
        get
        {
            var fxr = Editor.Selection.Entry?.Fxr;
            return Editor.Selection.Item is FxrNode node && fxr != null && node != fxr.Root && FindParent(fxr.Root, node) != null
                ? node : null;
        }
    }

    /// <summary>The Duplicate and Delete shortcuts, while the Effect Tree has focus.</summary>
    public void Shortcuts()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.FxEditor_EffectTree) || EditableNode is not { } node)
            return;

        if (InputManager.IsPressed(KeybindID.Duplicate))
            PendingDuplicate = node;
        else if (InputManager.IsPressed(KeybindID.Delete))
            PendingDelete = node;
    }

    /// <summary>Puts a copy of the node (its configs, actions and children) just after it, and selects the copy.</summary>
    public void DuplicateNode(FxrNode node)
    {
        var entry = Editor.Selection.Entry;
        var parent = entry?.Fxr == null ? null : FindParent(entry.Fxr.Root, node);
        if (parent == null)
            return;

        FlushPending();
        var copy = FxClone.Node(node);
        Editor.ActionManager.ExecuteAction(FxNodeListAction.Add(Editor.Data.Bank, entry, parent, copy, parent.Children.IndexOf(node) + 1));
        Editor.Selection.SelectItem(copy);
    }

    /// <summary>Takes the node, and everything under it, out of its parent.</summary>
    public void DeleteNode(FxrNode node)
    {
        var entry = Editor.Selection.Entry;
        var parent = entry?.Fxr == null ? null : FindParent(entry.Fxr.Root, node);
        if (parent == null)
            return;

        FlushPending();
        Editor.ActionManager.ExecuteAction(FxNodeListAction.Remove(Editor.Data.Bank, entry, parent, node));

        // The selection may have been inside what went
        if (Editor.Selection.Item != null && !Contains(entry.Fxr.Root, Editor.Selection.Item))
            Editor.Selection.ClearItem();
    }

    /// <summary>A value still being typed belongs to the tree as it was, so it's committed first.</summary>
    private void FlushPending()
    {
        Editor.Inspector.FlushPending();
        Editor.CurveEditor.FlushPending();
    }

    private static FxrNode FindParent(FxrNode root, FxrNode target)
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

    /// <summary>Whether a tree item (state, node, config or action) is still in the effect under this node.</summary>
    private static bool Contains(FxrNode node, object item)
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

    // ---------------------------------------------------------------- display

    /// <summary>The eye before each node: hides the node, and everything under it, in playback.</summary>
    private void DisplayVisibilityButton(FxrNode node, int nodeId)
    {
        var playback = Editor.Playback;
        bool hidden = playback.IsNodeHidden(node);

        ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);

        if (ImGui.Button($"{(hidden ? Icons.EyeSlash : Icons.Eye)}##fxNodeVisibility_{nodeId}", DPI.InlineIconButtonSize))
            playback.SetNodeHidden(node, !hidden);

        ImGui.PopStyleColor(4);
        ImGui.PopItemFlag();
        GUI.Tooltip(LOC.Get("FX_EffectTree_Visibility_TT"));
        ImGui.SameLine();
    }

    private void DisplayNodeContextMenu(FxrNode node, FxrNode parent, int nodeId)
    {
        if (!ImGui.BeginPopupContextItem($"##fxNodeContext_{nodeId}"))
            return;

        // The context menu acts on the node it was opened on
        Editor.Selection.SelectItem(node);

        bool editable = parent != null;

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Duplicate")}##fxNodeDuplicate", InputManager.GetHint(KeybindID.Duplicate), false, editable))
            PendingDuplicate = node;
        GUI.Tooltip(LOC.Get("FX_EffectTree_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Delete")}##fxNodeDelete", InputManager.GetHint(KeybindID.Delete), false, editable))
            PendingDelete = node;
        GUI.Tooltip(LOC.Get("FX_EffectTree_Delete_TT"));

        ImGui.Separator();

        bool hidden = Editor.Playback.IsNodeHidden(node);
        if (ImGui.MenuItem($"{LOC.Get(hidden ? "FX_EffectTree_Show" : "FX_EffectTree_Hide")}##fxNodeVisibility"))
            Editor.Playback.SetNodeHidden(node, !hidden);

        ImGui.EndPopup();
    }

    private void DisplayNode(FxEntry entry, FxrNode node, FxrNode parent, ref int id, int depth, bool parentHidden)
    {
        int nodeId = id++;
        var label = FxNames.NodeType(node.Type);

        // Proxy nodes name the effect they start
        if (node.Type == 2001 && node.Actions.FirstOrDefault(a => a.Type == 132) is { Fields1.Count: > 0 } proxy)
            label = $"{label} → f{(int)proxy.Fields1[0]:D9}";
        if (node.Children.Count > 0)
            label = $"{label} ({node.Children.Count})";

        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
        if (depth == 0) flags |= ImGuiTreeNodeFlags.DefaultOpen;
        if (Editor.Selection.IsSelected(node)) flags |= ImGuiTreeNodeFlags.Selected;

        DisplayVisibilityButton(node, nodeId);

        // Hidden nodes, and nodes under a hidden one, are dimmed
        bool hidden = parentHidden || Editor.Playback.IsNodeHidden(node);
        if (hidden)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        bool open = ImGui.TreeNodeEx($"{label}##fxNode_{nodeId}", flags);

        if (hidden)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Editor.Selection.SelectItem(node);

        DisplayNodeContextMenu(node, parent, nodeId);

        if (!open)
            return;

        // The node's own actions
        foreach (var action in node.Actions)
            DisplayAction(entry, action, ref id);

        // Configs, with the states that use them
        var map = FxNames.StateConfigMap(node);
        for (int c = 0; c < node.Configs.Count; c++)
        {
            var config = node.Configs[c];
            var cfgLabel = FxNames.ConfigType(config.Type);
            if (map != null)
            {
                var states = Enumerable.Range(0, map.Count).Where(s => map[s] == c).ToList();
                cfgLabel = states.Count > 0
                    ? $"{cfgLabel} {LOC.Get("FX_EffectTree_Config_States", string.Join(", ", states))}"
                    : $"{cfgLabel} {LOC.Get("FX_EffectTree_Config_Unused")}";
            }

            var cflags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen;
            if (Editor.Selection.IsSelected(config)) cflags |= ImGuiTreeNodeFlags.Selected;

            int configId = id++;
            bool copen = ImGui.TreeNodeEx($"{cfgLabel}##fxConfig_{configId}", cflags);
            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                Editor.Selection.SelectItem(config);
            if (copen)
            {
                foreach (var action in config.Actions)
                    DisplayAction(entry, action, ref id);
                ImGui.TreePop();
            }
        }

        // Child nodes
        foreach (var child in node.Children)
            DisplayNode(entry, child, node, ref id, depth + 1, hidden);

        ImGui.TreePop();
    }

    private void DisplayAction(FxEntry entry, FxrAction action, ref int id)
    {
        // Empty slots (type 0) aren't worth a row
        if (action.Type == 0 && action.Fields1.Count + action.Fields2.Count + action.Properties1.Count + action.Properties2.Count == 0)
            return;

        int actionId = id++;
        var flags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (Editor.Selection.IsSelected(action)) flags |= ImGuiTreeNodeFlags.Selected;

        ImGui.TreeNodeEx($"{FxNames.Action(Editor.Data, entry, action)}##fxAction_{actionId}", flags);
        if (ImGui.IsItemClicked())
            Editor.Selection.SelectItem(action);
    }
}
