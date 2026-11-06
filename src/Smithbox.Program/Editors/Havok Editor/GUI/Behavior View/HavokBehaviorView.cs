using Hexa.NET.ImGui;
using StudioCore.Utilities;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorView
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public string PropFilter = "";
    public bool ExactPropFilter = false;

    public bool IsBehaviorGraph = false;

    public HavokAnimationClipView AnimationClipView;
    public HavokAnimationSelectorView AnimationSelectorView;
    public HavokStateMachineView StateMachineView;
    public HavokVariableView VariableView;
    public HavokOutlineView OutlineView;
    public HavokAnimationTableView AnimationTableView;
    public HavokProblemsView ProblemsView;
    public HavokBehaviorActionsView ActionsView;
    public HavokStateMachineGraphView GraphView;

    /// <summary>
    /// The names the file's indices refer to, or null if the shown file isn't a behavior graph
    /// </summary>
    public HavokBehaviorSymbols Symbols;

    /// <summary>
    /// The animations of the character the shown behavior belongs to (its time act entries and animation files),
    /// loaded in the background, or null if the shown file isn't a behavior graph
    /// </summary>
    public HavokAnimationIndex Animations;

    public HavokBehaviorView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        
        AnimationClipView = new(view, this, project);
        AnimationSelectorView = new(view, this, project);
        StateMachineView = new(view, this, project);
        VariableView = new(view, this, project);
        OutlineView = new(view, this, project);
        AnimationTableView = new(view, this, project);
        ProblemsView = new(view, this, project);
        ActionsView = new(view, this, project);
        GraphView = new(view, this, project);
    }

    public void ResetSelection()
    {
        IsBehaviorGraph = false;
        Symbols = null;
        Animations = null;

        AnimationClipView.ResetSelection();
        AnimationSelectorView.ResetSelection();
        StateMachineView.ResetSelection();
        VariableView.ResetSelection();
        OutlineView.ResetSelection();
        AnimationTableView.ResetSelection();
        ProblemsView.ResetSelection();
        GraphView.ResetSelection();
    }

    public void SetupBehaviorView(object sourceObject)
    {
        // Determine if selected file is a Behavior Graph
        // (this also re-runs after structural edits, so it starts from scratch)
        IsBehaviorGraph = false;

        // Built for any version (it reads fields by name), but only for behavior files, since it walks the whole file
        Symbols = View.Selection.CategoryMode is HavokCategoryMode.Behavior
            ? HavokBehaviorSymbols.Build(sourceObject, View.PropertyCache.GetCachedHavokFields)
            : null;

        // A behbnd is named after its character (c0000.behbnd), and its shared c9997.hkx belongs to it too
        Animations = Symbols != null
            ? HavokAnimationIndex.Get(Project, View.Selection.BinderFileEntry?.Filename)
            : null;

        // By class name, so it is the same for every SDK's classes
        if (HavokTreeSearch.FindAllOfClass(sourceObject, "hkbBehaviorGraph", View.PropertyCache.GetCachedHavokFields).Count > 0)
            IsBehaviorGraph = true;

        AnimationClipView.Setup(sourceObject);
        AnimationSelectorView.Setup(sourceObject);
        StateMachineView.Setup(sourceObject);
        VariableView.Setup(sourceObject);
        OutlineView.Setup(sourceObject);
        AnimationTableView.Setup(sourceObject);
        ProblemsView.Setup(sourceObject);
        GraphView.Setup(sourceObject);
    }

    #region Windows
    /// <summary>
    /// The category selected in the Behavior Categories window, which the Contents window shows
    /// </summary>
    public HavokBehaviorContentsMode ContentsMode = HavokBehaviorContentsMode.Outline;

    /// <summary>
    /// Which window the property panel follows: the Contents window, or the Graph window
    /// (whichever was clicked last)
    /// </summary>
    public HavokBehaviorSelectionSource SelectionSource = HavokBehaviorSelectionSource.Contents;

    private static readonly (HavokBehaviorContentsMode Mode, string Key, string Tooltip)[] Categories =
    {
        (HavokBehaviorContentsMode.Outline, "HAVOK_BehaviorView_Tab_Outline", "HAVOK_BehaviorCategory_Outline_TT"),
        (HavokBehaviorContentsMode.Animations, "HAVOK_BehaviorView_Tab_Animations", "HAVOK_BehaviorCategory_Animations_TT"),
        (HavokBehaviorContentsMode.StateMachines, "HAVOK_BehaviorView_Tab_State_Machines", "HAVOK_BehaviorCategory_State_Machines_TT"),
        (HavokBehaviorContentsMode.Selectors, "HAVOK_BehaviorView_Tab_Anim_Selectors", "HAVOK_BehaviorCategory_Selectors_TT"),
        (HavokBehaviorContentsMode.Clips, "HAVOK_BehaviorView_Tab_Clip_Generators", "HAVOK_BehaviorCategory_Clips_TT"),
        (HavokBehaviorContentsMode.Variables, "HAVOK_BehaviorView_Tab_Variables", "HAVOK_BehaviorCategory_Variables_TT"),
        (HavokBehaviorContentsMode.Problems, "HAVOK_BehaviorView_Tab_Problems", "HAVOK_BehaviorCategory_Problems_TT"),
    };

    /// <summary>
    /// Whether the shown file is a behavior graph the behavior windows can show
    /// </summary>
    public bool IsShown => IsBehaviorGraph && Symbols != null;

    /// <summary>
    /// Sets which view the property panel draws: the selected category's, or the graph's
    /// </summary>
    private void SyncCurrentViews()
    {
        var contents = SelectionSource is HavokBehaviorSelectionSource.Contents || !GraphView.HasSelection;

        OutlineView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Outline);
        AnimationTableView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Animations);
        StateMachineView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.StateMachines);
        AnimationSelectorView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Selectors);
        AnimationClipView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Clips);
        VariableView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Variables);
        ProblemsView.SetTabState(contents && ContentsMode is HavokBehaviorContentsMode.Problems);
        GraphView.SetTabState(!contents);
    }

    /// <summary>
    /// The Behavior Categories window: picks what the Contents window lists
    /// </summary>
    public void DisplayCategoryWindow()
    {
        if (!IsShown)
        {
            GUI.WrappedText(LOC.Get("HAVOK_BehaviorView_No_Behavior"));
            return;
        }

        foreach (var (mode, key, tooltip) in Categories)
        {
            var label = LOC.Get(key);

            if (mode is HavokBehaviorContentsMode.Problems && ProblemsView.ProblemCount > 0)
                label = LOC.Get("HAVOK_BehaviorView_Tab_Problems_Count", ProblemsView.ProblemCount);

            // The Animations table needs selectors to list
            if (mode is HavokBehaviorContentsMode.Animations && Symbols.Selectors.Count == 0)
                continue;

            if (ImGui.Selectable($"{label}###behaviorCategory_{mode}", ContentsMode == mode))
            {
                ContentsMode = mode;
                SelectionSource = HavokBehaviorSelectionSource.Contents;
            }

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
            GUI.WrappedText(LOC.Get("HAVOK_BehaviorView_No_Behavior"));
            return;
        }

        SyncCurrentViews();

        switch (ContentsMode)
        {
            case HavokBehaviorContentsMode.Outline: OutlineView.DisplayContents(); break;
            case HavokBehaviorContentsMode.Animations: AnimationTableView.DisplayContents(); break;
            case HavokBehaviorContentsMode.StateMachines: StateMachineView.DisplayContents(); break;
            case HavokBehaviorContentsMode.Selectors: AnimationSelectorView.DisplayContents(); break;
            case HavokBehaviorContentsMode.Clips: AnimationClipView.DisplayContents(); break;
            case HavokBehaviorContentsMode.Variables: VariableView.DisplayContents(); break;
            case HavokBehaviorContentsMode.Problems: ProblemsView.DisplayContents(); break;
        }
    }

    /// <summary>
    /// The Graph window: the state machine of the Contents selection
    /// </summary>
    public void DisplayGraphWindow()
    {
        if (!IsShown)
        {
            GUI.WrappedText(LOC.Get("HAVOK_BehaviorView_No_Behavior"));
            return;
        }

        GraphView.DisplayWindow();
    }

    /// <summary>
    /// The selected object of the Contents window's category (not the graph's), or null
    /// </summary>
    public object GetContentsSelectedObject()
    {
        return ContentsMode switch
        {
            HavokBehaviorContentsMode.Outline => OutlineView.Selected,
            HavokBehaviorContentsMode.Animations => AnimationTableView.Selected,
            HavokBehaviorContentsMode.StateMachines => StateMachineView.SelectedStateMachines.FirstOrDefault(),
            HavokBehaviorContentsMode.Selectors => AnimationSelectorView.SelectedAnimSelectors.FirstOrDefault(),
            HavokBehaviorContentsMode.Clips => AnimationClipView.SelectedAnimationClips.FirstOrDefault(),
            _ => null
        };
    }
    #endregion


    /// <summary>
    /// The height the Used By section took last frame, which the property grid above it leaves free
    /// </summary>
    private float UsedByHeight = 0f;

    /// <summary>
    /// The size of each view's property grid: the window's width, and its height less the Used By section below
    /// </summary>
    public System.Numerics.Vector2 PropertySectionSize =>
        new(0, UsedByHeight > 0 ? -UsedByHeight : 0);

    public void DisplayProperties()
    {
        SyncCurrentViews();

        DisplayPropertyGrid();

        // Below the grid; measured, so the grid leaves it room next frame
        var start = ImGui.GetCursorPosY();
        DisplayUsedBy();
        var height = ImGui.GetCursorPosY() - start;

        UsedByHeight = height > 0 ? height + ImGui.GetStyle().ItemSpacing.Y : 0;
    }

    private void DisplayPropertyGrid()
    {
        if (OutlineView.CanDisplayProperties())
        {
            OutlineView.DisplayProperties();
        }
        else if (GraphView.CanDisplayProperties())
        {
            GraphView.DisplayProperties();
        }
        else if (AnimationTableView.CanDisplayProperties())
        {
            AnimationTableView.DisplayProperties();
        }
        else if (StateMachineView.CanDisplayProperties())
        {
            StateMachineView.DisplayProperties();
        }
        else if (AnimationSelectorView.CanDisplayProperties())
        {
            AnimationSelectorView.DisplayProperties();
        }
        else if (AnimationClipView.CanDisplayProperties())
        {
            AnimationClipView.DisplayProperties();
        }
        else if (VariableView.CanDisplayProperties())
        {
            VariableView.DisplayProperties();
        }
        else
        {
            GUI.WrappedText(LOC.Get("HAVOK_BehaviorView_No_Entry_Selected"));
        }
    }

    /// <summary>
    /// The object shown in the property panel by the current tab, or null (the Variables tab shows a variable,
    /// which isn't one object)
    /// </summary>
    public object GetSelectedObject()
    {
        if (OutlineView.CanDisplayProperties())
            return OutlineView.Selected;

        if (GraphView.CanDisplayProperties())
            return GraphView.GetSelectedObject();

        if (AnimationTableView.CanDisplayProperties())
            return AnimationTableView.Selected;

        if (StateMachineView.CanDisplayProperties())
            return StateMachineView.SelectedStateMachines.FirstOrDefault();

        if (AnimationSelectorView.CanDisplayProperties())
            return AnimationSelectorView.SelectedAnimSelectors.FirstOrDefault();

        if (AnimationClipView.CanDisplayProperties())
            return AnimationClipView.SelectedAnimationClips.FirstOrDefault();

        return null;
    }

    /// <summary>
    /// What references the selected object, each with the field it's held in. Clicking one shows it in the outline.
    /// </summary>
    private void DisplayUsedBy()
    {
        var selected = GetSelectedObject();

        if (selected == null || Symbols == null)
            return;

        var parents = Symbols.GetParents(selected);

        ImGui.Separator();

        var headerOpen = ImGui.CollapsingHeader($"{LOC.Get("HAVOK_UsedBy_Header", parents.Count)}###havokUsedBy");
        GUI.Tooltip(LOC.Get("HAVOK_UsedBy_Header_TT"));

        // Show in Outline, for a selection made in another tab
        if (!OutlineView.IsCurrentTab)
        {
            ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get("HAVOK_UsedBy_Show_In_Outline")}##showInOutline"))
                OutlineView.Navigate(selected);

            GUI.Tooltip(LOC.Get("HAVOK_UsedBy_Show_In_Outline_TT"));
        }

        if (!headerOpen || parents.Count == 0)
            return;

        // At most 8 rows tall, so the property grid below keeps its space
        var rowHeight = ImGui.GetTextLineHeightWithSpacing();
        var height = Math.Min(parents.Count, 8) * rowHeight + ImGui.GetStyle().WindowPadding.Y * 2;

        object unlinkParent = null;

        ImGui.BeginChild("havokUsedByList", new System.Numerics.Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < parents.Count; i++)
        {
            var parent = parents[i];
            var field = HavokBehaviorSymbols.FindFieldSegment(parent, selected) ?? "?";
            var label = $"{HavokBehaviorSymbols.GetLabel(parent)}  .{field}";

            // Unlink: remove this reference (undoable). The last one removes the object from the file.
            if (ImGui.SmallButton($"{LOC.Get("HAVOK_UsedBy_Unlink")}##unlink{i}"))
                unlinkParent = parent;

            GUI.Tooltip(parents.Count == 1
                ? LOC.Get("HAVOK_UsedBy_Unlink_Last_TT")
                : LOC.Get("HAVOK_UsedBy_Unlink_TT", parents.Count - 1));

            ImGui.SameLine();

            if (ImGui.Selectable($"{label}##usedBy{i}"))
                OutlineView.Navigate(parent);

            // The outline shows nodes only, so a reference from an object between nodes jumps to the node above it
            if (!HavokBehaviorSymbols.IsOutlineNode(parent))
            {
                var node = Symbols.FindOutlineNode(parent);
                GUI.Tooltip(LOC.Get("HAVOK_UsedBy_Entry_Via_TT",
                    node == null ? "-" : HavokBehaviorSymbols.GetOutlineLabel(node)));
            }
        }

        ImGui.EndChild();

        // After the list is drawn, since the edit changes it
        if (unlinkParent != null)
            ActionsView.Unlink(unlinkParent, selected);
    }
}

/// <summary>
/// What the Contents window lists, picked in the Behavior Categories window
/// </summary>
public enum HavokBehaviorContentsMode
{
    Outline,
    Animations,
    StateMachines,
    Selectors,
    Clips,
    Variables,
    Problems
}

/// <summary>
/// The window the property panel shows the selection of
/// </summary>
public enum HavokBehaviorSelectionSource
{
    Contents,
    Graph
}
