using ImGuiNET;
using StudioCore.Core;
using StudioCore.Localization;
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

        if(ImGui.BeginTabItem($"{LOC.Get("PROJECT_MODAL__LOAD_PROJECT")}##loadProjectTab"))
        {
            DisplayProjectLoadOptions();

            ImGui.EndTabItem();
        }
;
        if (ImGui.BeginTabItem($"{LOC.Get("PROJECT_MODAL__CREATE_PROJECT")}##createProjectTab"))
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
                    $"{LOC.Get("PROJECT__PROJECT_FILE_DOES_NOT_EXIST")}" +
                    $"{LOC.Get("PROJECT__PROJECT_FILE")}" + $"{p.ProjectFile}",
                    $"{LOC.Get("PROJECT__PROJECT_MISSING")}", 
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CFG.RemoveRecentProject(p);
                }
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable($"{LOC.Get("PROJECT__REMOVE_FROM_LIST")}##removeFromListButton"))
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
            ImguiUtils.WrappedText($"{LOC.Get("PROJECT_MODAL__RECENT_PROJECTS")}");
            ImGui.Separator();

            Smithbox.ProjectHandler.DisplayRecentProjects();

            ImGui.Separator();
        }

        if (ImGui.Button($"{LOC.Get("PROJECT_MODAL__LOAD_NEW_PROJECT")}##loadNewProjectButton", new Vector2(width * 95, 32 * scale)))
        {
            Smithbox.ProjectHandler.OpenProjectDialog();
        }

        if (CFG.Current.LastProjectFile != "")
        {
            if (ImGui.Button($"{LOC.Get("PROJECT_MODAL__LOAD_RECENT_PROJECT")}##loadRecentProjectButton", new Vector2(width * 95, 32 * scale)))
            {
                Smithbox.ProjectHandler.LoadRecentProject();
            }
        }
    }

    public void DisplayNewProjectCreation()
    {
        // Project Name
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{LOC.Get("PROJECT_MODAL__PROJECT_NAME")}");
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("PROJECT_MODAL__PROJECT_NAME_TOOLTIP")}");
        ImGui.SameLine();
        ImGui.SameLine();

        var pname = newProject.Config != null ? newProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            newProject.Config.ProjectName = pname;
        }

        // Project Directory
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{LOC.Get("PROJECT_MODAL__PROJECT_DIRECTORY")}");
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("PROJECT_MODAL__PROJECT_DIRECTORY_TOOLTIP")}");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref newProjectDirectory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog($"{LOC.Get("PROJECT_MODAL__SELECT_PROJECT_DIR")}", out var path))
            {
                if (IsLogicalDrive(path))
                {
                    DialogResult message = PlatformUtils.Instance.MessageBox(
                        $"{LOC.Get("PROJECT_MODAL__PROJECT_PLACED_AT_DRIVE_ROOT")}",
                        $"{LOC.Get("ERROR")}",
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
        ImGui.Text($"{LOC.Get("PROJECT_MODAL__GAME_EXECUTABLE")}");
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("PROJECT_MODAL__GAME_EXECUTABLE_TOOLTIP")}");
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
                    $"{LOC.Get("PROJECT_MODAL__SELECT_GAME_EXECUTABLE")}",
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
            ImGui.Text($"{LOC.Get("PROJECT_MODAL__LOOSE_PARAMS")}");
            ImguiUtils.ShowHoverTooltip($"{LOC.Get("PROJECT_MODAL__LOOSE_PARAMS_TOOLTIP")}");
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
        ImGui.Text($"{LOC.Get("PROJECT_MODAL__IMPORT_ROW_NAMES")}");
        ImGui.SameLine();
        ImguiUtils.ShowHoverTooltip($"{LOC.Get("PROJECT_MODAL__IMPORT_ROW_NAMES_TOOLTIP")}");
        ImGui.SameLine();
        ImGui.Checkbox("##loadDefaultNames", ref loadDefaultRowNamesOnCreation);
        ImGui.NewLine();

        ImGui.Separator();

        // Create
        if (ImGui.Button($"{LOC.Get("PROJECT_MODAL__CREATE_PROJECT")}##createProjectButton", new Vector2(120, 0) * Smithbox.GetUIScale()))
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
                    $"{LOC.Get("PROJECT_MODAL__INVALID_GAME_EXECUTABLE")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && newProject.Config.GameType == ProjectType.Undefined)
            {
                PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__UNSUPPORTED_GAME")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProjectDirectory == null || !Directory.Exists(newProjectDirectory)))
            {
                PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__INVALID_DIRECTORY")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && File.Exists($@"{newProjectDirectory}\project.json"))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__ALREADY_EXISTING_PROJECT")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.YesNo);
                if (message == DialogResult.No)
                {
                    validated = false;
                }
            }

            if (validated && newProject.Config.GameRoot == newProjectDirectory)
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__PROJECT_MATCHES_GAME_ROOT")}",
                    $"{LOC.Get("WARNING")}",
                    MessageBoxButtons.OKCancel);
                if (message != DialogResult.OK)
                {
                    validated = false;
                }
            }

            if (validated && IsLogicalDrive(newProjectDirectory))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__PROJECT_PLACED_AT_DRIVE_ROOT")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (newProject.Config.ProjectName == null || newProject.Config.ProjectName == ""))
            {
                PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__NO_PROJECT_NAME")}",
                    $"{ LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
                validated = false;
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox(
                    $"{LOC.Get("PROJECT_MODAL__INVALID_GAME")}",
                    $"{LOC.Get("ERROR")}",
                    MessageBoxButtons.OK);
            validated = false;
        }

        return validated;
    }
}
