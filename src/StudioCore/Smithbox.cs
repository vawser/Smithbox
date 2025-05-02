using Hexa.NET.ImGui;
using Silk.NET.SDL;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Platform;
using StudioCore.Resource;
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
using Renderer = StudioCore.Scene.Renderer;
using Thread = System.Threading.Thread;
using Version = System.Version;
using StudioCore.Interface;
using StudioCore.Core;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Tasks;
using StudioCore.Tools;
using StudioCore.Core.Project;
using StudioCore.Tools.Randomiser;
using StudioCore.Editors.TextEditor;
using Hexa.NET.ImGui.Utilities;

namespace StudioCore;

public class Smithbox
{
    public static EditorHandler EditorHandler;
    public static CommonMenubarHandler WindowHandler;
    public static BankHandler BankHandler;
    public static AliasCacheHandler AliasCacheHandler;
    public static ProjectHandler ProjectHandler;

    public static ProjectType ProjectType = ProjectType.Undefined;
    public static string GameRoot = "";
    public static string ProjectRoot = "";
    public static string SmithboxDataRoot = AppContext.BaseDirectory; // Fallback directory

    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    public static IGraphicsContext _context;

    public static bool FontRebuildRequest;

    public static string _programTitle;

    private readonly string _version;

    public static bool _programUpdateAvailable;
    public static string _releaseUrl = "";

    private bool _showImGuiDebugLogWindow;

    private readonly SoapstoneService _soapstoneService;

    // ImGui Debug windows
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiStackToolWindow;

    public static ImGuiTextureLoader TextureLoader;

    public unsafe Smithbox(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Version {_version}";

        UIHelper.RestoreImguiIfMissing();

        DPI.UIScaleChanged += (_, _) =>
        {
            FontRebuildRequest = true;
        };

        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);

        CFG.AttemptLoadOrDefault();

        UI.AttemptLoadOrDefault();
        InterfaceTheme.SetupThemes();
        InterfaceTheme.SetTheme(true);

        RandomiserCFG.AttemptLoadOrDefault();

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        BinaryReaderEx.IgnoreAsserts = CFG.Current.System_IgnoreAsserts;

        // Handlers
        ProjectHandler = new ProjectHandler();
        EditorHandler = new EditorHandler(_context);
        WindowHandler = new CommonMenubarHandler(_context);

        TextBank.LoadTextFiles();

        _soapstoneService = new SoapstoneService(_version);

        SetupImGui();
        SetupFonts();

        _context.ImguiRenderer.OnSetupDone();

        TextureLoader = new ImGuiTextureLoader(context.Device, context.ImguiRenderer);
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

    public static void InitializeBanks()
    {
        BankHandler = new BankHandler();
        BankHandler.UpdateBanks();
    }

    public static void InitializeNameCaches()
    {
        AliasCacheHandler = new AliasCacheHandler();
        AliasCacheHandler.UpdateCaches();
    }

