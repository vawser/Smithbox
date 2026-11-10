using Hexa.NET.ImGui;
using StudioCore.Application;
using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The View tab's Stitch group (Navigation Editor Plan, Phase 3): finds the connections of the drawn set whose edges no
/// longer fit, and the open boundary edges that fit each other, within the map and across its NVC files with loaded
/// neighbours, then makes those changes as one undo. See NavStitchPlanner.
/// </summary>
public partial class NavigationTool
{
    /// <summary>Which navmeshes a re-stitch looks at: the whole drawn set, the picked face's, or the selected model's.</summary>
    private enum StitchScope { All, Picked, Model }

    private StitchScope StitchOnly = StitchScope.All;

    /// <summary>Set by a hand-off from the Models tab: Find runs once the overlay is drawn.</summary>
    private bool StitchFindPending;

    private bool StitchNeighbours = true;
    private bool StitchOpenOnly = true;
    private bool StitchRemoveBroken = true;
    private bool StitchAddNew = true;
    private bool ShowStitchList;
    private float StitchSeparation = new NavStitchParameters().MaxSeparation;
    private float StitchStep = new NavStitchParameters().MaxStepHeight;
    private float StitchOverlap = new NavStitchParameters().MinOverlap;

    /// <summary>The last plan, for the map and set it was made for; dropped by any navigation edit.</summary>
    private NavStitchPlan StitchPlan;

    /// <summary>What the last apply did, shown until the next plan.</summary>
    private string StitchMessage;

