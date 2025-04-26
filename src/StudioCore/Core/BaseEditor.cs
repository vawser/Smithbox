using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;

using StudioCore.Resource;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Veldrid;
using Veldrid.Sdl2;
using Thread = System.Threading.Thread;

namespace StudioCore.Core;

public class BaseEditor
{
    ///------------------------------------------------
    /// Program State
    ///------------------------------------------------
    public IGraphicsContext GraphicsContext;
    public string _programTitle;
    public readonly string Version;

    public ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
    public ImGuiWindowFlags SubWindowFlags = ImGuiWindowFlags.NoMove;

    public bool LowRequirementsMode;

    private bool _initialLoadComplete;
    public bool FirstFrame = true;

    private double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private readonly bool _limitFrameRate = true;
    public bool FontRebuildRequest;

    public Project SelectedProject;
    public List<Project> Projects = new();

    public bool HasSetup = false;

    public ProjectDisplay ProjectDisplayConfig;

    public unsafe BaseEditor(IGraphicsContext context, string version)
    {
        if (HasSetup)
            return;

        HasSetup = true;

        // Program Window
        Version = version;
        _programTitle = $"Version {Version}";

        GraphicsContext = context;
        GraphicsContext.Initialize();
        GraphicsContext.Window.Title = _programTitle;

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        SetupFonts();

        GraphicsContext.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;

        // Program Flow
        ProjectUtils.SetupFolders();

        CFG.Load();
        UI.Load();
        KeyBindings.Load();
        InterfaceTheme.SetupThemes();
        InterfaceTheme.SetTheme(true);

        LoadProjectDisplayConfig();
        LoadExistingProjects();

        // Set culture to invariant so we don't get funny issues due to localization
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Setup ignore asserts feature
        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;
    }

    /// <summary>
    /// Viewport main loop
    /// </summary>
    public void Run()
    {
        long previousFrameTicks = 0;

        Stopwatch sw = new();
        sw.Start();

        while (GraphicsContext.Window.Exists)
        {
            if (!GraphicsContext.Window.Focused)
            {
                _desiredFrameLengthSeconds = 1.0 / 20.0f;
            }
            else
            {
                _desiredFrameLengthSeconds = 1.0 / CFG.Current.System_Frame_Rate;
            }

            var currentFrameTicks = sw.ElapsedTicks;
            var deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

            while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
            {
                currentFrameTicks = sw.ElapsedTicks;
                deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                Thread.Sleep(!GraphicsContext.Window.Focused ? 0 : 1);
            }

            previousFrameTicks = currentFrameTicks;

            InputSnapshot snapshot = null;
            Sdl2Events.ProcessEvents();

            snapshot = GraphicsContext.Window.PumpEvents();

            InputTracker.UpdateFrameInput(snapshot, GraphicsContext.Window);

            Update((float)deltaSeconds);

            if (!GraphicsContext.Window.Exists)
            {
                break;
            }

            GraphicsContext.Draw(SelectedProject);

            ResourceManager.UpdateTasks();
        }

        Exit();
    }

