using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Utilities;
using Microsoft.Extensions.Logging;
using Smithbox.Core.JSON;
using Smithbox.Core.Utilities;
using SoulsFormats;
using StudioCore;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tools;
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
using Version = System.Version;

namespace Smithbox.Core.Core;

public class Smithbox
{
    /// <summary>
    /// If true, Smithbox is using the OpenGL rendering backend
    /// </summary>
    public static bool LowRequirementsMode;

    /// <summary>
    /// The current rendering context
    /// </summary>
    public static IGraphicsContext _context;

    /// <summary>
    /// The currently selected project instance
    /// </summary>
    public ProjectInstance SelectedProject;

    /// <summary>
    /// The current project instances detected in storage
    /// </summary>
    public List<ProjectInstance> Projects = new();

    /// <summary>
    /// Whether initial setup has finished
    /// </summary>
    public bool HasSetup = false;

    /// <summary>
    /// The display arrangement for the project list
    /// </summary>
    public ProjectDisplayConfig ProjectDisplayConfig;

    /// <summary>
    /// The desired frame length in seconds.
    /// </summary>
    private double DesiredFrameLengthSeconds = 1.0 / 20.0f;

    /// <summary>
    /// Whether to limit the frame rate
    /// </summary>
    private readonly bool LimitFrameRate = true;

    /// <summary>
    /// Whether to rebuild fonts
    /// </summary>
    public bool FontRebuildRequest;

    /// <summary>
    /// The soapstone service RPC server
    /// </summary>
    private readonly SoapstoneService SoapstoneService;

    /// <summary>
    /// Smithbox initialization
    /// </summary>
    /// <param name="context"></param>
    public unsafe Smithbox(IGraphicsContext context)
    {
        UIHelper.RestoreImguiIfMissing();

        UI.Load();
        CFG.Load();
        KeyBindings.Load();

        InterfaceTheme.SetupThemes();
        InterfaceTheme.SetTheme(true);

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        // Create window
        _context = context;
        _context.Initialize();
        _context.Window.Title = $"Smithbox - Version 2.0.0";

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        SetupFonts();
        _context.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;

        // Setup ignore asserts feature
        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;

        // Set culture to invariant so we don't get funny issues due to localization
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Smithbox main loop
    /// </summary>
    public void Run()
    {
        // Handlers
        //ProjectHandler = new ProjectHandler();
        //EditorHandler = new EditorHandler(_context);
        //WindowHandler = new CommonMenubarHandler(_context);
        //TextBank.LoadTextFiles();
        //_soapstoneService = new SoapstoneService(_version);

        //if (CFG.Current.Enable_Soapstone_Server)
        //{
        //    TaskManager.LiveTask task = new(
        //        "system_setupSoapstoneServer",
        //        "System",
        //        "soapstone server is running.",
        //        "soapstone server is not running.",
        //        TaskManager.RequeueType.None,
        //        false,
        //        () =>
        //        {
        //            SoapstoneServer.RunAsync(KnownServer.DSMapStudio, _soapstoneService).Wait();
        //        }
        //    );

        //    TaskManager.RunPassiveTask(task);
        //}

        //if (CFG.Current.System_Check_Program_Update)
        //{
        //    TaskManager.LiveTask task = new(
        //        "system_checkProgramUpdate",
        //        "System",
        //        "program update check has run.",
        //        "program update check has failed to run.",
        //        TaskManager.RequeueType.None,
        //        true,
        //        CheckProgramUpdate
        //    );

        //    TaskManager.Run(task);
        //}

        long previousFrameTicks = 0;

        Stopwatch sw = new();
        sw.Start();

        while (_context.Window.Exists)
        {
            DPI.UIScaleChanged += (_, _) =>
            {
                FontRebuildRequest = true;
            };

            // Limit frame rate when window isn't focused
            var focused = _context.Window.Focused;

            if (!focused)
            {
                DesiredFrameLengthSeconds = 1.0 / 20.0f;
            }
            else
            {
                DesiredFrameLengthSeconds = 1.0 / CFG.Current.System_Frame_Rate;
            }

            var currentFrameTicks = sw.ElapsedTicks;
            var deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

            while (LimitFrameRate && deltaSeconds < DesiredFrameLengthSeconds)
            {
                currentFrameTicks = sw.ElapsedTicks;
                deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                Thread.Sleep(focused ? 0 : 1);
            }

            previousFrameTicks = currentFrameTicks;

            InputSnapshot snapshot = null;

            Sdl2Events.ProcessEvents();

            snapshot = _context.Window.PumpEvents();

            InputTracker.UpdateFrameInput(snapshot, _context.Window);

            Update((float)deltaSeconds);

            if (!_context.Window.Exists)
            {
                break;
            }

            // For viewport
            //_context.Draw(EditorHandler.EditorList, EditorHandler.FocusedEditor);
        }

        Exit();
    }

    /// <summary>
    /// Smithbox is closing
    /// </summary>
    public void Exit()
    {
        ResourceManager.Shutdown();
        CFG.Save();
        UI.Save();

        _context.Dispose();
    }

    /// <summary>
    /// ImGui-specific main loop
    /// </summary>
    /// <param name="deltaseconds"></param>
    private unsafe void Update(float deltaseconds)
    {
        DPI.UpdateDpi(_context);
        UI.OnGui();
        var scale = DPI.GetUIScale();

        if (FontRebuildRequest)
        {
            _context.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, SetupFonts);
            FontRebuildRequest = false;
        }
        else
        {
            _context.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, null);
        }

