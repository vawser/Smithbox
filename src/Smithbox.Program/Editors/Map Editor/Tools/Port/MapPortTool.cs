using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Port;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Map Editor's Port tool: the map port (<see cref="MapPort"/>) with the loaded project as the output,
/// so only the source (DS3) project is picked. A target map loaded in the editor is unloaded before the port writes
/// its files, as saving it afterwards would overwrite the ported MSB.
/// </summary>
public class MapPortTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private List<ProjectDescriptor> SourceProjects = new();
    private bool ProjectsRead;
    private Guid SourceId = Guid.Empty;

    // Map lists, read in the background when the source changes
    private Guid ListedSource = Guid.Empty;
    private List<string> SourceMaps = new();
    private List<string> TargetMaps = new();
    private Task ListTask;

    private string SourceMap = "";
    private string TargetMap = "";
    private Vector3 Offset = Vector3.Zero;
    private Vector3 OffsetEdit = Vector3.Zero;
    private bool ClearTarget;
    private bool Collision = true;
    private bool Objects = true;
    private bool Breakables = true;
    private bool LightShell;

    private MapPort Port;
    private Task PortTask;
    private CancellationTokenSource Cancel;

    // Set when a finished port's files have been added to the project's file lists
    private bool PortRegistered = true;

    public MapPortTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>Only Elden Ring projects: the port writes DS3 maps into ER maps.</summary>
    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER;

    private bool Running => PortTask is { IsCompleted: false };

    private ProjectDescriptor Source => SourceProjects.FirstOrDefault(p => p.ProjectGUID == SourceId);

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAP_Port_Header")}##mapPortToolHeader"))
            return;

        ImGui.BeginChild("MapPortToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (!ProjectsRead)
        {
            ProjectsRead = true;
            ReadProjects();
        }

        FinishPort();

        GUI.WrappedText(LOC.Get("MAP_Port_Description", Project.Descriptor.ProjectName));

        // Source project
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Port_Source_Header"), LOC.Get("MAP_Port_Source_Header_TT"));

        ImGui.BeginDisabled(Running);
        DisplaySourceProjects();
        ImGui.EndDisabled();

        GUI.ConditionalMultiButtonInput("mapPortProjectActions",
            "mapPortRefresh", LOC.Get("PORT_Refresh_Projects"), LOC.Get("PORT_Refresh_Projects_TT"), ReadProjects, !Running);

        var problems = PortChecks.Validate(Source, Project.Descriptor);
        if (problems.Count > 0)
        {
            GUI.Spacer();
            foreach (var p in problems)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, p);

            DisplayProgress();
            return;
        }

        RefreshMapLists();

        // Maps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Port_Maps_Header"), LOC.Get("MAP_Port_Maps_Header_TT"));

        if (ListTask is { IsCompleted: false })
        {
            ImGui.TextDisabled(LOC.Get("PORT_Reading_Maps"));
            DisplayProgress();
            return;
        }

        ImGui.BeginDisabled(Running);

        MapCombo(LOC.Get("PORT_DS3_Map"), SourceMaps, ref SourceMap, "mapPortSourceMap", false);
        MapCombo(LOC.Get("PORT_Target_Map"), TargetMaps, ref TargetMap, "mapPortTargetMap", true);

        if (TargetMap != "" && IsLoaded(TargetMap))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Port_Target_Loaded", TargetMap));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Port_Options_Header"), LOC.Get("MAP_Port_Options_Header_TT"));

        // Applied when the field is left, not on every keystroke (CLAUDE.md: no EnterReturnsTrue)
        ImGui.Text(LOC.Get("PORT_Offset"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##mapPortOffset", ref OffsetEdit);
        if (GUI.InputCommitted()) Offset = OffsetEdit;
        else if (!ImGui.IsItemActive()) OffsetEdit = Offset;
        GUI.Tooltip(LOC.Get("PORT_Offset_TT"));

        ImGui.Checkbox($"{LOC.Get("PORT_Clear_Target")}##mapPortClear", ref ClearTarget);
        GUI.Tooltip(LOC.Get("PORT_Clear_Target_TT"));
        ImGui.Checkbox($"{LOC.Get("PORT_Collision")}##mapPortCollision", ref Collision);
        GUI.Tooltip(LOC.Get("PORT_Collision_TT"));
        ImGui.Checkbox($"{LOC.Get("PORT_Objects")}##mapPortObjects", ref Objects);
        GUI.Tooltip(LOC.Get("PORT_Objects_TT"));
        ImGui.BeginDisabled(!Objects);
        ImGui.Indent();
        ImGui.Checkbox($"{LOC.Get("PORT_Breakables")}##mapPortBreakables", ref Breakables);
        GUI.Tooltip(LOC.Get("PORT_Breakables_TT"));
        ImGui.Unindent();
        ImGui.EndDisabled();
        ImGui.Checkbox($"{LOC.Get("PORT_LightShell")}##mapPortLightShell", ref LightShell);
        GUI.Tooltip(LOC.Get("PORT_LightShell_TT"));

        ImGui.EndDisabled();

        // Objects need asset param rows, which are added in the Param Editor's loaded params
        var paramsReady = !Objects || MapPortParams.TryGetParam(Project, PortObjects.AssetParamName, out _, out _);
        if (!paramsReady)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Port_Params_Not_Ready", PortObjects.AssetParamName));
        else if (Objects)
            GUI.WrappedText(LOC.Get("MAP_Port_Params_Note"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Port_Actions_Header"), "");

        GUI.ConditionalMultiButtonInput("mapPortActions",
            "mapPortRun", LOC.Get("PORT_Run"), LOC.Get("PORT_Run_TT", Project.Descriptor.ProjectPath),
            StartPort, !Running && SourceMap != "" && TargetMap != "" && paramsReady);

        DisplayProgress();
    }

    private void DisplaySourceProjects()
    {
        if (SourceProjects.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PORT_No_Projects_Of_Type", ProjectType.DS3));
            return;
        }

        if (ImGui.BeginTable("mapPortSourceProjects", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Name"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Project_Folder"));
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Game_Folder"));
            ImGui.TableHeadersRow();

            foreach (var p in SourceProjects)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (ImGui.Selectable($"{p.ProjectName}##mapPortSource_{p.ProjectGUID}", SourceId == p.ProjectGUID, ImGuiSelectableFlags.SpanAllColumns))
                    SourceId = p.ProjectGUID;
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(p.ProjectPath);
                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(p.DataPath);
            }

            ImGui.EndTable();
        }
    }

    private void DisplayProgress()
    {
        if (Port == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Port_Progress_Header"), "");

        ImGui.ProgressBar(Port.Progress, new Vector2(-1, 0), Port.Stage);

        if (Running)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Port_Running_Note", TargetMap));

            GUI.ConditionalMultiButtonInput("mapPortCancelActions",
                "mapPortCancel", LOC.Get("PORT_Cancel"), "", () => Cancel?.Cancel(), true);
        }
        else if (Port.ReportPath != null)
        {
            GUI.WrappedText(LOC.Get(Port.Succeeded ? "PORT_Finished" : "PORT_Finished_With_Failures", Port.Log.Failures, Port.Log.Warnings));
            GUI.WrappedText(LOC.Get("PORT_Report_At", Port.ReportPath));
        }

        if (ImGui.BeginChild("##mapPortLog", new Vector2(-1, 300 * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY, ImGuiWindowFlags.HorizontalScrollbar))
        {
            foreach (var line in Port.Log.Snapshot())
            {
                if (line.StartsWith("  FAIL")) ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), line);
                else if (line.StartsWith("  !!")) ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, line);
                else ImGui.TextUnformatted(line);
            }

            if (Running && ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 5)
                ImGui.SetScrollHereY(1.0f);
        }
        ImGui.EndChild();
    }

    private void MapCombo(string label, List<string> maps, ref string selected, string id, bool markLoaded)
    {
        ImGui.Text(label);
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##{id}", selected == "" ? LOC.Get("PORT_Pick_Map") : selected))
        {
            foreach (var m in maps)
            {
                var text = markLoaded && IsLoaded(m) ? LOC.Get("MAP_Port_Map_Loaded_Suffix", m) : m;
                if (ImGui.Selectable($"{text}##{id}_{m}", m == selected))
                    selected = m;
            }
            ImGui.EndCombo();
        }
    }

    /// <summary>Whether the map is loaded (or loading) in any Map Editor view of this project.</summary>
    private bool IsLoaded(string mapId) => View.Selection.GetMapContainerFromMapID(mapId) != null;

    private void ReadProjects()
    {
        var projects = new List<ProjectDescriptor>();
        foreach (var file in ProjectOrchestrator.GetStoredProjectJsonList())
        {
            try
            {
                var descriptor = JsonSerializer.Deserialize(File.ReadAllText(file), ProjectJsonSerializerContext.Default.ProjectDescriptor);
                if (descriptor is { ProjectType: ProjectType.DS3 })
                    projects.Add(descriptor);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PORT_Project_Read_Failed", file), e);
            }
        }

        SourceProjects = projects.OrderBy(p => p.ProjectName, StringComparer.OrdinalIgnoreCase).ToList();

        // A refresh reads the maps again too, for maps added since
        ListedSource = Guid.Empty;
    }

    /// <summary>Reads the maps of the source and the loaded project in the background when the source changes.</summary>
    private void RefreshMapLists()
    {
        if (ListTask is { IsCompleted: false } || ListedSource == SourceId)
            return;

        var source = Source;
        var output = Project.Descriptor;
        ListedSource = SourceId;
        ListTask = Task.Run(() =>
        {
            try
            {
                using var i = PortGame.Open(source);
                using var o = PortGame.Open(output);
                var sourceMaps = PortChecks.SourceMaps(i);
                var targetMaps = PortChecks.TargetMaps(o);
                SourceMaps = sourceMaps;
                TargetMaps = targetMaps;
                if (!sourceMaps.Contains(SourceMap)) SourceMap = "";
                if (!targetMaps.Contains(TargetMap)) TargetMap = "";
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PORT_Read_Maps_Failed"), e);
            }
        });
    }

    private void StartPort()
    {
        // The port rewrites the target map's MSB and binders: a loaded copy would be stale, and saving it would
        // write over the port. Unloading offers to save its unsaved changes first, and can be cancelled there.
        if (IsLoaded(TargetMap))
        {
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_Port_Unload_Prompt", TargetMap),
                LOC.Get("MAP_Port_Unload_Prompt_Title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            View.Universe.RequestUnloadMap(TargetMap);

            if (IsLoaded(TargetMap))
            {
                Smithbox.LogWarning(this, LOC.Get("MAP_Port_Unload_Cancelled", TargetMap));
                return;
            }
        }

        var request = new PortRequest
        {
            Input = Source,
            Output = Project.Descriptor,
            Ds3Map = SourceMap,
            TargetMap = TargetMap,
            Offset = Offset,
            ClearTarget = ClearTarget,
            Collision = Collision,
            Objects = Objects,
            Breakables = Breakables,
            LightShell = LightShell,
            // Read here, on the main thread: the port's thread never touches the Param Editor's params
            AssetParamGroups = Objects ? MapPortParams.AssetParamGroups(Project) : null
        };

        Port = new MapPort();
        Cancel = new CancellationTokenSource();
        PortRegistered = false;

        var token = Cancel.Token;
        var port = Port;
        PortTask = Task.Run(() => port.Run(request, token));

        Smithbox.Log(this, LOC.Get("MAP_Port_Started", SourceMap, request.Input.ProjectName, TargetMap));
    }

    /// <summary>
    /// Once a port has finished, on the main thread: adds the files it wrote to the project's file lists, so the
    /// new map pieces, assets and textures are found without reloading the project.
    /// </summary>
    private void FinishPort()
    {
        if (PortRegistered || Running || Port == null)
            return;

        PortRegistered = true;

        // The rows of the ported assets, as an undoable edit in the Param Editor (the port itself writes no params)
        if (Port.ParamPlan is { Rows.Count: > 0 })
        {
            try
            {
                var applied = MapPortParams.Apply(Project, Port.ParamPlan, Port.Log);
                if (applied > 0)
                    Smithbox.Log(this, LOC.Get("MAP_Port_Params_Applied", applied, Port.ParamPlan.ParamName));
            }
            catch (Exception e)
            {
                Port.Log.Fail($"Params: {e.GetType().Name}: {e.Message}");
                Smithbox.LogError(this, LOC.Get("MAP_Port_Params_Apply_Failed"), e);
            }

            Port.UpdateReport();
        }

        var added = 0;
        try
        {
            added = Project.Locator?.RegisterNewProjectFiles() ?? 0;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Port_Register_Failed"), e);
        }

        if (Port.Succeeded)
            Smithbox.Log(this, LOC.Get("MAP_Port_Finished_Log", TargetMap, added));
        else
            Smithbox.LogWarning(this, LOC.Get("MAP_Port_Finished_Failures_Log", TargetMap, Port.Log.Failures, Port.Log.Warnings));
    }
}
