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

    public async Task<bool> Initialize(ProjectInitType initType, bool silent)
    {
        ProjectData = new(Project);

        // Project Data
        Task<bool> commonDataTask = ProjectData.Setup();
        bool commonDataResult = await commonDataTask;

        if (!silent)
        {
            if (commonDataResult)
            {
                TaskLogs.AddLog($"Setup Project Data.");
            }
            else
            {
                TaskLogs.AddLog($"Failed to setup Project Data.");
            }
        }

        EditorScreen firstEditor = null;

        // File Browser
        if (Project.Descriptor.EnableFileBrowser
            && initType is ProjectInitType.ProjectDefined
            && ProjectUtils.SupportsFileBrowser(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = FileBrowser;

            await InitializeFileBrowser(silent);
        }

        // Map Editor
        if (Project.Descriptor.EnableMapEditor
            && initType is ProjectInitType.ProjectDefined or ProjectInitType.MapEditorOnly
            && ProjectUtils.SupportsModelEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = MapEditor;

            await InitializeMapEditor(silent);
        }

        // Model Editor
        if (Project.Descriptor.EnableModelEditor
            && initType is ProjectInitType.ProjectDefined
            && ProjectUtils.SupportsModelEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = ModelEditor;

            await InitializeModelEditor(silent);
        }

        // Param Editor
        if (Project.Descriptor.EnableParamEditor
            && initType is ProjectInitType.ProjectDefined or ProjectInitType.ParamEditorOnly
            && ProjectUtils.SupportsParamEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = ParamEditor;

            await InitializeParamEditor(silent);
        }

        // Text Editor
        if (Project.Descriptor.EnableTextEditor
            && initType is ProjectInitType.ProjectDefined or ProjectInitType.TextEditorOnly
            && ProjectUtils.SupportsTextEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = TextEditor;

            await InitializeTextEditor(silent);
        }

        // Graphics Param Editor
        if (Project.Descriptor.EnableGparamEditor
            && initType is ProjectInitType.ProjectDefined
            && ProjectUtils.SupportsGraphicsParamEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = GparamEditor;

            await InitializeGparamEditor(silent);
        }

        // Material Editor
        if (Project.Descriptor.EnableMaterialEditor
            && initType is ProjectInitType.ProjectDefined
            && ProjectUtils.SupportsMaterialEditor(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = MaterialEditor;

            await InitializeMaterialEditor(silent);
        }

        // Texture Viewer
        if (Project.Descriptor.EnableTextureViewer
            && initType is ProjectInitType.ProjectDefined
            && ProjectUtils.SupportsTextureViewer(Project.Descriptor.ProjectType))
        {
            if (firstEditor == null)
                firstEditor = TextureViewer;

            await InitializeTextureViewer(silent);
        }

        FocusedEditor = firstEditor;

        return true;
    }
    private async Task<bool> InitializeFileBrowser(bool silent)
    {
        await Task.Yield();

        try
        {
            FileBrowser = new FileBrowserScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"[File Browser] Failed to setup File Browser: {ex}");
            return false;
        }

        return true;
    }


    private async Task<bool> InitializeMapEditor(bool silent)
    {
        MapData = new(Project);

        // Map Data
        Task<bool> mapDataTask = MapData.Setup();
        bool mapDataTaskResult = await mapDataTask;

        if (!silent)
        {
            if (mapDataTaskResult)
            {
                TaskLogs.AddLog($"[Map Editor] Setup Map Data Banks.");
            }
            else
            {
                TaskLogs.AddError($"[Map Editor] Failed to setup Map Data Banks.");
            }
        }

        // Only do this once, as 3 editors may invoke this.
        if (MaterialData == null)
        {
            MaterialData = new(Project);

            Task<bool> materialDataTask = MaterialData.Setup();
            bool materialDataTaskResult = await materialDataTask;

            if (!silent)
            {
                if (materialDataTaskResult)
                {
                    TaskLogs.AddLog($"[Map Editor] Setup Material Data.");
                }
                else
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to setup Material Data.");
                }
            }
        }

        try
        {
            MapEditor = new MapEditorScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Map Editor] Failed to setup Map Editor", ex);
            return false;
        }

        return true;
    }

    private async Task<bool> InitializeModelEditor(bool silent)
    {
        ModelData = new(Project);

        // Model Data
        Task<bool> modelDataTask = ModelData.Setup();
        bool modelDataTaskResult = await modelDataTask;

        if (!silent)
        {
            if (modelDataTaskResult)
            {
                TaskLogs.AddLog($"[Model Editor] Setup Model Data Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[Model Editor] Failed to setup Model Data Banks.");
            }
        }

        // Only do this once, as 3 editors may invoke this.
        if (MaterialData == null)
        {
            MaterialData = new(Project);

            Task<bool> materialDataTask = MaterialData.Setup();
            bool materialDataTaskResult = await materialDataTask;

            if (!silent)
            {
                if (materialDataTaskResult)
                {
                    TaskLogs.AddLog($"[Model Editor] Setup Material Data.");
                }
                else
                {
                    TaskLogs.AddError($"[Model Editor] Failed to setup Material Data.");
                }
            }
        }

        try
        {
            ModelEditor = new ModelEditorScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Model Editor] Failed to setup Model Editor", ex);
            return false;
        }

        return true;
    }

    private async Task<bool> InitializeParamEditor(bool silent)
    {
        ParamData = new(Project);

        // Param Banks
        Task<bool> paramBankTask = ParamData.Setup();
        bool paramBankTaskResult = await paramBankTask;

        if (!silent)
        {
            if (paramBankTaskResult)
            {
                TaskLogs.AddLog($"[Param Editor] Setup PARAM Banks.");
            }
            else
            {
                TaskLogs.AddError($"[Param Editor] Failed to setup PARAM Banks.");
            }
        }

        // Added this so throws during init can be logged
        try
        {
            ParamEditor = new ParamEditorScreen(Project);

            // Placed here so the mass edit stuff is initialized once the editor is setup fully
            if (ParamEditor != null)
            {
                ParamEditor.MassEditHandler.Setup();
            }
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup Param Editor", ex);
            return false;
        }

        return true;
    }

    private async Task<bool> InitializeTextEditor(bool silent)
    {
        TextData = new(Project);

        // Text Banks
        Task<bool> textBankTask = TextData.Setup();
        bool textBankTaskResult = await textBankTask;

        if (!silent)
        {
            if (textBankTaskResult)
            {
                TaskLogs.AddLog($"[Text Editor] Setup FMG Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[Text Editor] Failed to setup FMG Banks.");
            }
        }

        try
        {
            TextEditor = new TextEditorScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Text Editor] Failed to setup Text Editor", ex);
            return false;
        }

        return true;
    }

    private async Task<bool> InitializeGparamEditor(bool silent)
    {
        GparamData = new(Project);

        // Gparam Bank
        Task<bool> gparamBankTask = GparamData.Setup();
        bool gparamBankTaskResult = await gparamBankTask;

        if (!silent)
        {
            if (gparamBankTaskResult)
            {
                TaskLogs.AddLog($"[Graphics Param Editor] Setup GPARAM Banks.");
            }
            else
            {
                TaskLogs.AddLog($"[Graphics Param Editor] Failed to setup GPARAM Banks.");
            }
        }

        try
        {
            GparamEditor = new GparamEditorScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Graphics Param Editor] Failed to setup Graphics Param Editor", ex);
            return false;
        }

        return true;
    }

    private async Task<bool> InitializeMaterialEditor(bool silent)
    {
        // Only do this once, as 3 editors may invoke this.
        if (MaterialData == null)
        {
            MaterialData = new(Project);

            Task<bool> materialDataTask = MaterialData.Setup();
            bool materialDataTaskResult = await materialDataTask;

            if (!silent)
            {
                if (materialDataTaskResult)
                {
                    TaskLogs.AddLog($"[Material Editor] Setup Material Data.");
                }
                else
                {
                    TaskLogs.AddLog($"[Material Editor] Failed to setup Material Data.");
                }
            }
        }

        try
        {
            MaterialEditor = new MaterialEditorScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Material Editor] Failed to setup Material Editor", ex);
            return false;
        }

        return true;
    }


    private async Task<bool> InitializeTextureViewer(bool silent)
    {
        TextureData = new(Project);

        // Texture Banks
        Task<bool> textureDataTask = TextureData.Setup();
        bool textureDataTaskResult = await textureDataTask;

        if (!silent)
        {
            if (textureDataTaskResult)
            {
                TaskLogs.AddLog($"[Texture Viewer] Setup texture bank.");
            }
            else
            {
                TaskLogs.AddLog($"[Texture Viewer] Failed to setup texture bank.");
            }
        }

        try
        {
            TextureViewer = new TextureViewerScreen(Project);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"[Material Editor] Failed to setup Material Editor", ex);
            return false;
        }

        return true;
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

        ProjectData = null;

        FileBrowser = null;

        MapData = null;
        MapEditor = null;

        ModelData = null;
        ModelEditor = null;

        ParamEditor = null;
        ParamData = null;

        TextData = null;
        TextEditor = null;

        GparamData = null;
        GparamEditor = null;

        MaterialData = null;
        MaterialEditor = null;

        TextureData = null;
        TextureViewer = null;
    }
    #endregion
}
