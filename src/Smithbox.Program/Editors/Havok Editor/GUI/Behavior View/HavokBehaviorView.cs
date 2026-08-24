using Hexa.NET.ImGui;
using HKLib.hk2018;
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

    public HavokBehaviorView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        
        AnimationClipView = new(view, this, project);
        AnimationSelectorView = new(view, this, project);
        StateMachineView = new(view, this, project);
        VariableView = new(view, this, project);
    }

    public void ResetSelection()
    {
        IsBehaviorGraph = false;

        AnimationClipView.ResetSelection();
        AnimationSelectorView.ResetSelection();
        StateMachineView.ResetSelection();
        VariableView.ResetSelection();
    }

    public void SetupBehaviorView(object sourceObject)
    {
        // Determine if selected file is a Behavior Graph
        var behaviorGraphs = HavokTreeSearch.FindAll<hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);

        if (behaviorGraphs.Count > 0)
            IsBehaviorGraph = true;

        AnimationClipView.Setup(sourceObject);
        AnimationSelectorView.Setup(sourceObject);
        StateMachineView.Setup(sourceObject);
        VariableView.Setup(sourceObject);
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

    public void DisplayBehaviorHeader(bool minimized = false)
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
        ImGui.BeginTabBar("behaviorTabs", ImGuiTabBarFlags.FittingPolicyResizeDown);

        StateMachineView.DisplayTab();
        AnimationSelectorView.DisplayTab();
        AnimationClipView.DisplayTab();
        VariableView.DisplayTab();

        ImGui.EndTabBar();
    }

    public void DisplayTabPropertyHeader()
    {
        View.PropertyView.DisplayHeader();
    }

    public void DisplayProperties()
    {
        if (StateMachineView.CanDisplayProperties())
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
}
