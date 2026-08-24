using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public class HavokStateMachineView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<hkbStateMachine> SelectedStateMachines = new();
    private List<hkbStateMachine> StateMachines = new();

    public HavokStateMachineView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedStateMachines.Clear();
    }

    public void Setup(object sourceObject)
    {
        StateMachines = HavokTreeSearch.FindAll<hkbStateMachine>(sourceObject, View.PropertyCache.GetCachedHavokFields);
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    public void DisplayTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_BehaviorView_Tab_State_Machines")}##tabStateMachines"))
        {
            SetTabState(true);

            DisplayTabHeader();
            DisplayTabContents();

            ImGui.EndTabItem();
        }
        else
        {
            SetTabState(false);
        }

    }

    public void DisplayTabContents()
    {
        ImGui.BeginChild("havokBehaviorElementListSection");

        foreach (var entry in StateMachines)
        {
            var clipName = entry.m_name;
            var selected = SelectedStateMachines.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, clipName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{clipName}##stateMachine_{clipName}", selected))
            {
                if (InputManager.HasCtrlDown())
                {
                    SelectedStateMachines.Add(entry);
                }
                else
                {
                    SelectedStateMachines.Clear();
                    SelectedStateMachines.Add(entry);
                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayTabHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_TabListHeader", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();
    }

    public bool CanDisplayProperties()
    {
        var firstSelection = SelectedStateMachines.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedStateMachines.FirstOrDefault();

        if (firstSelection == null)
            return;

        var havokMeta = HavokMetaHelper.GetMeta(Project, firstSelection.GetType());

        ImGui.BeginChild("havokBehaviorPropEditSection");

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(firstSelection, havokMeta);

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
