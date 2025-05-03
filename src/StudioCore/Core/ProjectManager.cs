using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Formats.JSON;
using StudioCore.GraphicsEditor;
using StudioCore.MaterialEditor;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Core;

/// <summary>
/// Manager for the project data
/// </summary>
public class ProjectManager
{
    private Smithbox BaseEditor;

    public List<ProjectEntry> Projects = new();
    public ProjectEntry SelectedProject;

    public ProjectManager(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Update(float dt)
    {
        var flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;

        // Project List
        ImGui.Begin("Projects##projectList");

        DisplayProjectList();

        ImGui.End();

        // Project Dock
        ImGui.Begin("Project##projectDock");

        DisplayProjectDock(dt);

        ImGui.End();
    }

    /// <summary>
    /// Display the project list -- contains all stored project entries.
    /// </summary>
    private async void DisplayProjectList()
    {
        // WIP: once process is done, implement the project creation modal
        if(ImGui.Button("Test Project"))
        {
            var newProject = new ProjectEntry(
                BaseEditor,
                Guid.NewGuid(),
                "Test Project",
                "G:\\Modding\\Elden Ring\\Projects\\ER-Test\\mod",
                "F:\\SteamLibrary\\steamapps\\common\\ELDEN RING\\Game",
                ProjectType.ER);

            Projects.Add(newProject);

            Task<bool> projectSetupTask = newProject.Init();
            bool projectSetupTaskResult = await projectSetupTask;

            SelectedProject = newProject;
        }
    }

    /// <summary>
    /// Display the project dock -- contains all the editor windows
    /// </summary>
    /// <param name="dt"></param>
    private void DisplayProjectDock(float dt)
    {
        if(SelectedProject == null)
        {
            ImGui.Text("No project has been selected yet.");
            return;
        }

        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.MenuBar;

        ImGui.Begin("ProjectDockspace_W", windowFlags);

        uint dockspaceID = ImGui.GetID("ProjectDockspace");

        ImGui.End();

        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        SelectedProject.Update(dt);
    }

    /// <summary>
    /// Pass editor resized along to selected project (if present)
    /// </summary>
    /// <param name="window"></param>
    /// <param name="device"></param>
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (SelectedProject == null)
            return;

        SelectedProject.EditorResized(window, device);
    }

    /// <summary>
    /// Pass editor draw along to selected project (if present)
    /// </summary>
    /// <param name="device"></param>
    /// <param name="cl"></param>
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (SelectedProject == null)
            return;

        SelectedProject.Draw(device, cl);
    }
}

/// <summary>
/// Represents a 'project'
/// </summary>
public class ProjectEntry
{
    // JSON
    public Guid ProjectGUID;
    public string ProjectName;
    public string ProjectPath;
    public string DataPath;
    public ProjectType ProjectType;

    public bool ImportedParamRowNames;
    public bool EnableParamRowStrip;
    public bool AutoSelect;

    public bool EnableMapEditor;
    public bool EnableModelEditor;
    public bool EnableTextEditor;
    public bool EnableParamEditor;
    public bool EnableTimeActEditor;
    public bool EnableGparamEditor;
    public bool EnableMaterialEditor;
    public bool EnableEmevdEditor;
    public bool EnableEsdEditor;
    public bool EnableTextureViewer;

    // Legacy
    public List<string> PinnedParams { get; set; } = new();
    public Dictionary<string, List<int>> PinnedRows { get; set; } = new();
    public Dictionary<string, List<string>> PinnedFields { get; set; } = new();

    // Editors
    [JsonIgnore]
    public Smithbox BaseEditor;
    [JsonIgnore]
    public List<EditorScreen> EditorList = new();
    [JsonIgnore]
    public EditorScreen FocusedEditor;
    [JsonIgnore]
    public MapEditorScreen MapEditor;
    [JsonIgnore]
    public ModelEditorScreen ModelEditor;
    [JsonIgnore]
    public TextEditorScreen TextEditor;
    [JsonIgnore]
    public ParamEditorScreen ParamEditor;
    [JsonIgnore]
    public TimeActEditorScreen TimeActEditor;
    [JsonIgnore]
    public GparamEditorScreen GparamEditor;
    [JsonIgnore]
    public MaterialEditorScreen MaterialEditor;
    [JsonIgnore]
    public EmevdEditorScreen EmevdEditor;
    [JsonIgnore]
    public EsdEditorScreen EsdEditor;
    [JsonIgnore]
    public TextureViewerScreen TextureViewer;

