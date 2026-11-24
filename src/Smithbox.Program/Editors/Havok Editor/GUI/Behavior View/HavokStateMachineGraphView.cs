using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using StudioCore.Editors.Common;
using System.Collections;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// A state machine as a node graph: one node per state, a link per pair of states with transitions between them.
///
/// Wildcard transitions aren't drawn as links: in about 96% of the game's state machines they reach every state
/// (Tools/BehaviorSurvey, ER/NR/DS3), so an "any state" node would be linked to everything. Each node lists the
/// events whose wildcard transitions lead to it instead. 80% of machines have no state-to-state links at all.
///
/// The machine shown is the one the selection is in (from any tab). Double-click a state whose generator is a
/// state machine to open it. Drag from a state's output to another state's input to add a transition.
/// The nodes are static: laid out automatically (linked states in layers along their transitions, the rest in a grid),
/// with the cells sized to the largest node, so nothing overlaps.
/// </summary>
public class HavokStateMachineGraphView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    private ImNodesEditorContextPtr EditorContext;

    // The machine shown, and the machines opened from it (for Back)
    private object StateMachine;
    private readonly Stack<object> OpenedFrom = new();
    private bool FollowSelection = true;
    private object LastFollowed;

    // The layout of the shown machine (grid space), and the cell size it was made with.
    // The cell size is measured from the drawn nodes, so the layout is redone once after a machine is first drawn.
    private Dictionary<object, Vector2> Layout;
    private Vector2 CellSize = new(280f, 150f);
    private bool LayoutDirty = true;

    private static readonly Vector2 CellPadding = new(60f, 30f);

    // Selection in the graph: a state, or the transitions of a link
    private object SelectedState;
    private List<object> SelectedTransitions;

    // Drawn this frame, by ID
    private readonly List<object> NodeStates = new();
    private readonly List<(int From, int To, List<object> Transitions)> Links = new();

    private const int InputPinBase = 100000;
    private const int OutputPinBase = 200000;
    private const int LinkBase = 300000;

    // The "Any state" stub: drawn only for the wildcards asked for (the selected state's, or the highlighted event's),
    // since in most machines wildcards reach every state
    private const int AnyNodeId = 1000000;
    private const int AnyPin = 1100000;
    private const int WildcardLinkBase = 1200000;
    private readonly List<(int To, List<object> Transitions)> WildcardLinks = new();

    // Event highlight: transitions on events whose name contains the text
    private string EventHighlight = "";

    // Find a state: centres the view on the next match
    private string StateFind = "";
    private string StateFindFor;
    private List<object> StateFindMatches = new();
    private int StateFindIndex = -1;
    private object CenterState;
    private Vector2 PaneSize;


    public HavokStateMachineGraphView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        StateMachine = null;
        OpenedFrom.Clear();
        SelectedState = null;
        SelectedTransitions = null;
        FollowSelection = true;
        LayoutDirty = true;
    }

    public void Setup(object sourceObject)
    {
        var symbols = Owner.Symbols;

        if (symbols == null || !symbols.Contains(StateMachine))
        {
            StateMachine = null;
            OpenedFrom.Clear();
        }

        if (symbols == null || !symbols.Contains(SelectedState))
            SelectedState = null;

        SelectedTransitions = null;

        // States may have been added or removed
        LayoutDirty = true;
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Draws the Graph window: the shown machine's header and graph
    /// </summary>
    public void DisplayWindow()
    {
        if (Owner.Symbols == null)
        {
            GUI.WrappedText(LOC.Get("HAVOK_GraphView_No_Behavior"));
            return;
        }

        UpdateShownMachine();
        DisplayTabHeader();
        DisplayGraph();
    }

    /// <summary>
    /// Follows the selection: the machine the selected object is in (or the selected machine itself)
    /// </summary>
    private void UpdateShownMachine()
    {
        if (!FollowSelection)
            return;

        // Only when the Contents selection changes, so a machine opened in the graph stays shown
        var selected = Owner.GetContentsSelectedObject();

        if (ReferenceEquals(selected, LastFollowed) && StateMachine != null)
            return;

        LastFollowed = selected;

        var machine = Owner.Symbols.FindOwningStateMachine(selected);

        // A state's own machine is above it; a selected machine is shown itself
        if (machine == null && StateMachine == null)
            machine = Owner.Symbols.Objects.FirstOrDefault(o => o.GetType().Name == "hkbStateMachine");

        if (machine != null && !ReferenceEquals(machine, StateMachine))
            Show(machine, rememberCurrent: false);
    }

    private void Show(object machine, bool rememberCurrent)
    {
        if (rememberCurrent && StateMachine != null)
            OpenedFrom.Push(StateMachine);

        StateMachine = machine;
        SelectedState = null;
        SelectedTransitions = null;
        LayoutDirty = true;
    }

    private void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        ImGui.BeginDisabled(OpenedFrom.Count == 0);
        if (ImGui.ArrowButton("##graphBack", ImGuiDir.Left))
        {
            StateMachine = OpenedFrom.Pop();
            SelectedState = null;
            SelectedTransitions = null;
            LayoutDirty = true;
            FollowSelection = false;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_GraphView_Back_TT"));

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(StateMachine == null ? LOC.Get("HAVOK_GraphView_No_Machine") : HavokBehaviorSymbols.GetOutlineLabel(StateMachine));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("HAVOK_GraphView_Follow")}##graphFollow", ref FollowSelection);
        GUI.Tooltip(LOC.Get("HAVOK_GraphView_Follow_TT"));

        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("HAVOK_UsedBy_Show_In_Outline")}##graphShowInOutline") && StateMachine != null)
            Owner.OutlineView.Navigate(SelectedState ?? StateMachine);

        ImGui.EndChild();

        if (StateMachine == null)
            return;

        DisplayBreadcrumb();

        // Highlight an event
        ImGui.SetNextItemWidth(200f * DPI.UIScale());
        ImGui.InputTextWithHint("##graphEventHighlight", LOC.Get("HAVOK_GraphView_Highlight_Hint"), ref EventHighlight, 255);
        GUI.Tooltip(LOC.Get("HAVOK_GraphView_Highlight_TT"));

        // Find a state
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f * DPI.UIScale());
        ImGui.InputTextWithHint("##graphStateFind", LOC.Get("HAVOK_GraphView_Find_Hint"), ref StateFind, 255);
        GUI.Tooltip(LOC.Get("HAVOK_GraphView_Find_TT"));

        UpdateStateFind();

        ImGui.SameLine();
        ImGui.BeginDisabled(StateFindMatches.Count == 0);
        if (ImGui.ArrowButton("##graphFindNext", ImGuiDir.Down))
            StepStateFind();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_GraphView_Find_Next_TT"));

        if (StateFind.Length > 0)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(LOC.Get("HAVOK_Script_Find_Count", StateFindIndex + 1, StateFindMatches.Count));
        }
    }

    /// <summary>
    /// The machines above the shown one, outermost first; clicking one shows it (Back returns)
    /// </summary>
    private void DisplayBreadcrumb()
    {
        var path = Owner.GetBreadcrumb(StateMachine).Where(n => n.GetType().Name == "hkbStateMachine").ToList();

        if (path.Count == 0)
            return;

        ImGui.AlignTextToFramePadding();

        for (var i = 0; i < path.Count; i++)
        {
            if (i > 0)
            {
                ImGui.SameLine(0, 2);
                ImGui.TextDisabled("›");
                ImGui.SameLine(0, 2);
            }

            if (ImGui.SmallButton($"{HavokBehaviorSymbols.GetValue(path[i], "m_name") ?? "?"}##graphCrumb{i}"))
            {
                FollowSelection = false;
                Show(path[i], rememberCurrent: true);
            }
        }

        ImGui.SameLine(0, 2);
        ImGui.TextDisabled("›");
        ImGui.SameLine(0, 2);
        ImGui.Text(HavokBehaviorSymbols.GetValue(StateMachine, "m_name") as string ?? "?");
    }

    private void UpdateStateFind()
    {
        if (StateFind == StateFindFor)
            return;

        StateFindFor = StateFind;
        StateFindMatches = new List<object>();
        StateFindIndex = -1;

        if (StateFind.Length == 0 || HavokBehaviorSymbols.GetValue(StateMachine, "m_states") is not IList states)
            return;

        foreach (var state in states)
        {
            if (state != null && (HavokBehaviorSymbols.GetValue(state, "m_name") as string ?? "").Contains(StateFind, StringComparison.OrdinalIgnoreCase))
                StateFindMatches.Add(state);
        }

        // The first match at once
        StepStateFind();
    }

    private void StepStateFind()
    {
        if (StateFindMatches.Count == 0)
            return;

        StateFindIndex = (StateFindIndex + 1) % StateFindMatches.Count;
        CenterState = StateFindMatches[StateFindIndex];
    }

    /// <summary>
    /// Whether a transition's event name contains the highlight text
    /// </summary>
    private bool IsHighlighted(object transition)
    {
        if (EventHighlight.Length == 0)
            return false;

        var name = HavokBehaviorSymbols.GetName(Owner.Symbols.EventNames, Convert.ToInt32(HavokBehaviorSymbols.GetValue(transition, "m_eventId") ?? -1));

        return name != null && name.Contains(EventHighlight, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The wildcard transitions to draw from the Any stub: those into the selected state, and the highlighted ones
    /// </summary>
    private void BuildWildcardLinks(List<object> states)
    {
        WildcardLinks.Clear();

        var wildcards = HavokBehaviorSymbols.GetValue(HavokBehaviorSymbols.GetValue(StateMachine, "m_wildcardTransitions"), "m_transitions") as IList;

        if (wildcards == null)
            return;

        var selectedId = SelectedState == null ? int.MinValue : Convert.ToInt32(HavokBehaviorSymbols.GetValue(SelectedState, "m_stateId") ?? -1);
        var byTarget = new Dictionary<int, List<object>>();

        foreach (var transition in wildcards)
        {
            if (transition == null)
                continue;

            var to = Convert.ToInt32(HavokBehaviorSymbols.GetValue(transition, "m_toStateId") ?? -1);

            if (to != selectedId && !IsHighlighted(transition))
                continue;

            if (!byTarget.TryGetValue(to, out var list))
                byTarget[to] = list = new List<object>();

            list.Add(transition);
        }

        for (var i = 0; i < states.Count; i++)
        {
            var id = Convert.ToInt32(HavokBehaviorSymbols.GetValue(states[i], "m_stateId") ?? -1);

            if (byTarget.TryGetValue(id, out var list))
                WildcardLinks.Add((i, list));
        }
    }

    #region Graph
    private void DisplayGraph()
    {
        if (StateMachine == null)
            return;

        if (EditorContext.IsNull)
            EditorContext = ImNodes.EditorContextCreate();

        ImNodes.EditorContextSet(EditorContext);

        var symbols = Owner.Symbols;
        var states = (HavokBehaviorSymbols.GetValue(StateMachine, "m_states") as IList)?.Cast<object>().Where(s => s != null).ToList() ?? new();
        var startStateId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(StateMachine, "m_startStateId") ?? -1);
        var wildcardEvents = GetWildcardEvents(StateMachine);

        BuildLinks(states);
        BuildWildcardLinks(states);

        // States a highlighted transition leads to
        var highlightedTargets = new HashSet<int>();

        foreach (var (_, to, transitions) in Links)
        {
            if (transitions.Any(IsHighlighted))
                highlightedTargets.Add(to);
        }

        foreach (var (to, transitions) in WildcardLinks)
        {
            if (transitions.Any(IsHighlighted))
                highlightedTargets.Add(to);
        }

        if (LayoutDirty || Layout == null || Layout.Count != states.Count)
        {
            Layout = AutoLayout(states);
            LayoutDirty = false;
        }

        PaneSize = ImGui.GetContentRegionAvail();

        ImNodes.BeginNodeEditor();

        NodeStates.Clear();
        NodeStates.AddRange(states);

        for (var i = 0; i < states.Count; i++)
        {
            var state = states[i];
            var nodeId = i + 1;

            // Static: placed by the layout every frame, and not draggable
            ImNodes.SetNodeGridSpacePos(nodeId, Layout.TryGetValue(state, out var position) ? position : Vector2.Zero);

            var stateId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(state, "m_stateId") ?? -1);
            var isStart = stateId == startStateId;
            var isHighlighted = highlightedTargets.Contains(i);
            var titleColoured = isHighlighted || isStart;

            // A state a highlighted event leads to, else the start state
            if (isHighlighted)
                ImNodes.PushColorStyle(ImNodesCol.TitleBar, ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text * new Vector4(0.6f, 0.6f, 0.6f, 1f)));
            else if (isStart)
                ImNodes.PushColorStyle(ImNodesCol.TitleBar, ImGui.GetColorU32(UI.Current.ImGui_Benefit_Text_Color * new Vector4(0.6f, 0.6f, 0.6f, 1f)));

            ImNodes.BeginNode(nodeId);

            ImNodes.BeginNodeTitleBar();
            ImGui.Text(HavokBehaviorSymbols.GetOutlineLabel(state));
            ImNodes.EndNodeTitleBar();

            ImNodes.BeginInputAttribute(InputPinBase + i);
            var generator = HavokBehaviorSymbols.GetValue(state, "m_generator");
            ImGui.TextDisabled(generator == null ? "-" : $"{HavokBehaviorSymbols.GetOutlineTag(generator)}: {HavokBehaviorSymbols.GetValue(generator, "m_name") ?? ""}");
            ImNodes.EndInputAttribute();

            // Wildcard events leading here
            if (wildcardEvents.TryGetValue(stateId, out var events) && events.Count > 0)
            {
                ImNodes.BeginStaticAttribute(OutputPinBase + 50000 + i);
                var names = events.Select(e => HavokBehaviorSymbols.GetName(symbols.EventNames, e) ?? $"#{e}").ToList();
                var text = string.Join(", ", names.Take(2)) + (names.Count > 2 ? $" +{names.Count - 2}" : "");
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, $"* {text}");
                ImNodes.EndStaticAttribute();
            }

            ImNodes.BeginOutputAttribute(OutputPinBase + i);
            ImGui.TextDisabled(LOC.Get("HAVOK_GraphView_Out"));
            ImNodes.EndOutputAttribute();

            ImNodes.EndNode();
            ImNodes.SetNodeDraggable(nodeId, false);

            if (titleColoured)
                ImNodes.PopColorStyle();
        }

        var highlightColour = ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text);

        for (var k = 0; k < Links.Count; k++)
        {
            var highlighted = Links[k].Transitions.Any(IsHighlighted);

            if (highlighted)
                ImNodes.PushColorStyle(ImNodesCol.Link, highlightColour);

            ImNodes.Link(LinkBase + k, OutputPinBase + Links[k].From, InputPinBase + Links[k].To);

            if (highlighted)
                ImNodes.PopColorStyle();
        }

        DisplayAnyNode(states, highlightColour);

        ImNodes.MiniMap(0.15f, ImNodesMiniMapLocation.BottomRight);

        ImNodes.EndNodeEditor();

        // Size the cells to the largest node drawn, so no two overlap; a change lays the machine out again
        var largest = Vector2.Zero;

        for (var i = 0; i < states.Count; i++)
            largest = Vector2.Max(largest, ImNodes.GetNodeDimensions(i + 1));

        var cell = largest + CellPadding * DPI.UIScale();

        if (states.Count > 0 && (Math.Abs(cell.X - CellSize.X) > 1f || Math.Abs(cell.Y - CellSize.Y) > 1f))
        {
            CellSize = cell;
            LayoutDirty = true;
        }

        CenterOnRequested(states);

        HandleInteraction(states, wildcardEvents);
    }

    /// <summary>
    /// The Any stub, left of the states its wildcard links lead to, with one link per target state
    /// </summary>
    private void DisplayAnyNode(List<object> states, uint highlightColour)
    {
        if (WildcardLinks.Count == 0)
            return;

        var minX = Layout.Values.Count == 0 ? 0 : Layout.Values.Min(p => p.X);
        var targets = WildcardLinks.Select(w => Layout.TryGetValue(states[w.To], out var p) ? p.Y : 0).ToList();

        ImNodes.SetNodeGridSpacePos(AnyNodeId, new Vector2(minX - CellSize.X * 0.8f, targets.Average()));

        ImNodes.PushColorStyle(ImNodesCol.TitleBar, ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text * new Vector4(0.45f, 0.45f, 0.45f, 1f)));
        ImNodes.BeginNode(AnyNodeId);

        ImNodes.BeginNodeTitleBar();
        ImGui.Text(LOC.Get("HAVOK_GraphView_Any_State"));
        ImNodes.EndNodeTitleBar();

        ImNodes.BeginOutputAttribute(AnyPin);
        ImGui.TextDisabled(LOC.Get("HAVOK_GraphView_Any_Count", WildcardLinks.Sum(w => w.Transitions.Count)));
        ImNodes.EndOutputAttribute();

        ImNodes.EndNode();
        ImNodes.PopColorStyle();
        ImNodes.SetNodeDraggable(AnyNodeId, false);

        for (var k = 0; k < WildcardLinks.Count; k++)
        {
            ImNodes.PushColorStyle(ImNodesCol.Link, highlightColour);
            ImNodes.Link(WildcardLinkBase + k, AnyPin, InputPinBase + WildcardLinks[k].To);
            ImNodes.PopColorStyle();
        }
    }

    /// <summary>
    /// Centres the view on, and selects, the state the find asked for (after the nodes are drawn: imnodes only knows
    /// the nodes it drew this frame)
    /// </summary>
    private void CenterOnRequested(List<object> states)
    {
        if (CenterState == null)
            return;

        var index = states.IndexOf(CenterState);
        var state = CenterState;
        CenterState = null;

        if (index < 0 || !Layout.TryGetValue(state, out var target))
            return;

        var size = ImNodes.GetNodeDimensions(index + 1);
        ImNodes.EditorContextResetPanning(-target + (PaneSize - size) * 0.5f);

        ImNodes.ClearNodeSelection();
        ImNodes.ClearLinkSelection();
        ImNodes.SelectNode(index + 1);

        SelectedState = state;
        SelectedTransitions = null;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Graph;
    }

    private void HandleInteraction(List<object> states, Dictionary<int, List<int>> wildcardEvents)
    {
        var symbols = Owner.Symbols;

        // Hover: a node's wildcard events, a link's events
        int hoveredNode = 0, hoveredLink = 0;

        if (ImNodes.IsNodeHovered(ref hoveredNode) && hoveredNode >= 1 && hoveredNode <= states.Count)
        {
            var state = states[hoveredNode - 1];
            var stateId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(state, "m_stateId") ?? -1);

            if (wildcardEvents.TryGetValue(stateId, out var events) && events.Count > 0)
            {
                ImGui.SetTooltip(LOC.Get("HAVOK_GraphView_Wildcards_TT", events.Count) + "\n" +
                    string.Join("\n", events.Take(40).Select(e => HavokBehaviorSymbols.GetName(symbols.EventNames, e) ?? $"#{e}")) +
                    (events.Count > 40 ? "\n..." : ""));
            }

            // Double-click a state with a nested machine to open it
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                var generator = HavokBehaviorSymbols.GetValue(state, "m_generator");

                if (generator?.GetType().Name == "hkbStateMachine")
                {
                    FollowSelection = false;
                    Show(generator, rememberCurrent: true);
                    return;
                }
            }
        }
        else if (ImNodes.IsLinkHovered(ref hoveredLink) && hoveredLink >= WildcardLinkBase && hoveredLink - WildcardLinkBase < WildcardLinks.Count)
        {
            var wildcard = WildcardLinks[hoveredLink - WildcardLinkBase];
            ImGui.SetTooltip(LOC.Get("HAVOK_GraphView_Wildcards_TT", wildcard.Transitions.Count) + "\n" +
                string.Join("\n", wildcard.Transitions.Select(t =>
                    HavokBehaviorSymbols.GetName(symbols.EventNames, Convert.ToInt32(HavokBehaviorSymbols.GetValue(t, "m_eventId") ?? -1)) ?? "?")));
        }
        else if (ImNodes.IsLinkHovered(ref hoveredLink) && hoveredLink >= LinkBase && hoveredLink - LinkBase < Links.Count)
        {
            var link = Links[hoveredLink - LinkBase];
            ImGui.SetTooltip(LOC.Get("HAVOK_GraphView_Link_TT", link.Transitions.Count) + "\n" +
                string.Join("\n", link.Transitions.Select(t =>
                    HavokBehaviorSymbols.GetName(symbols.EventNames, Convert.ToInt32(HavokBehaviorSymbols.GetValue(t, "m_eventId") ?? -1)) ?? "?")));
        }

        // Selection
        var selectedNodes = ImNodes.NumSelectedNodes();
        var selectedLinks = ImNodes.NumSelectedLinks();

        if (selectedNodes > 0)
        {
            var ids = new int[selectedNodes];
            ImNodes.GetSelectedNodes(ref ids[0]);

            // The Any stub isn't a state: clicking it keeps the selection (and so its links)
            if (ids[0] != AnyNodeId)
            {
                var index = ids[0] - 1;
                SelectedState = index >= 0 && index < states.Count ? states[index] : null;
                SelectedTransitions = null;
            }
        }
        else if (selectedLinks > 0)
        {
            var ids = new int[selectedLinks];
            ImNodes.GetSelectedLinks(ref ids[0]);

            if (ids[0] >= WildcardLinkBase)
            {
                // A wildcard link: its transitions, keeping the state so the link stays drawn
                var wildcardIndex = ids[0] - WildcardLinkBase;
                SelectedTransitions = wildcardIndex < WildcardLinks.Count ? WildcardLinks[wildcardIndex].Transitions : null;
            }
            else
            {
                var index = ids[0] - LinkBase;
                SelectedTransitions = index >= 0 && index < Links.Count ? Links[index].Transitions : null;
                SelectedState = null;
            }
        }

        // A link dragged from a state's output to a state's input: add a transition between them
        int startPin = 0, endPin = 0;

        if (ImNodes.IsLinkCreated(ref startPin, ref endPin))
        {
            var from = startPin - OutputPinBase;
            var to = endPin - InputPinBase;

            // Dragged the other way round
            if (from < 0 || from >= states.Count)
            {
                from = endPin - OutputPinBase;
                to = startPin - InputPinBase;
            }

            if (from >= 0 && from < states.Count && to >= 0 && to < states.Count)
                Owner.ActionsView.RequestAddTransition(StateMachine, states[from], states[to]);
        }
    }

    /// <summary>
    /// The machine's state-to-state transitions, one link per (from, to) pair
    /// </summary>
    private void BuildLinks(List<object> states)
    {
        Links.Clear();

        var indexById = new Dictionary<int, int>();

        for (var i = 0; i < states.Count; i++)
            indexById.TryAdd(Convert.ToInt32(HavokBehaviorSymbols.GetValue(states[i], "m_stateId") ?? -1), i);

        var pairs = new Dictionary<(int, int), List<object>>();

        for (var i = 0; i < states.Count; i++)
        {
            var transitions = HavokBehaviorSymbols.GetValue(HavokBehaviorSymbols.GetValue(states[i], "m_transitions"), "m_transitions") as IList;

            if (transitions == null)
                continue;

            foreach (var transition in transitions)
            {
                if (transition == null)
                    continue;

                var to = Convert.ToInt32(HavokBehaviorSymbols.GetValue(transition, "m_toStateId") ?? -1);

                if (!indexById.TryGetValue(to, out var toIndex))
                    continue;

                if (!pairs.TryGetValue((i, toIndex), out var list))
                {
                    list = new List<object>();
                    pairs[(i, toIndex)] = list;
                }

                list.Add(transition);
            }
        }

        foreach (var ((from, to), list) in pairs)
            Links.Add((from, to, list));
    }

    /// <summary>
    /// For each state ID, the events of the wildcard transitions that lead to it
    /// </summary>
    private static Dictionary<int, List<int>> GetWildcardEvents(object stateMachine)
    {
        var result = new Dictionary<int, List<int>>();
        var wildcards = HavokBehaviorSymbols.GetValue(HavokBehaviorSymbols.GetValue(stateMachine, "m_wildcardTransitions"), "m_transitions") as IList;

        if (wildcards == null)
            return result;

        foreach (var transition in wildcards)
        {
            if (transition == null)
                continue;

            var to = Convert.ToInt32(HavokBehaviorSymbols.GetValue(transition, "m_toStateId") ?? -1);
            var eventId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(transition, "m_eventId") ?? -1);

            if (!result.TryGetValue(to, out var list))
            {
                list = new List<int>();
                result[to] = list;
            }

            if (!list.Contains(eventId))
                list.Add(eventId);
        }

        return result;
    }

    /// <summary>
    /// The machine's layout, in cells of the largest node's size:
    /// - each group of states linked by transitions in layers, left to right along the transitions
    ///   (from the states nothing leads to, or the first state of a cycle), the groups stacked top to bottom;
    /// - below them, the states with no links (most states of most machines) in a grid, in state order,
    ///   with the start state first.
    /// </summary>
    private Dictionary<object, Vector2> AutoLayout(List<object> states)
    {
        var layout = new Dictionary<object, Vector2>(ReferenceEqualityComparer.Instance);

        var outgoing = new Dictionary<int, List<int>>();
        var incoming = new Dictionary<int, int>();
        var undirected = new Dictionary<int, List<int>>();

        foreach (var (from, to, _) in Links)
        {
            if (from == to)
                continue;

            if (!outgoing.TryGetValue(from, out var o)) outgoing[from] = o = new List<int>();
            if (!o.Contains(to)) o.Add(to);

            incoming[to] = incoming.GetValueOrDefault(to) + 1;

            if (!undirected.TryGetValue(from, out var a)) undirected[from] = a = new List<int>();
            if (!undirected.TryGetValue(to, out var b)) undirected[to] = b = new List<int>();
            a.Add(to);
            b.Add(from);
        }

        var placed = new HashSet<int>();
        var y = 0f;

        // Linked groups
        for (var i = 0; i < states.Count; i++)
        {
            if (placed.Contains(i) || !undirected.ContainsKey(i))
                continue;

            // The group: every state reachable through links either way
            var group = new List<int>();
            var queue = new Queue<int>();
            var inGroup = new HashSet<int> { i };
            queue.Enqueue(i);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                group.Add(current);

                foreach (var n in undirected[current])
                {
                    if (inGroup.Add(n))
                        queue.Enqueue(n);
                }
            }

            group.Sort();

            // Layers: the distance along the transitions from the group's sources
            var layer = new Dictionary<int, int>();
            var sources = group.Where(s => !incoming.ContainsKey(s)).ToList();

            if (sources.Count == 0)
                sources.Add(group[0]);

            void Spread(IEnumerable<int> starts)
            {
                var frontier = new Queue<int>();

                foreach (var s in starts)
                {
                    if (layer.TryAdd(s, 0))
                        frontier.Enqueue(s);
                }

                while (frontier.Count > 0)
                {
                    var current = frontier.Dequeue();

                    if (!outgoing.TryGetValue(current, out var next))
                        continue;

                    foreach (var n in next.OrderBy(n => n))
                    {
                        if (layer.TryAdd(n, layer[current] + 1))
                            frontier.Enqueue(n);
                    }
                }
            }

            Spread(sources);

            // States only reachable through a cycle that doesn't include a source
            foreach (var s in group.Where(s => !layer.ContainsKey(s)).ToList())
                Spread(new[] { s });

            var rowsInLayer = new Dictionary<int, int>();
            var rows = 0;

            foreach (var s in group.OrderBy(s => layer[s]).ThenBy(s => s))
            {
                var row = rowsInLayer.GetValueOrDefault(layer[s]);
                rowsInLayer[layer[s]] = row + 1;
                rows = Math.Max(rows, row + 1);

                layout[states[s]] = new Vector2(layer[s] * CellSize.X, y + row * CellSize.Y);
                placed.Add(s);
            }

            y += rows * CellSize.Y + CellSize.Y * 0.5f;
        }

        // Unlinked states: a grid, start state first
        var startStateId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(StateMachine, "m_startStateId") ?? -1);
        var rest = Enumerable.Range(0, states.Count).Where(s => !placed.Contains(s))
            .OrderBy(s => Convert.ToInt32(HavokBehaviorSymbols.GetValue(states[s], "m_stateId") ?? -1) == startStateId ? 0 : 1)
            .ThenBy(s => s)
            .ToList();

        // Wider than tall, as the graph pane is
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(rest.Count * 2.0)));

        for (var k = 0; k < rest.Count; k++)
            layout[states[rest[k]]] = new Vector2(k % columns * CellSize.X, y + k / columns * CellSize.Y);

        return layout;
    }
    #endregion

    /// <summary>
    /// Whether a state or link is selected in the graph
    /// </summary>
    public bool HasSelection => SelectedState != null || SelectedTransitions is { Count: > 0 };

    public bool CanDisplayProperties()
    {
        return IsCurrentTab && (SelectedState != null || SelectedTransitions is { Count: > 0 });
    }

    /// <summary>
    /// The selected state (for Used By), or null for a link
    /// </summary>
    public object GetSelectedObject() => SelectedState;

    public void DisplayProperties()
    {
        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = CFG.Current.HavokEditor.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);

        // Transitions first: a wildcard link is selected with its target state kept
        if (SelectedTransitions is { Count: > 0 })
        {
            // A link stands for every transition between its two states
            for (var i = 0; i < SelectedTransitions.Count; i++)
            {
                var transition = SelectedTransitions[i];

                ImGui.PushID(i);

                var open = ImGui.TreeNodeEx($"{LOC.Get("HAVOK_GraphView_Transition", i)}##graphTransition", ImGuiTreeNodeFlags.DefaultOpen);
                ImGui.NextColumn();
                ImGui.NextColumn();

                if (CFG.Current.HavokEditor.HavokEditor_Properties_Display_Type_Column)
                    ImGui.NextColumn();

                if (open)
                {
                    View.PropertyView.HavokPropEditGeneric(transition, HavokMetaHelper.GetMeta(Project, transition.GetType()));
                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
        }
        else if (SelectedState != null)
        {
            View.PropertyView.HavokPropEditGeneric(SelectedState, HavokMetaHelper.GetMeta(Project, SelectedState.GetType()));
        }

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
