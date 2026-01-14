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
    public TextureData TextureData;
    public TextureViewerScreen TextureViewer;
    public TextureViewerStub TextureViewerStub;

    // Data tasks
    private Task<bool> _projectDataTask;
    private Task<bool> _mapDataTask;
    private Task<bool> _modelDataTask;
    private Task<bool> _paramDataTask;
    private Task<bool> _textDataTask;
    private Task<bool> _gparamDataTask;
    private Task<bool> _materialDataTask;
    private Task<bool> _textureDataTask;

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
    }

    public async Task<bool> InitializeData(ProjectInitType initType, bool silent)
    {
        var tasks = new List<Task<bool>>();

        // Project data (always required)
        ProjectData = new(Project);
        _projectDataTask = ProjectData.Setup();
        tasks.Add(_projectDataTask);

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

        // Texture
        if (Project.Descriptor.EnableTextureViewer &&
            initType is ProjectInitType.ProjectDefined)
        {
            TextureData = new(Project);
            _textureDataTask = TextureData.Setup();
            tasks.Add(_textureDataTask);
        }

        bool[] results = await Task.WhenAll(tasks);

        if (!silent)
            LogDataResults();

        return results.All(r => r);
    }

    public void LogDataResults()
    {
        if (_projectDataTask?.Result == true)
            TaskLogs.AddLog("Setup Project Data.");
        else
            TaskLogs.AddError("Failed to setup Project Data.");

        if (_mapDataTask != null)
            TaskLogs.AddLog(_mapDataTask.Result
                ? "[Map Editor] Setup Map Data Banks."
                : "[Map Editor] Failed to setup Map Data Banks.");

        if (_modelDataTask != null)
            TaskLogs.AddLog(_modelDataTask.Result
                ? "[Model Editor] Setup Model Data Banks."
                : "[Model Editor] Failed to setup Model Data Banks.");

        if (_materialDataTask != null)
            TaskLogs.AddLog(_materialDataTask.Result
                ? "[Material Data] Setup Material Data."
                : "[Material Data] Failed to setup Material Data.");

        if (_paramDataTask != null)
            TaskLogs.AddLog(_paramDataTask.Result
                ? "[Param Editor] Setup PARAM Banks."
                : "[Param Editor] Failed to setup PARAM Banks.");

        if (_textDataTask != null)
            TaskLogs.AddLog(_textDataTask.Result
                ? "[Text Editor] Setup FMG Banks."
                : "[Text Editor] Failed to setup FMG Banks.");

        if (_gparamDataTask != null)
            TaskLogs.AddLog(_gparamDataTask.Result
                ? "[Graphics Param Editor] Setup GPARAM Banks."
                : "[Graphics Param Editor] Failed to setup GPARAM Banks.");

        if (_textureDataTask != null)
            TaskLogs.AddLog(_textureDataTask.Result
                ? "[Texture Viewer] Setup texture bank."
                : "[Texture Viewer] Failed to setup texture bank.");
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
            ParamEditor.MassEditHandler.Setup();
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
    }
    #endregion
}
