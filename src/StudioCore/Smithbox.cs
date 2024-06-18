using ImGuiNET;
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
using StudioCore.Editors.AssetBrowser;
using StudioCore.Core;

namespace StudioCore;

public class Smithbox
{
    public static EditorHandler EditorHandler;
    public static WindowHandler WindowHandler;
    public static BankHandler BankHandler;
    public static NameCacheHandler NameCacheHandler;
    public static ProjectHandler ProjectHandler;

    public static ProjectType ProjectType = ProjectType.Undefined;
    public static string GameRoot = "";
    public static string ProjectRoot = "";
    public static string SmithboxDataRoot = "";

    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    public static IGraphicsContext _context;

    public static GraphicsDevice _graphicsDevice;

    public static bool FontRebuildRequest;

    public static string _programTitle;

    private readonly SoapstoneService _soapstoneService;
    private readonly string _version;

    private bool _programUpdateAvailable;
    private string _releaseUrl = "";
    private bool _showImGuiDebugLogWindow;

    // ImGui Debug windows
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiStackToolWindow;

    public static EventHandler UIScaleChanged;

    public static bool ShowNewProjectModal = false;

    public unsafe Smithbox(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Version {_version}";

        ImguiUtils.RestoreImguiIfMissing();

        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);
        CFG.AttemptLoadOrDefault();
        UI.SetupThemes();
        UI.SetTheme(true);

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;
        _graphicsDevice = context.Device;

        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        // SoulsFormats toggles
        BinaryReaderEx.IsFlexible = CFG.Current.System_FlexibleUnpack;

        // Project
        ProjectHandler = new ProjectHandler();
        ProjectHandler.LoadLastProject();
        ProjectHandler.UpdateProjectVariables();

        // Banks
        BankHandler = new BankHandler();
        BankHandler.UpdateBanks();
        BankHandler.SelectionGroups.CreateSelectionGroups();

        // Name Caches
        NameCacheHandler = new NameCacheHandler();
        NameCacheHandler.UpdateCaches();

        // Editors
        EditorHandler = new EditorHandler(_context);

        // Windows
        WindowHandler = new WindowHandler(_context);

        // Soapstone Service
        _soapstoneService = new SoapstoneService(_version);

