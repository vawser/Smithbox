using StudioCore.Editors.Common;
using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected file's states as a node graph.
///
/// The selected state's node lists each transition as its own output pin, in the order the game tries them (last in
/// the file first), with one link per transition. Every other node is compact: one output pin for all its
/// transitions (hover it for the list in game order) and one link per state it leads to. Self transitions have no
/// link (the game skips them).
///
/// Focus 1 or 2 shows the selected state's neighbourhood: states leading in to the left, states it leads to on the
/// right; compact links are drawn only from one column to the next. All shows the whole file (small files only), in
/// layers from the states nothing leads to (entered by code).
///
/// Budget: imnodes draws the whole graph into one ImGui draw list, which the Vulkan ImGui renderer limits to 65535
/// vertices (16-bit indices, no RendererHasVtxOffset). A base file (300+ states, ~45 transitions each) drawn with a
/// pin per transition on every node went far past that and crashed in ImGui's Render. Hence compact nodes,
/// column-to-column links and <see cref="MaxNodes"/>.
///
/// Click a node or link to select it in the other windows. Drag a transition pin of the selected state onto a node
/// to retarget it; drag a compact pin or the + pin onto a node to add a transition to it.
/// </summary>
public class BhvGraphWindow
{
    private readonly BhvEditorView View;

    private ImNodesEditorContextPtr EditorContext;

    /// <summary>Most nodes one graph draws; the nearest states to the focus are kept</summary>
    private const int MaxNodes = 60;

    /// <summary>Files up to this many states can be shown whole</summary>
    private const int MaxAllStates = 60;

    /// <summary>Most compact links one graph draws; those into the focus and nearest it are kept</summary>
    private const int MaxPairLinks = 100;
    private int HiddenLinks;

    /// <summary>0 = the whole file, 1 or 2 = steps around the selected state</summary>
    private int Depth = -1;

    // The layout and what it was made for
    private Dictionary<int, Vector2> Layout;
    private Dictionary<int, int> Columns = new();
    private int HiddenCount;
    private BhvFileWrapper LayoutFile;
    private int LayoutRevision = -1;
    private int LayoutFocus = -2;
    private int LayoutDepth = -1;
    private readonly Dictionary<int, Vector2> NodeSizes = new();
    private static readonly Vector2 DefaultNodeSize = new(220f, 80f);
    private static readonly Vector2 CellPadding = new(90f, 24f);

    /// <summary>Most states stacked in one sub-column</summary>
    private const int MaxRows = 10;

    /// <summary>Link segments per pixel of length while drawing this graph (imnodes default 0.1)</summary>
    private const float LinkSegmentsPerLength = 0.01f;

    /// <summary>
    /// Vertices the graph aims to stay under (ImGui asserts in Render at 65535 per draw list with the Vulkan renderer).
    /// Measured after every draw; going over shrinks the node and link caps for this file.
    /// </summary>
    private const int VertexSoftLimit = 35000;
    private float Budget = 1f;
    private int LastVertexCount;
    private int NodeCap => Math.Max(8, (int)(MaxNodes * Budget));
    private int LinkCap => Math.Max(10, (int)(MaxPairLinks * Budget));

    // Selection sync: what the graph last selected, and a pending push of the editor's selection into the graph
    private int LastGraphNode = -1;
    private int LastGraphLink = -1;
    private int SyncedState = -2;
    private bool FrameSelected;
    private Vector2 PaneSize;

    // This frame's pins and links: the focus state's transitions, and compact (from, to) links
    private readonly List<int> FocusPins = new();
    private readonly List<(int From, int To)> PairLinks = new();
    private List<int> Shown = new();
    private int ShownFocus = -2;

    private const int InputPinBase = 100000;
    private const int FocusPinBase = 200000;
    private const int FocusLinkBase = 300000;
    private const int AddPinBase = 400000;
    private const int CompactPinBase = 500000;
    private const int PairLinkBase = 600000;
    private const int StaticBase = 700000;

    public BhvGraphWindow(BhvEditorView view)
    {
        View = view;
    }

    private BhvSelection Selection => View.Selection;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_Graph_Header"), LOC.Get("BHV_Graph_Header_TT"));

