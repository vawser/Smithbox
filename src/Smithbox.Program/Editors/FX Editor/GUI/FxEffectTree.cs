using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The selected effect's structure: its states (with their conditions), then the node tree. Each node shows
/// its own actions and its configs (labelled with the states that use them), and each config its actions.
///
/// Nodes can be hidden in playback, soloed, copied and pasted (across effects), duplicated, deleted, moved, saved
/// as templates and given notes; configs duplicated, deleted and assigned to states; actions copied and pasted over
/// actions of the same kind (FX Effect Tree Actions Plan).
/// </summary>
public class FxEffectTree
{
    private readonly FxEditorScreen Editor;

    public readonly FxClipboard Clipboard = new();
    public readonly FxTemplates Templates;
    public readonly FxNotes Notes;

    private string Filter = "";
    private bool ExactFilter = false;

    /// <summary>Structure changes wait until the tree is drawn, so the lists aren't changed while they're walked.</summary>
    private readonly List<Action> Deferred = new();

    public FxEffectTree(FxEditorScreen editor)
    {
        Editor = editor;
        Templates = new FxTemplates(editor);
        Notes = new FxNotes(editor);
        editor.ActionManager.ActionPerformed += () => Edits++;
    }

    private FxData Data => Editor.Data;

    // ---------------------------------------------------------------- per-node info

    private int Edits;

    private class NodeInfo
    {
        public string Summary;
        public List<string> Problems;
        public string SearchText;
    }

    private readonly Dictionary<FxrNode, NodeInfo> Info = new(ReferenceEqualityComparer.Instance);
    private (object Fxr, int Edits, int State) InfoKey;
    private DateTime InfoTime;

