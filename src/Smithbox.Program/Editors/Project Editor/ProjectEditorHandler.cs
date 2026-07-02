using Microsoft.Extensions.Logging;
using StudioCore.Editors.AnimEditor;
using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapDataEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectEditorHandler : IDisposable
{
    public ProjectEntry Project;

    public EditorScreen FocusedEditor;

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

    // Animation Editor
    public AnimData AnimData;
    public AnimEditorScreen AnimEditor;
    public AnimEditorStub AnimEditorStub;

    // Map Param Editor
    public MapDataHandler MapDataHandler;
    public MapDataEditorScreen MapDataEditor;
    public MapDataEditorStub MapDataEditorStub;


    // Data tasks
    private Task<bool> _projectDataTask;
    private Task<bool> _mapDataTask;
    private Task<bool> _modelDataTask;
    private Task<bool> _paramDataTask;
    private Task<bool> _textDataTask;
    private Task<bool> _gparamDataTask;
    private Task<bool> _materialDataTask;
    private Task<bool> _textureDataTask;
    private Task<bool> _animDataTask;
    private Task<bool> _mapDataDataTask;

    public ProjectEditorHandler(ProjectEntry project)
    {
        Project = project;
    }

    public void InitStubs()
    {
        MapEditorStub = new(Project);
        ModelEditorStub = new(Project);
        TextEditorStub = new(Project);
        ParamEditorStub = new(Project);
        GparamEditorStub = new(Project);
        MaterialEditorStub = new(Project);
        TextureViewerStub = new(Project);
        FileBrowserStub = new(Project);
        AnimEditorStub = new(Project);
        MapDataEditorStub = new(Project);
    }

    public async Task<bool> InitializeData(ProjectInitType initType, bool silent)
    {
        var tasks = new List<Task<bool>>();

        // Project data
        ProjectData = new(Project);
        _projectDataTask = ProjectData.Setup();
        tasks.Add(_projectDataTask);

        // Texture data 
        TextureData = new(Project);
        _textureDataTask = TextureData.Setup();
        tasks.Add(_textureDataTask);

        // Material data (shared dependency)
        _materialDataTask = Task.Run(async () =>
        {
            MaterialData = new(Project);
            return await MaterialData.Setup();
        });

        // Map
        if (Project.Descriptor.EnableMapEditor &&
            initType is ProjectInitType.ProjectDefined or ProjectInitType.MapEditorOnly)
        {
            MapData = new(Project);
            _mapDataTask = MapData.Setup();
            tasks.Add(_mapDataTask);
            tasks.Add(_materialDataTask);
        }

        // Model
        if (Project.Descriptor.EnableModelEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            ModelData = new(Project);
            _modelDataTask = ModelData.Setup();
            tasks.Add(_modelDataTask);
            tasks.Add(_materialDataTask);
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

        // Animation
        if (Project.Descriptor.EnableAnimEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            AnimData = new(Project);
            _animDataTask = AnimData.Setup();
            tasks.Add(_animDataTask);
        }

        // Map Data
        if (Project.Descriptor.EnableMapDataEditor &&
            initType is ProjectInitType.ProjectDefined)
        {
            MapDataHandler = new(Project);
            _mapDataDataTask = MapDataHandler.Setup();
            tasks.Add(_mapDataDataTask);
        }

        bool[] results = await Task.WhenAll(tasks);

        if (!silent)
            LogDataResults();

        return results.All(r => r);
    }

    public void LogDataResults()
    {
        if (_projectDataTask?.Result == true)
            Smithbox.Log(this, LOC.Get("PROJECT_Data_Setup_Project_Data_PASS"));
        else
            Smithbox.LogError(this, LOC.Get("PROJECT_Data_Setup_Project_Data_FAIL"));

        if (_mapDataTask != null)
            Smithbox.Log(this, _mapDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Map_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Map_Data_FAIL"));

        if (_modelDataTask != null)
            Smithbox.Log(this, _modelDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Model_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Model_Data_FAIL"));

        if (_materialDataTask != null)
            Smithbox.Log(this, _materialDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Material_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Material_Data_FAIL"));

        if (_paramDataTask != null)
            Smithbox.Log(this, _paramDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Param_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Param_Data_FAIL"));

        if (_textDataTask != null)
            Smithbox.Log(this, _textDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Text_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Text_Data_FAIL"));

        if (_gparamDataTask != null)
            Smithbox.Log(this, _gparamDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Gparam_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Gparam_Data_FAIL"));

        if (_textureDataTask != null)
            Smithbox.Log(this, _textureDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Texture_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Texture_Data_FAIL"));

        if (_animDataTask != null)
            Smithbox.Log(this, _animDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Anim_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Anim_Data_FAIL"));

        if (_mapDataDataTask != null)
            Smithbox.Log(this, _mapDataDataTask.Result
                ? LOC.Get("PROJECT_Data_Setup_Map_Data_PASS")
                : LOC.Get("PROJECT_Data_Setup_Map_Data_FAIL"));
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
            Project.Descriptor.EnableMaterialEditor)
        {
            MaterialEditor = new MaterialEditorScreen(Project);
            firstEditor ??= MaterialEditor;
        }

        if (TextureData != null)
        {
            TextureViewer = new TextureViewerScreen(Project);
            firstEditor ??= TextureViewer;
        }

        if (AnimData != null)
        {
            AnimEditor = new AnimEditorScreen(Project);
            firstEditor ??= AnimEditor;
        }

        if (MapDataHandler != null)
        {
            MapDataEditor = new MapDataEditorScreen(Project);
            firstEditor ??= MapDataEditor;
        }

        FocusedEditor = firstEditor;
    }

    #region Dispose

    public void Dispose()
    {
        ProjectData?.Dispose();

        MapData?.Dispose();
        ModelData?.Dispose();
        ParamData?.Dispose();
        TextData?.Dispose();
        GparamData?.Dispose();
        MaterialData?.Dispose();
        TextureData?.Dispose();
        MapDataHandler?.Dispose();
    }
    #endregion
}
