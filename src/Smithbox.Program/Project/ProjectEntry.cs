using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Application;

public class ProjectEntry
{
    public ProjectDescriptor Descriptor;

    public ProjectVFS VFS;
    public ProjectFileLocator Locator;
    public ProjectEditorHandler Handler;

    // State
    public bool Initialized = false;
    public bool IsInitializing = false;
    public bool IsLoadingData { get; private set; }
    public bool IsCreatingEditors { get; private set; }

    public bool IsSelected = false;

    public int AutomaticSaveInterval = 60;
    public DateTime _nextAutoSaveTime = DateTime.MinValue;

    public ProjectEntry() { }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Handler.MapEditorStub != null)
            Handler.MapEditorStub.Draw(device, cl);

        if (Handler.ModelEditorStub != null)
            Handler.ModelEditorStub.Draw(device, cl);
    }

    public unsafe void Update(float dt)
    {
        var commands = EditorCommandQueue.GetNextCommand();

        if (Handler.MapEditorStub != null)
            Handler.MapEditorStub.Display(dt, commands);

        if (Handler.ModelEditorStub != null)
            Handler.ModelEditorStub.Display(dt, commands);

        if (Handler.TextEditorStub != null)
            Handler.TextEditorStub.Display(dt, commands);

        if (Handler.ParamEditorStub != null)
            Handler.ParamEditorStub.Display(dt, commands);

        if (Handler.GparamEditorStub != null)
            Handler.GparamEditorStub.Display(dt, commands);

        if (Handler.MaterialEditorStub != null)
            Handler.MaterialEditorStub.Display(dt, commands);

        if (Handler.TextureViewerStub != null)
            Handler.TextureViewerStub.Display(dt, commands);

        if (Handler.FileBrowserStub != null)
            Handler.FileBrowserStub.Display(dt, commands);

        // Auto-Save
        AutomaticSaveInterval = (int)CFG.Current.Project_Automatic_Save_Interval;

        // Do this so we don't auto-save at init
        if (!AutomaticSaveSetup)
        {
            AutomaticSaveSetup = true;
            _nextAutoSaveTime = DateTime.UtcNow.AddSeconds(AutomaticSaveInterval);
        }
        else if (AutomaticSaveInterval > 0 && DateTime.UtcNow >= _nextAutoSaveTime)
        {
            PerformAutoSave();
            _nextAutoSaveTime = DateTime.UtcNow.AddSeconds(AutomaticSaveInterval);
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if(Handler.MapEditorStub != null)
            Handler.MapEditorStub.EditorResized(window, device);

        if (Handler.ModelEditorStub != null)
            Handler.ModelEditorStub.EditorResized(window, device);
    }

    public async Task<bool> Init(Action<ProjectLoadProgress> reportProgress, bool silent = false, ProjectInitType initType = ProjectInitType.ProjectDefined)
    {
        // Sanity checks
        if(Descriptor.ProjectType is ProjectType.Undefined)
        {
            TaskLogs.AddError($"[Smithbox] Project initialization failed. Project Type is undefined.");

            return false;
        }

        if(!Directory.Exists(Descriptor.ProjectPath))
        {
            TaskLogs.AddError($"[Smithbox] Project initialization failed. Project path does not exist: {Descriptor.ProjectPath}");

            return false;
        }

        if (!Directory.Exists(Descriptor.DataPath))
        {
            TaskLogs.AddError($"[Smithbox] Project initialization failed. Data path does not exist: {Descriptor.DataPath}");

            return false;
        }

        Initialized = false;
        IsInitializing = true;


        VFS = new(this);
        Locator = new(this);
        Handler = new(this);

        try
        {
            SetupDLLs();
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup DLLs", e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Setting up VFS",
                Percent = 0.05f
            });
        }

        try
        {
            VFS.Initialize();
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup VFS", e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Initializing editor stubs",
                Percent = 0.1f
            });
        }

        try
        {
            Handler.InitStubs();
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup editor stubs", e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Indexing project files",
                Percent = 0.2f
            });
        }

        try
        {
            await Locator.Initialize(reportProgress, silent);
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup file locator", e);
        }

        IsLoadingData = true;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Loading project data",
                Percent = 0.4f
            });
        }

        try
        {
            await Handler.InitializeData(initType, silent);
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup editor data", e);
        }

        IsLoadingData = false;
        IsCreatingEditors = true;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Loading project editors",
                Percent = 0.8f
            });
        }

        try
        {
            Handler.InitializeEditors(initType);
        }
        catch (Exception e)
        {
            TaskLogs.AddError("Failed to setup editors", e);
        }

        IsCreatingEditors = false;
        Initialized = true;
        IsInitializing = false;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Finalizing",
                Percent = 1.0f
            });
        }

        return true;
    }

    #region Automatic Save
    private bool AutomaticSaveSetup = false;

    private void PerformAutoSave()
    {
        if (CFG.Current.Project_Enable_Automatic_Save)
        {
            try
            {
                TaskLogs.AddInfo($"[Smithbox] Auto-save triggered.");

                if (CFG.Current.Project_Automatic_Save_Include_Map_Editor)
                {
                    if (Handler.MapEditor != null && Handler.FocusedEditor is MapEditorScreen)
                    {
                        Handler.MapEditor.Save(true);
                    }
                }

                if (CFG.Current.AutomaticSave_ModelEditor)
                {
                    if (Handler.ModelEditor != null && Handler.FocusedEditor is ModelEditorScreen)
                    {
                        Handler.ModelEditor.Save(true);
                    }
                }

                if (CFG.Current.Project_Automatic_Save_Include_Param_Editor && Handler.FocusedEditor is ParamEditorScreen)
                {
                    if (Handler.ParamEditor != null)
                    {
                        Handler.ParamEditor.Save(true);
                    }
                }
                if (CFG.Current.Project_Automatic_Save_Include_Text_Editor && Handler.FocusedEditor is TextEditorScreen)
                {
                    if (Handler.TextEditor != null)
                    {
                        Handler.TextEditor.Save(true);
                    }
                }
                if (CFG.Current.Project_Automatic_Save_Include_Gparam_Editor && Handler.FocusedEditor is GparamEditorScreen)
                {
                    if (Handler.GparamEditor != null)
                    {
                        Handler.GparamEditor.Save(true);
                    }
                }
                if (CFG.Current.Project_Automatic_Save_Include_Material_Editor && Handler.FocusedEditor is MaterialEditorScreen)
                {
                    if (Handler.MaterialEditor != null)
                    {
                        Handler.MaterialEditor.Save(true);
                    }
                }
            }
            catch (Exception ex)
            {
                TaskLogs.AddError($"[Smithbox] Auto-save failed.", ex);
            }
        }
    }
    #endregion

    #region Setup DLLS
    public void SetupDLLs()
    {
        if (Descriptor.ProjectType is ProjectType.SDT or ProjectType.ER)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_6_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_6_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.6.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.6.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.6");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.6");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }

        if (Descriptor.ProjectType is ProjectType.AC6)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_8_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_8_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.8.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.8.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.8");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.8");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }


        if (Descriptor.ProjectType is ProjectType.NR)
        {
#if WINDOWS
            var rootDllPath = Path.Join(Descriptor.DataPath, "oo2core_9_win64.dll");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "oo2core_9_win64.dll");
#elif OSX
            var rootDllPath = Path.Join(DataPath, "liboo2coremac64.2.9.dylib");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2coremac64.2.9.dylib");
#elif LINUX
            var rootDllPath = Path.Join(DataPath, "liboo2corelinux64.so.9");
            var projectDllPath = Path.Join(AppContext.BaseDirectory, "liboo2corelinux64.so.9");
#endif

            if (File.Exists(rootDllPath))
            {
                if (!File.Exists(projectDllPath))
                {
                    File.Copy(rootDllPath, projectDllPath);
                }
            }
        }
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        Handler?.Dispose();
        Locator?.Dispose();
        VFS?.Dispose();

        Handler = null;
        Locator = null;
        VFS = null;

        Initialized = false;
        IsInitializing = false;
    }
    #endregion
}