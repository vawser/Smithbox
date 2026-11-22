using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Port;

/// <summary>
/// Smithbox.Port's window (Smithbox Port Plan.md §2): the editor rail with a Home section, where the DS3 input and ER
/// output projects are picked from the stored projects, and a Map section, which ports a whole DS3 map into an
/// existing ER map (<see cref="MapPort"/>).
/// </summary>
public class PortTool
{
    private const string HomeId = "port_home";
    private const string MapId = "port_map";

    private string ActiveSection = HomeId;

    private List<ProjectDescriptor> Projects = new();
    private bool ProjectsRead;
    private Guid InputId = Guid.Empty;
    private Guid OutputId = Guid.Empty;

    // Map lists of the picked projects, read when the Map section first needs them
    private Guid ListedInput = Guid.Empty;
    private Guid ListedOutput = Guid.Empty;
    private List<string> InputMaps = new();
    private List<string> OutputMaps = new();
    private Task ListTask;

    private string Ds3Map = "";
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

    public PortTool() { }

    public void Display(float dt, uint mainDockspaceID)
    {
        if (!ProjectsRead)
        {
            ProjectsRead = true;
            ReadProjects();
        }

        var items = new List<EditorRail.RailItem>
        {
            new(HomeId, Icons.Home, LOC.Get("PORT_Home")),
            new(MapId, Icons.Map, LOC.Get("PORT_Map"), Separator: true)
        };

        var clicked = EditorRail.DisplayItems(items, ActiveSection);
        if (clicked != null) ActiveSection = clicked;

        if (EditorHost.Begin($"{LOC.Get(ActiveSection == HomeId ? "PORT_Home" : "PORT_Map")}###portSection", ImGuiWindowFlags.None))
        {
            if (ActiveSection == HomeId) DisplayHome();
            else DisplayMap();
        }
        ImGui.End();
    }

    // ---- Home ---------------------------------------------------------------------------------------------------

    private void ReadProjects()
    {
        Projects = new();
        foreach (var file in ProjectOrchestrator.GetStoredProjectJsonList())
        {
            try
            {
                var descriptor = JsonSerializer.Deserialize(File.ReadAllText(file), ProjectJsonSerializerContext.Default.ProjectDescriptor);
                if (descriptor != null) Projects.Add(descriptor);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PORT_Project_Read_Failed", file), e);
            }
        }
        Projects = Projects.OrderBy(p => p.ProjectName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private ProjectDescriptor Input => Projects.FirstOrDefault(p => p.ProjectGUID == InputId);
    private ProjectDescriptor Output => Projects.FirstOrDefault(p => p.ProjectGUID == OutputId);

    private void DisplayHome()
    {
        GUI.WrappedText(LOC.Get("PORT_Home_Description"));
        ImGui.Separator();

        ProjectList(LOC.Get("PORT_Input_Project"), ProjectType.DS3, ref InputId);
        ImGui.Spacing();
        ProjectList(LOC.Get("PORT_Output_Project"), ProjectType.ER, ref OutputId);
        ImGui.Spacing();

        if (ImGui.Button($"{LOC.Get("PORT_Refresh_Projects")}##portRefresh"))
            ReadProjects();
        GUI.Tooltip(LOC.Get("PORT_Refresh_Projects_TT"));

        ImGui.Separator();
        var problems = Validate();
        if (problems.Count == 0)
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1), LOC.Get("PORT_Ready"));
        else
            foreach (var p in problems)
                ImGui.TextColored(new Vector4(0.95f, 0.6f, 0.3f, 1), p);
    }

    private void ProjectList(string title, ProjectType type, ref Guid selected)
    {
        ImGui.Text(title);
        var list = Projects.Where(p => p.ProjectType == type).ToList();
        if (list.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("PORT_No_Projects_Of_Type", type));
            return;
        }

        if (ImGui.BeginTable($"portProjects_{type}", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Name"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Project_Folder"));
            ImGui.TableSetupColumn(LOC.Get("PORT_Col_Game_Folder"));
            ImGui.TableHeadersRow();

