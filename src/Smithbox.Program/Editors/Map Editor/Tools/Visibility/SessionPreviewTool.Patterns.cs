using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Session Preview, phase 4 of the analysis doc: small bases shown in place (M4), the sites of one map pattern
/// labelled on the loaded m60 tiles (M5), and the variant actions on the selection (M6).
/// </summary>
public partial class SessionPreviewTool
{
    // M5: the pattern whose sites are labelled, and the loaded regions it uses
    private int? PatternId;
    private string PatternFilter = "";
    private SortedDictionary<int, string> Patterns;
    private bool PatternLabelsDirty = true;
    private int? ShiftingFor;
    private int Shifting = -1;
    private readonly List<(Entity Region, string Label, NrPatternSites.Site Site)> PatternLabels = new();

    // M6: the values the variant actions set
    private int VariantValue;
    private bool VariantAlsoPlus10;
    private int RandomAppearRow;

    #region Small bases in place (M4)

    private void DisplaySmallBases()
    {
        var placements = View.SmallBasePlacement.Placements;

        var loadedBases = Project.Handler.MapData.PrimaryBank.Maps
            .Select(e => e.Value?.MapContainer)
            .Where(m => m is { LoadState: MapContentLoadState.Loaded } && NrPatternSites.OfBase(Project, m.Name).Count > 0)
            .Select(m => m.Name)
            .OrderBy(n => n)
            .ToList();

        if (loadedBases.Count == 0 && placements.Count == 0)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SmallBase_InPlace_Header"), LOC.Get("MAP_SmallBase_InPlace_Header_TT"));

        foreach (var baseMap in loadedBases)
        {
            ImGui.TextUnformatted(baseMap);

            if (placements.TryGetValue(baseMap, out var placement))
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("MAP_SmallBase_At", placement.RegionName, placement.AttachMapID));

                ImGui.SameLine();

                if (ImGui.SmallButton($"{LOC.Get("MAP_SmallBase_Reset")}##smallBaseInPlaceReset_{baseMap}"))
                    View.SmallBasePlacement.Reset(baseMap);
                GUI.Tooltip(LOC.Get("MAP_SmallBase_Reset_TT"));
            }

