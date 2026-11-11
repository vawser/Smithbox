using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Utilities;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Logger;
using StudioCore.Renderer;
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

    /// <summary>
    /// Initialized only for another project's comparison data (see <see cref="ProjectOrchestrator.LoadAuxiliaryProject"/>),
    /// with only some of its editors. Not offered as an open project.
    /// </summary>
    public bool IsAuxiliary = false;

    /// <summary>
    /// Fully loaded and listed under Active Projects. Several projects can be active at once; the selected one is
    /// the one the editors show.
    /// </summary>
    public bool IsActive => Initialized && !IsAuxiliary;

    public int AutomaticSaveInterval = 60;
    public DateTime _nextAutoSaveTime = DateTime.MinValue;

    public ProjectEntry() { }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Handler != null)
        {
            if (Handler.MapEditorStub != null)
                Handler.MapEditorStub.Draw(device, cl);

            if (Handler.ModelEditorStub != null)
                Handler.ModelEditorStub.Draw(device, cl);

            if (Handler.TimeActEditorStub != null)
                Handler.TimeActEditorStub.Draw(device, cl);

            if (Handler.GparamEditorStub != null)
                Handler.GparamEditorStub.Draw(device, cl);

            if (Handler.FxEditorStub != null)
                Handler.FxEditorStub.Draw(device, cl);

            ModelPreviewCache.Draw(this);
            MeshPreviewRenderer.Draw();
        }
    }

    public unsafe void Update(float dt)
    {
        var commands = EditorCommandQueue.GetNextCommand();

        if (Handler != null)
        {
            if (Handler.MapEditorStub != null)
                Handler.MapEditorStub.Display(dt, commands);

            if (Handler.ModelEditorStub != null)
                Handler.ModelEditorStub.Display(dt, commands);

            if (Handler.TimeActEditorStub != null)
                Handler.TimeActEditorStub.Display(dt, commands);

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

            if (Handler.HavokEditorStub != null)
                Handler.HavokEditorStub.Display(dt, commands);

            if (Handler.FxEditorStub != null)
                Handler.FxEditorStub.Display(dt, commands);

            if (Handler.BehaviorEditorStub != null)
                Handler.BehaviorEditorStub.Display(dt, commands);

            if (Handler.EmevdEditorStub != null)
                Handler.EmevdEditorStub.Display(dt, commands);

            if (Handler.EsdEditorStub != null)
                Handler.EsdEditorStub.Display(dt, commands);
        }

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
        if (Handler != null)
        {
            if (Handler.MapEditorStub != null)
                Handler.MapEditorStub.EditorResized(window, device);

            if (Handler.ModelEditorStub != null)
                Handler.ModelEditorStub.EditorResized(window, device);

            if (Handler.TimeActEditorStub != null)
                Handler.TimeActEditorStub.EditorResized(window, device);

            if (Handler.GparamEditorStub != null)
                Handler.GparamEditorStub.EditorResized(window, device);

            if (Handler.FxEditorStub != null)
                Handler.FxEditorStub.EditorResized(window, device);
        }
    }

    /// <summary>
    /// Checks the descriptor can be initialized, logging why not.
    /// </summary>
    public bool ValidateForInit()
    {
        return ValidateForInit(out _);
    }

    /// <param name="reason">Why the project can't be initialized, or null if it can.</param>
    public bool ValidateForInit(out string reason)
    {
        reason = null;

        if (Descriptor.ProjectType is ProjectType.Undefined)
            reason = LOC.Get("PROJECT_Setup_Failed_Init_Undefined_Type");
        else if (!Directory.Exists(Descriptor.ProjectPath))
            reason = LOC.Get("PROJECT_Setup_Failed_Init_Bad_Project_Path", Descriptor.ProjectPath);
        else if (!Directory.Exists(Descriptor.DataPath))
            reason = LOC.Get("PROJECT_Setup_Failed_Init_Bad_Data_Path", Descriptor.DataPath);

        if (reason == null)
            return true;

        Smithbox.LogError(this, reason, LogPriority.High);

        return false;
    }

    public async Task<bool> Init(Action<ProjectLoadProgress> reportProgress, bool silent = false, ProjectInitType initType = ProjectInitType.ProjectDefined)
    {
        if (!ValidateForInit())
            return false;

        // Re-initializing (a reload, or an auxiliary project loaded again) replaces the previous parts, so release them
        Dispose();

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
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_DLL_Setup"), e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Setup_VFS"),
                Percent = 0.05f
            });
        }

        try
        {
            VFS.Initialize();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_VFS_Setup"), e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Setup_Editor_Stub"),
                Percent = 0.1f
            });
        }

        try
        {
            Handler.InitStubs();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_Editor_Stub_Setup"), e);
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Index_Project_Files"),
                Percent = 0.2f
            });
        }

        try
        {
            await Locator.Initialize(reportProgress, silent);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_Locator_Setup"), e);
        }

        IsLoadingData = true;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Load_Project_Data"),
                Percent = 0.4f
            });
        }

        try
        {
            await Handler.InitializeData(initType, silent);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_Project_Data_Setup"), e);
        }

        IsLoadingData = false;
        IsCreatingEditors = true;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Load_Project_Editors"),
                Percent = 0.8f
            });
        }

        try
        {
            Handler.InitializeEditors(initType);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PROJECT_Setup_Failed_Init_Failed_Project_Editor_Setup"), e);
        }

        IsCreatingEditors = false;
        Initialized = true;
        IsInitializing = false;

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PROJECT_Setup_Progress_Phase_Init_Project"),
                StepLabel = LOC.Get("PROJECT_Setup_Progress_Step_Finalizing"),
                Percent = 1.0f
            });
        }

        return true;
    }

    #region Unsaved Changes
    /// <summary>
    /// The alias types changed in the Project Metadata editor since they were last saved. Set by
    /// <see cref="ProjectAliasMenu"/>, which saves one alias type at a time.
    /// </summary>
    public HashSet<ProjectAliasType> UnsavedAliasTypes = new();

    /// <summary>
    /// Whether the param enums were changed in the Project Metadata editor since they were last saved
    /// </summary>
    public bool HasUnsavedEnums;

    /// <summary>
    /// The editors of this project, for the unsaved changes prompt. Empty unless the project is active.
    /// </summary>
    public IEnumerable<EditorScreen> GetEditors()
    {
        if (!IsActive || Handler == null)
            yield break;

        EditorScreen[] editors =
        [
            Handler.MapEditor,
            Handler.ModelEditor,
            Handler.TimeActEditor,
            Handler.ParamEditor,
            Handler.TextEditor,
            Handler.GparamEditor,
            Handler.MaterialEditor,
            Handler.HavokEditor,
            Handler.FxEditor,
            Handler.BehaviorEditor,
            Handler.EmevdEditor,
            Handler.EsdEditor,
            Handler.TextureViewer,
            Handler.FileBrowser,
        ];

        foreach (var editor in editors)
        {
            if (editor != null)
                yield return editor;
        }
    }

    /// <summary>
    /// Whether the project configuration form holds edits of this project not yet applied with Update
    /// </summary>
    public bool HasUnsavedConfiguration()
    {
        var menu = Smithbox.Orchestrator?.ProjectEditor?.ConfigureMenu;

        return menu != null && menu.Project == this && menu.HasUnsavedChanges();
    }

    /// <summary>
    /// The editors with unsaved changes, and the files each lists
    /// </summary>
    /// <param name="includeConfiguration">Also the configuration form's unapplied edits, for actions that lose them
    /// (exit and unload). Checked even for a project that isn't loaded, as the form edits those too.</param>
    public List<UnsavedEditor> GetUnsavedChanges(bool includeConfiguration = false)
    {
        var result = new List<UnsavedEditor>();

        if (includeConfiguration && HasUnsavedConfiguration())
        {
            var menu = Smithbox.Orchestrator.ProjectEditor.ConfigureMenu;

            AddUnsaved(result, LOC.Get("UNSAVED_Configuration"),
                () => HasUnsavedConfiguration() ? [LOC.Get("UNSAVED_Configuration_File")] : new List<string>(),
                () => menu.ApplyWithoutReload());
        }

        foreach (var editor in GetEditors())
        {
            var screen = editor;

            // Some names carry an ImGui id ("Material Editor##MaterialEditor"), which would hide the prompt's file count
            var name = screen.EditorName ?? "";
            var idStart = name.IndexOf("##", StringComparison.Ordinal);
            if (idStart >= 0)
                name = name[..idStart];

            AddUnsaved(result, name, () => screen.GetUnsavedFiles(), () => screen.SaveUnsaved());
        }

        // The alias and enum editors are shared by every project, so their edits are kept on the project
        if (IsActive)
        {
            var metadata = Smithbox.Orchestrator?.ProjectEditor;

            if (metadata != null)
            {
                AddUnsaved(result, LOC.Get("UNSAVED_Metadata_Aliases"),
                    () => UnsavedAliasTypes.Select(e => LOC.Get(e.GetDisplayName())).OrderBy(e => e).ToList(),
                    () => metadata.AliasMenu.SaveUnsavedLocal(this));

                AddUnsaved(result, LOC.Get("UNSAVED_Metadata_Enums"),
                    () => HasUnsavedEnums ? [LOC.Get("UNSAVED_Metadata_Enums_File")] : new List<string>(),
                    () => metadata.EnumMenu.SaveUnsavedLocal(this));
            }
        }

        return result;
    }

    private void AddUnsaved(List<UnsavedEditor> result, string name, Func<List<string>> getFiles, Action save)
    {
        List<string> files;

        try
        {
            files = getFiles();
        }
        catch (Exception e)
        {
            // A failed check must not stop the prompt, or the app couldn't be closed
            Smithbox.LogError(this, LOC.Get("UNSAVED_Check_Failed", name), e);
            return;
        }

        if (files is { Count: > 0 })
            result.Add(new UnsavedEditor(name, files, save));
    }

    /// <summary>
    /// True while a save started by the unsaved changes prompt is still running in any editor
    /// </summary>
    public bool IsSavingUnsaved()
    {
        return GetEditors().Any(e => e.IsSavingUnsaved);
    }
    #endregion

    #region Automatic Save
    private bool AutomaticSaveSetup = false;

    private void PerformAutoSave()
    {
        if (CFG.Current.Project_Enable_Automatic_Save)
        {
            if (Handler != null)
            {
                try
                {
                    Smithbox.Log(this, LOC.Get("PROJECT_Setup_AutoSave_Triggered"));

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
                            // All changed containers, matching the Text Editor's Save keybind
                            Handler.TextEditor.SaveAll(true);
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

                    // Gated by HavokEditor_Allow_Automatic_Save within the editor
                    if (Handler.FocusedEditor is HavokEditorScreen)
                    {
                        if (Handler.HavokEditor != null)
                        {
                            Handler.HavokEditor.SaveAll(true);
                        }
                    }

                    // Gated by FxEditor_Allow_Automatic_Save within the editor
                    if (Handler.FocusedEditor is FxEditorScreen)
                    {
                        Handler.FxEditor?.SaveAll(true);
                    }

                    // Gated by BehaviorEditor_Allow_Automatic_Save within the editor
                    if (Handler.FocusedEditor is BehaviorEditorScreen)
                    {
                        Handler.BehaviorEditor?.Save(true);
                    }

                    // Gated by EmevdEditor_Allow_Automatic_Save within the editor
                    if (Handler.FocusedEditor is EmevdEditorScreen)
                    {
                        Handler.EmevdEditor?.Save(true);
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("PROJECT_Setup_AutoSave_Failed"), ex);
                }
            }
        }
    }
    #endregion

    #region Setup DLLS
    public void SetupDLLs()
    {
        OodleSetup.CopyFromGameDirectory(Descriptor.DataPath);
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

        // The edited aliases and enums were held by the data just released
        UnsavedAliasTypes.Clear();
        HasUnsavedEnums = false;
    }
    #endregion
}
