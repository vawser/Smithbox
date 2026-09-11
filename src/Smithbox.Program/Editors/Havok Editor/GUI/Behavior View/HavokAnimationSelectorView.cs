using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationSelectorView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<ICustomManualSelectorGenerator> SelectedAnimSelectors = new();
    private List<ICustomManualSelectorGenerator> AnimSelectors = new();

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
        if (HavokTypeUtils.IsHKX3(Project))
        {
            AnimSelectors = HavokTreeSearch.FindAll<HKLib.hk2018.CustomManualSelectorGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<ICustomManualSelectorGenerator>();
        }
        else if (HavokTypeUtils.IsHKX2(Project))
        {
            AnimSelectors = HavokTreeSearch.FindAll<HKX2.CustomManualSelectorGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<ICustomManualSelectorGenerator>();
        }
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

        for (int i = 0; i < AnimSelectors.Count; i++)
        {
            var entry = AnimSelectors[i];
            var entryName = "unknown";

            if (HavokTypeUtils.IsHKX3(Project))
            {
                var curCMSG = (HKLib.hk2018.CustomManualSelectorGenerator)entry;
                entryName = curCMSG.m_name;
            }
            else if (HavokTypeUtils.IsHKX2(Project))
            {
                var curCMSG = (HKX2.CustomManualSelectorGenerator)entry;
                entryName = curCMSG.m_name;
            }

            var selected = SelectedAnimSelectors.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{entryName}##animSelector_{entryName}{i}", selected))
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