        // Load CurrentProject Project
        ProjectHandler.LoadProject(ProjectHandler.CurrentProject.ProjectJsonPath);
        if (ProjectType != ProjectType.Undefined)
        {
            ProjectHandler.UpdateTimer();
        }

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        SetupFonts();
        _context.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;

    }

    public static void SetProgramTitle(string projectName)
    {
        _context.Window.Title = $"{projectName} - {_programTitle}";
    }

    private unsafe void SetupFonts()
    {
        ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
        var fileEn = Path.Combine(AppContext.BaseDirectory, @"Assets\Fonts\RobotoMono-Light.ttf");
        var fontEn = File.ReadAllBytes(fileEn);
        var fontEnNative = ImGui.MemAlloc((uint)fontEn.Length);
        Marshal.Copy(fontEn, 0, fontEnNative, fontEn.Length);
        var fileOther = Path.Combine(AppContext.BaseDirectory, @"Assets\Fonts\NotoSansCJKtc-Light.otf");
        var fontOther = File.ReadAllBytes(fileOther);
        var fontOtherNative = ImGui.MemAlloc((uint)fontOther.Length);
        Marshal.Copy(fontOther, 0, fontOtherNative, fontOther.Length);
        var fileIcon = Path.Combine(AppContext.BaseDirectory, @"Assets\Fonts\forkawesome-webfont.ttf");
        var fontIcon = File.ReadAllBytes(fileIcon);
        var fontIconNative = ImGui.MemAlloc((uint)fontIcon.Length);
        Marshal.Copy(fontIcon, 0, fontIconNative, fontIcon.Length);
        fonts.Clear();

        var scale = GetUIScale();

        // English fonts
        {
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.GlyphMinAdvanceX = 5.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;
            fonts.AddFontFromMemoryTTF(fontEnNative, fontIcon.Length, 14.0f * scale, cfg,
                fonts.GetGlyphRangesDefault());
        }

        // Other language fonts
        {
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 7.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;

            ImFontGlyphRangesBuilderPtr glyphRanges =
                new(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            glyphRanges.AddRanges(fonts.GetGlyphRangesJapanese());
            Array.ForEach(FontUtils.SpecialCharsJP, c => glyphRanges.AddChar(c));

            if (CFG.Current.System_Font_Chinese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
            }

            if (CFG.Current.System_Font_Korean)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
            }

            if (CFG.Current.System_Font_Thai)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
            }

            if (CFG.Current.System_Font_Vietnamese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
            }

            if (CFG.Current.System_Font_Cyrillic)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesCyrillic());
            }

            glyphRanges.BuildRanges(out ImVector glyphRange);
            fonts.AddFontFromMemoryTTF(fontOtherNative, fontOther.Length, 16.0f * scale, cfg, glyphRange.Data);
            glyphRanges.Destroy();
        }

        // Icon fonts
        {
            ushort[] ranges = { ForkAwesome.IconMin, ForkAwesome.IconMax, 0 };
            ImFontConfig* ptr = ImGuiNative.ImFontConfig_ImFontConfig();
            ImFontConfigPtr cfg = new(ptr);
            cfg.MergeMode = true;
            cfg.GlyphMinAdvanceX = 12.0f;
            cfg.OversampleH = 5;
            cfg.OversampleV = 5;
            ImFontGlyphRangesBuilder b = new();

            fixed (ushort* r = ranges)
            {
                ImFontPtr f = fonts.AddFontFromMemoryTTF(fontIconNative, fontIcon.Length, 16.0f * scale, cfg,
                    (IntPtr)r);
            }
        }

        _context.ImguiRenderer.RecreateFontDeviceTexture();
    }

    public void SetupCSharpDefaults()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
        SetupCSharpDefaults();

        if (CFG.Current.System_Enable_Soapstone_Server)
        {
            TaskManager.RunPassiveTask(new TaskManager.LiveTask("Soapstone Server",
                TaskManager.RequeueType.None, true,
                () => SoapstoneServer.RunAsync(KnownServer.Smithbox, _soapstoneService).Wait()));
        }

        if (CFG.Current.System_Check_Program_Update)
        {
            TaskManager.Run(new TaskManager.LiveTask("Check Program Updates",
                TaskManager.RequeueType.None, true,
                () => CheckProgramUpdate()));
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
    }

    public void ApplyStyle()
    {
        var scale = GetUIScale();
        ImGuiStylePtr style = ImGui.GetStyle();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.ImGui_MainBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, CFG.Current.ImGui_PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, CFG.Current.ImGui_Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Background);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, CFG.Current.ImGui_Input_Background_Hover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, CFG.Current.ImGui_Input_Background_Active);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.ImGui_TitleBarBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.ImGui_TitleBarBg_Active);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, CFG.Current.ImGui_MenuBarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, CFG.Current.ImGui_ScrollbarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, CFG.Current.ImGui_ScrollbarGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, CFG.Current.ImGui_ScrollbarGrab_Hover);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, CFG.Current.ImGui_ScrollbarGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, CFG.Current.ImGui_Input_CheckMark);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, CFG.Current.ImGui_SliderGrab);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, CFG.Current.ImGui_SliderGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.Button, CFG.Current.ImGui_Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, CFG.Current.ImGui_Button_Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, CFG.Current.ImGui_ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.ImGui_Selection);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, CFG.Current.ImGui_Selection_Hover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, CFG.Current.ImGui_Selection_Active);
        ImGui.PushStyleColor(ImGuiCol.Tab, CFG.Current.ImGui_Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, CFG.Current.ImGui_Tab_Hover);
        ImGui.PushStyleColor(ImGuiCol.TabActive, CFG.Current.ImGui_Tab_Active);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, CFG.Current.ImGui_UnfocusedTab);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, CFG.Current.ImGui_UnfocusedTab_Active);

        // Sizes
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0.0f);

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 16.0f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(100f, 100f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, style.CellPadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, style.IndentSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, style.ItemInnerSpacing * scale);
    }

    public void UnapplyStyle()
    {
        ImGui.PopStyleColor(29);
        ImGui.PopStyleVar(10);
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

        UpdateDpi();
        var scale = GetUIScale();

        BankHandler.OnGui();
        NameCacheHandler.OnGui();

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
        ApplyStyle();
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
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.NoSplit);
        ImGui.PopStyleVar(1);
        ImGui.End();
        ImGui.PopStyleColor(1);
        Tracy.TracyCZoneEnd(ctx);

        ctx = Tracy.TracyCZoneN(1, "Menu");
        ShowNewProjectModal = false;
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            EditorHandler.HandleEditorSharedBar();
            EditorHandler.FocusedEditor.DrawEditorMenu();

            TaskLogs.Display();

            ImGui.Separator();

            WindowHandler.HandleWindowIconBar();

            // Program Update
            if (_programUpdateAvailable)
            {
                ImGui.Separator();

                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Benefit_Text_Color);
                if (ImGui.Button("Update Available"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = _releaseUrl;
                    myProcess.Start();
                }

                ImGui.PopStyleColor();
            }

            ImGui.EndMainMenuBar();
        }

        WindowHandler.OnGui();

        ImGui.PopStyleVar();
        Tracy.TracyCZoneEnd(ctx);

        var open = true;
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
            ImGui.ShowStackToolWindow(ref WindowHandler.DebugWindow._showImGuiStackToolWindow);
        }

        // New project modal
        if (ShowNewProjectModal)
        {
            ImGui.OpenPopup("New Project");
        }

        if (ImGui.BeginPopupModal("New Project", ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ProjectHandler.NewProjectModal.CreateNewProjectModal();

            ImGui.EndPopup();
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
                editor.FirstFrame = true;
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                ImGui.End();
            }
        }

        // Global shortcut keys
        if (!EditorHandler.FocusedEditor.InputCaptured())
        {
            EditorHandler.HandleEditorShortcuts();
            WindowHandler.HandleWindowShortcuts();
        }

        ImGui.PopStyleVar(2);
        UnapplyStyle();
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

    private const float DefaultDpi = 96f;
    private static float _dpi = DefaultDpi;

    private static float Dpi
    {
        get => _dpi;
        set
        {
            if (Math.Abs(_dpi - value) < 0.0001f) return; // Skip doing anything if no difference

            if (Math.Abs(value - _dpi) > 0.9f)
                FontRebuildRequest = true;
            _dpi = value;
            UIScaleChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    private static unsafe void UpdateDpi()
    {
        if (SdlProvider.SDL.IsValueCreated && _context?.Window != null)
        {
            var window = _context.Window.SdlWindowHandle;
            int index = SdlProvider.SDL.Value.GetWindowDisplayIndex(window);
            float ddpi = 96f;
            float _ = 0f;
            SdlProvider.SDL.Value.GetDisplayDPI(index, ref ddpi, ref _, ref _);

            Dpi = ddpi;
        }
    }

    public static float GetUIScale()
    {
        return CFG.Current.System_UI_Scale / DefaultDpi * Dpi;
    }
}

