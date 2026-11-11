using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Application;

public class ProjectEditorHandler : IDisposable
{
    public ProjectEntry Project;

    public EditorScreen FocusedEditor;

    // Common Data
    public CommonData CommonData;

    // Project Data
    public ProjectData ProjectData;
    public TextureData TextureData;

    // File Browser
    public FileBrowserScreen FileBrowser;
    public FileBrowserStub FileBrowserStub;

    // Map Editor
    public MapData MapData;
    public MapEditorScreen MapEditor;
    public MapEditorStub MapEditorStub;

    // Model Editor
    public ModelData ModelData;
    public ModelEditorScreen ModelEditor;
    public ModelEditorStub ModelEditorStub;

    // TimeAct Editor
    public TimeActData TimeActData;
    public TimeActEditorScreen TimeActEditor;
    public TimeActEditorStub TimeActEditorStub;

    // Param Editor
    public ParamData ParamData;
    public ParamEditorScreen ParamEditor;
    public ParamEditorStub ParamEditorStub;

    // Text Editor
    public TextData TextData;
    public TextEditorScreen TextEditor;
    public TextEditorStub TextEditorStub;

    // Graphics Param Editor
    public GparamData GparamData;
    public GparamEditorScreen GparamEditor;
    public GparamEditorStub GparamEditorStub;

    // Material Editor
    public MaterialData MaterialData;
    public MaterialEditorScreen MaterialEditor;
    public MaterialEditorStub MaterialEditorStub;

    // Texture Viewer
    public TextureViewerScreen TextureViewer;
    public TextureViewerStub TextureViewerStub;

    // Map Param Editor

    // Havok Editor
    public HavokData HavokData;
    public HavokEditorScreen HavokEditor;
    public HavokEditorStub HavokEditorStub;

    // FX Editor
    public FxData FxData;
    public FxEditorScreen FxEditor;
    public FxEditorStub FxEditorStub;

    // Behavior Editor (AC6 .bhv)
    public BhvData BehaviorData;
    public BehaviorEditorScreen BehaviorEditor;
    public BehaviorEditorStub BehaviorEditorStub;

    // EMEVD Editor (event scripts)
    public EmevdData EmevdData;
    public EmevdEditorScreen EmevdEditor;
    public EmevdEditorStub EmevdEditorStub;

    // ESD Editor (EzState machines)
    public EsdData EsdData;
    public EsdEditorScreen EsdEditor;
    public EsdEditorStub EsdEditorStub;


    // The lighting the Model and Time Act Editors' viewports borrow from a map, made when a viewport first asks
    private ViewportEnvironment _viewportEnvironment;

    public ViewportEnvironment ViewportEnvironment => _viewportEnvironment ??= new ViewportEnvironment(Project);

    /// <summary>The viewport environment if one has been made, for clearing its caches without making one</summary>
    public ViewportEnvironment ExistingViewportEnvironment => _viewportEnvironment;

    // Data tasks
    private Task<bool> _commonDataTask;
    private Task<bool> _projectDataTask;
    private Task<bool> _mapDataTask;
    private Task<bool> _modelDataTask;
    private Task<bool> _paramDataTask;
    private Task<bool> _textDataTask;
    private Task<bool> _gparamDataTask;
    private Task<bool> _materialDataTask;
    private Task<bool> _textureDataTask;
    private Task<bool> _havokDataTask;
    private Task<bool> _fxDataTask;
    private Task<bool> _behaviorDataTask;
    private Task<bool> _emevdDataTask;
    private Task<bool> _esdDataTask;
    private Task<bool> _timeActDataTask;

    public ProjectEditorHandler(ProjectEntry project)
    {
        Project = project;
    }

