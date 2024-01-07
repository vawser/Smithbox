using ImGuiNET;
using Microsoft.Extensions.Logging;
using Octokit;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Graphics;
using StudioCore.Help;
using StudioCore.MsbEditor;
using StudioCore.ParamEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Tests;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore;

public class MapStudioNew
{
    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    private readonly AssetLocator _assetLocator;

    private readonly IGraphicsContext _context;

    private readonly List<EditorScreen> _editors;
    private readonly HelpBrowser _helpBrowser;

    private readonly NewProjectOptions _newProjectOptions = new();
    private readonly string _programTitle;
    private readonly SettingsMenu _settingsMenu = new();

    private readonly SoapstoneService _soapstoneService;
    private readonly string _version;

    /// <summary>
    ///     Characters to load that FromSoft use, but aren't included in the ImGui Japanese glyph range.
    /// </summary>
    private readonly char[] SpecialCharsJP =
    {
        '鉤', '梟', '倅', '…', '飴', '護', '戮', 'ā', 'ī', 'ū', 'ē', 'ō', 'Ā', 'Ē', 'Ī', 'Ō', 'Ū', '—', '薄', '靄'
    };

    private EditorScreen _focusedEditor;

    private bool _programUpdateAvailable;
    private ProjectSettings _projectSettings;
    private string _releaseUrl = "";
    private bool _showImGuiDebugLogWindow;

    // ImGui Debug windows
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiStackToolWindow;

    private bool _standardProjectUIOpened = true;

    public unsafe MapStudioNew(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Dark Souls Map Studio version {_version}";

        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);
        CFG.AttemptLoadOrDefault();

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;
        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        _assetLocator = new AssetLocator();
        MsbEditorScreen msbEditor = new(_context.Window, _context.Device, _assetLocator);
        ModelEditorScreen modelEditor = new(_context.Window, _context.Device, _assetLocator);
        ParamEditorScreen paramEditor = new(_context.Window, _context.Device, _assetLocator);
        TextEditorScreen textEditor = new(_context.Window, _context.Device, _assetLocator);
        _editors = new List<EditorScreen> { msbEditor, modelEditor, paramEditor, textEditor };
        _focusedEditor = msbEditor;

        _soapstoneService = new SoapstoneService(_version, _assetLocator, msbEditor);

        _settingsMenu.MsbEditor = msbEditor;
        _settingsMenu.ModelEditor = modelEditor;
        _settingsMenu.ParamEditor = paramEditor;
        _settingsMenu.TextEditor = textEditor;

        _helpBrowser = new HelpBrowser("HelpBrowser", _assetLocator);

        AliasBank.SetAssetLocator(_assetLocator);
        ParamBank.PrimaryBank.SetAssetLocator(_assetLocator);
        ParamBank.VanillaBank.SetAssetLocator(_assetLocator);
        FMGBank.SetAssetLocator(_assetLocator);
        MtdBank.LoadMtds(_assetLocator);

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        SetupFonts();
        _context.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;

        if (CFG.Current.LastProjectFile != null && CFG.Current.LastProjectFile != "")
        {
            if (File.Exists(CFG.Current.LastProjectFile))
            {
                ProjectSettings settings = ProjectSettings.Deserialize(CFG.Current.LastProjectFile);
                if (settings == null)
                {
                    CFG.Current.LastProjectFile = "";
                    CFG.Save();
                }
                else
                {
                    try
                    {
                        AttemptLoadProject(settings, CFG.Current.LastProjectFile);
                    }
                    catch
                    {
                        CFG.Current.LastProjectFile = "";
                        CFG.Save();
                        PlatformUtils.Instance.MessageBox(
                            "Failed to load last project. Project will not be loaded after restart.",
                            "Project Load Error", MessageBoxButtons.OK);
                        throw;
                    }
                }
            }
            else
            {
                CFG.Current.LastProjectFile = "";
                CFG.Save();
                TaskLogs.AddLog($"Cannot load project: \"{CFG.Current.LastProjectFile}\" does not exist.",
                    LogLevel.Warning, TaskLogs.LogPriority.High);
            }
        }
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
            Array.ForEach(SpecialCharsJP, c => glyphRanges.AddChar(c));

