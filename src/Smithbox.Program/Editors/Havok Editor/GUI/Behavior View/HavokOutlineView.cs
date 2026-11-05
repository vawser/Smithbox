using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The behavior graph as a tree of its generators and states, without the objects in between
/// (graph data, binding sets, transition arrays, blender children).
/// Selecting a node edits it in the property panel; Back and Forward step through the nodes selected.
/// </summary>
public class HavokOutlineView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    /// <summary>
    /// The selected node, or null
    /// </summary>
    public object Selected;

    private bool FocusTabRequested = false;
    private bool ScrollToSelected = false;

    /// <summary>
    /// Nodes to open on the next draw, so a node selected from elsewhere is shown
    /// </summary>
    private HashSet<object> RevealNodes = new(ReferenceEqualityComparer.Instance);

    // Navigation history
    private const int MaxHistory = 100;
    private readonly List<object> History = new();
    private int HistoryIndex = -1;

    // Search: whether each node, or a node below it, matches the filter
    private readonly Dictionary<object, bool> MatchCache = new(ReferenceEqualityComparer.Instance);
    private string MatchCacheFilter = null;
    private bool MatchCacheExact = false;
    private HavokBehaviorSymbols MatchCacheSymbols = null;

    /// <summary>
    /// Guards against a reference cycle (not seen in ER or DS3 files, but a hand-edited file could have one)
    /// </summary>
    private const int MaxDepth = 128;

    public HavokOutlineView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        Selected = null;
        History.Clear();
        HistoryIndex = -1;
        RevealNodes.Clear();
        MatchCache.Clear();
    }

    public void Setup(object sourceObject)
    {
        var symbols = Owner.Symbols;

        // Drop what an edit removed from the file
        if (symbols == null || !symbols.Contains(Selected))
            Selected = null;

        for (var i = History.Count - 1; i >= 0; i--)
        {
            if (symbols == null || !symbols.Contains(History[i]))
            {
                History.RemoveAt(i);

                if (HistoryIndex >= i)
                    HistoryIndex--;
            }
        }

        HistoryIndex = Math.Clamp(HistoryIndex, -1, History.Count - 1);

        MatchCache.Clear();
    }

    #region Navigation
    /// <summary>
    /// Select a node (or the nearest node above an object), open the tree down to it and show this tab
    /// </summary>
    public void Navigate(object obj, bool record = true)
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        var node = symbols.FindOutlineNode(obj);

        if (node == null)
            return;

        if (record && !ReferenceEquals(node, Selected))
        {
            // A new step drops the steps that were gone back over
            if (HistoryIndex < History.Count - 1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);

            History.Add(node);

            if (History.Count > MaxHistory)
                History.RemoveAt(0);

            HistoryIndex = History.Count - 1;
        }

        Selected = node;
        RevealNodes = symbols.GetOutlinePath(node);
        ScrollToSelected = true;
        FocusTabRequested = true;
    }

    public bool CanGoBack => HistoryIndex > 0;
    public bool CanGoForward => HistoryIndex < History.Count - 1;

    public void GoBack()
    {
        if (!CanGoBack)
            return;

        HistoryIndex--;
        Navigate(History[HistoryIndex], record: false);
    }

    public void GoForward()
    {
        if (!CanGoForward)
            return;

        HistoryIndex++;
        Navigate(History[HistoryIndex], record: false);
    }

    /// <summary>
    /// Clicking a node in the tree: select it and add it to the history, without moving the tree
    /// </summary>
    private void SelectFromTree(object node)
    {
        Navigate(node);

        RevealNodes.Clear();
        ScrollToSelected = false;
        FocusTabRequested = false;
    }
    #endregion

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    public void DisplayTab()
    {
        if (Owner.Symbols == null)
        {
            SetTabState(false);
            return;
        }

        var flags = FocusTabRequested ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
        FocusTabRequested = false;

        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_BehaviorView_Tab_Outline")}##tabOutline", flags))
        {
            SetTabState(true);

            Shortcuts();

            DisplayTabHeader();
            DisplayTabContents();

            ImGui.EndTabItem();
        }
        else
        {
            SetTabState(false);
        }
    }

    private void Shortcuts()
    {
        // Not while typing, so Alt+Left still moves the text cursor
        if (ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.HavokEditor_Navigate_Back))
            GoBack();

        if (InputManager.IsPressed(KeybindID.HavokEditor_Navigate_Forward))
            GoForward();
    }

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        ImGui.BeginDisabled(!CanGoBack);
        if (ImGui.ArrowButton("##outlineBack", ImGuiDir.Left))
            GoBack();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_OutlineView_Back_TT"));

        ImGui.SameLine();

        ImGui.BeginDisabled(!CanGoForward);
        if (ImGui.ArrowButton("##outlineForward", ImGuiDir.Right))
            GoForward();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_OutlineView_Forward_TT"));

        ImGui.SameLine();

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();
    }

    public void DisplayTabContents()
    {
        var symbols = Owner.Symbols;

        ImGui.BeginChild("havokBehaviorElementListSection");

        if (symbols != null)
        {
            ValidateMatchCache(symbols);

            var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);

            for (var i = 0; i < symbols.BehaviorGraphs.Count; i++)
            {
                ImGui.PushID(i);
                DrawNode(symbols, symbols.BehaviorGraphs[i], ancestors, 0);
                ImGui.PopID();
            }
        }

        // Opened once; after that the user can close them again
        RevealNodes.Clear();

        ImGui.EndChild();
    }

    private void DrawNode(HavokBehaviorSymbols symbols, object node, HashSet<object> ancestors, int depth)
    {
        var searching = IsSearching;

        if (searching && !SubtreeMatches(symbols, node))
            return;

        var label = HavokBehaviorSymbols.GetOutlineLabel(node);

        if (depth > MaxDepth || ancestors.Contains(node))
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_OutlineView_Cycle", label));
            return;
        }

        var children = symbols.GetOutlineChildren(node);
        var isLeaf = children.Count == 0;
        var isSelected = ReferenceEquals(node, Selected);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

        if (isLeaf)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        if (isSelected)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (!isLeaf)
        {
            // While searching, open the nodes a match is below
            if (searching && children.Any(c => SubtreeMatches(symbols, c)))
                ImGui.SetNextItemOpen(true);
            else if (RevealNodes.Contains(node))
                ImGui.SetNextItemOpen(true);
        }

        var open = ImGui.TreeNodeEx($"{label}##outlineNode", flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            SelectFromTree(node);

        if (isSelected && ScrollToSelected)
        {
            ImGui.SetScrollHereY();
            ScrollToSelected = false;
        }

        // Type tag
        ImGui.SameLine();
        ImGui.TextDisabled(HavokBehaviorSymbols.GetOutlineTag(node));

        // Shared: referenced from more than one place
        var parentCount = symbols.GetParents(node).Count;

        if (parentCount > 1)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_OutlineView_Shared", parentCount));
            GUI.Tooltip(LOC.Get("HAVOK_OutlineView_Shared_TT", parentCount));
        }

        if (open && !isLeaf)
        {
            ancestors.Add(node);

            for (var i = 0; i < children.Count; i++)
            {
                ImGui.PushID(i);
                DrawNode(symbols, children[i], ancestors, depth + 1);
                ImGui.PopID();
            }

            ancestors.Remove(node);

            ImGui.TreePop();
        }
    }

    #region Search
    private bool IsSearching => !string.IsNullOrEmpty(Owner.PropFilter);

    private void ValidateMatchCache(HavokBehaviorSymbols symbols)
    {
        if (MatchCacheFilter != Owner.PropFilter ||
            MatchCacheExact != Owner.ExactPropFilter ||
            !ReferenceEquals(MatchCacheSymbols, symbols))
        {
            MatchCache.Clear();

            MatchCacheFilter = Owner.PropFilter;
            MatchCacheExact = Owner.ExactPropFilter;
            MatchCacheSymbols = symbols;
        }
    }

    /// <summary>
    /// Whether the node's label matches the filter, or a node below it does
    /// </summary>
    private bool SubtreeMatches(HavokBehaviorSymbols symbols, object node)
    {
        if (MatchCache.TryGetValue(node, out var cached))
            return cached;

        // Recorded first, so a cycle counts as no match instead of recursing forever
        MatchCache[node] = false;

        var result = EditorFilters.IsMatch(Owner.PropFilter, HavokBehaviorSymbols.GetOutlineLabel(node), Owner.ExactPropFilter);

        if (!result)
        {
            foreach (var child in symbols.GetOutlineChildren(node))
            {
                if (SubtreeMatches(symbols, child))
                {
                    result = true;
                    break;
                }
            }
        }

        MatchCache[node] = result;

        return result;
    }
    #endregion

    public bool CanDisplayProperties()
    {
        return IsCurrentTab && Selected != null;
    }

    public void DisplayProperties()
    {
        if (Selected == null)
            return;

        ImGui.BeginChild("havokBehaviorPropEditSection");

        var columnCount = CFG.Current.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(Selected, HavokMetaHelper.GetMeta(Project, Selected.GetType()));

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
