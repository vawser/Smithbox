using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Application;

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
    public bool AutoSelect;

    public bool EnableMapEditor;
    public bool EnableModelEditor;
    public bool EnableTextEditor;
    public bool EnableParamEditor;
    public bool EnableGparamEditor;
    public bool EnableMaterialEditor;
    public bool EnableTextureViewer;
    public bool EnableFileBrowser;

    public bool EnableExternalMaterialData;

    public string FolderTag = "";

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

    [JsonIgnore]
    public Smithbox BaseEditor;
    [JsonIgnore]
    public EditorScreen FocusedEditor;

    // Stubs
    [JsonIgnore]
    public MapEditorStub MapEditorStub;
    [JsonIgnore]
    public ModelEditorStub ModelEditorStub;
    [JsonIgnore]
    public TextEditorStub TextEditorStub;
    [JsonIgnore]
    public ParamEditorStub ParamEditorStub;
    [JsonIgnore]
    public GparamEditorStub GparamEditorStub;
    [JsonIgnore]
    public MaterialEditorStub MaterialEditorStub;
    [JsonIgnore]
    public TextureViewerStub TextureViewerStub;
    [JsonIgnore]
    public FileBrowserStub FileBrowserStub;

    // Editors
    [JsonIgnore]
    public MapEditorScreen MapEditor;
    [JsonIgnore]
    public ModelEditorScreen ModelEditor;
    [JsonIgnore]
    public TextEditorScreen TextEditor;
    [JsonIgnore]
    public ParamEditorScreen ParamEditor;
    [JsonIgnore]
    public GparamEditorScreen GparamEditor;
    [JsonIgnore]
    public MaterialEditorScreen MaterialEditor;
    [JsonIgnore]
    public TextureViewerScreen TextureViewer;
    [JsonIgnore]
    public FileBrowserScreen FileBrowser;

    // Data Banks
    [JsonIgnore]
    public CommonData CommonData;
    [JsonIgnore]
    public FileData FileData;
    [JsonIgnore]
    public MapData MapData;
    [JsonIgnore]
    public ModelData ModelData;
    [JsonIgnore]
    public ParamData ParamData;
    [JsonIgnore]
    public MaterialData MaterialData;
    [JsonIgnore]
    public GparamData GparamData;
    [JsonIgnore]
    public TextData TextData;
    [JsonIgnore]
    public TextureData TextureData;

    /// <summary>
    /// Action manager for project-level changes (e.g. aliases)
    /// </summary>
    [JsonIgnore]
    public ActionManager ActionManager;

    [JsonIgnore]
    public bool Initialized = false;
    [JsonIgnore]
    public bool IsInitializing = false;
    [JsonIgnore]
    public bool IsSelected = false;

    public int AutomaticSaveInterval = 60;

    [JsonIgnore]
    public DateTime _nextAutoSaveTime = DateTime.MinValue;

    public void UpdateAutoSaveInterval(int newInterval)
    {
        AutomaticSaveInterval = newInterval;
        _nextAutoSaveTime = DateTime.UtcNow.AddSeconds(AutomaticSaveInterval);
    }

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

        // Defaults
        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableTextEditor = true;
        EnableParamEditor = true;
        EnableGparamEditor = true;
        EnableTextureViewer = true;

        EnableMaterialEditor = false;
        EnableFileBrowser = false;

        ActionManager = new ActionManager();

        InitStubs(baseEditor);
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
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Project Type is undefined.", LogLevel.Error, LogPriority.High);
            return false;
        }

        if(!Directory.Exists(ProjectPath))
        {
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Project path does not exist: {ProjectPath}", LogLevel.Error, LogPriority.High);
            return false;
        }

        if (!Directory.Exists(DataPath))
        {
            TaskLogs.AddLog($"[{ProjectName}] Project initialization failed. Data path does not exist: {DataPath}", LogLevel.Error, LogPriority.High);
            return false;
        }

        InitStubs(BaseEditor);

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
            TaskLogs.AddLog($"[{ProjectName}] Failed to grab oo2core.");
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

        CommonData = new(BaseEditor, this);

        // Common Data
        Task<bool> commonDataTask = CommonData.Setup();
        bool commonDataResult = await commonDataTask;

        if (!silent)
        {
            if (commonDataResult)
            {
                TaskLogs.AddLog($"[{ProjectName}] Setup Common Data.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}] Failed to setup Common Data.");
            }
        }

        ClearEditors();
        await InitializeEditors(initType, silent);

        Initialized = true;
        IsInitializing = false;

        return true;
    }

    public void InitStubs(Smithbox baseEditor)
    {
        MapEditorStub = new(baseEditor, this);
        ModelEditorStub = new(baseEditor, this);
        TextEditorStub = new(baseEditor, this);
        ParamEditorStub = new(baseEditor, this);
        GparamEditorStub = new(baseEditor, this);
        MaterialEditorStub = new(baseEditor, this);
        TextureViewerStub = new(baseEditor, this);
        FileBrowserStub = new(baseEditor, this);
    }

    public void ClearEditors()
    {
        // Clear all existing editors and editor data
        MapEditor = null;
        ModelEditor = null;
        TextEditor = null;
        ParamEditor = null;
        GparamEditor = null;
        MaterialEditor = null;
        TextureViewer = null;
        MapEditor = null;
        FileBrowser = null;

        MapData = null;
        ModelData = null;
        ParamData = null;
        MaterialData = null;
        GparamData = null;
        TextData = null;
        TextureData = null;
        FileData = null;

    }

    public async Task InitializeEditors(InitType initType, bool silent = false)
    {
        List<Task> initTasks = [];

        // ---- Map Editor ----
        if (EnableMapEditor
            && initType is InitType.ProjectDefined or InitType.MapEditorOnly
            && ProjectUtils.SupportsMapEditor(ProjectType))
        {
            initTasks.Add(InitializeMapEditor(silent));
        }

        // ---- Model Editor ----
        if (EnableModelEditor
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsModelEditor(ProjectType))
        {
            initTasks.Add(InitializeModelEditor(silent));
        }

        // ---- Text Editor ----
        if (EnableTextEditor
            && initType is InitType.ProjectDefined or InitType.TextEditorOnly
            && ProjectUtils.SupportsTextEditor(ProjectType))
        {
            initTasks.Add(InitializeTextEditor(silent));
        }

        // ---- Param Editor ----
        if (EnableParamEditor
            && initType is InitType.ProjectDefined or InitType.ParamEditorOnly
            && ProjectUtils.SupportsParamEditor(ProjectType))
        {
            initTasks.Add(InitializeParamEditor(silent));
        }

        // ---- Graphics Param Editor ----
        if (EnableGparamEditor
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsGraphicsParamEditor(ProjectType))
        {
            initTasks.Add(InitializeGparamEditor(silent));
        }

        // ---- Material Editor ----
        if (EnableMaterialEditor
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsMaterialEditor(ProjectType))
        {
            initTasks.Add(InitializeMaterialEditor(silent));
        }

        // ---- Texture Viewer ----
        if (EnableTextureViewer
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsTextureViewer(ProjectType))
        {
            initTasks.Add(InitializeTextureViewer(silent));
        }

        // ---- File Browser ----
        if (EnableFileBrowser
            && initType is InitType.ProjectDefined
            && ProjectUtils.SupportsFileBrowser(ProjectType))
        {
            initTasks.Add(InitializeFileBrowser(silent));
        }

        await Task.WhenAll(initTasks);
    }

    private async Task InitializeFileBrowser(bool silent)
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

    private async Task InitializeTextureViewer(bool silent)
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

    private async Task InitializeMaterialEditor(bool silent)
    {
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

    private async Task InitializeGparamEditor(bool silent)
    {
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

    private async Task InitializeParamEditor(bool silent)
    {

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

        // Added this so throws during init can be logged
        try
        {
            ParamEditor = new ParamEditorScreen(BaseEditor, this);
        }
        catch(Exception ex)
        {
            TaskLogs.AddLog($"[{ProjectName}:Param Editor]: Failed to setup Param Editor: {ex}");
        }

        // Placed here so the mass edit stuff is initialized once the editor is setup fully
        if (ParamEditor != null)
        {
            ParamEditor.MassEditHandler.Setup();
        }
    }

    private async Task InitializeTextEditor(bool silent)
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
    private async Task InitializeModelEditor(bool silent)
    {
        ModelData = new(BaseEditor, this);

        // Model Data
        Task<bool> modelDataTask = ModelData.Setup();
        bool modelDataTaskResult = await modelDataTask;

        if (!silent)
        {
            if (modelDataTaskResult)
            {
                TaskLogs.AddLog($"[{ProjectName}:Model Editor] Setup Model Data Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[{ProjectName}:Model Editor] Failed to setup Model Data Banks.");
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

    private async Task InitializeMapEditor(bool silent)
    {
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

    /// <summary>
    /// Unload the project editors and data banks
    /// </summary>
    public void Unload()
    {
        Initialized = false;

        MapEditor = null;
        ModelEditor = null;
        TextEditor = null;
        ParamEditor = null;
        GparamEditor = null;
        MaterialEditor = null;
        TextureViewer = null;
        MapEditor = null;
        FileBrowser = null;

        MapData = null;
        ModelData = null;
        ParamData = null;
        MaterialData = null;
        GparamData = null;
        TextData = null;
        TextureData = null;
        FileData = null;
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

        if(MapEditorStub != null)
            MapEditorStub.Display(dt, commands);

        if (ModelEditorStub != null)
            ModelEditorStub.Display(dt, commands);

        if (TextEditorStub != null)
            TextEditorStub.Display(dt, commands);

        if (ParamEditorStub != null)
            ParamEditorStub.Display(dt, commands);

        if (GparamEditorStub != null)
            GparamEditorStub.Display(dt, commands);

        if (MaterialEditorStub != null)
            MaterialEditorStub.Display(dt, commands);

        if (TextureViewerStub != null)
            TextureViewerStub.Display(dt, commands);

        if (FileBrowserStub != null)
            FileBrowserStub.Display(dt, commands);

        // Auto-Save
        AutomaticSaveInterval = (int)CFG.Current.AutomaticSaveIntervalTime;

        if (AutomaticSaveInterval > 0 && DateTime.UtcNow >= _nextAutoSaveTime)
        {
            PerformAutoSave();
            _nextAutoSaveTime = DateTime.UtcNow.AddSeconds(AutomaticSaveInterval);
        }
    }
    private void PerformAutoSave()
    {
        if (CFG.Current.EnableAutomaticSave)
        {
            try
            {
                TaskLogs.AddLog($"[{ProjectName}] Auto-save triggered.", LogLevel.Information);

                if (CFG.Current.AutomaticSave_MapEditor)
                {
                    if (MapEditor != null)
                    {
                        MapEditor.Save();
                    }
                }

                if (CFG.Current.AutomaticSave_ParamEditor)
                {
                    if (ParamEditor != null)
                    {
                        ParamEditor.Save();
                    }
                }
                if (CFG.Current.AutomaticSave_TextEditor)
                {
                    if (TextEditor != null)
                    {
                        TextEditor.Save();
                    }
                }
                if (CFG.Current.AutomaticSave_GparamEditor)
                {
                    if (GparamEditor != null)
                    {
                        GparamEditor.Save();
                    }
                }
                if (CFG.Current.AutomaticSave_MaterialEditor)
                {
                    if (MaterialEditor != null)
                    {
                        MaterialEditor.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"[{ProjectName}] Auto-save failed.", LogLevel.Error, LogPriority.High, ex);
            }
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

        if (MapEditorStub != null)
            MapEditorStub.EditorResized(window, device);

        if (ModelEditorStub != null)
            ModelEditorStub.EditorResized(window, device);
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

        if (MapEditorStub != null)
            MapEditorStub.Draw(device, cl);

        if (ModelEditorStub != null)
            ModelEditorStub.Draw(device, cl);
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
#if WINDOWS
            var rootDllPath = Path.Join(DataPath, "oo2core_6_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_6_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.6.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.6.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.6");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.6");
#endif

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
#if WINDOWS
            var rootDllPath = Path.Join(DataPath, "oo2core_8_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_8_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.8.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.8.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.8");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.8");
#endif

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


        if (ProjectType is ProjectType.NR)
        {
#if WINDOWS
            var rootDllPath = Path.Join(DataPath, "oo2core_9_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_9_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.9.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.9.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.9");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.9");
#endif

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

        var folder = Path.Join(AppContext.BaseDirectory,"Assets","File Dictionaries");
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
            case ProjectType.NR:
                file = "NR-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = Path.Join(folder, file);

        FileDictionary = new();
        FileDictionary.Entries = new();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(filepath);

                try
                {
                    FileDictionary = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the file dictionary: {filepath}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the file dictionary: {filepath}", LogLevel.Error, LogPriority.High, e);
            }
        }

        // Create merged dictionary, including unique entries present in the project directory only.
        var projectFileDictionary = ProjectUtils.BuildFromSource(ProjectPath, FileDictionary, ProjectType);
        FileDictionary = ProjectUtils.MergeFileDictionaries(FileDictionary, projectFileDictionary);

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