    [JsonIgnore]
    public EmevdBank EmevdBank;
    [JsonIgnore]
    public EsdBank EsdBank;
    [JsonIgnore]
    public GparamBank GparamBank;
    [JsonIgnore]
    public MsbBank MsbBank;
    [JsonIgnore]
    public MaterialBank MaterialBank;

    // Additional Data
    [JsonIgnore]
    public AliasStore Aliases;

    [JsonIgnore]
    public ProjectEnumResource ProjectParamEnums;

    [JsonIgnore]
    public FormatResource MsbInformation;
    [JsonIgnore]
    public FormatEnum MsbEnums;
    [JsonIgnore]
    public FormatMask MsbMasks;

    [JsonIgnore]
    public FormatResource FlverInformation;
    [JsonIgnore]
    public FormatEnum FlverEnums;

    [JsonIgnore]
    public FormatResource GparamInformation;
    [JsonIgnore]
    public FormatEnum GparamEnums;

    [JsonIgnore]
    public GameOffsetResource ParamMemoryOffsets;

    [JsonIgnore]
    public ParamCategoryResource ParamCategories;

    [JsonIgnore]
    public ParamCommutativeResource CommutativeParamGroups;

    [JsonIgnore]
    public SpawnStateResource MapSpawnStates;

    [JsonIgnore]
    public EntitySelectionGroupList MapEntitySelections;

    [JsonIgnore]
    private bool Initialized = false;
    [JsonIgnore]
    private bool IsInitializing = false;
    [JsonIgnore]
    private bool IsSelected = false;

    public ProjectEntry() { }

    public ProjectEntry(Smithbox baseEditor, Guid newGuid, string projectName, string projectPath, string dataPath, ProjectType projectType)
    {
        BaseEditor = baseEditor;
        ProjectGUID = newGuid;
        ProjectName = projectName;
        ProjectPath = projectPath;
        DataPath = dataPath;
        ProjectType = projectType;
        ImportedParamRowNames = false;
        EnableParamRowStrip = false;

        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableTextEditor = true;
        EnableParamEditor = true;
        EnableTimeActEditor = true;
        EnableGparamEditor = true;
        EnableMaterialEditor = true;
        EnableEmevdEditor = true;
        EnableEsdEditor = true;
        EnableTextureViewer = true;
    }

