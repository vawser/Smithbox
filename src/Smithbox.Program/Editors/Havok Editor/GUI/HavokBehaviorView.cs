using Hexa.NET.ImGui;
using HKLib.hk2018;
using HKLib.hk2018.castTest;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tracy;
using Veldrid.MetalBindings;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorView
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public string PropFilter = "";
    public bool ExactPropFilter = false;

    public BehaviorViewSelection Selection = new();

    private List<hkbClipGenerator> ClipGenerators = new();

    public HavokBehaviorView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void SetupBehaviorView(object sourceObject)
    {
        // Determine if selected file is a Behavior Graph
        var behaviorGraphs = HavokTreeSearch.FindAll<hkbBehaviorGraph>(sourceObject, View.PropertyCache.GetCachedHavokFields);

        if (behaviorGraphs.Count > 0)
            View.PropertyView.BehaviorView.Selection.IsBehaviorGraph = true;

        // Clip Generators
        ClipGenerators = HavokTreeSearch.FindAll<hkbClipGenerator>(sourceObject, View.PropertyCache.GetCachedHavokFields);
    }

    public void Draw(object sourceObject)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"behaviorLayoutTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Tabs", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Properties", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            DisplayBehaviorTabs();

            ImGui.TableSetColumnIndex(1);

            DisplayProperties();

            ImGui.EndTable();
        }
    }

    public void DisplayBehaviorTabs()
    {
        ImGui.BeginTabBar("behaviorTabs", ImGuiTabBarFlags.FittingPolicyResizeDown);

        if (ImGui.BeginTabItem("Clip Generators"))
        {
            Selection.InClipGeneratorTab = true;

            DisplayTabListHeader();
            DisplayClipGeneratorList();

            ImGui.EndTabItem();
        }
        else
        {
            Selection.InClipGeneratorTab = false;
        }

        ImGui.EndTabBar();
    }

    public void DisplayClipGeneratorList()
    {
        ImGui.BeginChild("havokBehaviorElementListSection");

        foreach (var entry in ClipGenerators)
        {
            var clipName = entry.m_name;
            var selected = entry == Selection.SelectedClipGenerator;

            var isMatch = EditorFilters.IsMatch(PropFilter, clipName, ExactPropFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{clipName}##clipGenerator_{clipName}", selected))
            {
                Selection.SelectedClipGenerator = entry;
            }
        }

        ImGui.EndChild();
    }


    public void DisplayTabListHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_TabListHeader", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokBehaviorTabListSearch", ref PropFilter, ref ExactPropFilter);

        ImGui.EndChild();
    }


    public void DisplayProperties()
    {
        if (Selection.InClipGeneratorTab && Selection.SelectedClipGenerator != null)
        {
            var havokMeta = HavokMetaHelper.GetMeta(Project, Selection.SelectedClipGenerator.GetType());

            GUI.SimpleHeader(
                LOC.Get("HAVOK_PropertyView_Header"),
                LOC.Get("HAVOK_PropertyView_Header_TT"));

            ImGui.BeginChild("havokBehaviorPropEditSection");

            var columnCount = 2;
            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                columnCount = 3;
            }

            ImGui.Columns(columnCount);

            View.PropertyView.HavokPropEditGeneric(Selection.SelectedClipGenerator, havokMeta);

            ImGui.Columns(1);

            ImGui.EndChild();
        }
        else
        {
            GUI.WrappedText($"No entry selected yet.");
        }
    }

    // Specific state when in Behavior View (for selected file)
    public class BehaviorViewSelection
    {
        public bool IsBehaviorGraph = false;

        public bool InClipGeneratorTab = false;

        public hkbClipGenerator SelectedClipGenerator;

        public void Reset()
        {
            IsBehaviorGraph = false;

            SelectedClipGenerator = null;
        }
    }
}
