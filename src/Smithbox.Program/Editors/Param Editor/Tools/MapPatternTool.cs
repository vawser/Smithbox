using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Map Pattern (Tools window, Data): one map pattern as a whole. A pattern is the rows with one patternId across
/// LotResultPlayAreaParam (play area, Nightlord and bosses), LotResultMapPatternFlag (modifiers, among them the
/// Shifting Earth, 10–15), LotResultSmallBaseAndSpot (the small base at each attach point) and ManualMapPattern.
/// Shows them together with links to the rows and the Map Editor, and clones a pattern to a new ID in one undo step.
/// Shown when the project has a lot result param with a patternId field. Analysis doc P3.
/// </summary>
public class MapPatternTool
{
    private readonly ParamEditorView View;
    private readonly ProjectEntry Project;

    private int PatternId = -1;
    private int CloneTargetId = -1;
    private int CloneTargetFor = int.MinValue;

    private string Filter = "";

    // The patterns list, rebuilt when the params change
    private SortedDictionary<int, string> Patterns;
    private int PatternsVersion = -1;

    private const string ShiftingEarthEnum = "PATTERN_MODIFIER";
    private const string NightlordEnum = "NIGHTLORD";

    public MapPatternTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private bool CanUse()
    {
        var param = MapPatternLinks.GetParam(Project, MapPatternLinks.LotSiteParam)
            ?? MapPatternLinks.GetParam(Project, MapPatternLinks.LotPlayAreaParam);

        return param != null && param.Rows.Count > 0 && MapPatternLinks.HasFields(param.Rows.First(), "patternId");
    }

    public void Display()
    {
        if (!CanUse())
            return;

        if (!ToolSections.Header($"{LOC.Get("PARAM_MapPattern_Header")}##mapPatternToolHeader"))
            return;

        ImGui.BeginChild("MapPatternToolSection", ImGuiChildFlags.Borders);

        DisplayContents();

        ImGui.EndChild();
    }

    private void DisplayContents()
    {
        var version = View.Editor.ActionManager.Version;

        if (Patterns == null || PatternsVersion != version)
        {
            Patterns = MapPatternLinks.GetPatterns(Project);
            PatternsVersion = version;
        }

        if (Patterns.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_MapPattern_None"));
            return;
        }

        if (PatternId < 0 || !Patterns.ContainsKey(PatternId))
            PatternId = Patterns.Keys.First();

        // Pattern
        GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Pattern_Header"), LOC.Get("PARAM_MapPattern_Pattern_Header_TT"));
        DisplayPatternPicker();

        var playAreas = MapPatternLinks.PatternRows(Project, MapPatternLinks.LotPlayAreaParam, PatternId);
        var flags = MapPatternLinks.PatternRows(Project, MapPatternLinks.LotFlagParam, PatternId);
        var sites = MapPatternLinks.PatternRows(Project, MapPatternLinks.LotSiteParam, PatternId);
        var manual = MapPatternLinks.PatternRows(Project, MapPatternLinks.ManualPatternParam, PatternId);

        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Summary_Header"), LOC.Get("PARAM_MapPattern_Summary_Header_TT"));
        DisplaySummary(playAreas, flags, sites, manual);

        // Play Area
        if (playAreas.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Play_Area_Header"), LOC.Get("PARAM_MapPattern_Play_Area_Header_TT"));
            DisplayPlayAreas(playAreas);
        }

        // Modifiers
        if (flags.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Flags_Header"), LOC.Get("PARAM_MapPattern_Flags_Header_TT"));
            DisplayFlags(flags);
        }

        // Sites
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Sites_Header"), LOC.Get("PARAM_MapPattern_Sites_Header_TT"));
        DisplaySites(sites);

        // Clone
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_MapPattern_Clone_Header"), LOC.Get("PARAM_MapPattern_Clone_Header_TT"));
        DisplayClone(playAreas, flags, sites, manual);
    }

