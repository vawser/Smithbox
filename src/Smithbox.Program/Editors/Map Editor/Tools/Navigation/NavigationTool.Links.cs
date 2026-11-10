using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The View tab's Links group (Navigation Editor Plan, Phase 3): adding a link (a pair of user edges) between two faces
/// picked in the viewport, and removing one from the face inspector. See NavUserEdges for how the game lays them out.
/// </summary>
public partial class NavigationTool
{
    private bool LinkMode;
    private bool LinkBothWays;
    private int LinkClearance = NavUserEdges.DefaultClearance;
    private int LinkEdgeData = NavUserEdges.DefaultEdgeData;
    private string LinkEdgeDataText = NavUserEdges.DefaultEdgeData.ToString("X");

    /// <summary>The face the link being added starts from, picked with the first click.</summary>
    private NavFaceHit LinkStart;

    /// <summary>The text of why the last link edit was refused, or null.</summary>
    private string LinkError;

    private void DisplayLinks()
    {
        // DS3, BB and SDT have no user edges in their meshes: their links are setups
        if (!Bank.Profile.HasMeshUserEdges)
        {
            DisplaySetupLinks();
            return;
        }

        // Links
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Link_Header"), LOC.Get("MAP_Navigation_Link_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Link_Mode")}##navigationLinkMode", ref LinkMode))
        {
            LinkStart = null;
            LinkError = null;

            if (LinkMode)
            {
                PickMode = false;
                PaintMode = false;
                StopGateMode();
                StopVertexEdit();
            }
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Link_Mode_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Link_Both_Ways")}##navigationLinkBothWays", ref LinkBothWays);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Link_Both_Ways_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##navigationLinkEdgeData", ref LinkEdgeDataText, 16);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Link_Edge_Data_TT"));
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (TryParseHex(LinkEdgeDataText, out var parsed))
                LinkEdgeData = (int)parsed;
            LinkEdgeDataText = ((uint)LinkEdgeData).ToString("X");
        }

        // The edge data bits the game's cost reads
        foreach (var (bit, name) in NavFaceDataBits.Edge)
        {
            bool on = (LinkEdgeData & bit) != 0;
            if (ImGui.Checkbox($"0x{bit:X} {LOC.Get($"MAP_Navigation_Bit_{name}")}##navigationLinkBit_{bit}", ref on))
            {
                LinkEdgeData = on ? LinkEdgeData | (int)bit : LinkEdgeData & ~(int)bit;
                LinkEdgeDataText = ((uint)LinkEdgeData).ToString("X");
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Bit_{name}_TT"));
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderInt("##navigationLinkClearance", ref LinkClearance, 0, NavUserEdges.DefaultClearance, $"{LOC.Get("MAP_Navigation_Link_Clearance")} %d");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Link_Clearance_TT"));

        if (LinkMode && ShowOverlay)
            GUI.WrappedText(LinkStart == null
                ? LOC.Get("MAP_Navigation_Link_Pick_Start")
                : LOC.Get("MAP_Navigation_Link_Pick_Target", LinkStart.Face, LinkStart.File.Name));

        if (LinkMode && LinkStart != null && ImGui.Button($"{LOC.Get("MAP_Navigation_Link_Cancel")}##navigationLinkCancel"))
        {
            LinkStart = null;
            SetPicked(null);
        }

        if (LinkError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(LinkError));
    }

    /// <summary>A click while adding links: the first picks the start face, the second the target, which adds the link.</summary>
    private void LinkClick(NavFaceHit hit)
    {
        if (hit == null)
            return;

        LinkError = null;

        if (LinkStart == null)
        {
            LinkStart = hit;
            SetPicked(hit);
            return;
        }

        var start = LinkStart;
        LinkStart = null;

        if (!Bank.Profile.HasMeshUserEdges)
        {
            AddSetup(start, hit);
            return;
        }

        if (!ReferenceEquals(start.File, hit.File))
        {
            LinkError = "MAP_Navigation_Link_Error_Other_Navmesh";
            SetPicked(null);
            return;
        }

        var world = EntryWorld(OverlayContainer, start.Entry);
        if (!Matrix4x4.Invert(world, out var inverse))
            return;

        var link = new NavUserEdgeLink(start.Face, Vector3.Transform(start.Point, inverse), hit.Face, Vector3.Transform(hit.Point, inverse),
            LinkBothWays, LinkEdgeData, (byte)Math.Clamp(LinkClearance, 0, NavUserEdges.DefaultClearance));

        EditLinks(start, mesh => NavUserEdges.Add(mesh, NavUserEdges.CachesOf(mesh), link));
        SetPicked(start);
    }

    /// <summary>Removes the link a user edge of the picked face belongs to: the edge and the edge it is paired with.</summary>
    private void RemoveLink(NavFaceHit hit, int edge)
    {
        LinkError = null;
        EditLinks(hit, mesh => NavUserEdges.Remove(mesh, NavUserEdges.CachesOf(mesh), edge));
    }

    /// <summary>
    /// The Links group for games whose links are user edge setups (DS3, BB, SDT): add one with two clicks (a face at a
    /// ledge, then where it lands, on any navmesh), and the picked navmesh's setups, each with Remove.
    /// </summary>
    private void DisplaySetupLinks()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Setup_Header"), LOC.Get("MAP_Navigation_Setup_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Link_Mode")}##navigationSetupMode", ref LinkMode))
        {
            LinkStart = null;
            LinkError = null;

            if (LinkMode)
            {
                PickMode = false;
                PaintMode = false;
                StopGateMode();
                StopVertexEdit();
            }
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Setup_Mode_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Link_Both_Ways")}##navigationSetupBothWays", ref LinkBothWays);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Setup_Both_Ways_TT"));

        if (LinkMode && ShowOverlay)
            GUI.WrappedText(LinkStart == null
                ? LOC.Get("MAP_Navigation_Setup_Pick_Start")
                : LOC.Get("MAP_Navigation_Setup_Pick_Target", LinkStart.Face, LinkStart.File.Name));

        if (LinkMode && LinkStart != null && ImGui.Button($"{LOC.Get("MAP_Navigation_Link_Cancel")}##navigationSetupCancel"))
        {
            LinkStart = null;
            SetPicked(null);
        }

        if (LinkError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(LinkError));

        // The picked navmesh's setups
        var file = (LinkStart ?? Picked)?.File;
        if (file == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Setup_None_Picked"));
            return;
        }

        var setups = NavSetups.Of(file.Document)?.Setups ?? Array.Empty<HavokUserEdgeSetup>();
        GUI.WrappedText(LOC.Get("MAP_Navigation_Setup_Count", file.Name, setups.Count));

        int remove = -1;
        for (int i = 0; i < setups.Count; i++)
        {
            var s = setups[i];
            var (a, b) = NavSetups.Centres(s);
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Setup_Remove")}##navigationSetupRemove_{i}"))
                remove = i;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Setup_Remove_TT"));
            ImGui.SameLine();
            ImGui.Text(LOC.Get(s.Direction == HavokUserEdgeDirection.Bidirectional ? "MAP_Navigation_Setup_Entry_Both" : "MAP_Navigation_Setup_Entry_One",
                i, $"{Vector3.Distance(a, b):0.##}", $"{a.Y - b.Y:0.##}", LOC.Get(s.WorldSpace ? "MAP_Navigation_Setup_World" : "MAP_Navigation_Setup_Local")));
        }

        if (remove >= 0)
            EditSetups(file, NavSetups.Remove(file, remove), "MAP_Navigation_Setup_Removed");
    }

    /// <summary>Adds a setup from the start face's nearest ledge to the target point, in the start navmesh's own space.</summary>
    private void AddSetup(NavFaceHit start, NavFaceHit target)
    {
        if (!Matrix4x4.Invert(EntryWorld(OverlayContainer, start.Entry), out var toMesh))
            return;

        var setup = NavSetups.Make(start.File.Mesh, start.Face, Vector3.Transform(start.Point, toMesh), Vector3.Transform(target.Point, toMesh),
            LinkBothWays, Bank.Profile);
        EditSetups(start.File, NavSetups.Add(start.File, setup), "MAP_Navigation_Setup_Added");
        SetPicked(start);
    }

    /// <summary>Records a setup edit as one undoable action: the file's document is replaced; the NVA isn't touched.</summary>
    private void EditSetups(NavmeshFile file, NavSetupResult result, string done)
    {
        if (OverlayContainer == null || OverlayNavigation == null)
            return;

        if (!result.Ok)
        {
            LinkError = result.Error;
            return;
        }

        LinkError = null;
        var state = new NavModelState(OverlayNavigation);
        var changes = new NavRemapResult { NvaChanged = false };
        ModelEditMap = OverlayContainer;
        View.ViewportActionManager.ExecuteAction(new NavigationModelEditAction(OverlayContainer, Bank, state, state,
            new[] { result.File }, changes, OnModelEdited));
        Smithbox.Log(this, LOC.Get(done, file.Name));
    }

    /// <summary>Makes a link edit to a face's navmesh file and records it as one undoable action.</summary>
    private void EditLinks(NavFaceHit hit, Func<IHavokNavMeshEdit, NavUserEdgeResult> edit)
    {
        if (OverlayContainer == null || OverlayNavigation == null)
            return;

        var mesh = hit.File.Mesh;
        var caches = NavUserEdges.CachesOf(mesh);

        // Only user edges move; a connection naming one would need remapping, which this doesn't do
        if (NavUserEdges.ConnectionsNameEdgesFrom(OverlayNavigation, hit.Entry.NameID, NavUserEdges.OriginalEdgeCount(mesh)))
        {
            LinkError = "MAP_Navigation_Link_Error_Connections";
            return;
        }

        var before = new NavMeshState(mesh, caches);
        var result = edit(mesh);

        // Refused edits change nothing
        if (!result.Ok)
        {
            LinkError = result.Error;
            return;
        }

        var after = new NavMeshState(mesh, caches);
        View.ViewportActionManager.ExecuteAction(new NavigationLinkEditAction(OverlayContainer, hit.File, before, after, OnNavigationEdited));
    }
}