            foreach (var p in list)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (ImGui.Selectable($"{p.ProjectName}##portProject_{type}_{p.ProjectGUID}", selected == p.ProjectGUID, ImGuiSelectableFlags.SpanAllColumns))
                    selected = p.ProjectGUID;
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(p.ProjectPath);
                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(p.DataPath);
            }
            ImGui.EndTable();
        }
    }

    /// <summary>What stops a port, as messages (none when it can run).</summary>
    private List<string> Validate()
    {
        var problems = new List<string>();
        var input = Input;
        var output = Output;
        if (input == null) problems.Add(LOC.Get("PORT_Need_Input"));
        if (output == null) problems.Add(LOC.Get("PORT_Need_Output"));
        if (input != null && !Directory.Exists(input.DataPath)) problems.Add(LOC.Get("PORT_Missing_Game_Folder", input.ProjectName, input.DataPath));
        if (output != null && !Directory.Exists(output.DataPath)) problems.Add(LOC.Get("PORT_Missing_Game_Folder", output.ProjectName, output.DataPath));
        if (output != null && !Directory.Exists(output.ProjectPath)) problems.Add(LOC.Get("PORT_Missing_Project_Folder", output.ProjectName, output.ProjectPath));
        if (input != null && output != null && string.Equals(Path.GetFullPath(input.ProjectPath), Path.GetFullPath(output.ProjectPath), StringComparison.OrdinalIgnoreCase))
            problems.Add(LOC.Get("PORT_Same_Folder"));
        if (output != null && Directory.Exists(output.DataPath) && !Directory.EnumerateFiles(output.DataPath, "oo2core_*").Any())
            problems.Add(LOC.Get("PORT_No_Oodle", output.DataPath));
        foreach (var file in new[] { "Materials.json", "HitMaterials.json" })
            if (!File.Exists(Path.Combine(MapPort.AssetFolder, file)))
                problems.Add(LOC.Get("PORT_Missing_Asset", Path.Combine(MapPort.AssetFolder, file)));
        return problems;
    }

    // ---- Map ----------------------------------------------------------------------------------------------------

    private bool Running => PortTask is { IsCompleted: false };

    private void DisplayMap()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            GUI.WrappedText(LOC.Get("PORT_Map_Needs_Home"));
            foreach (var p in problems)
                ImGui.TextColored(new Vector4(0.95f, 0.6f, 0.3f, 1), p);
            return;
        }

        RefreshMapLists();
        GUI.WrappedText(LOC.Get("PORT_Map_Description", Input.ProjectName, Output.ProjectName));
        ImGui.Separator();

        if (ListTask is { IsCompleted: false })
        {
            ImGui.TextDisabled(LOC.Get("PORT_Reading_Maps"));
            return;
        }

        ImGui.BeginDisabled(Running);

        MapCombo(LOC.Get("PORT_DS3_Map"), InputMaps, ref Ds3Map, "portDs3Map");
        MapCombo(LOC.Get("PORT_Target_Map"), OutputMaps, ref TargetMap, "portTargetMap");

        // Applied when the field is left, not on every keystroke (CLAUDE.md: no EnterReturnsTrue)
        ImGui.InputFloat3($"{LOC.Get("PORT_Offset")}##portOffset", ref OffsetEdit);
        if (GUI.InputCommitted()) Offset = OffsetEdit;
        else if (!ImGui.IsItemActive()) OffsetEdit = Offset;
        GUI.Tooltip(LOC.Get("PORT_Offset_TT"));

        ImGui.Checkbox($"{LOC.Get("PORT_Clear_Target")}##portClear", ref ClearTarget);
        GUI.Tooltip(LOC.Get("PORT_Clear_Target_TT"));
        ImGui.Checkbox($"{LOC.Get("PORT_Collision")}##portCollision", ref Collision);
        GUI.Tooltip(LOC.Get("PORT_Collision_TT"));
        ImGui.Checkbox($"{LOC.Get("PORT_Objects")}##portObjects", ref Objects);
        GUI.Tooltip(LOC.Get("PORT_Objects_TT"));
        ImGui.BeginDisabled(!Objects);
        ImGui.Indent();
        ImGui.Checkbox($"{LOC.Get("PORT_Breakables")}##portBreakables", ref Breakables);
        GUI.Tooltip(LOC.Get("PORT_Breakables_TT"));
        ImGui.Unindent();
        ImGui.EndDisabled();
        ImGui.Checkbox($"{LOC.Get("PORT_LightShell")}##portLightShell", ref LightShell);
        GUI.Tooltip(LOC.Get("PORT_LightShell_TT"));

        ImGui.EndDisabled();
        ImGui.Separator();

        var canRun = !Running && Ds3Map != "" && TargetMap != "";
        ImGui.BeginDisabled(!canRun);
        if (ImGui.Button($"{LOC.Get("PORT_Run")}##portRun"))
            StartPort();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("PORT_Run_TT", Output?.ProjectPath ?? ""));

        if (Running)
        {
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("PORT_Cancel")}##portCancel"))
                Cancel?.Cancel();
        }

        if (Port != null)
        {
            ImGui.ProgressBar(Port.Progress, new Vector2(-1, 0), Port.Stage);
            if (!Running && Port.ReportPath != null)
            {
                ImGui.TextUnformatted(LOC.Get(Port.Succeeded ? "PORT_Finished" : "PORT_Finished_With_Failures", Port.Log.Failures, Port.Log.Warnings));
                ImGui.TextUnformatted(LOC.Get("PORT_Report_At", Port.ReportPath));
            }

            if (ImGui.BeginChild("##portLog", new Vector2(-1, -1), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar))
            {
                var lines = Port.Log.Snapshot();
                foreach (var line in lines)
                {
                    if (line.StartsWith("  FAIL")) ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), line);
                    else if (line.StartsWith("  !!")) ImGui.TextColored(new Vector4(0.95f, 0.75f, 0.3f, 1), line);
                    else ImGui.TextUnformatted(line);
                }
                if (Running && ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 5)
                    ImGui.SetScrollHereY(1.0f);
            }
            ImGui.EndChild();
        }
    }

    private static void MapCombo(string label, List<string> maps, ref string selected, string id)
    {
        if (ImGui.BeginCombo($"{label}##{id}", selected == "" ? LOC.Get("PORT_Pick_Map") : selected))
        {
            foreach (var m in maps)
                if (ImGui.Selectable(m, m == selected))
                    selected = m;
            ImGui.EndCombo();
        }
    }

    /// <summary>Reads the maps of the picked projects in the background when the pick changes.</summary>
    private void RefreshMapLists()
    {
        if (ListTask is { IsCompleted: false } || (ListedInput == InputId && ListedOutput == OutputId))
            return;

        var input = Input;
        var output = Output;
        ListedInput = InputId;
        ListedOutput = OutputId;
        ListTask = Task.Run(() =>
        {
            try
            {
                using var i = PortGame.Open(input);
                using var o = PortGame.Open(output);
                InputMaps = i.Maps().Where(m => i.Exists($"/map/{m}/{"h" + m[1..]}.hkxbhd") || i.MapPieces(m).Count > 0).ToList();
                OutputMaps = o.Maps().Where(m => o.MapPieces(m).Count > 0).ToList();
                if (!InputMaps.Contains(Ds3Map)) Ds3Map = "";
                if (!OutputMaps.Contains(TargetMap)) TargetMap = "";
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PORT_Read_Maps_Failed"), e);
            }
        });
    }

    private void StartPort()
    {
        var request = new PortRequest
        {
            Input = Input,
            Output = Output,
            Ds3Map = Ds3Map,
            TargetMap = TargetMap,
            Offset = Offset,
            ClearTarget = ClearTarget,
            Collision = Collision,
            Objects = Objects,
            Breakables = Breakables,
            LightShell = LightShell
        };

        Port = new MapPort();
        Cancel = new CancellationTokenSource();
        var token = Cancel.Token;
        var port = Port;
        PortTask = Task.Run(() => port.Run(request, token));
    }
}
