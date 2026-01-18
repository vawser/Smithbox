using Hexa.NET.ImGui;
using Octokit;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Preferences;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Application.HelpWindow;
using static StudioCore.Application.SettingsWindow;
using Thread = System.Threading.Thread;

namespace StudioCore;

public class Smithbox
{
    public static Smithbox Instance { get; set; }
    public static ProjectOrchestrator Orchestrator { get; set; }

    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    public IGraphicsContext _context;

    public static bool FontRebuildRequest;

    public static string _programTitle;

    public readonly string _version;

    public SoapstoneService _soapstoneService;

    public HelpWindow Help;
    public DeveloperTools DebugTools;

    public PreferencesMenu PreferencesMenu = new();
    public KeybindsMenu KeybindsMenu = new();

    public ActionLogger ActionLogger;
    public WarningLogger WarningLogger;

    public unsafe Smithbox(IGraphicsContext context, string version, bool isLowRequirements)
    {
        Instance = this;

        LowRequirementsMode = isLowRequirements;

        _version = version;
        _programTitle = $"Smithbox - {_version}";

        UIHelper.RestoreImguiIfMissing();
        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        Setup();

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        Orchestrator = new();
        ActionLogger = new();
        WarningLogger = new();

        DPI.UpdateDpi(_context);
        DPI.UIScaleChanged += (_, _) =>
        {
            FontRebuildRequest = true;
        };

        SetupImGui();
        SetupFonts();

        _context.ImguiRenderer.OnSetupDone();

        Help = new();
        KeybindsMenu = new();
        PreferencesMenu = new();
        DebugTools = new();

        _soapstoneService = new(version);
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
        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;
        BinaryReaderEx.UseDCXHeuristicOnReadFailure = CFG.Current.System_UseDCXHeuristicOnReadFailure;

        CheckProgramUpdate();
    }


    public void SetProgramName(ProjectEntry curProject)
    {
        _context.Window.Title = $"{curProject.Descriptor.ProjectName} - {_version}";
    }

    public void SaveConfiguration()
    {
        CFG.Save();
        UI.Save();
        InputManager.Save();
    }

    /// <summary>
    /// Called when Smithbox is shutting down
    /// </summary>
    private void Exit()
    {
        CFG.Save();
        UI.Save();
        InputManager.Save();
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
        string EnglishFontRelPath = Path.Join("Assets", "Fonts", "RobotoMono-Light.ttf");
        string NonEnglishFontRelPath = Path.Join("Assets", "Fonts", "NotoSansCJKtc-Light.otf");
        string IconFontRelPath = Path.Join("Assets", "Fonts", "forkawesome-webfont.ttf");

        if (!string.IsNullOrWhiteSpace(CFG.Current.System_English_Font) &&
            File.Exists(CFG.Current.System_English_Font))
        {
            EnglishFontRelPath = CFG.Current.System_English_Font;
        }

        if (!string.IsNullOrWhiteSpace(CFG.Current.System_Other_Font) &&
            File.Exists(CFG.Current.System_Other_Font))
        {
            NonEnglishFontRelPath = CFG.Current.System_Other_Font;
        }

        var englishFontPath = Path.Combine(AppContext.BaseDirectory, EnglishFontRelPath);
        var englishFontData = File.ReadAllBytes(englishFontPath);
        var englishFontPtr = ImGui.MemAlloc((uint)englishFontData.Length);
        Marshal.Copy(englishFontData, 0, (nint)englishFontPtr, englishFontData.Length);

        var nonEnglishFontPath = Path.Combine(AppContext.BaseDirectory, NonEnglishFontRelPath);
        var nonEnglishFontData = File.ReadAllBytes(nonEnglishFontPath);
        var nonEnglishFontPtr = ImGui.MemAlloc((uint)nonEnglishFontData.Length);
        Marshal.Copy(nonEnglishFontData, 0, (nint)nonEnglishFontPtr, nonEnglishFontData.Length);

        var iconFontPath = Path.Combine(AppContext.BaseDirectory, IconFontRelPath);
        var iconFontData = File.ReadAllBytes(iconFontPath);
        var iconFontPtr = ImGui.MemAlloc((uint)iconFontData.Length);
        Marshal.Copy(iconFontData, 0, (nint)iconFontPtr, iconFontData.Length);

        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
        fonts.Clear();

        var scaleFine = (float)Math.Round(CFG.Current.Interface_FontSize * DPI.UIScale());
        var scaleLarge = (float)Math.Round((CFG.Current.Interface_FontSize + 2) * DPI.UIScale());

        ImFontConfigPtr cfg = ImGui.ImFontConfig();

        // Base English Font
        cfg.MergeMode = false;
        cfg.GlyphMinAdvanceX = 5.0f;
        cfg.OversampleH = 3;
        cfg.OversampleV = 2;

        fonts.AddFontFromMemoryTTF(englishFontPtr, englishFontData.Length, scaleFine, cfg,
            fonts.GetGlyphRangesDefault());

        // Non-English Font
        cfg.MergeMode = true;
        cfg.GlyphMinAdvanceX = 7.0f;
        cfg.OversampleH = 2;
        cfg.OversampleV = 2;

        ImFontGlyphRangesBuilderPtr glyphRanges = ImGui.ImFontGlyphRangesBuilder();
        glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());

        glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());

        Array.ForEach(InterfaceUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

        if (CFG.Current.System_Font_Chinese)
            glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
        if (CFG.Current.System_Font_Korean)
            glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
        if (CFG.Current.System_Font_Thai)
            glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
        if (CFG.Current.System_Font_Vietnamese)
            glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
        if (CFG.Current.System_Font_Cyrillic)
            glyphRanges.AddRanges(fonts.GetGlyphRangesCyrillic());

        ImVector<uint> outGlyphRanges;
        glyphRanges.BuildRanges(&outGlyphRanges);
        fonts.AddFontFromMemoryTTF(nonEnglishFontPtr, nonEnglishFontData.Length, scaleFine, cfg, outGlyphRanges.Data);
        glyphRanges.Destroy();

        // TODO: fix this so it works
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

        fonts.AddFontFromMemoryTTF(iconFontPtr, iconFontData.Length, scaleFine, cfg, iconGlyphRanges.Data);
        iconGlyphBuilder.Destroy();

        _context.ImguiRenderer.RecreateFontDeviceTexture();
    }

    public void Run()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (_soapstoneService != null)
        {
            TaskManager.LiveTask task = new(
                "system_setupSoapstoneServer",
                "[System]",
                "Soapstone server is running.",
                "Soapstone server is not running.",
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
                _desiredFrameLengthSeconds = 1.0 / CFG.Current.System_Frame_Rate;
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
        Tracy.Shutdown();
        ResourceManager.Shutdown();
        _context.Dispose();
    }

    /// <summary>
    /// Called from Program.cs - Saves modded files to a recovery directory in the mod folder on crash
    /// </summary>
    public void AttemptSaveOnCrash()
    {
        if (!_initialLoadComplete)
        {
            // Program crashed on initial load, clear recent project to let the user launch the program next time without issue.
            try
            {
                CFG.Save();
                UI.Save();
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Unable to save config during crash recovery.\n" +
                                                  $"If you continue to crash on startup, delete config in {Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Smithbox")}\n\n" +
                                                  $"{e.Message} {e.StackTrace}",
                    "Error",
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

        UIHelper.ApplyBaseStyle();

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

        ImGui.DockSpace(dockspaceID, Vector2.Zero, dockFlags);

        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);

        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        ActionLogger.Draw();
        WarningLogger.Draw();

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Projects"))
            {
                Orchestrator.DisplayMenuOptions();

                ImGui.EndMenu();
            }

            // Preferences
            if (ImGui.MenuItem("Preferences"))
            {
                PreferencesMenu.IsDisplayed = !PreferencesMenu.IsDisplayed;
            }

            // Keybinds
            if (ImGui.MenuItem("Shortcuts"))
            {
                KeybindsMenu.IsDisplayed = !KeybindsMenu.IsDisplayed;
            }

            // Help
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Articles"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Articles);
                }
                UIHelper.Tooltip("View the articles that relate to this project.");

                if (ImGui.MenuItem("Tutorials"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Tutorials);
                }
                UIHelper.Tooltip("View the tutorials that relate to this project.");

                if (ImGui.MenuItem("Glossary"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Glossary);
                }
                UIHelper.Tooltip("View the glossary that relate to this project.");

                if (ImGui.MenuItem("Mass Edit"))
                {
                    Help.ToggleWindow(SelectedHelpTab.MassEdit);
                }
                UIHelper.Tooltip("View the mass edit help instructions.");

                if (ImGui.MenuItem("Regex"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Regex);
                }
                UIHelper.Tooltip("View the regex help instructions.");

                if (ImGui.MenuItem("Links"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Links);
                }
                UIHelper.Tooltip("View the community links.");

                if (ImGui.MenuItem("Credits"))
                {
                    Help.ToggleWindow(SelectedHelpTab.Credits);
                }
                UIHelper.Tooltip("View the credits.");

                ImGui.EndMenu();
            }

#if DEBUG
            // Debugging
            DebugTools.DisplayMenu();
#endif

            // Action Logger
            ActionLogger.DisplayTopbarToggle();

            // Warning Logger
            WarningLogger.DisplayTopbarToggle();

            ImGui.EndMainMenuBar();
        }

        ImGui.PopStyleVar();

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        Orchestrator.Update(deltaseconds);

        Help.Display();
        DebugTools.Display();

        KeybindsMenu.Draw();
        PreferencesMenu.Draw();

        // Tool windows
        ColorPicker.DisplayColorPicker();

        if (_programUpdateAvailable)
        {
            ImGui.Separator();

            if (ImGui.BeginMenu("Update"))
            {
                if (ImGui.MenuItem("Go to Release"))
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

        UIHelper.UnapplyBaseStyle();

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
        if (!CFG.Current.System_Check_Program_Update)
            return;

        try
        {
            GitHubClient gitHubClient = new(new ProductHeaderValue("Smithbox"));
            Release release = gitHubClient.Repository.Release.GetLatest("vawser", "Smithbox").Result;
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
        catch (Exception) { }
    }
}

