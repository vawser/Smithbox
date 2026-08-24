using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorGraphView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    private List<hkbBehaviorGraph> BehaviorGraphs = new();

    public HavokBehaviorGraphView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void Setup(object sourceObject)
    {
        BehaviorGraphs = HavokTreeSearch.FindAll<hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);
    }

    public void SetTabState(bool state)
    {
        Owner.Selection.InBehaviorGraphTab = state;
    }

    public void DisplayTab()
    {
        // Behavior Graphs
        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_BehaviorView_Tab_Behavior_Graphs")}##tabBehaviorGraphs"))
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

        foreach (var entry in BehaviorGraphs)
        {
            var clipName = entry.m_name;
            var selected = entry == Owner.Selection.SelectedBehaviorGraph;

            var isMatch = EditorFilters.IsMatch(Owner.PropFilter, clipName, Owner.ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{clipName}##behaviorGraph_{clipName}", selected))
            {
                Owner.Selection.SelectedBehaviorGraph = entry;
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
        return Owner.Selection.InBehaviorGraphTab && Owner.Selection.SelectedBehaviorGraph != null;
    }

    public void DisplayProperties()
    {
        var havokMeta = HavokMetaHelper.GetMeta(Project, Owner.Selection.SelectedBehaviorGraph.GetType());

        ImGui.BeginChild("havokBehaviorPropEditSection");

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(Owner.Selection.SelectedBehaviorGraph, havokMeta);

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