    public void InitStubs()
    {
        MapEditorStub = new(Project);
        ModelEditorStub = new(Project);
        TimeActEditorStub = new(Project);
        TextEditorStub = new(Project);
        ParamEditorStub = new(Project);
        GparamEditorStub = new(Project);
        MaterialEditorStub = new(Project);
        TextureViewerStub = new(Project);
        FileBrowserStub = new(Project);
        HavokEditorStub = new(Project);
        FxEditorStub = new(Project);
        BehaviorEditorStub = new(Project);
        EmevdEditorStub = new(Project);
        EsdEditorStub = new(Project);
    }

    public async Task<bool> InitializeData(ProjectInitType initType, bool silent)
    {
        var tasks = new List<Task<bool>>();

        // Project data
        CommonData = new(Project);
        _commonDataTask = CommonData.Setup();
        tasks.Add(_commonDataTask);

        // Project data
        ProjectData = new(Project);
        _projectDataTask = ProjectData.Setup();
        tasks.Add(_projectDataTask);

        // Texture data 
        TextureData = new(Project);
        _textureDataTask = TextureData.Setup();
        tasks.Add(_textureDataTask);

        // Material data (shared dependency of the map, model and time act viewports, and the Material Editor).
        // Only started when something uses it, and always awaited: InitializeEditors reads MaterialData, which an
        // unawaited task might not have assigned yet.
        var needsMaterialData =
            initType is ProjectInitType.ProjectDefined ||
            (initType is ProjectInitType.MapEditorOnly && Project.Descriptor.EnableMapEditor);

        if (needsMaterialData)
        {
            _materialDataTask = Task.Run(async () =>
            {
                MaterialData = new(Project);
                return await MaterialData.Setup();
            });

            tasks.Add(_materialDataTask);
        }

        // Map
        if (Project.Descriptor.EnableMapEditor &&
            initType is ProjectInitType.ProjectDefined or ProjectInitType.MapEditorOnly)
        {
            MapData = new(Project);
            _mapDataTask = MapData.Setup();
            tasks.Add(_mapDataTask);
        }

        // Model
        if (Project.Descriptor.EnableModelEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            ModelData = new(Project);
            _modelDataTask = ModelData.Setup();
            tasks.Add(_modelDataTask);
        }

        // Time Act
        if (Project.Descriptor.EnableTimeActEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            TimeActData = new(Project);
            _timeActDataTask = TimeActData.Setup();
            tasks.Add(_timeActDataTask);
        }

        // Param
        if (Project.Descriptor.EnableParamEditor &&
            initType is ProjectInitType.ProjectDefined or ProjectInitType.ParamEditorOnly)
        {
            ParamData = new(Project);
            _paramDataTask = ParamData.Setup();
            tasks.Add(_paramDataTask);
        }

        // Text
        if (Project.Descriptor.EnableTextEditor &&
            initType is ProjectInitType.ProjectDefined or ProjectInitType.TextEditorOnly)
        {
            TextData = new(Project);
            _textDataTask = TextData.Setup();
            tasks.Add(_textDataTask);
        }

        // Gparam
        if (Project.Descriptor.EnableGparamEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            GparamData = new(Project);
            _gparamDataTask = GparamData.Setup();
            tasks.Add(_gparamDataTask);
        }

        // Havok Data
        if (Project.Descriptor.EnableHavokEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            HavokData = new(Project);
            _havokDataTask = HavokData.Setup();
            tasks.Add(_havokDataTask);
        }

        // FX
        if (Project.Descriptor.EnableFxEditor &&
            EditorConditions.SupportsFxEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            FxData = new(Project);
            _fxDataTask = FxData.Setup();
            tasks.Add(_fxDataTask);
        }

        // Behavior (AC6 .bhv)
        if (Project.Descriptor.EnableBehaviorEditor &&
            EditorConditions.SupportsBehaviorEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            BehaviorData = new(Project);
            _behaviorDataTask = BehaviorData.Setup();
            tasks.Add(_behaviorDataTask);
        }

        // EMEVD (event scripts)
        if (Project.Descriptor.EnableEmevdEditor &&
            EditorConditions.SupportsEmevdEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            EmevdData = new(Project);
            _emevdDataTask = EmevdData.Setup();
            tasks.Add(_emevdDataTask);
        }

        // ESD (EzState machines)
        if (Project.Descriptor.EnableEsdEditor &&
            EditorConditions.SupportsEsdEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            EsdData = new(Project);
            _esdDataTask = EsdData.Setup();
            tasks.Add(_esdDataTask);
        }

        bool[] results = await Task.WhenAll(tasks);

        if (!silent)
            LogDataResults();

        return results.All(r => r);
    }

