using Andre.IO.VFS;
using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.Editors.TextEditor.Data;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Formats.JSON;
using StudioCore.GraphicsEditor;
using StudioCore.MaterialEditor;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
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

    // Filesystems
    [JsonIgnore]
    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;
    [JsonIgnore]
    public VirtualFileSystem ProjectFS = EmptyVirtualFileSystem.Instance;
    [JsonIgnore]
    public VirtualFileSystem VanillaBinderFS = EmptyVirtualFileSystem.Instance;
    [JsonIgnore]
    public VirtualFileSystem VanillaRealFS = EmptyVirtualFileSystem.Instance;
    [JsonIgnore]
    public VirtualFileSystem VanillaFS = EmptyVirtualFileSystem.Instance;
    [JsonIgnore]
    public FileDictionary FileDictionary;

    // Editors
    [JsonIgnore]
    public Smithbox BaseEditor;
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

    // Data Banks
    [JsonIgnore]
    public EmevdBank EmevdBank; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public EsdBank EsdBank; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public GparamBank GparamBank; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public MsbBank MsbBank; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public MaterialBank MaterialBank; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public ParamData ParamData; // TODO: utilise file dictionary
    [JsonIgnore]
    public TextData TextData; // TODO: utilise file dictionary, change this to lazy load style
    [JsonIgnore]
    public TextureData TextureData; // TODO: utilise file dictionary
    [JsonIgnore]
    public TimeActData TimeActData; // TODO: utilise file dictionary, change this to lazy load style

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
    public bool Initialized = false;
    [JsonIgnore]
    public bool IsInitializing = false;
    [JsonIgnore]
    public bool IsSelected = false;

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

        // Defaults
        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableTextEditor = true;
        EnableParamEditor = true;
        EnableGparamEditor = true;
        EnableTextureViewer = true;

        EnableTimeActEditor = false;
        EnableMaterialEditor = false;
        EnableEmevdEditor = false;
        EnableEsdEditor = false;
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

        await Task.Delay(1);

        FocusedEditor = null;

        // DLLs
        Task<bool> dllGrabTask = SetupDLLs();
        bool dllGrabResult = await dllGrabTask;

        if (!dllGrabResult)
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to grab oo2core.dll");
        }

        // VFS
        Task<bool> vfsTask = SetupVFS();
        bool vfsSetup = await vfsTask;

        if (vfsSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup virtual filesystem.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup virtual filesystem.");
        }

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

        InitializeEditors();
        
        Initialized = true;
        IsInitializing = false;

        return true;
    }

    public async void InitializeEditors()
    {
        // Clear all existing editors and editor data
        MapEditor = null;
        ModelEditor = null;
        TextEditor = null;
        ParamEditor = null;
        TimeActEditor = null;
        GparamEditor = null;
        MaterialEditor = null;
        EmevdEditor = null;
        EsdEditor = null;
        TextureViewer = null;
        MapEditor = null;

        EmevdBank = null;
        EsdBank = null;
        GparamBank = null;
        MsbBank = null;
        MaterialBank = null;
        ParamData = null;
        TextData = null;
        TextureData = null;
        TimeActData = null;

        // ---- Map Editor ----
        if (EnableMapEditor)
        {
            // MSB Information
            Task<bool> msbInfoTask = SetupMsbInfo();
            bool msbInfoResult = await msbInfoTask;

            if (msbInfoResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup MSB information.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup MSB information.");
            }

            // Spawn States (per project) -- DS2 specific
            Task<bool> mapSpawnStatesTask = SetupMapSpawnStates();
            bool mapSpawnStatesResult = await mapSpawnStatesTask;

            if (mapSpawnStatesResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup Spawn States information.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup Spawn States information.");
            }

            // Entity Selection Groups (per project)
            Task<bool> entitySelectionGroupTask = SetupMapEntitySelections();
            bool entitySelectionGroupResult = await entitySelectionGroupTask;

            if (entitySelectionGroupResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup Entity Selection Groups.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup Entity Selection Groups.");
            }

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

        // ---- Model Editor ----
        if (EnableModelEditor)
        {
            // FLVER Information
            Task<bool> flverInfoTask = SetupFlverInfo();
            bool flverInfoResult = await flverInfoTask;

            if (flverInfoResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Model Editor] Setup FLVER information.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Model Editor] Failed to setup FLVER information.");
            }

            ModelEditor = new ModelEditorScreen(BaseEditor, this);
        }

        // ---- Text Editor ----
        if (EnableTextEditor)
        {
            TextData = new(BaseEditor, this);

            // Text Banks
            Task<bool> textBankTask = TextData.Setup();
            bool textBankTaskResult = await textBankTask;

            if (textBankTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Text Editor] Setup FMG Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Text Editor] Failed to setup FMG Banks.");
            }

            TextEditor = new TextEditorScreen(BaseEditor, this);
        }

        // ---- Param Editor ----
        if (EnableParamEditor)
        {
            // Game Offsets (per project)
            Task<bool> gameOffsetTask = SetupParamMemoryOffsets();
            bool gameOffsetResult = await gameOffsetTask;

            if (gameOffsetResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Param Memory Offsets.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Param Memory Offsets.");
            }

            // Param Categories (per project)
            Task<bool> paramCategoryTask = SetupParamCategories();
            bool paramCategoryResult = await paramCategoryTask;

            if (paramCategoryResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Param Categories.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Param Categories.");
            }

            // Commutative Param Groups (per project)
            Task<bool> commutativeParamGroupTask = SetupCommutativeParamGroups();
            bool commutativeParamGroupResult = await commutativeParamGroupTask;

            if (commutativeParamGroupResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Commutative Param Groups.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Commutative Param Groups.");
            }

            ParamData = new(BaseEditor, this);

            // Param Banks
            Task<bool> paramBankTask = ParamData.Setup();
            bool paramBankTaskResult = await paramBankTask;

            if (paramBankTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup PARAM Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup PARAM Banks.");
            }

            ParamEditor = new ParamEditorScreen(BaseEditor, this);
        }

        // ---- Time Act Editor ----
        if (EnableTimeActEditor)
        {
            TimeActData = new(BaseEditor, this);

            // Time Act Banks
            Task<bool> timeActTask = TimeActData.Setup();
            bool timeActTaskResult = await timeActTask;

            if (timeActTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Time Act Editor] Setup Time Act bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Time Act Editor] Failed to setup Time Act bank.");
            }

            TimeActEditor = new TimeActEditorScreen(BaseEditor, this);
        }

        // ---- Graphics Param Editor ----
        if (EnableGparamEditor)
        {
            // GPARAM Information
            Task<bool> gparamInfoTask = SetupGparamInfo();
            bool gparamInfoResult = await gparamInfoTask;

            if (gparamInfoResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Setup GPARAM information.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Failed to setup GPARAM information.");
            }

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

        // ---- Material Editor ----
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

        // ---- Event Script Editor ----
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

        // ---- EzState Script Editor ----
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

        // ---- Texture Viewer ----
        if (EnableTextureViewer)
        {
            TextureData = new(BaseEditor, this);

            // Texture Banks
            Task<bool> textureDataTask = TextureData.Setup();
            bool textureDataTaskResult = await textureDataTask;

            if (textureDataTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Texture Viewer] Setup texture bank.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Texture Viewer] Failed to setup texture bank.");
            }

            TextureViewer = new TextureViewerScreen(BaseEditor, this);
        }

    }

    /// <summary>
    /// Called when a new project is being selected.
    /// </summary>
    private bool SuspendUpdate = false;
    public void Suspend()
    {
        SuspendUpdate = true;
    }

    public void Reset()
    {
        SuspendUpdate = false;
    }

    /// <summary>
    /// Editor loop
    /// </summary>
    /// <param name="dt"></param>
    public unsafe void Update(float dt)
    {
        if (SuspendUpdate)
            return;

        if (EnableMapEditor && MapEditor != null)
        {
            HandleEditor(MapEditor, dt);
        }
        if (EnableModelEditor && ModelEditor != null)
        {
            HandleEditor(ModelEditor, dt);
        }
        if (EnableTextEditor && TextEditor != null)
        {
            HandleEditor(TextEditor, dt);
        }
        if (EnableParamEditor && ParamEditor != null)
        {
            HandleEditor(ParamEditor, dt);
        }
        if (EnableTimeActEditor && TimeActEditor != null)
        {
            HandleEditor(TimeActEditor, dt);
        }
        if (EnableGparamEditor && GparamEditor != null)
        {
            HandleEditor(GparamEditor, dt);
        }
        if (EnableMaterialEditor && MaterialEditor != null)
        {
            HandleEditor(MaterialEditor, dt);
        }
        if (EnableEmevdEditor && EmevdEditor != null)
        {
            HandleEditor(EmevdEditor, dt);
        }
        if (EnableEsdEditor && EsdEditor != null)
        {
            HandleEditor(EsdEditor, dt);
        }
        if (EnableTextureViewer && TextureViewer != null)
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
        if (SuspendUpdate)
            return;

        if (EnableMapEditor && MapEditor != null && FocusedEditor is MapEditorScreen)
        {
            MapEditor.EditorResized(window, device);
        }
        if (EnableModelEditor && ModelEditor != null && FocusedEditor is ModelEditorScreen)
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
        if (SuspendUpdate)
            return;

        if (EnableMapEditor && MapEditor != null && FocusedEditor is MapEditorScreen)
        {
            MapEditor.Draw(device, cl);
        }
        if (EnableModelEditor && ModelEditor != null && FocusedEditor is ModelEditorScreen)
        {
            ModelEditor.Draw(device, cl);
        }
    }

    #region Setup DLLS
    /// <summary>
    /// Grab the decompression DLLs if relevant to this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupDLLs()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup VFS
    public async Task<bool> SetupVFS()
    {
        await Task.Delay(1000);

        List<VirtualFileSystem> fileSystems = [];

        ProjectFS.Dispose();
        VanillaRealFS.Dispose();
        VanillaBinderFS.Dispose();
        VanillaFS.Dispose();
        FS.Dispose();

        // Order of addition to FS determines precendence when getting a file
        // e.g. ProjectFS is prioritised over VanillaFS

        // Project File System
        if (Directory.Exists(ProjectPath))
        {
            ProjectFS = new RealVirtualFileSystem(ProjectPath, false);
            fileSystems.Add(ProjectFS);
        }
        else
        {
            ProjectFS = EmptyVirtualFileSystem.Instance;
        }

        // Vanilla File System
        if (Directory.Exists(DataPath))
        {
            VanillaRealFS = new RealVirtualFileSystem(DataPath, false);
            fileSystems.Add(VanillaRealFS);

            var andreGame = ProjectType.AsAndreGame();

            if (andreGame != null)
            {
                if (!ProjectType.IsLooseGame())
                {
                    VanillaBinderFS = ArchiveBinderVirtualFileSystem.FromGameFolder(DataPath, andreGame.Value);
                    fileSystems.Add(VanillaBinderFS);
                }

                VanillaFS = new CompundVirtualFileSystem([VanillaRealFS, VanillaBinderFS]);
            }
            else
            {
                VanillaRealFS = EmptyVirtualFileSystem.Instance;
                VanillaFS = EmptyVirtualFileSystem.Instance;
            }
        }
        else
        {
            VanillaRealFS = EmptyVirtualFileSystem.Instance;
            VanillaFS = EmptyVirtualFileSystem.Instance;
        }


        if (fileSystems.Count == 0)
            FS = EmptyVirtualFileSystem.Instance;
        else
            FS = new CompundVirtualFileSystem(fileSystems);

        var folder = @$"{AppContext.BaseDirectory}\Assets\File Dictionaries\";
        var file = "";

        // Build the file dictionary JSON objects here
        switch (ProjectType)
        {
            case ProjectType.DES:
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = $"{folder}{file}";

        FileDictionary = new();
        FileDictionary.Entries = new();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = File.ReadAllText(filepath);
                var options = new JsonSerializerOptions();
                FileDictionary = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FileDictionary);

                if (FileDictionary == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load file dictionary.");
            }
        }

        // Create merged dictionary, including unique entries present in the project directory only.
        var projectFileDictionary = ProjectUtils.BuildFromSource(ProjectPath);
        FileDictionary = ProjectUtils.MergeFileDictionaries(FileDictionary, projectFileDictionary);

        return true;
    }

    #endregion

    #region Setup Aliases
    /// <summary>
    /// Setup the alias store for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupAliases()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup MSB Information
    /// <summary>
    /// Setup the MSB information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMsbInfo()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup FLVER Information
    /// <summary>
    /// Setup the FLVER information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupFlverInfo()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup GPARAM Information
    /// <summary>
    /// Setup the GPARAM information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupGparamInfo()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup Param Reloader Offsets
    /// <summary>
    /// Setup the PARAM memory offsets for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamMemoryOffsets()
    {
        await Task.Delay(1);

        ParamMemoryOffsets = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Param Reload Offsets.json");

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
    #endregion

    #region Setup Project Enums
    /// <summary>
    /// Setup the project-specific PARAM enums for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupProjectParamEnums()
    {
        await Task.Delay(1);

        ProjectParamEnums = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Shared Param Enums.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Shared Param Enums.json");

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
    #endregion

    #region Setup Param Categories
    /// <summary>
    /// Setup the param categories for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamCategories()
    {
        await Task.Delay(1);

        ParamCategories = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Param Categories.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Param Categories.json");

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
    #endregion

    #region Setup Commutative Param Groups

    /// <summary>
    /// Setup the param categories for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupCommutativeParamGroups()
    {
        await Task.Delay(1);

        CommutativeParamGroups = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Commutative Params.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Commutative Params.json");

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
    #endregion

    #region Setup Map Spawn States
    /// <summary>
    /// Setup the map spawn states for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMapSpawnStates()
    {
        await Task.Delay(1);

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
    #endregion

    #region Setup Map Entity Selections
    /// <summary>
    /// Setup the map spawn states for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMapEntitySelections()
    {
        await Task.Delay(1);

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
    #endregion
}