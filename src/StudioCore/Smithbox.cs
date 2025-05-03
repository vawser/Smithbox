using Hexa.NET.ImGui;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Configuration.Help;
using StudioCore.Configuration.Keybinds;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Configuration.Help.HelpWindow;
using static StudioCore.Configuration.Keybinds.KeybindWindow;
using Renderer = StudioCore.Scene.Renderer;
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

    public bool _showImGuiDebugLogWindow;

    public SoapstoneService _soapstoneService;

    public static ImGuiTextureLoader TextureLoader;

    public ProjectManager ProjectManager;

    public SettingsWindow Settings;
    public HelpWindow Help;
    public KeybindWindow Keybinds;

    public unsafe Smithbox(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Version {_version}";

        TextureLoader = new ImGuiTextureLoader(context.Device, context.ImguiRenderer);

        UIHelper.RestoreImguiIfMissing();
        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        Setup();

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        ProjectManager = new(this);

        Settings = new(this);
        Help = new(this);
        Keybinds = new(this);

        SetupImGui();
        SetupFonts();

        _context.ImguiRenderer.OnSetupDone();

    }

    private void Setup()
    {
        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        CFG.AttemptLoadOrDefault();
        UI.AttemptLoadOrDefault();
        InterfaceTheme.SetupThemes();
        InterfaceTheme.SetTheme(true);

        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;
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

        if (!string.IsNullOrWhiteSpace(UI.Current.System_English_Font) &&
            File.Exists(UI.Current.System_English_Font))
        {
            EnglishFontRelPath = UI.Current.System_English_Font;
        }

        if (!string.IsNullOrWhiteSpace(UI.Current.System_Other_Font) &&
            File.Exists(UI.Current.System_Other_Font))
        {
            NonEnglishFontRelPath = UI.Current.System_Other_Font;
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

        var scaleFine = (float)Math.Round(UI.Current.Interface_FontSize * DPI.GetUIScale());
        var scaleLarge = (float)Math.Round((UI.Current.Interface_FontSize + 2) * DPI.GetUIScale());

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

        if (UI.Current.System_Font_Chinese)
            glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
        if (UI.Current.System_Font_Korean)
            glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
        if (UI.Current.System_Font_Thai)
            glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
        if (UI.Current.System_Font_Vietnamese)
            glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
        if (UI.Current.System_Font_Cyrillic)
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

        ushort[] iconRangesRaw = { ForkAwesome.IconMin, ForkAwesome.IconMax, 0 };

        ImFontGlyphRangesBuilderPtr iconGlyphBuilder = ImGui.ImFontGlyphRangesBuilder();
        fixed (ushort* r = iconRangesRaw)
        {
            iconGlyphBuilder.AddRanges((uint*)r);
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

        if (CFG.Current.Enable_Soapstone_Server && _soapstoneService != null)
        {
            TaskManager.LiveTask task = new(
                "system_setupSoapstoneServer",
                "System",
                "soapstone server is running.",
                "soapstone server is not running.",
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
                "System",
                "program update check has run.",
                "program update check has failed to run.",
                TaskManager.RequeueType.None,
                true,
                CheckProgramUpdate
            );

            TaskManager.Run(task);
        }

        long previousFrameTicks = 0;
        Stopwatch sw = new();
        sw.Start();
        Tracy.Startup();
        while (_context.Window.Exists)
        {
            Tracy.TracyCFrameMark();

            // Limit frame rate when window isn't focused unless we are profiling
            var focused = Tracy.EnableTracy ? true : _context.Window.Focused;
            if (!focused)
            {
                _desiredFrameLengthSeconds = 1.0 / 20.0f;
            }
            else
            {
                _desiredFrameLengthSeconds = 1.0 / CFG.Current.System_Frame_Rate;
            }

            var currentFrameTicks = sw.ElapsedTicks;
            var deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneNC(1, "Sleep", 0xFF0000FF);
            while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
            {
                currentFrameTicks = sw.ElapsedTicks;
                deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                Thread.Sleep(focused ? 0 : 1);
            }

            Tracy.TracyCZoneEnd(ctx);

            previousFrameTicks = currentFrameTicks;

            ctx = Tracy.TracyCZoneNC(1, "Update", 0xFF00FF00);
            InputSnapshot snapshot = null;

            Sdl2Events.ProcessEvents();

            snapshot = _context.Window.PumpEvents();

            InputTracker.UpdateFrameInput(snapshot, _context.Window);

            Update((float)deltaSeconds);

            Tracy.TracyCZoneEnd(ctx);

            if (!_context.Window.Exists)
            {
                break;
            }

            if (true) //_window.Focused)
            {
                ctx = Tracy.TracyCZoneNC(1, "Draw", 0xFFFF0000);

                _context.Draw(ProjectManager);

                Tracy.TracyCZoneEnd(ctx);
            }
            else
            {
                // Flush the background queues
                Renderer.Frame(null, true);
            }
        }

        //DestroyAllObjects();
        Tracy.Shutdown();
        ResourceManager.Shutdown();
        _context.Dispose();
        CFG.Save();
        UI.Save();
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
                CFG.Current.LastProjectFile = "";
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
        Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "Imgui");

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

        Tracy.TracyCZoneEnd(ctx);
        TaskManager.ThrowTaskExceptions();

        //var commandsplit = EditorCommandQueue.GetNextCommand();
        //if (commandsplit != null && commandsplit[0] == "windowFocus")
        //{
        //    //this is a hack, cannot grab focus except for when un-minimising
        //    _user32_ShowWindow(_context.Window.Handle, 6);
        //    _user32_ShowWindow(_context.Window.Handle, 9);
        //}

        ctx = Tracy.TracyCZoneN(1, "Style");

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
        Tracy.TracyCZoneEnd(ctx);

        ctx = Tracy.TracyCZoneN(1, "Menu");
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            // Settings
            if (ImGui.MenuItem("Settings"))
            {
                Settings.ToggleMenuVisibility();
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
        Tracy.TracyCZoneEnd(ctx);

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        ctx = Tracy.TracyCZoneN(1, "Editor");

        ProjectManager.Update(deltaseconds);

        Settings.Display();
        Help.Display();
        Keybinds.Display();

        // Tool windows
        ColorPicker.DisplayColorPicker();

        ImGui.PopStyleVar(2);

        UIHelper.UnapplyBaseStyle();

        Tracy.TracyCZoneEnd(ctx);

        ctx = Tracy.TracyCZoneN(1, "Resource");
        ResourceManager.UpdateTasks();
        Tracy.TracyCZoneEnd(ctx);

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