    /// <summary>Summaries, checks and search text, rebuilt after an edit, a new preview state or once a second.</summary>
    private void RefreshInfo(FxEntry entry)
    {
        var key = ((object)entry.Fxr, Edits, Editor.Playback.PreviewState);
        if (key.Equals(InfoKey) && DateTime.UtcNow - InfoTime < TimeSpan.FromSeconds(1))
            return;
        InfoKey = key;
        InfoTime = DateTime.UtcNow;
        Info.Clear();

        foreach (var node in FXR.Walk(entry.Fxr.Root))
        {
            var info = new NodeInfo
            {
                Summary = FxNodeOps.Summary(Data, entry, node, Editor.Playback.PreviewState),
                Problems = FxNodeOps.Check(Editor, entry, node),
            };

            // What a search looks at: the type, summary, note, action names and resource ids
            var parts = new List<string> { FxNames.NodeType(node.Type), info.Summary, Notes.Get(node) };
            foreach (var action in FxNodeOps.OwnActions(node))
            {
                var view = Data.View(entry, action);
                parts.Add(view.Definition?.Name);
                foreach (var r in FxNodeOps.Resources(view))
                    parts.Add($"{r.Id} s{r.Id:D5}");
            }
            info.SearchText = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));
            Info[node] = info;
        }
    }

    private NodeInfo InfoOf(FxrNode node) => Info.TryGetValue(node, out var i) ? i : null;

    // ---------------------------------------------------------------- search

    private readonly HashSet<FxrNode> FilterShown = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<FxrNode> FilterMatches = new(ReferenceEqualityComparer.Instance);

    private bool Filtering => !string.IsNullOrWhiteSpace(Filter);

    /// <summary>Nodes that match the search, and their ancestors so the matches can be reached.</summary>
    private void RefreshFilter(FxEntry entry)
    {
        FilterShown.Clear();
        FilterMatches.Clear();
        if (!Filtering)
            return;

        bool Visit(FxrNode node)
        {
            bool match = EditorFilters.IsMatch(Filter, InfoOf(node)?.SearchText ?? "", ExactFilter);
            if (match)
                FilterMatches.Add(node);
            bool any = match;
            foreach (var child in node.Children)
                any |= Visit(child);
            if (any)
                FilterShown.Add(node);
            return any;
        }
        Visit(entry.Fxr.Root);
    }

    // ---------------------------------------------------------------- display

    public void Display()
    {
        GUI.TitleHeader(LOC.Get("FX_EffectTree_Header"), LOC.Get("FX_EffectTree_Header_TT"));

        var entry = Editor.Selection.Entry;

        DisplayToolbar(entry);

        ImGui.BeginChild("FxEffectTreeSection", ImGuiChildFlags.Borders);

        if (entry?.Fxr == null)
        {
            GUI.WrappedText(LOC.Get("FX_EffectTree_No_Effect"));
            ImGui.EndChild();
            return;
        }

        var fxr = entry.Fxr;
        Notes.EnsureLoaded(entry);
        RefreshInfo(entry);
        RefreshFilter(entry);

        // States
        bool statesOpen = !Filtering && ImGui.TreeNodeEx($"{LOC.Get("FX_EffectTree_States", fxr.States.Count)}##fxStates", ImGuiTreeNodeFlags.DefaultOpen);
        if (!Filtering && ImGui.BeginPopupContextItem("##fxStatesContext"))
        {
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Add")}##fxStatesAdd"))
                Deferred.Add(() => AddState(entry));
            GUI.Tooltip(LOC.Get("FX_EffectTree_State_Add_TT"));
            ImGui.EndPopup();
        }
        if (statesOpen)
        {
            for (int i = 0; i < fxr.States.Count; i++)
            {
                var state = fxr.States[i];
                var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow;
                if (state.Conditions.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
                if (Editor.Selection.IsSelected(state)) flags |= ImGuiTreeNodeFlags.Selected;

                var stateLabel = LOC.Get("FX_EffectTree_State", i);
                if (i == 0)
                    stateLabel = $"{stateLabel} {LOC.Get("FX_EffectTree_State_Start")}";
                if (i == Editor.Playback.PreviewState)
                    stateLabel = $"{stateLabel} {LOC.Get("FX_EffectTree_State_Previewed")}";

                bool open = ImGui.TreeNodeEx($"{stateLabel}##fxState_{i}", flags);
                if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                    Editor.Selection.SelectItem(state);
                DisplayStateContextMenu(entry, state, i);
                if (open)
                {
                    foreach (var c in state.Conditions)
                        GUI.WrappedText(FxNames.Condition(c));
                    ImGui.TreePop();
                }
            }
            ImGui.TreePop();
        }

        if (Filtering && FilterShown.Count == 0)
            GUI.WrappedText(LOC.Get("FX_EffectTree_No_Matches"));

        // Nodes
        int id = 0;
        DisplayNode(entry, fxr.Root, null, ref id, 0, false);

        ImGui.EndChild();

        DisplayPopups(entry);

        foreach (var op in Deferred)
            op();
        Deferred.Clear();
    }

    private void DisplayToolbar(FxEntry entry)
    {
        ImGui.BeginChild("FxEffectTreeSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("fxEditor_EffectTree", ref Filter, ref ExactFilter);

        GUI.DisplayToggleButton("fxTreeHighlight", Icons.Crosshairs,
            ref CFG.Current.FxEditor_EffectTree_Highlight_Selected,
            "FX_EffectTree_Highlight_Off", "FX_EffectTree_Highlight_On", "FX_EffectTree_Highlight_TT");

        GUI.DisplayToggleButton("fxTreeSummaries", Icons.Tag,
            ref CFG.Current.FxEditor_EffectTree_Show_Summaries,
            "FX_EffectTree_Summaries_Off", "FX_EffectTree_Summaries_On", "FX_EffectTree_Summaries_TT");

        GUI.DisplayToggleButton("fxTreeChecks", Icons.ExclamationTriangle,
            ref CFG.Current.FxEditor_EffectTree_Show_Checks,
            "FX_EffectTree_Checks_Off", "FX_EffectTree_Checks_On", "FX_EffectTree_Checks_TT");

        var root = entry?.Fxr?.Root;

        ImGui.SameLine();
        ImGui.BeginDisabled(root == null);
        if (ImGui.Button($"{Icons.Eye}##fxTreeShowAll", DPI.IconButtonSize))
            ShowAll(root);
        GUI.Tooltip(LOC.Get("FX_EffectTree_Show_All_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{Icons.EyeSlash}##fxTreeHideAll", DPI.IconButtonSize))
            HideAll(root);
        GUI.Tooltip(LOC.Get("FX_EffectTree_Hide_All_TT"));
        ImGui.EndDisabled();

        ImGui.EndChild();
    }

    private void DisplayNode(FxEntry entry, FxrNode node, FxrNode parent, ref int id, int depth, bool parentHidden)
    {
        int nodeId = id++;

        if (Filtering && !FilterShown.Contains(node))
            return;

        var info = InfoOf(node);
        var label = FxNames.NodeType(node.Type);
        if (node.Children.Count > 0)
            label = $"{label} ({node.Children.Count})";
        if (CFG.Current.FxEditor_EffectTree_Show_Summaries && info?.Summary != null)
            label = node.Type == FxNodeOps.ProxyType ? $"{label} → {info.Summary}" : $"{label}: {info.Summary}";
        else if (node.Type == FxNodeOps.ProxyType && info?.Summary != null)
            label = $"{label} → {info.Summary}";

        var note = Notes.Get(node);
        if (note != null)
            label = $"{label}  · {Shorten(note, 40)}";

        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
        if (depth == 0) flags |= ImGuiTreeNodeFlags.DefaultOpen;
        if (Editor.Selection.IsSelected(node)) flags |= ImGuiTreeNodeFlags.Selected;

        DisplayVisibilityButton(entry, node, nodeId);
        DisplayProblems(info);

        // Hidden nodes, and nodes under a hidden one, are dimmed; search matches are coloured
        bool hidden = parentHidden || Editor.Playback.IsNodeHidden(node);
        bool colored = hidden || Filtering && FilterMatches.Contains(node);
        if (colored)
            ImGui.PushStyleColor(ImGuiCol.Text, hidden ? ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled] : UI.Current.ImGui_Benefit_Text_Color);

        // A search opens the way to its matches
        if (Filtering && node.Children.Any(FilterShown.Contains))
            ImGui.SetNextItemOpen(true, ImGuiCond.Always);

        bool open = ImGui.TreeNodeEx($"{label}##fxNode_{nodeId}", flags);

        if (colored)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Editor.Selection.SelectItem(node);

        if (note != null && ImGui.IsItemHovered())
            ImGui.SetTooltip(note);

        DragAndDrop(entry, node, parent);
        DisplayNodeContextMenu(entry, node, parent, nodeId);

        if (!open)
            return;

        if (!Filtering)
        {
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
                DisplayConfigContextMenu(entry, node, config, c, configId);
                if (copen)
                {
                    foreach (var action in config.Actions)
                        DisplayAction(entry, action, ref id);
                    ImGui.TreePop();
                }
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

        ImGui.TreeNodeEx($"{FxNames.Action(Data, entry, action)}##fxAction_{actionId}", flags);
        if (ImGui.IsItemClicked())
            Editor.Selection.SelectItem(action);

        DisplayActionContextMenu(entry, action, actionId);
    }

    /// <summary>The eye before each node: hides the node, and everything under it, in playback. Alt+click solos it.</summary>
    private void DisplayVisibilityButton(FxEntry entry, FxrNode node, int nodeId)
    {
        var playback = Editor.Playback;
        bool hidden = playback.IsNodeHidden(node);

        ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);

        if (ImGui.Button($"{(hidden ? Icons.EyeSlash : Icons.Eye)}##fxNodeVisibility_{nodeId}", DPI.InlineIconButtonSize))
        {
            if (ImGui.GetIO().KeyAlt)
                Solo(entry.Fxr.Root, node);
            else
                SetHidden(node, !hidden);
        }

        ImGui.PopStyleColor(4);
        ImGui.PopItemFlag();
        GUI.Tooltip(LOC.Get("FX_EffectTree_Visibility_TT"));
        ImGui.SameLine();
    }

    private static void DisplayProblems(NodeInfo info)
    {
        if (!CFG.Current.FxEditor_EffectTree_Show_Checks || info?.Problems is not { Count: > 0 } problems)
            return;

        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(string.Join("\n", problems));
        ImGui.SameLine();
    }

    private static string Shorten(string s, int max)
    {
        var line = s.Split('\n')[0];
        return line.Length > max ? line[..max] + "…" : line;
    }

    // ---------------------------------------------------------------- context menus

    private void DisplayNodeContextMenu(FxEntry entry, FxrNode node, FxrNode parent, int nodeId)
    {
        if (!ImGui.BeginPopupContextItem($"##fxNodeContext_{nodeId}"))
            return;

        // The context menu acts on the node it was opened on
        Editor.Selection.SelectItem(node);

        bool editable = parent != null;
        bool canPaste = CanPasteNode(out var pasteWhy);

        // Clipboard
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Copy")}##fxNodeCopy", InputManager.GetHint(KeybindID.Copy), false, editable))
            CopyNode(entry, node);
        GUI.Tooltip(LOC.Get("FX_EffectTree_Copy_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Cut")}##fxNodeCut", "", false, editable))
        {
            CopyNode(entry, node);
            Deferred.Add(() => DeleteNode(node));
        }
        GUI.Tooltip(LOC.Get("FX_EffectTree_Cut_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Paste_After")}##fxNodePasteAfter", InputManager.GetHint(KeybindID.Paste), false, canPaste && editable))
            Deferred.Add(() => PasteNode(node, false));
        GUI.Tooltip(canPaste ? LOC.Get("FX_EffectTree_Paste_After_TT", Clipboard.Source) : pasteWhy);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Paste_Child")}##fxNodePasteChild", "", false, canPaste))
            Deferred.Add(() => PasteNode(node, true));
        GUI.Tooltip(canPaste ? LOC.Get("FX_EffectTree_Paste_Child_TT", Clipboard.Source) : pasteWhy);

        ImGui.Separator();

        // Structure
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Duplicate")}##fxNodeDuplicate", InputManager.GetHint(KeybindID.Duplicate), false, editable))
            Deferred.Add(() => DuplicateNode(node));
        GUI.Tooltip(LOC.Get("FX_EffectTree_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Delete")}##fxNodeDelete", InputManager.GetHint(KeybindID.Delete), false, editable))
            Deferred.Add(() => DeleteNode(node));
        GUI.Tooltip(LOC.Get("FX_EffectTree_Delete_TT"));

        int index = parent?.Children.IndexOf(node) ?? -1;
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxNodeUp", InputManager.GetHint(KeybindID.Reorder_Up), false, editable && index > 0))
            Deferred.Add(() => MoveNode(node, -1));
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxNodeDown", InputManager.GetHint(KeybindID.Reorder_Down), false, editable && index >= 0 && index < parent.Children.Count - 1))
            Deferred.Add(() => MoveNode(node, 1));
        if (parent?.Type == FxNodeOps.LodType)
            GUI.Tooltip(LOC.Get("FX_EffectTree_Move_Lod_TT"));

        ImGui.Separator();

        // Templates
        if (ImGui.BeginMenu($"{LOC.Get("FX_EffectTree_Add_Template")}##fxNodeAddTemplate"))
        {
            var names = Templates.Names;
            if (names.Count == 0)
                ImGui.TextDisabled(LOC.Get("FX_EffectTree_No_Templates"));
            foreach (var name in names)
            {
                if (ImGui.MenuItem($"{name}##fxTemplate_{name}"))
                {
                    var n = name;
                    Deferred.Add(() => AddFromTemplate(node, n));
                }
            }
            ImGui.Separator();
            if (Templates.IsBuildingStarters)
                ImGui.TextDisabled(LOC.Get("FX_EffectTree_Building_Starters", Templates.StarterDone, Templates.StarterTotal));
            else if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Build_Starters")}##fxBuildStarters", "", false, Data.Bank.IsIndexed))
                Templates.BuildStarters();
            GUI.Tooltip(LOC.Get("FX_EffectTree_Build_Starters_TT"));

            if (names.Count > 0)
            {
                if (ImGui.BeginMenu($"{LOC.Get("FX_EffectTree_Remove_Template")}##fxRemoveTemplate"))
                {
                    foreach (var name in names)
                        if (ImGui.MenuItem($"{name}##fxRemoveTemplate_{name}"))
                        {
                            var n = name;
                            Deferred.Add(() => Templates.Archive(n));
                        }
                    ImGui.EndMenu();
                }
                GUI.Tooltip(LOC.Get("FX_EffectTree_Remove_Template_TT"));
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("FX_EffectTree_Add_Template_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Save_Template")}##fxNodeSaveTemplate", "", false, editable))
            OpenPopup(TemplatePopup, node, FxNames.NodeType(node.Type));
        GUI.Tooltip(LOC.Get("FX_EffectTree_Save_Template_TT", Templates.Folder));

        ImGui.Separator();

        // Viewing
        bool hidden = Editor.Playback.IsNodeHidden(node);
        if (ImGui.MenuItem($"{LOC.Get(hidden ? "FX_EffectTree_Show" : "FX_EffectTree_Hide")}##fxNodeVisibility"))
            SetHidden(node, !hidden);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Solo")}##fxNodeSolo"))
            Solo(entry.Fxr.Root, node);
        GUI.Tooltip(LOC.Get("FX_EffectTree_Solo_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Frame")}##fxNodeFrame", InputManager.GetHint(KeybindID.Frame)))
            Editor.ViewportWindow?.FrameSelected();
        GUI.Tooltip(LOC.Get("FX_EffectTree_Frame_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Edit_Note")}##fxNodeNote"))
            OpenPopup(NotePopup, node, Notes.Get(node) ?? "");
        GUI.Tooltip(LOC.Get("FX_EffectTree_Edit_Note_TT"));

        // Proxy
        if (node.Type == FxNodeOps.ProxyType && node.Actions.FirstOrDefault(a => a.Type == FxNodeOps.ProxyAction) is { Fields1.Count: > 0 } proxy)
        {
            ImGui.Separator();
            int target = (int)proxy.Fields1[0];
            bool exists = Data.Bank.Entries.ContainsKey(target);
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Open_Proxy", $"f{target:D9}")}##fxNodeOpenProxy", "", false, exists && target != entry.Id))
                Deferred.Add(() => Editor.JumpTo(target));
            GUI.Tooltip(LOC.Get("FX_EffectTree_Open_Proxy_TT"));

            var refusal = FxProxyOps.ParentRefusal(parent);
            bool canInline = exists && target != entry.Id && refusal == null;
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Inline_Proxy")}##fxNodeInline", "", false, canInline))
                RequestInline(entry, node);
            GUI.Tooltip(refusal != null ? LOC.Get($"FX_EffectTree_Inline_Refused_{refusal}") : LOC.Get("FX_EffectTree_Inline_Proxy_TT"));
        }

        if (editable && node.Type != FxNodeOps.ProxyType)
        {
            ImGui.Separator();
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Extract")}##fxNodeExtract", "", false, entry.Binders.Count > 0))
                RequestExtract(entry, node);
            GUI.Tooltip(LOC.Get("FX_EffectTree_Extract_TT"));
        }

        ImGui.EndPopup();
    }

    private void DisplayStateContextMenu(FxEntry entry, FxrState state, int index)
    {
        if (!ImGui.BeginPopupContextItem($"##fxStateContext_{index}"))
            return;

        Editor.Selection.SelectItem(state);
        int count = entry.Fxr.States.Count;

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Preview")}##fxStatePreview", "", Editor.Playback.PreviewState == index))
            Editor.Playback.SetPreviewState(index);
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Preview_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Add")}##fxStateAdd"))
            Deferred.Add(() => AddState(entry));
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Add_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Duplicate")}##fxStateDuplicate", InputManager.GetHint(KeybindID.Duplicate)))
            Deferred.Add(() => DuplicateState(entry, state));
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Delete")}##fxStateDelete", InputManager.GetHint(KeybindID.Delete), false, count > 1))
            RequestDeleteState(state);
        GUI.Tooltip(LOC.Get(count > 1 ? "FX_EffectTree_State_Delete_TT" : "FX_EffectTree_State_Delete_Last_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxStateUp", InputManager.GetHint(KeybindID.Reorder_Up), false, index > 0))
            Deferred.Add(() => MoveState(entry, state, -1));
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxStateDown", InputManager.GetHint(KeybindID.Reorder_Down), false, index < count - 1))
            Deferred.Add(() => MoveState(entry, state, 1));
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Move_TT"));

        ImGui.EndPopup();
    }

    private void DisplayConfigContextMenu(FxEntry entry, FxrNode node, FxrConfig config, int index, int configId)
    {
        if (!ImGui.BeginPopupContextItem($"##fxConfigContext_{configId}"))
            return;

        Editor.Selection.SelectItem(config);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Config_Duplicate")}##fxConfigDuplicate"))
            Deferred.Add(() => Execute(FxConfigListAction.Add(Data.Bank, entry, node, FxClone.Config(config))));
        GUI.Tooltip(LOC.Get("FX_EffectTree_Config_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Config_Delete")}##fxConfigDelete", "", false, node.Configs.Count > 1))
        {
            Deferred.Add(() =>
            {
                Execute(FxConfigListAction.Remove(Data.Bank, entry, node, config));
                if (!FxNodeOps.Contains(entry.Fxr.Root, Editor.Selection.Item ?? node))
                    Editor.Selection.ClearItem();
            });
        }
        GUI.Tooltip(LOC.Get("FX_EffectTree_Config_Delete_TT"));

        var stateMap = FxNodeOps.StateMap(node);
        if (ImGui.BeginMenu($"{LOC.Get("FX_EffectTree_Config_Use_For")}##fxConfigUseFor", stateMap != null))
        {
            var map = FxNames.StateConfigMap(node);
            for (int s = 0; s < entry.Fxr.States.Count; s++)
            {
                bool uses = map != null && (s < map.Count ? map[s] : map.Count > 0 ? map[0] : 0) == index;
                if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State", s)}##fxConfigState_{s}", "", uses, !uses))
                {
                    int state = s;
                    Deferred.Add(() => Execute(FxReplaceActionAction.SetStateConfig(Data.Bank, entry, stateMap, state, index)));
                }
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get(stateMap != null ? "FX_EffectTree_Config_Use_For_TT" : "FX_EffectTree_Config_No_Map_TT"));

        DisplaySlotsMenu(entry, config);

        ImGui.EndPopup();
    }

    /// <summary>
    /// The config's fixed action slots (plan §23): each with its action, and the action types that can fill it in this
    /// game. Setting one copies the first example of that type from the project's effects (so its values are laid out
    /// as the game stores them); clearing one leaves an empty action, where the slot allows it.
    /// </summary>
    private void DisplaySlotsMenu(FxEntry entry, FxrConfig config)
    {
        var schema = Data.Schema;
        var slots = FxSlotOps.SlotsFor(schema, config.Type, Data.Game);
        if (!ImGui.BeginMenu($"{LOC.Get("FX_EffectTree_Slots")}##fxConfigSlots", slots != null))
        {
            GUI.Tooltip(LOC.Get(slots != null ? "FX_EffectTree_Slots_TT" : "FX_EffectTree_Slots_None_TT"));
            return;
        }

        var usage = Editor.EffectList.Usage;
        if (!usage.HasScan)
        {
            usage.StartScan();
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Scanning", usage.ScanDone, usage.ScanTotal));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            var current = i < config.Actions.Count ? config.Actions[i] : null;
            var slotName = FxSlotOps.SlotName(schema, slots[i], i);
            var currentName = current == null || current.Type == 0
                ? LOC.Get("FX_EffectTree_Slot_Empty")
                : schema.Actions.TryGetValue(current.Type, out var cd) ? cd.Name : current.Type.ToString();

            if (!ImGui.BeginMenu($"{FxNames.Pretty(slotName)}: {currentName}##fxSlot{i}"))
                continue;

            int slot = i;
            foreach (var type in FxSlotOps.TypesFor(schema, slots[i], Data.Game))
            {
                var name = schema.Actions.TryGetValue(type, out var d) ? d.Name : type.ToString();
                bool isCurrent = current?.Type == type;
                bool has = usage.TryGetExample(type, out var example, out var from);
                if (ImGui.MenuItem($"{name} ({type})##fxSlot{i}_{type}", "", isCurrent, has && !isCurrent))
                {
                    var contents = example;
                    Deferred.Add(() => SetSlot(entry, config, slot, contents));
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip(has
                        ? LOC.Get("FX_EffectTree_Slot_Example_TT", $"f{from:D9}", d?.Description ?? "")
                        : LOC.Get(usage.HasScan ? "FX_EffectTree_Slot_No_Example_TT" : "FX_EffectTree_Slot_Scanning_TT"));
            }

            if (FxSlotOps.IsNullable(schema, slots[i], i))
            {
                ImGui.Separator();
                bool empty = current == null || current.Type == 0;
                if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Slot_Clear")}##fxSlotClear{i}", "", false, !empty))
                    Deferred.Add(() => SetSlot(entry, config, slot, FxSlotOps.Empty()));
                GUI.Tooltip(LOC.Get("FX_EffectTree_Slot_Clear_TT"));
            }
            ImGui.EndMenu();
        }
        ImGui.EndMenu();
    }

    private void SetSlot(FxEntry entry, FxrConfig config, int slot, FxrAction contents)
    {
        var action = new FxSetSlotAction(Data.Bank, entry, config, slot, contents);
        Execute(action);
        if (action.Target is { Type: not 0 } target)
            Editor.Selection.SelectItem(target);
        else
            Editor.Selection.SelectItem(config);
    }

    private void DisplayActionContextMenu(FxEntry entry, FxrAction action, int actionId)
    {
        if (!ImGui.BeginPopupContextItem($"##fxActionContext_{actionId}"))
            return;

        Editor.Selection.SelectItem(action);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Action_Copy")}##fxActionCopy", InputManager.GetHint(KeybindID.Copy)))
            Clipboard.CopyAction(action, Data.Game, entry.Stem);
        GUI.Tooltip(LOC.Get("FX_EffectTree_Action_Copy_TT"));

        bool canPaste = CanPasteAction(entry, action, out var why);
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Action_Paste")}##fxActionPaste", InputManager.GetHint(KeybindID.Paste), false, canPaste))
            Deferred.Add(() => PasteAction(entry, action));
        GUI.Tooltip(canPaste ? LOC.Get("FX_EffectTree_Action_Paste_TT", Clipboard.Source) : why);

        ImGui.EndPopup();
    }

    // ---------------------------------------------------------------- popups

    private const string TemplatePopup = "fxSaveTemplatePopup";
    private const string NotePopup = "fxNotePopup";

    private string PopupToOpen;
    private FxrNode PopupNode;
    private string PopupText = "";

    private void OpenPopup(string popup, FxrNode node, string text)
    {
        PopupToOpen = popup;
        PopupNode = node;
        PopupText = text;
    }

    private void DisplayPopups(FxEntry entry)
    {
        if (PopupToOpen != null)
        {
            ImGui.OpenPopup($"{LOC.Get(PopupToOpen == TemplatePopup ? "FX_EffectTree_Save_Template" : "FX_EffectTree_Edit_Note")}###{PopupToOpen}");
            PopupToOpen = null;
        }

        DisplayInlinePopup(entry);
        DisplayExtractPopup(entry);

        // Delete State, confirmed: the other states and the node maps are renumbered
        if (DeleteStateToOpen)
        {
            ImGui.OpenPopup($"{LOC.Get("FX_EffectTree_State_Delete")}###{DeleteStatePopup}");
            DeleteStateToOpen = false;
        }
        if (ImGui.BeginPopupModal($"{LOC.Get("FX_EffectTree_State_Delete")}###{DeleteStatePopup}", ImGuiWindowFlags.AlwaysAutoResize))
        {
            int index = PendingDeleteState == null ? -1 : entry.Fxr.States.IndexOf(PendingDeleteState);
            if (index < 0)
            {
                ImGui.CloseCurrentPopup();
            }
            else
            {
                int into = FxStateOps.ConditionsInto(entry.Fxr, index);
                int later = entry.Fxr.States.Count - index - 1;
                GUI.WrappedText(LOC.Get("FX_EffectTree_State_Delete_Confirm", index));
                if (into > 0)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_State_Delete_Into", into, index));
                if (later > 0)
                    GUI.WrappedText(LOC.Get("FX_EffectTree_State_Delete_Renumber", index + 1, entry.Fxr.States.Count - 1));
                if (index == 0)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_State_Delete_Start"));

                if (ImGui.Button($"{LOC.Get("FX_EffectTree_State_Delete")}##fxStateDeleteConfirm"))
                {
                    var state = PendingDeleteState;
                    Deferred.Add(() => DeleteState(entry, state));
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
                    ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }

        // Save as Template
        if (ImGui.BeginPopupModal($"{LOC.Get("FX_EffectTree_Save_Template")}###{TemplatePopup}", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text(LOC.Get("FX_EffectTree_Template_Name"));
            ImGui.SetNextItemWidth(300 * DPI.UIScale());
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            bool enter = ImGui.InputText("##fxTemplateName", ref PopupText, 128, ImGuiInputTextFlags.EnterReturnsTrue);

            var clean = FxTemplates.Clean(PopupText ?? "");
            if (clean != null && Templates.Exists(clean))
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_Template_Exists"));

            ImGui.BeginDisabled(clean == null);
            if ((ImGui.Button(LOC.Get("FX_EffectTree_Save")) || enter && clean != null) && PopupNode != null)
            {
                Templates.Save(clean, entry, PopupNode);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }

        // Note
        if (ImGui.BeginPopupModal($"{LOC.Get("FX_EffectTree_Edit_Note")}###{NotePopup}", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            ImGui.InputTextMultiline("##fxNoteText", ref PopupText, 1024, new Vector2(400, 120) * DPI.UIScale());
            GUI.WrappedText(LOC.Get("FX_EffectTree_Note_Hint"));

            if (ImGui.Button(LOC.Get("FX_EffectTree_Save")) && PopupNode != null)
            {
                Notes.Set(entry, PopupNode, PopupText);
                Edits++;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    // ---------------------------------------------------------------- drag and drop

    private FxrNode DragNode;

    /// <summary>Dragging a node onto another makes it that node's last child.</summary>
    private void DragAndDrop(FxEntry entry, FxrNode node, FxrNode parent)
    {
        if (parent != null && ImGui.BeginDragDropSource())
        {
            DragNode = node;
            unsafe
            {
                byte dummy = 0;
                ImGui.SetDragDropPayload("FX_NODE", &dummy, 1);
            }
            ImGui.Text(LOC.Get("FX_EffectTree_Drag", FxNames.NodeType(node.Type)));
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            unsafe
            {
                var payload = ImGui.AcceptDragDropPayload("FX_NODE");
                if (!payload.IsNull && DragNode != null && !FxNodeOps.IsInBranch(DragNode, node))
                {
                    var dragged = DragNode;
                    var target = node;
                    Deferred.Add(() => Reparent(entry, dragged, target));
                    DragNode = null;
                }
            }
            ImGui.EndDragDropTarget();
        }
    }

    // ---------------------------------------------------------------- shortcuts

    /// <summary>The node shortcuts, while the Effect Tree has focus and nothing is being typed.</summary>
    public void Shortcuts()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.FxEditor_EffectTree) || ImGui.GetIO().WantTextInput)
            return;

        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null)
            return;

        if (Editor.Selection.Item is FxrAction action)
        {
            if (InputManager.IsPressed(KeybindID.Copy))
                Clipboard.CopyAction(action, Data.Game, entry.Stem);
            else if (InputManager.IsPressed(KeybindID.Paste) && CanPasteAction(entry, action, out _))
                Deferred.Add(() => PasteAction(entry, action));
            return;
        }

        if (Editor.Selection.Item is FxrState state)
        {
            if (InputManager.IsPressed(KeybindID.Duplicate))
                Deferred.Add(() => DuplicateState(entry, state));
            else if (InputManager.IsPressed(KeybindID.Delete) && entry.Fxr.States.Count > 1)
                RequestDeleteState(state);
            else if (InputManager.IsPressed(KeybindID.Reorder_Up))
                Deferred.Add(() => MoveState(entry, state, -1));
            else if (InputManager.IsPressed(KeybindID.Reorder_Down))
                Deferred.Add(() => MoveState(entry, state, 1));
            return;
        }

        if (Editor.Selection.Item is not FxrNode node)
            return;

        bool isRoot = node == entry.Fxr.Root;

        if (InputManager.IsPressed(KeybindID.Frame))
            Editor.ViewportWindow?.FrameSelected();

        if (InputManager.IsPressed(KeybindID.Paste) && CanPasteNode(out _))
            Deferred.Add(() => PasteNode(node, isRoot));

        if (isRoot || EditableNode == null)
            return;

        if (InputManager.IsPressed(KeybindID.Copy))
            CopyNode(entry, node);
        else if (InputManager.IsPressed(KeybindID.Duplicate))
            Deferred.Add(() => DuplicateNode(node));
        else if (InputManager.IsPressed(KeybindID.Delete))
            Deferred.Add(() => DeleteNode(node));
        else if (InputManager.IsPressed(KeybindID.Reorder_Up))
            Deferred.Add(() => MoveNode(node, -1));
        else if (InputManager.IsPressed(KeybindID.Reorder_Down))
            Deferred.Add(() => MoveNode(node, 1));
    }

    // ---------------------------------------------------------------- visibility

    private void SetHidden(FxrNode node, bool hidden)
    {
        Editor.Playback.SetNodeHidden(node, hidden);
        if (Editor.ViewportWindow != null)
            Editor.ViewportWindow.HiddenVersion++;
    }

    /// <summary>Shows only the node's branch: the nodes above it stay shown, and every other branch is hidden.</summary>
    public void Solo(FxrNode root, FxrNode node)
    {
        var path = new List<FxrNode>();
        for (var n = node; n != null; n = FxNodeOps.FindParent(root, n))
            path.Insert(0, n);

        for (int i = 0; i < path.Count - 1; i++)
        {
            SetHidden(path[i], false);
            foreach (var child in path[i].Children)
                if (!ReferenceEquals(child, path[i + 1]))
                    SetHidden(child, true);
        }

        foreach (var n in FXR.Walk(node))
            SetHidden(n, false);
    }

    public void ShowAll(FxrNode root)
    {
        if (root == null)
            return;
        foreach (var n in FXR.Walk(root))
            SetHidden(n, false);
    }

    /// <summary>Hides the root's branches, so showing one again shows all of it.</summary>
    public void HideAll(FxrNode root)
    {
        if (root == null)
            return;
        ShowAll(root);
        foreach (var child in root.Children)
            SetHidden(child, true);
    }

    // ---------------------------------------------------------------- node actions

    /// <summary>The selected node, when it can be duplicated, deleted, moved or copied (any node but the root).</summary>
    public FxrNode EditableNode
    {
        get
        {
            var fxr = Editor.Selection.Entry?.Fxr;
            return Editor.Selection.Item is FxrNode node && fxr != null && node != fxr.Root && FxNodeOps.FindParent(fxr.Root, node) != null
                ? node : null;
        }
    }

    private void Execute(EditorAction action)
    {
        // A value still being typed belongs to the tree as it was, so it's committed first
        Editor.Inspector.FlushPending();
        Editor.CurveEditor.FlushPending();
        Editor.ActionManager.ExecuteAction(action);
    }

    private FxrNode ParentOf(FxrNode node)
    {
        var root = Editor.Selection.Entry?.Fxr?.Root;
        return root == null ? null : FxNodeOps.FindParent(root, node);
    }

    /// <summary>Puts a copy of the node (its configs, actions and children) just after it, and selects the copy.</summary>
    public void DuplicateNode(FxrNode node)
    {
        var entry = Editor.Selection.Entry;
        var parent = ParentOf(node);
        if (parent == null)
            return;

        var copy = FxClone.Node(node);
        Execute(FxNodeListAction.Add(Data.Bank, entry, parent, copy, parent.Children.IndexOf(node) + 1));
        Editor.Selection.SelectItem(copy);
    }

    /// <summary>Takes the node, and everything under it, out of its parent.</summary>
    public void DeleteNode(FxrNode node)
    {
        var entry = Editor.Selection.Entry;
        var parent = ParentOf(node);
        if (parent == null)
            return;

        Execute(FxNodeListAction.Remove(Data.Bank, entry, parent, node));

        // The selection may have been inside what went
        if (Editor.Selection.Item != null && !FxNodeOps.Contains(entry.Fxr.Root, Editor.Selection.Item))
            Editor.Selection.ClearItem();
    }

    /// <summary>Moves the node one place up (-1) or down (+1) among its siblings.</summary>
    public void MoveNode(FxrNode node, int step)
    {
        var entry = Editor.Selection.Entry;
        var parent = ParentOf(node);
        if (parent == null)
            return;
        int index = parent.Children.IndexOf(node) + step;
        if (index < 0 || index >= parent.Children.Count)
            return;
        Execute(new FxNodeMoveAction(Data.Bank, entry, node, parent, parent, index));
    }

    /// <summary>Makes the node the last child of <paramref name="target"/>.</summary>
    private void Reparent(FxEntry entry, FxrNode node, FxrNode target)
    {
        var parent = FxNodeOps.FindParent(entry.Fxr.Root, node);
        if (parent == null || FxNodeOps.IsInBranch(node, target))
            return;
        if (ReferenceEquals(parent, target) && parent.Children[^1] == node)
            return;
        Execute(new FxNodeMoveAction(Data.Bank, entry, node, parent, target, target.Children.Count));
        Editor.Selection.SelectItem(node);
    }

    private void CopyNode(FxEntry entry, FxrNode node)
    {
        Clipboard.CopyNode(node, Data.Game, entry.Fxr.States.Count, entry.Stem);
    }

    private bool CanPasteNode(out string why)
    {
        why = null;
        if (!Clipboard.HasNode)
        {
            why = LOC.Get("FX_EffectTree_Paste_Empty");
            return false;
        }
        if (Clipboard.Game != Data.Game)
        {
            why = LOC.Get("FX_EffectTree_Paste_Other_Game", Clipboard.Game);
            return false;
        }
        return true;
    }

    /// <summary>Pastes the clipboard node after <paramref name="target"/>, or as its last child.</summary>
    public void PasteNode(FxrNode target, bool asChild)
    {
        if (!CanPasteNode(out _))
            return;
        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null)
            return;

        var parent = asChild ? target : FxNodeOps.FindParent(entry.Fxr.Root, target);
        if (parent == null)
        {
            parent = entry.Fxr.Root;
            asChild = true;
        }
        int index = asChild ? parent.Children.Count : parent.Children.IndexOf(target) + 1;
        Insert(entry, parent, index, Clipboard.TakeNode(), Clipboard.StateCount, Clipboard.Source);
    }

    /// <summary>Adds a template as the last child of <paramref name="target"/>.</summary>
    private void AddFromTemplate(FxrNode target, string name)
    {
        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null || Templates.Load(name) is not { } template)
            return;
        Insert(entry, target, target.Children.Count, template.Node, template.StateCount, name);
    }

    /// <summary>Adds a node from elsewhere (the clipboard or a template), fitting its state maps to this effect.</summary>
    private void Insert(FxEntry entry, FxrNode parent, int index, FxrNode node, int sourceStates, string source)
    {
        int states = entry.Fxr.States.Count;
        if (sourceStates != states)
        {
            int fitted = FxNodeOps.FitStateMaps(node, states);
            if (fitted > 0)
                Smithbox.Log(this, LOC.Get("FX_EffectTree_Paste_Fitted", source, sourceStates, states, fitted));
        }

        Execute(FxNodeListAction.Add(Data.Bank, entry, parent, node, index));
        Editor.Selection.SelectItem(node);
    }

    // ---------------------------------------------------------------- states

    private const string DeleteStatePopup = "fxDeleteStatePopup";
    private bool DeleteStateToOpen;
    private FxrState PendingDeleteState;

    private void RequestDeleteState(FxrState state)
    {
        PendingDeleteState = state;
        DeleteStateToOpen = true;
    }

    private void ExecuteStates(FxEntry entry, Action<FXR> apply)
    {
        Execute(new FxStatesAction(Data.Bank, entry, apply));
        // The preview state must still exist
        if (Editor.Playback.PreviewState >= entry.Fxr.States.Count)
            Editor.Playback.SetPreviewState(entry.Fxr.States.Count - 1);
    }

    public void AddState(FxEntry entry)
    {
        int added = -1;
        ExecuteStates(entry, fxr => added = FxStateOps.Add(fxr));
        if (added >= 0)
            Editor.Selection.SelectItem(entry.Fxr.States[added]);
    }

    public void DuplicateState(FxEntry entry, FxrState state)
    {
        int index = entry.Fxr.States.IndexOf(state);
        if (index < 0)
            return;
        int added = -1;
        ExecuteStates(entry, fxr => added = FxStateOps.Duplicate(fxr, index));
        if (added >= 0)
            Editor.Selection.SelectItem(entry.Fxr.States[added]);
    }

    public void DeleteState(FxEntry entry, FxrState state)
    {
        int index = entry.Fxr.States.IndexOf(state);
        if (index < 0 || entry.Fxr.States.Count <= 1)
            return;
        ExecuteStates(entry, fxr => FxStateOps.Delete(fxr, index));
        if (ReferenceEquals(Editor.Selection.Item, state))
            Editor.Selection.ClearItem();
    }

    public void MoveState(FxEntry entry, FxrState state, int step)
    {
        int index = entry.Fxr.States.IndexOf(state);
        int to = index + step;
        if (index < 0 || to < 0 || to >= entry.Fxr.States.Count)
            return;
        ExecuteStates(entry, fxr => FxStateOps.Swap(fxr, index, to));
    }

    // ---------------------------------------------------------------- proxies

    private const string InlinePopup = "fxInlinePopup";
    private const string ExtractPopup = "fxExtractPopup";
    private bool InlineToOpen, ExtractToOpen;
    private FxrNode ProxyNode;
    private FXR ProxyTarget;
    private List<string> ProxyDifferences = new();
    private int ExtractId;

    /// <summary>The Proxy node's target effect as it is now (with unsaved edits), or a fresh read of it.</summary>
    private FXR TargetEffect(int id)
    {
        if (!Data.Bank.Entries.TryGetValue(id, out var target))
            return null;
        return target.IsLoaded ? target.Fxr : Data.Bank.ReadCopy(target);
    }

    private void RequestInline(FxEntry entry, FxrNode node)
    {
        var target = TargetEffect(FxProxyOps.Target(node));
        if (target == null)
            return;
        ProxyNode = node;
        ProxyTarget = target;
        ProxyDifferences = FxProxyOps.InlineDifferences(entry.Fxr, target);
        InlineToOpen = true;
    }

    private void DisplayInlinePopup(FxEntry entry)
    {
        var title = $"{LOC.Get("FX_EffectTree_Inline_Proxy")}###{InlinePopup}";
        if (InlineToOpen)
        {
            ImGui.OpenPopup(title);
            InlineToOpen = false;
        }
        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        int id = ProxyNode == null ? -1 : FxProxyOps.Target(ProxyNode);
        var target = ProxyTarget;
        if (target == null || !FxNodeOps.Contains(entry.Fxr.Root, ProxyNode))
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("FX_EffectTree_Inline_Confirm", $"f{id:D9}", target.Root.Children.Count));
        if (ProxyDifferences.Count == 0)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("FX_EffectTree_Inline_Same"));
        foreach (var d in ProxyDifferences)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get($"FX_EffectTree_Proxy_Diff_{d}"));

        if (ImGui.Button($"{LOC.Get("FX_EffectTree_Inline_Proxy")}##fxInlineConfirm"))
        {
            var node = ProxyNode;
            var from = ProxyTarget;
            Deferred.Add(() => InlineProxy(entry, node, from));
            ImGui.CloseCurrentPopup();
        }
        ImGui.SameLine();
        if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
            ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }

    /// <summary>Replaces the Proxy node with copies of the nodes of the effect it starts.</summary>
    public void InlineProxy(FxEntry entry, FxrNode node, FXR target)
    {
        var parent = FxNodeOps.FindParent(entry.Fxr.Root, node);
        if (parent == null || target == null || FxProxyOps.ParentRefusal(parent) != null)
            return;

        var nodes = FxProxyOps.InlineNodes(target, entry.Fxr.States.Count);
        Execute(new FxNodeSpliceAction(Data.Bank, entry, parent, node, nodes));
        if (nodes.Count > 0)
            Editor.Selection.SelectItem(nodes[0]);
        else
            Editor.Selection.SelectItem(parent);
    }

    private void RequestExtract(FxEntry entry, FxrNode node)
    {
        ProxyNode = node;
        ProxyDifferences = FxProxyOps.ExtractDifferences(entry.Fxr, FxNodeOps.FindParent(entry.Fxr.Root, node));
        // The first unused id after this effect's
        ExtractId = entry.Id + 1;
        while (Data.Bank.Entries.ContainsKey(ExtractId))
            ExtractId++;
        ExtractToOpen = true;
    }

    private void DisplayExtractPopup(FxEntry entry)
    {
        var title = $"{LOC.Get("FX_EffectTree_Extract")}###{ExtractPopup}";
        if (ExtractToOpen)
        {
            ImGui.OpenPopup(title);
            ExtractToOpen = false;
        }
        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (ProxyNode == null || !FxNodeOps.Contains(entry.Fxr.Root, ProxyNode))
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("FX_EffectTree_Extract_Explain", FxNames.NodeType(ProxyNode.Type)));
        ImGui.Text(LOC.Get("FX_EffectTree_Extract_Id"));
        ImGui.SetNextItemWidth(200 * DPI.UIScale());
        ImGui.InputInt("##fxExtractId", ref ExtractId);
        ExtractId = Math.Max(0, ExtractId);
        ImGui.SameLine();
        ImGui.TextDisabled($"f{ExtractId:D9}");

        bool taken = Data.Bank.Entries.ContainsKey(ExtractId);
        if (taken)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_Extract_Taken"));

        GUI.WrappedText(LOC.Get("FX_EffectTree_Extract_Binders", entry.Binders.Count,
            string.Join(", ", entry.Binders.Take(4).Select(b => b.Binder.Filename)) + (entry.Binders.Count > 4 ? ", ..." : "")));
        foreach (var d in ProxyDifferences)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get($"FX_EffectTree_Proxy_Diff_{d}"));

        ImGui.BeginDisabled(taken);
        if (ImGui.Button($"{LOC.Get("FX_EffectTree_Extract_Confirm")}##fxExtractConfirm"))
        {
            var node = ProxyNode;
            int id = ExtractId;
            Deferred.Add(() => ExtractNode(entry, node, id));
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
            ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }

    /// <summary>
    /// Moves the node into a new effect <paramref name="id"/> (in the same binders as this one) and puts a Proxy
    /// node starting it in its place.
    /// </summary>
    public void ExtractNode(FxEntry entry, FxrNode node, int id)
    {
        var parent = FxNodeOps.FindParent(entry.Fxr.Root, node);
        if (parent == null || Data.Bank.Entries.ContainsKey(id))
            return;

        var stem = $"f{id:D9}";
        var created = new FxEntry
        {
            Id = id,
            Fxr = FxProxyOps.ExtractEffect(entry.Fxr, Data.Game, id, node),
            Binders = entry.Binders.Select(b => new FxBinderRef(b.Binder, FxBank.SiblingName(b.InternalName, stem, ".fxr"))).ToList(),
            ResourceListFrom = entry.Id,
        };
        var proxy = FxProxyOps.MakeProxy(id);

        Execute(new FxExtractAction(Data.Bank, entry, created, parent, node, proxy));
        Editor.Selection.SelectItem(proxy);
        Smithbox.Log(this, LOC.Get("FX_EffectTree_Extract_Done", stem, entry.Stem, entry.Binders.Count));
    }

    // ---------------------------------------------------------------- action clipboard

    private bool CanPasteAction(FxEntry entry, FxrAction target, out string why)
    {
        why = null;
        if (!Clipboard.HasAction)
        {
            why = LOC.Get("FX_EffectTree_Action_Paste_Empty");
            return false;
        }
        if (Clipboard.Game != Data.Game)
        {
            why = LOC.Get("FX_EffectTree_Paste_Other_Game", Clipboard.Game);
            return false;
        }

        // The same slot (e.g. one Appearance for another), or the same type where the schema gives no slot
        var from = Data.Schema.Actions.GetValueOrDefault(Clipboard.Action.Type);
        var to = Data.Schema.Actions.GetValueOrDefault(target.Type);
        bool fits = Clipboard.Action.Type == target.Type || from?.Slot != null && from.Slot == to?.Slot;
        if (!fits)
        {
            why = LOC.Get("FX_EffectTree_Action_Paste_Slot", from?.Slot ?? Clipboard.Action.Type.ToString());
            return false;
        }
        return true;
    }

    private void PasteAction(FxEntry entry, FxrAction target)
    {
        if (!CanPasteAction(entry, target, out _))
            return;
        Execute(new FxReplaceActionAction(Data.Bank, entry, target, Clipboard.Action));
        Editor.Selection.Property = null;
    }
}
