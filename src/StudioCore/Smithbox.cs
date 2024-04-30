using ImGuiNET;
using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.CutsceneEditor;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Graphics;
using StudioCore.GraphicsEditor;
using StudioCore.MaterialEditor;
using StudioCore.MsbEditor;
using StudioCore.ParticleEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.EmevdEditor;
using StudioCore.Settings;
using StudioCore.TalkEditor;
using StudioCore.Tests;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
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
using StudioCore.Banks;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface.Windows;
using StudioCore.Interface;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.BehaviorEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.BanksMain;
using StudioCore.UserProject.Locators;
using static SoulsFormats.MCP;
using static SoulsFormats.DRB.Control;

namespace StudioCore;

public class Smithbox
{
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

    // Editors
    public static List<EditorScreen> _editors;
    private EditorScreen _focusedEditor;

    private readonly SoapstoneService _soapstoneService;
    private readonly string _version;

    /// <summary>
    ///     Characters to load that FromSoft use, but aren't included in the ImGui Japanese glyph range.
    /// </summary>
    private readonly char[] SpecialCharsJP =
    {
        '⇒','⑮','⑭','⑲','⑳','⑥','⑤','㎏','⑯','②','①','⑨','⑦','⑰','―','㌃','’','㌦','㌧','㌻','㍉','㍊','㍍','㍑','㍗','㍻','㍼','㍽','㍾','⑫','㌶','Ⅱ','※','⑱','⑧','⑩','⑪','㊤','㊦','㊧','㊨','●','㌍','⑬','㌣','“','”','•','㎎','←','↑','→','↓','蜘','蛛','牢','墟','壷','熔','吊','塵','屍','♀','彷','徨','徘','徊','吠','祀','…','騙','眷','嘘','穢','擲','罠','邂','逅','涜','扁','罹','脆','蠢','繍','蝕','袂','鍮','鴉','嘴','檻','娼','腑','賤','鍔','囁','祓','縋','歪','躯','仇','啜','繋','痺','搦','壺','覗','咆','哮','夥','隕','蹂','躙','詛','哭','捩','嘯','蕩','舐','嗜','僻','裔','贄','抉','鉈','叩','膂','迸','厭','鉾','誂','呆','跪','攫','滲','唸','躊','躇','∞','靭','棘','篝','㌔','㌘','㌢','㌫','③','④','瑕','疵','■','頬','髭','痣','埃','窩','枷','戮','僥','濘','侘','噛','呻','怯','碌','懺','吼','縷','爺','餞','誑','邁','儂','儚','憑','糞','眩','瞞','讐','澹','軛','鶯','瀉','鋤','蝋','⇔','磔','Ω','Ⅰ','賽','渠','瞑','蛆','澱','揶','揄','篭','贖','帷','冑','熾','斃','屠','謳','烙','痒','爛','褪','鑽','矮','傅','虔','瘴','躱','泄','瘤','蟲','燻','滓','蝙','蝠','楔','剥','膿','簒','矜','拗','欒','炸','烽','譚','謐','咬','佇','蜥','蜴','噺','嵌','掴','僭','貶','朧','峙','棍','鋲','鬨','薔','薇','滾','洩','髯','剃','Ⅲ','™','竄','–','誅','掻','愴','鼠','涎','蛭','蛾','贔','屓','鎚','鉤','芒','傀','儡','α','β','γ','礫','♂','○','鍾','囮','踵','誹','囃','碍','鄙','賎','掟','娑','弩','蜀','靄','蛙','轢','嗟','贅','Ⅳ','齧','咎','奢','頚','燐','填','鏃','△','□','謬','諌','憺','媚','垢','宸','憫','蝿','蟇','嚢','─','悶','櫃','咳','狗','艱','倅','箪','淤','飴','梟','曰','仄','呟','吽','刎','鬘','睨','鈷','屏','汞','翡','籃','蝉','箒','猩','埒','閂','癪','皺','憚','杞','甕','弑','祟','狐','貉','撓','褄','★','祠','廠','燼','衒','狸','酩','酊','殲','鹵','閾','謗','—','한','국','어','體','简','Р','у','с','к','и','й','언','변','경','최','종','사','용','자','라','이','선','스','계','약','개','인','정','보','처','리','방','침','데','터','에','관','동','의','페','지','넘','기','택','실','행','닫','하','다','거','절','變','權','隱','擇','關','语','变','终','户','许','协','议','隐','游','戏','换','页','选','择','执','关','闭','ę','Ż','Ń','Ś','ż','ź','ń','П','а','р','м','е','т','ы','я','з','Л','И','Ц','Е','Н','З','О','С','Г','А','Ш','К','Ч','Ы','М','Ь','В','Т','Ф','Д','о','г','л','ш','н','б','п','ь','в','д','х','Я','ю','猜','諜','聘','站','腿','恫','賁','戈','絨','毯','攪','倶','洒','掩','頸','懣','愾','啼','狽','捌','頷','轍','輜','儘','淘','餐','廓','撹','飄','坩','堝','屹','鬣','孕','痾','衾','聳','嘶','疼','蠅','茹','朦','鹸','閨','闢','竦','焉','斂','蛹','蜃','孵','蟻','癌','瘡','蠍','鋏','讒','姦','仗','拵','跋','扈','鮫','笏','錨','銛','撻','⟪','⟫','ภ','า','ษ','​','ไ','ท','ย','禿','驟','咥','慟','糺','麾','藉','蠱','奸','躾','吝','嗇','孺','濾','滸','訝','煽','蛸','‐','恍','隧','臍','蟷','螂','蜷','œ','„','업','트','년','월','일','본','을','신','중','게','읽','으','십','시','오','귀','가','당','임','또','는','서','비','접','근','나','를','것','은','술','된','모','든','조','건','그','고','참','급','통','해','부','루','구','속','되','로','함','미','합','니','러','두','않','우','마','재','항','제','과','집','단','소','송','포','있','며','외','역','주','들','적','습','독','특','됩','세','내','및','확','유','럽','연','호','북','남','칭','와','멀','티','플','레','온','면','전','문','콘','텐','츠','운','드','능','련','위','액','입','판','매','아','여','대','명','권','른','품','존','떤','식','도','키','추','혹','삭','공','요','금','청','차','프','션','규','칙','음','징','될','수','달','룹','상','충','체','결','립','따','치','같','취','뒤','안','완','히','준','할','법','성','령','만','후','견','야','코','등','록','넷','디','털','못','작','반','영','컬','컴','퓨','웨','랫','폼','크','웃','됨','했','간','점','직','배','타','양','불','널','버','별','예','범','느','허','락','복','목','분','석','설','파','셈','블','발','생','물','저','알','렵','벗','회','폐','래','텍','픽','진','화','템','열','산','더','장','없','족','효','각','써','무','런','책','즉','료','환','받','량','새','메','승','필','획','득','럼','철','교','줄','혀','향','칠','격','증','익','원','현','려','져','색','출','활','걸','쳐','누','평','찬','킬','표','검','토','감','때','론','퇴','훼','손','욕','쾌','란','노','골','괴','롭','협','박','퍼','학','킹','편','밀','광','력','뜨','팸','류','봇','뮬','움','순','올','네','워','끊','밖','었','바','담','닙','황','팩','질','형','태','천','애','묵','롯','므','강','축','쟁','패','큼','까','백','민','초','험','망','멸','휴','벌','총','높','테','심','탈','병','엄','앞','울','엇','닌','탕','겠','뿐','냄','납','긴','날','찍','름','번','케','팅','념','엔','캘','섭','랑','클','카','갖','쿄','옹','탁','머','잔','너','캔','르','객','갱','컨','롤','짜','릴','홈','살','펴','벨','델','브','뉴','밍','센','랭','글','균','맞','춥','랍','돕','응','답','벤','링','좋','탐','률','채','턴','웹','믿','옵','난','람','떠','곳','낼','閱','讀','們','您','處','豁','歐','澳','稱','雙','說','內','屬','售','會','產','刪','絕','銷','另','發','點','參','當','效','區','齡','對','號','續','數','腦','經','據','獨','譯','圖','找','碼','衍','檔','沒','滿','隨','仍','繼','兌','值','賺','賬','戶','圍','佔','稅','做','聲','譽','歸','擔','查','佈','雖','猥','褻','擾','勵','攬','假','僱','聯','亂','喊','寫','垃','圾','缺','礙','斷','幫','贏','證','贊','輕','濟','潛','兩','址','份','釋','簽','遞','餘','啟','你','窗','瀏','舉','營','估','獎','趨','聊','辨','隸','竊','请','细','阅','读','访','问','们','务','处','约','这','节','为','则','适','于','详','见','亚','订','进','并','网','络','发','线','载','书','电','该','权','产','删','确','绝','张','贴','费','结','销','规','说','间','冲','类','缔','签','实','决','过','显','获','时','经','达','辖','龄','监','护','须','对','册','续','连','联','损','运','设','备','统','帐','拥','话','转','让','补','带','况','复','个','业','编','译','汇','试','图','寻','码','创','标','计','范','围','它','尝','饰','义','软','币','拟','档','财','识','现','满','毁','负','责','暂','还','购','买','无','继','视','邮','应','紧','货','专','门','兑','价','际','认','响','赚','赎','额','赠','赁','员','账','开','弃','频','项','广','传','赔','偿','资','归','错','误','论','违','审','储','报','虽','辑','从','给','诽','谤','伤','秽','亵','骚','扰','胁','滥','动','诈','种','揽','输','坛','领','导','陈','坏','优','势','骗','仿','维','难','击','败','帮','赢','赛','卖','师','东','证','质','赞','荐','颁','样','减','强','竞','仅','测','诺','长','严','伙','惩','罚','润','轻','坚','众','调','构','济','诉','讼','团','纠','纷','讯','别','组','较','两','亲','阶','级','针','释','预','递','题','记','录','营','赋','圣','县','杂','夺','启','扫','忆','侦','浏','览','阐','顶','沟','态','评','术','奖','兴','闻','趋','检','顾','绍','单','织','丢','链','齐','异','ć','Ę','Ć','Ą','Ł','ą','ł','ś','Ź','Й','Э','У','Ж','Б','Ю','Щ','Х','ж','ф','ц','ч','щ','э','ъ','Ъ','เ','ล','ื','','อ','น','ก','ด','ํ','ิ','ร','ป','ี','ม','ั','บ','่','','ข','้','ต','ง','ใ','ห','ส','ธ','','แ','ผ','ู','ช','จ','ุ','ค','','ณ','ถ','ึ','ะ','พ','ว','ญ','โ','ซ','ศ','ฐ','','ฏ','์','ๆ','็','ฉ','ฑ','ฎ','ฟ','','','ฮ','','ฝ','','','ฆ','ฌ','ฤ','ฯ','ฒ','','鎗','≪','≫','隘','髑','髏'
    };