    /// <summary>
    /// Editor main loop
    /// </summary>
    /// <param name="deltaseconds"></param>
    private unsafe void Update(float deltaseconds)
    {
        if (CFG.Current.AllowInterfaceMovement)
        {
            MainWindowFlags = ImGuiWindowFlags.MenuBar;
            SubWindowFlags = ImGuiWindowFlags.None;
        }
        else
        {
            MainWindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove;
            SubWindowFlags = ImGuiWindowFlags.NoMove;
        }

        DPI.UpdateDpi(GraphicsContext);

        var scale = DPI.GetUIScale();

        if (FontRebuildRequest)
        {
            GraphicsContext.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, SetupFonts);
            FontRebuildRequest = false;
        }
        else
        {
            GraphicsContext.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, null);
        }

        TaskManager.ThrowTaskExceptions();

        var commandsplit = EditorCommandQueue.GetNextCommand();
        if (commandsplit != null && commandsplit[0] == "windowFocus")
        {
            //this is a hack, cannot grab focus except for when un-minimising
            _user32_ShowWindow(GraphicsContext.Window.Handle, 6);
            _user32_ShowWindow(GraphicsContext.Window.Handle, 9);
        }

        RenderDockspace();

        UIHelper.ApplyBaseStyle();

        // Special modals
        ProjectCreation.Draw();
        ProjectSettings.Draw();
        ProjectAliasEditor.Draw();
        EditorSettings.Draw();
        ControlSettings.Draw();
        InterfaceSettings.Draw();
        HelpGuide.Draw();

        Menubar();

        if (CFG.Current.DisplayProjectListWindow)
        {
            ImGui.Begin($"Project List##ProjectListWindow");

            DisplayProjectActions();
            DisplayProjectList();

            ImGui.End();
        }

        if (CFG.Current.DisplayProjectWindow)
        {
            foreach (var projectEntry in Projects)
            {
                if (projectEntry == SelectedProject)
                {
                    projectEntry.Draw(commandsplit);
                }
            }
        }

        UIHelper.UnapplyBaseStyle();

        // Create new project if triggered to do so
        if (ProjectCreation.Create)
        {
            ProjectCreation.Create = false;
            CreateProject();
        }

        UIHelper.UnapplyBaseStyle();
    }

    public void Exit()
    {
        ResourceManager.Shutdown();

        GraphicsContext.Dispose();

        CFG.Save();
        UI.Save();
    }

    private void RenderDockspace()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.MenuBar;

        ImGui.Begin("MainDockspace_W", windowFlags);

        uint dockspaceID = ImGui.GetID("MainDockspace");

        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.End();

        ImGui.PopStyleVar();
    }

    private void Menubar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu($"Settings"))
            {
                if (ImGui.MenuItem("Editor"))
                {
                    EditorSettings.Show();
                }
                UIHelper.Tooltip("Open the Editor settings window.");

                if (ImGui.MenuItem("Controls"))
                {
                    ControlSettings.Show();
                }
                UIHelper.Tooltip("Open the Controls settings window.");

                if (ImGui.MenuItem("Interface"))
                {
                    InterfaceSettings.Show();
                }
                UIHelper.Tooltip("Open the Interface settings window.");

                if (ImGui.MenuItem("Help"))
                {
                    HelpGuide.Show();
                }
                UIHelper.Tooltip("Open the Help guide window.");

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Project List", CFG.Current.DisplayProjectListWindow))
                {
                    CFG.Current.DisplayProjectListWindow = !CFG.Current.DisplayProjectListWindow;
                }
                UIHelper.Tooltip("Toggle the visibility of the Project Lists window.");

                if (ImGui.MenuItem("Project", CFG.Current.DisplayProjectWindow))
                {
                    CFG.Current.DisplayProjectWindow = !CFG.Current.DisplayProjectWindow;
                }
                UIHelper.Tooltip("Toggle the visibility of the Project window.");

                ImGui.EndMenu();
            }

            ImGui.Separator();

            TaskLogs.DisplayActionLoggerBar();
            TaskLogs.DisplayActionLoggerWindow();

            ImGui.Separator();

            TaskLogs.DisplayWarningLoggerBar();
            TaskLogs.DisplayWarningLoggerWindow();

            ImGui.EndMainMenuBar();
        }
    }

    /// <summary>
    /// Actions to scan for projects or add a new project
    /// </summary>
    public void DisplayProjectActions()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;
        var buttonSize = new Vector2(windowWidth, 24);

        if (ImGui.Button("Add Project", buttonSize))
        {
            ProjectCreation.Show();
        }
        UIHelper.Tooltip($"Add a new project to the project list.");
    }

    /// <summary>
    /// The list of stored projects.
    /// </summary>
    public unsafe void DisplayProjectList()
    {
        UIHelper.SimpleHeader("projectListHeader", "Available Projects", "The projects currently available to select from.", UI.Current.ImGui_AliasName_Text);

        var orderedProjects = Projects
        .OrderBy(p =>
        {
            foreach (var kvp in ProjectDisplayConfig.ProjectOrder)
            {
                if (kvp.Value == p.ProjectGUID)
                {
                    return kvp.Key;
                }
            }
            return int.MaxValue; // Put untracked projects at the end
        })
        .ToList();

        int dragSourceIndex = -1;
        int dragTargetIndex = -1;

        for (int i = 0; i < orderedProjects.Count; i++)
        {
            var project = orderedProjects[i];
            var imGuiID = project.ProjectGUID;

            // Highlight selectable
            if (ImGui.Selectable($"{project.ProjectName}##{imGuiID}", SelectedProject == project))
            {
                SelectedProject.Close();

                SelectedProject = project;

                foreach (var tEntry in Projects)
                    tEntry.IsSelected = false;

                SelectedProject.IsSelected = true;
            }

            // Begin drag
            if (ImGui.BeginDragDropSource())
            {
                int payloadIndex = i;
                ImGui.SetDragDropPayload("PROJECT_DRAG", &payloadIndex, sizeof(int));
                ImGui.Text(project.ProjectName);
                ImGui.EndDragDropSource();
            }

            // Accept drop
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("PROJECT_DRAG");
                if (payload.Handle != null)
                {
                    int* droppedIndex = (int*)payload.Data;
                    dragSourceIndex = *droppedIndex;
                    dragTargetIndex = i;
                }
                ImGui.EndDragDropTarget();
            }

            if (ImGui.BeginPopupContextItem($"ProjectListContextMenu{imGuiID}"))
            {
                if (ImGui.MenuItem($"Open Project Settings##projectSettings_{imGuiID}"))
                {
                    ProjectSettings.Show(this, SelectedProject);
                }

                if (ImGui.MenuItem($"Open Project Aliases##projectAliases_{imGuiID}"))
                {
                    ProjectAliasEditor.Show(this, SelectedProject);
                }

                if (CFG.Current.ModEngineInstall != "")
                {
                    if (ImGui.MenuItem($"Launch Mod##launchMod{imGuiID}"))
                    {
                        ProjectUtils.LaunchMod(SelectedProject);
                    }
                }

                ImGui.EndPopup();
            }
        }

        if (dragSourceIndex >= 0 && dragTargetIndex >= 0 && dragSourceIndex != dragTargetIndex)
        {
            var movedProject = orderedProjects[dragSourceIndex];
            orderedProjects.RemoveAt(dragSourceIndex);
            orderedProjects.Insert(dragTargetIndex, movedProject);

            // Rebuild order dictionary
            ProjectDisplayConfig.ProjectOrder.Clear();
            for (int i = 0; i < orderedProjects.Count; i++)
            {
                ProjectDisplayConfig.ProjectOrder[i] = orderedProjects[i].ProjectGUID;
            }

            SaveProjectDisplayConfig();
        }
    }

    private void LoadProjectDisplayConfig()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        if (!File.Exists(file))
        {
            ProjectDisplayConfig = new ProjectDisplay();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                ProjectDisplayConfig = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectDisplay);

                if (ProjectDisplayConfig == null)
                {
                    throw new Exception("[Smithbox] Failed to read Project Display.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Project Display.json");

                ProjectDisplayConfig = new ProjectDisplay();
            }
        }
    }

    public void SaveProjectDisplayConfig()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        var json = JsonSerializer.Serialize(ProjectDisplayConfig, SmithboxSerializerContext.Default.ProjectDisplay);

        File.WriteAllText(file, json);
    }

    private void LoadExistingProjects()
    {
        // Read all the stored project jsons and create an existing Project dict
        var projectJsonList = ProjectUtils.GetStoredProjectJsonList();

        var buildProjectOrder = false;

        // If it is not set, create initial order
        if (ProjectDisplayConfig.ProjectOrder == null)
        {
            ProjectDisplayConfig.ProjectOrder = new();
            buildProjectOrder = true;
        }

        for (int i = 0; i < projectJsonList.Count; i++)
        {
            var entry = projectJsonList[i];

            if (File.Exists(entry))
            {

                try
                {
                    var filestring = File.ReadAllText(entry);
                    var options = new JsonSerializerOptions();

                    var curProject = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.Project);

                    if (curProject == null)
                    {
                        TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        curProject.BaseEditor = this;

                        // Ignore unsupported projects
                        if (ProjectUtils.IsSupportedProjectType(curProject.ProjectType))
                        {
                            Projects.Add(curProject);

                            if (buildProjectOrder)
                            {
                                ProjectDisplayConfig.ProjectOrder.Add(i, curProject.ProjectGUID);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                }
            }
        }

        if (buildProjectOrder)
        {
            SaveProjectDisplayConfig();
        }

        if (Projects.Count > 0)
        {
            foreach (var projectEntry in Projects)
            {
                if (projectEntry.AutoSelect)
                {
                    SelectedProject = projectEntry;
                    SelectedProject.IsSelected = true;
                }
            }
        }
    }

    private void CreateProject()
    {
        var guid = GUID.Generate();
        var projectName = ProjectCreation.ProjectName;
        var projectPath = ProjectCreation.ProjectPath;
        var dataPath = ProjectCreation.DataPath;
        var projectType = ProjectCreation.ProjectType;

        var newProject = new Project(this, guid, projectName, projectPath, dataPath, projectType);

        ProjectCreation.Reset();

        Projects.Add(newProject);

        newProject.Save();

        // Auto-select new project
        foreach (var tEntry in Projects)
        {
            tEntry.IsSelected = false;
        }
        SelectedProject.IsSelected = true;
    }

    private unsafe void SetupFonts()
    {
        string engFont = @"Assets\Fonts\RobotoMono-Light.ttf";
        string otherFont = @"Assets\Fonts\NotoSansCJKtc-Light.otf";

        if (!string.IsNullOrWhiteSpace(UI.Current.System_English_Font) && File.Exists(UI.Current.System_English_Font))
            engFont = UI.Current.System_English_Font;
        if (!string.IsNullOrWhiteSpace(UI.Current.System_Other_Font) && File.Exists(UI.Current.System_Other_Font))
            otherFont = UI.Current.System_Other_Font;

        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
        var fileEn = Path.Combine(AppContext.BaseDirectory, engFont);
        var fontEn = File.ReadAllBytes(fileEn);
        var fontEnNative = ImGui.MemAlloc((uint)fontEn.Length);
        Marshal.Copy(fontEn, 0, (nint)fontEnNative, fontEn.Length);

        var fileOther = Path.Combine(AppContext.BaseDirectory, otherFont);
        var fontOther = File.ReadAllBytes(fileOther);
        var fontOtherNative = ImGui.MemAlloc((uint)fontOther.Length);
        Marshal.Copy(fontOther, 0, (nint)fontOtherNative, fontOther.Length);

        var fileIcon = Path.Combine(AppContext.BaseDirectory, @"Assets\Fonts\forkawesome-webfont.ttf");
        var fontIcon = File.ReadAllBytes(fileIcon);
        var fontIconNative = ImGui.MemAlloc((uint)fontIcon.Length);
        Marshal.Copy(fontIcon, 0, (nint)fontIconNative, fontIcon.Length);

        fonts.Clear();

        var scaleFine = (float)Math.Round(UI.Current.Interface_FontSize * DPI.GetUIScale());
        var scaleLarge = (float)Math.Round((UI.Current.Interface_FontSize + 2) * DPI.GetUIScale());

        fonts.AddFontDefault();

        // English fonts
        //{
        //    ImFontConfigPtr cfg = ImGui.ImFontConfig();
        //    cfg.GlyphMinAdvanceX = 5.0f;
        //    cfg.OversampleH = 3;
        //    cfg.OversampleV = 2;
        //    fonts.AddFontFromMemoryTTF(fontEnNative, fontIcon.Length, scaleFine, cfg,
        //        fonts.GetGlyphRangesDefault());
        //}

        // Other language fonts
        {
            ImFontConfigPtr cfg = ImGui.ImFontConfig();
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            cfg.OversampleH = 2;
            cfg.OversampleV = 2;

            ImFontGlyphRangesBuilderPtr glyphRanges = ImGui.ImFontGlyphRangesBuilder();
            glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());
            Array.ForEach(InterfaceUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

            if (UI.Current.System_Font_Chinese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
            }

            if (UI.Current.System_Font_Korean)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
            }

            if (UI.Current.System_Font_Thai)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
            }

            if (UI.Current.System_Font_Vietnamese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
            }

            if (UI.Current.System_Font_Cyrillic)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesCyrillic());
            }

            ImVector<uint> outGlyphRanges;

            glyphRanges.BuildRanges(&outGlyphRanges);
            fonts.AddFontFromMemoryTTF(fontOtherNative, fontOther.Length, scaleLarge, cfg, outGlyphRanges.Data);
            glyphRanges.Destroy();
        }

        // Icon fonts
        {
            ushort[] ranges = { ForkAwesome.IconMin, ForkAwesome.IconMax, 0 };
            ImFontConfigPtr cfg = ImGui.ImFontConfig();
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 12.0f;
            cfg.OversampleH = 3;
            cfg.OversampleV = 3;
            ImFontGlyphRangesBuilder b = new();

            fixed (ushort* r = ranges)
            {
                ImFontPtr f = fonts.AddFontFromMemoryTTF(fontIconNative, fontIcon.Length, scaleLarge, cfg,
                    (uint*)r);
            }
        }

        GraphicsContext.ImguiRenderer.RecreateFontDeviceTexture();
    }

    //Unhappy with this being here
    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _user32_ShowWindow(nint hWnd, int nCmdShow);

    public void CrashShutdown()
    {
        Tracy.Shutdown();
        ResourceManager.Shutdown();
        GraphicsContext.Dispose();
    }
}

