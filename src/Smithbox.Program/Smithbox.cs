using Hexa.NET.ImGui;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Configuration.Windows;
using StudioCore.Core;
using StudioCore.DebugNS;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Configuration.Windows.HelpWindow;
using static StudioCore.Configuration.Windows.KeybindWindow;
using static StudioCore.Configuration.Windows.SettingsWindow;
using Thread = System.Threading.Thread;
using Version = System.Version;

namespace StudioCore;

public class Smithbox
{
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

    public static bool _programUpdateAvailable;
    public static string _releaseUrl = "";

    public SoapstoneService _soapstoneService;

    public static ImGuiTextureLoader TextureLoader;

    public ProjectManager ProjectManager;

    public SettingsWindow Settings;
    public HelpWindow Help;
    public KeybindWindow Keybinds;
    public DebugTools DebugTools;

    public unsafe Smithbox(IGraphicsContext context, string version, bool isLowRequirements)
    {
        LowRequirementsMode = isLowRequirements;

        _version = version;
        _programTitle = $"Smithbox - {_version}";

        TextureLoader = new ImGuiTextureLoader(context.Device, context.ImguiRenderer);

        UIHelper.RestoreImguiIfMissing();
        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        Setup();

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        // Hack to pass the current project status into the Resource system for now
        // This is not to be used outside of the resource system
        ResourceManager.BaseEditor = this;

        ProjectManager = new(this);
        ProjectManager.Setup();

        Settings = new(this);
        Help = new(this);
        Keybinds = new(this);
        DebugTools = new(this);

        _soapstoneService = new(this, version);

        SetupImGui();
        SetupFonts();

        _context.ImguiRenderer.OnSetupDone();

    }

    public void SetProgramName(ProjectEntry curProject)
    {
        _context.Window.Title = $"{curProject.ProjectName} - {_version}";
    }

    /// <summary>
    /// Called when Smithbox is starting up
    /// </summary>
    private void Setup()
    {
        CFG.Setup();
        UI.Setup();
        KeyBindings.Setup();

        CFG.Load();
        UI.Load();
        KeyBindings.Load();

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        BinaryReaderEx.CurrentProjectType = "";
        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;
        BinaryReaderEx.UseDCXHeuristicOnReadFailure = CFG.Current.System_UseDCXHeuristicOnReadFailure;
    }

    public void SaveConfiguration()
    {
        CFG.Save();
        UI.Save();
        KeyBindings.Save();
    }

