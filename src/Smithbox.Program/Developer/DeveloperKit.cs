using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Developer;

public class DeveloperKit
{
    public ProjectType TargetProject = ProjectType.Undefined;
    public Dictionary<ProjectType, DataProjectEntry> Projects = new();
    public bool GeneratedProjects = false;

    public DeveloperKit() { }

    public unsafe void Display(float dt, uint mainDockspaceID)
    {
        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.SetNextWindowDockID(mainDockspaceID, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);

        if (ImGui.Begin($"{LOC.Get("DEV_KIT_Window_Name")}###DeveloperKit", GUI.GetInnerWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            ImGui.BeginChild("kitSection", ImGuiChildFlags.Borders);

            DisplayDataSources();
            DisplayKitTools();

            ImGui.EndChild();

            ImGui.End();
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    public void DisplayDataSources()
    {
        GUI.ConditionalHeader(
            LOC.Get("DEV_KIT_Data_Sources_Header"),
            LOC.Get("DEV_KIT_Data_Sources_Header_TT"), ref CFG.Current.DEVKIT_DisplayDataSources);

        if (CFG.Current.DEVKIT_DisplayDataSources)
        {
            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.BeginTable($"dataSourceTable", 3, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("SelectButton", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("InputText", ImGuiTableColumnFlags.WidthStretch);

                DisplayEntry(LOC.Get("DEV_KIT_Col_Data_Folder"), ref CFG.Current.DEVKIT_DataPath_DataFolder);
                DisplayEntry(LOC.Get("DEV_KIT_Col_Output_Folder"), ref CFG.Current.DEVKIT_DataPath_OutputFolder);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DES"), ref CFG.Current.DEVKIT_DataPath_DES);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DS1"), ref CFG.Current.DEVKIT_DataPath_DS1);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DS1R"), ref CFG.Current.DEVKIT_DataPath_DS1R);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DS2"), ref CFG.Current.DEVKIT_DataPath_DS2);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DS2S"), ref CFG.Current.DEVKIT_DataPath_DS2S);
                DisplayEntry(LOC.Get("DEV_KIT_Col_DS3"), ref CFG.Current.DEVKIT_DataPath_DS3);
                DisplayEntry(LOC.Get("DEV_KIT_Col_BB"), ref CFG.Current.DEVKIT_DataPath_BB);
                DisplayEntry(LOC.Get("DEV_KIT_Col_SDT"), ref CFG.Current.DEVKIT_DataPath_SDT);
                DisplayEntry(LOC.Get("DEV_KIT_Col_ER"), ref CFG.Current.DEVKIT_DataPath_ER);
                DisplayEntry(LOC.Get("DEV_KIT_Col_AC6"), ref CFG.Current.DEVKIT_DataPath_AC6);
                DisplayEntry(LOC.Get("DEV_KIT_Col_NR"), ref CFG.Current.DEVKIT_DataPath_NR);

                ImGui.EndTable();
            }

            GUI.MultiButtonInput("kitActions",
                "generateProjects",
                LOC.Get("DEV_KIT_Action_Generate_Projects"),
                LOC.Get("DEV_KIT_Action_Generate_Projects_TT"),
                GenerateProjects);
        }
    }

    public void DisplayEntry(string name, ref string cfgPath)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.Text(name);

        ImGui.TableSetColumnIndex(1);

        if (ImGui.Button($"{LOC.Get("DEV_KIT_Action_Select")}##selectDataPath_{name}", DPI.SelectorButtonSize))
        {
            var success = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("DEV_KIT_Dialog_Select_Folder"), out var path);
            if (success)
            {
                cfgPath = path;
            }
        }

        ImGui.TableSetColumnIndex(2);

        GUI.SetInputWidth();
        ImGui.InputText($"##dataPath_{name}", ref cfgPath, 255);
    }

    public void DisplayKitTools()
    {
        GUI.Spacer();
        GUI.SimpleHeader("Target Project", "");

        GUI.SetInputWidth();

        var previewName = LOC.Get(TargetProject.GetDisplayName());

        if (ImGui.BeginCombo("##projectTypePicker", previewName))
        {
            foreach (var entry in ProjectTypeOrder.Order)
            {
                var type = (ProjectType)entry;

                var displayName = LOC.Get(type.GetDisplayName());

                if (ImGui.Selectable(displayName))
                {
                    TargetProject = type;

                }
            }

            ImGui.EndCombo();
        }

        if (!GeneratedProjects)
        {
            ImGui.Text(LOC.Get("DEV_KIT_No_Projects_Generated_Hint"));
        }
        else
        {
            GUI.Spacer();
            ImGui.BeginTabBar("developerKitTabBar");

            if (ImGui.BeginTabItem($"{LOC.Get("DEV_KIT_Tab_Common")}##scriptTab"))
            {
                ImGui.BeginChild("commonSection", ImGuiChildFlags.Borders);

                DisplayCommonActions();

                ImGui.EndChild();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public void GenerateProjects()
    {
        GenerateProject(ProjectType.DES, CFG.Current.DEVKIT_DataPath_DES);
        GenerateProject(ProjectType.DS1, CFG.Current.DEVKIT_DataPath_DS1);
        GenerateProject(ProjectType.DS1R, CFG.Current.DEVKIT_DataPath_DS1R);
        GenerateProject(ProjectType.DS2, CFG.Current.DEVKIT_DataPath_DS2);
        GenerateProject(ProjectType.DS2S, CFG.Current.DEVKIT_DataPath_DS2S);
        GenerateProject(ProjectType.DS3, CFG.Current.DEVKIT_DataPath_DS3);
        GenerateProject(ProjectType.BB, CFG.Current.DEVKIT_DataPath_BB);
        GenerateProject(ProjectType.SDT, CFG.Current.DEVKIT_DataPath_SDT);
        GenerateProject(ProjectType.ER, CFG.Current.DEVKIT_DataPath_ER);
        GenerateProject(ProjectType.AC6, CFG.Current.DEVKIT_DataPath_AC6);
        GenerateProject(ProjectType.NR, CFG.Current.DEVKIT_DataPath_NR);

        GeneratedProjects = true;
    }

    public void GenerateProject(ProjectType projectType, string dataPath)
    {
        var newProject = new DataProjectEntry();
        newProject.Descriptor = new()
        {
            ProjectType = projectType,
            DataPath = dataPath
        };

        var task = newProject.Init();
        task.Wait();

        Projects.Add(projectType, newProject);

    }

    // Single-click actions
    public void DisplayCommonActions()
    {

    }
}