        TaskManager.ThrowTaskExceptions();

        var command = EditorCommandQueue.GetNextCommand();
        if (command != null && command[0] == "windowFocus")
        {
            //this is a hack, cannot grab focus except for when un-minimising
            _user32_ShowWindow(_context.Window.Handle, 6);
            _user32_ShowWindow(_context.Window.Handle, 9);
        }

        RenderDockspace();

        UIHelper.ApplyBaseStyle();

        // Static modals go here
        ProjectCreation.Draw();

        Menubar();
        DisplayProjectList();
        DisplayProjectView();

        foreach (var projectEntry in Projects)
        {
            if (projectEntry == SelectedProject)
            {
                projectEntry.Draw(command);
            }
        }

        UIHelper.UnapplyBaseStyle();

        // Create new project if triggered to do so
        if (ProjectCreation.Create)
        {
            ProjectCreation.Create = false;
            CreateProject();
        }

        ResourceManager.UpdateTasks();
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

        var scale = DPI.GetUIScale();

        // setup fonts.
        ImGuiFontBuilder baseFont = new();

        var fontSize = UI.Current.Interface_FontSize;

        baseFont.AddDefaultFont();

        baseFont.SetOption(
            config =>
            {
                config.GlyphMinAdvanceX = 18;
                config.GlyphOffset = new(0, 4);
            }
            );

        baseFont.AddFontFromFileTTF("Assets/Fonts/SEGMDL2.TTF", fontSize, [(char)0xE700, (char)0xF800]);

        ImGuiFontBuilder japaneseFont = new();
        japaneseFont.SetOption(cfg =>
        {
            cfg.MergeMode = true;
            cfg.PixelSnapH = true;
            cfg.OversampleH = 4;
            cfg.OversampleV = 4;
        });

        // Add a font with wide Japanese character support
        japaneseFont.AddFontFromFileTTF(
            "Assets/Fonts/NotoSansMonoCJKtc-Regular.otf",
            fontSize,
            new ReadOnlySpan<uint>(new uint[]
            {
                    0x3040, 0x309F, // Hiragana
                    0x30A0, 0x30FF, // Katakana
                    0x4E00, 0x9FFF  // Common Kanji
            })
        );

        ImGuiFontBuilder icontFont = new();

