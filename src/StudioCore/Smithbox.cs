using ImGuiNET;
using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.CutsceneEditor;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.InfoBank;
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
using StudioCore.AssetLocator;
using StudioCore.Banks;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface.Windows;
using StudioCore.Interface;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Banks.ResourceBank;

namespace StudioCore;

public class Smithbox
{
    private static double _desiredFrameLengthSeconds = 1.0 / 20.0f;
    private static readonly bool _limitFrameRate = true;

    private static bool _initialLoadComplete;
    private static bool _firstframe = true;
    public static bool FirstFrame = true;

    public static bool LowRequirementsMode;

    private static IGraphicsContext _context;

    public static bool FontRebuildRequest;

    private readonly NewProjectOptions _newProjectOptions = new();
    private readonly string _programTitle;

    // Editors
    private readonly List<EditorScreen> _editors;
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
    private ProjectSettings _projectSettings;
    private string _releaseUrl = "";
    private bool _showImGuiDebugLogWindow;

    // ImGui Debug windows
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiStackToolWindow;

    private bool _standardProjectUIOpened = true;

    public unsafe Smithbox(IGraphicsContext context, string version)
    {
        _version = version;
        _programTitle = $"Smithbox - Version {_version}";

        // Hack to make sure dialogs work before the main window is created
        PlatformUtils.InitializeWindows(null);
        CFG.AttemptLoadOrDefault();

        Environment.SetEnvironmentVariable("PATH",
            Environment.GetEnvironmentVariable("PATH") + Path.PathSeparator + "bin");

        _context = context;
        _context.Initialize();
        _context.Window.Title = _programTitle;
        PlatformUtils.InitializeWindows(context.Window.SdlWindowHandle);

        // Banks
        MapAliasBank.Bank = new AliasBank(AliasType.Map);
        ModelAliasBank.Bank = new AliasBank(AliasType.Model);
        FlagAliasBank.Bank = new AliasBank(AliasType.EventFlag);
        ParticleAliasBank.Bank = new AliasBank(AliasType.Particle);
        MsbInfoBank.Bank = new InfoBank(FormatType.MSB);
        FlverInfoBank.Bank = new InfoBank(FormatType.FLVER);
        MaterialResourceBank.Setup();

        // Windows
        WindowContainer.SettingsWindow = new SettingsWindow();
        WindowContainer.HelpWindow = new HelpWindow();
        WindowContainer.EventFlagWindow = new EventFlagWindow();
        WindowContainer.DebugWindow = new DebugWindow();
        WindowContainer.MapNameWindow = new MapNameWindow();
        WindowContainer.KeybindWindow = new KeybindWindow();

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

        _editors.Add(EditorContainer.TextEditor);

        _focusedEditor = EditorContainer.MsbEditor;

        _soapstoneService = new SoapstoneService(_version);

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

    private void ChangeProjectSettings(ProjectSettings newsettings, string moddir, NewProjectOptions options)
    {
        _projectSettings = newsettings;

        Project.Type = newsettings.GameType;
        Project.GameRootDirectory = newsettings.GameRoot;
        Project.GameModDirectory = moddir;
        MapAssetLocator.FullMapList = null;

        WindowContainer.SettingsWindow.ProjSettings = _projectSettings;

        ModelAliasBank.Bank.ReloadAliasBank();
        FlagAliasBank.Bank.ReloadAliasBank();
        ParticleAliasBank.Bank.ReloadAliasBank();
        MapAliasBank.Bank.ReloadAliasBank();
        MsbInfoBank.Bank.ReloadInfoBank();
        FlverInfoBank.Bank.ReloadInfoBank();

        ParamBank.ReloadParams(newsettings, options);
        MaterialResourceBank.Setup();

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

    private bool GameNotUnpackedWarning(ProjectType gameType)
    {
        if (gameType is ProjectType.DS1 or ProjectType.DS2S)
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
                        new[] { FilterStrings.GameExecutableFilter },
                        out var path))
                {
                    settings.GameRoot = path;
                    ProjectType gametype = Project.GetProjectTypeFromExecutable(settings.GameRoot);

                    if (gametype == settings.GameType)
                    {
                        success = true;
                        settings.GameRoot = Path.GetDirectoryName(settings.GameRoot);

                        if (settings.GameType == ProjectType.BB)
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
            if (!LocatorUtils.CheckFilesExpanded(settings.GameRoot, settings.GameType))
            {
                if (!GameNotUnpackedWarning(settings.GameType))
                {
                    return false;
                }
            }

            if (settings.GameType == ProjectType.SDT || settings.GameType == ProjectType.ER)
            {
                if (!StealGameDllIfMissing(settings, "oo2core_6_win64"))
                {
                    return false;
                }
            }
            else if (settings.GameType == ProjectType.AC6)
            {
                if (!StealGameDllIfMissing(settings, "oo2core_8_win64"))
                {
                    return false;
                }
            }

            _projectSettings = settings;
            ChangeProjectSettings(_projectSettings, Path.GetDirectoryName(filename), options);
            _context.Window.Title = $"{_projectSettings.ProjectName}  -  {_programTitle}";

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
                                                  $"If you continue to crash on startup, delete config in AppData\\Local\\Smithbox\n\n" +
                                                  $"{e.Message} {e.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

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
        var games = Enum.GetNames(typeof(ProjectType));
        var gameIndex = Array.IndexOf(games, _newProjectOptions.settings.GameType.ToString());
        if (ImGui.Combo("##GameTypeCombo", ref gameIndex, games, games.Length))
        {
            _newProjectOptions.settings.GameType = Enum.Parse<ProjectType>(games[gameIndex]);
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
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    newProject = true;
                }

                if (ImGui.MenuItem("Open Project", "", false, !TaskManager.AnyActiveTasks()))
                {
                    if (PlatformUtils.Instance.OpenFileDialog(
                            "Choose the project json file",
                            new[] { FilterStrings.ProjectJsonFilter },
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
                                DialogResult result = PlatformUtils.Instance.MessageBox(
                                    $"Project file at \"{p.ProjectFile}\" does not exist.\n\n" +
                                    $"Remove project from list of recent projects?",
                                    $"Project.json cannot be found", MessageBoxButtons.YesNo);
                                if (result == DialogResult.Yes)
                                {
                                    CFG.RemoveRecentProject(p);
                                }
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
                        var projectPath = Project.GameModDirectory;
                        Process.Start("explorer.exe", projectPath);
                    }

                    if (ImGui.MenuItem("Open Game Folder", "", false, !TaskManager.AnyActiveTasks()))
                    {
                        var gamePath = Project.GameRootDirectory;
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

                ImGui.EndMenu();
            }

            _focusedEditor.DrawEditorMenu();

            // Task Bar
            TaskLogs.Display();

            ImGui.Separator();

            // Configuration Bar
            if (ImGui.Button($"{ForkAwesome.Cogs}"))
            {
                WindowContainer.SettingsWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowButtonTooltip("Settings");

            if (ImGui.Button($"{ForkAwesome.KeyboardO}"))
            {
                WindowContainer.KeybindWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowButtonTooltip("Keybinds");

            if (ImGui.Button($"{ForkAwesome.Book}"))
            {
                WindowContainer.HelpWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowButtonTooltip("Help");

            if (FeatureFlags.DebugMenu)
            {
                if (ImGui.Button($"{ForkAwesome.Bell}"))
                {
                    WindowContainer.DebugWindow.ToggleMenuVisibility();
                }
                ImguiUtils.ShowButtonTooltip("Debug");
            }

            ImGui.Separator();

            // Shared Tool Bar
            if (ImGui.Button($"{ForkAwesome.LightbulbO}"))
            {
                WindowContainer.EventFlagWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowButtonTooltip("Event Flags");

            if (ImGui.Button($"{ForkAwesome.Building}"))
            {
                WindowContainer.MapNameWindow.ToggleMenuVisibility();
            }
            ImguiUtils.ShowButtonTooltip("Map Names");

            // Program Update
            if (_programUpdateAvailable)
            {
                ImGui.Separator();

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

            ImGui.EndMainMenuBar();
        }

        WindowContainer.SettingsWindow.Display();
        WindowContainer.HelpWindow.Display();
        WindowContainer.EventFlagWindow.Display();
        WindowContainer.DebugWindow.Display();
        WindowContainer.MapNameWindow.Display();
        WindowContainer.KeybindWindow.Display();

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
                    _newProjectOptions.settings.GameType = ProjectType.Undefined;
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

                    _newProjectOptions.settings.GameType = Project.GetProjectTypeFromExecutable(gname);

                    if (_newProjectOptions.settings.GameType == ProjectType.BB)
                    {
                        _newProjectOptions.settings.GameRoot += @"\dvdroot_ps4";
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button($@"{ForkAwesome.FileO}##fd2"))
                {
                    if (PlatformUtils.Instance.OpenFileDialog(
                            "Select executable for the game you want to mod...",
                            new[] { FilterStrings.GameExecutableFilter },
                            out var path))
                    {
                        _newProjectOptions.settings.GameRoot = Path.GetDirectoryName(path);
                        _newProjectOptions.settings.GameType = Project.GetProjectTypeFromExecutable(path);

                        if (_newProjectOptions.settings.GameType == ProjectType.BB)
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
            if (_newProjectOptions.settings.GameType is ProjectType.DS2S or ProjectType.DS3)
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
            else if (FeatureFlags.EnablePartialParam && _newProjectOptions.settings.GameType == ProjectType.ER)
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
            else if (_newProjectOptions.settings.GameType is ProjectType.AC6)
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

            if (_newProjectOptions.settings.GameType == ProjectType.Undefined)
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

                if (validated && _newProjectOptions.settings.GameType == ProjectType.Undefined)
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
                if (!LocatorUtils.CheckFilesExpanded(gameroot, _newProjectOptions.settings.GameType))
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

            if (_newProjectOptions.settings.GameType == ProjectType.Undefined)
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

            if (InputTracker.GetKeyDown(KeyBindings.Current.Window_Help))
            {
                WindowContainer.HelpWindow.ToggleMenuVisibility();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Window_FlagBrowser))
            {
                WindowContainer.EventFlagWindow.ToggleMenuVisibility();
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

