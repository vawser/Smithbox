using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Cleanup tab of the Collision Editor: finds the collision files of the active map that no part of its MSB uses,
/// and removes them from the map's collision binders. A file is used when a part's model has its number: h001234 uses
/// h.._001234, l.._001234 and f.._001234. Files whose names don't follow that pattern are always kept.
/// </summary>
public class CollisionEditorCleanup
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CollisionEditorTool Tool;

    // The last scan: the map it was of, and the unused files it found
    private string ScannedMap;
    private List<string> Unused;

    // Files unticked in the list, which Cleanup leaves: kept across scans of the same map, so a rescan keeps the choice
    private readonly HashSet<string> Unticked = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The listed files that are ticked, which Cleanup removes.</summary>
    private List<string> Ticked => Unused?.Where(n => !Unticked.Contains(n)).ToList() ?? new List<string>();

    private bool ShowList = true;
    private bool OpenConfirm;

    private const string ConfirmPopup = "###collisionEditorCleanupConfirm";

    // h10_00_00_00_001234.hkx.dcx: the layer, the map (without its m) and the model number
    private static readonly Regex FilePattern = new(@"^([hlf])(\d\d_\d\d_\d\d_\d\d)_(\d{6})\.hkx\.dcx$", RegexOptions.IgnoreCase);

    // h001234 (collision and connect collision parts)
    private static readonly Regex ModelPattern = new(@"^h(\d{6})$", RegexOptions.IgnoreCase);

    public CollisionEditorCleanup(MapEditorView view, ProjectEntry project, CollisionEditorTool tool)
    {
        View = view;
        Project = project;
        Tool = tool;
    }

    #region Display

    public void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cleanup_Description"));

        var map = View.Selection.ResolveTargetMap(MapTargetPicker.ActiveMap);

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Cleanup_Map_Header"), LOC.Get("MAP_CollisionEditor_Cleanup_Map_Header_TT"));

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cleanup_No_Map"));
            return;
        }

        GUI.WrappedText(map.Name);
        GUI.DisplayAlias(AliasHelper.GetMapNameAlias(Project, map.Name));

        // A scan of another map doesn't apply here
        if (ScannedMap != map.Name)
        {
            ScannedMap = null;
            Unused = null;
            Unticked.Clear();
        }

        var tickedCount = Ticked.Count;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Cleanup_Actions_Header"), LOC.Get("MAP_CollisionEditor_Cleanup_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("collisionEditorCleanupActions",
            "collisionEditorCleanupFind", LOC.Get("MAP_CollisionEditor_Cleanup_Find"), LOC.Get("MAP_CollisionEditor_Cleanup_Find_TT"),
            () => Scan(map), true,

            "collisionEditorCleanupRemove", LOC.Get("MAP_CollisionEditor_Cleanup_Remove"), LOC.Get("MAP_CollisionEditor_Cleanup_Remove_TT"),
            () => OpenConfirm = true, tickedCount > 0);

        Tool.DisplayLastResult(CollisionEditorSource.Cleanup);

        DisplayConfirm(map);

        if (Unused == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cleanup_Not_Scanned"));
            return;
        }

        // Unused Files
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Cleanup_List_Header"), LOC.Get("MAP_CollisionEditor_Cleanup_List_Header_TT"), ref ShowList);

        if (Unused.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_CollisionEditor_Cleanup_None", map.Name));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cleanup_Count", Unused.Count, tickedCount));

        if (!ShowList)
            return;

        GUI.MultiButtonInput("collisionEditorCleanupTickActions",
            "collisionEditorCleanupTickAll", LOC.Get("MAP_CollisionEditor_Cleanup_Tick_All"), LOC.Get("MAP_CollisionEditor_Cleanup_Tick_All_TT"),
            Unticked.Clear,

            "collisionEditorCleanupTickNone", LOC.Get("MAP_CollisionEditor_Cleanup_Tick_None"), LOC.Get("MAP_CollisionEditor_Cleanup_Tick_None_TT"),
            () => Unticked.UnionWith(Unused));

        var height = Math.Min(Unused.Count, 12) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;

        ImGui.BeginChild("collisionEditorCleanupList", new Vector2(-1, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(Unused.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var name = Unused[i];
                var ticked = !Unticked.Contains(name);

                if (ImGui.Checkbox($"{name}##collisionEditorCleanupTick{name}", ref ticked))
                {
                    if (ticked)
                        Unticked.Remove(name);
                    else
                        Unticked.Add(name);
                }
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Cleanup_Tick_TT"));
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void DisplayConfirm(MapContainer map)
    {
        if (OpenConfirm)
        {
            ImGui.OpenPopup(ConfirmPopup);
            OpenConfirm = false;
        }

        ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;

        if (!ImGui.BeginPopupModal($"{LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Title")}{ConfirmPopup}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Text", Ticked.Count, map.Name));

        GUI.Spacer();

        GUI.MultiButtonInput("collisionEditorCleanupConfirmActions",
            "collisionEditorCleanupConfirmRemove", LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Remove"), LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Remove_TT"),
            () =>
            {
                Remove(map);
                ImGui.CloseCurrentPopup();
            },

            "collisionEditorCleanupConfirmCancel", LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Cancel"), LOC.Get("MAP_CollisionEditor_Cleanup_Confirm_Cancel_TT"),
            ImGui.CloseCurrentPopup);

        ImGui.EndPopup();
    }

    #endregion

    #region Scan and Remove

    private void Scan(MapContainer map)
    {
        ScannedMap = map.Name;
        Unused = FindUnused(map);
    }

    /// <summary>
    /// The collision files of a map that no part's model points at, in name order. Files with pending Edit tab changes,
    /// files of another map, and files whose names don't follow the h/l/f + map + number pattern are left out.
    /// </summary>
    private List<string> FindUnused(MapContainer map)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Every part with a collision model counts, enabled or not: the game reads them all from the MSB
        foreach (var entity in map.Objects.OfType<MsbEntity>())
        {
            if (entity.GetPropertyValue("ModelName") is string model && ModelPattern.Match(model) is { Success: true } match)
                used.Add(match.Groups[1].Value);
        }

        var mapNumber = map.Name.Substring(1);

        return View.HavokCollisionBank.GetMapCollisionNames(map.Name)
            .Where(name => FilePattern.Match(name) is { Success: true } match
                && string.Equals(match.Groups[2].Value, mapNumber, StringComparison.OrdinalIgnoreCase)
                && !used.Contains(match.Groups[3].Value))
            .Where(name => !Tool.HasPendingEdits(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void Remove(MapContainer map)
    {
        // Scanned again, so a part added since the scan keeps its collision; only the ticked files go
        var unused = FindUnused(map).Where(n => !Unticked.Contains(n)).ToList();

        if (unused.Count == 0)
        {
            Unused = unused;
            Tool.ReportSuccess(GetType(), CollisionEditorSource.Cleanup, false, LOC.Get("MAP_CollisionEditor_Cleanup_None", map.Name));
            return;
        }

        var success = View.HavokCollisionBank.RemoveMapCollision(map.Name, unused, out var removed);

        Tool.OnCollisionCreated();

        // Not undoable, so the result doesn't carry the undo note (wrote: false), though the binders were written
        if (success)
            Tool.ReportSuccess(GetType(), CollisionEditorSource.Cleanup, false, LOC.Get("MAP_CollisionEditor_Cleanup_Removed", removed, map.Name));
        else
            Tool.ReportFailure(GetType(), CollisionEditorSource.Cleanup, LOC.Get("MAP_CollisionEditor_Cleanup_Failed", map.Name));

        Scan(map);
    }

    #endregion
}
