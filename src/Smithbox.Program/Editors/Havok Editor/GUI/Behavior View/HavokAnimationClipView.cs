using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationClipView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<IClipGenerator> SelectedAnimationClips = new();
    private List<IClipGenerator> AnimationClips = new();

    public HavokAnimationClipView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedAnimationClips.Clear();
    }

    public void Setup(object sourceObject)
    {
        if (HavokTypeUtils.IsHKX3(Project))
        {
            AnimationClips = HavokTreeSearch.FindAll<HKLib.hk2018.hkbClipGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<IClipGenerator>();
        }
        else if (HavokTypeUtils.IsHKX2(Project))
        {
            AnimationClips = HavokTreeSearch.FindAll<HKX2.hkbClipGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList<IClipGenerator>();
        }
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    public void DisplayTab()
    {
        // Clip Generators
        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_BehaviorView_Tab_Clip_Generators")}##tabClipGenerators"))
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

        for(int i = 0; i < AnimationClips.Count; i++)
        {
            var entry = AnimationClips[i];
            var entryName = "unknown";

            if (HavokTypeUtils.IsHKX3(Project))
            {
                var curClipGenerator = (HKLib.hk2018.hkbClipGenerator)entry;
                entryName = curClipGenerator.m_name;
            }
            else if (HavokTypeUtils.IsHKX2(Project))
            {
                var curClipGenerator = (HKX2.hkbClipGenerator)entry;
                entryName = curClipGenerator.m_name;
            }

            var selected = SelectedAnimationClips.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{entryName}##clipGenerator_{entryName}{i}", selected))
            {
                if (InputManager.HasCtrlDown())
                {
                    SelectedAnimationClips.Add(entry);
                }
                else
                {
                    SelectedAnimationClips.Clear();
                    SelectedAnimationClips.Add(entry);
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
        var firstSelection = SelectedAnimationClips.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedAnimationClips.FirstOrDefault();

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
