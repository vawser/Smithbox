using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationClipView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<hkbClipGenerator> SelectedAnimationClips = new();
    private List<hkbClipGenerator> AnimationClips = new();

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
        AnimationClips = HavokTreeSearch.FindAll<hkbClipGenerator>(sourceObject, View.PropertyCache.GetCachedHavokFields);
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

        foreach (var entry in AnimationClips)
        {
            var clipName = entry.m_name;
            var selected = SelectedAnimationClips.Contains(entry);

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, clipName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{clipName}##clipGenerator_{clipName}", selected))
            {
                if(InputManager.HasCtrlDown())
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