    public static void SetProgramTitle(string projectName)
    {
        _context.Window.Title = $"{projectName} - {_programTitle}";
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

        if (CFG.Current.Enable_Soapstone_Server)
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

                _context.Draw(EditorHandler.EditorList, EditorHandler.FocusedEditor);

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

        if (CFG.Current.System_EnableRecoveryFolder)
        {
            var success = ProjectHandler.CreateRecoveryProject();
            if (success)
            {
                EditorHandler.SaveAllFocusedEditor();

                PlatformUtils.Instance.MessageBox(
                    $"Attempted to save project files to {ProjectRoot} for manual recovery.\n" +
                    "You must manually replace your project files with these recovery files should you wish to restore them.\n" +
                    "Given the program has crashed, these files may be corrupt and you should backup your last good saved\n" +
                    "files before attempting to use these.",
                    "Saved recovery",
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

        var commandsplit = EditorCommandQueue.GetNextCommand();
        if (commandsplit != null && commandsplit[0] == "windowFocus")
        {
            //this is a hack, cannot grab focus except for when un-minimising
            _user32_ShowWindow(_context.Window.Handle, 6);
            _user32_ShowWindow(_context.Window.Handle, 9);
        }

        ctx = Tracy.TracyCZoneN(1, "Style");
        UIHelper.ApplyBaseStyle();
        ImGuiViewportPtr vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.Pos);
        ImGui.SetNextWindowSize(vp.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        flags |= ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.MenuBar;
        flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        flags |= ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        if (ImGui.Begin("DockSpace_W", flags))
        {

        }

        var dsid = ImGui.GetID("DockSpace");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.NoDockingSplit);
        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);
        Tracy.TracyCZoneEnd(ctx);

        ctx = Tracy.TracyCZoneN(1, "Menu");
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            WindowHandler.ProjectDropdown();
            EditorHandler.FileDropdown();

            if (EditorHandler.FocusedEditor != null)
            {
                EditorHandler.FocusedEditor.EditDropdown();
                EditorHandler.FocusedEditor.ViewDropdown();
                EditorHandler.FocusedEditor.EditorUniqueDropdowns();
            }

            WindowHandler.HelpDropdown();
            WindowHandler.AliasDropdown();
            WindowHandler.KeybindsDropdown();
            WindowHandler.SettingsDropdown();
            WindowHandler.DebugDropdown();

            WindowHandler.SmithboxUpdateButton();

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

        ImGui.PopStyleVar();
        Tracy.TracyCZoneEnd(ctx);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14.0f, 8.0f) * scale);

        // ImGui Debug windows
        if (WindowHandler.DebugWindow._showImGuiDemoWindow)
        {
            ImGui.ShowDemoWindow(ref WindowHandler.DebugWindow._showImGuiDemoWindow);
        }

        if (WindowHandler.DebugWindow._showImGuiMetricsWindow)
        {
            ImGui.ShowMetricsWindow(ref WindowHandler.DebugWindow._showImGuiMetricsWindow);
        }

        if (WindowHandler.DebugWindow._showImGuiDebugLogWindow)
        {
            ImGui.ShowDebugLogWindow(ref WindowHandler.DebugWindow._showImGuiDebugLogWindow);
        }

        if (WindowHandler.DebugWindow._showImGuiStackToolWindow)
        {
            ImGui.ShowIDStackToolWindow(ref WindowHandler.DebugWindow._showImGuiStackToolWindow);
        }

        ImGui.PopStyleVar(3);

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        ctx = Tracy.TracyCZoneN(1, "Editor");

        foreach (EditorScreen editor in EditorHandler.EditorList)
        {
            string[] commands = null;
            if (commandsplit != null && commandsplit[0] == editor.CommandEndpoint)
            {
                commands = commandsplit.Skip(1).ToArray();
                ImGui.SetNextWindowFocus();
            }

            if (_context.Device == null)
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

            if (ImGui.Begin(editor.EditorName))
            {
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                editor.OnGUI(commands);
                ImGui.End();
                EditorHandler.FocusedEditor = editor;
                editor.Update(deltaseconds);
            }
            else
            {
                // Reset this so on Focus the first frame focusing happens
                editor.OnDefocus();
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                ImGui.End();
            }
        }

        // Global shortcut keys
        if (EditorHandler.FocusedEditor != null)
        {
            if (!EditorHandler.FocusedEditor.InputCaptured())
            {
                EditorHandler.HandleEditorShortcuts();
            }
        }

        ProjectHandler.OnGui();
        WindowHandler.OnGui();

        if(BankHandler != null)
            BankHandler.OnGui();

        if(AliasCacheHandler != null)
            AliasCacheHandler.OnGui();

        ImGui.PopStyleVar(2);

        UIHelper.UnapplyBaseStyle();

        // Tool windows
        ColorPicker.DisplayColorPicker();

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

