using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Session Preview: hides or ghosts the assets and enemies a session wouldn't place, for a chosen map variation and
/// random slot (<see cref="NrPlacementFilter"/>). Display only: it sets <see cref="Entity.PlacementPreview"/>, which
/// is kept apart from the user's own visibility, and never touches the MSB.
/// Shown when a loaded map has parts with a Variation field, so it follows the data, not the game.
/// </summary>
public class SessionPreviewTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    public bool Enabled;

    // The random slot (0–99) the preview uses, or null for any: RandomAppearParamID then filters nothing
    private int? Slot;

    // The chosen variation per map ID; a map not in here is "any"
    private readonly Dictionary<string, int> MapVariations = new(StringComparer.OrdinalIgnoreCase);

    // What the last pass found, per loaded map with placement parts
    private readonly Dictionary<string, MapStats> Stats = new(StringComparer.OrdinalIgnoreCase);

    // Lot result counts per (AABB small-base map, variation), from LotResultSmallBaseAndSpot when present
    private Dictionary<(int Map, int Variation), int> LotCounts;

    private bool HasRandomAppearParam;

    // Set when the preview has changed something, so turning it off knows to put the parts back
    private bool HasApplied;

    // The loaded maps and edit version of the last pass, to redo it when either changes
    private long LastSignature = long.MinValue;
    private bool Dirty = true;

    /// <summary>True once a loaded map has parts this tool can filter</summary>
    public bool CanUse => Stats.Count > 0;

    public SessionPreviewTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Every frame: redoes the pass when maps were loaded or unloaded, or the maps were edited
    /// </summary>
    public void Update()
    {
        // While the preview is off, only the loaded maps matter (for the section and its counts), so edits elsewhere
        // don't rescan every part
        var signature = ComputeMapSignature();

        if (Enabled || HasApplied)
            signature = signature * 31 + View.ViewportActionManager.Version;

        if (signature == LastSignature && !Dirty)
            return;

        LastSignature = signature;
        Dirty = false;

        Apply();
    }

    public void OnMapUnloaded(MapContainer map)
    {
        if (map?.Name != null)
            Stats.Remove(map.Name);

        Dirty = true;
    }

    private long ComputeMapSignature()
    {
        long signature = 17;

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var map = entry.Value.MapContainer;

            if (map == null)
                continue;

            signature = signature * 31 + RuntimeHelpers.GetHashCode(map) + map.Objects.Count;
        }

        return signature;
    }

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_SessionPreview_Intro"));
        GUI.Spacer();

        if (ImGui.Checkbox($"{LOC.Get("MAP_SessionPreview_Enable")}##sessionPreviewEnable", ref Enabled))
            Dirty = true;
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Enable_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_SessionPreview_Ghost")}##sessionPreviewGhost", ref CFG.Current.MapEditor.MapEditor_SessionPreview_Ghost))
            Dirty = true;
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Ghost_TT"));

        if (Enabled && HasApplied)
        {
            var total = Stats.Values.Sum(x => x.Total);
            var notPlaced = Stats.Values.Sum(x => x.NotPlaced);
            GUI.WrappedText(LOC.Get("MAP_SessionPreview_Status", notPlaced, total));
        }

        DisplayRandomSlot();
        DisplayMaps();
    }

    private void DisplayRandomSlot()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SessionPreview_Random_Header"), LOC.Get("MAP_SessionPreview_Random_Header_TT"));

        if (!HasRandomAppearParam)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_SessionPreview_No_RandomAppear"));
            return;
        }

        var fixedSlot = Slot.HasValue;

        if (ImGui.Checkbox($"{LOC.Get("MAP_SessionPreview_Fixed_Slot")}##sessionPreviewFixedSlot", ref fixedSlot))
        {
            Slot = fixedSlot ? Random.Shared.Next(NrPlacementFilter.SlotCount) : null;
            Dirty = true;
        }
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Fixed_Slot_TT"));

        if (!Slot.HasValue)
            return;

        var slot = Slot.Value;

        GUI.SetInputWidth();
        if (ImGui.SliderInt($"{LOC.Get("MAP_SessionPreview_Slot")}##sessionPreviewSlot", ref slot, 0, NrPlacementFilter.SlotCount - 1))
        {
            Slot = slot;
            Dirty = true;
        }
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Slot_TT"));

        if (ImGui.Button($"{LOC.Get("MAP_SessionPreview_Roll")}##sessionPreviewRoll"))
        {
            Slot = Random.Shared.Next(NrPlacementFilter.SlotCount);
            Dirty = true;
        }
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Roll_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("MAP_SessionPreview_Refresh")}##sessionPreviewRefresh"))
            Dirty = true;
        GUI.Tooltip(LOC.Get("MAP_SessionPreview_Refresh_TT"));
    }

    private void DisplayMaps()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SessionPreview_Maps_Header"), LOC.Get("MAP_SessionPreview_Maps_Header_TT"));

        var maps = Stats.Values.Where(x => x.Variations.Count > 0).OrderBy(x => x.MapId).ToList();

        if (maps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_SessionPreview_No_Variations"));
            return;
        }

        var tblFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit;

        if (!ImGui.BeginTable("sessionPreviewMaps", 3, tblFlags))
            return;

        ImGui.TableSetupColumn(LOC.Get("MAP_SessionPreview_Column_Map"));
        ImGui.TableSetupColumn(LOC.Get("MAP_SessionPreview_Column_Variation"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("MAP_SessionPreview_Column_Placed"));
        ImGui.TableHeadersRow();

        foreach (var stats in maps)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text(stats.MapId);

            var alias = AliasHelper.GetMapNameAlias(Project, stats.MapId);
            if (!string.IsNullOrEmpty(alias))
                GUI.Tooltip(alias);

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(-1);

            var hasChoice = MapVariations.TryGetValue(stats.MapId, out var chosen);
            var preview = hasChoice ? VariationLabel(stats, chosen) : LOC.Get("MAP_SessionPreview_Any");

            if (ImGui.BeginCombo($"##sessionPreviewVariation_{stats.MapId}", preview))
            {
                if (ImGui.Selectable(LOC.Get("MAP_SessionPreview_Any"), !hasChoice))
                {
                    MapVariations.Remove(stats.MapId);
                    Dirty = true;
                }

                foreach (var value in stats.Variations.Keys.OrderBy(x => x))
                {
                    if (ImGui.Selectable($"{VariationLabel(stats, value)}##sessionPreviewValue{value}", hasChoice && chosen == value))
                    {
                        MapVariations[stats.MapId] = value;
                        Dirty = true;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MAP_SessionPreview_Variation_TT"));

            ImGui.TableSetColumnIndex(2);
            ImGui.Text(Enabled ? $"{stats.Total - stats.NotPlaced} / {stats.Total}" : $"{stats.Total}");
        }

        ImGui.EndTable();
    }

    private string VariationLabel(MapStats stats, int value)
    {
        var count = stats.Variations.TryGetValue(value, out var c) ? c : 0;
        if (LotCounts != null && NrPlacementFilter.TryGetSmallBaseId(stats.MapId, out var smallBase) &&
            LotCounts.Keys.Any(k => k.Map == smallBase))
        {
            var lots = LotCounts.TryGetValue((smallBase, value), out var l) ? l : 0;
            return LOC.Get("MAP_SessionPreview_Value_Lots", value, count, lots);
        }

        return LOC.Get("MAP_SessionPreview_Value", value, count);
    }

    /// <summary>
    /// Turns the preview on with a map's variation set, as the Map Data overview's variation lines do
    /// </summary>
    public void PreviewMapVariation(string mapId, int? variation)
    {
        if (mapId == null)
            return;

        if (variation is { } value)
            MapVariations[mapId] = value;
        else
            MapVariations.Remove(mapId);

        Enabled = true;
        Dirty = true;
    }

    /// <summary>
    /// The variation the preview uses for a map, or null for any
    /// </summary>
    public int? GetMapVariation(string mapId) =>
        Enabled && mapId != null && MapVariations.TryGetValue(mapId, out var value) ? value : null;

    /// <summary>
    /// Sets each placement part's preview state, and gathers the per-map stats the window shows
    /// </summary>
    private void Apply()
    {
        var table = RandomAppearTable.Build(Project);
        HasRandomAppearParam = table != null;

        var slot = HasRandomAppearParam ? Slot : null;
        var ghost = CFG.Current.MapEditor.MapEditor_SessionPreview_Ghost;
        var changed = false;

        Stats.Clear();

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var map = entry.Value.MapContainer;

            if (map?.Name == null)
                continue;

            MapStats stats = null;
            int? variation = MapVariations.TryGetValue(map.Name, out var v) ? v : null;

            foreach (var obj in map.Objects)
            {
                if (!NrPlacementFilter.TryGetFields(obj, out var fields))
                    continue;

                stats ??= new MapStats(map.Name);
                stats.Total++;

                foreach (var value in NrPlacementFilter.MatchingVariations(fields, map.Name))
                {
                    stats.Variations[value] = stats.Variations.GetValueOrDefault(value) + 1;
                }

                var state = PlacementPreviewState.Placed;

                if (Enabled && !NrPlacementFilter.IsPlaced(fields, map.Name, variation, slot, table))
                {
                    state = ghost ? PlacementPreviewState.Ghosted : PlacementPreviewState.Hidden;
                    stats.NotPlaced++;
                }

                if (obj.PlacementPreview != state)
                {
                    obj.PlacementPreview = state;
                    changed = true;
                }
            }

            if (stats != null)
                Stats[map.Name] = stats;
        }

        LotCounts = Stats.Keys.Any(x => NrPlacementFilter.TryGetSmallBaseId(x, out _)) ? NrPlacementFilter.BuildLotCounts(Project) : null;

        HasApplied = Enabled;

        if (changed)
            View.DelayPicking();
    }

    private class MapStats
    {
        public readonly string MapId;
        public int Total;
        public int NotPlaced;

        // Parts placed by each map variation (an asset that also matches +10 counts for both)
        public readonly Dictionary<int, int> Variations = new();

        public MapStats(string mapId)
        {
            MapId = mapId;
        }
    }
}