    public void LogDataResults()
    {
        LogDataResult(_commonDataTask, "PROJECT_Data_Setup_Common_Data_PASS", "PROJECT_Data_Setup_Common_Data_FAIL");
        LogDataResult(_projectDataTask, "PROJECT_Data_Setup_Project_Data_PASS", "PROJECT_Data_Setup_Project_Data_FAIL");
        LogDataResult(_mapDataTask, "PROJECT_Data_Setup_Map_Data_PASS", "PROJECT_Data_Setup_Map_Data_FAIL");
        LogDataResult(_modelDataTask, "PROJECT_Data_Setup_Model_Data_PASS", "PROJECT_Data_Setup_Model_Data_FAIL");
        LogDataResult(_timeActDataTask, "PROJECT_Data_Setup_TimeAct_Data_PASS", "PROJECT_Data_Setup_TimeAct_Data_FAIL");
        LogDataResult(_materialDataTask, "PROJECT_Data_Setup_Material_Data_PASS", "PROJECT_Data_Setup_Material_Data_FAIL");
        LogDataResult(_paramDataTask, "PROJECT_Data_Setup_Param_Data_PASS", "PROJECT_Data_Setup_Param_Data_FAIL");
        LogDataResult(_textDataTask, "PROJECT_Data_Setup_Text_Data_PASS", "PROJECT_Data_Setup_Text_Data_FAIL");
        LogDataResult(_gparamDataTask, "PROJECT_Data_Setup_Gparam_Data_PASS", "PROJECT_Data_Setup_Gparam_Data_FAIL");
        LogDataResult(_textureDataTask, "PROJECT_Data_Setup_Texture_Data_PASS", "PROJECT_Data_Setup_Texture_Data_FAIL");
        LogDataResult(_havokDataTask, "PROJECT_Data_Setup_Havok_Data_PASS", "PROJECT_Data_Setup_Havok_Data_FAIL");
        LogDataResult(_fxDataTask, "PROJECT_Data_Setup_Fx_Data_PASS", "PROJECT_Data_Setup_Fx_Data_FAIL");
        LogDataResult(_behaviorDataTask, "PROJECT_Data_Setup_Behavior_Data_PASS", "PROJECT_Data_Setup_Behavior_Data_FAIL");
        LogDataResult(_emevdDataTask, "PROJECT_Data_Setup_Emevd_Data_PASS", "PROJECT_Data_Setup_Emevd_Data_FAIL");
        LogDataResult(_esdDataTask, "PROJECT_Data_Setup_Esd_Data_PASS", "PROJECT_Data_Setup_Esd_Data_FAIL");
    }

    /// <summary>
    /// Logs a data task's outcome, as an error if it failed. Tasks that weren't started are skipped.
    /// </summary>
    private void LogDataResult(Task<bool> task, string passKey, string failKey)
    {
        if (task == null)
            return;

        if (task.IsCompletedSuccessfully && task.Result)
            Smithbox.Log(this, LOC.Get(passKey));
        else
            Smithbox.LogError(this, LOC.Get(failKey));
    }

