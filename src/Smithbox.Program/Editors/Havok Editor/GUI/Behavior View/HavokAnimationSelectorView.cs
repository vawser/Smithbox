using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationSelectorView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<CustomManualSelectorGenerator> SelectedAnimSelectors = new();
    private List<CustomManualSelectorGenerator> AnimSelectors = new();

    public HavokAnimationSelectorView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedAnimSelectors.Clear();
    }

    public void Setup(object sourceObject)
    {
        AnimSelectors = HavokTreeSearch.FindAll<CustomManualSelectorGenerator>(sourceObject, View.PropertyCache.GetCachedHavokFields);
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    public void DisplayTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_BehaviorView_Tab_Anim_Selectors")}##tabAnimSelectors"))
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

        foreach (var entry in AnimSelectors)
        {
            var clipName = entry.m_name;
            var selected = SelectedAnimSelectors.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, clipName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{clipName}##animSelector_{clipName}", selected))
            {
                if (InputManager.HasCtrlDown())
                {
                    SelectedAnimSelectors.Add(entry);
                }
                else
                {
                    SelectedAnimSelectors.Clear();
                    SelectedAnimSelectors.Add(entry);
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
        var firstSelection = SelectedAnimSelectors.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedAnimSelectors.FirstOrDefault();

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