    /// <summary>
    /// Called when Smithbox is shutting down
    /// </summary>
    private void Exit()
    {
        CFG.Save();
        UI.Save();
        KeyBindings.Save();
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
        string EnglishFontRelPath = @"Assets\Fonts\RobotoMono-Light.ttf";
        string NonEnglishFontRelPath = @"Assets\Fonts\NotoSansCJKtc-Light.otf";
        string IconFontRelPath = @"Assets\Fonts\forkawesome-webfont.ttf";

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

        var scaleFine = (float)Math.Round(CFG.Current.Interface_FontSize * DPI.GetUIScale());
        var scaleLarge = (float)Math.Round((CFG.Current.Interface_FontSize + 2) * DPI.GetUIScale());

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
    private void CheckProgramUpdate()
    {
        Octokit.GitHubClient gitHubClient = new(new Octokit.ProductHeaderValue("Smithbox"));
        Octokit.Release release = gitHubClient.Repository.Release.GetLatest("vawser", "Smithbox").Result;
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

        if (Version.Parse(verstring) > Version.Parse(_version))
        {
            // Update available
            _programUpdateAvailable = true;
            _releaseUrl = release.HtmlUrl;
        }
    }

    public void Run()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        if (_soapstoneService != null)
        {
            TaskManager.LiveTask task = new(
                "system_setupSoapstoneServer",
                "[System]",
                "Soapstone server is running.",
                "Soapstone server is not running.",
                TaskManager.RequeueType.None,
                false,
                () =>
                {
                    SoapstoneServer.RunAsync(KnownServer.DSMapStudio, _soapstoneService).Wait();
                }
            );

            TaskManager.RunPassiveTask(task);
        }

        if (CFG.Current.System_Check_Program_Update)
        {
            TaskManager.LiveTask task = new(
                "system_checkProgramUpdate",
                "[System]",
                "Program update check has run.",
                "Program update check has failed to run.",
                TaskManager.RequeueType.None,
                true,
                CheckProgramUpdate
            );

            TaskManager.Run(task);
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

            InputTracker.UpdateFrameInput(snapshot, _context.Window);

            Update((float)deltaSeconds);

            _context.Draw(ProjectManager);

            if (!_context.Window.Exists)
            {
                ProjectManager.Exit();
                Exit();

                break;
            }
        }

        ProjectManager.Exit();
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
                                                  $"If you continue to crash on startup, delete config in AppData\\Local\\Smithbox\n\n" +
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

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));

        ImGui.Begin("MainDockspace_W", windowFlags);

        uint dockspaceID = ImGui.GetID("MainDockspace");

        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);

        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        ProjectCreation.Draw();
        ProjectSettings.Draw();
        ProjectAliasEditor.Draw();

        // Create new project if triggered to do so
        if (ProjectCreation.Create)
        {
            ProjectCreation.Create = false;
            ProjectManager.CreateProject();
        }

        if (ImGui.BeginMainMenuBar())
        {
            // Settings
            if (ImGui.BeginMenu("Settings"))
            {
                if (ImGui.MenuItem("System"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.System);
                }
                UIHelper.Tooltip("Open the settings related to Smithbox's systems.");

                if (ImGui.MenuItem("Viewport"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.Viewport);
                }
                UIHelper.Tooltip("Open the settings related to Viewport in Smithbox.");

                if (ImGui.MenuItem("Interface"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.Interface);
                }
                UIHelper.Tooltip("Open the settings related to interface of Smithbox.");

                if (ImGui.MenuItem("Map Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.MapEditor);
                }
                UIHelper.Tooltip("Open the settings related to Map Editor in Smithbox.");

                if (ImGui.MenuItem("Model Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.ModelEditor);
                }
                UIHelper.Tooltip("Open the settings related to Model Editor in Smithbox.");

                if (ImGui.MenuItem("Param Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.ParamEditor);
                }
                UIHelper.Tooltip("Open the settings related to Param Editor in Smithbox.");

                if (ImGui.MenuItem("Text Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.TextEditor);
                }
                UIHelper.Tooltip("Open the settings related to Text Editor in Smithbox.");

                if (ImGui.MenuItem("Graphics Param Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.GparamEditor);
                }
                UIHelper.Tooltip("Open the settings related to Gparam Editor in Smithbox.");

                if (ImGui.MenuItem("Time Act Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.TimeActEditor);
                }
                UIHelper.Tooltip("Open the settings related to Time Act Editor in Smithbox.");

                if (ImGui.MenuItem("Event Script Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.EmevdEditor);
                }
                UIHelper.Tooltip("Open the settings related to Emevd Editor in Smithbox.");

                if (ImGui.MenuItem("EzState Script Editor"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.EsdEditor);
                }
                UIHelper.Tooltip("Open the settings related to Esd Editor in Smithbox.");

                if (ImGui.MenuItem("Texture Viewer"))
                {
                    Settings.ToggleWindow(SelectedSettingTab.TextureViewer);
                }
                UIHelper.Tooltip("Open the settings related to Texture Viewer in Smithbox.");

                ImGui.EndMenu();
            }

            // Keybinds
            if (ImGui.BeginMenu("Keybinds"))
            {
                if (ImGui.MenuItem("Common"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.Common);
                }
                UIHelper.Tooltip("View the common keybinds shared between all editors.");

                if (ImGui.MenuItem("Viewport"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.Viewport);
                }
                UIHelper.Tooltip("View the keybinds that apply to the viewport.");

                if (ImGui.MenuItem("Map Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.MapEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Map Editor.");

                if (ImGui.MenuItem("Model Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.ModelEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Model Editor.");

                if (ImGui.MenuItem("Param Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.ParamEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Param Editor.");

                if (ImGui.MenuItem("Text Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.TextEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Text Editor.");

                if (ImGui.MenuItem("Gparam Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.GparamEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Gparam Editor.");

                if (ImGui.MenuItem("Time Act Editor"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.TimeActEditor);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Time Act Editor.");

                if (ImGui.MenuItem("Texture Viewer"))
                {
                    Keybinds.ToggleWindow(SelectedKeybindTab.TextureViewer);
                }
                UIHelper.Tooltip("View the keybinds that apply when in the Texture Viewer.");

                ImGui.EndMenu();
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

            // Debugging
            DebugTools.DisplayMenu();

            // View
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Project List"))
                {
                    CFG.Current.Interface_Editor_ProjectList = !CFG.Current.Interface_Editor_ProjectList;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Interface_Editor_ProjectList);

                ImGui.EndMenu();
            }

            // Action Logger
            TaskLogs.DisplayActionLoggerBar();
            TaskLogs.DisplayActionLoggerWindow();

            // Warning Logger
            TaskLogs.DisplayWarningLoggerBar();
            TaskLogs.DisplayWarningLoggerWindow();

            // Program Update
            if (Smithbox._programUpdateAvailable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);
                if (ImGui.Button("Update Available"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = Smithbox._releaseUrl;
                    myProcess.Start();
                }

                ImGui.PopStyleColor();
            }

            ImGui.EndMainMenuBar();
        }

        ImGui.PopStyleVar();

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        ProjectManager.Update(deltaseconds);

        Settings.Display();
        Help.Display();
        Keybinds.Display();
        DebugTools.Display();

        // Tool windows
        ColorPicker.DisplayColorPicker();

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
}