            if (CFG.Current.FontChinese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesChineseFull());
            }

            if (CFG.Current.FontKorean)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesKorean());
            }

            if (CFG.Current.FontThai)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesThai());
            }

            if (CFG.Current.FontVietnamese)
            {
                glyphRanges.AddRanges(fonts.GetGlyphRangesVietnamese());
            }

            if (CFG.Current.FontCyrillic)
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
        GitHubClient gitHubClient = new(new ProductHeaderValue("DSMapStudio"));
        Release release = gitHubClient.Repository.Release.GetLatest("soulsmods", "DSMapStudio").Result;
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

    public void ManageImGuiConfigBackups()
    {
        if (!File.Exists("imgui.ini"))
        {
            if (File.Exists("imgui.ini.backup"))
            {
                File.Copy("imgui.ini.backup", "imgui.ini");
            }
        }
        else if (!File.Exists("imgui.ini.backup"))
        {
            if (File.Exists("imgui.ini"))
            {
                File.Copy("imgui.ini", "imgui.ini.backup");
            }
        }
    }

    public void Run()
    {
        SetupCSharpDefaults();
        ManageImGuiConfigBackups();

        if (CFG.Current.EnableSoapstone)
        {
            TaskManager.RunPassiveTask(new TaskManager.LiveTask("Soapstone Server",
                TaskManager.RequeueType.None, true,
                () => SoapstoneServer.RunAsync(KnownServer.DSMapStudio, _soapstoneService).Wait()));
        }

        if (CFG.Current.EnableCheckProgramUpdate)
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
                _desiredFrameLengthSeconds = 1.0 / 60.0f;
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
                _context.Draw(_editors, _focusedEditor);
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

    // Try to shutdown things gracefully on a crash
    public void CrashShutdown()
    {
        Tracy.Shutdown();
        ResourceManager.Shutdown();
        _context.Dispose();
    }

    private void ChangeProjectSettings(ProjectSettings newsettings, string moddir, NewProjectOptions options)
    {
        _projectSettings = newsettings;
        _assetLocator.SetFromProjectSettings(newsettings, moddir);
        _settingsMenu.ProjSettings = _projectSettings;

        AliasBank.ReloadAliases();
        ParamBank.ReloadParams(newsettings, options);
        MtdBank.ReloadMtds();

        foreach (EditorScreen editor in _editors)
        {
            editor.OnProjectChanged(_projectSettings);
        }
    }

    public void ApplyStyle()
    {
        var scale = GetUIScale();
        ImGuiStylePtr style = ImGui.GetStyle();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        //ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.145f, 0.145f, 0.149f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.106f, 0.106f, 0.110f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.247f, 0.247f, 0.275f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.200f, 0.200f, 0.216f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.247f, 0.247f, 0.275f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.200f, 0.200f, 0.216f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TitleBg, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0.243f, 0.243f, 0.249f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0.408f, 0.408f, 0.408f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.635f, 0.635f, 0.635f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(1.000f, 1.000f, 1.000f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(1.000f, 1.000f, 1.000f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, new Vector4(0.635f, 0.635f, 0.635f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, new Vector4(1.000f, 1.000f, 1.000f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.247f, 0.247f, 0.275f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.200f, 0.600f, 1.000f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.000f, 0.478f, 0.800f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.247f, 0.247f, 0.275f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.161f, 0.550f, 0.939f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.110f, 0.592f, 0.918f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.200f, 0.600f, 1.000f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, new Vector4(0.176f, 0.176f, 0.188f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, new Vector4(0.247f, 0.247f, 0.275f, 1.0f));

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
        ImGui.PopStyleColor(27);
        ImGui.PopStyleVar(10);
    }

    private void DumpFlverLayouts()
    {
        if (PlatformUtils.Instance.SaveFileDialog("Save Flver layout dump", new[] { AssetLocator.TxtFilter },
                out var path))
        {
            using (StreamWriter file = new(path))
            {
                foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
                {
                    file.WriteLine(mat.Key + ":");
                    foreach (FLVER.LayoutMember member in mat.Value)
                    {
                        file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                    }

                    file.WriteLine();
                }
            }
        }
    }

    private bool GameNotUnpackedWarning(GameType gameType)
    {
        if (gameType is GameType.DarkSoulsPTDE or GameType.DarkSoulsIISOTFS)
        {
            TaskLogs.AddLog(
                $"The files for {gameType} do not appear to be unpacked. Please use UDSFM for DS1:PTDE and UXM for DS2 to unpack game files",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return false;
        }

        TaskLogs.AddLog(
            $"The files for {gameType} do not appear to be fully unpacked. Functionality will be limited. Please use UXM selective unpacker to unpack game files",
            LogLevel.Warning);
        return true;
    }

    private bool AttemptLoadProject(ProjectSettings settings, string filename, NewProjectOptions options = null)
    {
        var success = true;
        // Check if game exe exists
        if (!Directory.Exists(settings.GameRoot))
        {
            success = false;
            PlatformUtils.Instance.MessageBox(
                $@"Could not find game data directory for {settings.GameType}. Please select the game executable.",
                "Error",
                MessageBoxButtons.OK);

            while (true)
            {
                if (PlatformUtils.Instance.OpenFileDialog(
                        $"Select executable for {settings.GameType}...",
                        new[] { AssetLocator.GameExecutableFilter },
                        out var path))
                {
                    settings.GameRoot = path;
                    GameType gametype = _assetLocator.GetGameTypeForExePath(settings.GameRoot);
                    if (gametype == settings.GameType)
                    {
                        success = true;
                        settings.GameRoot = Path.GetDirectoryName(settings.GameRoot);
                        if (settings.GameType == GameType.Bloodborne)
                        {
                            settings.GameRoot += @"\dvdroot_ps4";
                        }

                        settings.Serialize(filename);
                        break;
                    }

                    PlatformUtils.Instance.MessageBox(
                        $@"Selected executable was not for {settings.GameType}. Please select the correct game executable.",
                        "Error",
                        MessageBoxButtons.OK);
                }
                else
                {
                    break;
                }
            }
        }

        if (success)
        {
            if (!_assetLocator.CheckFilesExpanded(settings.GameRoot, settings.GameType))
            {
                if (!GameNotUnpackedWarning(settings.GameType))
                {
                    return false;
                }
            }

            if (settings.GameType == GameType.Sekiro || settings.GameType == GameType.EldenRing)
            {
                if (!StealGameDllIfMissing(settings, "oo2core_6_win64"))
                {
                    return false;
                }
            }
            else if (settings.GameType == GameType.ArmoredCoreVI)
            {
                if (!StealGameDllIfMissing(settings, "oo2core_8_win64"))
                {
                    return false;
                }
            }

            _projectSettings = settings;
            ChangeProjectSettings(_projectSettings, Path.GetDirectoryName(filename), options);
            _context.Window.Title = $"{_programTitle}  -  {_projectSettings.ProjectName}";

            CFG.RecentProject recent = new()
            {
                Name = _projectSettings.ProjectName,
                GameType = _projectSettings.GameType,
                ProjectFile = filename
            };
            CFG.AddMostRecentProject(recent);
        }

        return success;
    }

    private bool StealGameDllIfMissing(ProjectSettings settings, string dllName)
    {
        dllName = dllName + ".dll";
        if (File.Exists(Path.Join(Path.GetFullPath("."), dllName)))
        {
            return true;
        }

        if (!File.Exists(Path.Join(settings.GameRoot, dllName)))
        {
            PlatformUtils.Instance.MessageBox(
                $"Could not find file \"{dllName}\" in \"{settings.GameRoot}\", which should be included by default.\n\nTry verifying or reinstalling the game.",
                "Error",
                MessageBoxButtons.OK);
            return false;
        }

        File.Copy(Path.Join(settings.GameRoot, dllName), Path.Join(Path.GetFullPath("."), dllName));
        return true;
    }

    //Unhappy with this being here
    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _user32_ShowWindow(IntPtr hWnd, int nCmdShow);

    public void SaveAll()
    {
        foreach (EditorScreen editor in _editors)
        {
            editor.SaveAll();
        }
    }

    // Saves modded files to a recovery directory in the mod folder on crash
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
                                                  $"If you continue to crash on startup, delete config in AppData\\Local\\DSMapStudio\n\n" +
                                                  $"{e.Message} {e.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        var success = _assetLocator.CreateRecoveryProject();
        if (success)
        {
            SaveAll();
            PlatformUtils.Instance.MessageBox(
                $"Attempted to save project files to {_assetLocator.GameModDirectory} for manual recovery.\n" +
                "You must manually replace your project files with these recovery files should you wish to restore them.\n" +
                "Given the program has crashed, these files may be corrupt and you should backup your last good saved\n" +
                "files before attempting to use these.",
                "Saved recovery",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void SaveFocusedEditor()
    {
        if (_projectSettings != null && _projectSettings.ProjectName != null)
        {
            // Danger zone assuming on lastProjectFile
            _projectSettings.Serialize(CFG.Current.LastProjectFile);
            _focusedEditor.Save();
        }
    }

    private void NewProject_NameGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Name:      ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectName",
            "Project's display name. Only affects visuals within DSMS.");
        ImGui.SameLine();
        var pname = _newProjectOptions.settings.ProjectName;
        if (ImGui.InputText("##pname", ref pname, 255))
        {
            _newProjectOptions.settings.ProjectName = pname;
        }
    }

    private void NewProject_ProjectDirectoryGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Project Directory: ");
        ImGui.SameLine();
        Utils.ImGuiGenericHelpPopup("?", "##Help_ProjectDirectory",
            "The location mod files will be saved.\nTypically, this should be Mod Engine's Mod folder.");
        ImGui.SameLine();
        ImGui.InputText("##pdir", ref _newProjectOptions.directory, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{ForkAwesome.FileO}"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
            {
                _newProjectOptions.directory = path;
            }
        }
    }

    private void NewProject_GameTypeComboGUI()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(@"Game Type:         ");
        ImGui.SameLine();
        var games = Enum.GetNames(typeof(GameType));
        var gameIndex = Array.IndexOf(games, _newProjectOptions.settings.GameType.ToString());
        if (ImGui.Combo("##GameTypeCombo", ref gameIndex, games, games.Length))
        {
            _newProjectOptions.settings.GameType = Enum.Parse<GameType>(games[gameIndex]);
        }
    }

    private unsafe void Update(float deltaseconds)
    {
        Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "Imgui");

        var scale = GetUIScale();

        if (_settingsMenu.FontRebuildRequest)
        {
            _context.ImguiRenderer.Update(deltaseconds, InputTracker.FrameSnapshot, SetupFonts);
            _settingsMenu.FontRebuildRequest = false;
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
        var newProject = false;
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Enable Texturing (alpha)", "", CFG.Current.EnableTexturing))
                {
                    CFG.Current.EnableTexturing = !CFG.Current.EnableTexturing;
                }

                if (ImGui.MenuItem("New Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    newProject = true;
                }

                if (ImGui.MenuItem("Open Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    if (PlatformUtils.Instance.OpenFileDialog(
                            "Choose the project json file",
                            new[] { AssetLocator.ProjectJsonFilter },
                            out var path))
                    {
                        ProjectSettings settings = ProjectSettings.Deserialize(path);
                        if (settings != null)
                        {
                            AttemptLoadProject(settings, path);
                        }
                    }
                }

                if (ImGui.BeginMenu("Recent Projects",
                        !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
                {
                    CFG.RecentProject recent = null;
                    var id = 0;
                    foreach (CFG.RecentProject p in CFG.Current.RecentProjects.ToArray())
                    {
                        if (ImGui.MenuItem($@"{p.GameType}: {p.Name}##{id}"))
                        {
                            if (File.Exists(p.ProjectFile))
                            {
                                ProjectSettings settings = ProjectSettings.Deserialize(p.ProjectFile);
                                if (settings != null)
                                {
                                    if (AttemptLoadProject(settings, p.ProjectFile))
                                    {
                                        recent = p;
                                    }
                                }
                            }
                            else
                            {
                                TaskLogs.AddLog(
                                    $"Project.json at \"{p.ProjectFile}\" does not exist.\nRemoving project from recent projects list.",
                                    LogLevel.Warning, TaskLogs.LogPriority.High);
                                CFG.RemoveRecentProject(p);
                                CFG.Save();
                            }
                        }

                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.Selectable("Remove from list"))
                            {
                                CFG.RemoveRecentProject(p);
                                CFG.Save();
                            }

                            ImGui.EndPopup();
                        }

                        id++;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Open in Explorer",
                        !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
                {
                    if (ImGui.MenuItem("Open Project Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var projectPath = _assetLocator.GameModDirectory;
                        Process.Start("explorer.exe", projectPath);
                    }

                    if (ImGui.MenuItem("Open Game Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var gamePath = _assetLocator.GameRootDirectory;
                        Process.Start("explorer.exe", gamePath);
                    }

                    if (ImGui.MenuItem("Open Config Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var configPath = CFG.GetConfigFolderPath();
                        Process.Start("explorer.exe", configPath);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem($"Save {_focusedEditor.SaveType}",
                        KeyBindings.Current.Core_SaveCurrentEditor.HintText))
                {
                    SaveFocusedEditor();
                }

                if (ImGui.MenuItem("Save All", KeyBindings.Current.Core_SaveAllEditors.HintText))
                {
                    SaveAll();
                }

                if (ImGui.MenuItem("Editor Settings"))
                {
                    _settingsMenu.MenuOpenState = true;
                }

                if (FlverResource.CaptureMaterialLayouts && ImGui.MenuItem("Dump Flver Layouts (Debug)", ""))
                {
                    DumpFlverLayouts();
                }

                ImGui.EndMenu();
            }

            _focusedEditor.DrawEditorMenu();

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Help Menu", KeyBindings.Current.Core_HelpMenu.HintText))
                {
                    _helpBrowser.ToggleMenuVisibility();
                }

                ImGui.EndMenu();
            }

            if (FeatureFlags.TestMenu)
            {
                if (ImGui.BeginMenu("Tests"))
                {
                    if (ImGui.MenuItem("Crash me (will actually crash)"))
                    {
                        var badArray = new int[2];
                        var crash = badArray[5];
                    }

                    if (ImGui.MenuItem("MSBE read/write test"))
                    {
                        MSBReadWrite.Run(_assetLocator);
                    }

                    if (ImGui.MenuItem("MSB_AC6 Read/Write Test"))
                    {
                        MSB_AC6_Read_Write.Run(_assetLocator);
                    }

                    if (ImGui.MenuItem("BTL read/write test"))
                    {
                        BTLReadWrite.Run(_assetLocator);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("ImGui Debug"))
                {
                    if (ImGui.MenuItem("Demo"))
                    {
                        _showImGuiDemoWindow = true;
                    }

                    if (ImGui.MenuItem("Metrics"))
                    {
                        _showImGuiMetricsWindow = true;
                    }

                    if (ImGui.MenuItem("Debug Log"))
                    {
                        _showImGuiDebugLogWindow = true;
                    }

                    if (ImGui.MenuItem("Stack Tool"))
                    {
                        _showImGuiStackToolWindow = true;
                    }

                    ImGui.EndMenu();
                }
            }

            if (_programUpdateAvailable)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                if (ImGui.Button("Update Available"))
                {
                    Process myProcess = new();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = _releaseUrl;
                    myProcess.Start();
                }

                ImGui.PopStyleColor();
            }

            if (ImGui.BeginMenu("Tasks", TaskManager.GetLiveThreads().Count > 0))
            {
                foreach (var task in TaskManager.GetLiveThreads())
                {
                    ImGui.Text(task);
                }

                ImGui.EndMenu();
            }

            TaskLogs.Display();

            ImGui.EndMainMenuBar();
        }

        SettingsGUI();
        HelpGUI();

        ImGui.PopStyleVar();
        Tracy.TracyCZoneEnd(ctx);

        var open = true;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14.0f, 8.0f) * scale);

        // ImGui Debug windows
        if (_showImGuiDemoWindow)
        {
            ImGui.ShowDemoWindow(ref _showImGuiDemoWindow);
        }

        if (_showImGuiMetricsWindow)
        {
            ImGui.ShowMetricsWindow(ref _showImGuiMetricsWindow);
        }

        if (_showImGuiDebugLogWindow)
        {
            ImGui.ShowDebugLogWindow(ref _showImGuiDebugLogWindow);
        }

        if (_showImGuiStackToolWindow)
        {
            ImGui.ShowStackToolWindow(ref _showImGuiStackToolWindow);
        }

        // New project modal
        if (newProject)
        {
            _newProjectOptions.settings = new ProjectSettings();
            _newProjectOptions.directory = "";
            ImGui.OpenPopup("New Project");
        }

        if (ImGui.BeginPopupModal("New Project", ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            //
            ImGui.BeginTabBar("NewProjectTabBar");
            if (ImGui.BeginTabItem("Standard"))
            {
                if (!_standardProjectUIOpened)
                {
                    _newProjectOptions.settings.GameType = GameType.Undefined;
                }

                _standardProjectUIOpened = true;

                NewProject_NameGUI();
                NewProject_ProjectDirectoryGUI();

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Game Executable:   ");
                ImGui.SameLine();
                Utils.ImGuiGenericHelpPopup("?", "##Help_GameExecutable",
                    "The location of the game's .EXE or EBOOT.BIN file.\nThe folder with the executable will be used to obtain unpacked game data.");
                ImGui.SameLine();
                var gname = _newProjectOptions.settings.GameRoot;
                if (ImGui.InputText("##gdir", ref gname, 255))
                {
                    if (File.Exists(gname))
                    {
                        _newProjectOptions.settings.GameRoot = Path.GetDirectoryName(gname);
                    }
                    else
                    {
                        _newProjectOptions.settings.GameRoot = gname;
                    }

                    _newProjectOptions.settings.GameType = _assetLocator.GetGameTypeForExePath(gname);

                    if (_newProjectOptions.settings.GameType == GameType.Bloodborne)
                    {
                        _newProjectOptions.settings.GameRoot += @"\dvdroot_ps4";
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
                {
                    if (PlatformUtils.Instance.OpenFileDialog(
                            "Select executable for the game you want to mod...",
                            new[] { AssetLocator.GameExecutableFilter },
                            out var path))
                    {
                        _newProjectOptions.settings.GameRoot = Path.GetDirectoryName(path);
                        _newProjectOptions.settings.GameType = _assetLocator.GetGameTypeForExePath(path);

                        if (_newProjectOptions.settings.GameType == GameType.Bloodborne)
                        {
                            _newProjectOptions.settings.GameRoot += @"\dvdroot_ps4";
                        }
                    }
                }

                ImGui.Text($@"Detected Game:      {_newProjectOptions.settings.GameType}");

                ImGui.EndTabItem();
            }
            else
            {
                _standardProjectUIOpened = false;
            }

            if (ImGui.BeginTabItem("Advanced"))
            {
                NewProject_NameGUI();
                NewProject_ProjectDirectoryGUI();

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Game Directory:    ");
                ImGui.SameLine();
                Utils.ImGuiGenericHelpPopup("?", "##Help_GameDirectory",
                    "The location of game files.\nTypically, this should be the location of the game executable.");
                ImGui.SameLine();
                var gname = _newProjectOptions.settings.GameRoot;
                if (ImGui.InputText("##gdir", ref gname, 255))
                {
                    _newProjectOptions.settings.GameRoot = gname;
                }

                ImGui.SameLine();
                if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
                {
                    if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                    {
                        _newProjectOptions.settings.GameRoot = path;
                    }
                }

                NewProject_GameTypeComboGUI();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            //

            ImGui.Separator();
            if (_newProjectOptions.settings.GameType is GameType.DarkSoulsIISOTFS or GameType.DarkSoulsIII)
            {
                ImGui.NewLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(@"Loose Params:      ");
                ImGui.SameLine();
                Utils.ImGuiGenericHelpPopup("?", "##Help_LooseParams",
                    "Default: OFF\n" +
                    "DS2: Save and Load parameters as individual .param files instead of regulation.\n" +
                    "DS3: Save and Load parameters as decrypted .parambnd instead of regulation.");
                ImGui.SameLine();
                var looseparams = _newProjectOptions.settings.UseLooseParams;
                if (ImGui.Checkbox("##looseparams", ref looseparams))
                {
                    _newProjectOptions.settings.UseLooseParams = looseparams;
                }
            }
            else if (FeatureFlags.EnablePartialParam && _newProjectOptions.settings.GameType == GameType.EldenRing)
            {
                ImGui.NewLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Text(@"Save partial regulation:  ");
                ImGui.SameLine();
                Utils.ImGuiGenericHelpPopup("TODO (disbababled)", "##Help_PartialParam",
                    "TODO: why does this setting exist separately from loose params?");
                ImGui.SameLine();
                var partialReg = _newProjectOptions.settings.PartialParams;
                if (ImGui.Checkbox("##partialparams", ref partialReg))
                {
                    _newProjectOptions.settings.PartialParams = partialReg;
                }

                ImGui.SameLine();
                ImGui.TextUnformatted(
                    "Warning: partial params require merging before use in game.\nRow names on unchanged rows will be forgotten between saves");
            }
            else if (_newProjectOptions.settings.GameType is GameType.ArmoredCoreVI)
            {
                //TODO AC6
            }

            ImGui.NewLine();

            ImGui.AlignTextToFramePadding();
            ImGui.Text(@"Import row names:  ");
            ImGui.SameLine();
            Utils.ImGuiGenericHelpPopup("?", "##Help_ImportRowNames",
                "Default: ON\nImports and applies row names from lists stored in Assets folder.\nRow names can be imported at any time in the param editor's Edit menu.");
            ImGui.SameLine();
            ImGui.Checkbox("##loadDefaultNames", ref _newProjectOptions.loadDefaultNames);
            ImGui.NewLine();

            if (_newProjectOptions.settings.GameType == GameType.Undefined)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Create", new Vector2(120, 0) * scale))
            {
                var validated = true;
                if (_newProjectOptions.settings.GameRoot == null ||
                    !Directory.Exists(_newProjectOptions.settings.GameRoot))
                {
                    PlatformUtils.Instance.MessageBox(
                        "Your game executable path does not exist. Please select a valid executable.", "Error",
                        MessageBoxButtons.OK);
                    validated = false;
                }

                if (validated && _newProjectOptions.settings.GameType == GameType.Undefined)
                {
                    PlatformUtils.Instance.MessageBox("Your game executable is not a valid supported game.",
                        "Error",
                        MessageBoxButtons.OK);
                    validated = false;
                }

                if (validated && (_newProjectOptions.directory == null ||
                                  !Directory.Exists(_newProjectOptions.directory)))
                {
                    PlatformUtils.Instance.MessageBox("Your selected project directory is not valid.", "Error",
                        MessageBoxButtons.OK);
                    validated = false;
                }

                if (validated && File.Exists($@"{_newProjectOptions.directory}\project.json"))
                {
                    DialogResult message = PlatformUtils.Instance.MessageBox(
                        "Your selected project directory already contains a project.json. Would you like to replace it?",
                        "Error",
                        MessageBoxButtons.YesNo);
                    if (message == DialogResult.No)
                    {
                        validated = false;
                    }
                }

                if (validated && _newProjectOptions.settings.GameRoot == _newProjectOptions.directory)
                {
                    DialogResult message = PlatformUtils.Instance.MessageBox(
                        "Project Directory is the same as Game Directory, which allows game files to be overwritten directly.\n\n" +
                        "It's highly recommended you use the Mod Engine mod folder as your project folder instead (if possible).\n\n" +
                        "Continue and create project anyway?", "Caution",
                        MessageBoxButtons.OKCancel);
                    if (message != DialogResult.OK)
                    {
                        validated = false;
                    }
                }

                if (validated && (_newProjectOptions.settings.ProjectName == null ||
                                  _newProjectOptions.settings.ProjectName == ""))
                {
                    PlatformUtils.Instance.MessageBox("You must specify a project name.", "Error",
                        MessageBoxButtons.OK);
                    validated = false;
                }

                var gameroot = _newProjectOptions.settings.GameRoot;
                if (!_assetLocator.CheckFilesExpanded(gameroot, _newProjectOptions.settings.GameType))
                {
                    if (!GameNotUnpackedWarning(_newProjectOptions.settings.GameType))
                    {
                        validated = false;
                    }
                }

                if (validated)
                {
                    _newProjectOptions.settings.GameRoot = gameroot;
                    _newProjectOptions.settings.Serialize($@"{_newProjectOptions.directory}\project.json");
                    AttemptLoadProject(_newProjectOptions.settings, $@"{_newProjectOptions.directory}\project.json",
                        _newProjectOptions);

                    ImGui.CloseCurrentPopup();
                }
            }

            if (_newProjectOptions.settings.GameType == GameType.Undefined)
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0) * scale))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        ImGui.PopStyleVar(3);

        if (FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        ctx = Tracy.TracyCZoneN(1, "Editor");
        foreach (EditorScreen editor in _editors)
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
                _focusedEditor = editor;
                editor.Update(deltaseconds);
            }
            else
            {
                ImGui.PopStyleColor(1);
                ImGui.PopStyleVar(1);
                ImGui.End();
            }
        }

        // Global shortcut keys
        if (!_focusedEditor.InputCaptured())
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_SaveCurrentEditor))
            {
                SaveFocusedEditor();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_SaveAllEditors))
            {
                SaveAll();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_HelpMenu))
            {
                _helpBrowser.ToggleMenuVisibility();
            }
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

    public void SettingsGUI()
    {
        _settingsMenu.Display();
    }

    public void HelpGUI()
    {
        _helpBrowser.Display();
    }

    public static float GetUIScale()
    {
        // TODO: Multiply by monitor DPI when available.
        return CFG.Current.UIScale;
    }
}
