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
    public ProgramType ProgramType = ProgramType.Editor;

    // Updater
    public ProgramUpdater Updater;
    public DeveloperKit DeveloperKit;

    public unsafe Smithbox(string version, RenderingBackend backendType, ProgramType programType)
    {
        ProgramType = programType;
        CurrentBackend = backendType;

        Instance = this;

        _version = version;

        // The logger comes first: everything below, including the configuration loading in Setup, may log, and
        // the log windows only receive the events logged after they subscribe
        SetupLogging();

        if(ProgramType is ProgramType.Editor)
        {
            _programTitle = $"{LOC.Get("PROGRAM_TITLE")} - {_version}";
        }
        else if(ProgramType is ProgramType.Updater)
        {
            _programTitle = $"{LOC.Get("PROGRAM_UPDATER_TITLE")} - {_version}";
        }
        else if (ProgramType is ProgramType.Developer)
        {
            _programTitle = $"{LOC.Get("PROGRAM_DEVELOPER_TITLE")} - {_version}";
        }

        if (ProgramType is ProgramType.Updater)
        {
            Updater = new();
        }
        if (ProgramType is ProgramType.Developer)
        {
            DeveloperKit = new();
        }

        GUI.RestoreImguiIfMissing();
        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        Setup();

        // The bindings are loaded in Setup, before there was a logger to report on them
        InputManager.FlushPendingLogs();

        CreateGraphicsContext();

        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(_context.Window.SdlWindowHandle);

        Orchestrator = new();

        // Can be asked on the window's event thread, so this only notes the request; the main loop handles it
        _context.Window.CloseRequested = () =>
        {
            _closeRequested = true;
            return false;
        };

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

        if (ProgramType is ProgramType.Editor)
        {
            _soapstoneService = new(version);

            TextureManager = new();
        }

        if (CurrentBackend is RenderingBackend.OpenGL)
        {
            if (!CFG.Current.Block_OpenGL_Warning)
            {
                var dialog = PlatformUtils.Instance.MessageBox(
                    LOC.Get("SYS_OpenGL_Warning_Text"),
                    LOC.Get("SYS_OpenGL_Warning_Caption"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if(dialog is DialogResult.OK)
                {
                    CFG.Current.Block_OpenGL_Warning = true;
                    CFG.Save();
                }
            }
        }
    }

    /// <summary>
    /// Creates and initializes the graphics context. Vulkan falls back to OpenGL if it can't be initialized.
    /// </summary>
    private void CreateGraphicsContext()
    {
        if (CurrentBackend is RenderingBackend.Vulkan)
        {
            // Vulkan can only fail in Initialize, where the window and graphics device are created
            var vulkanContext = new VulkanGraphicsContext();

            try
            {
                vulkanContext.Initialize();
                _context = vulkanContext;

                return;
            }
            catch (Exception ex)
            {
                Smithbox.LogWarning(this,
                    LOC.Get("SYS_VULKAN_CONTEXT_FAILED"), ex);

                // A failure after the window was created leaves it open, so it is closed before the OpenGL one opens.
                // Disposing a device that failed can throw too, which must not keep the window open.
                try
                {
                    vulkanContext.Dispose();
                }
                catch (Exception disposeEx)
                {
                    Smithbox.LogWarning(this, "Failed to clean up the Vulkan context.", disposeEx);
                }
                finally
                {
                    try
                    {
                        if (vulkanContext.Window is { Exists: true })
                        {
                            vulkanContext.Window.Close();
                        }
                    }
                    catch (Exception closeEx)
                    {
                        Smithbox.LogWarning(this, "Failed to close the Vulkan window.", closeEx);
                    }
                }

                // The scene renderer was set up with the device that has just been disposed
                SceneRenderer.Device = null;

                CurrentBackend = RenderingBackend.OpenGL;
            }
        }

        _context = new OpenGLCompatGraphicsContext();
        _context.Initialize();
    }

    private void SetupLogging()
    {
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

        // The settings are read when the logs are drawn, so CFG doesn't need to be loaded yet
        ActionLogger = new(
            "LOG_Action_Log",
            evt => evt.Level is not (LogLevel.Warning or LogLevel.Error),
            //fade time is calculated to be the configured fade time in frames, but we need ms.
            () => (enabled: CFG.Current.Logger_Enable_Action_Log, fadeTime: (int)(CFG.Current.Logger_Action_Fade_Time * 1000 / 60f), fadeColor: CFG.Current.Logger_Enable_Color_Fade)
        );
        WarningLogger = new(
            "LOG_Error_Log",
            evt => evt.Level is LogLevel.Warning or LogLevel.Error && !IsHiddenFileDictionaryWarning(evt),
            () => (enabled: CFG.Current.Logger_Enable_Warning_Log, fadeTime: (int)(CFG.Current.Logger_Warning_Fade_Time * 1000 / 60f), fadeColor: CFG.Current.Logger_Enable_Color_Fade)
        );
        HighPriorityLogSubscription = TaskLogs.Subscribe(
            evt => evt.Priority == LogPriority.High
        );
    }

    /// <summary>
    /// File dictionary lookup warnings from the archive VFS (unnamed hashes, duplicate names,
    /// uncached lookups) are frequent and usually just noise, so they can be hidden.
    /// </summary>
    private static bool IsHiddenFileDictionaryWarning(LogEvent evt)
    {
        return evt.Level is LogLevel.Warning
            && evt.Category == typeof(Andre.IO.VFS.ArchiveBinderVirtualFileSystem).FullName
            && CFG.Current is { Logger_Hide_File_Dictionary_Warnings: true };
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
        // Holds the language picked on the first launch, which is otherwise only saved from the preferences
        Startup.Save();
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

    // Set while the fonts are added with the oversampling cut down, to fit the atlas on the GPU
    private bool _reduceFontOversample;

    private void SetOversample(ImFontConfigPtr cfg, int h, int v)
    {
        cfg.OversampleH = _reduceFontOversample ? 1 : h;
        cfg.OversampleV = _reduceFontOversample ? 1 : v;
    }

    private unsafe void SetupFonts()
    {
        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;

        var scaleFine = (float)Math.Round(CFG.Current.Interface_Font_Size * DPI.UIScale());

        // The CJK ranges of the additional fonts hold tens of thousands of glyphs. ImGui makes the atlas at most
        // 4096 wide and grows it in height, which at a large font size or UI scale can pass the largest texture
        // the GPU can create. Each step here makes the atlas smaller, until it fits.
        var maxSize = _context.ImguiRenderer.MaxFontTextureSize;
        var wideWidth = 1 << (int)Math.Floor(Math.Log2(maxSize));

        (int desiredWidth, bool reduceOversample, bool includeAdditional, string description)[] steps =
        [
            (0, false, true, null),
            (wideWidth, false, true, $"a {wideWidth} wide atlas"),
            (wideWidth, true, true, $"a {wideWidth} wide atlas without oversampling"),
            (wideWidth, true, false, "only the English and icon fonts, as the additional fonts don't fit"),
        ];

        for (var i = 0; i < steps.Length; i++)
        {
            var step = steps[i];

            AddFonts(fonts, scaleFine, step.desiredWidth, step.reduceOversample, step.includeAdditional);

            if (!fonts.Build())
            {
                Smithbox.LogWarning(this, "The font atlas failed to build.");
                continue;
            }

            if (fonts.TexWidth <= maxSize && fonts.TexHeight <= maxSize)
            {
                if (step.description != null)
                {
                    Smithbox.LogWarning(this,
                        $"The font atlas is larger than this GPU allows ({maxSize}) at font size {scaleFine}, " +
                        $"so it was built with {step.description}: {fonts.TexWidth}x{fonts.TexHeight}.");
                }

                break;
            }

            if (i == steps.Length - 1)
            {
                Smithbox.LogError(this,
                    $"The font atlas is {fonts.TexWidth}x{fonts.TexHeight}, larger than this GPU allows " +
                    $"({maxSize}). Lower the font size or UI scale in the preferences.");
            }
        }

        _reduceFontOversample = false;

        _context.ImguiRenderer.RecreateFontDeviceTexture();
    }

    private unsafe void AddFonts(ImFontAtlasPtr fonts, float scale, int desiredWidth, bool reduceOversample,
        bool includeAdditional)
    {
        fonts.Clear();
        fonts.TexDesiredWidth = desiredWidth;
        _reduceFontOversample = reduceOversample;

        ImFontConfigPtr cfg = ImGui.ImFontConfig();

        // The first font of an atlas can't be a merge font, so the built-in one stands in when the English font
        // can't be used. Otherwise a bad font path in the preferences would fail every launch.
        if (!AddEnglishFont(fonts, cfg, CFG.Current.English_Font, scale))
        {
            cfg.MergeMode = false;
            cfg.GlyphMinAdvanceX = 5.0f;
            SetOversample(cfg, 3, 2);
            cfg.SizePixels = scale;
            fonts.AddFontDefault(cfg);
        }

        if (includeAdditional)
        {
            AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_1, scale);
            AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_2, scale);
            AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_3, scale);
            AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_4, scale);
            AddAdditionaFont(fonts, cfg, CFG.Current.Additional_Font_5, scale);
        }

        AddIconFont(fonts, cfg, scale);

        // The atlas keeps a copy of the config of each font
        cfg.Destroy();
    }

    /// <summary>
    /// Reads a font file, or returns null (with a warning) if it is missing or isn't a TrueType/OpenType font.
    /// ImGui only parses the data when the atlas is built, where a bad file fails an assert rather than an error.
    /// </summary>
    private byte[] TryReadFontFile(string fontPath)
    {
        if (string.IsNullOrWhiteSpace(fontPath))
        {
            return null;
        }

        var fullPath = Path.Combine(AppContext.BaseDirectory, fontPath);

        if (!File.Exists(fullPath))
        {
            Smithbox.LogWarning(this, $"Font not found, so it was skipped: {fullPath}");
            return null;
        }

        byte[] data;

        try
        {
            data = File.ReadAllBytes(fullPath);
        }
        catch (Exception e)
        {
            Smithbox.LogWarning(this, $"Font could not be read, so it was skipped: {fullPath}", e);
            return null;
        }

        // The tags stb_truetype accepts: TrueType 1.0, 'true', 'OTTO' (CFF) and 'ttcf' (collections)
        var tag = data.Length >= 4 ? (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]) : 0;

        if (tag is not (0x00010000 or 0x74727565 or 0x4F54544F or 0x74746366))
        {
            Smithbox.LogWarning(this, $"File is not a TrueType or OpenType font, so it was skipped: {fullPath}");
            return null;
        }

        return data;
    }

    private unsafe bool AddEnglishFont(ImFontAtlasPtr fonts, ImFontConfigPtr cfg, string fontPath, float scale)
    {
        var englishFontData = TryReadFontFile(fontPath);

        if (englishFontData != null)
        {
            var englishFontPtr = ImGui.MemAlloc((uint)englishFontData.Length);
            Marshal.Copy(englishFontData, 0, (nint)englishFontPtr, englishFontData.Length);

            cfg.MergeMode = false;
            cfg.GlyphMinAdvanceX = 5.0f;
            SetOversample(cfg, 3, 2);

            fonts.AddFontFromMemoryTTF(englishFontPtr, englishFontData.Length, scale, cfg,
                fonts.GetGlyphRangesDefault());

            return true;
        }

        return false;
    }

    private unsafe void AddAdditionaFont(ImFontAtlasPtr fonts, ImFontConfigPtr cfg, string fontPath, float scale)
    {
        var additionalFontData = TryReadFontFile(fontPath);

        if (additionalFontData != null)
        {
            var additionalFontPtr = ImGui.MemAlloc((uint)additionalFontData.Length);
            Marshal.Copy(additionalFontData, 0, (nint)additionalFontPtr, additionalFontData.Length);

            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            SetOversample(cfg, 2, 2);

            ImFontGlyphRangesBuilderPtr glyphRanges = ImGui.ImFontGlyphRangesBuilder();
            glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());
            glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());

            Array.ForEach(InterfaceUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

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

        var iconFontData = TryReadFontFile(IconFontRelPath);

        if (iconFontData != null)
        {
            var iconFontPtr = ImGui.MemAlloc((uint)iconFontData.Length);
            Marshal.Copy(iconFontData, 0, (nint)iconFontPtr, iconFontData.Length);

            // Icon Font
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            SetOversample(cfg, 3, 3);

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

    // Set when the user closes the window; see HandleCloseRequest
    private volatile bool _closeRequested;

    /// <summary>
    /// Closes the window the user asked to close, once the unsaved changes prompt allows it. Closing with nothing
    /// unsaved happens at once, which leaves the main loop after this frame's update.
    /// </summary>
    private void HandleCloseRequest()
    {
        if (!_closeRequested)
            return;

        _closeRequested = false;

        if (ProgramType is not ProgramType.Editor || Orchestrator == null)
        {
            _context.Window.Close();
            return;
        }

        try
        {
            Orchestrator.RequestExit(() => _context.Window.Close());
        }
        catch (Exception e)
        {
            // A failed check must not keep the window from closing
            Smithbox.LogError(this, LOC.Get("UNSAVED_Check_Failed_Exit"), e);
            _context.Window.Close();
        }
    }

    public void Run()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (ProgramType is ProgramType.Editor)
        {
            if (_soapstoneService != null && Startup.Current.System_Enable_Soapstone_Server)
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
                // Clamped to the preference slider's range: a hand-edited 0 would make the frame limiter wait forever
                _desiredFrameLengthSeconds = 1.0 / Math.Clamp(CFG.Current.Viewport_Frame_Rate, 20.0f, 240.0f);
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

            // The first person mode's captured mouse is given back if nothing claimed it this frame
            StudioCore.Editors.MapEditor.FirstPersonMouse.EndFrame();

            // After the update, so a close goes straight to the check below
            HandleCloseRequest();

            // The shutdown after the loop saves everything, so closing here only leaves the loop
            if (!_context.Window.Exists)
            {
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

        // A project that crashes while loading would crash every launch if it stayed set to auto-load
        var loadingProject = Orchestrator?.LoadingProject;

        if (loadingProject?.Descriptor is { AutoSelect: true })
        {
            try
            {
                loadingProject.Descriptor.AutoSelect = false;
                Orchestrator.SaveProject(loadingProject);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to clear the auto-load of the crashed project : " + e.Message);
            }
        }

        if (!_initialLoadComplete)
        {
            // Program crashed on initial load, save the configuration so the next launch starts from it
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

        if (ProgramType is ProgramType.Editor)
        {
            Orchestrator.Update(deltaseconds, dockspaceID);
        }
        else if(ProgramType is ProgramType.Updater)
        {
            if(Updater != null)
            {
                Updater.Display(deltaseconds, dockspaceID);
            }
        }
        else if (ProgramType is ProgramType.Developer)
        {
            if (DeveloperKit != null)
            {
                DeveloperKit.Display(deltaseconds, dockspaceID);
            }
        }

        DeveloperPanel.Display(dockspaceID);

        if (DeveloperPanel.ShowDemoWindow)
        {
            ImGui.ShowDemoWindow();
        }

        KeybindsMenu.Draw();
        PreferencesMenu.Draw();

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
