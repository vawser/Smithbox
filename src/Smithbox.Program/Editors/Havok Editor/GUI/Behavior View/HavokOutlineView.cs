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

    /// <summary>
    /// A node to select once the symbols are rebuilt (after an edit that added it, it isn't in them yet)
    /// </summary>
    private object PendingNavigate;

    public void NavigateAfterRebuild(object obj)
    {
        PendingNavigate = obj;
    }

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

        if (PendingNavigate != null)
        {
            if (symbols != null && symbols.Contains(PendingNavigate))
                Navigate(PendingNavigate);

            PendingNavigate = null;
        }
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
        Owner.ContentsMode = HavokBehaviorContentsMode.Outline;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
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
    }
    #endregion

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Draws this view in the Contents window (its category is selected)
    /// </summary>
    public void DisplayContents()
    {
        if (Owner.Symbols == null)
            return;

        Shortcuts();

        DisplayTabHeader();
        DisplayTabContents();
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

        // Modifiers under their modifier generators
        ImGui.Checkbox($"{LOC.Get("HAVOK_OutlineView_Show_Modifiers")}##outlineShowModifiers", ref CFG.Current.BehaviorEditor.BehaviorEditor_Outline_Show_Modifiers);
        GUI.Tooltip(LOC.Get("HAVOK_OutlineView_Show_Modifiers_TT"));

        ImGui.SameLine();

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();
    }

    public void DisplayTabContents()
    {
        var symbols = Owner.Symbols;

        // Read here, while the Contents window is current (its focus is what's checked); uses last frame's rows
        HandleKeys();

        ImGui.BeginChild("havokBehaviorElementListSection");

        Rows.Clear();

        if (symbols != null)
        {
            ValidateMatchCache(symbols);

            var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);

            for (var i = 0; i < symbols.BehaviorGraphs.Count; i++)
            {
                ImGui.PushID(i);
                DrawNode(symbols, symbols.BehaviorGraphs[i], ancestors, 0, -1);
                ImGui.PopID();
            }
        }

        (LastRows, Rows) = (Rows, LastRows);

        // Opened (or closed) once; after that the user can change them again
        RevealNodes.Clear();
        RevealModifiers.Clear();
        CloseNodes.Clear();

        ImGui.EndChild();
    }

    #region Keys
    /// <summary>
    /// A drawn row of the tree, in draw order. A shared node is drawn once per parent, so it can have several.
    /// </summary>
    private struct Row
    {
        public object Obj;
        public bool IsModifier;
        public bool IsLeaf;
        public bool IsOpen;

        /// <summary>The row of the node it's drawn under, or -1</summary>
        public int Parent;
    }

    // This frame's rows, and last frame's (which the keys move through, as they're read before the tree is drawn)
    private List<Row> Rows = new();
    private List<Row> LastRows = new();

    /// <summary>
    /// The row of the selected node, so stepping from a shared node goes on from the copy that was selected
    /// </summary>
    private int SelectedRow = -1;

    /// <summary>
    /// Nodes and modifier lists to close on the next draw (Left)
    /// </summary>
    private readonly HashSet<object> CloseNodes = new(ReferenceEqualityComparer.Instance);

    // Set by a key step, so the tree scrolls to the new selection if it's out of view
    private bool KeyScroll = false;

    private int FindSelectedRow()
    {
        if (Selected == null)
            return -1;

        if (SelectedRow >= 0 && SelectedRow < LastRows.Count && ReferenceEquals(LastRows[SelectedRow].Obj, Selected))
            return SelectedRow;

        return LastRows.FindIndex(r => ReferenceEquals(r.Obj, Selected));
    }

    /// <summary>
    /// Up / Down step through the shown rows; Right opens a node (or goes to its first child if it's open), Left
    /// closes it (or goes to the node above if it's closed)
    /// </summary>
    private void HandleKeys()
    {
        if (LastRows.Count == 0 || !HavokListKeys.HasFocus())
            return;

        var io = ImGui.GetIO();

        // Alt+Left / Right are Back and Forward
        if (io.KeyCtrl || io.KeyAlt || io.KeyShift)
            return;

        var current = FindSelectedRow();
        var step = GUI.ListStepKey();

        if (step != 0)
        {
            var target = HavokListKeys.StepIndex(current, LastRows.Count, step);

            if (target >= 0)
                SelectRow(target);

            return;
        }

        if (current < 0)
            return;

        var row = LastRows[current];

        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
        {
            if (row.IsLeaf)
                return;

            if (!row.IsOpen)
            {
                if (row.IsModifier)
                    RevealModifiers.Add(row.Obj);
                else
                    RevealNodes.Add(row.Obj);
            }
            else if (current + 1 < LastRows.Count && LastRows[current + 1].Parent == current)
            {
                SelectRow(current + 1);
            }
        }
        else if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
        {
            if (!row.IsLeaf && row.IsOpen)
                CloseNodes.Add(row.Obj);
            else if (row.Parent >= 0)
                SelectRow(row.Parent);
        }
    }

    private void SelectRow(int index)
    {
        var row = LastRows[index];

        if (row.IsModifier)
        {
            Selected = row.Obj;
            Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
        }
        else
        {
            // Into the history like a click, so Back returns to the node stepped from
            SelectFromTree(row.Obj);
        }

        SelectedRow = index;
        KeyScroll = true;
    }

    /// <summary>
    /// Records a drawn row and returns its index. Call right after the row's TreeNodeEx.
    /// </summary>
    private int AddRow(object obj, bool isModifier, bool isLeaf, bool isOpen, int parentRow)
    {
        Rows.Add(new Row { Obj = obj, IsModifier = isModifier, IsLeaf = isLeaf, IsOpen = isOpen, Parent = parentRow });

        return Rows.Count - 1;
    }
    #endregion

    private void DrawNode(HavokBehaviorSymbols symbols, object node, HashSet<object> ancestors, int depth, int parentRow)
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

        // A modifier generator's modifier, drawn under it when the toggle is on
        var modifier = CFG.Current.BehaviorEditor.BehaviorEditor_Outline_Show_Modifiers && node.GetType().Name == "hkbModifierGenerator"
            ? HavokBehaviorSymbols.GetValue(node, "m_modifier")
            : null;

        var isLeaf = children.Count == 0 && modifier == null;
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
            else if (CloseNodes.Contains(node))
                ImGui.SetNextItemOpen(false);
        }

        // Vanilla state: Modified / Added colour, explained in the tooltip
        var status = Owner.Changes.ObjectStatus(node);
        var coloured = VanillaMarkers.PushColour(status);
        var open = ImGui.TreeNodeEx($"{label}##outlineNode", flags);
        VanillaMarkers.PopColour(coloured);
        VanillaMarkers.Tooltip(status);

        var rowIndex = AddRow(node, false, isLeaf, open, parentRow);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            SelectFromTree(node);
            SelectedRow = rowIndex;
        }

        if (isSelected && rowIndex == SelectedRow)
            HavokListKeys.ScrollIfRequested(ref KeyScroll);

        if (ImGui.BeginPopupContextItem("##outlineNodeMenu"))
        {
            Owner.ActionsView.DisplayNodeMenu(node);
            ImGui.EndPopup();
        }

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
                DrawNode(symbols, children[i], ancestors, depth + 1, rowIndex);
                ImGui.PopID();
            }

            if (modifier != null)
            {
                ImGui.PushID("modifier");
                DrawModifier(symbols, modifier, new HashSet<object>(ReferenceEqualityComparer.Instance), 0, rowIndex);
                ImGui.PopID();
            }

            ancestors.Remove(node);

            ImGui.TreePop();
        }
    }

    #region Modifiers
    // The modifier lists to open so a modifier shown from elsewhere is visible
    private readonly HashSet<object> RevealModifiers = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// A modifier under its modifier generator (Modifiers toggle): its name and type, a modifier list's entries below
    /// it. Selecting one shows it in Properties; its menu is the Modifiers category's.
    /// </summary>
    private void DrawModifier(HavokBehaviorSymbols symbols, object modifier, HashSet<object> ancestors, int depth, int parentRow)
    {
        if (modifier == null)
            return;

        var label = HavokBehaviorSymbols.GetValue(modifier, "m_name") as string ?? modifier.GetType().Name;

        if (depth > MaxDepth || ancestors.Contains(modifier))
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_OutlineView_Cycle", label));
            return;
        }

        var entries = HavokBehaviorSymbols.GetValue(modifier, "m_modifiers") as System.Collections.IList;
        var isLeaf = entries == null || entries.Count == 0;
        var isSelected = ReferenceEquals(modifier, Selected);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

        if (isLeaf)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        if (isSelected)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (!isLeaf && RevealModifiers.Contains(modifier))
            ImGui.SetNextItemOpen(true);
        else if (!isLeaf && CloseNodes.Contains(modifier))
            ImGui.SetNextItemOpen(false);

        var status = Owner.Changes.ObjectStatus(modifier);
        var coloured = VanillaMarkers.PushColour(status);
        var open = ImGui.TreeNodeEx($"{label}##outlineModifier", flags);
        VanillaMarkers.PopColour(coloured);
        VanillaMarkers.Tooltip(status);

        var rowIndex = AddRow(modifier, true, isLeaf, open, parentRow);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            Selected = modifier;
            SelectedRow = rowIndex;
            Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
        }

        if (isSelected && rowIndex == SelectedRow)
            HavokListKeys.ScrollIfRequested(ref KeyScroll);

        if (ImGui.BeginPopupContextItem("##outlineModifierMenu"))
        {
            Selected = modifier;
            Owner.ActionsView.DisplayModifierItems(modifier);
            ImGui.EndPopup();
        }

        if (isSelected && ScrollToSelected)
        {
            ImGui.SetScrollHereY();
            ScrollToSelected = false;
        }

        ImGui.SameLine();
        ImGui.TextDisabled(HavokBehaviorSymbols.GetOutlineTag(modifier));

        if (HavokBehaviorOperations.IsModifierEnabled(modifier) == false)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("HAVOK_Modifiers_Disabled_Tag"));
        }

        if (open && !isLeaf)
        {
            ancestors.Add(modifier);

            for (var i = 0; i < entries.Count; i++)
            {
                ImGui.PushID(i);
                DrawModifier(symbols, entries[i], ancestors, depth + 1, rowIndex);
                ImGui.PopID();
            }

            ancestors.Remove(modifier);

            ImGui.TreePop();
        }
    }

    /// <summary>
    /// Shows a modifier in the Outline: turns the Modifiers toggle on, opens the tree down to its modifier generator
    /// and the lists above it, and selects it
    /// </summary>
    public void ShowModifier(object modifier)
    {
        var symbols = Owner.Symbols;

        if (symbols == null || modifier == null)
            return;

        CFG.Current.BehaviorEditor.BehaviorEditor_Outline_Show_Modifiers = true;

        // The modifier generator above it, through the lists holding it
        RevealModifiers.Clear();
        object generator = null;
        var current = modifier;

        for (var depth = 0; depth < 16 && current != null && generator == null; depth++)
        {
            object next = null;

            foreach (var parent in symbols.GetParents(current))
            {
                if (parent.GetType().Name == "hkbModifierGenerator")
                {
                    generator = parent;
                    break;
                }

                if (HavokBehaviorSymbols.IsModifier(parent))
                {
                    RevealModifiers.Add(parent);
                    next = parent;
                }
            }

            current = next;
        }

        if (generator == null)
            return;

        Navigate(generator);

        // Open the generator itself too, so its modifier is drawn
        RevealNodes.Add(generator);

        Selected = modifier;
        ScrollToSelected = true;
    }
    #endregion

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

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = CFG.Current.HavokEditor.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(Selected, HavokMetaHelper.GetMeta(Project, Selected.GetType()));

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
