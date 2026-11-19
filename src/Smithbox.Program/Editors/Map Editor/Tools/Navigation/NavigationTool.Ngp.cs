using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Navigation tool's NGP mode, for DS2 and DS2S (Navigation Editor DS2 Plan, Phases G0 and G1). These games keep a
/// map's navigation in one .ngp file: a mesh per MSB navmesh part, the gates where meshes meet and the gate graph. The
/// mode draws it over the map (faces by their flags, edges, gates, links), picks faces and gates, paints the face flags
/// (level, type, attributes; NGP Format.md section 6), lists the gates, and checks it all. Every edit is one undoable
/// action (NavigationNgpEditAction); the map's save writes the file (NgpBank).
/// </summary>
public partial class NavigationTool
{
    private NgpBank Ngp => View.NgpBank;

    /// <summary>The project's game keeps NGP navigation (DS2, DS2S).</summary>
    private bool IsNgpMode => View.NgpBank?.CanUse() == true;

    private enum NgpTab { Overlay, Models, Regenerate, Flags, Gates, Edit, Check }

    /// <summary>What a click in the viewport does in the NGP mode.</summary>
    private enum NgpClick { None, Pick, Paint, Gate }

    /// <summary>What a paint stroke does to a face's flags.</summary>
    private enum NgpPaint { Level, Type, AddAttributes, RemoveAttributes, Flags }

    private NgpTab? NgpOpenTab;
    private string NgpOpenTabKey = "";
    private NgpClick NgpClickMode;

    // Overlay
    private bool NgpShowOverlay;
    private bool NgpOverlayDirty;
    private float NgpOpacity = 45;
    private bool NgpOnlyPickedMesh;
    private bool NgpShowLegend = true;
    private NgpColorBy NgpColorMode = NgpColorBy.Type;
    private MapContainer NgpContainer;
    private NgpMap NgpData;

    private readonly HashSet<NavOverlayLayer> NgpLayers = new()
    {
        NavOverlayLayer.Faces,
        NavOverlayLayer.OpenEdges,
        NavOverlayLayer.ConnectedInMap,
        NavOverlayLayer.GateNodes,
    };

    private static readonly NavOverlayLayer[] NgpToggleLayers =
    {
        NavOverlayLayer.Faces, NavOverlayLayer.InnerEdges, NavOverlayLayer.OpenEdges, NavOverlayLayer.ConnectedInMap,
        NavOverlayLayer.GateNodes, NavOverlayLayer.GateCosts,
    };

    // Selection, by index so it outlives the NgpMap being built again after an edit
    private int NgpPickedMesh = -1;
    private int NgpPickedFace = -1;
    private Vector3 NgpPickedPoint;
    private int NgpSelectedGate = -1;
    private bool NgpGatesOfPickedMesh;

    // Painting
    private NgpPaint NgpPaintMode = NgpPaint.Type;
    private uint NgpPaintLevel = NgpFlags.DefaultLevel;
    private uint NgpPaintType;
    private uint NgpPaintAttributes = 0x100;
    private uint NgpPaintFlags = NgpFlags.DefaultLevel;
    private readonly Dictionary<(int Mesh, int Face), uint> NgpStroke = new();

    private string NgpMessage;
    private bool NgpMessageIsError;

    // Check
    private sealed class NgpCheckResult
    {
        public string MapId;
        public int Run;
        public List<NavIssue> Issues;
        public bool BaselineDone;
        public string BaselineError;
        public bool Stale;
    }

    private NgpCheckResult NgpCurrent;
    private int NgpRunCounter;
    private CancellationTokenSource NgpCancel;
    private readonly ConcurrentQueue<(int Run, List<NavIssue> Vanilla, string Error)> NgpBaselineResults = new();

    #region Tool window

    private void DisplayNgp(MapContainer map)
    {
        while (NgpBaselineResults.TryDequeue(out var baseline))
        {
            if (NgpCurrent == null || baseline.Run != NgpCurrent.Run)
                continue;

            if (baseline.Error != null)
                NgpCurrent.BaselineError = baseline.Error;
            else
                NgpCurrent.Issues = NavigationValidator.MarkInherited(NgpCurrent.Issues, baseline.Vanilla);

            NgpCurrent.BaselineDone = true;
        }

        if (!Ngp.HasFile(map))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_No_File", map.Name));
            return;
        }

        DisplayNgpHeader(map);

        GUI.Spacer();

        // The overlay follows the map chosen above; edits and undo redraw it from the viewport too
        if (NgpShowOverlay && (NgpContainer != map || NgpOverlayDirty) && NgpStroke.Count == 0)
            RebuildNgpOverlay(map);

