using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StudioCore.Platform;
using StudioCore.BanksMain;
using static StudioCore.CFG;
using ImGuiNET;
using StudioCore.Settings;
using StudioCore.Utilities;
using System.Numerics;
using System.Timers;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors;
using StudioCore.Locators;

namespace StudioCore.UserProject;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public static class Project
{
    public static ProjectType Type { get; set; } = ProjectType.Undefined;

    /// <summary>
    /// The game interroot where all the game assets are
    /// </summary>
    public static string GameRootDirectory { get; set; }

    /// <summary>
    /// An optional override mod directory where modded files are stored
    /// </summary>
    public static string GameModDirectory { get; set; }

    /// <summary>
    /// Directory where misc Smithbox files associated with a project are stored.
    /// </summary>
    public static string ProjectDataDir => @$"{GameModDirectory}\.smithbox";

    /// <summary>
    /// Holds the configuration parameters from the project.json
    /// </summary>
    public static ProjectConfiguration Config;

    /// <summary>
    /// Current project.json path.
    /// </summary>
    public static string ProjectJsonPath;

    /// <summary>
    /// Current automatic save timer.
    /// </summary>
    private static Timer AutomaticSaveTimer;

    /// <summary>
    /// Current recent project.
    /// </summary>
    private static CFG.RecentProject recentProject;

    /// <summary>
    /// Initialize, or re-initalize the automatic save timer.
    /// </summary>
    public static void UpdateTimer()
    {
        if (AutomaticSaveTimer != null)
        {
            AutomaticSaveTimer.Close();
        }

        if (CFG.Current.System_EnableAutoSave)
        {
            var interval = CFG.Current.System_AutoSaveIntervalSeconds * 1000;
            if (interval < 10000)
                interval = 10000;

            AutomaticSaveTimer = new Timer(interval);
            AutomaticSaveTimer.Elapsed += OnAutomaticSave;
            AutomaticSaveTimer.AutoReset = true;
            AutomaticSaveTimer.Enabled = true;
        }
    }

    /// <summary>
    /// Automatic save actions
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private static void OnAutomaticSave(Object source, ElapsedEventArgs e)
    {
        if (CFG.Current.System_EnableAutoSave)
        {
            if (Project.Type != ProjectType.Undefined)
            {
                if (CFG.Current.System_EnableAutoSave_Project)
                {
                    SaveProjectJson();
                }

                if (CFG.Current.System_EnableAutoSave_MapEditor)
                {
                    EditorContainer.MsbEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ModelEditor)
                {
                    EditorContainer.ModelEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_ParamEditor)
                {
                    EditorContainer.ParamEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_TextEditor)
                {
                    EditorContainer.TextEditor.SaveAll();
                }

                if (CFG.Current.System_EnableAutoSave_GparamEditor)
                {
                    EditorContainer.GparamEditor.SaveAll();
                }

                TaskLogs.AddLog($"Automatic Save occured at {e.SignalTime}");
            }
        }
    }

    /// <summary>
    /// Save the current project.json
    /// </summary>
    public static void SaveProjectJson()
    {
        if (Config != null && ProjectJsonPath != null)
        {
            SerializeProjectConfiguration();
        }
    }

