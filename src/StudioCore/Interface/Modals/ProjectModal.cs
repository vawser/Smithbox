using ImGuiNET;
using StudioCore.Core;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Modals;

public class ProjectModal
{
    public Project newProject;

    public string newProjectDirectory = "";
    public bool loadDefaultRowNamesOnCreation = false;

    public ProjectModal()
    {
        newProject = new Project();
        newProject.Config.PinnedParams = new();
        newProject.Config.PinnedRows = new();
        newProject.Config.PinnedFields = new();

        newProjectDirectory = "";
        loadDefaultRowNamesOnCreation = false;
    }

    public bool IsLogicalDrive(string path)
    {
        return Directory.GetLogicalDrives().Contains(path);
    }

    public void Display()
    {
        ImGui.BeginTabBar("项目模块栏 ProjectModelTabs");

        if(ImGui.BeginTabItem("加载 Load Project"))
        {
            DisplayProjectLoadOptions();

            ImGui.EndTabItem();
        }
;
        if (ImGui.BeginTabItem("新建 Create Project"))
        {
            DisplayNewProjectCreation();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void RecentProjectEntry(CFG.RecentProject p, int id)
    {
        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
        {
            if (File.Exists(p.ProjectFile))
            {
                var path = p.ProjectFile;

                Smithbox.ProjectHandler.LoadProjectFromJSON(path);
                Smithbox.ProjectHandler.IsInitialLoad = false;
            }
            else
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"项目文件不存在 Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                    $"从历史里移除吗 Remove project from list of recent projects?",
                    $"Project.json cannot be found 未找到", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("移除 Remove from list"))
            {
                CFG.RemoveRecentProject(p);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }

    public void DisplayProjectLoadOptions()
    {
        var scale = Smithbox.GetUIScale();
        var width = ImGui.GetWindowWidth() / 100;

        if (CFG.Current.RecentProjects.Count > 0)
        {
            ImGui.Separator();
            ImguiUtils.WrappedText("历史 Recent Projects");
            ImGui.Separator();

            Smithbox.ProjectHandler.DisplayRecentProjects();

            ImGui.Separator();
        }

        if (ImGui.Button("加载新项目 Load New Project", new Vector2(width * 95, 32 * scale)))
        {
            Smithbox.ProjectHandler.OpenProjectDialog();
        }

        if (CFG.Current.LastProjectFile != "")
        {
            if (ImGui.Button("加载上次项目 Load Recent Project", new Vector2(width * 95, 32 * scale)))
            {
                Smithbox.ProjectHandler.LoadRecentProject();
            }
        }
    }

    public void DisplayNewProjectCreation()
    {
        // Project Name
        ImGui.AlignTextToFramePadding();
        ImGui.Text("项目名称 Project Name:      ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectName",
            "起啥都行 Project's display name. Only affects visuals within Smithbox.");
        ImGui.SameLine();

        var pname = newProject.Config != null ? newProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            newProject.Config.ProjectName = pname;
        }

        // Project Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("项目路径 Project Directory: ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectDirectory",
            "最好是mod路径 Game其次 The location mod files will be saved.\nTypically, this should be Mod Engine's Mod folder.");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref newProjectDirectory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("选择项目文件夹 Select project directory...", out var path))
            {
                if (IsLogicalDrive(path))
                {
                    DialogResult message = PlatformUtils.Instance.MessageBox(
                        "当前路径不可用 请更换其他路径 Project Directory has been placed in a drive root. This is not allowed. Please select a different location.", "Error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    newProjectDirectory = path;
                }
            }
        }

        // Game Executable
        ImGui.AlignTextToFramePadding();
        ImGui.Text("游戏路径 Game Executable:   ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_GameExecutable",
            "游戏exe文件路径 必须包含UMX解包数据 The location of the game's .EXE or EBOOT.BIN file.\nThe folder with the executable will be used to obtain unpacked game data.");
        ImGui.SameLine();

        var gname = newProject.Config != null ? newProject.Config.GameRoot : "";
        if (ImGui.InputText("##gdir", ref gname, 255))
        {
            if (File.Exists(gname))
            {
                newProject.Config.GameRoot = Path.GetDirectoryName(gname);
            }
            else
            {
                newProject.Config.GameRoot = gname;
            }

            newProject.Config.GameType = Smithbox.ProjectHandler.GetProjectTypeFromExecutable(gname);

            if (newProject.Config.GameType == ProjectType.BB)
            {
                newProject.Config.GameRoot += @"\dvdroot_ps4";
            }
        }

        ImGui.SameLine();

        if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
        {
            if (PlatformUtils.Instance.OpenFileDialog(
                    "选择游戏exe文件 Select executable for the game you want to mod...",
                    new[] { FilterStrings.GameExecutableFilter },
                    out var path))
            {
                newProject.Config.GameRoot = Path.GetDirectoryName(path);
                newProject.Config.GameType = Smithbox.ProjectHandler.GetProjectTypeFromExecutable(path);

                if (newProject.Config.GameType == ProjectType.BB)
                {
                    newProject.Config.GameRoot += @"\dvdroot_ps4";
                }

                newProject.ProjectJsonPath = $@"{newProjectDirectory}\project.json";

            }
        }

        // Project Gametype
        ImGui.Text($@"游戏类型 Game Type: {newProject.Config.GameType}");

        // Project Options - Loose Params
        if (newProject.Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
        {
            ImGui.NewLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(@"Loose Params:      ");
            ImGui.SameLine();
            Utils.ImGuiGenericHelpPopup("?", "##Help_LooseParams",
                "Default: OFF\n" +
                "DS2: Save and Load parameters as individual .param files instead of regulation.\n" +
                "DS3: Save and Load parameters as decrypted .parambnd instead of regulation.");
            ImGui.SameLine();
            var looseparams = newProject.Config.UseLooseParams;
            if (ImGui.Checkbox("##looseparams", ref looseparams))
            {
                newProject.Config.UseLooseParams = looseparams;
            }
        }

        ImGui.NewLine();

        // Project Options - Import Row Names
        ImGui.AlignTextToFramePadding();
        ImGui.Text(@"导入行名称 Import row names:  ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ImportRowNames",
            "Default: OFF\nImports and applies row names from lists stored in Assets folder.\nRow names can be imported at any time in the param editor's Edit menu.");
        ImGui.SameLine();
        ImGui.Checkbox("##loadDefaultNames", ref loadDefaultRowNamesOnCreation);
        ImGui.NewLine();

        ImGui.Separator();

        // Create
        if (ImGui.Button("创建 Create", new Vector2(120, 0) * Smithbox.GetUIScale()))
        {
            bool validProject = CanCreateNewProject();

            if(validProject)
            {
                Smithbox.ProjectHandler.CheckUnpackedState(newProject);

                Smithbox.ProjectHandler.WriteProjectConfig(newProject);

                Smithbox.ProjectHandler.CurrentProject = newProject;

                // Only proceed if load is successful
                if(Smithbox.ProjectHandler.LoadProject(newProject.ProjectJsonPath))
                    Smithbox.ProjectHandler.IsInitialLoad = false;
            }
        }
    }

    public bool CanCreateNewProject()
    {
        var validated = true;

        if (newProject.Config.GameType != ProjectType.Undefined)
        {
            if (newProject.Config.GameRoot == null ||
                !Directory.Exists(newProject.Config.GameRoot))
            {
                PlatformUtils.Instance.MessageBox(
                    "游戏exe路径不存在\nYour game executable path does not exist. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && newProject.Config.GameType == ProjectType.Undefined)
            {
                PlatformUtils.Instance.MessageBox("游戏版本不支持\nYour game executable is not a valid supported game.",
                    "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProjectDirectory == null || !Directory.Exists(newProjectDirectory)))
            {
                PlatformUtils.Instance.MessageBox("选中的项目已失效\nYour selected project directory is not valid.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && File.Exists($@"{newProjectDirectory}\project.json"))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "你要覆盖原本的project.json吗 Your selected project directory already contains a project.json. Would you like to replace it?",
                    "Error",
                    MessageBoxButtons.YesNo);
                if (message == DialogResult.No)
                {
                    validated = false;
                }
            }

            if (validated && newProject.Config.GameRoot == newProjectDirectory)
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "项目目录与游戏目录相同，这可能会直接覆盖游戏文件。\n" +
                    "强烈建议您使用 Mod Engine 的 mod 文件夹作为您的项目文件夹\n" +
                    "仍然继续并创建项目？\n" +

                    "Project Directory is the same as Game Directory, which allows game files to be overwritten directly.\n\n" +
                    "It's highly recommended you use the Mod Engine mod folder as your project folder instead (if possible).\n\n" +
                    "Continue and create project anyway?", "警告 Caution",
                    MessageBoxButtons.OKCancel);

                if (message != DialogResult.OK)
                {
                    validated = false;
                }
            }

            if (validated && IsLogicalDrive(newProjectDirectory))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "当前路径不可用 请更换其他路径 Project Directory has been placed in a drive root. This is not allowed. Please select a different location.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProject.Config.ProjectName == null || newProject.Config.ProjectName == ""))
            {
                PlatformUtils.Instance.MessageBox("项目名不能为空 You must specify a project name.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox(
                    "游戏被删除了 请重新选择一个有效的\nNo valid game has been detected. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