        var file = Selection.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File"));
            return;
        }

        var b = file.BHV;

        // Small files read best whole; large ones (base: 300+ states) around the selection
        if (Depth < 0 || (Depth == 0 && b.States.Count > MaxAllStates))
            Depth = b.States.Count <= 40 ? 0 : 1;

        DrawHeader(b);

        var focus = Selection.StateIndex;
        if (Depth > 0 && focus < 0)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Graph_Select_State"));
            return;
        }

        DrawGraph(file, b, focus);
    }

    private void DrawHeader(BHV b)
    {
        ImGui.BeginChild("bhvGraphHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("BHV_Graph_Focus"));
        ImGui.SameLine();

        string DepthLabel(int d) => d == 0 ? LOC.Get("BHV_Graph_Focus_All", b.States.Count) : LOC.Get("BHV_Graph_Focus_Steps", d);

        ImGui.SetNextItemWidth(160);
        if (ImGui.BeginCombo("##bhvGraphDepth", DepthLabel(Depth)))
        {
            for (int d = 0; d <= 2; d++)
            {
                var allowed = d > 0 || b.States.Count <= MaxAllStates;

                ImGui.BeginDisabled(!allowed);
                if (ImGui.Selectable(DepthLabel(d), d == Depth))
                {
                    Depth = d;
                    FrameSelected = true;
                }
                ImGui.EndDisabled();

                if (!allowed)
                    GUI.Tooltip(LOC.Get("BHV_Graph_Focus_All_Too_Big", MaxAllStates));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("BHV_Graph_Focus_TT"));

        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("BHV_Graph_Frame")}##bhvGraphFrame"))
            FrameSelected = true;
        GUI.Tooltip(LOC.Get("BHV_Graph_Frame_TT"));

        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("BHV_Graph_Shown", Shown.Count));
        GUI.Tooltip(LOC.Get("BHV_Graph_Shown_TT", LastVertexCount, VertexSoftLimit));

        if (HiddenCount > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("BHV_Graph_Hidden", HiddenCount));
            GUI.Tooltip(LOC.Get("BHV_Graph_Hidden_TT", NodeCap));
        }

        if (HiddenLinks > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("BHV_Graph_Hidden_Links", HiddenLinks));
            GUI.Tooltip(LOC.Get("BHV_Graph_Hidden_Links_TT", LinkCap));
        }

        ImGui.EndChild();
    }

    private void DrawGraph(BhvFileWrapper file, BHV b, int focus)
    {
        if (EditorContext.IsNull)
            EditorContext = ImNodes.EditorContextCreate();

        var incoming = Enumerable.Range(0, b.States.Count).Select(i => View.Format.IncomingTransitions(file, i).Count).ToArray();
        var preambleTargets = new HashSet<int>(BhvLayout.PreambleStateSlots(b)
            .Where(o => o + 2 <= b.Preamble.Length).Select(o => (int)BitConverter.ToInt16(b.Preamble, o)));

        var layoutFocus = Depth == 0 ? -1 : focus;
        if (Layout == null || LayoutFile != file || LayoutRevision != file.Revision || LayoutFocus != layoutFocus || LayoutDepth != Depth)
        {
            if (LayoutFile != file || LayoutFocus != layoutFocus || LayoutDepth != Depth)
                FrameSelected = true;

            if (LayoutFile != file)
            {
                Budget = 1f;
                NodeSizes.Clear();
            }

            Columns = Depth == 0 ? ColumnsAll(b, incoming) : ColumnsAround(b, focus, Depth);
            Columns = Cap(Columns, focus);
            Layout = Place(Columns);
            var shown = Layout.Keys.OrderBy(i => i).ToList();

            // imnodes frees nodes and links that stop being submitted but keeps them in its selection, and link IDs
            // are renumbered here, so a stale selection would point at freed or different items. When what is drawn
            // changes, start from a fresh editor context; the graph is framed again anyway.
            if (LayoutFile != file || !shown.SequenceEqual(Shown) || focus != ShownFocus || LayoutRevision != file.Revision)
            {
                ImNodes.EditorContextFree(EditorContext);
                EditorContext = ImNodes.EditorContextCreate();
                LastGraphNode = -1;
                LastGraphLink = -1;
                SyncedState = -2;
                FrameSelected = true;
            }

            Shown = shown;
            ShownFocus = focus;
            LayoutFile = file;
            LayoutRevision = file.Revision;
            LayoutFocus = layoutFocus;
            LayoutDepth = Depth;
        }

        ImNodes.EditorContextSet(EditorContext);

        var shownSet = new HashSet<int>(Shown);
        FocusPins.Clear();
        BuildPairLinks(b, focus, shownSet);

        PaneSize = ImGui.GetContentRegionAvail();
        ImNodes.PushStyleVar(ImNodesStyleVar.LinkLineSegmentsPerLength, LinkSegmentsPerLength);
        ImNodes.BeginNodeEditor();

        // The editor's own draw list (its scrolling child); after EndNodeEditor it holds everything the graph drew
        var drawList = ImGui.GetWindowDrawList();

        foreach (var i in Shown)
            DrawNode(b, i, focus, incoming[i], preambleTargets.Contains(i), shownSet);

        // The focus state's transitions, one link each
        for (int k = 0; k < FocusPins.Count; k++)
        {
            var target = b.States[focus].Transitions[FocusPins[k]].TargetStateIndex;
            if (target != focus && shownSet.Contains(target))
                ImNodes.Link(FocusLinkBase + k, FocusPinBase + k, InputPinBase + target);
        }

        // Everyone else: one link per state led to
        for (int k = 0; k < PairLinks.Count; k++)
            ImNodes.Link(PairLinkBase + k, CompactPinBase + PairLinks[k].From, InputPinBase + PairLinks[k].To);

        // No minimap: it draws every node and link a second time into the same draw list

        ImNodes.EndNodeEditor();
        ImNodes.PopStyleVar();

        // Stay well under ImGui's 65535-vertex draw list limit: a dense graph shrinks the caps and lays out again
        LastVertexCount = drawList.VtxBuffer.Size;
        if (LastVertexCount > VertexSoftLimit && Budget > 0.15f)
        {
            Budget *= Math.Clamp(VertexSoftLimit * 0.8f / LastVertexCount, 0.3f, 0.8f);
            Layout = null;
        }

        MeasureCells();
        SyncSelection(focus);
        HandleInteraction(file, b, focus);
    }

    private void DrawNode(BHV b, int i, int focus, int incoming, bool preambleTarget, HashSet<int> shownSet)
    {
        var s = b.States[i];
        var nodeId = i + 1;
        var isFocus = i == focus;

        ImNodes.SetNodeGridSpacePos(nodeId, Layout[i]);

        // Entered by code or the preamble, not by a transition
        var codeEntered = incoming == 0;
        if (codeEntered)
            ImNodes.PushColorStyle(ImNodesCol.TitleBar, ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text * new Vector4(0.45f, 0.45f, 0.45f, 1f)));
        if (isFocus)
            ImNodes.PushColorStyle(ImNodesCol.NodeOutline, ImGui.GetColorU32(UI.Current.ImGui_Benefit_Text_Color));

        ImNodes.BeginNode(nodeId);

        ImNodes.BeginNodeTitleBar();
        ImGui.Text(LOC.Get("BHV_Graph_Node_Title", i, s.ID));
        ImNodes.EndNodeTitleBar();

        ImNodes.BeginInputAttribute(InputPinBase + i);
        var animId = BhvDisplay.AnimationId(b, BhvDisplay.StateAnimationIndex(s));
        ImGui.TextDisabled(animId >= 0
            ? LOC.Get("BHV_Graph_Node_Info", s.Kind, animId, incoming)
            : LOC.Get("BHV_Graph_Node_Info_No_Anim", s.Kind, incoming));
        ImNodes.EndInputAttribute();

        if (preambleTarget)
        {
            ImNodes.BeginStaticAttribute(StaticBase + i);
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("BHV_Graph_Node_Preamble"));
            ImNodes.EndStaticAttribute();
        }

        if (isFocus)
        {
            // Transitions in the order the game tries them
            for (int t = s.Transitions.Count - 1; t >= 0; t--)
            {
                var tr = s.Transitions[t];
                var pin = FocusPins.Count;
                FocusPins.Add(t);

                ImNodes.BeginOutputAttribute(FocusPinBase + pin);
                if (tr.TargetStateIndex == i)
                    ImGui.TextDisabled(LOC.Get("BHV_Graph_Pin_Self", t, tr.Conditions.Count));
                else if (shownSet.Contains(tr.TargetStateIndex))
                    ImGui.Text(LOC.Get("BHV_Graph_Pin", t, tr.TargetStateIndex, tr.Conditions.Count));
                else
                    ImGui.TextColored(UI.Current.ImGui_Ref_Text, LOC.Get("BHV_Graph_Pin_Hidden", t, tr.TargetStateIndex, tr.Conditions.Count));
                ImNodes.EndOutputAttribute();
            }

            ImNodes.BeginOutputAttribute(AddPinBase + i);
            ImGui.TextDisabled(LOC.Get("BHV_Graph_Pin_Add"));
            ImNodes.EndOutputAttribute();
        }
        else
        {
            ImNodes.BeginOutputAttribute(CompactPinBase + i);
            ImGui.TextDisabled(LOC.Get("BHV_Graph_Pin_Compact", s.Transitions.Count));
            ImNodes.EndOutputAttribute();
        }

        ImNodes.EndNode();
        ImNodes.SetNodeDraggable(nodeId, false);

        if (isFocus)
            ImNodes.PopColorStyle();
        if (codeEntered)
            ImNodes.PopColorStyle();
    }

    /// <summary>
    /// Compact links (not from the focus): every shown pair in All; in step mode only from one column to the next,
    /// which is how the neighbourhood was found
    /// </summary>
    private void BuildPairLinks(BHV b, int focus, HashSet<int> shownSet)
    {
        PairLinks.Clear();

        foreach (var from in Shown)
        {
            if (from == focus)
                continue;

            foreach (var to in b.States[from].Transitions.Select(t => t.TargetStateIndex).Distinct())
            {
                if (to == from || !shownSet.Contains(to))
                    continue;

                if (Depth > 0 && Columns[to] != Columns[from] + 1)
                    continue;

                PairLinks.Add((from, to));
            }
        }

        // Keep links into the focus first, then those nearest it
        HiddenLinks = Math.Max(0, PairLinks.Count - LinkCap);
        if (HiddenLinks > 0)
        {
            var kept = PairLinks
                .OrderBy(p => p.To == focus ? 0 : 1)
                .ThenBy(p => Math.Abs(Columns[p.From]) + Math.Abs(Columns[p.To]))
                .ThenBy(p => p.From).ThenBy(p => p.To)
                .Take(LinkCap).ToList();
            PairLinks.Clear();
            PairLinks.AddRange(kept);
        }
    }

    /// <summary>
    /// Records each drawn node's size; if any changed, the graph is laid out again with the real sizes (once after a
    /// change, since sizes then stay the same)
    /// </summary>
    private void MeasureCells()
    {
        var changed = false;

        foreach (var i in Shown)
        {
            var size = ImNodes.GetNodeDimensions(i + 1);
            if (!NodeSizes.TryGetValue(i, out var old) || Math.Abs(old.X - size.X) > 1f || Math.Abs(old.Y - size.Y) > 1f)
            {
                NodeSizes[i] = size;
                changed = true;
            }
        }

        if (changed && Shown.Count > 0)
            Layout = null;
    }

    /// <summary>
    /// Pushes the editor's selected state into the graph when it changed elsewhere, and pans to it on request
    /// </summary>
    private void SyncSelection(int focus)
    {
        var focusDrawn = focus >= 0 && Shown.Contains(focus);

        if (focus != SyncedState && focusDrawn)
        {
            ImNodes.ClearNodeSelection();
            ImNodes.ClearLinkSelection();
            ImNodes.SelectNode(focus + 1);
            LastGraphNode = focus + 1;
            LastGraphLink = -1;
            SyncedState = focus;
        }

        if (FrameSelected && Layout != null)
        {
            // Only ask imnodes about a node it drew this frame (it asserts on unknown IDs)
            var target = focusDrawn && Layout.TryGetValue(focus, out var p) ? p : Vector2.Zero;
            var size = focusDrawn ? ImNodes.GetNodeDimensions(focus + 1) : Vector2.Zero;

            ImNodes.EditorContextResetPanning(-target + (PaneSize - size) * 0.5f);
            FrameSelected = false;
        }
    }

    private void HandleInteraction(BhvFileWrapper file, BHV b, int focus)
    {
        int hoveredLink = 0;
        if (ImNodes.IsLinkHovered(ref hoveredLink))
        {
            if (hoveredLink >= FocusLinkBase && hoveredLink - FocusLinkBase < FocusPins.Count)
                ImGui.SetTooltip(TransitionTooltip(b, focus, FocusPins[hoveredLink - FocusLinkBase]));
            else if (hoveredLink >= PairLinkBase && hoveredLink - PairLinkBase < PairLinks.Count)
                ImGui.SetTooltip(PairTooltip(b, PairLinks[hoveredLink - PairLinkBase]));
        }

        int hoveredPin = 0;
        var pinHovered = ImNodes.IsPinHovered(ref hoveredPin);
        if (pinHovered)
        {
            if (hoveredPin >= FocusPinBase && hoveredPin - FocusPinBase < FocusPins.Count)
                ImGui.SetTooltip(TransitionTooltip(b, focus, FocusPins[hoveredPin - FocusPinBase]) + "\n\n" + LOC.Get("BHV_Graph_Pin_TT"));
            else if (hoveredPin >= AddPinBase && hoveredPin < AddPinBase + b.States.Count)
                ImGui.SetTooltip(LOC.Get("BHV_Graph_Pin_Add_TT"));
            else if (hoveredPin >= CompactPinBase && hoveredPin < CompactPinBase + b.States.Count)
                ImGui.SetTooltip(CompactTooltip(b, hoveredPin - CompactPinBase));
        }

        int hoveredNode = 0;
        if (!pinHovered && ImNodes.IsNodeHovered(ref hoveredNode) && hoveredNode >= 1 && hoveredNode <= b.States.Count
            && View.Format.IncomingTransitions(file, hoveredNode - 1).Count == 0)
        {
            ImGui.SetTooltip(LOC.Get("BHV_StateList_No_Incoming_TT"));
        }

        // Selection made in the graph goes to the other windows
        var selectedNodes = ImNodes.NumSelectedNodes();
        var selectedLinks = ImNodes.NumSelectedLinks();

        if (selectedNodes > 0)
        {
            var ids = new int[selectedNodes];
            ImNodes.GetSelectedNodes(ref ids[0]);

            if (ids[0] != LastGraphNode && ids[0] >= 1 && ids[0] <= b.States.Count)
            {
                LastGraphNode = ids[0];
                LastGraphLink = -1;
                SyncedState = ids[0] - 1;
                Selection.SelectState(ids[0] - 1, true);
            }
        }
        else if (selectedLinks > 0)
        {
            var ids = new int[selectedLinks];
            ImNodes.GetSelectedLinks(ref ids[0]);

            if (ids[0] != LastGraphLink)
            {
                LastGraphLink = ids[0];
                LastGraphNode = -1;

                if (ids[0] >= FocusLinkBase && ids[0] - FocusLinkBase < FocusPins.Count)
                {
                    SyncedState = focus;
                    Selection.SelectTransition(FocusPins[ids[0] - FocusLinkBase]);
                }
                else if (ids[0] >= PairLinkBase && ids[0] - PairLinkBase < PairLinks.Count)
                {
                    // The transition to that state the game tries first
                    var (from, to) = PairLinks[ids[0] - PairLinkBase];
                    var t = b.States[from].Transitions.FindLastIndex(x => x.TargetStateIndex == to);
                    SyncedState = from;
                    Selection.SelectState(from, true);
                    Selection.SelectTransition(t);
                }
            }
        }
        else
        {
            LastGraphNode = -1;
            LastGraphLink = -1;
        }

        // A link dragged from a pin to a node
        int startPin = 0, endPin = 0;
        if (!ImNodes.IsLinkCreated(ref startPin, ref endPin))
            return;

        // Dragged the other way round
        if (startPin >= InputPinBase && startPin < FocusPinBase)
            (startPin, endPin) = (endPin, startPin);

        var to2 = endPin - InputPinBase;
        if (to2 < 0 || to2 >= b.States.Count)
            return;

        if (startPin >= AddPinBase && startPin < AddPinBase + b.States.Count)
        {
            View.Structure.AddTransition(file, startPin - AddPinBase, to2);
        }
        else if (startPin >= CompactPinBase && startPin < CompactPinBase + b.States.Count)
        {
            View.Structure.AddTransition(file, startPin - CompactPinBase, to2);
        }
        else if (startPin >= FocusPinBase && startPin - FocusPinBase < FocusPins.Count)
        {
            var t = FocusPins[startPin - FocusPinBase];
            var transition = b.States[focus].Transitions[t];

            if (transition.TargetStateIndex != to2)
            {
                View.SetValue(file, () => transition.TargetStateIndex, v => transition.TargetStateIndex = v, to2);
                Selection.SelectTransition(t);
            }
        }
    }

    #region Tooltips
    private string TransitionTooltip(BHV b, int from, int t)
    {
        var tr = b.States[from].Transitions[t];
        var lines = new List<string>
        {
            LOC.Get("BHV_Graph_Link_TT", t, BhvDisplay.StateLabel(b, from), BhvDisplay.StateLabel(b, tr.TargetStateIndex)),
        };

        if (b.Version >= 10 && tr.Params.Length >= 8)
        {
            var matrix = BitConverter.ToInt32(tr.Params, BHV.V12.ParamsChangeStateMatrixId);
            if (matrix != 0)
                lines.Add(LOC.Get("BHV_TransitionView_Matrix", matrix));
        }

        for (int c = 0; c < tr.Conditions.Count; c++)
        {
            var cond = tr.Conditions[c];
            var text = LOC.Get("BHV_TransitionView_Condition", c, cond.Type, cond.Data.Length);

            var field = BhvLayout.ConditionDataFields(b, cond).FirstOrDefault();
            var value = field?.Read(cond.Data);
            if (value != null)
            {
                text += $"  {LOC.Get(field.LabelKey)} = {BhvDisplay.FormatValue(field, value.Value)}";
                var meaning = View.Format.Meaning(b, field, value.Value);
                if (meaning.Length > 0)
                    text += $" ({meaning})";
            }

            if (cond.Unk02 != 0)
                text += "  " + LOC.Get("BHV_TransitionView_Optional");

            lines.Add(text);
        }

        return string.Join("\n", lines);
    }

    /// <summary>A compact pin: the state's transitions in game order (up to 30)</summary>
    private static string CompactTooltip(BHV b, int state)
    {
        var s = b.States[state];
        var lines = new List<string> { LOC.Get("BHV_Graph_Compact_TT", BhvDisplay.StateLabel(b, state)) };

        var shown = 0;
        for (int t = s.Transitions.Count - 1; t >= 0 && shown < 30; t--, shown++)
        {
            var tr = s.Transitions[t];
            lines.Add(tr.TargetStateIndex == state
                ? LOC.Get("BHV_Graph_Pin_Self", t, tr.Conditions.Count)
                : LOC.Get("BHV_Graph_Pin", t, tr.TargetStateIndex, tr.Conditions.Count));
        }

        if (s.Transitions.Count > 30)
            lines.Add(LOC.Get("BHV_Graph_More", s.Transitions.Count - 30));

        return string.Join("\n", lines);
    }

    /// <summary>A compact link: the transitions between the two states, in game order</summary>
    private static string PairTooltip(BHV b, (int From, int To) pair)
    {
        var transitions = b.States[pair.From].Transitions;
        var lines = new List<string> { $"{BhvDisplay.StateLabel(b, pair.From)} → {BhvDisplay.StateLabel(b, pair.To)}" };

        for (int t = transitions.Count - 1; t >= 0; t--)
            if (transitions[t].TargetStateIndex == pair.To)
                lines.Add(LOC.Get("BHV_Graph_Pin", t, pair.To, transitions[t].Conditions.Count));

        lines.Add(LOC.Get("BHV_Graph_Pair_TT"));
        return string.Join("\n", lines);
    }
    #endregion

    #region Layout
    /// <summary>
    /// The states within <paramref name="depth"/> steps of the focus: the focus in column 0, states it leads to in
    /// columns 1..depth, states leading into it (and not reached forwards) in columns -1..-depth
    /// </summary>
    private static Dictionary<int, int> ColumnsAround(BHV b, int focus, int depth)
    {
        var column = new Dictionary<int, int> { [focus] = 0 };

        var frontier = new List<int> { focus };
        for (int d = 1; d <= depth; d++)
        {
            var next = new List<int>();
            foreach (var s in frontier)
                foreach (var t in b.States[s].Transitions)
                    if (t.TargetStateIndex >= 0 && t.TargetStateIndex < b.States.Count && column.TryAdd(t.TargetStateIndex, d))
                        next.Add(t.TargetStateIndex);
            frontier = next;
        }

        var sources = Sources(b);
        frontier = new List<int> { focus };
        for (int d = 1; d <= depth; d++)
        {
            var next = new List<int>();
            foreach (var s in frontier)
                foreach (var from in sources[s])
                    if (column.TryAdd(from, -d))
                        next.Add(from);
            frontier = next;
        }

        return column;
    }

    /// <summary>
    /// The whole file in layers: from the states nothing leads to (entered by code), along the transitions;
    /// states only reachable in cycles start new layer runs
    /// </summary>
    private static Dictionary<int, int> ColumnsAll(BHV b, int[] incoming)
    {
        var column = new Dictionary<int, int>();

        void Spread(IEnumerable<int> starts)
        {
            var queue = new Queue<int>();
            foreach (var s in starts)
                if (column.TryAdd(s, 0))
                    queue.Enqueue(s);

            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                foreach (var t in b.States[s].Transitions.Select(t => t.TargetStateIndex).Distinct().OrderBy(x => x))
                    if (t >= 0 && t < b.States.Count && column.TryAdd(t, column[s] + 1))
                        queue.Enqueue(t);
            }
        }

        Spread(Enumerable.Range(0, b.States.Count).Where(i => incoming[i] == 0));

        for (int i = 0; i < b.States.Count; i++)
            if (!column.ContainsKey(i))
                Spread([i]);

        return column;
    }

    /// <summary>Keeps at most <see cref="MaxNodes"/> states: the focus, then the nearest columns, then lowest index</summary>
    private Dictionary<int, int> Cap(Dictionary<int, int> column, int focus)
    {
        HiddenCount = Math.Max(0, column.Count - NodeCap);
        if (HiddenCount == 0)
            return column;

        return column
            .OrderBy(kv => kv.Key == focus ? 0 : 1)
            .ThenBy(kv => Math.Abs(kv.Value))
            .ThenBy(kv => kv.Key)
            .Take(NodeCap)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <summary>
    /// Columns left to right, states in index order within each. A column of more than <see cref="MaxRows"/> states
    /// wraps into side-by-side sub-columns (away from the focus on the left side), so the graph stays short and
    /// links stay short: imnodes gives a link one segment per few pixels of its length, so height costs vertices.
    /// Nodes stack by their own measured height (the focus node can be 900+ px with 45 pins).
    /// </summary>
    private Dictionary<int, Vector2> Place(Dictionary<int, int> column)
    {
        var layout = new Dictionary<int, Vector2>();
        var pad = CellPadding * DPI.UIScale();

        Vector2 Size(int state) => NodeSizes.TryGetValue(state, out var s) && s.X > 0 ? s : DefaultNodeSize;

        var x = 0f;
        foreach (var group in column.GroupBy(kv => kv.Value).OrderBy(g => g.Key))
        {
            var states = group.Select(kv => kv.Key).OrderBy(s => s).ToList();
            var subColumns = (states.Count + MaxRows - 1) / MaxRows;

            for (int sc = 0; sc < subColumns; sc++)
            {
                // On the left of the focus, the first states go in the sub-column nearest it (the rightmost)
                var slot = group.Key < 0 ? subColumns - 1 - sc : sc;
                var members = states.Skip(slot * MaxRows).Take(MaxRows).ToList();

                var y = 0f;
                var width = 0f;
                foreach (var state in members)
                {
                    layout[state] = new Vector2(x, y);
                    y += Size(state).Y + pad.Y;
                    width = Math.Max(width, Size(state).X);
                }

                x += width + pad.X;
            }
        }

        return layout;
    }

    private static List<int>[] Sources(BHV b)
    {
        var sources = new List<int>[b.States.Count];
        for (int i = 0; i < sources.Length; i++)
            sources[i] = new();

        for (int s = 0; s < b.States.Count; s++)
            foreach (var t in b.States[s].Transitions)
                if (t.TargetStateIndex >= 0 && t.TargetStateIndex < b.States.Count && t.TargetStateIndex != s && !sources[t.TargetStateIndex].Contains(s))
                    sources[t.TargetStateIndex].Add(s);

        return sources;
    }
    #endregion
}
