using Hexa.NET.ImGui;
using Havok.Shared;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The Categories and Contents windows for files that aren't behavior graphs (animations, skeletons,
/// collision, navmeshes, cutscenes, ragdolls ...): the Outline category, a tree of the file's objects.
/// Selecting a node edits it in the property panel; with nothing selected the panel edits the whole file.
/// </summary>
public class HavokFileOutlineView
{
    private HavokEditorView View;
    private ProjectEntry Project;

    /// <summary>
    /// The outline of the shown file, or null if no file is shown (or it's shown by the behavior windows)
    /// </summary>
    public HavokFileOutline Outline;

    /// <summary>
    /// The category selected in the Categories window, which the Contents window shows
    /// </summary>
    public HavokFileContentsMode ContentsMode = HavokFileContentsMode.Outline;

    /// <summary>
    /// The selected node, or null
    /// </summary>
    public object Selected;

    public string Filter = "";
    public bool ExactFilter = false;

    // Search: whether each node, or a node below it, matches the filter
    private readonly Dictionary<object, bool> MatchCache = new(ReferenceEqualityComparer.Instance);
    private string MatchCacheFilter = null;
    private bool MatchCacheExact = false;
    private HavokFileOutline MatchCacheOutline = null;

    /// <summary>
    /// Guards against a reference cycle
    /// </summary>
    private const int MaxDepth = 128;

    private static readonly (HavokFileContentsMode Mode, string Key, string Tooltip)[] Categories =
    {
        (HavokFileContentsMode.Outline, "HAVOK_FileOutline_Category_Outline", "HAVOK_FileOutline_Category_Outline_TT"),
    };

    public HavokFileOutlineView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void ResetSelection()
    {
        Outline = null;
        Selected = null;
        MatchCache.Clear();
    }

    /// <summary>
    /// Builds the outline of the shown file (also re-run after structural edits, keeping the selection if it's still there)
    /// </summary>
    public void Setup(object sourceObject)
    {
        Outline = HavokFileOutline.Build(sourceObject, View.PropertyCache.GetCachedHavokFields);

        if (Outline == null || !Outline.Contains(Selected))
            Selected = null;

        MatchCache.Clear();
    }

    public bool IsShown => Outline != null;

    /// <summary>
    /// The Categories window: picks what the Contents window lists
    /// </summary>
    public void DisplayCategoryWindow()
    {
        if (!IsShown)
        {
            GUI.WrappedText(LOC.Get("HAVOK_FileOutline_No_File"));
            return;
        }

        foreach (var (mode, key, tooltip) in Categories)
        {
            if (ImGui.Selectable($"{LOC.Get(key)}###fileCategory_{mode}", ContentsMode == mode))
                ContentsMode = mode;

            GUI.Tooltip(LOC.Get(tooltip));
        }
    }

    /// <summary>
    /// The Contents window: the list of the selected category
    /// </summary>
    public void DisplayContentsWindow()
    {
        if (!IsShown)
        {
            GUI.WrappedText(LOC.Get("HAVOK_FileOutline_No_File"));
            return;
        }

        switch (ContentsMode)
        {
            case HavokFileContentsMode.Outline:
                DisplayOutlineHeader();
                DisplayOutlineTree();
                break;
        }
    }

    #region Outline
    private void DisplayOutlineHeader()
    {
        ImGui.BeginChild($"framedList_FileOutlineHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokFileOutlineSearch", ref Filter, ref ExactFilter);

        ImGui.EndChild();
    }

    private void DisplayOutlineTree()
    {
        ImGui.BeginChild("havokFileOutlineSection");

        ValidateMatchCache();

        var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);

        DrawNode(Outline.Root, ancestors, 0);

        ImGui.EndChild();
    }

    private void DrawNode(object node, HashSet<object> ancestors, int depth)
    {
        var searching = IsSearching;

        if (searching && !SubtreeMatches(node))
            return;

        var label = Outline.GetLabel(node);
        var tag = HavokFileOutline.GetTag(node);

        if (depth > MaxDepth || ancestors.Contains(node))
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_OutlineView_Cycle", label));
            return;
        }

        var children = Outline.GetChildren(node);
        var isLeaf = children.Count == 0;
        var isSelected = ReferenceEquals(node, Selected);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

        if (isLeaf)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        if (isSelected)
            flags |= ImGuiTreeNodeFlags.Selected;

        // The root is open to begin with, so its entries are shown straight away
        if (depth == 0)
            flags |= ImGuiTreeNodeFlags.DefaultOpen;

        // While searching, open the nodes a match is below
        if (!isLeaf && searching && children.Any(SubtreeMatches))
            ImGui.SetNextItemOpen(true);

        var open = ImGui.TreeNodeEx($"{label}##fileOutlineNode", flags);

        // Clicking the selected node again deselects it, so the panel goes back to the whole file
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Selected = isSelected ? null : node;

        // Type tag, unless the label already is the type
        if (tag != label)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(tag);
        }

        if (open && !isLeaf)
        {
            ancestors.Add(node);

            for (var i = 0; i < children.Count; i++)
            {
                ImGui.PushID(i);
                DrawNode(children[i], ancestors, depth + 1);
                ImGui.PopID();
            }

            ancestors.Remove(node);

            ImGui.TreePop();
        }
    }
    #endregion

    #region Search
    private bool IsSearching => !string.IsNullOrEmpty(Filter);

    private void ValidateMatchCache()
    {
        if (MatchCacheFilter != Filter ||
            MatchCacheExact != ExactFilter ||
            !ReferenceEquals(MatchCacheOutline, Outline))
        {
            MatchCache.Clear();

            MatchCacheFilter = Filter;
            MatchCacheExact = ExactFilter;
            MatchCacheOutline = Outline;
        }
    }

    /// <summary>
    /// Whether the node's name or type matches the filter, or a node below it does
    /// </summary>
    private bool SubtreeMatches(object node)
    {
        if (MatchCache.TryGetValue(node, out var cached))
            return cached;

        // Recorded first, so a cycle counts as no match instead of recursing forever
        MatchCache[node] = false;

        var result = EditorFilters.IsMatch(Filter, Outline.GetLabel(node), ExactFilter)
            || EditorFilters.IsMatch(Filter, HavokFileOutline.GetTag(node), ExactFilter);

        if (!result)
        {
            foreach (var child in Outline.GetChildren(node))
            {
                if (SubtreeMatches(child))
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

    /// <summary>
    /// The property panel for the file: the selected node, or the whole file if nothing (or the root) is selected
    /// </summary>
    public void DisplayProperties(IRootContainer root)
    {
        ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

        if (Selected == null || ReferenceEquals(Selected, root))
        {
            View.PropertyView.HavokPropEdit(root);
        }
        else
        {
            var columnCount = CFG.Current.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

            ImGui.Columns(columnCount);

            View.PropertyView.HavokPropEditGeneric(Selected, HavokMetaHelper.GetMeta(Project, Selected.GetType()));

            ImGui.Columns(1);
        }

        ImGui.EndChild();
    }
}

/// <summary>
/// What the Contents window lists for a file that isn't a behavior graph
/// </summary>
public enum HavokFileContentsMode
{
    Outline
}
