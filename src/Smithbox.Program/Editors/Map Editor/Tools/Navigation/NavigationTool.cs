using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Map Editor's Navigation tool (Navigation Editor Plan, Phase 0): a summary of a loaded ER map's navigation data
/// (NVA, navmesh files, ONAV, NVC files) and a check of its invariants. Issues the vanilla game files also have are
/// marked as inherited, so only what the project changed stands out.
/// </summary>
public partial class NavigationTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private string SelectedMap = "";
    private bool ShowInherited;
    private bool ShowIssues = true;

    /// <summary>The last check of a map.</summary>
    private sealed class Result
    {
        public string MapId;
        public int Run;
        public NavigationMap Map;
        public List<NavIssue> Issues;

        /// <summary>Set once the vanilla data has been checked and the issues compared with it.</summary>
        public bool BaselineDone;
        public string BaselineError;
    }

    private Result Current;
    private int RunCounter;
    private CancellationTokenSource Cancel;
    private readonly ConcurrentQueue<(int Run, List<NavIssue> Vanilla, string Error)> BaselineResults = new();

    public NavigationTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private NavigationBank Bank => View.NavigationBank;

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB_NT;

    private List<MapContainer> LoadedMaps()
    {
        return Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .OrderBy(m => m.Name)
            .ToList();
    }

    public void OnToolWindow()
    {
        ImGui.BeginChild("navigationToolSection", ImGuiChildFlags.Borders);

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Unsupported_Game"));
            ImGui.EndChild();
            return;
        }

        // Picked up before anything is drawn, so the baseline keeps arriving while the map changes
        while (BaselineResults.TryDequeue(out var baseline))
        {
            if (Current == null || baseline.Run != Current.Run)
                continue;

            if (baseline.Error != null)
                Current.BaselineError = baseline.Error;
            else
                Current.Issues = NavigationValidator.MarkInherited(Current.Issues, baseline.Vanilla);

            Current.BaselineDone = true;
        }

        GUI.WrappedText(LOC.Get("MAP_Navigation_Description"));

        var maps = LoadedMaps();

        if (maps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_No_Maps"));
            ImGui.EndChild();
            return;
        }

        if (maps.All(m => m.Name != SelectedMap))
        {
            var selected = View.Selection.SelectedMapContainer;
            SelectedMap = selected != null && maps.Contains(selected) ? selected.Name : maps[0].Name;
        }

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Map_Header"), LOC.Get("MAP_Navigation_Map_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##navigationMap", SelectedMap))
        {
            foreach (var m in maps)
            {
                if (ImGui.Selectable($"{m.Name}##navigationMap_{m.Name}", m.Name == SelectedMap))
                    SelectedMap = m.Name;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Map_TT"));

        var map = maps.First(m => m.Name == SelectedMap);

        if (map.NavmeshParent?.WrappedObject is not SoulsFormats.NVA)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_No_Nva"));
            ImGui.EndChild();
            return;
        }

        if (!Bank.HasNavmeshFiles(map))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_No_Havok"));
            ImGui.EndChild();
            return;
        }

        GUI.Spacer();

        // The overlay is kept up to date whichever tab is open
        UpdateOverlay(map);

        if (ImGui.BeginTabBar("navigationTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Overlay")}##navigationTabOverlay"))
            {
                DisplayOverlayTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Edit")}##navigationTabEdit"))
            {
                DisplayEditTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Face_Data")}##navigationTabFaceData"))
            {
                DisplayFaceDataTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Links")}##navigationTabLinks"))
            {
                DisplayLinksTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Gates")}##navigationTabGates"))
            {
                DisplayGatesTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Stitch")}##navigationTabStitch"))
            {
                DisplayStitchTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Models")}##navigationTabModels"))
            {
                DisplayModelsTab(map);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Navigation_Tab_Check")}##navigationTabCheck"))
            {
                DisplayCheckTab(map, maps);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    private void DisplayCheckTab(MapContainer map, List<MapContainer> maps)
    {
        // Actions
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Actions_Header"), LOC.Get("MAP_Navigation_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("navigationActions",
            "navigationValidate", LOC.Get("MAP_Navigation_Validate"), LOC.Get("MAP_Navigation_Validate_TT"),
            () => Validate(map, maps), true,

            "navigationClear", LOC.Get("MAP_Navigation_Clear"), LOC.Get("MAP_Navigation_Clear_TT"),
            ClearResult, Current != null);

        if (Current == null || Current.MapId != map.Name)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Not_Checked"));
            return;
        }

        DisplaySummary(Current.Map);
        DisplayIssues(Current);
    }

    /// <summary>
    /// Checks the map against its loaded neighbours, then compares the issues with the vanilla data in the background.
    /// </summary>
    private void Validate(MapContainer map, List<MapContainer> loaded)
    {
        ClearResult();

        var navigation = Bank.GetMap(map);

        if (navigation == null)
            return;

        // Neighbours are checked as loaded in the project; the rest only on this map's side
        var neighbours = new Dictionary<string, NavigationMap>();
        foreach (var other in loaded.Where(m => m != map && navigation.Nvcs.Any(n => n.Opposite(map.Name) == m.Name)))
        {
            if (Bank.HasNavmeshFiles(other) && Bank.GetMap(other) is NavigationMap n)
                neighbours[other.Name] = n;
        }

        var run = ++RunCounter;

        Current = new Result
        {
            MapId = map.Name,
            Run = run,
            Map = navigation,
            Issues = NavigationValidator.Validate(navigation, neighbours.GetValueOrDefault),
        };

        Cancel = new CancellationTokenSource();
        var token = Cancel.Token;
        var mapId = map.Name;
        var neighbourIds = neighbours.Keys.ToList();

        Task.Run(() =>
        {
            try
            {
                var vanilla = Bank.GetVanillaIssues(mapId, neighbourIds);

                if (!token.IsCancellationRequested)
                    BaselineResults.Enqueue((run, vanilla, null));
            }
            catch (Exception e)
            {
                BaselineResults.Enqueue((run, null, e.Message));
            }
        }, token);
    }

    private void ClearResult()
    {
        Cancel?.Cancel();
        Cancel = null;
        Current = null;
    }

    private void DisplaySummary(NavigationMap navigation)
    {
        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Summary_Header"), LOC.Get("MAP_Navigation_Summary_Header_TT"));

        var models = navigation.Models;
        int placeholders = models.Count(m => m.IsPlaceholder);
        int quartered = models.Count(m => m.Quarters.Count > 0);
        long nFaces = models.Sum(m => (long)(m.NFile?.FaceCount ?? 0) + m.Quarters.Sum(q => q.File?.FaceCount ?? 0));
        long oFaces = models.Sum(m => (long)(m.OFile?.FaceCount ?? 0));

        GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Navmeshes", navigation.Nva.Navmeshes.Count, models.Count, placeholders, quartered));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Files", navigation.Files.Count, nFaces, oFaces));

        int inMap = navigation.Nva.Connectors.Sum(c => c.NavMeshConnections.Count);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Connectors", navigation.Nva.Connectors.Count, inMap));

        int crossMap = navigation.Nvcs.Sum(n => n.Nvc.Connectors.Sum(c => c.NavMeshConnections.Count));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Nvc", navigation.Nvcs.Count, crossMap));

        if (navigation.Onav == null || navigation.Onav.Areas.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Onav_Empty"));
        }
        else
        {
            var onav = navigation.Onav;
            GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Onav", onav.Areas.Count, onav.Areas.Sum(a => a.Layers.Count),
                onav.Areas.Sum(a => a.Links.Count), onav.UnkAreas.Count));
        }

        GUI.WrappedText(LOC.Get("MAP_Navigation_Summary_Uid", navigation.MapUid?.ToString("X8") ?? "-", navigation.NameBase));
    }

    private void DisplayIssues(Result result)
    {
        var issues = result.Issues;
        int newErrors = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
        int newWarnings = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);
        int inherited = issues.Count(i => i.Inherited);

        // Issues: the counts and baseline state stay outside the list toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Issues_Header"), LOC.Get("MAP_Navigation_Issues_Header_TT"), ref ShowIssues);

        if (newErrors == 0 && newWarnings == 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_Navigation_Issues_None_New"));
        else
            GUI.WrappedTextColored(newErrors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_Navigation_Issues_Counts", newErrors, newWarnings));

        if (!result.BaselineDone)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Running"));
        else if (result.BaselineError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Baseline_Failed", result.BaselineError));
        else
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Inherited", inherited));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Show_Inherited")}##navigationShowInherited", ref ShowInherited);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Show_Inherited_TT"));

        if (!ShowIssues)
            return;

        var shown = issues.Where(i => ShowInherited || !i.Inherited)
            .OrderBy(i => i.Inherited).ThenBy(i => i.Severity).ThenBy(i => i.File).ToList();

        if (shown.Count == 0)
            return;

        if (ImGui.BeginTable("##navigationIssues", 3,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY,
            new System.Numerics.Vector2(0, 300 * DPI.UIScale())))
        {
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
    }
}