            DisplayAttachPicker(baseMap);
        }
    }

    /// <summary>
    /// The attach points a base is placed at by the lot results, with its variation there and the patterns that do it
    /// </summary>
    private void DisplayAttachPicker(string baseMap)
    {
        var choices = NrPatternSites.OfBase(Project, baseMap)
            .GroupBy(s => (s.AttachRowId, s.Variation))
            .Select(g => (g.Key.AttachRowId, g.Key.Variation, Patterns: g.Select(s => s.PatternId).Distinct().Count()))
            .OrderByDescending(c => c.Patterns)
            .ToList();

        ImGui.SetNextItemWidth(-1);

        if (!ImGui.BeginCombo($"##smallBaseAttachPicker_{baseMap}", LOC.Get("MAP_SmallBase_Place_At"), ImGuiComboFlags.HeightLarge))
        {
            GUI.Tooltip(LOC.Get("MAP_SmallBase_Place_At_TT"));
            return;
        }

        foreach (var (attachRowId, variation, patterns) in choices)
        {
            var attachRow = MapPatternLinks.FindRow(Project, MapPatternLinks.AttachPointParam, attachRowId);
            var label = LOC.Get("MAP_SmallBase_Attach_Choice", MapPatternLinks.RowLabel(attachRow) is { Length: > 0 } l ? l : $"{attachRowId}",
                variation, patterns);

            if (ImGui.Selectable($"{label}##smallBaseAttach_{baseMap}_{attachRowId}_{variation}") && attachRow != null)
                MapPatternLinks.PreviewInPlace(Project, attachRow, baseMap, variation);
        }

        ImGui.EndCombo();
    }

    #endregion

    #region Map pattern labels (M5)

    private void DisplayPattern()
    {
        if (!NrPatternSites.IsAvailable(Project))
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Pattern_Header"), LOC.Get("MAP_Pattern_Header_TT"));

        Patterns ??= MapPatternLinks.GetPatterns(Project);

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##sessionPatternFilter", LOC.Get("PARAM_MapPattern_Filter_Hint"), ref PatternFilter, 128);
        GUI.Tooltip(LOC.Get("PARAM_MapPattern_Filter_TT"));

        ImGui.SetNextItemWidth(-1);

        var preview = PatternId is { } current ? PatternLabel(current) : LOC.Get("MAP_Pattern_None");

        if (ImGui.BeginCombo("##sessionPatternCombo", preview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.Selectable(LOC.Get("MAP_Pattern_None"), PatternId == null))
            {
                PatternId = null;
                PatternLabelsDirty = true;
            }

            foreach (var id in Patterns.Keys)
            {
                var label = PatternLabel(id);

                if (!string.IsNullOrEmpty(PatternFilter) && !label.Contains(PatternFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##sessionPattern{id}", PatternId == id))
                {
                    PatternId = id;
                    PatternLabelsDirty = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Pattern_Combo_TT"));

        if (ImGui.SmallButton($"{LOC.Get("MAP_SessionPreview_Refresh")}##sessionPatternRefresh"))
        {
            Patterns = null;
            ShiftingFor = null;
            PatternLabelsDirty = true;
        }
        GUI.Tooltip(LOC.Get("MAP_Pattern_Refresh_TT"));

        if (PatternId is not { } patternId)
            return;

        // The pattern's Shifting Earth: its LotResultMapPatternFlag modifier from 10 (Limveld) to 15
        // Read once per pattern, not every frame
        if (ShiftingFor != patternId)
        {
            ShiftingFor = patternId;
            Shifting = MapPatternLinks.PatternRows(Project, MapPatternLinks.LotFlagParam, patternId)
                .Where(f => MapPatternLinks.HasFields(f, "modifier"))
                .Select(f => MapPatternLinks.GetInt(f, "modifier"))
                .FirstOrDefault(m => m is >= 10 and <= 15, -1);
        }

        var shifting = Shifting;

        if (shifting >= 0)
        {
            ImGui.TextUnformatted(LOC.Get("PARAM_MapPattern_Shifting_Earth", MapPatternLinks.EnumLabel(Project, "PATTERN_MODIFIER", shifting)));
            GUI.Tooltip(LOC.Get("MAP_Pattern_Shifting_Earth_TT", shifting - 10));
        }

        if (PatternLabelsDirty)
            RebuildPatternLabels();

        var sites = NrPatternSites.InPattern(Project, patternId);
        ImGui.TextUnformatted(LOC.Get("MAP_Pattern_Labelled", PatternLabels.Count, sites.Count));
        GUI.Tooltip(LOC.Get("MAP_Pattern_Labelled_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Pattern_Show_Labels")}##sessionPatternLabels", ref CFG.Current.MapEditor.MapEditor_SessionPreview_Pattern_Labels);
        GUI.Tooltip(LOC.Get("MAP_Pattern_Show_Labels_TT"));

        if (PatternLabels.Count > 0)
        {
            GUI.MultiButtonInput("sessionPatternActions",
                "placeLoaded", LOC.Get("MAP_Pattern_Place_Loaded"), LOC.Get("MAP_Pattern_Place_Loaded_TT"), PlaceLoadedSites);
        }
    }

    private string PatternLabel(int id) =>
        Patterns != null && Patterns.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name) ? $"{id}: {name}" : $"{id}";

    /// <summary>The loaded regions the pattern uses, each with what the pattern puts there</summary>
    private void RebuildPatternLabels()
    {
        PatternLabelsDirty = false;
        PatternLabels.Clear();

        if (PatternId is not { } patternId)
            return;

        var byEntity = NrPatternSites.InPattern(Project, patternId)
            .Where(s => s.AttachEntityId != 0)
            .GroupBy(s => s.AttachEntityId)
            .ToDictionary(g => g.Key, g => g.First());

        if (byEntity.Count == 0)
            return;

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var map = entry.Value?.MapContainer;

            if (map is not { LoadState: MapContentLoadState.Loaded })
                continue;

            foreach (var obj in map.Objects)
            {
                if (obj is not MsbEntity { Type: MsbEntityType.Region })
                    continue;

                if (!byEntity.TryGetValue(MsbEntityIds.GetUInt(obj.WrappedObject), out var site))
                    continue;

                var name = string.IsNullOrWhiteSpace(site.BaseName) ? "" : $" {site.BaseName}";
                PatternLabels.Add((obj, LOC.Get("MAP_Pattern_Label", site.BaseMap, name, site.Variation), site));
            }
        }
    }

    /// <summary>Shows every site of the pattern whose region is loaded, each base at its region</summary>
    private void PlaceLoadedSites()
    {
        var bases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (region, _, site) in PatternLabels)
        {
            if (site.BaseMap == null || region.Container is not MapContainer map)
                continue;

            // One map can be shown once: a base the pattern places twice stays at its first site
            if (!bases.Add(site.BaseMap))
            {
                Smithbox.Log(this, LOC.Get("MAP_Pattern_Base_Twice", site.BaseMap));
                continue;
            }

            EditorCommandQueue.AddCommand($"map/placebase/{site.BaseMap}/{map.Name}/{region.Name}");
            EditorCommandQueue.AddCommand($"map/sessionpreview/{site.BaseMap}/{site.Variation}");
        }
    }

    /// <summary>The pattern's site labels, drawn at their regions in the viewport</summary>
    public void DrawLabels(VulkanViewport viewport)
    {
        if (PatternId == null || !CFG.Current.MapEditor.MapEditor_SessionPreview_Pattern_Labels || PatternLabels.Count == 0)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(viewport.X, viewport.Y);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var color = ImGui.GetColorU32(UI.Current.ImGui_Benefit_Text_Color);
        var lift = 4f * DPI.UIScale();

        foreach (var (region, label, _) in PatternLabels)
        {
            if (region.IsDisposed || region.RenderSceneMesh == null)
                continue;

            var screen = viewport.WorldToScreen(region.RenderSceneMesh.World.Translation, out var inFront);

            if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X > viewport.Width || screen.Y > viewport.Height)
                continue;

            var size = ImGui.CalcTextSize(label);
            var at = origin + screen - new Vector2(size.X / 2, size.Y + lift);

            drawList.AddText(at + Vector2.One, shadow, label);
            drawList.AddText(at, color, label);
        }
    }

    #endregion

    #region Variant actions (M6)

    private void DisplayVariantActions()
    {
        var selected = SelectedPlacementParts();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Variant_Header"), LOC.Get("MAP_Variant_Header_TT"));

        if (selected.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Variant_No_Selection"));
            return;
        }

        // Variation
        GUI.SetInputWidth();
        ImGui.InputInt($"{LOC.Get("MAP_Variant_Value")}##variantValue", ref VariantValue);
        VariantValue = Math.Max(-1, VariantValue);
        GUI.Tooltip(LOC.Get("MAP_Variant_Value_TT"));

        var plus10Applies = selected.Any(e => EntityHelper.IsPartAsset(e) && NrPlacementFilter.AllowsAlsoPlus10((e as MsbEntity)?.MapID));

        if (plus10Applies)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Variant_Also_Plus10")}##variantPlus10", ref VariantAlsoPlus10);
            GUI.Tooltip(LOC.Get("MAP_Variant_Also_Plus10_TT"));
        }

        GUI.MultiButtonInput("variantActions",
            "setVariation", LOC.Get("MAP_Variant_Set"), LOC.Get("MAP_Variant_Set_TT", selected.Count), () => SetVariation(selected),
            "duplicateAsVariation", LOC.Get("MAP_Variant_Duplicate"), LOC.Get("MAP_Variant_Duplicate_TT", selected.Count), () => DuplicateAsVariation(selected));

        // Random Appear pair
        if (!HasRandomAppearParam)
            return;

        GUI.SetInputWidth();
        ImGui.InputInt($"{LOC.Get("MAP_Variant_Random_Row")}##variantRandomRow", ref RandomAppearRow);
        GUI.Tooltip(LOC.Get("MAP_Variant_Random_Row_TT"));

        if (selected.Count == 2)
        {
            GUI.MultiButtonInput("variantRandomActions",
                "splitPair", LOC.Get("MAP_Variant_Split_Pair"), LOC.Get("MAP_Variant_Split_Pair_TT"), () => SplitRandomPair(selected));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Variant_Split_Needs_Two"));
        }
    }

    /// <summary>The selected assets and enemies with placement fields</summary>
    private List<Entity> SelectedPlacementParts() =>
        View.ViewportSelection.GetFilteredSelection<Entity>()
            .Where(e => NrPlacementFilter.TryGetFields(e, out _))
            .ToList();

    /// <summary>A change of one placement field of an object, or null if it has no such field</summary>
    private static PropChangeAction FieldChange(Entity ent, string path, long value)
    {
        if (!MsbEntryColumns.TryGetField(ent.WrappedObject, path, out var field) || field.Property == null)
            return null;

        var action = new PropChangeAction(ent, field.Owner);
        action.AddPropertyChange(field.Property, Convert.ChangeType(value, field.ValueType));
        return action;
    }

    /// <summary>The Variation (and, for assets in m16_00/m19_00, Also Placed at +10) changes for a set of objects</summary>
    private ViewportAction VariationChanges(IEnumerable<Entity> entities)
    {
        var actions = new List<ViewportAction>();

        foreach (var ent in entities)
        {
            if (FieldChange(ent, NrPlacementFilter.VariationField, VariantValue) is { } variation)
                actions.Add(variation);

            if (EntityHelper.IsPartAsset(ent) && NrPlacementFilter.AllowsAlsoPlus10((ent as MsbEntity)?.MapID) &&
                FieldChange(ent, NrPlacementFilter.AlsoPlus10Field, VariantAlsoPlus10 ? 1 : 0) is { } plus10)
            {
                actions.Add(plus10);
            }
        }

        return actions.Count == 0 ? null : new ViewportCompoundAction(actions);
    }

    private void SetVariation(List<Entity> selected)
    {
        if (VariationChanges(selected) is { } action)
        {
            View.ViewportActionManager.ExecuteAction(action);
            Dirty = true;
        }
    }

    /// <summary>
    /// Duplicates the selection with the Duplicate tool's settings, the copies taking the Variation, as one undo step
    /// </summary>
    private void DuplicateAsVariation(List<Entity> selected)
    {
        var objects = selected.OfType<MsbEntity>().ToList();

        if (objects.Count == 0)
            return;

        var duplicate = new EntDuplicateAction(
            view: View,
            objects: objects,
            incrementEntityID: CFG.Current.MapEditor.Toolbar_Duplicate_Increment_Entity_ID,
            incrementInstanceID: CFG.Current.MapEditor.Toolbar_Duplicate_Increment_InstanceID,
            incrementPartNames: CFG.Current.MapEditor.Toolbar_Duplicate_Increment_PartNames,
            clearEntityID: CFG.Current.MapEditor.Toolbar_Duplicate_Clear_Entity_ID,
            clearEntityGroupID: CFG.Current.MapEditor.Toolbar_Duplicate_Clear_Entity_Group_IDs,
            placeAtListEnd: CFG.Current.MapEditor.Toolbar_Duplicate_Place_at_List_End);

        var action = new DuplicateAsVariationAction(duplicate, clones => VariationChanges(clones),
            LOC.Get("MAP_Variant_Duplicate_History", objects.Count, VariantValue));

        View.ViewportActionManager.ExecuteAction(action);
        View.DelayPicking();
        Dirty = true;
    }

    /// <summary>
    /// Puts two objects on a RandomAppearParam row and its other half, so exactly one of them appears each session
    /// </summary>
    private void SplitRandomPair(List<Entity> selected)
    {
        var table = RandomAppearTable.Build(Project);
        var complement = table?.FindComplement(RandomAppearRow);

        if (table == null || table.SlotCountOf(RandomAppearRow) < 0)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Variant_Random_Missing", RandomAppearRow));
            return;
        }

        if (complement == null)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Variant_Random_No_Complement", RandomAppearRow));
            return;
        }

        var actions = new List<ViewportAction>();

        if (FieldChange(selected[0], NrPlacementFilter.RandomAppearField, RandomAppearRow) is { } first)
            actions.Add(first);

        if (FieldChange(selected[1], NrPlacementFilter.RandomAppearField, complement.Value) is { } second)
            actions.Add(second);

        if (actions.Count == 2)
        {
            View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
            Dirty = true;
        }
    }

    #endregion
}