    /// <summary>
    /// Setup project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Init()
    {
        /// The order of operations here is important:
        /// 1. Externals 
        /// 2. VFS
        /// 3. Aliases and other external information sources
        /// 4a. Data Bank
        /// 4b. Editor

        // --- Reset
        Initialized = false;
        IsInitializing = true;

        await Task.Delay(1000);

        EditorList.Clear();
        FocusedEditor = null;

        // DLLs
        Task<bool> dllGrabTask = SetupDLLs();
        bool dllGrabResult = await dllGrabTask;

        if (!dllGrabResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to grab oo2core.dll");
        }

        // VFS
        // TODO

        // Aliases
        Task<bool> aliasesTask = SetupAliases();
        bool aliasesSetup = await aliasesTask;

        if (aliasesSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup aliases.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup aliases.");
        }

        // MSB Information
        Task<bool> msbInfoTask = SetupMsbInfo();
        bool msbInfoResult = await msbInfoTask;

        if (msbInfoResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup MSB information.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup MSB information.");
        }

        // FLVER Information
        Task<bool> flverInfoTask = SetupFlverInfo();
        bool flverInfoResult = await flverInfoTask;

        if (flverInfoResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup FLVER information.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup FLVER information.");
        }

        // GPARAM Information
        Task<bool> gparamInfoTask = SetupGparamInfo();
        bool gparamInfoResult = await gparamInfoTask;

        if (gparamInfoResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup GPARAM information.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup GPARAM information.");
        }

        // Game Offsets (per project)
        Task<bool> gameOffsetTask = SetupParamMemoryOffsets();
        bool gameOffsetResult = await gameOffsetTask;

        if (gameOffsetResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Param Memory Offsets.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Param Memory Offsets.");
        }

        // Project Enums (per project)
        Task<bool> projectParamEnumTask = SetupProjectParamEnums();
        bool projectParamEnumResult = await projectParamEnumTask;

        if (projectParamEnumResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Project Param Enums.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Project Param Enums.");
        }

        // Param Categories (per project)
        Task<bool> paramCategoryTask = SetupParamCategories();
        bool paramCategoryResult = await paramCategoryTask;

        if (paramCategoryResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Param Categories.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Param Categories.");
        }

        // Commutative Param Groups (per project)
        Task<bool> commutativeParamGroupTask = SetupCommutativeParamGroups();
        bool commutativeParamGroupResult = await commutativeParamGroupTask;

        if (commutativeParamGroupResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Commutative Param Groups.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Commutative Param Groups.");
        }

        // Spawn States (per project) -- DS2 specific
        Task<bool> mapSpawnStatesTask = SetupMapSpawnStates();
        bool mapSpawnStatesResult = await mapSpawnStatesTask;

        if (mapSpawnStatesResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Spawn States information.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Spawn States information.");
        }

        // Entity Selection Groups (per project)
        Task<bool> entitySelectionGroupTask = SetupMapEntitySelections();
        bool entitySelectionGroupResult = await entitySelectionGroupTask;

        if (entitySelectionGroupResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup Entity Selection Groups.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup Entity Selection Groups.");
        }

        // Editors
        if (EnableMapEditor)
        {
            MsbBank = new(BaseEditor, this);

            // MSB Bank
            Task<bool> msbBankTask = MsbBank.Setup();
            bool msbBankTaskResult = await msbBankTask;

            if (msbBankTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup MSB Bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup MSB Bank.");
            }

            MapEditor = new MapEditorScreen(BaseEditor, this);
        }
        if (EnableModelEditor)
        {
            ModelEditor = new ModelEditorScreen(BaseEditor, this);
        }
        if (EnableTextEditor)
        {
            TextEditor = new TextEditorScreen(BaseEditor, this);
        }
        if (EnableParamEditor)
        {
            ParamEditor = new ParamEditorScreen(BaseEditor, this);
        }
        if (EnableTimeActEditor)
        {
            TimeActEditor = new TimeActEditorScreen(BaseEditor, this);
        }
        if (EnableGparamEditor)
        {
            GparamBank = new(BaseEditor, this);

            // Gparam Bank
            Task<bool> gparamBankTask = GparamBank.Setup();
            bool gparamBankTaskResult = await gparamBankTask;

            if (gparamBankTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Setup GPARAM Bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Failed to setup GPARAM Bank.");
            }

            GparamEditor = new GparamEditorScreen(BaseEditor, this);
        }
        if (EnableMaterialEditor)
        {
            MaterialBank = new(BaseEditor, this);

            // Material Bank
            Task<bool> matBankTask = MaterialBank.Setup();
            bool matBankTaskResult = await matBankTask;

            if (matBankTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Material Editor] Setup Material Bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Material Editor] Failed to setup Material Bank.");
            }

