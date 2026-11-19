using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.LuaEditor;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.SoundEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Memory;
using StudioCore.Renderer;

namespace StudioCore.Application;

public class ProjectEditorHandler : IDisposable
{
    public ProjectEntry Project;

    public EditorScreen FocusedEditor;

    // Common Data
    public CommonData CommonData;

    // The connection to the running game, shared by the live tools (Material Reloader, World Reloader)
    private GameLink _gameLink;
    public GameLink GameLink => _gameLink ??= new GameLink(Project);

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

    // Cutscene Editor
    public CutsceneData CutsceneData;
    public CutsceneEditorScreen CutsceneEditor;
    public CutsceneEditorStub CutsceneEditorStub;

    // Projectile Editor (reads and edits the Param Editor's params)
    public ProjectileEditorScreen ProjectileEditor;
    public ProjectileEditorStub ProjectileEditorStub;

    // FaceGen Editor (reads and edits the Param Editor's face params)
    public StudioCore.Editors.FaceGenEditor.FaceGenEditorScreen FaceGenEditor;
    public StudioCore.Editors.FaceGenEditor.FaceGenEditorStub FaceGenEditorStub;

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

    // Lua Editor (AI scripts in .luabnd)
    public LuaData LuaData;
    public LuaEditorScreen LuaEditor;
    public LuaEditorStub LuaEditorStub;

    // Sound Editor (FMOD / Wwise)
    public SoundData SoundData;
    public SoundEditorScreen SoundEditor;
    public SoundEditorStub SoundEditorStub;

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
    private Task<bool> _cutsceneDataTask;
    private Task<bool> _behaviorDataTask;
    private Task<bool> _emevdDataTask;
    private Task<bool> _esdDataTask;
    private Task<bool> _luaDataTask;
    private Task<bool> _soundDataTask;
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
        CutsceneEditorStub = new(Project);
        ProjectileEditorStub = new(Project);
        FaceGenEditorStub = new(Project);
        BehaviorEditorStub = new(Project);
        EmevdEditorStub = new(Project);
        EsdEditorStub = new(Project);
        LuaEditorStub = new(Project);
        SoundEditorStub = new(Project);
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
            // Auxiliary projects (TextEditorOnly) skip the background load of the other languages
            _textDataTask = TextData.Setup(preloadOtherLanguages: initType is ProjectInitType.ProjectDefined);
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

        // Cutscenes (MQB)
        if (Project.Descriptor.EnableCutsceneEditor &&
            EditorConditions.SupportsCutsceneEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            CutsceneData = new(Project);
            _cutsceneDataTask = CutsceneData.Setup();
            tasks.Add(_cutsceneDataTask);
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

        // Lua (AI scripts in .luabnd)
        if (Project.Descriptor.EnableLuaEditor &&
            EditorConditions.SupportsLuaEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            LuaData = new(Project);
            _luaDataTask = LuaData.Setup();
            tasks.Add(_luaDataTask);
        }

        // Sound (FMOD / Wwise)
        if (Project.Descriptor.EnableSoundEditor &&
            EditorConditions.SupportsSoundEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            SoundData = new(Project);
            _soundDataTask = SoundData.Setup();
            tasks.Add(_soundDataTask);
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
        LogDataResult(_cutsceneDataTask, "PROJECT_Data_Setup_Cutscene_Data_PASS", "PROJECT_Data_Setup_Cutscene_Data_FAIL");
        LogDataResult(_behaviorDataTask, "PROJECT_Data_Setup_Behavior_Data_PASS", "PROJECT_Data_Setup_Behavior_Data_FAIL");
        LogDataResult(_emevdDataTask, "PROJECT_Data_Setup_Emevd_Data_PASS", "PROJECT_Data_Setup_Emevd_Data_FAIL");
        LogDataResult(_esdDataTask, "PROJECT_Data_Setup_Esd_Data_PASS", "PROJECT_Data_Setup_Esd_Data_FAIL");
        LogDataResult(_luaDataTask, "PROJECT_Data_Setup_Lua_Data_PASS", "PROJECT_Data_Setup_Lua_Data_FAIL");
        LogDataResult(_soundDataTask, "PROJECT_Data_Setup_Sound_Data_PASS", "PROJECT_Data_Setup_Sound_Data_FAIL");
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

        // No data of its own: the Param Editor's params, so it is made with the Param Editor
        if (ParamData != null && ParamEditor != null &&
            Project.Descriptor.EnableProjectileEditor &&
            EditorConditions.SupportsProjectileEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            ProjectileEditor = new ProjectileEditorScreen(Project);
            firstEditor ??= ProjectileEditor;
        }

        // The same: the Param Editor's face params
        if (ParamData != null && ParamEditor != null &&
            Project.Descriptor.EnableFaceGenEditor &&
            EditorConditions.SupportsFaceGenEditor(Project.Descriptor.ProjectType) &&
            initType is ProjectInitType.ProjectDefined)
        {
            FaceGenEditor = new StudioCore.Editors.FaceGenEditor.FaceGenEditorScreen(Project);
            firstEditor ??= FaceGenEditor;
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

        if (CutsceneData != null)
        {
            CutsceneEditor = new CutsceneEditorScreen(Project);
            firstEditor ??= CutsceneEditor;
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

        if (LuaData != null)
        {
            LuaEditor = new LuaEditorScreen(Project);
            firstEditor ??= LuaEditor;
        }

        if (SoundData != null)
        {
            SoundEditor = new SoundEditorScreen(Project);
            firstEditor ??= SoundEditor;
        }

        FocusedEditor = firstEditor;
    }

    /// <summary>
    /// Makes an editor the one the editor host shows.
    /// </summary>
    /// <returns>Whether the editor is one of this project's top-level editors.</returns>
    public bool FocusEditorWindow(EditorScreen editor)
    {
        var entry = EditorRegistry.FindByScreen(this, editor);

        if (entry == null)
            return false;

        return Smithbox.Orchestrator.ActivateEditor(entry.Id);
    }

    #region Dispose

    public void Dispose()
    {
        // The editors' viewports first: their render pipelines' queues are registered with the global renderer, so
        // they would keep running (and their GPU targets stay allocated) after the project is gone
        MapEditor?.ViewHandler?.DisposeViews();
        ModelEditor?.ViewHandler?.DisposeViews();
        TimeActEditor?.ViewHandler?.DisposeViews();
        GparamEditor?.ViewHandler?.DisposeViews();
        LuaEditor?.Viewer?.Dispose();

        ModelPreviewCache.Release(Project);

        _gameLink?.Detach();
        _gameLink = null;

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
        CutsceneData?.Dispose();
        BehaviorData?.Dispose();
        EmevdData?.Dispose();
        EsdData?.Dispose();
        LuaData?.Dispose();

        // The Sound Editor's player holds an SDL audio device and maybe a decoder process
        SoundEditor?.Dispose();
        SoundData?.Dispose();

        // The FX Editor's debug shapes hold GPU buffers, and its viewport a render pipeline
        FxEditor?.Dispose();

        // The Cutscene Editor's actors hold meshes and animators, its lines GPU buffers, its viewport a render pipeline
        CutsceneEditor?.Dispose();

        // The Projectile Editor's lines and effects hold GPU buffers, its viewport a render pipeline
        ProjectileEditor?.Dispose();

        // The FaceGen Editor's character mesh, and its viewport's render pipeline
        FaceGenEditor?.Dispose();

        _viewportEnvironment?.Dispose();
        _viewportEnvironment = null;
    }
    #endregion
}
