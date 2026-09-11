using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokStateMachineView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<IStateMachine> SelectedStateMachines = new();
    private List<IStateMachine> StateMachines = new();

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
        if (HavokTypeUtils.IsHKX3(Project))
        {
            StateMachines = HavokTreeSearch.FindAll<HKLib.hk2018.hkbStateMachine>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<IStateMachine>();
        }
        else if (HavokTypeUtils.IsHKX2(Project))
        {
            StateMachines = HavokTreeSearch.FindAll<HKX2.hkbStateMachine>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<IStateMachine>();
        }
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

        for (int i = 0; i < StateMachines.Count; i++)
        {
            var entry = StateMachines[i];
            var entryName = "unknown";

            if (HavokTypeUtils.IsHKX3(Project))
            {
                var curSM = (HKLib.hk2018.hkbStateMachine)entry;
                entryName = curSM.m_name;
            }
            else if (HavokTypeUtils.IsHKX2(Project))
            {
                var curSM = (HKX2.hkbStateMachine)entry;
                entryName = curSM.m_name;
            }

            var selected = SelectedStateMachines.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{entryName}##stateMachine_{entryName}{i}", selected))
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
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

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