            MaterialEditor = new MaterialEditorScreen(BaseEditor, this);
        }
        if (EnableEmevdEditor)
        {
            EmevdBank = new(BaseEditor, this);

            // EMEVD Bank
            Task<bool> emevdBankTask = EmevdBank.Setup();
            bool emevdBankResult = await emevdBankTask;

            if (emevdBankResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Event Script Editor] Setup EMEVD Bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Event Script Editor] Failed to setup EMEVD Bank.");
            }

            EmevdEditor = new EmevdEditorScreen(BaseEditor, this);
        }
        if (EnableEsdEditor)
        {
            EsdBank = new(BaseEditor, this);

            // ESD Bank
            Task<bool> esdBankTask = EsdBank.Setup();
            bool esdBankResult = await esdBankTask;

            if (esdBankResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:EzState Script Editor] Setup ESD Bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:EzState Script Editor] Failed to setup ESD Bank.");
            }

            EsdEditor = new EsdEditorScreen(BaseEditor, this);
        }
        if (EnableTextureViewer)
        {
            TextureViewer = new TextureViewerScreen(BaseEditor, this);
        }

        Initialized = true;
        IsInitializing = false;

        return true;
    }

    /// <summary>
    /// Editor loop
    /// </summary>
    /// <param name="dt"></param>
    public unsafe void Update(float dt)
    {
        if (EnableMapEditor)
        {
            HandleEditor(MapEditor, dt);
        }
        if (EnableModelEditor)
        {
            HandleEditor(ModelEditor, dt);
        }
        if (EnableTextEditor)
        {
            HandleEditor(TextEditor, dt);
        }
        if (EnableParamEditor)
        {
            HandleEditor(ParamEditor, dt);
        }
        if (EnableTimeActEditor)
        {
            HandleEditor(TimeActEditor, dt);
        }
        if (EnableGparamEditor)
        {
            HandleEditor(GparamEditor, dt);
        }
        if (EnableMaterialEditor)
        {
            HandleEditor(MaterialEditor, dt);
        }
        if (EnableEmevdEditor)
        {
            HandleEditor(EmevdEditor, dt);
        }
        if (EnableEsdEditor)
        {
            HandleEditor(EsdEditor, dt);
        }
        if (EnableTextureViewer)
        {
            HandleEditor(TextureViewer, dt);
        }
    }

    /// <summary>
    /// Actual handling of the top-level window for each editor
    /// </summary>
    /// <param name="screen"></param>
    /// <param name="dt"></param>
    public unsafe void HandleEditor(EditorScreen screen, float dt)
    {
        var commands = EditorCommandQueue.GetNextCommand();

        if (commands != null && commands[0] == screen.CommandEndpoint)
        {
            commands = commands[1..]; // Remove the target editor command
            ImGui.SetNextWindowFocus();
        }

        if (BaseEditor._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        if (ImGui.Begin(screen.EditorName, ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            screen.OnGUI(commands);
            ImGui.End();
            FocusedEditor = screen;
            screen.Update(dt);
        }
        else
        {
            // Reset this so on Focus the first frame focusing happens
            screen.OnDefocus();
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    /// <summary>
    /// Editor viewport resized
    /// </summary>
    /// <param name="window"></param>
    /// <param name="device"></param>
    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (EnableMapEditor)
        {
            MapEditor.EditorResized(window, device);
        }
        if (EnableModelEditor)
        {
            ModelEditor.EditorResized(window, device);
        }
    }

    /// <summary>
    /// Editor draw to viewport 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="cl"></param>
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (EnableMapEditor)
        {
            MapEditor.Draw(device, cl);
        }
        if (EnableModelEditor)
        {
            ModelEditor.Draw(device, cl);
        }
    }

    /// <summary>
    /// Grab the decompression DLLs if relevant to this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupDLLs()
    {
        await Task.Delay(1000);

        if (ProjectType is ProjectType.SDT or ProjectType.ER)
        {
            var rootDllPath = Path.Join(DataPath, "oo2core_6_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_6_win64.dll");

            if (!File.Exists(rootDllPath))
            {
                return false;
            }
            else
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        if (ProjectType is ProjectType.AC6)
        {
            var rootDllPath = Path.Join(DataPath, "oo2core_8_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_8_win64.dll");

            if (!File.Exists(rootDllPath))
            {
                return false;
            }
            else
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the alias store for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupAliases()
    {
        await Task.Delay(1000);

        Aliases = new();

        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\Aliases\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Aliases.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\Aliases\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Aliases.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                Aliases = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.AliasStore);

                if (Aliases == null)
                {
                    throw new Exception("[Smithbox] Failed to read Aliases.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Aliases.json");
            }
        }

        return true;
    }


    /// <summary>
    /// Setup the MSB information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMsbInfo()
    {
        await Task.Delay(1000);

        MsbInformation = new();
        MsbEnums = new();
        MsbMasks = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\MSB\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\MSB\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Core.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                MsbInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);

                if (MsbInformation == null)
                {
                    throw new Exception("[Smithbox] Failed to read MSB information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load MSB information.");
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                MsbEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);

                if (MsbEnums == null)
                {
                    throw new Exception("[Smithbox] Failed to read MSB enum information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load MSB enum information.");
            }
        }

        // Masks
        sourceFile = Path.Combine(sourceFolder, "Masks.json");

        projectFile = Path.Combine(projectFolder, "Masks.json");

        targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                MsbMasks = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatMask);

                if (MsbMasks == null)
                {
                    throw new Exception("[Smithbox] Failed to read MSB Mask information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load MSB Mask information.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the FLVER information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupFlverInfo()
    {
        await Task.Delay(1000);

        FlverInformation = new();
        FlverEnums = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\FLVER\";
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\FLVER\";
        var projectFile = Path.Combine(projectFolder, "Core.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                FlverInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);

                if (FlverInformation == null)
                {
                    throw new Exception("[Smithbox] Failed to read FLVER information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load FLVER information.");
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                FlverEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);

                if (FlverEnums == null)
                {
                    throw new Exception("[Smithbox] Failed to read FLVER enum information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load FLVER enum information.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the GPARAM information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupGparamInfo()
    {
        await Task.Delay(1000);

        GparamInformation = new();
        GparamEnums = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\GPARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\GPARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Core.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                GparamInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);

                if (GparamInformation == null)
                {
                    throw new Exception("[Smithbox] Failed to read GPARAM information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load GPARAM information.");
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                GparamEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);

                if (GparamEnums == null)
                {
                    throw new Exception("[Smithbox] Failed to read GPARAM enum information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load GPARAM enum information.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the PARAM memory offsets for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamMemoryOffsets()
    {
        await Task.Delay(1000);

        ParamMemoryOffsets = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Offsets.json");

        var targetFile = sourceFile;

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                ParamMemoryOffsets = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.GameOffsetResource);

                if (ParamMemoryOffsets == null)
                {
                    throw new Exception("[Smithbox] Failed to read PARAM memory offsets.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load PARAM memory offsets.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the project-specific PARAM enums for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupProjectParamEnums()
    {
        await Task.Delay(1000);

        ProjectParamEnums = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Enums.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Enums.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                ProjectParamEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectEnumResource);

                if (ProjectParamEnums == null)
                {
                    throw new Exception("[Smithbox] Failed to read project param enums.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read project param enums.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the param categories for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamCategories()
    {
        await Task.Delay(1000);

        ParamCategories = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Categories.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Categories.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                ParamCategories = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamCategoryResource);

                if (ParamCategories == null)
                {
                    throw new Exception("[Smithbox] Failed to read param categories.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read param categories.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the param categories for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupCommutativeParamGroups()
    {
        await Task.Delay(1000);

        CommutativeParamGroups = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "CommutativeGroups.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "CommutativeGroups.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                CommutativeParamGroups = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamCommutativeResource);

                if (CommutativeParamGroups == null)
                {
                    throw new Exception("[Smithbox] Failed to read commutative param groups.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read commutative param groups.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the map spawn states for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMapSpawnStates()
    {
        await Task.Delay(1000);

        MapSpawnStates = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\MSB\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "SpawnStates.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\MSB\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "SpawnStates.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = File.ReadAllText(targetFile);
                var options = new JsonSerializerOptions();
                MapSpawnStates = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.SpawnStateResource);

                if (MapSpawnStates == null)
                {
                    throw new Exception("[Smithbox] Failed to read map spawn state information.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read map spawn state information.");
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the map spawn states for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMapEntitySelections()
    {
        await Task.Delay(1000);

        MapEntitySelections = new();

        // Information
        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\{ProjectUtils.GetGameDirectory(ProjectType)}\selections";
        var projectFile = Path.Combine(projectFolder, "selection_groups.json");

        if (File.Exists(projectFile))
        {
            try
            {
                var filestring = File.ReadAllText(projectFile);
                var options = new JsonSerializerOptions();
                MapEntitySelections = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.EntitySelectionGroupList);

                if (MapEntitySelections == null)
                {
                    throw new Exception("[Smithbox] Failed to read map entity selections.");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read map entity selections.");
            }
        }

        return true;
    }
}