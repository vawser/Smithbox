using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Smithbox.Core.FileBrowserNS;
using Smithbox.Core.MapEditorNS;
using StudioCore.Editor;
using StudioCore.Editors.BehaviorEditorNS;
using StudioCore.Editors.CutsceneEditorNS;
using StudioCore.Editors.EventScriptEditorNS;
using StudioCore.Editors.MapEditorNS;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Resources.JSON;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Core.ProjectNS;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public class Project
{
    public Guid ProjectGUID;
    public string ProjectName;
    public string ProjectPath;
    public string DataPath;
    public ProjectType ProjectType;

    /// <summary>
    /// Tracked so we only ever import param row names automatically once.
    /// </summary>
    public bool ImportedParamRowNames;

    /// <summary>
    /// Tracked so we know when to apply Param Row stripping, and if to restore them.
    /// </summary>
    public bool EnableParamRowStrip;

    /// <summary>
    /// If true, then this project is auto-selected on program start.
    /// </summary>
    public bool AutoSelect;

    /// <summary>
    /// Pinned elements in the Param Editor (TODO: remove if we overhaul editor to use drag and drop ordering)
    /// </summary>
    public List<string> PinnedParams;
    public Dictionary<string, List<int>> PinnedRows;
    public Dictionary<string, List<string>> PinnedFields;

    // Editors enabled for this project
    public bool EnableFileBrowser;
    public bool EnableMapEditor;
    public bool EnableModelEditor;
    public bool EnableParamEditor;
    public bool EnableTextEditor;
    public bool EnableCutsceneEditor;
    public bool EnableEventScriptEditor;
    public bool EnableEzStateEditor;
    public bool EnableGparamEditor;
    public bool EnableMaterialEditor;
    public bool EnableBehaviorEditor;
    public bool EnableTextureEditor;
    public bool EnableTimeActEditor;

    /// <summary>
    /// Base class
    /// </summary>
    [JsonIgnore]
    public BaseEditor BaseEditor;

    /// <summary>
    /// Base class
    /// </summary>
    [JsonIgnore]
    public SoapstoneService SoapstoneService;

    /// <summary>
    /// Action manager for project-level changes (e.g. aliases)
    /// </summary>
    [JsonIgnore]
    public ActionManager ActionManager;

    /// <summary>
    /// If true, the data elements (i.e. Aliases and Editors) for this project have been initialized.
    /// </summary>
    [JsonIgnore]
    private bool Initialized = false;

    /// <summary>
    /// Track if we are in the process of initialization
    /// </summary>
    [JsonIgnore]
    private bool IsInitializing = false;

    /// <summary>
    /// If true, this project is the currently selected project.
    /// </summary>
    [JsonIgnore]
    public bool IsSelected = false;

    /// <summary>
    /// Compound filesystem, contains all the other systems, in the order of precedence
    /// Project -> Vanilla (VanillaReal -> VanillaBinder)
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem FS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for files in the Project directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem ProjectFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the inner (bindered) files in the Data directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaBinderFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the files in the Data directory
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaRealFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// Filesystem for the files in the Data directory (groups Binder and Real) 
    /// </summary>
    [JsonIgnore]
    public VirtualFileSystem VanillaFS = EmptyVirtualFileSystem.Instance;

    /// <summary>
    /// The file dictionary used by this project.
    /// Used to build file lists without explicit lookups, since we are using VFS
    /// </summary>
    [JsonIgnore]
    public FileDictionary FileDictionary;

    /// <summary>
    /// Aliases
    /// </summary>
    [JsonIgnore]
    public AliasStore Aliases;

    /// <summary>
    /// Shoebox Layouts
    /// </summary>
    [JsonIgnore]
    public ShoeboxLayoutContainer ShoeboxLayouts;

    // File Browser
    [JsonIgnore]
    public FileBrowser FileBrowser;

    // Map Editor
    [JsonIgnore]
    public MapData MapData;

    [JsonIgnore]
    public MapEditor MapEditor;

    // Model Editor
    [JsonIgnore]
    public ModelData ModelData;

    [JsonIgnore]
    public ModelEditor ModelEditor;

    // Param Editor
    [JsonIgnore]
    public ParamData ParamData;

    [JsonIgnore]
    public ParamEditor ParamEditor;

    // Text Editor
    [JsonIgnore]
    public TextData TextData;

    [JsonIgnore]
    public TextEditor TextEditor;

    // Cutscene Editor
    [JsonIgnore]
    public CutsceneData CutsceneData;

    [JsonIgnore]
    public CutsceneEditor CutsceneEditor;

    // Emevd Editor
    [JsonIgnore]
    public EventScriptData EventScriptData;

    [JsonIgnore]
    public EventScriptEditor EventScriptEditor;

    // Esd Editor
    [JsonIgnore]
    public EzStateData EzStateData;

    [JsonIgnore]
    public EzStateEditor EzStateEditor;

    // Gparam Editor
    [JsonIgnore]
    public GparamData GparamData;

    [JsonIgnore]
    public GparamEditor GparamEditor;

    // Material Editor
    [JsonIgnore]
    public MaterialData MaterialData;

    [JsonIgnore]
    public MaterialEditor MaterialEditor;

    // Behavior Editor
    [JsonIgnore]
    public BehaviorData BehaviorData;

    [JsonIgnore]
    public BehaviorEditor BehaviorEditor;

    // Cutscene Editor
    [JsonIgnore]
    public TextureData TextureData;

    [JsonIgnore]
    public TextureEditor TextureEditor;

    // Time Act Editor
    [JsonIgnore]
    public TimeActData TimeActData;

    [JsonIgnore]
    public TimeActEditor TimeActEditor;

    public Project() { }

    public Project(BaseEditor baseEditor, Guid newGuid, string projectName, string projectPath, string dataPath, ProjectType projectType)
    {
        BaseEditor = baseEditor;
        ProjectGUID = newGuid;
        ProjectName = projectName;
        ProjectPath = projectPath;
        DataPath = dataPath;
        ProjectType = projectType;
        ImportedParamRowNames = false;
        EnableParamRowStrip = false;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        EnableFileBrowser = true;
        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableParamEditor = true;
        EnableTextEditor = true;
        EnableCutsceneEditor = true;
        EnableEventScriptEditor = true;
        EnableEzStateEditor = true;
        EnableGparamEditor = true;
        EnableMaterialEditor = true;
        EnableBehaviorEditor = true;
        EnableTextureEditor = true;
        EnableTimeActEditor = true;

        ActionManager = new ActionManager();
    }

    public async void Initialize()
    {
        TaskLogs.AddLog($"[{ProjectName}] Initializing...");

        ActionManager = new();
        SoapstoneService = new(BaseEditor.Version, this);

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

        // Image Shoebox
        Task<bool> shoeboxTask = SetupShoeboxLayouts();
        bool shoeboxSetup = await shoeboxTask;

        if (aliasesSetup)
        {
            TaskLogs.AddLog($"[{ProjectName}] Setup shoebox layouts.");
        }
        else
        {
            TaskLogs.AddLog($"[{ProjectName}] Failed to setup shoebox layouts.");
        }

        InitializeEditors();

        IsInitializing = false;
        Initialized = true;
    }

    /// <summary>
    /// Called if the project editor settings are changed
    /// </summary>
    public void InitializeEditors()
    {
        // TODO: this should probably dispose of existing stuff, not just null them
        FileBrowser = null;

        // File Browser
        if (EnableFileBrowser && FeatureFlags.EnableFileBrowser)
        {
            FileBrowser = new(0, this);
        }

        // Map Editor
        if (EnableMapEditor && FeatureFlags.EnableMapEditor)
        {
            MapData = new(BaseEditor, this);

            MapEditor = new(0, BaseEditor, this);
        }

        // Model Editor
        if (EnableModelEditor && FeatureFlags.EnableModelEditor)
        {
            ModelData = new(BaseEditor, this);

            ModelEditor = new(0, BaseEditor, this);
        }

        // Param Editor
        if (EnableParamEditor && FeatureFlags.EnableParamEditor)
        {
            ParamData = new(BaseEditor, this);

            ParamEditor = new(0, BaseEditor, this);
        }

        // Text Editor
        if (EnableTextEditor && FeatureFlags.EnableTextEditor)
        {
            TextData = new(BaseEditor, this);

            TextEditor = new(0, BaseEditor, this);
        }

        // Cutscene Editor
        if (EnableCutsceneEditor && FeatureFlags.EnableCutsceneEditor)
        {
            CutsceneData = new(BaseEditor, this);

            CutsceneEditor = new(0, BaseEditor, this);
        }

        // Event Script Editor
        if (EnableEventScriptEditor && FeatureFlags.EnableEventScriptEditor)
        {
            EventScriptData = new(BaseEditor, this);

            EventScriptEditor = new(0, BaseEditor, this);
        }

        // Ez State Editor
        if (EnableEzStateEditor && FeatureFlags.EnableEzStateEditor)
        {
            EzStateData = new(BaseEditor, this);

            EzStateEditor = new(0, BaseEditor, this);
        }

        // Gparam Editor
        if (EnableGparamEditor && FeatureFlags.EnableGparamEditor)
        {
            GparamData = new(BaseEditor, this);

            GparamEditor = new(0, BaseEditor, this);
        }

        // Material Editor
        if (EnableMaterialEditor && FeatureFlags.EnableMaterialEditor)
        {
            MaterialData = new(BaseEditor, this);

            MaterialEditor = new(0, BaseEditor, this);
        }

        // Behavior Editor
        if (EnableBehaviorEditor && FeatureFlags.EnableBehaviorEditor)
        {
            BehaviorData = new(BaseEditor, this);

            BehaviorEditor = new(0, BaseEditor, this);
        }

        // Texture Editor
        if (EnableTextureEditor && FeatureFlags.EnableTextureEditor)
        {
            TextureData = new(BaseEditor, this);

            TextureEditor = new(0, BaseEditor, this);
        }

        // Time Act Editor
        if (EnableTimeActEditor && FeatureFlags.EnableTimeActEditor)
        {
            TimeActData = new(BaseEditor, this);

            TimeActEditor = new(0, BaseEditor, this);
        }
    }

    public void Draw(float dt, string[] cmd)
    {
        ImGui.Begin($"Project##Project", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove);

        uint dockspaceID = ImGui.GetID("ProjectDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        // Only initialize once the project is selected
        // This is so we don't try and initialize all
        // projects in the stored list immediately
        if (IsSelected && !Initialized && !IsInitializing)
        {
            IsInitializing = true;
            Initialize();
        }

        if (Initialized)
        {
            Menubar();

            // File Browser
            if (EnableFileBrowser && FileBrowser != null)
            {
                FileBrowser.OnGUI(dt, cmd);
            }

            // Map Editor
            if (EnableMapEditor && MapEditor != null)
            {
                MapEditor.Display(dt, cmd);
            }

            // Model Editor
            if (EnableModelEditor && ModelEditor != null)
            {
                ModelEditor.Display(dt, cmd);
            }

            // Param Editor
            if (EnableParamEditor && ParamEditor != null)
            {
                ParamEditor.Display(dt, cmd);
            }

            // Text Editor
            if (EnableTextEditor && TextEditor != null)
            {
                TextEditor.Display(dt, cmd);
            }

            // Cutscene Editor
            if (EnableCutsceneEditor && CutsceneEditor != null)
            {
                CutsceneEditor.Display(dt, cmd);
            }

            // Event Script Editor
            if (EnableEventScriptEditor && EventScriptEditor != null)
            {
                EventScriptEditor.Display(dt, cmd);
            }

            // Ez State Editor
            if (EnableEzStateEditor && EzStateEditor != null)
            {
                EzStateEditor.Display(dt, cmd);
            }

            // Gparam Editor
            if (EnableGparamEditor && GparamEditor != null)
            {
                GparamEditor.Display(dt, cmd);
            }

            // Material Editor
            if (EnableMaterialEditor && MaterialEditor != null)
            {
                MaterialEditor.Display(dt, cmd);
            }

            // Behavior Editor
            if (EnableBehaviorEditor && BehaviorEditor != null)
            {
                BehaviorEditor.Display(dt, cmd);
            }

            // Texture Editor
            if (EnableTextureEditor && TextureEditor != null)
            {
                TextureEditor.Display(dt, cmd);
            }

            // Time Act Editor
            if (EnableTimeActEditor && TimeActEditor != null)
            {
                TimeActEditor.Display(dt, cmd);
            }
        }

        ImGui.End();
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu($"File"))
            {
                if (ImGui.MenuItem("Open Project Folder"))
                {
                    Process.Start("explorer.exe", ProjectPath);
                }
                UIHelper.Tooltip("Open the project folder for this project.");

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    public void Save()
    {
        var folder = ProjectUtils.GetProjectFolder();
        var file = Path.Combine(folder, $"{ProjectGUID}.json");

        var json = JsonSerializer.Serialize(this, SmithboxSerializerContext.Default.Project);

        File.WriteAllText(file, json);
    }

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
            case ProjectType.ERN:
                file = "ERN-File-Dictionary.json"; break;
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

        // TODO: add check for project-unique files, and add them to the FileDictionary so we can point to project-unique files

        return true;
    }

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

    public async Task<bool> SetupShoeboxLayouts()
    {
        await Task.Delay(1000);

        ShoeboxLayouts = new(this);

        if (ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu/hi/01_common.sblytbnd.dcx";

            if (FS.FileExists(sourcePath))
            {
                ShoeboxLayouts.LoadLayouts(sourcePath);
                ShoeboxLayouts.BuildTextureDictionary();
            }
            else
            {
                var filename = Path.GetFileNameWithoutExtension(sourcePath);
                TaskLogs.AddLog($"Failed to load Shoebox Layout: {filename} at {sourcePath}");
            }
        }

        return true;
    }

    // Call on old project when the user switches to another project
    // Mainly to dispose of viewport stuff
    public void Close()
    {
        // Map Editor
        MapEditor.Universe.UnloadAll();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

