using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Data;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.TextEditor.Data;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TextureViewer.Data;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EventScriptEditorNS;
using StudioCore.EzStateEditorNS;
using StudioCore.FileBrowserNS;
using StudioCore.Formats;
using StudioCore.Formats.JSON;
using StudioCore.GraphicsParamEditorNS;
using StudioCore.MaterialEditorNS;
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
    public bool EnableFileBrowser;

    public bool EnableExternalMaterialData;

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
    [JsonIgnore]
    public FileBrowserScreen FileBrowser;

    // Data Banks
    [JsonIgnore]
    public FileData FileData;
    [JsonIgnore]
    public MapData MapData;
    [JsonIgnore]
    public ParamData ParamData;
    [JsonIgnore]
    public MaterialData MaterialData;
    [JsonIgnore]
    public EmevdData EmevdData;
    [JsonIgnore]
    public EsdData EsdData;
    [JsonIgnore]
    public GparamData GparamData; 
    [JsonIgnore]
    public TextData TextData;
    [JsonIgnore]
    public TimeActData TimeActData;
    [JsonIgnore]
    public TextureData TextureData;

    /// <summary>
    /// Action manager for project-level changes (e.g. aliases)
    /// </summary>
    [JsonIgnore]
    public ActionManager ActionManager;

    // Additional Data
    [JsonIgnore]
    public AliasStore Aliases;

    [JsonIgnore]
    public ProjectEnumResource ProjectEnums;

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
    public MaterialDisplayConfiguration MaterialDisplayConfiguration;

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
        EnableFileBrowser = false;

        ActionManager = new ActionManager();
    }

    /// <summary>
    /// Setup project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Init(bool silent = false, InitType initType = InitType.ProjectDefined)
    {
        // Sanity checks
        if(ProjectType is ProjectType.Undefined)
        {
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Project Type is undefined.", LogLevel.Error, Tasks.LogPriority.High);
            return false;
        }

        if(!Directory.Exists(ProjectPath))
        {
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Project path does not exist: {ProjectPath}", LogLevel.Error, Tasks.LogPriority.High);
            return false;
        }

        if (!Directory.Exists(DataPath))
        {
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Data path does not exist: {DataPath}", LogLevel.Error, Tasks.LogPriority.High);
            return false;
        }

        /// The order of operations here is important:
        /// 1. Externals 
        /// 2. VFS
        /// 3. Aliases and other external information sources
        /// 4a. Data Bank
        /// 4b. Editor

        // --- Reset
        Initialized = false;
        IsInitializing = true;

        ActionManager = new();

        await Task.Yield();

        FocusedEditor = null;

        // DLLs
        Task<bool> dllGrabTask = SetupDLLs();
        bool dllGrabResult = await dllGrabTask;

        if (!dllGrabResult && !silent)
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to grab oo2core.dll");
        }

        // VFS
        Task<bool> vfsTask = SetupVFS();
        bool vfsSetup = await vfsTask;

        if (!silent)
        {
            if (vfsSetup)
            {
                TaskLogs.AddLog($"[{ProjectName}] Setup virtual filesystem.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}] Failed to setup virtual filesystem.");
            }
        }

        // Aliases
        Task<bool> aliasesTask = SetupAliases();
        bool aliasesSetup = await aliasesTask;

        if (!silent)
        {
            if (aliasesSetup)
            {
                TaskLogs.AddLog($"[{ProjectName}] Setup aliases.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}] Failed to setup aliases.");
            }
        }

        // Project Enums (per project)
        Task<bool> projectParamEnumTask = SetupProjectEnums();
        bool projectParamEnumResult = await projectParamEnumTask;

        if (!silent)
        {
            if (projectParamEnumResult)
            {
                TaskLogs.AddLog($"[{ProjectName}] Setup Project Param Enums.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}] Failed to setup Project Param Enums.");
            }
        }

        InitializeEditors(initType, silent);
        
        Initialized = true;
        IsInitializing = false;

        return true;
    }

    public async void InitializeEditors(InitType initType, bool silent = false)
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
        FileBrowser = null;

        MapData = null;
        ParamData = null;
        MaterialData = null;
        EmevdData = null;
        EsdData = null;
        GparamData = null;
        TextData = null;
        TextureData = null;
        TimeActData = null;
        FileData = null;

        // ---- Map Editor ----
        if (EnableMapEditor 
            && initType is InitType.ProjectDefined or InitType.MapEditorOnly
            && ProjectUtils.SupportsMapEditor(ProjectType))
        {
            // MSB Information
            Task<bool> msbInfoTask = SetupMsbInfo();
            bool msbInfoResult = await msbInfoTask;

            if (!silent)
            {
                if (msbInfoResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup MSB information.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup MSB information.");
                }
            }

            // Spawn States (per project) -- DS2 specific
            Task<bool> mapSpawnStatesTask = SetupMapSpawnStates();
            bool mapSpawnStatesResult = await mapSpawnStatesTask;

            if (!silent)
            {
                if (mapSpawnStatesResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup Spawn States information.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup Spawn States information.");
                }
            }

            // Entity Selection Groups (per project)
            Task<bool> entitySelectionGroupTask = SetupMapEntitySelections();
            bool entitySelectionGroupResult = await entitySelectionGroupTask;

            if (!silent)
            {
                if (entitySelectionGroupResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup Entity Selection Groups.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup Entity Selection Groups.");
                }
            }

            MapData = new(BaseEditor, this);

            // Map Data
            Task<bool> mapDataTask = MapData.Setup();
            bool mapDataTaskResult = await mapDataTask;

            if (!silent)
            {
                if (mapDataTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Setup Map Data Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Map Editor] Failed to setup Map Data Banks.");
                }
            }

            // Only do this once, as 3 editors may invoke this.
            if (MaterialData == null && EnableExternalMaterialData)
            {
                MaterialData = new(BaseEditor, this);

                Task<bool> materialDataTask = MaterialData.Setup();
                bool materialDataTaskResult = await materialDataTask;

                if (!silent)
                {
                    if (materialDataTaskResult)
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Setup Material Data.");
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Failed to setup Material Data.");
                    }
                }
            }

            MapEditor = new MapEditorScreen(BaseEditor, this);
        }

        // ---- Model Editor ----
        if (EnableModelEditor 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsModelEditor(ProjectType))
        {
            // FLVER Information
            Task<bool> flverInfoTask = SetupFlverInfo();
            bool flverInfoResult = await flverInfoTask;

            if (!silent)
            {
                if (flverInfoResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Model Editor] Setup FLVER information.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Model Editor] Failed to setup FLVER information.");
                }
            }

            // Only do this once, as 3 editors may invoke this.
            if (MaterialData == null && EnableExternalMaterialData)
            {
                MaterialData = new(BaseEditor, this);

                Task<bool> materialDataTask = MaterialData.Setup();
                bool materialDataTaskResult = await materialDataTask;

                if (!silent)
                {
                    if (materialDataTaskResult)
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Setup Material Data.");
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Failed to setup Material Data.");
                    }
                }
            }

            ModelEditor = new ModelEditorScreen(BaseEditor, this);
        }

        // ---- Text Editor ----
        if (EnableTextEditor 
            && initType is InitType.ProjectDefined or InitType.TextEditorOnly
            && ProjectUtils.SupportsTextEditor(ProjectType))
        {
            TextData = new(BaseEditor, this);

            // Text Banks
            Task<bool> textBankTask = TextData.Setup();
            bool textBankTaskResult = await textBankTask;

            if (!silent)
            {
                if (textBankTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Text Editor] Setup FMG Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Text Editor] Failed to setup FMG Banks.");
                }
            }

            TextEditor = new TextEditorScreen(BaseEditor, this);
        }

        // ---- Param Editor ----
        if (EnableParamEditor 
            && initType is InitType.ProjectDefined or InitType.ParamEditorOnly
            && ProjectUtils.SupportsParamEditor(ProjectType))
        {
            // Game Offsets (per project)
            Task<bool> gameOffsetTask = SetupParamMemoryOffsets();
            bool gameOffsetResult = await gameOffsetTask;

            if (!silent)
            {
                if (gameOffsetResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Param Memory Offsets.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Param Memory Offsets.");
                }
            }

            // Param Categories (per project)
            Task<bool> paramCategoryTask = SetupParamCategories();
            bool paramCategoryResult = await paramCategoryTask;

            if (!silent)
            {
                if (paramCategoryResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Param Categories.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Param Categories.");
                }
            }

            // Commutative Param Groups (per project)
            Task<bool> commutativeParamGroupTask = SetupCommutativeParamGroups();
            bool commutativeParamGroupResult = await commutativeParamGroupTask;

            if (!silent)
            {
                if (commutativeParamGroupResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup Commutative Param Groups.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup Commutative Param Groups.");
                }
            }

            ParamData = new(BaseEditor, this);

            // Param Banks
            Task<bool> paramBankTask = ParamData.Setup();
            bool paramBankTaskResult = await paramBankTask;

            if (!silent)
            {
                if (paramBankTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Setup PARAM Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Param Editor] Failed to setup PARAM Banks.");
                }
            }

            ParamEditor = new ParamEditorScreen(BaseEditor, this);

            // Placed here so the mass edit stuff is initialized once the editor is setup fully
            if(ParamEditor != null)
            {
                ParamEditor.MassEditHandler.Setup();
            }
        }

        // ---- Time Act Editor ----
        if (EnableTimeActEditor 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsTimeActEditor(ProjectType))
        {
            TimeActData = new(BaseEditor, this);

            // Time Act Banks
            Task<bool> timeActTask = TimeActData.Setup();
            bool timeActTaskResult = await timeActTask;

            if (!silent)
            {
                if (timeActTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Time Act Editor] Setup Time Act bank.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Time Act Editor] Failed to setup Time Act bank.");
                }
            }

            TimeActEditor = new TimeActEditorScreen(BaseEditor, this);
        }

        // ---- Graphics Param Editor ----
        if (EnableGparamEditor 
            && initType is InitType.ProjectDefined 
            && ProjectUtils.SupportsGraphicsParamEditor(ProjectType))
        {
            // GPARAM Information
            Task<bool> gparamInfoTask = SetupGparamInfo();
            bool gparamInfoResult = await gparamInfoTask;

            if (!silent)
            {
                if (gparamInfoResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Setup GPARAM information.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Failed to setup GPARAM information.");
                }
            }

            GparamData = new(BaseEditor, this);

            // Gparam Bank
            Task<bool> gparamBankTask = GparamData.Setup();
            bool gparamBankTaskResult = await gparamBankTask;

            if (!silent)
            {
                if (gparamBankTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Setup GPARAM Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Graphics Param Editor] Failed to setup GPARAM Banks.");
                }
            }

            GparamEditor = new GparamEditorScreen(BaseEditor, this);
        }

        // ---- Material Editor ----
        if (EnableMaterialEditor 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsMaterialEditor(ProjectType))
        {
            // Material Display Configuration
            Task<bool> matDispTask = SetupMaterialDisplayConfiguration();
            bool matDispTaskResult = await matDispTask;

            if (!silent)
            {
                if (matDispTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Material Editor] Setup Material Display Configuration.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Material Editor] Failed to setup Material Display Configuration.");
                }
            }

            // Only do this once, as 3 editors may invoke this.
            if (MaterialData == null)
            {
                MaterialData = new(BaseEditor, this);

                Task<bool> materialDataTask = MaterialData.Setup();
                bool materialDataTaskResult = await materialDataTask;

                if (!silent)
                {
                    if (materialDataTaskResult)
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Setup Material Data.");
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{ProjectName}] Failed to setup Material Data.");
                    }
                }
            }

            MaterialEditor = new MaterialEditorScreen(BaseEditor, this);
        }

        // ---- Event Script Editor ----
        if (EnableEmevdEditor 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsEventScriptEditor(ProjectType))
        {
            EmevdData = new(BaseEditor, this);

            // EMEVD Banks
            Task<bool> emevdBankTask = EmevdData.Setup();
            bool emevdBankResult = await emevdBankTask;

            if (!silent)
            {
                if (emevdBankResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Event Script Editor] Setup EMEVD Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Event Script Editor] Failed to setup EMEVD Banks.");
                }
            }

            EmevdEditor = new EmevdEditorScreen(BaseEditor, this);
        }

        // ---- EzState Script Editor ----
        if (EnableEsdEditor 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsEzStateScriptEditor(ProjectType))
        {
            EsdData = new(BaseEditor, this);

            // ESD Banks
            Task<bool> esdBankTask = EsdData.Setup();
            bool esdBankResult = await esdBankTask;

            if (!silent)
            {
                if (esdBankResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:EzState Script Editor] Setup ESD Banks.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:EzState Script Editor] Failed to setup ESD Banks.");
                }
            }

            EsdEditor = new EsdEditorScreen(BaseEditor, this);
        }

        // ---- Texture Viewer ----
        if (EnableTextureViewer 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsTextureViewer(ProjectType))
        {
            TextureData = new(BaseEditor, this);

            // Texture Banks
            Task<bool> textureDataTask = TextureData.Setup();
            bool textureDataTaskResult = await textureDataTask;

            if (!silent)
            {
                if (textureDataTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:Texture Viewer] Setup texture bank.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:Texture Viewer] Failed to setup texture bank.");
                }
            }

            TextureViewer = new TextureViewerScreen(BaseEditor, this);
        }

        // ---- File Browser ----
        if (EnableFileBrowser 
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsFileBrowser(ProjectType))
        {
            FileData = new(BaseEditor, this);

            // Text Banks
            Task<bool> fileDataTask = FileData.Setup();
            bool fileDataTaskResult = await fileDataTask;

            if (!silent)
            {
                if (fileDataTaskResult)
                {
                    TaskLogs.AddLog($"[{ProjectName}:File Browser] Setup directories.");
                }
                else
                {
                    TaskLogs.AddLog($"[{ProjectName}:File Browser] Setup directories.");
                }
            }

            FileBrowser = new FileBrowserScreen(BaseEditor, this);
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

        var commands = EditorCommandQueue.GetNextCommand();

        if (EnableMapEditor && MapEditor != null)
        {
            HandleEditor(commands, MapEditor, dt);
        }
        if (EnableModelEditor && ModelEditor != null)
        {
            HandleEditor(commands, ModelEditor, dt);
        }
        if (EnableTextEditor && TextEditor != null)
        {
            HandleEditor(commands, TextEditor, dt);
        }
        if (EnableParamEditor && ParamEditor != null)
        {
            HandleEditor(commands, ParamEditor, dt);
        }
        if (EnableTimeActEditor && TimeActEditor != null)
        {
            HandleEditor(commands, TimeActEditor, dt);
        }
        if (EnableGparamEditor && GparamEditor != null)
        {
            HandleEditor(commands, GparamEditor, dt);
        }
        if (EnableMaterialEditor && MaterialEditor != null)
        {
            HandleEditor(commands, MaterialEditor, dt);
        }
        if (EnableEmevdEditor && EmevdEditor != null)
        {
            HandleEditor(commands, EmevdEditor, dt);
        }
        if (EnableEsdEditor && EsdEditor != null)
        {
            HandleEditor(commands, EsdEditor, dt);
        }
        if (EnableTextureViewer && TextureViewer != null)
        {
            HandleEditor(commands, TextureViewer, dt);
        }
        if (EnableFileBrowser && FileBrowser != null)
        {
            HandleEditor(commands, FileBrowser, dt);
        }
    }

    /// <summary>
    /// Actual handling of the top-level window for each editor
    /// </summary>
    /// <param name="screen"></param>
    /// <param name="dt"></param>
    public unsafe void HandleEditor(string[] commands, EditorScreen screen, float dt)
    {
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
        await Task.Yield();

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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    FileDictionary = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the file dictionary: {filepath}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the file dictionary: {filepath}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        // Create merged dictionary, including unique entries present in the project directory only.
        var projectFileDictionary = ProjectUtils.BuildFromSource(ProjectPath, FileDictionary);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    Aliases = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.AliasStore);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the aliases: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the aliases: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    MsbInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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

                try
                {
                    var options = new JsonSerializerOptions();
                    MsbEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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

                try
                {
                    var options = new JsonSerializerOptions();
                    MsbMasks = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatMask);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB masks: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB masks: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    FlverInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the FLVER information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the FLVER information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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

                try
                {
                    var options = new JsonSerializerOptions();
                    FlverEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the FLVER enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the FLVER enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    GparamInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the GPARAM information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the GPARAM information: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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

                try
                {
                    var options = new JsonSerializerOptions();
                    GparamEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the GPARAM enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the GPARAM enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    ParamMemoryOffsets = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.GameOffsetResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Param Reload offsets: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Param Reload offsets: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
    public async Task<bool> SetupProjectEnums()
    {
        await Task.Yield();

        ProjectEnums = new();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    ProjectEnums = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectEnumResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Project Enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Project Enums: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    ParamCategories = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamCategoryResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Param Categories: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Param Categories: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    CommutativeParamGroups = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamCommutativeResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Commutative Param Groups: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Commutative Param Groups: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

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

                try
                {
                    var options = new JsonSerializerOptions();
                    MapSpawnStates = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.SpawnStateResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Map Spawn States: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Map Spawn States: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
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
        await Task.Yield();

        MapEntitySelections = new();

        // Information
        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\{ProjectUtils.GetGameDirectory(ProjectType)}\selections";
        var projectFile = Path.Combine(projectFolder, "selection_groups.json");

        if (File.Exists(projectFile))
        {
            try
            {
                var filestring = File.ReadAllText(projectFile);

                try
                {
                    var options = new JsonSerializerOptions();
                    MapEntitySelections = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.EntitySelectionGroupList);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Map Entity Selections: {projectFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Map Entity Selections: {projectFile}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return true;
    }
    #endregion

    #region Setup Material Display Configuration
    /// <summary>
    /// Setup the material display configuration for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMaterialDisplayConfiguration()
    {
        await Task.Yield();

        MaterialDisplayConfiguration = new();

        // Information
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\MATERIAL\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Display Configuration.json");

        var projectFolder = $@"{ProjectPath}\.smithbox\Assets\MATERIAL\{ProjectUtils.GetGameDirectory(ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Display Configuration.json");

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

                try
                {
                    var options = new JsonSerializerOptions();
                    MaterialDisplayConfiguration = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.MaterialDisplayConfiguration);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Material Display Configuration: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Material Display Configuration: {targetFile}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return true;
    }
    #endregion
}

/// <summary>
/// This is used to control which editors are loaded when initing a project via aux bank functions
/// </summary>
public enum InitType
{
    ProjectDefined,
    MapEditorOnly,
    ParamEditorOnly,
    TextEditorOnly
}