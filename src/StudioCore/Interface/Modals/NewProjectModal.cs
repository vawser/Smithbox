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

public class NewProjectModal
{
    public bool _standardProjectUIOpened = true;
    public NewProjectOptions NewProjectOpts;

    public NewProjectModal()
    {

    }

    public void CreateNewProjectModal()
    {
        ImGui.BeginTabBar("NewProjectTabBar");

        if (NewProjectOpts == null)
        {
            NewProjectOpts = new NewProjectOptions();
        }

        if (Smithbox.ProjectHandler.CurrentProject.Config == null)
        {
            Smithbox.ProjectHandler.CurrentProject.Config = new ProjectConfiguration();
        }

        if (ImGui.BeginTabItem("Standard"))
        {
            if (!_standardProjectUIOpened)
            {
                Smithbox.ProjectHandler.CurrentProject.Config.GameType = ProjectType.Undefined;
            }

            _standardProjectUIOpened = true;

            NewProject_NameGUI();
            NewProject_ProjectDirectoryGUI();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Game Executable:   ");
            ImGui.SameLine();
            Utils.ImGuiGenericHelpPopup("?", "##Help_GameExecutable",
                "The location of the game's .EXE or EBOOT.BIN file.\nThe folder with the executable will be used to obtain unpacked game data.");
            ImGui.SameLine();

            var gname = Smithbox.ProjectHandler.CurrentProject.Config != null ? Smithbox.ProjectHandler.CurrentProject.Config.GameRoot : "";
            if (ImGui.InputText("##gdir", ref gname, 255))
            {
                if (File.Exists(gname))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = Path.GetDirectoryName(gname);
                }
                else
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = gname;
                }

                Smithbox.ProjectHandler.CurrentProject.Config.GameType = Smithbox.ProjectHandler.GetProjectTypeFromExecutable(gname);

                if (Smithbox.ProjectHandler.CurrentProject.Config.GameType == ProjectType.BB)
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.GameRoot += @"\dvdroot_ps4";
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
                    Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = Path.GetDirectoryName(path);
                    Smithbox.ProjectHandler.CurrentProject.Config.GameType = Smithbox.ProjectHandler.GetProjectTypeFromExecutable(path);

                    if (Smithbox.ProjectHandler.CurrentProject.Config.GameType == ProjectType.BB)
                    {
                        Smithbox.ProjectHandler.CurrentProject.Config.GameRoot += @"\dvdroot_ps4";
                    }
                }
            }

            ImGui.Text($@"Detected Game:      {Smithbox.ProjectHandler.CurrentProject.Config.GameType}");

            ImGui.EndTabItem();
        }
        else
        {
            _standardProjectUIOpened = false;
        }

        if (ImGui.BeginTabItem("Advanced"))
        {
            NewProject_NameGUI();
            NewProject_ProjectDirectoryGUI();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Game Directory:    ");
            ImGui.SameLine();
            Utils.ImGuiGenericHelpPopup("?", "##Help_GameDirectory",
                "The location of game files.\nTypically, this should be the location of the game executable.");
            ImGui.SameLine();
            var gname = Smithbox.ProjectHandler.CurrentProject.Config.GameRoot;
            if (ImGui.InputText("##gdir", ref gname, 255))
            {
                Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = gname;
            }

            ImGui.SameLine();
            if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = path;
                }
            }

            NewProject_GameTypeComboGUI();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        //

        ImGui.Separator();
        if (Smithbox.ProjectHandler.CurrentProject.Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
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
            var looseparams = Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams;
            if (ImGui.Checkbox("##looseparams", ref looseparams))
            {
                Smithbox.ProjectHandler.CurrentProject.Config.UseLooseParams = looseparams;
            }
        }

        ImGui.NewLine();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(@"Import row names:  ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ImportRowNames",
            "Default: ON\nImports and applies row names from lists stored in Assets folder.\nRow names can be imported at any time in the param editor's Edit menu.");
        ImGui.SameLine();
        ImGui.Checkbox("##loadDefaultNames", ref NewProjectOpts.loadDefaultNames);
        ImGui.NewLine();

        if (Smithbox.ProjectHandler.CurrentProject.Config.GameType == ProjectType.Undefined)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Create", new Vector2(120, 0) * Smithbox.GetUIScale()))
        {
            var validated = true;
            if (Smithbox.ProjectHandler.CurrentProject.Config.GameRoot == null ||
                !Directory.Exists(Smithbox.ProjectHandler.CurrentProject.Config.GameRoot))
            {
                PlatformUtils.Instance.MessageBox(
                    "Your game executable path does not exist. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && Smithbox.ProjectHandler.CurrentProject.Config.GameType == ProjectType.Undefined)
            {
                PlatformUtils.Instance.MessageBox("Your game executable is not a valid supported game.",
                    "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && (NewProjectOpts.directory == null ||
                              !Directory.Exists(NewProjectOpts.directory)))
            {
                PlatformUtils.Instance.MessageBox("Your selected project directory is not valid.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && File.Exists($@"{NewProjectOpts.directory}\project.json"))
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

            if (validated && Smithbox.ProjectHandler.CurrentProject.Config.GameRoot == NewProjectOpts.directory)
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

            if (validated && (Smithbox.ProjectHandler.CurrentProject.Config.ProjectName == null ||
                               Smithbox.ProjectHandler.CurrentProject.Config.ProjectName == ""))
            {
                PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            var gameroot = Smithbox.ProjectHandler.CurrentProject.Config.GameRoot;

            Smithbox.ProjectHandler.CheckUnpackedState();

            if (validated)
            {
                // Remove previous CurrentProject entries for these variables
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams = new();
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows = new();
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields = new();

                Smithbox.ProjectHandler.CurrentProject.Config.GameRoot = gameroot;
                Smithbox.ProjectHandler.CurrentProject.ProjectJsonPath = $@"{NewProjectOpts.directory}\project.json";

                Smithbox.ProjectHandler.WriteProjectConfig();
                Smithbox.ProjectHandler.LoadProject(Smithbox.ProjectHandler.CurrentProject.ProjectJsonPath);

                ImGui.CloseCurrentPopup();
            }
        }

        if (Smithbox.ProjectType == ProjectType.Undefined)
        {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel", new Vector2(120, 0) * Smithbox.GetUIScale()))
        {
            ImGui.CloseCurrentPopup();
        }
    }

    public void NewProject_NameGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectName",
            "Project's display name. Only affects visuals within Smithbox.");
        ImGui.SameLine();

        var pname = Smithbox.ProjectHandler.CurrentProject.Config != null ? Smithbox.ProjectHandler.CurrentProject.Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            Smithbox.ProjectHandler.CurrentProject.Config.ProjectName = pname;
        }
    }

    public void NewProject_ProjectDirectoryGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Directory: ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectDirectory",
            "The location mod files will be saved.\nTypically, this should be Mod Engine's Mod folder.");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref NewProjectOpts.directory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
            {
                NewProjectOpts.directory = path;
            }
        }
    }

    public void NewProject_GameTypeComboGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(@"Game Type:         ");
        ImGui.SameLine();
        var games = Enum.GetNames(typeof(ProjectType));
        var gameIndex = Array.IndexOf(games, Smithbox.ProjectHandler.CurrentProject.Config.GameType.ToString());
        if (ImGui.Combo("##GameTypeCombo", ref gameIndex, games, games.Length))
        {
            Smithbox.ProjectHandler.CurrentProject.Config.GameType = Enum.Parse<ProjectType>(games[gameIndex]);
        }
    }
}
public class NewProjectOptions
{
    public string directory = "";
    public bool loadDefaultNames = false;

    public NewProjectOptions()
    {

    }
}