        if (ImGui.BeginTabBar("navigationNgpTabs"))
        {
            if (BeginNgpTab(NgpTab.Overlay, "MAP_Navigation_Tab_Overlay", "navigationNgpTabOverlay"))
            {
                DisplayNgpOverlayTab();
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Models, "MAP_Navigation_Tab_Models", "navigationNgpTabModels"))
            {
                DisplayNgpModelsTab(map);
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Regenerate, "MAP_Navigation_Tab_Regenerate", "navigationNgpTabRegenerate"))
            {
                DisplayNgpRegenerateTab(map);
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Flags, "MAP_Navigation_Ngp_Tab_Flags", "navigationNgpTabFlags"))
            {
                DisplayNgpFlagsTab();
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Gates, "MAP_Navigation_Tab_Gates", "navigationNgpTabGates"))
            {
                DisplayNgpGatesTab();
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Edit, "MAP_Navigation_Tab_Edit", "navigationNgpTabEdit"))
            {
                DisplayNgpEditTab();
                ImGui.EndTabItem();
            }

            if (BeginNgpTab(NgpTab.Check, "MAP_Navigation_Tab_Check", "navigationNgpTabCheck"))
            {
                DisplayNgpCheckTab(map);
                ImGui.EndTabItem();
            }

            // The open tab's help, behind a ? at the end of the tab bar
            if (NgpOpenTab is NgpTab open)
                GUI.TabHelpButton("navigationNgpTabHelp", LOC.Get(NgpOpenTabKey), new[] { LOC.Get($"MAP_Navigation_Ngp_Tab_{open}_Help") });

            ImGui.EndTabBar();
        }
    }

    private bool BeginNgpTab(NgpTab tab, string key, string id)
    {
        if (!ImGui.BeginTabItem($"{LOC.Get(key)}##{id}"))
            return false;

        if (NgpOpenTab != tab)
        {
            NgpOpenTab = tab;

            // The modes of other tabs stop, so a mode never takes the viewport's clicks out of sight; picking stays
            if (NgpClickMode is NgpClick.Paint && tab != NgpTab.Flags)
                SetNgpClick(NgpClick.None);
            if (NgpClickMode is NgpClick.Gate && tab != NgpTab.Gates)
                SetNgpClick(NgpClick.None);
            if (tab != NgpTab.Edit)
                StopNgpVertexEdit();
        }

        NgpOpenTabKey = key;
        return true;
    }

    private void SetNgpClick(NgpClick mode)
    {
        NgpClickMode = mode;
        NgpStroke.Clear();

        if (mode != NgpClick.None)
            StopNgpVertexEdit();
    }

    /// <summary>Above the tabs: the overlay toggle, what a click does, the selection, the last check.</summary>
    private void DisplayNgpHeader(MapContainer map)
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Viewport_Header"), LOC.Get("MAP_Navigation_Viewport_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Overlay_Show")}##navigationNgpOverlayShow", ref NgpShowOverlay))
        {
            if (NgpShowOverlay)
                NgpOverlayDirty = true;
            else
                ClearNgpOverlay();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Overlay_Show_TT"));

        // The mode that takes the clicks
        string mode = NgpVertexActive ? "MAP_Navigation_Mode_Vertex_Edit" : NgpClickMode switch
        {
            NgpClick.Pick => "MAP_Navigation_Mode_Pick",
            NgpClick.Paint => "MAP_Navigation_Mode_Paint",
            NgpClick.Gate => "MAP_Navigation_Ngp_Mode_Gate",
            _ => null,
        };

        if (mode == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Mode_None"));
        }
        else
        {
            if (ImGui.Button($"{Icons.Stop}##navigationNgpModeStop"))
            {
                SetNgpClick(NgpClick.None);
                StopNgpVertexEdit();
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Mode_Stop_TT"));
            ImGui.SameLine();

            if (!NgpShowOverlay)
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Mode_No_Overlay", LOC.Get(mode)));
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get(mode));
        }

        // The picked face and the chosen gate
        var data = NgpData != null && NgpContainer == map ? NgpData : null;

        if (data != null && PickedNgpMesh(data) is NgpMesh picked)
        {
            if (ImGui.Button($"{Icons.Times}##navigationNgpPickedClear"))
                SetNgpPicked(-1, -1, default);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Picked_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Picked_Status", NgpPickedFace, picked.Index, picked.Label));
        }

        if (data != null && NgpSelectedGate >= 0 && NgpSelectedGate < data.Ngp.Gates.Count)
        {
            if (ImGui.Button($"{Icons.Times}##navigationNgpGateClear"))
                SelectNgpGate(-1);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Gate_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gate_Status", NgpSelectedGate));
        }

        if (HasNgpVertexEdits)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_Navigation_Pending_Status", NgpVertexCopies.Values.Count(c => c.IsEdited), NgpVertexMap?.Name ?? "-"));

        if (NgpCurrent != null && NgpCurrent.MapId == map.Name)
        {
            int errors = NgpCurrent.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
            int warnings = NgpCurrent.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);
            var text = errors == 0 && warnings == 0 ? LOC.Get("MAP_Navigation_Check_Status_Clean") : LOC.Get("MAP_Navigation_Check_Status", errors, warnings);
            if (NgpCurrent.Stale)
                text += " " + LOC.Get("MAP_Navigation_Check_Status_Stale");

            GUI.WrappedTextColored(NgpCurrent.Stale ? UI.Current.ImGui_Invalid_Text_Color
                : errors > 0 ? UI.Current.ImGui_Warning_Text_Color
                : warnings > 0 ? UI.Current.ImGui_Invalid_Text_Color
                : UI.Current.ImGui_Benefit_Text_Color, text);
        }

        if (NgpMessage != null)
            GUI.WrappedTextColored(NgpMessageIsError ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Benefit_Text_Color, NgpMessage);
    }

    private void SetNgpMessage(string message, bool error = false)
    {
        NgpMessage = message;
        NgpMessageIsError = error;
    }

    #endregion

    #region Names

    private static string NgpTypeLabel(uint flags) => LOC.Get($"MAP_Navigation_Ngp_Type_{NgpFlags.TypeName(flags)}");

    private static string NgpAttributesLabel(uint flags)
    {
        var names = NgpFlags.AttributeNames(flags).Select(n => LOC.Get($"MAP_Navigation_Ngp_Attribute_{n}")).ToList();
        return names.Count == 0 ? LOC.Get("MAP_Navigation_Ngp_Attributes_None") : string.Join(", ", names);
    }

    /// <summary>A level, with the CHR_WEIGHT_TYPE values it admits.</summary>
    private static string NgpLevelLabel(uint level)
    {
        var admitted = NgpFlags.WeightTypesAdmitted(level).ToList();
        if (admitted.Count == 0)
            return LOC.Get("MAP_Navigation_Ngp_Level_None", level);
        if (admitted.Count == 8)
            return LOC.Get("MAP_Navigation_Ngp_Level_All", level);
        return LOC.Get("MAP_Navigation_Ngp_Level_Up_To", level, LOC.Get($"MAP_Navigation_Ngp_Weight_{admitted.Max()}"));
    }

    /// <summary>A legend value, as the colour mode keys it.</summary>
    private string NgpLegendLabel(uint value) => NgpColorMode switch
    {
        NgpColorBy.Type => NgpTypeLabel(value),
        NgpColorBy.Level => NgpLevelLabel(value),
        NgpColorBy.Attributes => NgpAttributesLabel(value),
        _ => LOC.Get("MAP_Navigation_Ngp_Flags_Entry", value.ToString("X3"), NgpTypeLabel(value), NgpFlags.Level(value), NgpAttributesLabel(value)),
    };

    #endregion

    #region Overlay tab

    private void DisplayNgpOverlayTab()
    {
        // Overlay
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Overlay_Header"), LOC.Get("MAP_Navigation_Overlay_Header_TT"));

        if (!NgpShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Overlay_Off"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##navigationNgpFaceOpacity", ref NgpOpacity, 5, 100, $"{LOC.Get("MAP_Navigation_Overlay_Opacity")} %.0f"))
            NgpOverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Opacity_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Only_Picked_Mesh")}##navigationNgpOnlyPicked", ref NgpOnlyPickedMesh))
            NgpOverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Only_Picked_Mesh_TT"));

        // Colour faces by
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Color_Header"), LOC.Get("MAP_Navigation_Ngp_Color_Header_TT"));

        foreach (var by in Enum.GetValues<NgpColorBy>())
        {
            if (ImGui.RadioButton($"{LOC.Get($"MAP_Navigation_Ngp_Color_{by}")}##navigationNgpColor_{by}", NgpColorMode == by))
            {
                NgpColorMode = by;
                NgpOverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Color_{by}_TT"));
        }

        // Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Layers_Header"), LOC.Get("MAP_Navigation_Layers_Header_TT"));

        foreach (var layer in NgpToggleLayers)
        {
            bool on = NgpLayers.Contains(layer);

            if (layer != NavOverlayLayer.Faces)
            {
                var c = NavigationOverlayRenderer.LayerColor(layer);
                ImGui.ColorButton($"##navigationNgpLayerColor_{layer}", new Vector4(c, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
                ImGui.SameLine();
            }

            if (ImGui.Checkbox($"{LOC.Get($"MAP_Navigation_Ngp_Layer_{layer}")}##navigationNgpLayer_{layer}", ref on))
            {
                if (on) NgpLayers.Add(layer); else NgpLayers.Remove(layer);
                NgpOverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Layer_{layer}_TT"));
        }

        // Summary
        if (NgpData != null)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Summary_Header"), LOC.Get("MAP_Navigation_Summary_Header_TT"));

            var d = NgpData;
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Summary_Meshes", d.Meshes.Count, d.PartCount, d.Meshes.Sum(m => m.Mesh.Faces.Count), d.Meshes.Sum(m => m.Mesh.Vertices.Count)));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Summary_Gates", d.Ngp.Gates.Count, d.Ngp.GateLinks.Count));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Summary_File", (int)d.Ngp.Version, d.Ngp.MapIndex.ToString("X2")));
        }

        DisplayNgpLegend();
    }

    /// <summary>The values drawn, in their colours, with a button to paint with one.</summary>
    private void DisplayNgpLegend()
    {
        var legend = OverlayRenderer.FaceLegend;

        if (!NgpLayers.Contains(NavOverlayLayer.Faces) || legend.Count == 0 || !NgpShowOverlay)
            return;

        // Legend
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Ngp_Legend_Header"), LOC.Get("MAP_Navigation_Ngp_Legend_Header_TT"), ref NgpShowLegend);

        if (!NgpShowLegend)
            return;

        foreach (var (value, color, faces) in legend)
        {
            ImGui.ColorButton($"##navigationNgpLegend_{value:X4}", new Vector4(color, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Legend_Use")}##navigationNgpLegendUse_{value:X4}"))
                UseNgpLegendValue(value);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Legend_Use_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Legend_Entry", NgpLegendLabel(value), faces));
        }
    }

    /// <summary>Paints with a legend value: the part the colour mode shows.</summary>
    private void UseNgpLegendValue(uint value)
    {
        switch (NgpColorMode)
        {
            case NgpColorBy.Type: NgpPaintMode = NgpPaint.Type; NgpPaintType = value; break;
            case NgpColorBy.Level: NgpPaintMode = NgpPaint.Level; NgpPaintLevel = value; break;
            case NgpColorBy.Attributes:
                NgpPaintMode = value == 0 ? NgpPaint.RemoveAttributes : NgpPaint.AddAttributes;
                NgpPaintAttributes = value == 0 ? NgpFlags.AttributeMask : value;
                break;
            default: NgpPaintMode = NgpPaint.Flags; NgpPaintFlags = value; break;
        }
    }

    #endregion

    #region Flags tab

    private void DisplayNgpFlagsTab()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Flags_Actions_Header"), LOC.Get("MAP_Navigation_Ngp_Flags_Actions_Header_TT"));

        GUI.MultiButtonInput("navigationNgpFlagModes",
            "navigationNgpPick", NgpClickMode == NgpClick.Pick ? LOC.Get("MAP_Navigation_Ngp_Pick_Stop") : LOC.Get("MAP_Navigation_Ngp_Pick_Start"),
            LOC.Get("MAP_Navigation_Ngp_Pick_TT"), () => SetNgpClick(NgpClickMode == NgpClick.Pick ? NgpClick.None : NgpClick.Pick),

            "navigationNgpPaint", NgpClickMode == NgpClick.Paint ? LOC.Get("MAP_Navigation_Ngp_Paint_Stop") : LOC.Get("MAP_Navigation_Ngp_Paint_Start"),
            LOC.Get("MAP_Navigation_Ngp_Paint_TT"), () => SetNgpClick(NgpClickMode == NgpClick.Paint ? NgpClick.None : NgpClick.Paint));

        // Paint
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Paint_Header"), LOC.Get("MAP_Navigation_Ngp_Paint_Header_TT"));

        foreach (var p in Enum.GetValues<NgpPaint>())
        {
            if (ImGui.RadioButton($"{LOC.Get($"MAP_Navigation_Ngp_Paint_{p}")}##navigationNgpPaintMode_{p}", NgpPaintMode == p))
                NgpPaintMode = p;
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Paint_{p}_TT"));
        }

        GUI.Spacer();

        switch (NgpPaintMode)
        {
            case NgpPaint.Level:
                NgpPaintLevel = NgpLevelCombo("navigationNgpPaintLevel", NgpPaintLevel);
                break;

            case NgpPaint.Type:
                NgpPaintType = NgpTypeCombo("navigationNgpPaintType", NgpPaintType);
                GUI.WrappedText(LOC.Get($"MAP_Navigation_Ngp_Type_{NgpFlags.TypeName(NgpPaintType)}_TT"));
                break;

            case NgpPaint.AddAttributes:
            case NgpPaint.RemoveAttributes:
                foreach (var (bit, name) in NgpFlags.AttributeBits)
                {
                    bool on = (NgpPaintAttributes & bit) != 0;
                    if (ImGui.Checkbox($"{LOC.Get($"MAP_Navigation_Ngp_Attribute_{name}")}##navigationNgpPaintAttr_{name}", ref on))
                        NgpPaintAttributes = on ? NgpPaintAttributes | bit : NgpPaintAttributes & ~bit;
                    GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Attribute_{name}_TT"));
                }
                break;

            case NgpPaint.Flags:
                GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Paint_Flags_Value", NgpLegendLabelFlags(NgpPaintFlags)));
                break;
        }

        DisplayNgpHelpers();
        DisplayNgpPickedFace();
        DisplayNgpLegend();
    }

    // Helpers (Phase G5)
    private float NgpSelectionMargin = 0.5f;
    private float NgpSteepSlope = 50f;
    private bool NgpSteepOnlyPicked;

    /// <summary>
    /// The Flags tab's helpers: paint the faces under the selected objects' bounds (a door's threshold strip, an event
    /// area) with the paint settings above, and mark steep ground faces as one-way drops.
    /// </summary>
    private void DisplayNgpHelpers()
    {
        // Paint Under Selection
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Under_Header"), LOC.Get("MAP_Navigation_Ngp_Under_Header_TT"));

        var boxes = SelectedNgpBoxes();
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Under_Selected", boxes.Count));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationNgpUnderMargin", ref NgpSelectionMargin, 0.1f, 0.5f, "%.2f m"))
            NgpSelectionMargin = Math.Clamp(NgpSelectionMargin, 0f, 10f);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Under_Margin_TT"));

        GUI.ConditionalMultiButtonInput("navigationNgpUnderActions",
            "navigationNgpUnderPaint", LOC.Get("MAP_Navigation_Ngp_Under_Paint"), LOC.Get("MAP_Navigation_Ngp_Under_Paint_TT"),
            () =>
            {
                var faces = NgpGeometry.FacesInBoxes(NgpData, boxes, NgpSelectionMargin);
                if (faces.Count == 0)
                {
                    SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Under_None"), true);
                    return;
                }
                ExecuteNgpFlags(faces.ToDictionary(f => f, f => NgpPainted(NgpData.Meshes[f.Mesh].Mesh.FaceFlags[f.Face])));
            }, NgpData != null && boxes.Count > 0);

        // Mark Steep Faces
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Steep_Header"), LOC.Get("MAP_Navigation_Ngp_Steep_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationNgpSteepSlope", ref NgpSteepSlope, 20f, 89f, $"{LOC.Get("MAP_Navigation_Ngp_Steep_Slope")} %.0f°");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Steep_Slope_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Steep_Only_Picked")}##navigationNgpSteepOnlyPicked", ref NgpSteepOnlyPicked);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Steep_Only_Picked_TT"));

        var picked = NgpData != null ? PickedNgpMesh(NgpData) : null;

        GUI.ConditionalMultiButtonInput("navigationNgpSteepActions",
            "navigationNgpSteepMark", LOC.Get("MAP_Navigation_Ngp_Steep_Mark"), LOC.Get("MAP_Navigation_Ngp_Steep_Mark_TT"),
            () =>
            {
                Func<NgpMesh, bool> include = NgpSteepOnlyPicked && picked != null ? m => m.Index == picked.Index : null;
                var faces = NgpGeometry.SteepFaces(NgpData, NgpSteepSlope, 0x00, include);
                if (faces.Count == 0)
                {
                    SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Steep_None", NgpSteepSlope.ToString("0")));
                    return;
                }
                ExecuteNgpFlags(faces.ToDictionary(f => f, f => NgpFlags.WithType(NgpData.Meshes[f.Mesh].Mesh.FaceFlags[f.Face], 0x40)));
            }, NgpData != null && (!NgpSteepOnlyPicked || picked != null));
    }

    /// <summary>The selected entities' bounds (model space, with their world matrix); entities drawn without a mesh are skipped.</summary>
    private List<(Matrix4x4 World, Vector3 Min, Vector3 Max)> SelectedNgpBoxes()
    {
        var boxes = new List<(Matrix4x4, Vector3, Vector3)>();

        foreach (var entity in View.ViewportSelection.GetFilteredSelection<MsbEntity>())
        {
            if (entity.WrappedObject is MSB2.Part.Navmesh)
                continue;

            var bounds = entity.RenderSceneMesh?.GetLocalBounds();
            if (bounds is BoundingBox box && box.Max.X >= box.Min.X && box.Max.Y >= box.Min.Y && box.Max.Z >= box.Min.Z)
                boxes.Add((entity.GetWorldMatrix(), box.Min, box.Max));
        }

        return boxes;
    }

    private static string NgpLegendLabelFlags(uint flags)
        => LOC.Get("MAP_Navigation_Ngp_Flags_Entry", flags.ToString("X3"), NgpTypeLabel(flags), NgpFlags.Level(flags), NgpAttributesLabel(flags));

    private uint NgpLevelCombo(string id, uint level)
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##{id}", NgpLevelLabel(level)))
        {
            for (uint l = 0; l <= 7; l++)
            {
                if (ImGui.Selectable($"{NgpLevelLabel(l)}##{id}_{l}", l == level))
                    level = l;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Level_TT"));
        return level;
    }

    private uint NgpTypeCombo(string id, uint type)
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##{id}", $"0x{type:X2} {NgpTypeLabel(type)}"))
        {
            foreach (var (value, name) in NgpFlags.Types)
            {
                if (ImGui.Selectable($"0x{value:X2} {LOC.Get($"MAP_Navigation_Ngp_Type_{name}")}##{id}_{value}", value == type))
                    type = value;
                GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Type_{name}_TT"));
            }
            ImGui.EndCombo();
        }
        return type;
    }

    /// <summary>The picked face: its mesh and flags, each part editable (one undoable action each).</summary>
    private void DisplayNgpPickedFace()
    {
        // Picked Face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Picked_Header"), LOC.Get("MAP_Navigation_Ngp_Picked_Header_TT"));

        if (NgpData == null || PickedNgpMesh(NgpData) is not NgpMesh picked)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Picked_None"));
            return;
        }

        var mesh = picked.Mesh;
        var face = mesh.Faces[NgpPickedFace];
        uint flags = mesh.FaceFlags[NgpPickedFace];

        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Picked_Mesh", picked.Index, picked.Label, picked.ModelName ?? "-"));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Picked_Vertices", face.V1, face.V2, face.V3));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Picked_Flags", flags.ToString("X3")));

        var key = (picked.Index, NgpPickedFace);

        uint level = NgpLevelCombo("navigationNgpPickedLevel", NgpFlags.Level(flags));
        if (level != NgpFlags.Level(flags))
            ExecuteNgpFlags(new Dictionary<(int, int), uint> { [key] = NgpFlags.WithLevel(flags, level) });

        uint type = NgpTypeCombo("navigationNgpPickedType", NgpFlags.Type(flags));
        if (type != NgpFlags.Type(flags))
            ExecuteNgpFlags(new Dictionary<(int, int), uint> { [key] = NgpFlags.WithType(flags, type) });

        GUI.WrappedText(LOC.Get($"MAP_Navigation_Ngp_Type_{NgpFlags.TypeName(flags)}_TT"));

        foreach (var (bit, name) in NgpFlags.AttributeBits)
        {
            bool on = (flags & bit) != 0;
            if (ImGui.Checkbox($"{LOC.Get($"MAP_Navigation_Ngp_Attribute_{name}")}##navigationNgpPickedAttr_{name}", ref on))
                ExecuteNgpFlags(new Dictionary<(int, int), uint> { [key] = on ? flags | bit : flags & ~bit });
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Ngp_Attribute_{name}_TT"));
        }

        if ((flags & ~NgpFlags.KnownMask) != 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Ngp_Picked_Unknown_Bits", (flags & ~NgpFlags.KnownMask).ToString("X")));
    }

    private uint NgpPainted(uint before) => NgpPaintMode switch
    {
        NgpPaint.Level => NgpFlags.WithLevel(before, NgpPaintLevel),
        NgpPaint.Type => NgpFlags.WithType(before, NgpPaintType),
        NgpPaint.AddAttributes => before | NgpPaintAttributes & NgpFlags.AttributeMask,
        NgpPaint.RemoveAttributes => before & ~(NgpPaintAttributes & NgpFlags.AttributeMask),
        NgpPaint.Flags => NgpPaintFlags,
        _ => before,
    };

    /// <summary>Records painted flags as one action: the map's NGP swapped for a copy with the new flags.</summary>
    private void ExecuteNgpFlags(IReadOnlyDictionary<(int Mesh, int Face), uint> changes)
    {
        var data = NgpData;
        if (data == null || NgpContainer == null || changes.Count == 0)
            return;

        var real = changes.Where(c => c.Key.Mesh >= 0 && c.Key.Mesh < data.Meshes.Count && c.Key.Face >= 0
                && c.Key.Face < data.Meshes[c.Key.Mesh].Mesh.FaceFlags.Count && data.Meshes[c.Key.Mesh].Mesh.FaceFlags[c.Key.Face] != c.Value)
            .ToDictionary(c => c.Key, c => c.Value);

        if (real.Count == 0)
            return;

        var after = NgpEdit.WithFlags(data.Ngp, real);
        View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(NgpContainer, Ngp, data.Ngp, after, OnNgpEdited));
        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Painted", real.Count, real.Keys.Select(k => k.Mesh).Distinct().Count()));
    }

    #endregion

    #region Gates tab

    private void DisplayNgpGatesTab()
    {
        var data = NgpData;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Gates_Header"), LOC.Get("MAP_Navigation_Ngp_Gates_Header_TT"));

        GUI.MultiButtonInput("navigationNgpGateModes",
            "navigationNgpGatePick", NgpClickMode == NgpClick.Gate ? LOC.Get("MAP_Navigation_Ngp_Gate_Pick_Stop") : LOC.Get("MAP_Navigation_Ngp_Gate_Pick_Start"),
            LOC.Get("MAP_Navigation_Ngp_Gate_Pick_TT"), () => SetNgpClick(NgpClickMode == NgpClick.Gate ? NgpClick.None : NgpClick.Gate));

        if (data == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Overlay_Off"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gates_Derived"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Gates_Of_Picked")}##navigationNgpGatesOfPicked", ref NgpGatesOfPickedMesh);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Gates_Of_Picked_TT"));

        var ngp = data.Ngp;
        var picked = PickedNgpMesh(data);
        var rows = Enumerable.Range(0, ngp.Gates.Count).Where(g => !NgpGatesOfPickedMesh || picked == null
            || NGP.Ids.MeshOf(ngp.Gates[g].MeshAId) == picked.Index || NGP.Ids.MeshOf(ngp.Gates[g].MeshBId) == picked.Index).ToList();

        if (rows.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gates_None"));
        else if (ImGui.BeginChild("##navigationNgpGateList", new Vector2(0, 200 * DPI.UIScale()), ImGuiChildFlags.Borders))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    int g = rows[i];
                    var gate = ngp.Gates[g];
                    if (ImGui.Selectable($"{LOC.Get("MAP_Navigation_Ngp_Gate_Row", g, MeshLabel(data, gate.MeshAId), MeshLabel(data, gate.MeshBId), gate.EdgeCount, gate.LinkCount)}##navigationNgpGate_{g}", g == NgpSelectedGate))
                        SelectNgpGate(g == NgpSelectedGate ? -1 : g);
                    GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Gate_Row_TT"));
                }
            }

            clipper.End();
        }
        if (rows.Count > 0)
            ImGui.EndChild();

        if (NgpSelectedGate < 0 || NgpSelectedGate >= ngp.Gates.Count)
            return;

        // Selected Gate
        var selected = ngp.Gates[NgpSelectedGate];
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Gate_Selected_Header"), LOC.Get("MAP_Navigation_Ngp_Gate_Selected_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gate_Meshes", MeshLabel(data, selected.MeshAId), MeshLabel(data, selected.MeshBId), selected.EdgeCount));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gate_Id", selected.Id.ToString("X8")));

        if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Ngp_Gate_Frame")}##navigationNgpGateFrame"))
            FrameNgpGate(data, NgpSelectedGate);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Gate_Frame_TT"));

        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Gate_Links", selected.LinkCount));

        for (int k = 0; k < selected.LinkCount; k++)
        {
            int li = selected.LinkStart + k;
            if (li < 0 || li >= ngp.GateLinks.Count)
                continue;

            var link = ngp.GateLinks[li];
            int target = link.TargetGate;
            string through = MeshLabel(data, link.EdgeRef);

            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Ngp_Gate_Link_Go")}##navigationNgpLinkGo_{k}") && target >= 0 && target < ngp.Gates.Count)
            {
                SelectNgpGate(target);
                FrameNgpGate(data, target);
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Gate_Link_Go_TT"));
            ImGui.SameLine();
            ImGui.Text(LOC.Get("MAP_Navigation_Ngp_Gate_Link_Row", target, through));
        }
    }

    private static string MeshLabel(NgpMap data, uint id)
    {
        int m = NGP.Ids.MeshOf(id);
        return m < data.Meshes.Count ? $"{m} {data.Meshes[m].Label}" : $"{m}";
    }

    private void SelectNgpGate(int gate)
    {
        NgpSelectedGate = gate;
        NgpOverlayDirty = true;
    }

    private void FrameNgpGate(NgpMap data, int gate)
    {
        var lines = NgpOverlayBuilder.GateEdgeLines(data, gate, Vector3.Zero).ToList();
        var p = data.Place(data.Ngp.Gates[gate].Position);
        var min = p;
        var max = p;
        foreach (var (a, b) in lines)
        {
            min = Vector3.Min(min, Vector3.Min(a, b));
            max = Vector3.Max(max, Vector3.Max(a, b));
        }

        Frame(new BoundingBox(min, max), 4f);
    }

    #endregion

    #region Check tab

    private void DisplayNgpCheckTab(MapContainer map)
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Actions_Header"), LOC.Get("MAP_Navigation_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("navigationNgpActions",
            "navigationNgpValidate", LOC.Get("MAP_Navigation_Validate"), LOC.Get("MAP_Navigation_Ngp_Validate_TT"),
            () => ValidateNgp(map), true,

            "navigationNgpClear", LOC.Get("MAP_Navigation_Clear"), LOC.Get("MAP_Navigation_Clear_TT"),
            ClearNgpResult, NgpCurrent != null);

        // Rebuild
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Rebuild_Header"), LOC.Get("MAP_Navigation_Ngp_Rebuild_Header_TT"));

        var picked = NgpData != null && NgpContainer == map ? PickedNgpMesh(NgpData) : null;

        GUI.ConditionalMultiButtonInput("navigationNgpRebuild",
            "navigationNgpRebuildPicked", LOC.Get("MAP_Navigation_Ngp_Rebuild_Picked"), LOC.Get("MAP_Navigation_Ngp_Rebuild_Picked_TT"),
            () => ExecuteNgpRebuild(map, new[] { picked.Index }), picked != null,

            "navigationNgpRebuildAll", LOC.Get("MAP_Navigation_Ngp_Rebuild_All"), LOC.Get("MAP_Navigation_Ngp_Rebuild_All_TT"),
            () => ExecuteNgpRebuild(map, null), true);

        if (NgpCurrent == null || NgpCurrent.MapId != map.Name)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Not_Checked"));
            return;
        }

        if (NgpCurrent.Stale)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Check_Stale"));

        var issues = NgpCurrent.Issues;
        int newErrors = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
        int newWarnings = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);

        // Issues
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Issues_Header"), LOC.Get("MAP_Navigation_Issues_Header_TT"), ref ShowIssues);

        if (newErrors == 0 && newWarnings == 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_Navigation_Issues_None_New"));
        else
            GUI.WrappedTextColored(newErrors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_Navigation_Issues_Counts", newErrors, newWarnings));

        if (!NgpCurrent.BaselineDone)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Running"));
        else if (NgpCurrent.BaselineError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Baseline_Failed", NgpCurrent.BaselineError));
        else
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Inherited", issues.Count(i => i.Inherited)));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Show_Inherited")}##navigationNgpShowInherited", ref ShowInherited);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Show_Inherited_TT"));

        if (!ShowIssues)
            return;

        var shown = issues.Where(i => ShowInherited || !i.Inherited).OrderBy(i => i.Inherited).ThenBy(i => i.Severity).ThenBy(i => i.Room ?? -1).ToList();

        if (shown.Count == 0 || !ImGui.BeginTable("##navigationNgpIssues", 3,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY,
            new Vector2(0, 300 * DPI.UIScale())))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_Severity"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_File"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_Issue"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var issue = shown[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                // A row on a mesh of this map goes to it
                if (issue.Room != null)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable("##navigationNgpIssueGoTo", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowOverlap))
                        GoToNgpIssue(map, issue);
                    ImGui.PopID();
                    GUI.Tooltip(LOC.Get("MAP_Navigation_Issue_Go_To_TT"));
                    ImGui.SameLine();
                }

                var severity = LOC.Get(issue.Severity == NavIssueSeverity.Error ? "MAP_Navigation_Severity_Error" : "MAP_Navigation_Severity_Warning");
                if (issue.Inherited)
                    GUI.WrappedText($"{severity} ({LOC.Get("MAP_Navigation_Severity_Inherited")})");
                else
                    GUI.WrappedTextColored(issue.Severity == NavIssueSeverity.Error ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Invalid_Text_Color, severity);

                ImGui.TableNextColumn();
                ImGui.Text(issue.File);

                ImGui.TableNextColumn();
                GUI.WrappedText(LOC.Get($"MAP_Navigation_Issue_{issue.Code}", issue.Args));
            }
        }

        clipper.End();
        ImGui.EndTable();
    }

    /// <summary>
    /// Rebuilds the derived tables (NgpRebuild) of the given meshes (null for all) and the map-wide gate tables, as one
    /// undoable action. Nothing happens when the rebuild gives the same file.
    /// </summary>
    private void ExecuteNgpRebuild(MapContainer map, IReadOnlyCollection<int> meshes)
    {
        var data = Ngp.GetMap(map);
        if (data == null)
            return;

        var changed = meshes ?? Enumerable.Range(0, data.Ngp.Meshes.Count).ToList();
        var after = NgpRebuild.Rebuild(data.Ngp, changed);

        if (after.Write().AsSpan().SequenceEqual(data.Ngp.Write()))
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Rebuild_Same"));
            return;
        }

        if (NgpContainer != map)
            RebuildNgpOverlay(map);

        View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(map, Ngp, data.Ngp, after, OnNgpEdited));
        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Rebuilt", changed.Count, data.Ngp.Gates.Count, after.Gates.Count));
    }

    private void ValidateNgp(MapContainer map)
    {
        ClearNgpResult();

        var data = Ngp.GetMap(map);
        if (data == null)
            return;

        var run = ++NgpRunCounter;
        NgpCurrent = new NgpCheckResult { MapId = map.Name, Run = run, Issues = NgpValidator.Validate(data) };

        NgpCancel = new CancellationTokenSource();
        var token = NgpCancel.Token;
        var mapId = map.Name;

        Task.Run(() =>
        {
            try
            {
                var vanilla = Ngp.GetVanillaIssues(mapId);
                if (!token.IsCancellationRequested)
                    NgpBaselineResults.Enqueue((run, vanilla, null));
            }
            catch (Exception e)
            {
                NgpBaselineResults.Enqueue((run, null, e.Message));
            }
        }, token);
    }

    private void ClearNgpResult()
    {
        NgpCancel?.Cancel();
        NgpCancel = null;
        NgpCurrent = null;
    }

    /// <summary>Picks the issue's face (or the mesh's first) and frames it, with the overlay on.</summary>
    private void GoToNgpIssue(MapContainer map, NavIssue issue)
    {
        if (!NgpShowOverlay || NgpContainer != map)
        {
            NgpShowOverlay = true;
            RebuildNgpOverlay(map);
        }

        var data = NgpData;
        if (data == null || issue.Room is not int m || m < 0 || m >= data.Meshes.Count || data.Meshes[m].Mesh.Faces.Count == 0)
            return;

        var mesh = data.Meshes[m].Mesh;
        int f = issue.Face is int face && face >= 0 && face < mesh.Faces.Count ? face : 0;

        if (NgpGeometry.InRange(mesh.Faces[f], mesh.Vertices.Count))
        {
            var (a, b, c) = NgpGeometry.Corners(mesh, f);
            var pa = data.Place(a);
            var pb = data.Place(b);
            var pc = data.Place(c);
            SetNgpPicked(m, f, (pa + pb + pc) / 3);

            if (issue.Face != null)
            {
                Frame(new BoundingBox(Vector3.Min(pa, Vector3.Min(pb, pc)), Vector3.Max(pa, Vector3.Max(pb, pc))), 2f);
                return;
            }
        }

        Frame(new BoundingBox(data.Place(mesh.BoundingBoxMin), data.Place(mesh.BoundingBoxMax)), 5f);
    }

    #endregion

    #region Viewport

    private NgpMesh PickedNgpMesh(NgpMap data)
    {
        if (data == null || NgpPickedMesh < 0 || NgpPickedMesh >= data.Meshes.Count)
            return null;

        var mesh = data.Meshes[NgpPickedMesh];
        return NgpPickedFace >= 0 && NgpPickedFace < mesh.Mesh.Faces.Count ? mesh : null;
    }

    private void SetNgpPicked(int mesh, int face, Vector3 point)
    {
        bool meshChanged = mesh != NgpPickedMesh;
        NgpPickedMesh = mesh;
        NgpPickedFace = face;
        NgpPickedPoint = point;

        if (meshChanged && NgpOnlyPickedMesh)
            NgpOverlayDirty = true;

        DrawNgpHighlight();
    }

    private void DrawNgpHighlight()
    {
        HighlightRenderer.Clear();

        var data = NgpData;
        var mesh = data != null ? PickedNgpMesh(data) : null;
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (mesh == null || scene == null || !NgpShowOverlay)
            return;

        var overlay = NgpOverlayBuilder.Highlight(data, new NgpHit(mesh, NgpPickedFace, 0, NgpPickedPoint), NgpEditedVertices(mesh));

        // The faces painted in the stroke under way, outlined with the picked one
        foreach (var ((m, f), _) in NgpStroke)
        {
            if (m < 0 || m >= data.Meshes.Count)
                continue;

            var stroke = NgpOverlayBuilder.Highlight(data, new NgpHit(data.Meshes[m], f, 0, default), NgpEditedVertices(data.Meshes[m]));
            overlay.Batch(NavOverlayLayer.Highlight).Lines.AddRange(stroke.Batch(NavOverlayLayer.Highlight).Lines);
        }

        HighlightRenderer.Show(scene, overlay, NgpOpacity);
    }

    private void ClearNgpOverlay()
    {
        OverlayRenderer.Clear();
        HighlightRenderer.Clear();
        NgpData = null;
        NgpContainer = null;
    }

    private void RebuildNgpOverlay(MapContainer map)
    {
        NgpOverlayDirty = false;
        NgpOverlayRevision = NgpVertexRevision;

        var data = Ngp.GetMap(map);
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (data == null || scene == null)
        {
            ClearNgpOverlay();
            return;
        }

        if (NgpContainer != map)
        {
            NgpPickedMesh = -1;
            NgpPickedFace = -1;
            NgpSelectedGate = -1;

            // TEMPORARY (G1-G3 test): where the overlay is placed, to compare with where the floor is
            var offset = data.World.Translation;
            var first = data.Meshes.FirstOrDefault(m => m.Mesh.Vertices.Count > 0);
            var part = first?.Part is MsbEntity entity && entity.WrappedObject is MSB2.Part.Navmesh p ? p.Position : (Vector3?)null;
            Smithbox.Log(this, $"[NGP test] {map.Name}: map offset {offset}, {data.Meshes.Count} meshes / {data.PartCount} parts, "
                + $"version {(int)data.Ngp.Version}, gates {data.Ngp.Gates.Count}; mesh 0 ({first?.Label}) bbox placed "
                + $"{(first != null ? data.Place(first.Mesh.BoundingBoxMin) : Vector3.Zero)} to {(first != null ? data.Place(first.Mesh.BoundingBoxMax) : Vector3.Zero)}, its part's position {part}");
        }

        NgpContainer = map;
        NgpData = data;

        var picked = PickedNgpMesh(data);
        Func<NgpMesh, bool> include = NgpOnlyPickedMesh && picked != null ? m => m.Index == picked.Index : null;

        var overlay = NgpOverlayBuilder.Build(data, NgpLayers, NgpColorMode, include, NgpSelectedGate, NgpEditedVertices);
        OverlayRenderer.Show(scene, overlay, NgpOpacity);
        DrawNgpHighlight();
    }

    /// <summary>After an NGP edit or its undo: the overlay is drawn again, and the last check may be out of date.</summary>
    private void OnNgpEdited()
    {
        NgpOverlayDirty = true;

        if (NgpCurrent != null)
            NgpCurrent.Stale = true;
    }

    /// <summary>
    /// The NGP mode's part of HandleViewportInput: keeps the overlay up to date and takes the viewport's clicks while a
    /// mode is on. Returns true while the clicks are taken.
    /// </summary>
    private bool HandleNgpViewportInput(Ray ray, bool canInteract)
    {
        if (NgpContainer != null && NgpContainer.LoadState != MapContentLoadState.Loaded)
        {
            NgpStroke.Clear();
            ClearNgpOverlay();
            NgpShowOverlay = false;
        }

        OverlayRenderer.Update();
        HighlightRenderer.Update();

        // Generations finish in the background; their edits are made here, even while the tool window is closed
        DrainNgpGenerateResults();

        bool mouseDown = InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action);

        // A finished paint stroke is recorded as one action
        if (!mouseDown && NgpStroke.Count > 0)
        {
            var stroke = new Dictionary<(int, int), uint>(NgpStroke);
            NgpStroke.Clear();
            ExecuteNgpFlags(stroke);
        }

        // Drawn again after an edit or undo
        if (NgpShowOverlay && NgpContainer == null && LoadedMaps().FirstOrDefault(m => m.Name == SelectedMap) is MapContainer selected)
            RebuildNgpOverlay(selected);
        else if (NgpShowOverlay && NgpContainer != null && NgpStroke.Count == 0
            && (NgpOverlayDirty || (NgpOverlayRevision != NgpVertexRevision && !OverlayRenderer.IsUpdating)))
            RebuildNgpOverlay(NgpContainer);

        if (UpdateNgpVertexEdit(ray, canInteract))
            return true;

        if (NgpData == null || NgpClickMode == NgpClick.None)
            return false;

        bool pressed = canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);
        Func<NgpMesh, bool> include = NgpOnlyPickedMesh && PickedNgpMesh(NgpData) is NgpMesh only ? m => m.Index == only.Index : null;

        switch (NgpClickMode)
        {
            case NgpClick.Pick:
                if (pressed && NgpOverlayBuilder.Pick(NgpData, ray.Origin, ray.Direction, include, NgpEditedVertices) is NgpHit pick)
                    SetNgpPicked(pick.Mesh.Index, pick.Face, pick.Point);
                break;

            case NgpClick.Paint:
                if (canInteract && mouseDown && NgpOverlayBuilder.Pick(NgpData, ray.Origin, ray.Direction, include, NgpEditedVertices) is NgpHit paint)
                {
                    var key = (paint.Mesh.Index, paint.Face);
                    if (!NgpStroke.ContainsKey(key))
                    {
                        NgpStroke[key] = NgpPainted(paint.Mesh.Mesh.FaceFlags[paint.Face]);
                        SetNgpPicked(paint.Mesh.Index, paint.Face, paint.Point);
                    }
                }
                break;

            case NgpClick.Gate:
                if (pressed)
                {
                    int gate = NgpOverlayBuilder.PickGate(NgpData, ray.Origin, ray.Direction, NavigationOverlayBuilder.GateSize);
                    if (gate >= 0)
                        SelectNgpGate(gate);
                }
                break;
        }

        return true;
    }

    #endregion
}