    /// <summary>
    /// Serialize the Project JSON
    /// </summary>
    public static void SerializeProjectConfiguration()
    {
        if (ProjectJsonPath != "")
        {
            string jsonString = JsonSerializer.Serialize(Config, typeof(ProjectConfiguration), ProjectConfigurationSerializationContext.Default);

            try
            {
                var fs = new FileStream(ProjectJsonPath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    /// <summary>
    /// Deserialize the Project JSON
    /// </summary>
    public static void DeserializeProjectConfiguration()
    {
        if (File.Exists(ProjectJsonPath))
        {
            using (var stream = File.OpenRead(ProjectJsonPath))
            {
                Config = JsonSerializer.Deserialize(stream, ProjectConfigurationSerializationContext.Default.ProjectConfiguration);
            }
        }
    }

    /// <summary>
    /// Deserialize and check the last loaded Project JSON (if it exists)
    /// </summary>
    public static void CheckForLastProject()
    {
        if (CFG.Current.LastProjectFile != null && CFG.Current.LastProjectFile != "")
        {
            if (File.Exists(CFG.Current.LastProjectFile))
            {
                ProjectJsonPath = CFG.Current.LastProjectFile;

                DeserializeProjectConfiguration();

                if (Config == null)
                {
                    CFG.Current.LastProjectFile = "";
                    CFG.Save();
                }
                else
                {
                    try
                    {
                        AttemptLoadProject();
                    }
                    catch
                    {
                        CFG.Current.LastProjectFile = "";
                        CFG.Save();
                        PlatformUtils.Instance.MessageBox(
                            "Failed to load last project. Project will not be loaded after restart.",
                            "Project Load Error", MessageBoxButtons.OK);
                        throw;
                    }
                }
            }
            else
            {
                CFG.Current.LastProjectFile = "";
                CFG.Save();
                TaskLogs.AddLog($"Cannot load project: \"{CFG.Current.LastProjectFile}\" does not exist.",
                    LogLevel.Warning, TaskLogs.LogPriority.High);
            }
        }
    }

    /// <summary>
    /// Attempt to load Project JSON and setup Project.Config
    /// </summary>
    /// <param name="projectJsonPath"></param>
    /// <returns></returns>
    private static bool AttemptLoadProject()
    {
        var success = true;

        if (!Directory.Exists(Config.GameRoot))
        {
            success = false;
            PlatformUtils.Instance.MessageBox(
                $@"Could not find game data directory for {Config.GameType}. Please select the game executable.",
                "Error",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"Select executable for {Config.GameType}...",
                        new[] { FilterStrings.GameExecutableFilter },
                        out var path))
                {
                    Config.GameRoot = path;
                    ProjectType gametype = Project.GetProjectTypeFromExecutable(Config.GameRoot);

                    if (gametype == Config.GameType)
                    {
                        success = true;
                        Config.GameRoot = Path.GetDirectoryName(Config.GameRoot);

                        if (Config.GameType == ProjectType.BB)
                        {
                            Config.GameRoot += @"\dvdroot_ps4";
                        }

                        SerializeProjectConfiguration();

                        break;
                    }

                    PlatformUtils.Instance.MessageBox(
                        $@"Selected executable was not for {Config.GameType}. Please select the correct game executable.",
                        "Error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    break;
                }
            }
        }

        if (success)
        {
            if (!ResourceLocatorUtils.CheckFilesExpanded(Config.GameRoot, Config.GameType))
            {
                if (!GameNotUnpackedWarning(Config.GameType))
                {
                    return false;
                }
            }

            if (Config.GameType == ProjectType.SDT || Config.GameType == ProjectType.ER)
            {
                if (!StealGameDllIfMissing("oo2core_6_win64"))
                {
                    return false;
                }
            }
            else if (Config.GameType == ProjectType.AC6)
            {
                if (!StealGameDllIfMissing("oo2core_8_win64"))
                {
                    return false;
                }
            }

            Project.Type = Project.Config.GameType;
            Project.GameRootDirectory = Project.Config.GameRoot;
            Project.GameModDirectory = Path.GetDirectoryName(ProjectJsonPath);
            ResourceMapLocator.FullMapList = null;

            BankUtils.ReloadBanks();

            foreach (EditorScreen editor in Smithbox._editors)
            {
                editor.OnProjectChanged();
            }

            Smithbox.SetProgramTitle(Config.ProjectName);

            CFG.RecentProject recent = new()
            {
                Name = Config.ProjectName,
                GameType = Config.GameType,
                ProjectFile = ProjectJsonPath
            };
            CFG.AddMostRecentProject(recent);
        }

        return success;
    }

    /// <summary>
    /// Steal DLL files from game directories if present
    /// </summary>
    /// <param name="dllName"></param>
    /// <returns></returns>
    private static bool StealGameDllIfMissing(string dllName)
    {
        dllName = dllName + ".dll";
        if (File.Exists(Path.Join(Path.GetFullPath("."), dllName)))
        {
            return true;
        }

        if (!File.Exists(Path.Join(Config.GameRoot, dllName)))
        {
            PlatformUtils.Instance.MessageBox(
                $"Could not find file \"{dllName}\" in \"{Config.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
                "Error",
                MessageBoxButtons.OK);
            return false;
        }

        File.Copy(Path.Join(Config.GameRoot, dllName), Path.Join(Path.GetFullPath("."), dllName));
        return true;
    }

    /// <summary>
    /// Warning if game is not unpacked.
    /// </summary>
    /// <param name="gameType"></param>
    /// <returns></returns>
    private static bool GameNotUnpackedWarning(ProjectType gameType)
    {
        if (gameType is ProjectType.DS1 or ProjectType.DS2S or ProjectType.DS2)
        {
            TaskLogs.AddLog(
                $"The files for {gameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                LogLevel.Error, TaskLogs.LogPriority.High);

            return false;
        }

        TaskLogs.AddLog(
            $"The files for {gameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
            LogLevel.Warning);
        return true;
    }

    /// <summary>
    /// Return the ProjectType as a string for a filepath.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string GetGameIDForDir()
    {
        switch (Project.Type)
        {
            case ProjectType.DES:
                return "DES";
            case ProjectType.DS1:
                return "DS1";
            case ProjectType.DS1R:
                return "DS1R";
            case ProjectType.DS2:
                return "DS2";
            case ProjectType.DS2S:
                return "DS2S";
            case ProjectType.BB:
                return "BB";
            case ProjectType.DS3:
                return "DS3";
            case ProjectType.SDT:
                return "SDT";
            case ProjectType.ER:
                return "ER";
            case ProjectType.AC6:
                return "AC6";
            default:
                throw new Exception("Game type not set");
        }
    }

    /// <summary>
    /// Get a ProjectType based on the executable name.
    /// </summary>
    /// <param name="exePath"></param>
    /// <returns></returns>
    public static ProjectType GetProjectTypeFromExecutable(string exePath)
    {
        var type = ProjectType.Undefined;

        if (exePath.ToLower().Contains("darksouls.exe"))
        {
            type = ProjectType.DS1;
        }
        else if (exePath.ToLower().Contains("darksoulsremastered.exe"))
        {
            type = ProjectType.DS1R;
        }
        else if (exePath.ToLower().Contains("darksoulsii.exe"))
        {
            type = ProjectType.DS2S; // Default to SOTFS
        }
        else if (exePath.ToLower().Contains("darksoulsiii.exe"))
        {
            type = ProjectType.DS3;
        }
        else if (exePath.ToLower().Contains("eboot.bin"))
        {
            var path = Path.GetDirectoryName(exePath);
            if (Directory.Exists($@"{path}\dvdroot_ps4"))
            {
                type = ProjectType.BB;
            }
            else
            {
                type = ProjectType.DES;
            }
        }
        else if (exePath.ToLower().Contains("sekiro.exe"))
        {
            type = ProjectType.SDT;
        }
        else if (exePath.ToLower().Contains("eldenring.exe"))
        {
            type = ProjectType.ER;
        }
        else if (exePath.ToLower().Contains("armoredcore6.exe"))
        {
            type = ProjectType.AC6;
        }

        return type;
    }

    /// <summary>
    /// Create a recovery project folder.
    /// </summary>
    /// <returns></returns>
    public static bool CreateRecoveryProject()
    {
        if (GameRootDirectory == null || GameModDirectory == null)
            return false;

        try
        {
            var time = DateTime.Now.ToString("dd-MM-yyyy-(hh-mm-ss)", CultureInfo.InvariantCulture);

            GameModDirectory = GameModDirectory + $@"\recovery\{time}";

            if (!Directory.Exists(GameModDirectory))
            {
                Directory.CreateDirectory(GameModDirectory);
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Display the "Open Project" dialog.
    /// </summary>
    public static void OpenProjectDialog()
    {
        var success = PlatformUtils.Instance.OpenFileDialog("Choose the project json file", new[] { FilterStrings.ProjectJsonFilter }, out var projectJsonPath);

        ProjectJsonPath = projectJsonPath;

        if (success)
        {
            DeserializeProjectConfiguration();

            if (Config != null)
            {
                AttemptLoadProject();
            }
        }
    }

    /// <summary>
    /// Display all projects that have been opened recently.
    /// </summary>
    public static void DisplayRecentProjects()
    {
        recentProject = null;
        var id = 0;

        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DES
            if (p.GameType == ProjectType.DES)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1
            if (p.GameType == ProjectType.DS1)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS1R
            if (p.GameType == ProjectType.DS1R)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2
            if (p.GameType == ProjectType.DS2)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS2S
            if (p.GameType == ProjectType.DS2S)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // BB
            if (p.GameType == ProjectType.BB)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // DS3
            if (p.GameType == ProjectType.DS3)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // SDT
            if (p.GameType == ProjectType.SDT)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // ER
            if (p.GameType == ProjectType.ER)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
        foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
        {
            // AC6
            if (p.GameType == ProjectType.AC6)
            {
                RecentProjectEntry(p, id);

                id++;
            }
        }
    }

    private static void RecentProjectEntry(CFG.RecentProject p, int id)
    {
        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
        {
            if (File.Exists(p.ProjectFile))
            {
                ProjectJsonPath = p.ProjectFile;

                DeserializeProjectConfiguration();
                if (Config != null)
                {
                    if (AttemptLoadProject())
                    {
                        recentProject = p;
                    }
                }
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

    private static bool _standardProjectUIOpened = true;

    public class NewProjectOptions
    {
        public string directory = "";
        public bool loadDefaultNames = true;

        public NewProjectOptions()
        {
        }
    }

    public static NewProjectOptions NewProjectOpts { get; private set; }

    public static void CreateNewProjectModal()
    {
        ImGui.BeginTabBar("NewProjectTabBar");

        if (NewProjectOpts == null)
        {
            NewProjectOpts = new NewProjectOptions();
        }

        if(Config == null)
        {
            Config = new ProjectConfiguration();
        }

        if (ImGui.BeginTabItem("Standard"))
        {
            if (!_standardProjectUIOpened)
            {
                Config.GameType = ProjectType.Undefined;
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

            var gname = Config != null ? Config.GameRoot : "";
            if (ImGui.InputText("##gdir", ref gname, 255))
            {
                if (File.Exists(gname))
                {
                    Config.GameRoot = Path.GetDirectoryName(gname);
                }
                else
                {
                    Config.GameRoot = gname;
                }

                Config.GameType = Project.GetProjectTypeFromExecutable(gname);

                if (Config.GameType == ProjectType.BB)
                {
                    Config.GameRoot += @"\dvdroot_ps4";
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
                    Config.GameRoot = Path.GetDirectoryName(path);
                    Config.GameType = Project.GetProjectTypeFromExecutable(path);

                    if (Config.GameType == ProjectType.BB)
                    {
                        Config.GameRoot += @"\dvdroot_ps4";
                    }
                }
            }

            ImGui.Text($@"Detected Game:      {Config.GameType}");

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
            var gname = Config.GameRoot;
            if (ImGui.InputText("##gdir", ref gname, 255))
            {
                Config.GameRoot = gname;
            }

            ImGui.SameLine();
            if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                {
                    Config.GameRoot = path;
                }
            }

            NewProject_GameTypeComboGUI();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        //

        ImGui.Separator();
        if (Config.GameType is ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS3)
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
            var looseparams = Config.UseLooseParams;
            if (ImGui.Checkbox("##looseparams", ref looseparams))
            {
                Config.UseLooseParams = looseparams;
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

        if (Config.GameType == ProjectType.Undefined)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Create", new Vector2(120, 0) * Smithbox.GetUIScale()))
        {
            var validated = true;
            if (Config.GameRoot == null ||
                !Directory.Exists(Config.GameRoot))
            {
                PlatformUtils.Instance.MessageBox(
                    "Your game executable path does not exist. Please select a valid executable.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            if (validated && Config.GameType == ProjectType.Undefined)
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

            if (validated && Config.GameRoot == NewProjectOpts.directory)
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

            if (validated && (Config.ProjectName == null ||
                              Config.ProjectName == ""))
            {
                PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                    MessageBoxButtons.OK);
                validated = false;
            }

            var gameroot = Config.GameRoot;
            if (!ResourceLocatorUtils.CheckFilesExpanded(gameroot, Config.GameType))
            {
                if (!GameNotUnpackedWarning(Config.GameType))
                {
                    validated = false;
                }
            }

            if (validated)
            {
                Config.GameRoot = gameroot;

                ProjectJsonPath = $@"{NewProjectOpts.directory}\project.json";

                Project.SerializeProjectConfiguration();

                AttemptLoadProject();

                ImGui.CloseCurrentPopup();
            }
        }

        if (Config.GameType == ProjectType.Undefined)
        {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel", new Vector2(120, 0) * Smithbox.GetUIScale()))
        {
            ImGui.CloseCurrentPopup();
        }
    }

    private static void NewProject_NameGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectName",
            "Project's display name. Only affects visuals within Smithbox.");
        ImGui.SameLine();

        var pname = Config != null ? Config.ProjectName : "Blank";

        if (ImGui.InputText("##pname", ref pname, 255))
        {
            Config.ProjectName = pname;
        }
    }

    private static void NewProject_ProjectDirectoryGUI()
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

    private static void NewProject_GameTypeComboGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(@"Game Type:         ");
        ImGui.SameLine();
        var games = Enum.GetNames(typeof(ProjectType));
        var gameIndex = Array.IndexOf(games, Config.GameType.ToString());
        if (ImGui.Combo("##GameTypeCombo", ref gameIndex, games, games.Length))
        {
            Config.GameType = Enum.Parse<ProjectType>(games[gameIndex]);
        }
    }

}