        icontFont.SetOption(
            config =>
            {
                config.MergeMode = true;
                config.GlyphMinAdvanceX = 12;
                config.OversampleH = 5;
                config.OversampleV = 5;
            }
            );

        icontFont.AddFontFromFileTTF("Assets/Fonts/forkawesome-webfont.ttf", fontSize, [(char)ForkAwesome.IconMin, (char)ForkAwesome.IconMax]);

        _context.ImguiRenderer.RecreateFontDeviceTexture();
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
        //WindowHandler.ProjectDropdown();
        //EditorHandler.FileDropdown();

        //if (EditorHandler.FocusedEditor != null)
        //{
        //    EditorHandler.FocusedEditor.EditDropdown();
        //    EditorHandler.FocusedEditor.ViewDropdown();
        //    EditorHandler.FocusedEditor.EditorUniqueDropdowns();
        //}

        //WindowHandler.HelpDropdown();
        //WindowHandler.AliasDropdown();
        //WindowHandler.KeybindsDropdown();
        //WindowHandler.SettingsDropdown();
        //WindowHandler.DebugDropdown();

        //WindowHandler.SmithboxUpdateButton();

        TaskLogs.DisplayActionLoggerBar();
        TaskLogs.DisplayActionLoggerWindow();

        if (UI.Current.System_ShowActionLogger)
        {
            ImGui.Separator();
        }

        TaskLogs.DisplayWarningLoggerBar();
        TaskLogs.DisplayWarningLoggerWindow();

        if (UI.Current.System_ShowWarningLogger)
        {
            ImGui.Separator();
        }

        ImGui.EndMainMenuBar();
    }

    private void DisplayProjectList()
    {
        ImGui.Begin($"Project List##ProjectListWindow");

        DisplayProjectActions();
        DisplayProjectList();

        ImGui.End();
    }

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

    private unsafe void DisplayProjectView()
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
                    //ProjectSettings.Show(this, SelectedProject);
                }

                if (ImGui.MenuItem($"Open Project Aliases##projectAliases_{imGuiID}"))
                {
                    //ProjectAliasEditor.Show(this, SelectedProject);
                }

                if (CFG.Current.ModEngineInstall != "")
                {
                    if (ImGui.MenuItem($"Launch Mod##launchMod{imGuiID}"))
                    {
                        ModEngineLauncher.LaunchMod(SelectedProject);
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
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        if (!File.Exists(file))
        {
            ProjectDisplayConfig = new ProjectDisplayConfig();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                ProjectDisplayConfig = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectDisplayConfig);

                if (ProjectDisplayConfig == null)
                {
                    throw new Exception("[Smithbox] Failed to read Project Display.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Project Display.json");

                ProjectDisplayConfig = new ProjectDisplayConfig();
            }
        }
    }

    public void SaveProjectDisplayConfig()
    {
        var folder = FolderUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Project Display.json");

        var json = JsonSerializer.Serialize(ProjectDisplayConfig, SmithboxSerializerContext.Default.ProjectDisplayConfig);

        File.WriteAllText(file, json);
    }

    private void LoadExistingProjects()
    {
        // Read all the stored project jsons and create an existing Project dict
        var projectJsonList = FolderUtils.GetStoredProjectJsonList();

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

                    var curProject = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ProjectInstance);

                    if (curProject == null)
                    {
                        TaskLogs.AddLog($"[Smithbox] Failed to load project: {entry}", LogLevel.Warning);
                    }
                    else
                    {
                        curProject.BaseEditor = this;

                        Projects.Add(curProject);

                        if (buildProjectOrder)
                        {
                            ProjectDisplayConfig.ProjectOrder.Add(i, curProject.ProjectGUID);
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

        var newProject = new ProjectInstance(this, guid, projectName, projectPath, dataPath, projectType);

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

    //Unhappy with this being here
    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _user32_ShowWindow(nint hWnd, int nCmdShow);
}

