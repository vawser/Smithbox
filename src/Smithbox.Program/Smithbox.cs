using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Developer;
using StudioCore.Keybinds;
using StudioCore.Logger;
using StudioCore.Logger.GUI;
using StudioCore.Preferences;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Tracy;
using Veldrid;
using Veldrid.Sdl2;
using Thread = System.Threading.Thread;

namespace StudioCore;

public class Smithbox
{
    public static Smithbox Instance { get; set; }
    public static ProjectOrchestrator Orchestrator { get; set; }
    public static TextureManager TextureManager { get; set; }

    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public IGraphicsContext _context;

    public static bool FontRebuildRequest;

    public static string _programTitle;

    public readonly string _version;

    public SoapstoneService _soapstoneService;

    public DeveloperPanel DeveloperPanel;

    public PreferencesMenu PreferencesMenu = new();
    public KeybindsMenu KeybindsMenu = new();

    public static TaskLogsProvider LogsProvider;
    public static ILogger SbLogger;
    public static ILoggerFactory SbLoggerFactory;
    public LoggerUI ActionLogger;
    public LoggerUI WarningLogger;
    public LogSubscription HighPriorityLogSubscription;

    public RenderingBackend CurrentBackend = RenderingBackend.Vulkan;

    public unsafe Smithbox(string version)
    {
        Instance = this;

        _version = version;
        _programTitle = $"{LOC.Get("PROGRAM_TITLE")} - {_version}";

        GUI.RestoreImguiIfMissing();
        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        Setup();

        if (CurrentBackend is RenderingBackend.OpenGL)
        {
            _context = new OpenGLCompatGraphicsContext();
        }

        if (CurrentBackend is RenderingBackend.Vulkan)
        {
            try
            {
                _context = new VulkanGraphicsContext();
            }
            catch (Exception ex)
            {
                Smithbox.LogWarning(this, 
                    LOC.Get("SYS_VULKAN_CONTEXT_FAILED"), ex);

                _context = new OpenGLCompatGraphicsContext();
                Startup.Current.System_RenderingBackend = RenderingBackend.OpenGL;
            }
        }

        // Set this so even if the user changes the CFG, the program won't suddenly switch its usage until a restart.
        CurrentBackend = Startup.Current.System_RenderingBackend;

        //set up logging
        if (LogsProvider is null)
        {
            LogsProvider = new TaskLogsProvider();
            SbLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(LogsProvider);
#if DEBUG
                builder.AddConsole();
#endif
            });
            SbLogger = SbLoggerFactory.CreateLogger<Smithbox>();
            SoulsFormats.Util.Logging.LoggerFactory = SbLoggerFactory;
            Andre.Core.AndreLogging.LoggerFactory = SbLoggerFactory;
        }

        ActionLogger = new(
            "LOG_Action_Log",
            evt => evt.Level is not (LogLevel.Warning or LogLevel.Error),
            //fade time is calculated to be the configured fade time in frames, but we need ms.
            () => (enabled: CFG.Current.Logger_Enable_Action_Log, fadeTime: (int)(CFG.Current.Logger_Action_Fade_Time * 1000 / 60f), fadeColor: CFG.Current.Logger_Enable_Color_Fade)
        );
        WarningLogger = new(
            "LOG_Error_Log",
            evt => evt.Level is LogLevel.Warning or LogLevel.Error,
            () => (enabled: CFG.Current.Logger_Enable_Warning_Log, fadeTime: (int)(CFG.Current.Logger_Warning_Fade_Time * 1000 / 60f), fadeColor: CFG.Current.Logger_Enable_Color_Fade)
        );
        HighPriorityLogSubscription = TaskLogs.Subscribe(
            evt => evt.Priority == LogPriority.High
        );

        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(_context.Window.SdlWindowHandle);

        Orchestrator = new();

        DPI.UpdateDpi(_context);
        DPI.UIScaleChanged += (_, _) =>
        {
            FontRebuildRequest = true;
        };

        SetupImGui();
        SetupFonts();

        _context.ImguiRenderer.OnSetupDone();

        KeybindsMenu = new();
        PreferencesMenu = new();
        DeveloperPanel = new();

        _soapstoneService = new(version);

        TextureManager = new();
    }

    private void Setup()
    {
        CFG.Setup();
        UI.Setup();
        InputManager.Init();

        CFG.Load();
        UI.Load();

        PreferencesUtil.Setup();

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        BinaryReaderEx.CurrentProjectType = "";
        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_Ignore_Read_Asserts;
        BinaryReaderEx.UseDCXHeuristicOnReadFailure = CFG.Current.System_Apply_DCX_Heuristic;

        CheckProgramUpdate();
    }


    public void SetProgramName(ProjectEntry curProject)
    {
        _context.Window.Title = $"{curProject.Descriptor.ProjectName} - {_version}";
    }
    public void ResetProgramName()
    {
        _context.Window.Title = $"Smithbox - {_version}";
    }

    public void SaveConfiguration()
    {
        using var __tracy = Profiler.TracyZoneAuto();
        Startup.Save();
        CFG.Save();
        UI.Save();
        InputManager.SaveKeybinds();
    }

    /// <summary>
    /// Called when Smithbox is shutting down
    /// </summary>
    private void Exit()
    {
        CFG.Save();
        UI.Save();
        InputManager.SaveKeybinds();
        // Shutdown Tracy profiler if running
        Profiler.Shutdown();
    }

    private unsafe void SetupImGui()
    {
        // Setup ImGui config.
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;         // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows
        io.ConfigViewportsNoAutoMerge = false;
        io.ConfigViewportsNoTaskBarIcon = false;

        // setup ImGui style
        var style = ImGui.GetStyle();
        style.TabBorderSize = 0;
    }

    private unsafe void SetupFonts()
    {
        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
        fonts.Clear();

        var scaleFine = (float)Math.Round(CFG.Current.Interface_Font_Size * DPI.UIScale());

        ImFontConfigPtr cfg = ImGui.ImFontConfig();

        AddEnglishFont(fonts, cfg, CFG.Current.English_Font, scaleFine);
        AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_1, scaleFine);
        AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_2, scaleFine);
        AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_3, scaleFine);
        AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_4, scaleFine);
        AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_5, scaleFine);
        AddIconFont(fonts, cfg, scaleFine);

        _context.ImguiRenderer.RecreateFontDeviceTexture();
    }

    private unsafe void AddEnglishFont(ImFontAtlasPtr fonts, ImFontConfigPtr cfg, string fontPath, float scale)
    {
        var englishFontPath = Path.Combine(AppContext.BaseDirectory, fontPath);

        if (File.Exists(englishFontPath))
        {
            var englishFontData = File.ReadAllBytes(englishFontPath);
            var englishFontPtr = ImGui.MemAlloc((uint)englishFontData.Length);
            Marshal.Copy(englishFontData, 0, (nint)englishFontPtr, englishFontData.Length);

            cfg.MergeMode = false;
            cfg.GlyphMinAdvanceX = 5.0f;
            cfg.OversampleH = 3;
            cfg.OversampleV = 2;

            fonts.AddFontFromMemoryTTF(englishFontPtr, englishFontData.Length, scale, cfg,
                fonts.GetGlyphRangesDefault());
        }
    }

    private unsafe void AddAdditionaFont(ImFontAtlasPtr fonts, ImFontConfigPtr cfg, string fontPath, float scale)
    {
        var additionalFontPath = Path.Combine(AppContext.BaseDirectory, fontPath);

        if (File.Exists(additionalFontPath))
        {
            var additionalFontData = File.ReadAllBytes(additionalFontPath);
            var additionalFontPtr = ImGui.MemAlloc((uint)additionalFontData.Length);
            Marshal.Copy(additionalFontData, 0, (nint)additionalFontPtr, additionalFontData.Length);

            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            cfg.OversampleH = 2;
            cfg.OversampleV = 2;

            ImFontGlyphRangesBuilderPtr glyphRanges = ImGui.ImFontGlyphRangesBuilder();
            glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());
            glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());

            Array.ForEach(InterfaceUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

            glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
            glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
            glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
            glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
            glyphRanges.AddRanges(fonts.GetGlyphRangesCyrillic());

            ImVector<uint> outGlyphRanges;
            glyphRanges.BuildRanges(&outGlyphRanges);
            fonts.AddFontFromMemoryTTF(additionalFontPtr, additionalFontData.Length, scale, cfg, outGlyphRanges.Data);
            glyphRanges.Destroy();
        }
    }

    private unsafe void AddIconFont(ImFontAtlasPtr fonts, ImFontConfigPtr cfg, float scale)
    {
        string IconFontRelPath = Path.Join("Assets", "Fonts", "forkawesome-webfont.ttf");

        var iconFontPath = Path.Combine(AppContext.BaseDirectory, IconFontRelPath);

        if (File.Exists(iconFontPath))
        {
            var iconFontData = File.ReadAllBytes(iconFontPath);
            var iconFontPtr = ImGui.MemAlloc((uint)iconFontData.Length);
            Marshal.Copy(iconFontData, 0, (nint)iconFontPtr, iconFontData.Length);

            // Icon Font
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            cfg.OversampleH = 3;
            cfg.OversampleV = 3;

            ImFontGlyphRangesBuilderPtr iconGlyphBuilder = ImGui.ImFontGlyphRangesBuilder();

            const int IconMin = 0xf000;
            const int IconMax = 0xf339;

            for (int i = IconMin; i <= IconMax; i++)
            {
                iconGlyphBuilder.AddChar((char)i);
            }

            ImVector<uint> iconGlyphRanges;
            iconGlyphBuilder.BuildRanges(&iconGlyphRanges);

            fonts.AddFontFromMemoryTTF(iconFontPtr, iconFontData.Length, scale, cfg, iconGlyphRanges.Data);
            iconGlyphBuilder.Destroy();
        }
    }

    public void Run()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (_soapstoneService != null)
        {
            TaskManager.LiveTask task = new(
                "system_setupSoapstoneServer",
                LOC.Get("SYS_Header"),
                LOC.Get("SYS_SOAPSTONE_SERVER_RUNNING"),
                LOC.Get("SYS_SOAPSTONE_SERVER_STOPPED"),
                TaskManager.RequeueType.None,
                true,
                () =>
                {
                    SoapstoneServer.RunAsync(KnownServer.DSMapStudio, _soapstoneService).Wait();
                }
            );

            TaskManager.RunPassiveTask(task);
        }

        long previousFrameTicks = 0;
        Stopwatch sw = new();
        sw.Start();

        while (_context.Window.Exists)
        {
            // Limit frame rate when window isn't focused unless we are profiling
            if (!_context.Window.Focused)
            {
                _desiredFrameLengthSeconds = 1.0 / 20.0f;
            }
            else
            {
                _desiredFrameLengthSeconds = 1.0 / CFG.Current.Viewport_Frame_Rate;
            }

            var currentFrameTicks = sw.ElapsedTicks;
            var deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

            while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
            {
                currentFrameTicks = sw.ElapsedTicks;
                deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                Thread.Sleep(_context.Window.Focused ? 0 : 1);
            }

            previousFrameTicks = currentFrameTicks;

            InputSnapshot snapshot = null;

            Sdl2Events.ProcessEvents();

            snapshot = _context.Window.PumpEvents();

            InputManager.Update(_context.Window, snapshot, deltaSeconds);

            Update((float)deltaSeconds);

            if (!_context.Window.Exists)
            {
                Orchestrator.Exit();
                Exit();

                break;
            }

            _context.Draw(Orchestrator);
            Profiler.TracyCFrameMark();
        }

        Orchestrator.Exit();
        Exit();
        ResourceManager.Shutdown();
        _context.Dispose();
    }


    //Unhappy with this being here
    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _user32_ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Called from Program.cs - Try to shutdown things gracefully on a crash
    /// </summary>
    public void CrashShutdown()
    {
        ResourceManager.Shutdown();
        _context.Dispose();
        Profiler.Shutdown();
    }

    /// <summary>
    /// Called from Program.cs - Saves modded files to a recovery directory in the mod folder on crash
    /// </summary>
    public void AttemptSaveOnCrash()
    {
        using var __tracy = Profiler.TracyZoneAuto();
        if (!_initialLoadComplete)
        {
            // Program crashed on initial load, clear recent project to let the user launch the program next time without issue.
            try
            {
                Startup.Save();
                CFG.Save();
                UI.Save();
            }
            catch (Exception e)
            {
                var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

                PlatformUtils.Instance.MessageBox(
                    LOC.Get("SYS_FAILED_TO_SAVE_CONFIG", path, e.Message, e.StackTrace),
                    LOC.Get("SYS_Error_Header"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private unsafe void Update(float deltaseconds)
    {
        DPI.UpdateDpi(_context);

        var scale = DPI.UIScale();

        if (FontRebuildRequest)
        {
            _context.ImguiRenderer.Update(deltaseconds, InputManager.InputSnapshot, SetupFonts);
            FontRebuildRequest = false;
        }
        else
        {
            _context.ImguiRenderer.Update(deltaseconds, InputManager.InputSnapshot, null);
        }

        TaskManager.ThrowTaskExceptions();

        //var commandsplit = EditorCommandQueue.GetNextCommand();
        //if (commandsplit != null && commandsplit[0] == "windowFocus")
        //{
        //    //this is a hack, cannot grab focus except for when un-minimising
        //    _user32_ShowWindow(_context.Window.Handle, 6);
        //    _user32_ShowWindow(_context.Window.Handle, 9);
        //}

        GUI.ApplyBaseStyle();

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();

        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.MenuBar;

        var dockFlags = ImGuiDockNodeFlags.PassthruCentralNode;

        if (_context is OpenGLCompatGraphicsContext glContext)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 0.1f));
            dockFlags = ImGuiDockNodeFlags.None;
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.Begin("MainDockspace_W", windowFlags);

        uint dockspaceID = ImGui.GetID("MainDockspace");

        ImGui.DockSpace(dockspaceID, Vector2.Zero, dockFlags, ref GUI.DockGroup_EditorView);

        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);

        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        // Drain logs and draw log windows
        ActionLogger.DrainMessages();
        WarningLogger.DrainMessages();

        ActionLogger.DrawWindow();
        WarningLogger.DrawWindow();
        // Show popups for high priority log events
        if (HighPriorityLogSubscription.TryDequeue(out var evt))
        {
            if (evt.Priority == LogPriority.High && CFG.Current.Logger_Enable_Log_Popups)
            {
                var popupMessage = evt.Message;

                if (evt.Exception != null)
                {
                    popupMessage += $"\n\n{LOC.Get("SYS_EXCEPTION_DETAILS")}\n{evt.Exception}";
                }

                PlatformUtils.Instance.MessageBox(popupMessage, evt.Level.ToString(), MessageBoxButtons.OK);
            }
        }

        if (ImGui.BeginMainMenuBar())
        {
            // Preferences
            if (ImGui.MenuItem($"{LOC.Get("PROGRAM_Menu_Header_Preferences")}##prefMenuHeader"))
            {
                PreferencesMenu.IsDisplayed = !PreferencesMenu.IsDisplayed;
            }

            // Keybinds
            if (ImGui.MenuItem($"{LOC.Get("PROGRAM_Menu_Header_Shortcuts")}##shortcutMenuHeader"))
            {
                KeybindsMenu.IsDisplayed = !KeybindsMenu.IsDisplayed;
            }

            // Help
            if (ImGui.BeginMenu($"{LOC.Get("PROGRAM_Menu_Header_Help")}##helpMenuHeader"))
            {
                ImGui.Text(LOC.Get("HELP_Developed_By"));
                ImGui.Text(LOC.Get("HELP_Current_Version", _version));

                ImGui.Separator();

                // Go to Wiki
                if (ImGui.MenuItem($"{LOC.Get("HELP_Action_Go_To_Wiki")}##wikiAction"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = LOC.Get("HELP_Wiki_Link");
                    myProcess.Start();
                }
                GUI.Tooltip(LOC.Get("HELP_Action_Go_To_Wiki_TT"));

                // Go to Github Repository
                if (ImGui.MenuItem($"{LOC.Get("HELP_Action_Go_To_Github")}##githubAction"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = LOC.Get("HELP_Github_Link");
                    myProcess.Start();
                }
                GUI.Tooltip(LOC.Get("HELP_Action_Go_To_Github_TT"));

                if (CFG.Current.Developer_Enable_Tools)
                {
                    DeveloperPanel.DisplayDropdown();
                }

                ImGui.EndMenu();
            }

            // draw log toggles
            ActionLogger.DrawTopBar();
            WarningLogger.DrawTopBar();

            ImGui.EndMainMenuBar();
        }

        ImGui.PopStyleVar();

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        Orchestrator.Update(deltaseconds, dockspaceID);

        DeveloperPanel.Display(dockspaceID);

        if (DeveloperPanel.ShowDemoWindow)
        {
            ImGui.ShowDemoWindow();
        }


        KeybindsMenu.Draw();
        PreferencesMenu.Draw();

        if (_programUpdateAvailable)
        {
            ImGui.Separator();

            if (ImGui.BeginMenu($"{LOC.Get("SYS_Menu_Header_Update")}##updateMenuHeader"))
            {
                if (ImGui.MenuItem($"{LOC.Get("SYS_Menu_Go_To_Release")}##releaseAction"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = _releaseUrl;
                    myProcess.Start();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();
        }

        ImGui.PopStyleVar(2);

        GUI.UnapplyBaseStyle();

        ResourceManager.UpdateTasks();

        if (!_initialLoadComplete)
        {
            if (!TaskManager.AnyActiveTasks())
            {
                _initialLoadComplete = true;
            }
        }

        if (!_firstframe)
        {
            FirstFrame = false;
        }

        _firstframe = false;
    }

    private bool _programUpdateAvailable = false;
    private string _releaseUrl = "";

    private void CheckProgramUpdate()
    {
        if (!Startup.Current.System_Check_Program_Update)
            return;

        var gitHubClient = new GitHubClient(new ProductHeaderValue(LOC.Get("GIT_REPO_NAME")));

        if (gitHubClient != null)
        {
            try
            {
                var release = gitHubClient.Repository.Release.GetLatest(
                    LOC.Get("GIT_REPO_OWNER"), 
                    LOC.Get("GIT_REPO_NAME")).Result;

                if (release != null)
                {
                    var isVer = false;
                    var verstring = "";
                    foreach (var c in release.TagName)
                    {
                        if (char.IsDigit(c) || (isVer && c == '.'))
                        {
                            verstring += c;
                            isVer = true;
                        }
                        else
                        {
                            isVer = false;
                        }
                    }

                    if (Version.Parse(verstring) > Version.Parse(_version.ToString()))
                    {
                        _programUpdateAvailable = true;
                        _releaseUrl = release.HtmlUrl;
                    }
                }
                else
                {
                    Smithbox.LogError<Smithbox>(LOC.Get("SYS_Failed_Smithbox_Release"));
                }
            }
            catch(Exception)
            {
            }
        }
        else
        {
            Smithbox.LogError<Smithbox>(LOC.Get("SYS_Failed_Smithbox_Release"));
        }
    }

#nullable enable
    /// <summary>
    /// Gets the logger for the specified type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ILogger Logger<T>()
    {
        return SbLoggerFactory.CreateLogger<T>();
    }

    public static ILogger Logger(Type type)
    {
        return SbLoggerFactory.CreateLogger(type);
    }

    public static void Log<T>(string message, LogLevel logLevel, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger<T>().LogWithPriority(logLevel, message, logPriority, exception, args);
    }

    public static void Log<T>(T _, string message, LogLevel logLevel, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Log<T>(message, logLevel, logPriority, exception, args);
    }
    public static void Log(Type type, string message, LogLevel logLevel, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger(type).LogWithPriority(logLevel, message, logPriority, exception, args);
    }

    public static void Log<T>(string message, LogLevel logLevel = LogLevel.Information, Exception? exception = null, params object?[] args)
    {
        Logger<T>().Log(logLevel, exception, message, args);
    }

    public static void Log<T>(T _, string message, LogLevel logLevel = LogLevel.Information, Exception? exception = null, params object?[] args)
    {
        Log<T>(message, logLevel, exception, args);
    }

    public static void Log(Type type, string message, LogLevel logLevel = LogLevel.Information, Exception? exception = null, params object?[] args)
    {
        Logger(type).Log(logLevel, exception, message, args);
    }

    public static void LogError<T>(string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger<T>().LogWithPriority(LogLevel.Error, message, logPriority, exception, args);
    }

    public static void LogError<T>(T _, string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        LogError<T>(message, logPriority, exception, args);
    }

    public static void LogError(Type type, string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger(type).LogWithPriority(LogLevel.Error, message, logPriority, exception, args);
    }

    public static void LogError<T>(string message, Exception? exception = null, params object?[] args)
    {
        Logger<T>().Log(LogLevel.Error, exception, message, args);
    }

    public static void LogError<T>(T _, string message, Exception? exception = null, params object?[] args)
    {
        LogError<T>(message, exception, args);
    }

    public static void LogError(Type type, string message, Exception? exception = null, params object?[] args)
    {
        Logger(type).Log(LogLevel.Error, exception, message, args);
    }

    public static void LogWarning<T>(string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger<T>().LogWithPriority(LogLevel.Warning, message, logPriority, exception, args);
    }

    public static void LogWarning<T>(T _, string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        LogWarning<T>(message, logPriority, exception, args);
    }

    public static void LogWarning(Type type, string message, LogPriority logPriority, Exception? exception = null, params object?[] args)
    {
        Logger(type).LogWithPriority(LogLevel.Warning, message, logPriority, exception, args);
    }

    public static void LogWarning<T>(string message, Exception? exception = null, params object?[] args)
    {
        Logger<T>().Log(LogLevel.Warning, exception, message, args);
    }

    public static void LogWarning<T>(T _, string message, Exception? exception = null, params object?[] args)
    {
        LogWarning<T>(message, exception, args);
    }

    public static void LogWarning(Type type, string message, Exception? exception = null, params object?[] args)
    {
        Logger(type).Log(LogLevel.Warning, exception, message, args);
    }

}