    private void DisplayStitch()
    {
        // Stitch
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Stitch_Header"), LOC.Get("MAP_Navigation_Stitch_Header_TT"));

        if (OverlayNavigation == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Stitch_Needs_Overlay"));
            return;
        }

        // Scope
        int scope = (int)StitchOnly;
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Stitch_Scope_All")}##navigationStitchScopeAll", ref scope, (int)StitchScope.All);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Scope_All_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Stitch_Scope_Picked")}##navigationStitchScopePicked", ref scope, (int)StitchScope.Picked);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Picked_Only_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Stitch_Scope_Model")}##navigationStitchScopeModel", ref scope, (int)StitchScope.Model);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Scope_Model_TT"));
        StitchOnly = (StitchScope)scope;

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Stitch_Neighbours")}##navigationStitchNeighbours", ref StitchNeighbours);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Neighbours_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Stitch_Open_Only")}##navigationStitchOpenOnly", ref StitchOpenOnly);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Open_Only_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationStitchSeparation", ref StitchSeparation, 0.01f, 1f, $"{LOC.Get("MAP_Navigation_Stitch_Separation")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Separation_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationStitchStep", ref StitchStep, 0.05f, 3f, $"{LOC.Get("MAP_Navigation_Stitch_Step")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Step_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationStitchOverlap", ref StitchOverlap, 0f, 0.5f, $"{LOC.Get("MAP_Navigation_Stitch_Overlap")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Overlap_TT"));

        GUI.MultiButtonInput("navigationStitchFindActions",
            "navigationStitchFind", LOC.Get("MAP_Navigation_Stitch_Find"), LOC.Get("MAP_Navigation_Stitch_Find_TT"),
            FindStitch);

        // A hand-off from the Models tab finds at once, as the overlay of the edited map is now drawn
        if (StitchFindPending && !OverlayDirty)
        {
            StitchFindPending = false;
            FindStitch();
        }

        if (StitchMessage != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, StitchMessage);

        // The plan is only for the map and set it was made for
        var plan = StitchPlan;
        if (plan == null || plan.MapId != OverlayNavigation.MapId || plan.Set != OverlaySet)
            return;

        GUI.WrappedText(LOC.Get("MAP_Navigation_Stitch_Result", plan.Checked, plan.Broken.Count, plan.AddedInMap - plan.MissingReverse, plan.MissingReverse, plan.AddedCrossMap));
        GUI.WrappedText(plan.Neighbours.Count == 0
            ? LOC.Get("MAP_Navigation_Stitch_No_Neighbours")
            : LOC.Get("MAP_Navigation_Stitch_With_Neighbours", string.Join(", ", plan.Neighbours)));

        if (plan.IsEmpty)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_Navigation_Stitch_Nothing"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Stitch_Remove_Broken")}##navigationStitchRemove", ref StitchRemoveBroken);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Remove_Broken_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Stitch_Add_New")}##navigationStitchAdd", ref StitchAddNew);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Add_New_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Stitch_Update_Gates")}##navigationStitchGates", ref StitchUpdateGates);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Stitch_Update_Gates_TT"));

        GUI.MultiButtonInput("navigationStitchApplyActions",
            "navigationStitchApply", LOC.Get("MAP_Navigation_Stitch_Apply"), LOC.Get("MAP_Navigation_Stitch_Apply_TT"),
            () => ApplyStitch(plan));

        // The changes, one per line; the counts above stay outside the toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Stitch_List_Header"), LOC.Get("MAP_Navigation_Stitch_List_Header_TT"), ref ShowStitchList);

        if (!ShowStitchList)
            return;

        const int limit = 200;

        foreach (var broken in plan.Broken.Take(limit))
        {
            var c = broken.Connection;
            float gap = plan.BrokenGaps.GetValueOrDefault(c, float.MaxValue);
            var where = broken.Nvc?.Name ?? "NVA";
            GUI.WrappedText(LOC.Get("MAP_Navigation_Stitch_List_Broken", where, (uint)broken.MainNameID, c.FaceIndex, c.EdgeIndex, (uint)broken.TargetNameID,
                c.OppositeFaceIndex, c.OppositeEdgeIndex, gap == float.MaxValue ? LOC.Get("MAP_Navigation_Stitch_List_Bad_Index") : $"{gap:0.##} m"));
        }

        foreach (var pair in plan.Added.Take(limit))
            GUI.WrappedText(LOC.Get("MAP_Navigation_Stitch_List_Added", pair.MapA, (uint)pair.NameA, pair.FaceA, pair.EdgeA, pair.MapB, (uint)pair.NameB, pair.FaceB, pair.EdgeB));

        if (plan.Broken.Count > limit || plan.Added.Count > limit)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Stitch_List_More", limit));
    }

    private void FindStitch()
    {
        StitchMessage = null;
        StitchPlan = null;

        if (OverlayNavigation == null)
            return;

        int? only = null;
        if (StitchOnly == StitchScope.Picked)
        {
            if (Picked == null)
            {
                StitchMessage = LOC.Get("MAP_Navigation_Stitch_No_Pick");
                return;
            }
            only = Picked.Entry.NameID;
        }
        else if (StitchOnly == StitchScope.Model)
        {
            // The planner takes one navmesh: the model's entry in the drawn set
            var model = OverlayNavigation.Models.FirstOrDefault(m => m.ModelID == SelectedModelID);
            if (model == null)
            {
                StitchMessage = LOC.Get("MAP_Navigation_Stitch_No_Model");
                return;
            }
            if (model.Quarters.Count > 0)
            {
                StitchMessage = LOC.Get("MAP_Navigation_Stitch_Model_Quarters");
                return;
            }

            var entry = model.Entry(OverlaySet);
            if (entry == null)
            {
                StitchMessage = LOC.Get("MAP_Navigation_Stitch_Model_Not_In_Set", SelectedModelID);
                return;
            }
            only = entry.NameID;
        }

        var parameters = new NavStitchParameters { MaxSeparation = StitchSeparation, MaxStepHeight = StitchStep, MinOverlap = StitchOverlap };
        var loaded = LoadedMaps();

        NavigationMap Neighbour(string id)
        {
            if (!StitchNeighbours)
                return null;

            var other = loaded.FirstOrDefault(m => m.Name == id);
            return other != null && Bank.HasNavmeshFiles(other) ? Bank.GetMap(other) : null;
        }

        StitchPlan = NavStitchPlanner.Plan(OverlayNavigation, OverlaySet, Neighbour, parameters, only, StitchOpenOnly);
    }

    private void ApplyStitch(NavStitchPlan plan)
    {
        if (OverlayContainer == null || OverlayNavigation == null || (!StitchRemoveBroken && !StitchAddNew))
            return;

        var map = OverlayNavigation;
        var before = new NavConnectionState(map.Nva, map.Nvcs);
        var gatesBefore = StitchUpdateGates ? new NavGateState(map.Nva) : null;
        var changes = NavStitchPlanner.Apply(map, plan, StitchRemoveBroken, StitchAddNew);
        var after = new NavConnectionState(map.Nva, map.Nvcs);

        // The pairs of the map's navmeshes whose connections changed get their gates rebuilt (gates never cross maps)
        NavGateState gatesAfter = null;
        int gatePairs = 0;
        if (StitchUpdateGates)
        {
            var pairs = new HashSet<(NVA.Navmesh, NVA.Navmesh)>();
            void Pair(int a, int b)
            {
                var ea = map.FindNavmesh(a);
                var eb = map.FindNavmesh(b);
                if (ea != null && eb != null && ea != eb)
                    pairs.Add(map.Nva.Navmeshes.IndexOf(ea) < map.Nva.Navmeshes.IndexOf(eb) ? (ea, eb) : (eb, ea));
            }

            if (StitchRemoveBroken)
                foreach (var broken in plan.Broken.Where(b => b.Nvc == null))
                    Pair(broken.MainNameID, broken.TargetNameID);
            if (StitchAddNew)
                foreach (var added in plan.Added.Where(p => p.MapA == p.MapB && p.MapA == map.MapId))
                    Pair(added.NameA, added.NameB);

            gatePairs = pairs.Count;
            if (pairs.Count > 0)
            {
                NavGates.RebuildPairs(map, pairs, GateToMap(OverlayContainer), GateJoin);
                gatesAfter = new NavGateState(map.Nva);
            }
            else
                gatesBefore = null;
        }

        View.ViewportActionManager.ExecuteAction(new NavigationStitchAction(OverlayContainer, Bank, before, after, changes, OnNavigationEdited, gatesBefore, gatesAfter));
        if (gatesAfter != null)
            OverlayContainer.MarkDirty(MapFileType.NVA);

        StitchPlan = null;
        StitchMessage = LOC.Get("MAP_Navigation_Stitch_Applied", StitchRemoveBroken ? changes.Dropped : 0, StitchAddNew ? changes.Changed : 0)
            + (gatePairs > 0 ? " " + LOC.Get("MAP_Navigation_Stitch_Gates_Updated", gatePairs) : "");
    }
}
