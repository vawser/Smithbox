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
        ImGui.BeginTabBar("ProjectModelTabs");

        if(ImGui.BeginTabItem("Load Project"))
        {
            DisplayProjectLoadOptions();

            ImGui.EndTabItem();
        }
;
        if (ImGui.BeginTabItem("Create Project"))
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
                    $"Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                    $"Remove project from list of recent projects?",
                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove from list"))
            {
                CFG.RemoveRecentProject(p);
                CFG.Save();
            }

            ImGui.EndPopup();
        }
    }

    public void DisplayProjectLoadOptions()
    {
        var width = ImGui.GetWindowWidth() / 100;

        if (CFG.Current.RecentProjects.Count > 0)
        {
            ImGui.Separator();
            ImguiUtils.WrappedText("Recent Projects");
            ImGui.Separator();

            Smithbox.ProjectHandler.DisplayRecentProjects();

            ImGui.Separator();
        }

        if (ImGui.Button("Load New Project", new Vector2(width * 95, 32)))
        {
            Smithbox.ProjectHandler.OpenProjectDialog();
        }

        if (CFG.Current.LastProjectFile != "")
        {
            if (ImGui.Button("Load Recent Project", new Vector2(width * 95, 32)))
            {
                Smithbox.ProjectHandler.LoadRecentProject();
            }
        }
    }

    public void DisplayNewProjectCreation()
    {
        // Project Name
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectName",
            "Project's display name. Only affects visuals within Smithbox.");
        ImGui.SameLine();

        var pname = newProject.Config != null ? newProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            newProject.Config.ProjectName = pname;
        }

        // Project Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Directory: ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectDirectory",
            "The location mod files will be saved.\nTypically, this should be Mod Engine's Mod folder.");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref newProjectDirectory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
            {
                if (IsLogicalDrive(path))
                {
                    DialogResult message = PlatformUtils.Instance.MessageBox(
                        "Project Directory has been placed in a drive root. This is not allowed. Please select a different location.", "Error",
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
        ImGui.Text("Game Executable:   ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_GameExecutable",
            "The location of the game's .EXE or EBOOT.BIN file.\nThe folder with the executable will be used to obtain unpacked game data.");
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
                    "Select executable for the game you want to mod...",
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
        ImGui.Text($@"Game Type: {newProject.Config.GameType}");

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
        ImGui.Text(@"Import row names:  ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ImportRowNames",
            "Default: OFF\nImports and applies row names from lists stored in Assets folder.\nRow names can be imported at any time in the param editor's Edit menu.");
        ImGui.SameLine();
        ImGui.Checkbox("##loadDefaultNames", ref loadDefaultRowNamesOnCreation);
        ImGui.NewLine();

        ImGui.Separator();

        // Create
        if (ImGui.Button("Create", new Vector2(120, 0) * Smithbox.GetUIScale()))
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
                    "Your game executable path does not exist. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && newProject.Config.GameType == ProjectType.Undefined)
            {
                PlatformUtils.Instance.MessageBox("Your game executable is not a valid supported game.",
                    "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProjectDirectory == null || !Directory.Exists(newProjectDirectory)))
            {
                PlatformUtils.Instance.MessageBox("Your selected project directory is not valid.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && File.Exists($@"{newProjectDirectory}\project.json"))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "Your selected project directory already contains a project.json. Would you like to replace it?",
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
                    "Project Directory is the same as Game Directory, which allows game files to be overwritten directly.\n\n" +
                    "It's highly recommended you use the Mod Engine mod folder as your project folder instead (if possible).\n\n" +
                    "Continue and create project anyway?", "Caution",
                    MessageBoxButtons.OKCancel);
                if (message != DialogResult.OK)
                {
                    validated = false;
                }
            }

            if (validated && IsLogicalDrive(newProjectDirectory))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "Project Directory has been placed in a drive root. This is not allowed. Please select a different location.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProject.Config.ProjectName == null || newProject.Config.ProjectName == ""))
            {
                PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox(
                    "No valid game has been detected. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
