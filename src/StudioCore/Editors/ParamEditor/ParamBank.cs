using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StudioCore.Editors.TextEditor;
using System.ComponentModel.DataAnnotations;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using StudioCore.Core;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class ParamBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public string SourcePath;
    public string FallbackPath;

    public ParamBank(Smithbox baseEditor, ProjectEntry project, string sourcePath, string fallbackPath)
    {
        BaseEditor = baseEditor;
        Project = project;
        SourcePath = sourcePath;
        FallbackPath = fallbackPath;
    }

    /// <summary>
    /// Setup for this bank
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        _params = new Dictionary<string, Param>();

        UICache.ClearCaches();

        if (Project.ProjectType == ProjectType.DES)
        {
            LoadParamsDES();
        }

        if (Project.ProjectType == ProjectType.DS1)
        {
            LoadParamsDS1();
        }

        if (Project.ProjectType == ProjectType.DS1R)
        {
            LoadParamsDS1R();
        }

        if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
        {
            LoadParamsDS2();
        }

        if (Project.ProjectType == ProjectType.DS3)
        {
            LoadParamsDS3();
        }

        if (Project.ProjectType == ProjectType.BB || Project.ProjectType == ProjectType.SDT)
        {
            LoadParamsBBSekiro();
        }

        if (Project.ProjectType == ProjectType.ER)
        {
            LoadParamsER();
        }

        if (Project.ProjectType == ProjectType.AC6)
        {
            LoadParamsAC6();
        }

        // NOTE: this will be set for the primary bank, meaning any other banks are ignored.
        if (!Project.ImportedParamRowNames)
        {
            Project.ImportedParamRowNames = true;

            try
            {
                new ActionManager().ExecuteAction(LoadParamDefaultNames());
                SaveParams();
            }
            catch
            {
                TaskLogs.AddLog("Could not locate or apply name files",
                    LogLevel.Warning);
            }
        }

        ClearParamDiffCaches();

        return true;
    }


    public enum ParamUpgradeResult
    {
        Success = 0,
        RowConflictsFound = -1,
        OldRegulationNotFound = -2,
        OldRegulationVersionMismatch = -3,
        OldRegulationMatchesCurrent = -4
    }

    public enum RowGetType
    {
        [Display(Name = "All Rows")] AllRows = 0,
        [Display(Name = "Modified Rows")] ModifiedRows = 1,
        [Display(Name = "Selected Rows")] SelectedRows = 2
    }

    public string ClipboardParam = null;
    public List<Param.Row> ClipboardRows = new();

    /// <summary>
    ///     Map related params.
    /// </summary>
    public readonly List<string> DS2MapParamlist = new()
    {
        "demopointlight",
        "demospotlight",
        "eventlocation",
        "eventparam",
        "GeneralLocationEventParam",
        "generatorparam",
        "generatorregistparam",
        "generatorlocation",
        "generatordbglocation",
        "hitgroupparam",
        "intrudepointparam",
        "mapobjectinstanceparam",
        "maptargetdirparam",
        "npctalkparam",
        "treasureboxparam"
    };

    private readonly HashSet<int> EMPTYSET = new();

    public Dictionary<string, Param> _params;

    private ulong _paramVersion;

    private bool _pendingUpgrade;
    private Dictionary<string, HashSet<int>> _primaryDiffCache; //If param != primaryparam
    private Dictionary<string, List<string>> _storedStrippedRowNames;

    /// <summary>
    ///     Dictionary of param file names that were given a tentative ParamType, and the original ParamType it had.
    ///     Used to later restore original ParamType on write (if possible).
    /// </summary>
    private Dictionary<string, string> UsedFakeParamTypes;

    private Dictionary<string, HashSet<int>> _vanillaDiffCache; //If param != vanillaparam

    private Param EnemyParam;

    public IReadOnlyDictionary<string, Param> Params
    {
        get
        {
            return _params;
        }
    }

    public ulong ParamVersion => _paramVersion;

    public IReadOnlyDictionary<string, HashSet<int>> VanillaDiffCache
    {
        get
        {
            return _vanillaDiffCache;
        }
    }

    public IReadOnlyDictionary<string, HashSet<int>> PrimaryDiffCache
    {
        get
        {
            return _primaryDiffCache;
        }
    }

    private FileNotFoundException CreateParamMissingException(ProjectType type)
    {
        if (type is ProjectType.DS1 or ProjectType.SDT)
        {
            return new FileNotFoundException(
                $"Cannot locate param files for {type}.\nThis game must be unpacked before modding, please use UXM Selective Unpacker.");
        }

        if (type is ProjectType.DES or ProjectType.BB)
        {
            return new FileNotFoundException(
                $"Cannot locate param files for {type}.\nYour game folder may be missing game files.");
        }

        return new FileNotFoundException(
            $"Cannot locate param files for {type}.\nYour game folder may be missing game files, please verify game files through steam to restore them.");
    }

    public CompoundAction LoadParamDefaultNames(string param = null, bool onlyAffectEmptyNames = false, bool onlyAffectVanillaNames = false, bool useProjectNames = false, bool useDeveloperNames = false, IEnumerable<Param.Row> affectedRows = null)
    {
        var dir = ParamLocator.GetParamNamesDir();

        if (useProjectNames && Project.ProjectType != ProjectType.Undefined)
        {
            dir = $"{Project.ProjectPath}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";

            // Fallback to Smithbox if the project ones don't exist
            if(!Directory.Exists(dir))
                dir = ParamLocator.GetParamNamesDir();
        }

        if (useDeveloperNames && Project.ProjectType != ProjectType.Undefined)
        {
            dir = $"{AppContext.BaseDirectory}\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Developer Names";

            // Fallback to Smithbox if the developer ones don't exist
            if (!Directory.Exists(dir))
                dir = ParamLocator.GetParamNamesDir();
        }

        var files = param == null
            ? Directory.GetFiles(dir, "*.txt")
            : new[] { Path.Combine(dir, $"{param}.txt") };

        List<EditorAction> actions = new();

        foreach (var f in files)
        {
            var fName = Path.GetFileNameWithoutExtension(f);

            if(!File.Exists(f))
            {
                continue;
            }

            if (!_params.ContainsKey(fName))
            {
                continue;
            }

            var lines = File.ReadAllLines(f);

            if (affectedRows != null)
            {
                var affectedIds = affectedRows.Select(a => a.ID.ToString());
                lines = lines.Where(n => affectedIds.Any(i => n.StartsWith(i))).ToArray();
            }

            var names = string.Join(Environment.NewLine, lines);

            (var result, CompoundAction action) =
                ParamIO.ApplySingleCSV(Project, Project.ParamData.PrimaryBank, names, fName, "Name", ' ', true, onlyAffectEmptyNames, onlyAffectVanillaNames, true);

            if (action == null)
            {
                TaskLogs.AddLog($"Could not apply name files for {fName}\nFile path: {f}",
                    LogLevel.Warning);
            }
            else
            {
                actions.Add(action);
            }
        }

        return new CompoundAction(actions);
    }

    public ActionManager TrimNewlineChrsFromNames()
    {
        (MassEditResult r, ActionManager child) =
            MassParamEditRegex.PerformMassEdit(this, "param .*: id .*: name: replace \r:0", null);
        return child;
    }

    public static List<string> GraphicsConfigParams = new List<string>();
    public static List<string> EventParams = new List<string>();
    public static List<string> SystemParams = new List<string>();

    private void LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version,
        bool checkVersion = false, string parambndFileName = "")
    {
        var success = ulong.TryParse(parambnd.Version, out version);
        if (checkVersion && !success)
        {
            throw new Exception(@"Failed to get regulation version. Params might be corrupt.");
        }

        // Load every param in the regulation
        foreach (BinderFile f in parambnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(f.Name);

            if (!f.Name.ToUpper().EndsWith(".PARAM"))
            {
                continue;
            }

            if (paramBank.ContainsKey(paramName))
            {
                continue;
            }

            // Populate the side parambnd lists so we can filter them out easily
            if(parambndFileName == "graphicsconfig")
            {
                GraphicsConfigParams.Add(paramName);
            }
            if (parambndFileName == "eventparam")
            {
                EventParams.Add(paramName);
            }
            if (parambndFileName == "systemparam")
            {
                SystemParams.Add(paramName);
            }

            Param p;

            // AC6/SDT - Tentative ParamTypes
            if (Project.ProjectType is ProjectType.AC6 or ProjectType.SDT)
            {
                UsedFakeParamTypes = new Dictionary<string, string>();
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!string.IsNullOrEmpty(p.ParamType) )
                {
                    if (!Project.ParamData.ParamDefs.ContainsKey(p.ParamType) || paramName == "EquipParamWeapon_Npc")
                    {
                        if (Project.ParamData.FakeParamTypes.TryGetValue(paramName, out var newParamType))
                        {
                            UsedFakeParamTypes.Add(paramName, p.ParamType);
                            p.ParamType = newParamType;
                            //TaskLogs.AddLog(
                            //    $"Couldn't find ParamDef for {paramName}, but tentative ParamType \"{newParamType}\" exists.",
                            //    LogLevel.Warning);
                        }
                        else
                        {
                            TaskLogs.AddLog(
                                $"Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.",
                                LogLevel.Error, LogPriority.High);
                            continue;
                        }
                    }
                }
                else
                {
                    if (Project.ParamData.FakeParamTypes.TryGetValue(paramName, out var newParamType))
                    {
                        UsedFakeParamTypes.Add(paramName, p.ParamType);
                        p.ParamType = newParamType;
                        //TaskLogs.AddLog(
                        //    $"Couldn't read ParamType for {paramName}, but tentative ParamType \"{newParamType}\" exists.",
                        //   LogLevel.Warning);
                    }
                    else
                    {
                        TaskLogs.AddLog(
                            $"Couldn't read ParamType for {paramName} and no tentative ParamType exists.",
                            LogLevel.Error, LogPriority.High);
                        continue;
                    }
                }
            }
            else
            {
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!Project.ParamData.ParamDefs.ContainsKey(p.ParamType ?? ""))
                {
                    TaskLogs.AddLog(
                        $"Couldn't find ParamDef for param {paramName} with ParamType \"{p.ParamType}\".",
                        LogLevel.Warning);
                    continue;
                }
            }

            // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
            // it's an easy fixup
            if (Project.ProjectType == ProjectType.ER && version >= 10601000)
            {
                if(p.ParamType == "CHR_MODEL_PARAM_ST")
                {
                    if (p.FixupERField(12, 16))
                        TaskLogs.AddLog($"CHR_MODEL_PARAM_ST fixed up.");
                }
            }

            // Add in the new data for these two params added in 1.12.1
            if (Project.ProjectType == ProjectType.ER && version >= 11210015)
            {
                if (p.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
                {
                    if(p.FixupERField(880, 1024))
                        TaskLogs.AddLog($"GAME_SYSTEM_COMMON_PARAM_ST fixed up.");
                }
                if (p.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
                {
                    if (p.FixupERField(112, 144))
                        TaskLogs.AddLog($"POSTURE_CONTROL_PARAM_WEP_RIGHT_ST fixed up.");
                }
                if (p.ParamType == "SIGN_PUDDLE_PARAM_ST")
                {
                    if (p.FixupERField(32, 48))
                        TaskLogs.AddLog($"SIGN_PUDDLE_PARAM_ST fixed up.");
                }
            }

            if (p.ParamType == null)
            {
                throw new Exception("Param type is unexpectedly null");
            }

            // Skip these for DS1 so the param load is not slowed down by the catching
            if(Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if(paramName is "m99_ToneCorrectBank" or "m99_ToneMapBank" or "default_ToneCorrectBank")
                {
                    TaskLogs.AddLog($"Skipped this param: {paramName}");
                    continue;
                }
            }

            PARAMDEF def = Project.ParamData.ParamDefs[p.ParamType];

            // Sekiro:
            // The GraphicsConfig.param within the graphicsconfig.parambnd uses a different layout to the main one
            // So intercept the used def here and force it to use the alternative one.
            if (Project.ProjectType is ProjectType.SDT)
            {
                if (parambndFileName == "graphicsconfig")
                {
                    if (paramName == "GraphicsConfig")
                    {
                        def = Project.ParamData.ParamDefs["CS_GRAPHICS_CONFIG_PARAM_ST_ALT"];
                    }
                }
            }

            try
            {
                p.ApplyParamdef(def, version);
                paramBank.Add(paramName, p);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                var message = $"Could not apply ParamDef for {name}";

                TaskLogs.AddLog(message, LogLevel.Warning, LogPriority.Normal, e);
            }
        }
    }

    /// <summary>
    ///     Checks for DeS paramBNDs and returns the name of the parambnd with the highest priority.
    /// </summary>
    private string GetDesGameparamName(string rootDirectory)
    {
        var name = "";
        name = "gameparamna.parambnd.dcx";
        if (File.Exists($@"{rootDirectory}\param\gameparam\{name}"))
        {
            return name;
        }

        name = "gameparamna.parambnd";
        if (File.Exists($@"{rootDirectory}\param\gameparam\{name}"))
        {
            return name;
        }

        name = "gameparam.parambnd.dcx";
        if (File.Exists($@"{rootDirectory}\param\gameparam\{name}"))
        {
            return name;
        }

        name = "gameparam.parambnd";
        if (File.Exists($@"{rootDirectory}\param\gameparam\{name}"))
        {
            return name;
        }

        return "";
    }

    private void LoadParamsDES()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var paramBinderName = GetDesGameparamName(mod);
        if (paramBinderName == "")
        {
            paramBinderName = GetDesGameparamName(dir);
        }

        // Load params
        var param = $@"{mod}\param\gameparam\{paramBinderName}";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\{paramBinderName}";
        }

        if (!File.Exists(param))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        LoadParamsDESFromFile(param);

        //DrawParam
        Dictionary<string, string> drawparams = new();
        if (Directory.Exists($@"{dir}\param\drawparam"))
        {
            foreach (var p in Directory.GetFiles($@"{dir}\param\drawparam", "*.parambnd.dcx"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        if (Directory.Exists($@"{mod}\param\drawparam"))
        {
            foreach (var p in Directory.GetFiles($@"{mod}\param\drawparam", "*.parambnd.dcx"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        foreach (KeyValuePair<string, string> drawparam in drawparams)
        {
            LoadParamsDESFromFile(drawparam.Value);
        }

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DES)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsDESFromFile(string path)
    {
        try
        {
            using var bnd = BND3.Read(path);
            LoadParamFromBinder(bnd, ref _params, out _paramVersion);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadParamsDS1()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd";
        }

        LoadParamsDS1FromFile(param);

        //DrawParam
        Dictionary<string, string> drawparams = new();
        if (Directory.Exists($@"{dir}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{dir}\param\DrawParam", "*.parambnd"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        if (Directory.Exists($@"{mod}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{mod}\param\DrawParam", "*.parambnd"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        foreach (KeyValuePair<string, string> drawparam in drawparams)
        {
            LoadParamsDS1FromFile(drawparam.Value);
        }

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS1)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsDS1FromFile(string path)
    {
        try
        {
            using var bnd = BND3.Read(path);
            LoadParamFromBinder(bnd, ref _params, out _paramVersion);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadParamsDS1R()
    {
        var dir = FallbackPath;
        var mod = SourcePath;
        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd.dcx"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd.dcx";
        }

        LoadParamsDS1RFromFile(param);

        //DrawParam
        Dictionary<string, string> drawparams = new();
        if (Directory.Exists($@"{dir}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{dir}\param\DrawParam", "*.parambnd.dcx"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        if (Directory.Exists($@"{mod}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{mod}\param\DrawParam", "*.parambnd.dcx"))
            {
                drawparams[Path.GetFileNameWithoutExtension(p)] = p;
            }
        }

        foreach (KeyValuePair<string, string> drawparam in drawparams)
        {
            LoadParamsDS1RFromFile(drawparam.Value);
        }

        if (Project.ProjectType is ProjectType.DS1R)
        {
            if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS1)
            {
                LoadExternalRowNames();
            }
        }
    }

    private void LoadParamsDS1RFromFile(string path)
    {
        try
        {
            using var bnd = BND3.Read(path);
            LoadParamFromBinder(bnd, ref _params, out _paramVersion);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadParamsBBSekiro()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        GraphicsConfigParams = new List<string>();

        if (!File.Exists($@"{dir}\\param\gameparam\gameparam.parambnd.dcx"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        // Load params
        var param = $@"{mod}\param\gameparam\gameparam.parambnd.dcx";

        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\gameparam.parambnd.dcx";
        }

        LoadParamsBBSekiroFromFile(param);

        if (Project.ProjectType is ProjectType.SDT)
        {
            var graphicsConfigParam = LocatorUtils.GetAssetPath(@"param\graphicsconfig\graphicsconfig.parambnd.dcx");
            if (File.Exists(graphicsConfigParam))
            {
                LoadParamsBBSekiroFromFile(graphicsConfigParam, "graphicsconfig");
            }
            else
            {
                TaskLogs.AddLog("Graphics Params file could not be found. These require an unpacked game to modify.", LogLevel.Warning, LogPriority.Normal);
            }
        }

        if (Project.ProjectType is ProjectType.BB)
        {
            if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_BB)
            {
                LoadExternalRowNames();
            }
        }

        if (Project.ProjectType is ProjectType.SDT)
        {
            if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_SDT)
            {
                LoadExternalRowNames();
            }
        }
    }

    private void LoadParamsBBSekiroFromFile(string path, string parambndFileName = "")
    {
        try
        {
            using var bnd = BND4.Read(path);
            LoadParamFromBinder(bnd, ref _params, out _paramVersion, false, parambndFileName);
        }
        catch(Exception e)
        {
            PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static List<string> GetLooseParamsInDir(string dir)
    {
        List<string> looseParams = new();

        if (Directory.Exists($@"{dir}\Param"))
        {
            looseParams.AddRange(Directory.GetFileSystemEntries($@"{dir}\Param", @"*.param"));
        }

        return looseParams;
    }

    private void LoadParamsDS2()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        if (!File.Exists($@"{dir}\enc_regulation.bnd.dcx"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        if (!BND4.Is($@"{dir}\enc_regulation.bnd.dcx"))
        {
            
        }

        // Load loose params (prioritizing ones in mod folder)
        List<string> looseParams = GetLooseParamsInDir(mod);

        if (Directory.Exists($@"{dir}\Param"))
        {
            // Include any params in game folder that are not in mod folder
            foreach (var path in Directory.GetFileSystemEntries($@"{dir}\Param", @"*.param"))
            {
                if (looseParams.Find(e => Path.GetFileName(e) == Path.GetFileName(path)) == null)
                {
                    // Project folder does not contain this loose param
                    looseParams.Add(path);
                }
            }
        }

        // Load reg params
        var param = $@"{mod}\enc_regulation.bnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\enc_regulation.bnd.dcx";
        }

        var enemyFile = $@"{mod}\Param\EnemyParam.param";
        if (!File.Exists(enemyFile))
        {
            enemyFile = $@"{dir}\Param\EnemyParam.param";
        }

        LoadParamsDS2FromFile(looseParams, param, enemyFile);

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS2)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsDS2FromFile(List<string> looseParams, string path, string enemypath)
    {
        BND4 paramBnd = null;
        if (!BND4.Is(path))
        {
            try
            {
                paramBnd = SFUtil.DecryptDS2Regulation(path);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            try
            {
                paramBnd = BND4.Read(path);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        BinderFile bndfile = paramBnd.Files.Find(x => Path.GetFileName(x.Name) == "EnemyParam.param");
        if (bndfile != null)
        {
            EnemyParam = Param.Read(bndfile.Bytes);
        }

        // Otherwise the param is a loose param
        if (File.Exists(enemypath))
        {
            EnemyParam = Param.Read(enemypath);
        }

        if (EnemyParam is { ParamType: not null })
        {
            try
            {
                PARAMDEF def = Project.ParamData.ParamDefs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, LogPriority.Normal, e);
            }
        }

        LoadParamFromBinder(paramBnd, ref _params, out _paramVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var lp = Param.Read(p);
            var fname = lp.ParamType;

            // Skip this param since it always fails and catching it slows down the param load
            if (Project.ProjectType == ProjectType.DS2S || Project.ProjectType == ProjectType.DS2)
            {
                if (fname is "GENERATOR_DBG_LOCATION_PARAM")
                    continue;
            }

            try
            {
                if (CFG.Current.Param_UseLooseParams)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!_params.ContainsKey(name))
                    {
                        PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"Could not apply ParamDef for {fname}";
                TaskLogs.AddLog(message, LogLevel.Warning, LogPriority.Normal, e);
            }
        }

        paramBnd.Dispose();
    }

    private void LoadParamsDS3()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        if (!File.Exists($@"{dir}\Data0.bdt"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        var vparam = $@"{dir}\Data0.bdt";
        // Load loose params if they exist
        if (CFG.Current.Param_UseLooseParams && File.Exists($@"{mod}\\param\gameparam\gameparam_dlc2.parambnd.dcx"))
        {
            LoadParamsDS3FromFile($@"{mod}\param\gameparam\gameparam_dlc2.parambnd.dcx");
        }
        else
        {
            var param = $@"{mod}\Data0.bdt";

            if (!File.Exists(param))
            {
                param = vparam;
            }

            LoadParamsDS3FromFile(param);
        }

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS3)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsDS3FromFile(string path, bool isVanillaLoad = false)
    {
        var tryLooseParams = CFG.Current.Param_UseLooseParams;
        if(isVanillaLoad)
        {
            tryLooseParams = false;
        }

        try
        {
            using BND4 lparamBnd = tryLooseParams ? BND4.Read(path) : SFUtil.DecryptDS3Regulation(path);
            LoadParamFromBinder(lparamBnd, ref _params, out _paramVersion);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadParamsER()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        SystemParams = new List<string>();
        EventParams = new List<string>();

        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        // Load params
        var param = $@"{mod}\regulation.bin";

        if (!File.Exists(param))
        {
            param = $@"{dir}\regulation.bin";
        }

        LoadParamsERFromFile(param);

        param = $@"{mod}\regulation.bin";

        var sysParam = LocatorUtils.GetAssetPath(@"param\systemparam\systemparam.parambnd.dcx");
        if (File.Exists(sysParam))
        {
            LoadParamsERFromFile(sysParam, false, "systemparam");
        }
        else
        {
            TaskLogs.AddLog("System Params could not be found. These require an unpacked game to modify.", LogLevel.Warning, LogPriority.Normal);
        }

        var eventParam = LocatorUtils.GetAssetPath(@"param\eventparam\eventparam.parambnd.dcx");
        if (File.Exists(eventParam))
        {
            LoadParamsERFromFile(eventParam, false, "eventparam");
        }
        else
        {
            //TaskLogs.AddLog("Event Params could not be found.", LogLevel.Warning, LogPriority.Normal);
        }

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_ER)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsERFromFile(string path, bool encrypted = true, string parambndFileName = "")
    {
        if (encrypted)
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptERRegulation(path);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true, parambndFileName);
            }
            catch(Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(path);
                LoadParamFromBinder(bnd, ref _params, out _, false, parambndFileName);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void LoadParamsAC6()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        GraphicsConfigParams = new List<string>();
        SystemParams = new List<string>();
        EventParams = new List<string>();

        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            throw CreateParamMissingException(Project.ProjectType);
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param))
        {
            param = $@"{dir}\regulation.bin";
        }

        LoadParamsAC6FromFile(param);

        var sysParam = LocatorUtils.GetAssetPath(@"param\systemparam\systemparam.parambnd.dcx");
        if (File.Exists(sysParam))
        {
            LoadParamsAC6FromFile(sysParam, false, "systemparam");
        }
        else
        {
            TaskLogs.AddLog("System Params could not be found. These require an unpacked game to modify.", LogLevel.Warning, LogPriority.Normal);
        }

        var graphicsConfigParam = LocatorUtils.GetAssetPath(@"param\graphicsconfig\graphicsconfig.parambnd.dcx");
        if (File.Exists(graphicsConfigParam))
        {
            LoadParamsAC6FromFile(graphicsConfigParam, false, "graphicsconfig");
        }
        else
        {
            TaskLogs.AddLog("Graphic Params could not be found. These require an unpacked game to modify.", LogLevel.Warning, LogPriority.Normal);
        }

        var eventParam = LocatorUtils.GetAssetPath(@"param\eventparam\eventparam.parambnd.dcx");
        if (File.Exists(eventParam))
        {
            LoadParamsAC6FromFile(eventParam, false, "eventparam");
        }
        else
        {
            //TaskLogs.AddLog("Event Params could not be found.", LogLevel.Warning, LogPriority.Normal);
        }

        if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_AC6)
        {
            LoadExternalRowNames();
        }
    }

    private void LoadParamsAC6FromFile(string path, bool encrypted = true, string parambndFileName = "")
    {
        if (encrypted)
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptAC6Regulation(path);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true, parambndFileName);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            try
            {
                using var bnd = BND4.Read(path);
                LoadParamFromBinder(bnd, ref _params, out _, false, parambndFileName);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {path} - {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public void ClearParamDiffCaches()
    {
        _vanillaDiffCache = new Dictionary<string, HashSet<int>>();
        _primaryDiffCache = new Dictionary<string, HashSet<int>>();

        foreach (var param in _params.Keys)
        {
            _vanillaDiffCache.Add(param, new HashSet<int>());
            _primaryDiffCache.Add(param, new HashSet<int>());
        }
    }

    public void RefreshPrimaryDiffCaches(bool checkVanillaDiff)
    {
        if (checkVanillaDiff)
        {
            _vanillaDiffCache = GetParamDiff(Project.ParamData.VanillaBank);
        }

        UICache.ClearCaches();
    }

    public void RefreshVanillaDiffCaches()
    {
        if (Project.ParamData.PrimaryBank._vanillaDiffCache != null)
        {
            _primaryDiffCache = Project.ParamData.PrimaryBank._vanillaDiffCache;
        }

        _primaryDiffCache = GetParamDiff(Project.ParamData.PrimaryBank);

        UICache.ClearCaches();
    }

    public void RefreshAuxDiffCaches(bool checkVanillaDiff)
    {
        if (checkVanillaDiff)
        {
            _vanillaDiffCache = GetParamDiff(Project.ParamData.VanillaBank);
        }

        _primaryDiffCache = GetParamDiff(Project.ParamData.PrimaryBank);

        UICache.ClearCaches();
    }

    private Dictionary<string, HashSet<int>> GetParamDiff(ParamBank otherBank)
    {
        if (otherBank == null)
        {
            return null;
        }

        Dictionary<string, HashSet<int>> newCache = new();
        foreach (var param in _params.Keys)
        {
            HashSet<int> cache = new();
            newCache.Add(param, cache);
            Param p = _params[param];

            if (!otherBank._params.ContainsKey(param))
            {
                Console.WriteLine("Missing vanilla param " + param);
                continue;
            }

            Param.Row[] rows = _params[param].Rows.OrderBy(r => r.ID).ToArray();
            Param.Row[] vrows = otherBank._params[param].Rows.OrderBy(r => r.ID).ToArray();

            var vanillaIndex = 0;
            var lastID = -1;
            ReadOnlySpan<Param.Row> lastVanillaRows = default;

            for (var i = 0; i < rows.Length; i++)
            {
                var ID = rows[i].ID;
                if (ID == lastID)
                {
                    RefreshParamRowDiffCache(rows[i], lastVanillaRows, cache);
                }
                else
                {
                    lastID = ID;
                    while (vanillaIndex < vrows.Length && vrows[vanillaIndex].ID < ID)
                    {
                        vanillaIndex++;
                    }

                    if (vanillaIndex >= vrows.Length)
                    {
                        RefreshParamRowDiffCache(rows[i], Span<Param.Row>.Empty, cache);
                    }
                    else
                    {
                        var count = 0;
                        while (vanillaIndex + count < vrows.Length && vrows[vanillaIndex + count].ID == ID)
                        {
                            count++;
                        }

                        lastVanillaRows = new ReadOnlySpan<Param.Row>(vrows, vanillaIndex, count);
                        RefreshParamRowDiffCache(rows[i], lastVanillaRows, cache);
                        vanillaIndex += count;
                    }
                }
            }
        }

        return newCache;
    }

    private void RefreshParamRowDiffCache(Param.Row row, ReadOnlySpan<Param.Row> otherBankRows,
        HashSet<int> cache)
    {
        if (IsChanged(row, otherBankRows))
        {
            cache.Add(row.ID);
        }
        else
        {
            cache.Remove(row.ID);
        }
    }

    public void RefreshParamRowDiffs(ParamEditorScreen editor, Param.Row row, string param)
    {
        if (param == null)
        {
            return;
        }

        if (editor.Project.ParamData.VanillaBank.Params.ContainsKey(param) && VanillaDiffCache != null && VanillaDiffCache.ContainsKey(param))
        {
            Param.Row[] otherBankRows = editor.Project.ParamData.VanillaBank.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, VanillaDiffCache[param]);
        }

        foreach (ParamBank aux in editor.Project.ParamData.AuxBanks.Values)
        {
            if (!aux.Params.ContainsKey(param) || aux.PrimaryDiffCache == null || !aux.PrimaryDiffCache.ContainsKey(param))
            {
                continue; // Don't try for now
            }

            Param.Row[] otherBankRows = aux.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, aux.PrimaryDiffCache[param]);
        }
    }

    private static bool IsChanged(Param.Row row, ReadOnlySpan<Param.Row> vanillaRows)
    {
        //List<Param.Row> vanils = vanilla.Rows.Where(cell => cell.ID == row.ID).ToList();
        if (vanillaRows.Length == 0)
        {
            return true;
        }

        foreach (Param.Row vrow in vanillaRows)
        {
            if (row.RowMatches(vrow))
            {
                return false; //if we find a matching vanilla row
            }
        }

        return true;
    }

    private void SaveParamsDS1()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\param\GameParam\GameParam.parambnd";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd";

        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd";
        }

        using var paramBnd = BND3.Read(param);

        if(CFG.Current.Param_StripRowNamesOnSave_DS1)
        {
            StripRowNames();
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"param\GameParam\GameParam.parambnd", paramBnd);

        if (CFG.Current.Param_StripRowNamesOnSave_DS1)
        {
            RestoreStrippedRowNames();
        }

        // Draw Params
        if (Directory.Exists($@"{FallbackPath}\param\DrawParam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{FallbackPath}\param\DrawParam", "*.parambnd"))
            {
                using var drawParamBnd = BND3.Read(bnd);
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                Utils.WriteWithBackup(dir, mod, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }
    }

    private void SaveParamsDS1R()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\param\GameParam\GameParam.parambnd.dcx";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd.dcx";
        }

        using var paramBnd = BND3.Read(param);

        if (CFG.Current.Param_StripRowNamesOnSave_DS1)
        {
            StripRowNames();
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"param\GameParam\GameParam.parambnd.dcx", paramBnd);

        if (CFG.Current.Param_StripRowNamesOnSave_DS1)
        {
            RestoreStrippedRowNames();
        }

        // Drawparam
        if (Directory.Exists($@"{FallbackPath}\param\DrawParam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{FallbackPath}\param\DrawParam", "*.parambnd.dcx"))
            {
                using var drawParamBnd = BND3.Read(bnd);

                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                Utils.WriteWithBackup(dir, mod, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }
    }

    private void SaveParamsDS2()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\enc_regulation.bnd.dcx";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\enc_regulation.bnd.dcx";
        BND4 paramBnd;

        if (!File.Exists(param))
        {
            // If there is no mod file, check the base file. Decrypt it if you have to.
            param = $@"{dir}\enc_regulation.bnd.dcx";
            if (!BND4.Is($@"{dir}\enc_regulation.bnd.dcx"))
            {
                // Decrypt the file
                paramBnd = SFUtil.DecryptDS2Regulation(param);

                // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                if (!File.Exists($@"{param}.bak"))
                {
                    File.Copy(param, $@"{param}.bak", true);
                    paramBnd.Write(param);
                }
            }
            // No need to decrypt
            else
            {
                paramBnd = BND4.Read(param);
            }
        }
        // Mod file exists, use that.
        else
        {
            paramBnd = BND4.Read(param);
        }

        if (!CFG.Current.Param_UseLooseParams)
        {
            // Save params non-loosely: Replace params regulation and write remaining params loosely.

            if (paramBnd.Files.Find(e => e.Name.EndsWith(".param")) == null)
            {
                if (PlatformUtils.Instance.MessageBox(
                        "It appears that you are trying to save params non-loosely with an \"enc_regulation.bnd\" that has previously been saved loosely." +
                        "\n\nWould you like to reinsert params into the bnd that were previously stripped out?",
                        "DS2 de-loose param",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    paramBnd.Dispose();
                    param = $@"{dir}\enc_regulation.bnd.dcx";

                    if (!BND4.Is($@"{dir}\enc_regulation.bnd.dcx"))
                    {
                        // Decrypt the file.
                        paramBnd = SFUtil.DecryptDS2Regulation(param);

                        // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                        if (!File.Exists($@"{param}.bak"))
                        {
                            File.Copy(param, $@"{param}.bak", true);
                            paramBnd.Write(param);
                        }
                    }
                    else
                        paramBnd = BND4.Read(param);
                }
            }

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                if (CFG.Current.Param_StripRowNamesOnSave_DS2)
                {
                    StripRowNames();
                }

                foreach (KeyValuePair<string, Param> p in _params)
                {
                    BinderFile bnd = paramBnd.Files.Find(e => Path.GetFileNameWithoutExtension(e.Name) == p.Key);

                    if (bnd != null)
                    {
                        // Regulation contains this param, overwrite it.
                        bnd.Bytes = p.Value.Write();
                    }
                    else
                    {
                        // Regulation does not contain this param, write param loosely.
                        Utils.WriteWithBackup(dir, mod, $@"Param\{p.Key}.param", p.Value);
                    }
                }
            }
            catch
            {
                if (CFG.Current.Param_StripRowNamesOnSave_DS2)
                {
                    RestoreStrippedRowNames();
                }
                throw;
            }

            if (CFG.Current.Param_StripRowNamesOnSave_DS2)
            {
                RestoreStrippedRowNames();
            }
        }
        else
        {
            // Save params loosely: Strip params from regulation and write all params loosely.

            List<BinderFile> newFiles = new();
            foreach (BinderFile p in paramBnd.Files)
            {
                // Strip params from regulation bnd
                if (!p.Name.ToUpper().Contains(".PARAM"))
                {
                    newFiles.Add(p);
                }
            }

            paramBnd.Files = newFiles;

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                if (CFG.Current.Param_StripRowNamesOnSave_DS2)
                {
                    StripRowNames();
                }

                // Write params to loose files.
                foreach (KeyValuePair<string, Param> p in _params)
                {
                    Utils.WriteWithBackup(dir, mod, $@"Param\{p.Key}.param", p.Value);
                }
            }
            catch
            {
                if (CFG.Current.Param_StripRowNamesOnSave_DS2)
                {
                    RestoreStrippedRowNames();
                }
                throw;
            }

            if (CFG.Current.Param_StripRowNamesOnSave_DS2)
            {
                RestoreStrippedRowNames();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"enc_regulation.bnd.dcx", paramBnd);
        paramBnd.Dispose();
    }

    private void SaveParamsDS3()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\Data0.bdt";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\Data0.bdt";
        if (!File.Exists(param))
        {
            param = $@"{dir}\Data0.bdt";
        }

        BND4 paramBnd = SFUtil.DecryptDS3Regulation(param);

        if (CFG.Current.Param_StripRowNamesOnSave_DS3)
        {
            StripRowNames();
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // If not loose write out the new regulation
        if (!CFG.Current.Param_UseLooseParams)
        {
            Utils.WriteWithBackup(dir, mod, @"Data0.bdt", paramBnd, ProjectType.DS3);
        }
        else
        {
            // Otherwise write them out as parambnds
            BND4 paramBND = new()
            {
                BigEndian = false,
                Compression = DCX.Type.DCX_DFLT_10000_44_9,
                Extended = 0x04,
                Unk04 = false,
                Unk05 = false,
                Format = Binder.Format.Compression | Binder.Format.Flag6 | Binder.Format.LongOffsets |
                         Binder.Format.Names1,
                Unicode = true,
                Files = paramBnd.Files.Where(f => f.Name.EndsWith(".param")).ToList()
            };

            Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam_dlc2.parambnd.dcx", paramBND);
            //Utils.WriteWithBackup(dir, mod, @"param\stayparam\stayparam.parambnd.dcx", stayBND);
        }

        if (CFG.Current.Param_StripRowNamesOnSave_DS3)
        {
            RestoreStrippedRowNames();
        }
    }

    /// <summary>
    /// Param Save: Bloodborne
    /// </summary>
    private void SaveParamsBB()
    {
        void OverwriteParamsBB(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = _params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\param\gameparam\gameparam.parambnd.dcx";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\gameparam\gameparam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\gameparam.parambnd.dcx";
        }

        // Params
        var paramBnd = BND4.Read(param);

        if (CFG.Current.Param_StripRowNamesOnSave_BB)
        {
            StripRowNames();
        }

        OverwriteParamsBB(paramBnd);
        Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        if (CFG.Current.Param_StripRowNamesOnSave_BB)
        {
            RestoreStrippedRowNames();
        }
    }

    /// <summary>
    /// Param Save: Sekiro
    /// </summary>
    private void SaveParamsSDT()
    {
        void OverwriteParamsSDT(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = _params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\param\gameparam\gameparam.parambnd.dcx";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\gameparam\gameparam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\gameparam.parambnd.dcx";
        }

        // Params
        var paramBnd = BND4.Read(param);

        if (CFG.Current.Param_StripRowNamesOnSave_SDT)
        {
            StripRowNames();
        }

        OverwriteParamsSDT(paramBnd);
        Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        if (CFG.Current.Param_StripRowNamesOnSave_SDT)
        {
            RestoreStrippedRowNames();
        }

        // Graphics Config
        var graphicsConfigParam = LocatorUtils.GetAssetPath(@"param\graphicsconfig\graphicsconfig.parambnd.dcx");
        if (File.Exists(graphicsConfigParam))
        {
            using var graphicsConfigParams = BND4.Read(graphicsConfigParam);
            OverwriteParamsSDT(graphicsConfigParams);
            Utils.WriteWithBackup(dir, mod, @"param\graphicsconfig\graphicsconfig.parambnd.dcx", graphicsConfigParams);
        }
    }

    private void SaveParamsDES()
    {
        var dir = FallbackPath;
        var mod = SourcePath;

        var paramBinderName = GetDesGameparamName(mod);
        if (paramBinderName == "")
            paramBinderName = GetDesGameparamName(dir);

        // Load params
        var param = $@"{mod}\param\gameparam\{paramBinderName}";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\{paramBinderName}";
        }

        if (!File.Exists(param))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {param}", LogLevel.Error, LogPriority.High);
            return;
        }

        using var paramBnd = BND3.Read(param);

        if (CFG.Current.Param_StripRowNamesOnSave_DES)
        {
            StripRowNames();
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // Write all gameparam variations since we don't know which one the the game will use.
        // Compressed
        paramBnd.Compression = DCX.Type.DCX_EDGE;
        var naParamPath = @"param\gameparam\gameparamna.parambnd.dcx";
        if (File.Exists($@"{dir}\{naParamPath}"))
        {
            Utils.WriteWithBackup(dir, mod, naParamPath, paramBnd);
        }

        Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        // Decompressed
        paramBnd.Compression = DCX.Type.None;
        naParamPath = @"param\gameparam\gameparamna.parambnd";
        if (File.Exists($@"{dir}\{naParamPath}"))
        {
            Utils.WriteWithBackup(dir, mod, naParamPath, paramBnd);
        }

        Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam.parambnd", paramBnd);

        if (CFG.Current.Param_StripRowNamesOnSave_DES)
        {
            RestoreStrippedRowNames();
        }

        // Drawparam
        List<string> drawParambndPaths = new();
        if (Directory.Exists($@"{FallbackPath}\param\drawparam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{FallbackPath}\param\drawparam", "*.parambnd.dcx"))
            {
                drawParambndPaths.Add(bnd);
            }

            // Also save decompressed parambnds because DeS debug uses them.
            foreach (var bnd in Directory.GetFiles($@"{FallbackPath}\param\drawparam", "*.parambnd"))
            {
                drawParambndPaths.Add(bnd);
            }

            foreach (var bnd in drawParambndPaths)
            {
                using var drawParamBnd = BND3.Read(bnd);

                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                Utils.WriteWithBackup(dir, mod, @$"param\drawparam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }
    }

    private void SaveParamsER()
    {
        void OverwriteParamsER(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = _params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\regulation.bin";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param) || _pendingUpgrade)
        {
            param = $@"{dir}\regulation.bin";
        }

        BND4 regParams = SFUtil.DecryptERRegulation(param);

        if (CFG.Current.Param_StripRowNamesOnSave_ER)
        {
            StripRowNames();
        }

        OverwriteParamsER(regParams);

        Utils.WriteWithBackup(dir, mod, @"regulation.bin", regParams, ProjectType.ER);

        if (CFG.Current.Param_StripRowNamesOnSave_ER)
        {
            RestoreStrippedRowNames();
        }

        var sysParam = LocatorUtils.GetAssetPath(@"param\systemparam\systemparam.parambnd.dcx");
        var eventParam = LocatorUtils.GetAssetPath(@"param\eventparam\eventparam.parambnd.dcx");

        if (File.Exists(sysParam))
        {
            using var sysParams = BND4.Read(sysParam);
            OverwriteParamsER(sysParams);
            Utils.WriteWithBackup(dir, mod, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        if (File.Exists(eventParam))
        {
            using var eventParams = BND4.Read(eventParam);
            OverwriteParamsER(eventParams);
            Utils.WriteWithBackup(dir, mod, @"param\eventparam\eventparam.parambnd.dcx", eventParams);
        }

        _pendingUpgrade = false;
    }

    private void SaveParamsAC6()
    {
        void OverwriteParamsAC6(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                var paramName = Path.GetFileNameWithoutExtension(p.Name);
                if (_params.TryGetValue(paramName, out Param paramFile))
                {
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;
                    if (Project.ProjectType is ProjectType.AC6)
                    {
                        if (UsedFakeParamTypes.TryGetValue(paramName, out var oldParamType))
                        {
                            // This param was given a tentative ParamType, return original ParamType if possible.
                            oldParamType ??= "";
                            var prevParamType = paramFile.ParamType;
                            paramFile.ParamType = oldParamType;

                            p.Bytes = paramFile.Write();
                            paramFile.ParamType = prevParamType;
                            paramFile.Rows = backup;
                            continue;
                        }
                    }

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var dir = FallbackPath;
        var mod = SourcePath;

        var savePath = $@"{dir}\\regulation.bin";

        if (!File.Exists(savePath))
        {
            TaskLogs.AddLog($"Save failed. Cannot locate param files: {savePath}", LogLevel.Error, LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param) || _pendingUpgrade)
        {
            param = $@"{dir}\regulation.bin";
        }

        BND4 regParams = SFUtil.DecryptAC6Regulation(param);

        if (CFG.Current.Param_StripRowNamesOnSave_AC6)
        {
            StripRowNames();
        }

        OverwriteParamsAC6(regParams);
        Utils.WriteWithBackup(dir, mod, @"regulation.bin", regParams, ProjectType.AC6);

        if (CFG.Current.Param_StripRowNamesOnSave_AC6)
        {
            RestoreStrippedRowNames();
        }

        var sysParam = LocatorUtils.GetAssetPath(@"param\systemparam\systemparam.parambnd.dcx");
        if (File.Exists(sysParam))
        {
            using var sysParams = BND4.Read(sysParam);
            OverwriteParamsAC6(sysParams);
            Utils.WriteWithBackup(dir, mod, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        var graphicsConfigParam = LocatorUtils.GetAssetPath(@"param\graphicsconfig\graphicsconfig.parambnd.dcx");
        if (File.Exists(graphicsConfigParam))
        {
            using var graphicsConfigParams = BND4.Read(graphicsConfigParam);
            OverwriteParamsAC6(graphicsConfigParams);
            Utils.WriteWithBackup(dir, mod, @"param\graphicsconfig\graphicsconfig.parambnd.dcx", graphicsConfigParams);
        }

        var eventParam = LocatorUtils.GetAssetPath(@"param\eventparam\eventparam.parambnd.dcx");
        if (File.Exists(eventParam))
        {
            using var eventParams = BND4.Read(eventParam);
            OverwriteParamsAC6(eventParams);
            Utils.WriteWithBackup(dir, mod, @"param\eventparam\eventparam.parambnd.dcx", eventParams);
        }

        _pendingUpgrade = false;
    }

    public void SaveParams()
    {
        if (_params == null)
        {
            return;
        }

        switch(Project.ProjectType)
        {
            case ProjectType.DS1: 
                SaveParamsDS1(); 
                break;

            case ProjectType.DS1R: 
                SaveParamsDS1R(); 
                break;

            case ProjectType.DES: 
                SaveParamsDES(); 
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S: 
                SaveParamsDS2(); 
                break;

            case ProjectType.DS3:
                SaveParamsDS3(); 
                break;

            case ProjectType.BB:
                SaveParamsBB(); 
                break;

            case ProjectType.SDT:
                SaveParamsSDT();
                break;

            case ProjectType.ER: 
                SaveParamsER(); 
                break;

            case ProjectType.AC6:
                SaveParamsAC6(); 
                break;
        }
    }

    public static Param UpgradeParam(Param source, Param oldVanilla, Param newVanilla, HashSet<int> rowConflicts)
    {
        //TargetLog(source, source.ParamType);

        // Presorting this would make it easier, but we're trying to preserve order as much as possible
        // Unfortunately given that rows aren't guaranteed to be sorted and there can be duplicate IDs,
        // we try to respect the existing order and IDs as much as possible.

        // In order to assemble the final param, the param needs to know where to sort rows from given the
        // following rules:
        // 1. If a row with a given ID is unchanged from source to oldVanilla, we source from newVanilla
        // 2. If a row with a given ID is deleted from source compared to oldVanilla, we don't take any row
        // 3. If a row with a given ID is changed from source compared to oldVanilla, we source from source
        // 4. If a row has duplicate IDs, we treat them as if the rows were deduplicated and process them
        //    in the order they appear.

        // List of rows that are in source but not oldVanilla
        Dictionary<int, List<Param.Row>> addedRows = new(source.Rows.Count);

        // List of rows in oldVanilla that aren't in source
        Dictionary<int, List<Param.Row>> deletedRows = new(source.Rows.Count);

        // List of rows that are in source and oldVanilla, but are modified
        Dictionary<int, List<Param.Row>> modifiedRows = new(source.Rows.Count);

        // List of rows that only had the name changed
        Dictionary<int, List<Param.Row>> renamedRows = new(source.Rows.Count);

        // List of ordered edit operations for each ID
        Dictionary<int, List<EditOperation>> editOperations = new(source.Rows.Count);

        // First off we go through source and everything starts as an added param
        foreach (Param.Row row in source.Rows)
        {
            if (!addedRows.ContainsKey(row.ID))
            {
                addedRows.Add(row.ID, new List<Param.Row>());
            }

            addedRows[row.ID].Add(row);
            //TargetLog(source, $"Source - Add row: {row.ID}");
        }

        // Next we go through oldVanilla to determine if a row is added, deleted, modified, or unmodified
        foreach (Param.Row row in oldVanilla.Rows)
        {
            // First off if the row did not exist in the source, it's deleted
            if (!addedRows.ContainsKey(row.ID))
            {
                if (!deletedRows.ContainsKey(row.ID))
                {
                    deletedRows.Add(row.ID, new List<Param.Row>());
                }

                deletedRows[row.ID].Add(row);

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<EditOperation>());
                }

                editOperations[row.ID].Add(EditOperation.Delete);
                //TargetLog(source, $"oldVanilla - EditOperation.Delete: {row.ID}");

                continue;
            }

            // Otherwise the row exists in source. Time to classify it.
            List<Param.Row> list = addedRows[row.ID];

            // First we see if we match the first target row. If so we can remove it.
            if (row.DataEquals(list[0]))
            {
                Param.Row modrow = list[0];
                list.RemoveAt(0);
                if (list.Count == 0)
                {
                    addedRows.Remove(row.ID);
                }

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<EditOperation>());
                }

                // See if the name was not updated
                if (modrow.Name == null && row.Name == null ||
                    modrow.Name != null && row.Name != null && modrow.Name == row.Name)
                {
                    editOperations[row.ID].Add(EditOperation.Match);
                    //TargetLog(source, $"oldVanilla - EditOperation.Match: {row.ID}");
                    continue;
                }

                // Name was updated
                editOperations[row.ID].Add(EditOperation.NameChange);
                //TargetLog(source, $"oldVanilla - EditOperation.NameChange: {row.ID}");

                if (!renamedRows.ContainsKey(row.ID))
                {
                    renamedRows.Add(row.ID, new List<Param.Row>());
                }

                renamedRows[row.ID].Add(modrow);

                continue;
            }

            // Otherwise it is modified
            if (!modifiedRows.ContainsKey(row.ID))
            {
                modifiedRows.Add(row.ID, new List<Param.Row>());
            }

            modifiedRows[row.ID].Add(list[0]);
            list.RemoveAt(0);
            if (list.Count == 0)
            {
                addedRows.Remove(row.ID);
            }

            if (!editOperations.ContainsKey(row.ID))
            {
                editOperations.Add(row.ID, new List<EditOperation>());
            }

            editOperations[row.ID].Add(EditOperation.Modify);
            //TargetLog(source, $"oldVanilla - EditOperation.Modify: {row.ID}");
        }

        // Mark all remaining rows as added
        foreach (KeyValuePair<int, List<Param.Row>> entry in addedRows)
        {
            if (!editOperations.ContainsKey(entry.Key))
            {
                editOperations.Add(entry.Key, new List<EditOperation>());
            }

            foreach (List<EditOperation> k in editOperations.Values)
            {
                editOperations[entry.Key].Add(EditOperation.Add);
                //TargetLog(source, $"oldVanilla - EditOperation.Add: {entry.Key}");
            }
        }

        // Reverted "Reject attempts to upgrade via regulation matching current params" fix from https://github.com/soulsmods/DSMapStudio/pull/721
        // This was causing the Param Upgrader to not actually add the new rows
        /*
        if (editOperations.All(kvp => kvp.Value.All(eo => eo == EditOperation.Match)))
        {
            TargetLog(source, $"Return oldVanilla param");
            return oldVanilla;
        }
        */

        Param dest = new(newVanilla);

        // Now try to build the destination from the new regulation with the edit operations in mind
        var pendingAdds = addedRows.Keys.OrderBy(e => e).ToArray();
        var currPendingAdd = 0;
        var lastID = 0;
        foreach (Param.Row row in newVanilla.Rows)
        {
            //TargetLog(source, $"newVanilla row");

            // See if we have any pending adds we can slot in
            while (currPendingAdd < pendingAdds.Length &&
                   pendingAdds[currPendingAdd] >= lastID &&
                   pendingAdds[currPendingAdd] < row.ID)
            {
                if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
                {
                    currPendingAdd++;
                    //TargetLog(source, $"newVanilla - currPendingAdd: {pendingAdds[currPendingAdd-1]}");
                    continue;
                }

                foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
                {
                    dest.AddRow(new Param.Row(arow, dest));
                    //TargetLog(source, $"newVanilla - AddRow");
                }

                addedRows.Remove(pendingAdds[currPendingAdd]);
                editOperations.Remove(pendingAdds[currPendingAdd]);
                currPendingAdd++;
            }

            lastID = row.ID;

            if (!editOperations.ContainsKey(row.ID))
            {
                // No edit operations for this ID, so just add it (likely a new row in the update)
                dest.AddRow(new Param.Row(row, dest));
                //TargetLog(source, $"newVanilla - AddRow (New)");
                continue;
            }

            // Pop the latest operation we need to do
            EditOperation operation = editOperations[row.ID][0];
            editOperations[row.ID].RemoveAt(0);

            if (editOperations[row.ID].Count == 0)
            {
                editOperations.Remove(row.ID);
            }

            if (operation == EditOperation.Add)
            {
                // Getting here means both the mod and the updated regulation added a row. Our current strategy is
                // to overwrite the new vanilla row with the modded one and add to the conflict log to give the user
                rowConflicts.Add(row.ID);
                dest.AddRow(new Param.Row(addedRows[row.ID][0], dest));
                addedRows[row.ID].RemoveAt(0);

                if (addedRows[row.ID].Count == 0)
                {
                    addedRows.Remove(row.ID);
                }
            }
            else if (operation == EditOperation.Match)
            {
                // Match means we inherit updated param
                dest.AddRow(new Param.Row(row, dest));
            }
            else if (operation == EditOperation.Delete)
            {
                // deleted means we don't add anything
                deletedRows[row.ID].RemoveAt(0);
                if (deletedRows[row.ID].Count == 0)
                {
                    deletedRows.Remove(row.ID);
                }
            }
            else if (operation == EditOperation.Modify)
            {
                // Modified means we use the modded regulation's param
                dest.AddRow(new Param.Row(modifiedRows[row.ID][0], dest));
                modifiedRows[row.ID].RemoveAt(0);
                if (modifiedRows[row.ID].Count == 0)
                {
                    modifiedRows.Remove(row.ID);
                }
            }
            else if (operation == EditOperation.NameChange)
            {
                // Inherit name
                Param.Row newRow = new(row, dest);
                newRow.Name = renamedRows[row.ID][0].Name;
                dest.AddRow(newRow);
                renamedRows[row.ID].RemoveAt(0);
                if (renamedRows[row.ID].Count == 0)
                {
                    renamedRows.Remove(row.ID);
                }
            }
        }

        // Take care of any more pending adds
        for (; currPendingAdd < pendingAdds.Length; currPendingAdd++)
        {
            // If the pending add doesn't exist in the added rows list, it was a conflicting row
            if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
            {
                continue;
            }

            foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
            {
                dest.AddRow(new Param.Row(arow, dest));
            }

            addedRows.Remove(pendingAdds[currPendingAdd]);
            editOperations.Remove(pendingAdds[currPendingAdd]);
        }

        return dest;
    }

    // Param upgrade. Currently for Elden Ring/Armored Core VI.
    public ParamUpgradeResult UpgradeRegulation(ParamEditorScreen editor, ParamBank vanillaBank, string oldVanillaParamPath,
        Dictionary<string, HashSet<int>> conflictingParams)
    {
        // First we need to load the old regulation
        if (!File.Exists(oldVanillaParamPath))
        {
            return ParamUpgradeResult.OldRegulationNotFound;
        }

        // Backup modded params
        var modRegulationPath = $@"{Project.ProjectPath}\regulation.bin";
        File.Copy(modRegulationPath, $@"{modRegulationPath}.upgrade.bak", true);

        // Load old vanilla regulation
        BND4 oldVanillaParamBnd;
        if (Project.ProjectType == ProjectType.ER)
        {
            oldVanillaParamBnd = SFUtil.DecryptERRegulation(oldVanillaParamPath);
        }
        else if (Project.ProjectType == ProjectType.AC6)
        {
            oldVanillaParamBnd = SFUtil.DecryptAC6Regulation(oldVanillaParamPath);
        }
        else
        {
            throw new NotImplementedException(
                $"Param upgrading for game type {Project.ProjectType} is not supported.");
        }

        Dictionary<string, Param> oldVanillaParams = new();
        ulong version;
        LoadParamFromBinder(oldVanillaParamBnd, ref oldVanillaParams, out version, true);
        if (version != ParamVersion)
        {
            return ParamUpgradeResult.OldRegulationVersionMismatch;
        }

        Dictionary<string, Param> updatedParams = new();
        // Now we must diff everything to try and find changed/added rows for each param
        var anyUpgrades = false;
        foreach (var k in vanillaBank.Params.Keys)
        {
            // If the param is completely new, just take it
            if (!oldVanillaParams.ContainsKey(k) || !Params.ContainsKey(k))
            {
                updatedParams.Add(k, vanillaBank.Params[k]);
                continue;
            }

            // Otherwise try to upgrade
            HashSet<int> conflicts = new();
            Param res = UpgradeParam(Params[k], oldVanillaParams[k], vanillaBank.Params[k], conflicts);
            if (res != oldVanillaParams[k])
                anyUpgrades = true;

            updatedParams.Add(k, res);

            if (conflicts.Count > 0)
            {
                conflictingParams.Add(k, conflicts);
            }
        }

        if (!anyUpgrades)
        {
            return ParamUpgradeResult.OldRegulationMatchesCurrent;
        }

        var oldVersion = _paramVersion;

        // Set new params
        _params = updatedParams;
        _paramVersion = editor.Project.ParamData.VanillaBank.ParamVersion;
        _pendingUpgrade = true;

        // Refresh dirty cache
        UICache.ClearCaches();
        Project.ParamData.RefreshAllParamDiffCaches(false);

        return conflictingParams.Count > 0 ? ParamUpgradeResult.RowConflictsFound : ParamUpgradeResult.Success;
    }

    public string GetChrIDForEnemy(long enemyID)
    {
        Param.Row enemy = EnemyParam?[(int)enemyID];
        return enemy != null ? $@"{enemy.GetCellHandleOrThrow("chr_id").Value:D4}" : null;
    }

    public string GetKeyForParam(Param param)
    {
        if (Params == null)
        {
            return null;
        }

        foreach (KeyValuePair<string, Param> pair in Params)
        {
            if (param == pair.Value)
            {
                return pair.Key;
            }
        }

        return null;
    }

    public Param GetParamFromName(string param)
    {
        if (Params == null)
        {
            return null;
        }

        foreach (KeyValuePair<string, Param> pair in Params)
        {
            if (param == pair.Key)
            {
                return pair.Value;
            }
        }

        return null;
    }

    public HashSet<int> GetVanillaDiffRows(string param)
    {
        IReadOnlyDictionary<string, HashSet<int>> allDiffs = VanillaDiffCache;

        if (allDiffs == null || !allDiffs.ContainsKey(param))
        {
            return EMPTYSET;
        }

        return allDiffs[param];
    }

    public HashSet<int> GetPrimaryDiffRows(string param)
    {
        IReadOnlyDictionary<string, HashSet<int>> allDiffs = PrimaryDiffCache;

        if (allDiffs == null || !allDiffs.ContainsKey(param))
        {
            return EMPTYSET;
        }

        return allDiffs[param];
    }

    /// <summary>
    ///     Loads row names from external files and applies them to params.
    ///     Uses indicies rather than IDs.
    /// </summary>
    private void LoadExternalRowNames()
    {
        var failCount = 0;
        foreach (KeyValuePair<string, Param> p in _params)
        {
            var path = ParamLocator.GetStrippedRowNamesPath(p.Key);
            if (File.Exists(path))
            {
                var names = File.ReadAllLines(path);
                if (names.Length != p.Value.Rows.Count)
                {
                    TaskLogs.AddLog($"External row names could not be applied to {p.Key}, row count does not match",
                        LogLevel.Warning, LogPriority.Low);
                    failCount++;
                    continue;
                }

                for (var i = 0; i < names.Length; i++)
                    p.Value.Rows[i].Name = names[i];
            }
        }

        if (failCount > 0)
        {
            TaskLogs.AddLog(
                $"External row names could not be applied to {failCount} params due to non-matching row counts.",
                LogLevel.Warning);
        }
    }

    /// <summary>
    ///     Strips row names from params, saves them to files, and stores them to be restored after saving params.
    ///     Should always be used in conjunction with RestoreStrippedRowNames().
    /// </summary>
    private void StripRowNames()
    {
        _storedStrippedRowNames = new Dictionary<string, List<string>>();
        foreach (KeyValuePair<string, Param> p in _params)
        {
            _storedStrippedRowNames.TryAdd(p.Key, new List<string>());
            List<string> list = _storedStrippedRowNames[p.Key];
            foreach (Param.Row r in p.Value.Rows)
            {
                var output = r.Name;

                list.Add(output);
                r.Name = "";
            }

            var path = ParamLocator.GetStrippedRowNamesPath(p.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, list);
        }
    }

    /// <summary>
    ///     Restores stripped row names back to all params.
    ///     Should always be used in conjunction with StripRowNames().
    /// </summary>
    private void RestoreStrippedRowNames()
    {
        if (_storedStrippedRowNames == null)
        {
            throw new InvalidOperationException("No stripped row names have been stored.");
        }

        foreach (KeyValuePair<string, Param> p in _params)
        {
            List<string> storedNames = _storedStrippedRowNames[p.Key];
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                p.Value.Rows[i].Name = storedNames[i];
            }
        }

        _storedStrippedRowNames = null;
    }

    private enum EditOperation
    {
        Add,
        Delete,
        Modify,
        NameChange,
        Match
    }

    
}