    private void DisplayPatternPicker()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##mapPatternFilter", LOC.Get("PARAM_MapPattern_Filter_Hint"), ref Filter, 128);
        GUI.Tooltip(LOC.Get("PARAM_MapPattern_Filter_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##mapPatternCombo", PatternLabel(PatternId), ImGuiComboFlags.HeightLarge))
        {
            foreach (var (id, _) in Patterns)
            {
                var label = PatternLabel(id);

                if (!string.IsNullOrEmpty(Filter) && !label.Contains(Filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##mapPattern{id}", id == PatternId))
                    PatternId = id;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_MapPattern_Combo_TT"));
    }

    private string PatternLabel(int id) =>
        Patterns.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name) ? $"{id}: {name}" : $"{id}";

    private void DisplaySummary(List<Param.Row> playAreas, List<Param.Row> flags, List<Param.Row> sites, List<Param.Row> manual)
    {
        // The Shifting Earth is the modifier from 10 (Limveld) to 15; its tens digit in the m60 tile IDs is modifier - 10
        var shifting = flags
            .Where(f => MapPatternLinks.HasFields(f, "modifier"))
            .Select(f => MapPatternLinks.GetInt(f, "modifier"))
            .FirstOrDefault(m => m is >= 10 and <= 15, -1);

        if (shifting >= 0)
            ImGui.TextUnformatted(LOC.Get("PARAM_MapPattern_Shifting_Earth", MapPatternLinks.EnumLabel(Project, ShiftingEarthEnum, shifting)));

        var boss = flags
            .Where(f => MapPatternLinks.HasFields(f, "targetBoss"))
            .Select(f => MapPatternLinks.GetInt(f, "targetBoss"))
            .FirstOrDefault(b => b >= 0, -1);

        if (boss >= 0)
            ImGui.TextUnformatted(LOC.Get("PARAM_MapPattern_Nightlord", MapPatternLinks.EnumLabel(Project, NightlordEnum, boss)));

        ImGui.TextDisabled(LOC.Get("PARAM_MapPattern_Counts", playAreas.Count, flags.Count, sites.Count, manual.Count));
    }

    private void DisplayPlayAreas(List<Param.Row> rows)
    {
        foreach (var row in rows)
        {
            var parts = new List<string>();

            foreach (var field in new[] { "playArea1", "playArea2", "bossId1", "bossId2", "extraBossId1", "extraBossId2" })
            {
                if (MapPatternLinks.HasFields(row, field))
                {
                    var value = MapPatternLinks.GetInt(row, field);

                    if (value > 0)
                        parts.Add($"{field} {value}");
                }
            }

            RowLink(MapPatternLinks.LotPlayAreaParam, row, string.Join(", ", parts));
        }
    }

    private void DisplayFlags(List<Param.Row> rows)
    {
        foreach (var row in rows)
        {
            var modifier = MapPatternLinks.HasFields(row, "modifier") ? MapPatternLinks.GetInt(row, "modifier") : 0;
            var flag = MapPatternLinks.HasFields(row, "eventFlag") ? MapPatternLinks.GetInt(row, "eventFlag") : 0;

            var text = MapPatternLinks.EnumLabel(Project, ShiftingEarthEnum, modifier);

            if (flag != 0)
                text += $", {LOC.Get("PARAM_MapPattern_Event_Flag", unchecked((uint)flag))}";

            RowLink(MapPatternLinks.LotFlagParam, row, text);
        }
    }

    private void DisplaySites(List<Param.Row> sites)
    {
        if (sites.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_MapPattern_No_Sites"));
            return;
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY;
        var height = Math.Min(sites.Count + 1, 16) * ImGui.GetFrameHeightWithSpacing();

        if (!ImGui.BeginTable("mapPatternSites", 4, flags, new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("PARAM_MapPattern_Column_Attach"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("PARAM_MapPattern_Column_Base"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("PARAM_MapPattern_Column_Variation"));
        ImGui.TableSetupColumn(LOC.Get("PARAM_MapPattern_Column_Row"));
        ImGui.TableHeadersRow();

        for (var i = 0; i < sites.Count; i++)
        {
            var site = sites[i];
            var attachId = MapPatternLinks.GetInt(site, "attachId");
            var attachRow = MapPatternLinks.FindRow(Project, MapPatternLinks.AttachPointParam, attachId);
            var baseId = MapPatternLinks.GetInt(site, "smallBaseMapId");
            var baseMap = MapPatternLinks.SmallBaseMap(baseId);
            var baseRow = MapPatternLinks.FindRow(Project, MapPatternLinks.SmallBaseParam, baseId);
            var variation = MapPatternLinks.GetInt(site, "variationId");

            ImGui.TableNextRow();

            // Attach point: click shows the region in the Map Editor
            ImGui.TableSetColumnIndex(0);
            var attachLabel = attachRow != null ? MapPatternLinks.RowLabel(attachRow) : $"{attachId}";

            if (ImGui.Selectable($"{attachLabel}##mapPatternAttach{i}") && attachRow != null)
                MapPatternLinks.ViewAttachPoint(Project, attachRow);
            GUI.Tooltip(attachRow != null
                ? LOC.Get("PARAM_MapPattern_Attach_TT", string.Join(", ", MapPatternLinks.AttachCandidates(Project, attachRow)))
                : LOC.Get("PARAM_MapPlacement_No_Attach_Row", attachId));

            // Small base: click opens it with the Session Preview at the variation
            ImGui.TableSetColumnIndex(1);
            var baseLabel = baseRow != null && !string.IsNullOrWhiteSpace(baseRow.Name) ? $"{baseMap} {baseRow.Name}" : baseMap ?? $"{baseId}";

            if (ImGui.Selectable($"{baseLabel}##mapPatternBase{i}") && baseMap != null)
                MapPatternLinks.OpenBase(baseMap, variation);
            GUI.Tooltip(LOC.Get("PARAM_MapPlacement_Open_Base_TT", variation));

            ImGui.TableSetColumnIndex(2);
            ImGui.TextUnformatted($"{variation}");

            ImGui.TableSetColumnIndex(3);

            if (ImGui.SmallButton($"{site.ID}##mapPatternRow{i}"))
                EditorCommandQueue.AddCommand($"param/select/-1/{MapPatternLinks.LotSiteParam}/{site.ID}");
            GUI.Tooltip(LOC.Get("PARAM_MapPattern_Row_TT"));
        }

        ImGui.EndTable();
    }

    /// <summary>A line for a row: the summary text, and a click selects the row</summary>
    private void RowLink(string param, Param.Row row, string text)
    {
        if (ImGui.Selectable($"{row.ID}: {text}##mapPatternRow_{param}_{row.ID}"))
            EditorCommandQueue.AddCommand($"param/select/-1/{param}/{row.ID}");
        GUI.Tooltip(LOC.Get("PARAM_MapPattern_Row_TT"));
    }

    private void DisplayClone(List<Param.Row> playAreas, List<Param.Row> flags, List<Param.Row> sites, List<Param.Row> manual)
    {
        if (CloneTargetFor != PatternId)
        {
            CloneTargetFor = PatternId;
            CloneTargetId = Patterns.Keys.Max() + 1;
        }

        GUI.SetInputWidth();
        ImGui.InputInt($"{LOC.Get("PARAM_MapPattern_Clone_Target")}##mapPatternCloneTarget", ref CloneTargetId);
        GUI.Tooltip(LOC.Get("PARAM_MapPattern_Clone_Target_TT"));

        if (Patterns.ContainsKey(CloneTargetId))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPattern_Clone_Taken", CloneTargetId));
            return;
        }

        if (CloneTargetId < 0)
            return;

        GUI.MultiButtonInput("mapPatternCloneActions",
            "clonePattern", LOC.Get("PARAM_MapPattern_Clone"), LOC.Get("PARAM_MapPattern_Clone_TT", PatternId, CloneTargetId),
            () => ClonePattern(playAreas, flags, sites, manual));
    }

    /// <summary>
    /// Copies every row of the pattern to the new pattern ID, each param's copies taking the IDs after its highest
    /// row, as one undo step
    /// </summary>
    private void ClonePattern(List<Param.Row> playAreas, List<Param.Row> flags, List<Param.Row> sites, List<Param.Row> manual)
    {
        var actions = new List<EditorAction>();

        foreach (var (paramName, rows) in new[]
                 {
                     (MapPatternLinks.LotPlayAreaParam, playAreas),
                     (MapPatternLinks.LotFlagParam, flags),
                     (MapPatternLinks.LotSiteParam, sites),
                     (MapPatternLinks.ManualPatternParam, manual)
                 })
        {
            var param = MapPatternLinks.GetParam(Project, paramName);

            if (param == null || rows.Count == 0)
                continue;

            var nextId = param.Rows.Max(r => r.ID) + 1;
            var templates = new List<Param.Row>();

            foreach (var row in rows)
            {
                var template = new Param.Row(row) { ID = nextId++, Name = row.Name ?? "" };
                var cell = template["patternId"];

                if (cell == null)
                    continue;

                var value = cell.Value;
                value.Value = Convert.ChangeType(CloneTargetId, value.Value.GetType());
                templates.Add(template);
            }

            actions.Add(new AddParamsAction(View.Editor, param, paramName, templates, false, false));
        }

        if (actions.Count == 0)
            return;

        var compound = new CompoundAction(actions) { DisplayName = LOC.Get("PARAM_MapPattern_Clone_History", PatternId, CloneTargetId) };
        View.Editor.ActionManager.ExecuteAction(compound);

        PatternId = CloneTargetId;
        Patterns = null;
    }
}
