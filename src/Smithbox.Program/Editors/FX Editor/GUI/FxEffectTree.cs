using Hexa.NET.ImGui;
using SoulsFormats;

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
        DisplayNode(entry, fxr.Root, ref id, 0);

        ImGui.EndChild();
    }

    private void DisplayNode(FxEntry entry, FxrNode node, ref int id, int depth)
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

        bool open = ImGui.TreeNodeEx($"{label}##fxNode_{nodeId}", flags);
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Editor.Selection.SelectItem(node);

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
            DisplayNode(entry, child, ref id, depth + 1);

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
