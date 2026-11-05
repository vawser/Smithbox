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

    /// <summary>
    /// The names the file's indices refer to, or null if the shown file isn't a behavior graph
    /// </summary>
    public HavokBehaviorSymbols Symbols;

    public HavokBehaviorView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        
        AnimationClipView = new(view, this, project);
        AnimationSelectorView = new(view, this, project);
        StateMachineView = new(view, this, project);
        VariableView = new(view, this, project);
        OutlineView = new(view, this, project);
    }

    public void ResetSelection()
    {
        IsBehaviorGraph = false;
        Symbols = null;

        AnimationClipView.ResetSelection();
        AnimationSelectorView.ResetSelection();
        StateMachineView.ResetSelection();
        VariableView.ResetSelection();
        OutlineView.ResetSelection();
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

        if (HavokTypeUtils.IsHKX3(Project))
        {
            var behaviorGraphs = HavokTreeSearch.FindAll<HKLib.hk2018.hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);

            if (behaviorGraphs.Count > 0)
                IsBehaviorGraph = true;
        }
        else if (HavokTypeUtils.IsHKX2(Project))
        {
            var behaviorGraphs = HavokTreeSearch.FindAll<HKX2.hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);

            if (behaviorGraphs.Count > 0)
                IsBehaviorGraph = true;
        }

        AnimationClipView.Setup(sourceObject);
        AnimationSelectorView.Setup(sourceObject);
        StateMachineView.Setup(sourceObject);
        VariableView.Setup(sourceObject);
        OutlineView.Setup(sourceObject);
    }

    public void Draw(object sourceObject)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable;

        if (ImGui.BeginTable($"behaviorLayoutTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Tabs", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Properties", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            DisplayBehaviorTabs();

            ImGui.TableSetColumnIndex(1);

            DisplayTabPropertyHeader();
            DisplayProperties();

            ImGui.EndTable();
        }
    }

    public void DisplayHeader(bool minimized = false)
    {
        var previewName = LOC.Get(View.Selection.PropertyViewType.GetDisplayName());

        if(minimized)
            ImGui.SetNextItemWidth(100f * DPI.UIScale());

        if (ImGui.BeginCombo("##subEditorMode", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(HavokPropertyViewType)))
            {
                var curType = (HavokPropertyViewType)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == View.Selection.PropertyViewType))
                {
                    View.Selection.PropertyViewType = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_PropertyView_DisplayType_TT"));
    }

    public void DisplayBehaviorTabs()
    {
        // EndTabBar must only be called when BeginTabBar returned true
        if (ImGui.BeginTabBar("behaviorTabs", ImGuiTabBarFlags.FittingPolicyResizeDown))
        {
            OutlineView.DisplayTab();
            StateMachineView.DisplayTab();
            AnimationSelectorView.DisplayTab();
            AnimationClipView.DisplayTab();
            VariableView.DisplayTab();

            ImGui.EndTabBar();
        }
    }

    public void DisplayTabPropertyHeader()
    {
        View.PropertyView.DisplayHeader();
    }

    public void DisplayProperties()
    {
        DisplayUsedBy();

        if (OutlineView.CanDisplayProperties())
        {
            OutlineView.DisplayProperties();
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

        ImGui.BeginChild("havokUsedByList", new System.Numerics.Vector2(0, height), ImGuiChildFlags.Borders);

        for (var i = 0; i < parents.Count; i++)
        {
            var parent = parents[i];
            var field = HavokBehaviorSymbols.FindFieldSegment(parent, selected) ?? "?";
            var label = $"{HavokBehaviorSymbols.GetLabel(parent)}  .{field}";

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
    }
}