    public void InitializeEditors(ProjectInitType initType)
    {
        EditorScreen firstEditor = null;

        if (Project.Descriptor.EnableFileBrowser &&
            initType is ProjectInitType.ProjectDefined)
        {
            FileBrowser = new FileBrowserScreen(Project);
            firstEditor ??= FileBrowser;
        }

        if (MapData != null)
        {
            MapEditor = new MapEditorScreen(Project);
            firstEditor ??= MapEditor;
        }

        if (ModelData != null)
        {
            ModelEditor = new ModelEditorScreen(Project);
            firstEditor ??= ModelEditor;
        }

        if (TimeActData != null)
        {
            TimeActEditor = new TimeActEditorScreen(Project);
            firstEditor ??= TimeActEditor;
        }

        if (ParamData != null)
        {
            ParamEditor = new ParamEditorScreen(Project);
            firstEditor ??= ParamEditor;
        }

        if (TextData != null)
        {
            TextEditor = new TextEditorScreen(Project);
            firstEditor ??= TextEditor;
        }

        if (GparamData != null)
        {
            GparamEditor = new GparamEditorScreen(Project);
            firstEditor ??= GparamEditor;
        }

        if (MaterialData != null &&
            Project.Descriptor.EnableMaterialEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            MaterialEditor = new MaterialEditorScreen(Project);
            firstEditor ??= MaterialEditor;
        }

        if (TextureData != null)
        {
            TextureViewer = new TextureViewerScreen(Project);
            firstEditor ??= TextureViewer;
        }

        if (HavokData != null)
        {
            HavokEditor = new HavokEditorScreen(Project);
            firstEditor ??= HavokEditor;
        }

        if (FxData != null)
        {
            FxEditor = new FxEditorScreen(Project);
            firstEditor ??= FxEditor;
        }

        if (BehaviorData != null)
        {
            BehaviorEditor = new BehaviorEditorScreen(Project);
            firstEditor ??= BehaviorEditor;
        }

        if (EmevdData != null)
        {
            EmevdEditor = new EmevdEditorScreen(Project);
            firstEditor ??= EmevdEditor;
        }

        if (EsdData != null)
        {
            EsdEditor = new EsdEditorScreen(Project);
            firstEditor ??= EsdEditor;
        }

        FocusedEditor = firstEditor;
    }

    /// <summary>
    /// Brings an editor's window to the front.
    /// </summary>
    /// <returns>Whether the editor has a window to focus.</returns>
    public bool FocusEditorWindow(EditorScreen editor)
    {
        // The window names the editor stubs draw their windows with
        var windowName = editor switch
        {
            MapEditorScreen => "###mapEditor",
            ModelEditorScreen => "###modelEditor",
            TimeActEditorScreen => "###timeActEditor",
            ParamEditorScreen => "###paramEditor",
            TextEditorScreen => "###textEditorStub",
            GparamEditorScreen => $"{LOC.Get("GPARAM_Editor_Stub")}##gparamEditorStub",
            MaterialEditorScreen => "###materialEditor",
            TextureViewerScreen => "###textureViewerStub",
            FileBrowserScreen => "###fileBrowserStub",
            HavokEditorScreen => "###havokEditor",
            FxEditorScreen => "###fxEditor",
            BehaviorEditorScreen => "###behaviorEditor",
            EmevdEditorScreen => "###emevdEditor",
            EsdEditorScreen => "###esdEditor",
            _ => null
        };

        if (windowName == null)
            return false;

        Hexa.NET.ImGui.ImGui.SetWindowFocus(windowName);

        return true;
    }

    #region Dispose

    public void Dispose()
    {
        ModelPreviewCache.Release(Project);

        CommonData?.Dispose();
        ProjectData?.Dispose();

        MapData?.Dispose();
        ModelData?.Dispose();
        TimeActData?.Dispose();
        ParamData?.Dispose();
        TextData?.Dispose();
        GparamData?.Dispose();
        MaterialData?.Dispose();
        TextureData?.Dispose();
        HavokData?.Dispose();
        FxData?.Dispose();
        BehaviorData?.Dispose();
        EmevdData?.Dispose();
        EsdData?.Dispose();

        // The FX Editor's debug shapes hold GPU buffers
        FxEditor?.ViewportWindow?.Dispose();

        _viewportEnvironment?.Dispose();
        _viewportEnvironment = null;
    }
    #endregion
}