    private bool _programUpdateAvailable;
    private string _releaseUrl = "";
    private bool _showImGuiDebugLogWindow;

    // ImGui Debug windows
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiStackToolWindow;

    public unsafe Smithbox(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Smithbox - Version {_version}";

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

        BankUtils.SetupBanks();

        // Windows
        WindowContainer.ProjectWindow = new ProjectWindow();
        WindowContainer.AliasWindow = new AliasWindow();
        WindowContainer.SettingsWindow = new SettingsWindow();
        WindowContainer.HelpWindow = new HelpWindow();
        WindowContainer.DebugWindow = new DebugWindow();
        WindowContainer.KeybindWindow = new KeybindWindow();
        WindowContainer.MemoryWindow = new MemoryWindow();
        WindowContainer.ColorPickerWindow = new ColorPickerWindow();

        // Editors
        EditorContainer.MsbEditor = new MapEditorScreen(_context.Window, _context.Device);
        EditorContainer.ModelEditor = new ModelEditorScreen(_context.Window, _context.Device);
        EditorContainer.TextEditor = new TextEditorScreen(_context.Window, _context.Device);
        EditorContainer.ParamEditor = new ParamEditorScreen(_context.Window, _context.Device);
        EditorContainer.TimeActEditor = new TimeActEditorScreen(_context.Window, _context.Device);
        EditorContainer.CutsceneEditor = new CutsceneEditorScreen(_context.Window, _context.Device);
        EditorContainer.GparamEditor = new GparamEditorScreen(_context.Window, _context.Device);
        EditorContainer.MaterialEditor = new MaterialEditorScreen(_context.Window, _context.Device);
        EditorContainer.ParticleEditor = new ParticleEditorScreen(_context.Window, _context.Device);
        EditorContainer.ScriptEditor = new EventScriptEditorScreen(_context.Window, _context.Device);
        EditorContainer.TalkEditor = new TalkScriptEditorScreen(_context.Window, _context.Device);
        EditorContainer.TextureViewer = new TextureViewerScreen(_context.Window, _context.Device);
        EditorContainer.BehaviorEditor = new BehaviorEditorScreen(_context.Window, _context.Device);

        WindowContainer.MemoryWindow._activeView = ParamEditorScreen._activeView;

        _editors = new List<EditorScreen>();

        _editors.Add(EditorContainer.MsbEditor);
        _editors.Add(EditorContainer.ModelEditor);
        _editors.Add(EditorContainer.ParamEditor);

        if (FeatureFlags.EnableEditor_TimeAct)
        {
            _editors.Add(EditorContainer.TimeActEditor);
        }

        if (FeatureFlags.EnableEditor_Cutscene)
        {
            _editors.Add(EditorContainer.CutsceneEditor);
        }

        if (FeatureFlags.EnableEditor_Material)
        {
            _editors.Add(EditorContainer.MaterialEditor);
        }

        if (FeatureFlags.EnableEditor_Particle)
        {
            _editors.Add(EditorContainer.ParticleEditor);
        }

        if (FeatureFlags.EnableEditor_Gparam)
        {
            _editors.Add(EditorContainer.GparamEditor);
        }

        if (FeatureFlags.EnableEditor_EventScript)
        {
            _editors.Add(EditorContainer.ScriptEditor);
        }

        if (FeatureFlags.EnableEditor_TalkScript)
        {
            _editors.Add(EditorContainer.TalkEditor);
        }

        if (FeatureFlags.EnableEditor_TextureViewer)
        {
            _editors.Add(EditorContainer.TextureViewer);
        }

        if (FeatureFlags.EnableEditor_BehaviorEditor)
        {
            _editors.Add(EditorContainer.BehaviorEditor);
        }

        _editors.Add(EditorContainer.TextEditor);

        _focusedEditor = EditorContainer.MsbEditor;

        _soapstoneService = new SoapstoneService(_version);

        foreach (EditorScreen editor in _editors)
        {
            editor.Init();
        }

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        SetupFonts();
        _context.ImguiRenderer.OnSetupDone();

        ImGuiStylePtr style = ImGui.GetStyle();
        style.TabBorderSize = 0;

        Project.CheckForLastProject();

        Project.UpdateTimer();
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
            Array.ForEach(SpecialCharsJP, c => glyphRanges.AddChar(c));

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

        if (CFG.Current.System_Enable_Soapstone_Server)
        {
            TaskManager.RunPassiveTask(new TaskManager.LiveTask("Soapstone Server",
                TaskManager.RequeueType.None, true,
                () => SoapstoneServer.RunAsync(KnownServer.DSMapStudio, _soapstoneService).Wait()));
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

    public void SaveAll()
    {
        Project.SaveProjectJson();

        foreach (EditorScreen editor in _editors)
        {
            editor.SaveAll();
        }
    }

    private void SaveFocusedEditor()
    {
        Project.SaveProjectJson();

        _focusedEditor.Save();
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
                                                  $"If you continue to crash on startup, delete config in AppData\\Local\\Smithbox\n\n" +
                                                  $"{e.Message} {e.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        if (CFG.Current.System_EnableRecoveryFolder)
        {
            var success = Project.CreateRecoveryProject();
            if (success)
            {
                SaveAll();
                PlatformUtils.Instance.MessageBox(
                    $"Attempted to save project files to {Project.GameModDirectory} for manual recovery.\n" +
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
        var newProject = false;
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0.0f);

        if (ImGui.BeginMainMenuBar())
        {
            // Dropdown: File
            if (ImGui.BeginMenu("File"))
            {
                // New Project
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.File}");
                if (ImGui.MenuItem("New Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    newProject = true;
                }

                // Open Project
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Folder}");
                if (ImGui.MenuItem("Open Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    Project.OpenProjectDialog();
                }

                // Recent Projects
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.FolderOpen}");
                if (ImGui.BeginMenu("Recent Projects",
                        !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
                {
                    Project.DisplayRecentProjects();

                    ImGui.EndMenu();
                }

                // Open in Explorer
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Archive}");
                if (ImGui.BeginMenu("Open in Explorer",
                        !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
                {
                    if (ImGui.MenuItem("Project Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var projectPath = Project.GameModDirectory;
                        Process.Start("explorer.exe", projectPath);
                    }

                    if (ImGui.MenuItem("Game Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var gamePath = Project.GameRootDirectory;
                        Process.Start("explorer.exe", gamePath);
                    }

                    if (ImGui.MenuItem("Config Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var configPath = CFG.GetConfigFolderPath();
                        Process.Start("explorer.exe", configPath);
                    }

                    ImGui.EndMenu();
                }

                // Save
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.FloppyO}");
                if (ImGui.MenuItem($"Save Selected {_focusedEditor.SaveType}",
                        KeyBindings.Current.Core_SaveCurrentEditor.HintText))
                {
                    SaveFocusedEditor();
                }

                // Save All
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.FloppyO}");
                if (ImGui.MenuItem($"Save All Modified {_focusedEditor.SaveType}", KeyBindings.Current.Core_SaveAllEditors.HintText))
                {
                    SaveAll();
                }

                ImGui.EndMenu();
            }

            _focusedEditor.DrawEditorMenu();

            // Task Bar
            TaskLogs.Display();

            ImGui.Separator();

            // Configuration Bar
            if (ImGui.Button($"{ForkAwesome.Cogs}##SettingsWindow"))
            {
                WindowContainer.SettingsWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Settings\n{KeyBindings.Current.ToggleWindow_Settings.HintText}");

            if (ImGui.Button($"{ForkAwesome.KeyboardO}##KeybindWindow"))
            {
                WindowContainer.KeybindWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Keybinds\n{KeyBindings.Current.ToggleWindow_Keybind.HintText}");

            if (ImGui.Button($"{ForkAwesome.Book}##HelpWindow"))
            {
                WindowContainer.HelpWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Help\n{KeyBindings.Current.ToggleWindow_Help.HintText}");

            if (FeatureFlags.DebugMenu)
            {
                if (ImGui.Button($"{ForkAwesome.Bell}##DebugWindow"))
                {
                    WindowContainer.DebugWindow.ToggleMenuVisibility();
                }
                ImguiUtils.ShowHoverTooltip($"Debug\n{KeyBindings.Current.ToggleWindow_Debug.HintText}");
            }

            ImGui.Separator();

            // Shared Tool Bar
            if (ImGui.Button($"{ForkAwesome.Wrench}##ProjectWindow"))
            {
                WindowContainer.ProjectWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Project\n{KeyBindings.Current.ToggleWindow_Project.HintText}");

            if (ImGui.Button($"{ForkAwesome.FileText}##AliasWindow"))
            {
                WindowContainer.AliasWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Aliases\n{KeyBindings.Current.ToggleWindow_Alias.HintText}");

            if (ImGui.Button($"{ForkAwesome.Database}##MemoryWindow"))
            {
                WindowContainer.MemoryWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Memory\n{KeyBindings.Current.ToggleWindow_Memory.HintText}");

            if (ImGui.Button($"{ForkAwesome.PaintBrush}##ColorPickerWindow"))
            {
                WindowContainer.ColorPickerWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowHoverTooltip($"Color Picker\n{KeyBindings.Current.ToggleWindow_ColorPicker.HintText}");

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

        WindowContainer.SettingsWindow.Display();
        WindowContainer.HelpWindow.Display();
        WindowContainer.DebugWindow.Display(_graphicsDevice);
        WindowContainer.KeybindWindow.Display();
        WindowContainer.MemoryWindow.Display();
        WindowContainer.ProjectWindow.Display();
        WindowContainer.AliasWindow.Display();
        WindowContainer.ColorPickerWindow.Display();

        ImGui.PopStyleVar();
        Tracy.TracyCZoneEnd(ctx);

        var open = true;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(14.0f, 8.0f) * scale);

        // ImGui Debug windows
        if (WindowContainer.DebugWindow._showImGuiDemoWindow)
        {
            ImGui.ShowDemoWindow(ref WindowContainer.DebugWindow._showImGuiDemoWindow);
        }

        if (WindowContainer.DebugWindow._showImGuiMetricsWindow)
        {
            ImGui.ShowMetricsWindow(ref WindowContainer.DebugWindow._showImGuiMetricsWindow);
        }

        if (WindowContainer.DebugWindow._showImGuiDebugLogWindow)
        {
            ImGui.ShowDebugLogWindow(ref WindowContainer.DebugWindow._showImGuiDebugLogWindow);
        }

        if (WindowContainer.DebugWindow._showImGuiStackToolWindow)
        {
            ImGui.ShowStackToolWindow(ref WindowContainer.DebugWindow._showImGuiStackToolWindow);
        }

        // New project modal
        if (newProject)
        {
            ImGui.OpenPopup("New Project");
        }

        if (ImGui.BeginPopupModal("New Project", ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            Project.CreateNewProjectModal();

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
                // Reset this so on Focus the first frame focusing happens
                editor.FirstFrame = true;
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

            // Shortcut: Open Project Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Project))
            {
                WindowContainer.ProjectWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Help Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Help))
            {
                WindowContainer.HelpWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Keybind Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Keybind))
            {
                WindowContainer.KeybindWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Memory Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Memory))
            {
                WindowContainer.MemoryWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Settings Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Settings))
            {
                WindowContainer.SettingsWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Alias Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Alias))
            {
                WindowContainer.AliasWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Color Picker Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_ColorPicker))
            {
                WindowContainer.ColorPickerWindow.ToggleMenuVisibility();
            }

            // Shortcut: Open Debug Window
            if (InputTracker.GetKeyDown(KeyBindings.Current.ToggleWindow_Debug))
            {
                WindowContainer.DebugWindow.ToggleMenuVisibility();
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

    private const float DefaultDpi = 96f;
    private static float _dpi = DefaultDpi;

    private static float Dpi
    {
        get => _dpi;
        set
        {
            if (Math.Abs(value - _dpi) > 0.9f)
                FontRebuildRequest = true;
            _dpi = value;
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

