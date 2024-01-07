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

namespace StudioCore.ParamEditor;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class ParamBank
{
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
        AllRows = 0,
        ModifiedRows = 1,
        SelectedRows = 2
    }

    public static ParamBank PrimaryBank = new();
    public static ParamBank VanillaBank = new();
    public static Dictionary<string, ParamBank> AuxBanks = new();


    public static string ClipboardParam = null;
    public static List<Param.Row> ClipboardRows = new();

    /// <summary>
    ///     Mapping from ParamType -> PARAMDEF.
    /// </summary>
    private static Dictionary<string, PARAMDEF> _paramdefs;

    /// <summary>
    ///     Mapping from Param filename -> Manual ParamType.
    ///     This is for params with no usable ParamType at some particular game version.
    ///     By convention, ParamTypes ending in "_TENTATIVE" do not have official data to reference.
    /// </summary>
    private static Dictionary<string, string> _tentativeParamType;

    /// <summary>
    ///     Map related params.
    /// </summary>
    public static readonly List<string> DS2MapParamlist = new()
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

    /// <summary>
    ///     Param name - FMGCategory map
    /// </summary>
    public static readonly List<(string, FmgEntryCategory)> ParamToFmgCategoryList = new()
    {
        ("EquipParamAccessory", FmgEntryCategory.Rings),
        ("EquipParamGoods", FmgEntryCategory.Goods),
        ("EquipParamWeapon", FmgEntryCategory.Weapons),
        ("EquipParamProtector", FmgEntryCategory.Armor),
        ("EquipParamGem", FmgEntryCategory.Gem),
        ("SwordArtsParam", FmgEntryCategory.SwordArts),
        ("EquipParamGenerator", FmgEntryCategory.Generator),
        ("EquipParamFcs", FmgEntryCategory.FCS),
        ("EquipParamBooster", FmgEntryCategory.Booster),
        ("ArchiveParam", FmgEntryCategory.Archive),
        ("MissionParam", FmgEntryCategory.Mission)
    };

    private static readonly HashSet<int> EMPTYSET = new();

    private Dictionary<string, Param> _params;

    private ulong _paramVersion;

    private bool _pendingUpgrade;
    private Dictionary<string, HashSet<int>> _primaryDiffCache; //If param != primaryparam
    private Dictionary<string, List<string?>> _storedStrippedRowNames;

    /// <summary>
    ///     Dictionary of param file names that were given a tentative ParamType, and the original ParamType it had.
    ///     Used to later restore original ParamType on write (if possible).
    /// </summary>
    private Dictionary<string, string?> _usedTentativeParamTypes;

    private Dictionary<string, HashSet<int>> _vanillaDiffCache; //If param != vanillaparam
    internal AssetLocator AssetLocator;

    private Param EnemyParam;

    public static bool IsDefsLoaded { get; private set; }
    public static bool IsMetaLoaded { get; private set; }
    public bool IsLoadingParams { get; private set; }

    public IReadOnlyDictionary<string, Param> Params
    {
        get
        {
            if (IsLoadingParams)
            {
                return null;
            }

            return _params;
        }
    }

    public ulong ParamVersion => _paramVersion;

    public IReadOnlyDictionary<string, HashSet<int>> VanillaDiffCache
    {
        get
        {
            if (IsLoadingParams)
            {
                return null;
            }

            {
                if (VanillaBank == this)
                {
                    return null;
                }
            }
            return _vanillaDiffCache;
        }
    }

    public IReadOnlyDictionary<string, HashSet<int>> PrimaryDiffCache
    {
        get
        {
            if (IsLoadingParams)
            {
                return null;
            }

            {
                if (PrimaryBank == this)
                {
                    return null;
                }
            }
            return _primaryDiffCache;
        }
    }

    private static FileNotFoundException CreateParamMissingException(GameType type)
    {
        if (type is GameType.DarkSoulsPTDE or GameType.Sekiro)
        {
            return new FileNotFoundException(
                $"Cannot locate param files for {type}.\nThis game must be unpacked before modding, please use UXM Selective Unpacker.");
        }

        if (type is GameType.DemonsSouls or GameType.Bloodborne)
        {
            return new FileNotFoundException(
                $"Cannot locate param files for {type}.\nYour game folder may be missing game files.");
        }

        return new FileNotFoundException(
            $"Cannot locate param files for {type}.\nYour game folder may be missing game files, please verify game files through steam to restore them.");
    }

    private static List<(string, PARAMDEF)> LoadParamdefs(AssetLocator assetLocator)
    {
        _paramdefs = new Dictionary<string, PARAMDEF>();
        _tentativeParamType = new Dictionary<string, string>();
        var dir = assetLocator.GetParamdefDir();
        var files = Directory.GetFiles(dir, "*.xml");
        List<(string, PARAMDEF)> defPairs = new();
        foreach (var f in files)
        {
            PARAMDEF pdef = PARAMDEF.XmlDeserialize(f, true);
            _paramdefs.Add(pdef.ParamType, pdef);
            defPairs.Add((f, pdef));
        }

        var tentativeMappingPath = assetLocator.GetTentativeParamTypePath();
        if (File.Exists(tentativeMappingPath))
        {
            // No proper CSV library is used currently, and all CSV parsing is in the context of param files.
            // If a CSV library is introduced in DSMapStudio, use it here.
            foreach (var line in File.ReadAllLines(tentativeMappingPath).Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new FormatException($"Malformed line in {tentativeMappingPath}: {line}");
                }

                _tentativeParamType[parts[0]] = parts[1];
            }
        }

        return defPairs;
    }

    public static void LoadParamMeta(List<(string, PARAMDEF)> defPairs, AssetLocator assetLocator)
    {
        var mdir = assetLocator.GetParammetaDir();
        foreach ((var f, PARAMDEF pdef) in defPairs)
        {
            var fName = f.Substring(f.LastIndexOf('\\') + 1);
            ParamMetaData.XmlDeserialize($@"{mdir}\{fName}", pdef);
        }
    }

    public CompoundAction LoadParamDefaultNames(string param = null, bool onlyAffectEmptyNames = false, bool onlyAffectVanillaNames = false)
    {
        var dir = AssetLocator.GetParamNamesDir();
        var files = param == null
            ? Directory.GetFiles(dir, "*.txt")
            : new[] { Path.Combine(dir, $"{param}.txt") };
        List<EditorAction> actions = new();
        foreach (var f in files)
        {
            var fName = Path.GetFileNameWithoutExtension(f);
            if (!_params.ContainsKey(fName))
            {
                continue;
            }

            var names = File.ReadAllText(f);
            (var result, CompoundAction action) =
                ParamIO.ApplySingleCSV(this, names, fName, "Name", ' ', true, onlyAffectEmptyNames, onlyAffectVanillaNames);
            if (action == null)
            {
                TaskLogs.AddLog($"Could not apply name files for {fName}",
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

    private void LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version,
        bool checkVersion = false)
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

            Param p;

            if (AssetLocator.Type == GameType.ArmoredCoreVI)
            {
                _usedTentativeParamTypes = new Dictionary<string, string>();
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!string.IsNullOrEmpty(p.ParamType))
                {
                    if (!_paramdefs.ContainsKey(p.ParamType))
                    {
                        if (_tentativeParamType.TryGetValue(paramName, out var newParamType))
                        {
                            _usedTentativeParamTypes.Add(paramName, p.ParamType);
                            p.ParamType = newParamType;
                            TaskLogs.AddLog(
                                $"Couldn't find ParamDef for {paramName}, but tentative ParamType \"{newParamType}\" exists.",
                                LogLevel.Debug);
                        }
                        else
                        {
                            TaskLogs.AddLog(
                                $"Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.",
                                LogLevel.Error, TaskLogs.LogPriority.High);
                            continue;
                        }
                    }
                }
                else
                {
                    if (_tentativeParamType.TryGetValue(paramName, out var newParamType))
                    {
                        _usedTentativeParamTypes.Add(paramName, p.ParamType);
                        p.ParamType = newParamType;
                        TaskLogs.AddLog(
                            $"Couldn't read ParamType for {paramName}, but tentative ParamType \"{newParamType}\" exists.",
                            LogLevel.Debug);
                    }
                    else
                    {
                        TaskLogs.AddLog(
                            $"Couldn't read ParamType for {paramName} and no tentative ParamType exists.",
                            LogLevel.Error, TaskLogs.LogPriority.High);
                        continue;
                    }
                }
            }
            else
            {
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!_paramdefs.ContainsKey(p.ParamType ?? ""))
                {
                    TaskLogs.AddLog(
                        $"Couldn't find ParamDef for param {paramName} with ParamType \"{p.ParamType}\".",
                        LogLevel.Warning);
                    continue;
                }
            }

            // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
            // it's an easy fixup
            if (AssetLocator.Type == GameType.EldenRing &&
                p.ParamType == "CHR_MODEL_PARAM_ST" &&
                version == 10601000)
            {
                p.FixupERChrModelParam();
            }

            if (p.ParamType == null)
            {
                throw new Exception("Param type is unexpectedly null");
            }

            PARAMDEF def = _paramdefs[p.ParamType];
            try
            {
                p.ApplyParamdef(def, version);
                paramBank.Add(paramName, p);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                var message = $"Could not apply ParamDef for {name}";

                if (AssetLocator.Type == GameType.DarkSoulsRemastered &&
                    name is "m99_ToneMapBank.param" or "m99_ToneCorrectBank.param"
                        or "default_ToneCorrectBank.param")
                {
                    // Known cases that don't affect standard modmaking
                    TaskLogs.AddLog(message,
                        LogLevel.Warning, TaskLogs.LogPriority.Low);
                }
                else
                {
                    TaskLogs.AddLog(message,
                        LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
                }
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
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;

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
            throw CreateParamMissingException(AssetLocator.Type);
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
    }

    private void LoadVParamsDES()
    {
        var paramBinderName = GetDesGameparamName(AssetLocator.GameRootDirectory);

        LoadParamsDESFromFile($@"{AssetLocator.GameRootDirectory}\param\gameparam\{paramBinderName}");
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\drawparam"))
        {
            foreach (var p in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\drawparam",
                         "*.parambnd.dcx"))
            {
                LoadParamsDS1FromFile(p);
            }
        }
    }

    private void LoadParamsDESFromFile(string path)
    {
        using BND3 bnd = BND3.Read(path);
        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
    }

    private void LoadParamsDS1()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
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
    }

    private void LoadVParamsDS1()
    {
        LoadParamsDS1FromFile($@"{AssetLocator.GameRootDirectory}\param\GameParam\GameParam.parambnd");
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\DrawParam",
                         "*.parambnd"))
            {
                LoadParamsDS1FromFile(p);
            }
        }
    }

    private void LoadParamsDS1FromFile(string path)
    {
        using BND3 bnd = BND3.Read(path);
        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
    }

    private void LoadParamsDS1R()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd.dcx"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
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
    }

    private void LoadVParamsDS1R()
    {
        LoadParamsDS1RFromFile($@"{AssetLocator.GameRootDirectory}\param\GameParam\GameParam.parambnd.dcx");
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\DrawParam"))
        {
            foreach (var p in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\DrawParam",
                         "*.parambnd.dcx"))
            {
                LoadParamsDS1FromFile(p);
            }
        }
    }

    private void LoadParamsDS1RFromFile(string path)
    {
        using BND3 bnd = BND3.Read(path);
        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
    }

    private void LoadParamsBBSekiro()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\gameparam\gameparam.parambnd.dcx"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        // Load params
        var param = $@"{mod}\param\gameparam\gameparam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\gameparam.parambnd.dcx";
        }

        LoadParamsBBSekiroFromFile(param);
    }

    private void LoadVParamsBBSekiro()
    {
        LoadParamsBBSekiroFromFile($@"{AssetLocator.GameRootDirectory}\param\gameparam\gameparam.parambnd.dcx");
    }

    private void LoadParamsBBSekiroFromFile(string path)
    {
        using BND4 bnd = BND4.Read(path);
        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
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

    private void LoadParamsDS2(bool loose)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\enc_regulation.bnd.dcx"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        if (!BND4.Is($@"{dir}\enc_regulation.bnd.dcx"))
        {
            PlatformUtils.Instance.MessageBox(
                "Attempting to decrypt DS2 regulation file, else functionality will be limited.", "",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            //return;
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

        LoadParamsDS2FromFile(looseParams, param, enemyFile, loose);
        LoadExternalRowNames();
    }

    private void LoadVParamsDS2(bool loose)
    {
        if (!File.Exists($@"{AssetLocator.GameRootDirectory}\enc_regulation.bnd.dcx"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        if (!BND4.Is($@"{AssetLocator.GameRootDirectory}\enc_regulation.bnd.dcx"))
        {
            PlatformUtils.Instance.MessageBox(
                "Attempting to decrypt DS2 regulation file, else functionality will be limited.", "",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Load loose params
        List<string> looseParams = GetLooseParamsInDir(AssetLocator.GameRootDirectory);

        LoadParamsDS2FromFile(looseParams, $@"{AssetLocator.GameRootDirectory}\enc_regulation.bnd.dcx",
            $@"{AssetLocator.GameRootDirectory}\Param\EnemyParam.param", loose);
    }

    private void LoadParamsDS2FromFile(List<string> looseParams, string path, string enemypath, bool loose)
    {
        BND4 paramBnd;
        if (!BND4.Is(path))
        {
            paramBnd = SFUtil.DecryptDS2Regulation(path);
        }
        // No need to decrypt
        else
        {
            paramBnd = BND4.Read(path);
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
                PARAMDEF def = _paramdefs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
            }
        }

        LoadParamFromBinder(paramBnd, ref _params, out _paramVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            Param lp = Param.Read(p);
            var fname = lp.ParamType;

            try
            {
                if (loose)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = _paramdefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!_params.ContainsKey(name))
                    {
                        PARAMDEF def = _paramdefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"Could not apply ParamDef for {fname}";
                if (AssetLocator.Type == GameType.DarkSoulsIISOTFS &&
                    fname is "GENERATOR_DBG_LOCATION_PARAM")
                {
                    // Known cases that don't affect standard modmaking
                    TaskLogs.AddLog(message,
                        LogLevel.Warning, TaskLogs.LogPriority.Low);
                }
                else
                {
                    TaskLogs.AddLog(message,
                        LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
                }
            }
        }

        paramBnd.Dispose();
    }

    private void LoadParamsDS3(bool loose)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\Data0.bdt"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        var vparam = $@"{dir}\Data0.bdt";
        // Load loose params if they exist
        if (loose && File.Exists($@"{mod}\\param\gameparam\gameparam_dlc2.parambnd.dcx"))
        {
            LoadParamsDS3FromFile($@"{mod}\param\gameparam\gameparam_dlc2.parambnd.dcx", true);
        }
        else
        {
            var param = $@"{mod}\Data0.bdt";
            if (!File.Exists(param))
            {
                param = vparam;
            }

            LoadParamsDS3FromFile(param, false);
        }
    }

    private void LoadVParamsDS3()
    {
        LoadParamsDS3FromFile($@"{AssetLocator.GameRootDirectory}\Data0.bdt", false);
    }

    private void LoadParamsDS3FromFile(string path, bool isLoose)
    {
        using BND4 lparamBnd = isLoose ? BND4.Read(path) : SFUtil.DecryptDS3Regulation(path);
        LoadParamFromBinder(lparamBnd, ref _params, out _paramVersion);
    }

    private void LoadParamsER(bool partial)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param) || partial)
        {
            param = $@"{dir}\regulation.bin";
        }

        LoadParamsERFromFile(param);

        param = $@"{mod}\regulation.bin";
        if (partial && File.Exists(param))
        {
            using BND4 pParamBnd = SFUtil.DecryptERRegulation(param);
            Dictionary<string, Param> cParamBank = new();
            ulong v;
            LoadParamFromBinder(pParamBnd, ref cParamBank, out v, true);
            foreach (KeyValuePair<string, Param> pair in cParamBank)
            {
                Param baseParam = _params[pair.Key];
                foreach (Param.Row row in pair.Value.Rows)
                {
                    Param.Row bRow = baseParam[row.ID];
                    if (bRow == null)
                    {
                        baseParam.AddRow(row);
                    }
                    else
                    {
                        bRow.Name = row.Name;
                        foreach (Param.Column field in bRow.Columns)
                        {
                            Param.Cell cell = bRow[field];
                            cell.Value = row[field].Value;
                        }
                    }
                }
            }
        }
    }

    private void LoadVParamsER()
    {
        LoadParamsERFromFile($@"{AssetLocator.GameRootDirectory}\regulation.bin");
    }

    private void LoadParamsERFromFile(string path)
    {
        LoadParamFromBinder(SFUtil.DecryptERRegulation(path), ref _params, out _paramVersion, true);
    }

    private void LoadParamsAC6()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            throw CreateParamMissingException(AssetLocator.Type);
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param))
        {
            param = $@"{dir}\regulation.bin";
        }

        LoadParamsAC6FromFile(param);
    }

    private void LoadVParamsAC6()
    {
        LoadParamsAC6FromFile($@"{AssetLocator.GameRootDirectory}\regulation.bin");
    }

    private void LoadParamsAC6FromFile(string path)
    {
        LoadParamFromBinder(SFUtil.DecryptAC6Regulation(path), ref _params, out _paramVersion, true);
    }

    //Some returns and repetition, but it keeps all threading and loading-flags visible inside this method
    public static void ReloadParams(ProjectSettings settings, NewProjectOptions options)
    {
        // Steal assetlocator from PrimaryBank.
        AssetLocator locator = PrimaryBank.AssetLocator;

        _paramdefs = new Dictionary<string, PARAMDEF>();
        IsDefsLoaded = false;
        IsMetaLoaded = false;

        AuxBanks = new Dictionary<string, ParamBank>();

        PrimaryBank._params = new Dictionary<string, Param>();
        PrimaryBank.IsLoadingParams = true;

        UICache.ClearCaches();

        TaskManager.Run(new TaskManager.LiveTask("Param - Load Params", TaskManager.RequeueType.WaitThenRequeue,
            false, () =>
            {
                if (PrimaryBank.AssetLocator.Type != GameType.Undefined)
                {
                    List<(string, PARAMDEF)> defPairs = LoadParamdefs(locator);
                    IsDefsLoaded = true;
                    TaskManager.Run(new TaskManager.LiveTask("Param - Load Meta",
                        TaskManager.RequeueType.WaitThenRequeue, false, () =>
                        {
                            LoadParamMeta(defPairs, locator);
                            IsMetaLoaded = true;
                        }));
                }

                if (locator.Type == GameType.DemonsSouls)
                {
                    PrimaryBank.LoadParamsDES();
                }

                if (locator.Type == GameType.DarkSoulsPTDE)
                {
                    PrimaryBank.LoadParamsDS1();
                }

                if (locator.Type == GameType.DarkSoulsRemastered)
                {
                    PrimaryBank.LoadParamsDS1R();
                }

                if (locator.Type == GameType.DarkSoulsIISOTFS)
                {
                    PrimaryBank.LoadParamsDS2(settings.UseLooseParams);
                }

                if (locator.Type == GameType.DarkSoulsIII)
                {
                    PrimaryBank.LoadParamsDS3(settings.UseLooseParams);
                }

                if (locator.Type == GameType.Bloodborne || locator.Type == GameType.Sekiro)
                {
                    PrimaryBank.LoadParamsBBSekiro();
                }

                if (locator.Type == GameType.EldenRing)
                {
                    PrimaryBank.LoadParamsER(settings.PartialParams);
                }

                if (locator.Type == GameType.ArmoredCoreVI)
                {
                    PrimaryBank.LoadParamsAC6();
                }

                PrimaryBank.ClearParamDiffCaches();
                PrimaryBank.IsLoadingParams = false;

                VanillaBank.IsLoadingParams = true;
                VanillaBank._params = new Dictionary<string, Param>();
                TaskManager.Run(new TaskManager.LiveTask("Param - Load Vanilla Params",
                    TaskManager.RequeueType.WaitThenRequeue, false, () =>
                    {
                        if (locator.Type == GameType.DemonsSouls)
                        {
                            VanillaBank.LoadVParamsDES();
                        }

                        if (locator.Type == GameType.DarkSoulsPTDE)
                        {
                            VanillaBank.LoadVParamsDS1();
                        }

                        if (locator.Type == GameType.DarkSoulsRemastered)
                        {
                            VanillaBank.LoadVParamsDS1R();
                        }

                        if (locator.Type == GameType.DarkSoulsIISOTFS)
                        {
                            VanillaBank.LoadVParamsDS2(settings.UseLooseParams);
                        }

                        if (locator.Type == GameType.DarkSoulsIII)
                        {
                            VanillaBank.LoadVParamsDS3();
                        }

                        if (locator.Type == GameType.Bloodborne || locator.Type == GameType.Sekiro)
                        {
                            VanillaBank.LoadVParamsBBSekiro();
                        }

                        if (locator.Type == GameType.EldenRing)
                        {
                            VanillaBank.LoadVParamsER();
                        }

                        if (locator.Type == GameType.ArmoredCoreVI)
                        {
                            VanillaBank.LoadVParamsAC6();
                        }

                        VanillaBank.IsLoadingParams = false;

                        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                            TaskManager.RequeueType.WaitThenRequeue, false,
                            () => RefreshAllParamDiffCaches(true)));
                    }));

                if (options != null)
                {
                    if (options.loadDefaultNames)
                    {
                        try
                        {
                            new ActionManager().ExecuteAction(PrimaryBank.LoadParamDefaultNames());
                            PrimaryBank.SaveParams(settings.UseLooseParams);
                        }
                        catch
                        {
                            TaskLogs.AddLog("Could not locate or apply name files",
                                LogLevel.Warning);
                        }
                    }
                }
            }));
    }

    public static void LoadAuxBank(string path, string looseDir, string enemyPath, ProjectSettings settings)
    {
        // Steal assetlocator
        AssetLocator locator = PrimaryBank.AssetLocator;
        ParamBank newBank = new();
        newBank.SetAssetLocator(locator);
        newBank._params = new Dictionary<string, Param>();
        newBank.IsLoadingParams = true;
        if (locator.Type == GameType.ArmoredCoreVI)
        {
            newBank.LoadParamsAC6FromFile(path);
        }
        else if (locator.Type == GameType.EldenRing)
        {
            newBank.LoadParamsERFromFile(path);
        }
        else if (locator.Type == GameType.Sekiro)
        {
            newBank.LoadParamsBBSekiroFromFile(path);
        }
        else if (locator.Type == GameType.DarkSoulsIII)
        {
            newBank.LoadParamsDS3FromFile(path, path.Trim().ToLower().EndsWith(".dcx"));
        }
        else if (locator.Type == GameType.Bloodborne)
        {
            newBank.LoadParamsBBSekiroFromFile(path);
        }
        else if (locator.Type == GameType.DarkSoulsIISOTFS)
        {
            List<string> looseParams = GetLooseParamsInDir(looseDir);
            newBank.LoadParamsDS2FromFile(looseParams, path, enemyPath, settings.UseLooseParams);
        }
        else if (locator.Type == GameType.DarkSoulsRemastered)
        {
            newBank.LoadParamsDS1RFromFile(path);
        }
        else if (locator.Type == GameType.DarkSoulsPTDE)
        {
            newBank.LoadParamsDS1FromFile(path);
        }
        else if (locator.Type == GameType.DemonsSouls)
        {
            newBank.LoadParamsDESFromFile(path);
        }

        newBank.ClearParamDiffCaches();
        newBank.IsLoadingParams = false;
        newBank.RefreshParamDiffCaches(true);
        AuxBanks[Path.GetFileName(Path.GetDirectoryName(path)).Replace(' ', '_')] = newBank;
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

    public static void RefreshAllParamDiffCaches(bool checkAuxVanillaDiff)
    {
        PrimaryBank.RefreshParamDiffCaches(true);
        foreach (KeyValuePair<string, ParamBank> bank in AuxBanks)
        {
            bank.Value.RefreshParamDiffCaches(checkAuxVanillaDiff);
        }

        UICache.ClearCaches();
    }

    public void RefreshParamDiffCaches(bool checkVanillaDiff)
    {
        if (this != VanillaBank && checkVanillaDiff)
        {
            _vanillaDiffCache = GetParamDiff(VanillaBank);
        }

        if (this == VanillaBank && PrimaryBank._vanillaDiffCache != null)
        {
            _primaryDiffCache = PrimaryBank._vanillaDiffCache;
        }
        else if (this != PrimaryBank)
        {
            _primaryDiffCache = GetParamDiff(PrimaryBank);
        }

        UICache.ClearCaches();
    }

    private Dictionary<string, HashSet<int>> GetParamDiff(ParamBank otherBank)
    {
        if (IsLoadingParams || otherBank == null || otherBank.IsLoadingParams)
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

    private static void RefreshParamRowDiffCache(Param.Row row, ReadOnlySpan<Param.Row> otherBankRows,
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

    public void RefreshParamRowDiffs(Param.Row row, string param)
    {
        if (param == null)
        {
            return;
        }

        if (VanillaBank.Params.ContainsKey(param) && VanillaDiffCache != null &&
            VanillaDiffCache.ContainsKey(param))
        {
            Param.Row[] otherBankRows = VanillaBank.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, VanillaDiffCache[param]);
        }

        if (this != PrimaryBank)
        {
            return;
        }

        foreach (ParamBank aux in AuxBanks.Values)
        {
            if (!aux.Params.ContainsKey(param) || aux.PrimaryDiffCache == null ||
                !aux.PrimaryDiffCache.ContainsKey(param))
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

    public void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
        //ReloadParams();
    }

    private void SaveParamsDS1()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd";
        }

        using BND3 paramBnd = BND3.Read(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"param\GameParam\GameParam.parambnd", paramBnd);

        // Drawparam
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\DrawParam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\DrawParam",
                         "*.parambnd"))
            {
                using BND3 drawParamBnd = BND3.Read(bnd);
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
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\GameParam\GameParam.parambnd.dcx"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\GameParam\GameParam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\GameParam\GameParam.parambnd.dcx";
        }

        using BND3 paramBnd = BND3.Read(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"param\GameParam\GameParam.parambnd.dcx", paramBnd);

        // Drawparam
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\DrawParam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\DrawParam",
                         "*.parambnd.dcx"))
            {
                using BND3 drawParamBnd = BND3.Read(bnd);
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

    private void SaveParamsDS2(bool loose)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\enc_regulation.bnd.dcx"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
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

        if (!loose)
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
                    {
                        paramBnd = BND4.Read(param);
                    }
                }
            }

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                StripRowNames();

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
                RestoreStrippedRowNames();
                throw;
            }

            RestoreStrippedRowNames();
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
                StripRowNames();

                // Write params to loose files.
                foreach (KeyValuePair<string, Param> p in _params)
                {
                    Utils.WriteWithBackup(dir, mod, $@"Param\{p.Key}.param", p.Value);
                }
            }
            catch
            {
                RestoreStrippedRowNames();
                throw;
            }

            RestoreStrippedRowNames();
        }

        Utils.WriteWithBackup(dir, mod, @"enc_regulation.bnd.dcx", paramBnd);
        paramBnd.Dispose();
    }

    private void SaveParamsDS3(bool loose)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\Data0.bdt"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\Data0.bdt";
        if (!File.Exists(param))
        {
            param = $@"{dir}\Data0.bdt";
        }

        BND4 paramBnd = SFUtil.DecryptDS3Regulation(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // If not loose write out the new regulation
        if (!loose)
        {
            Utils.WriteWithBackup(dir, mod, @"Data0.bdt", paramBnd, GameType.DarkSoulsIII);
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

            /*BND4 stayBND = new BND4
            {
                BigEndian = false,
                Compression = DCX.Type.DCX_DFLT_10000_44_9,
                Extended = 0x04,
                Unk04 = false,
                Unk05 = false,
                Format = Binder.Format.Compression | Binder.Format.Flag6 | Binder.Format.LongOffsets | Binder.Format.Names1,
                Unicode = true,
                Files = paramBnd.Files.Where(f => f.Name.EndsWith(".stayparam")).ToList()
            };*/

            Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam_dlc2.parambnd.dcx", paramBND);
            //Utils.WriteWithBackup(dir, mod, @"param\stayparam\stayparam.parambnd.dcx", stayBND);
        }
    }

    private void SaveParamsBBSekiro()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\param\gameparam\gameparam.parambnd.dcx"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\param\gameparam\gameparam.parambnd.dcx";
        if (!File.Exists(param))
        {
            param = $@"{dir}\param\gameparam\gameparam.parambnd.dcx";
        }

        BND4 paramBnd = BND4.Read(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = _params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        Utils.WriteWithBackup(dir, mod, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);
    }

    private void SaveParamsDES()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;

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
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        using BND3 paramBnd = BND3.Read(param);

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

        // Drawparam
        List<string> drawParambndPaths = new();
        if (Directory.Exists($@"{AssetLocator.GameRootDirectory}\param\drawparam"))
        {
            foreach (var bnd in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\drawparam",
                         "*.parambnd.dcx"))
            {
                drawParambndPaths.Add(bnd);
            }

            // Also save decompressed parambnds because DeS debug uses them.
            foreach (var bnd in Directory.GetFiles($@"{AssetLocator.GameRootDirectory}\param\drawparam",
                         "*.parambnd"))
            {
                drawParambndPaths.Add(bnd);
            }

            foreach (var bnd in drawParambndPaths)
            {
                using BND3 drawParamBnd = BND3.Read(bnd);
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

    private void SaveParamsER(bool partial)
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param) || _pendingUpgrade)
        {
            param = $@"{dir}\regulation.bin";
        }

        BND4 paramBnd = SFUtil.DecryptERRegulation(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (_params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                Param paramFile = _params[Path.GetFileNameWithoutExtension(p.Name)];
                IReadOnlyList<Param.Row> backup = paramFile.Rows;
                List<Param.Row> changed = new();
                if (partial)
                {
                    TaskManager.WaitAll(); //wait on dirtycache update
                    HashSet<int> dirtyCache = _vanillaDiffCache[Path.GetFileNameWithoutExtension(p.Name)];
                    foreach (Param.Row row in paramFile.Rows)
                    {
                        if (dirtyCache.Contains(row.ID))
                        {
                            changed.Add(row);
                        }
                    }

                    paramFile.Rows = changed;
                }

                p.Bytes = paramFile.Write();
                paramFile.Rows = backup;
            }
        }

        Utils.WriteWithBackup(dir, mod, @"regulation.bin", paramBnd, GameType.EldenRing);
        _pendingUpgrade = false;
    }

    private void SaveParamsAC6()
    {
        var dir = AssetLocator.GameRootDirectory;
        var mod = AssetLocator.GameModDirectory;
        if (!File.Exists($@"{dir}\\regulation.bin"))
        {
            TaskLogs.AddLog("Cannot locate param files. Save failed.",
                LogLevel.Error, TaskLogs.LogPriority.High);
            return;
        }

        // Load params
        var param = $@"{mod}\regulation.bin";
        if (!File.Exists(param) || _pendingUpgrade)
        {
            param = $@"{dir}\regulation.bin";
        }

        BND4 paramBnd = SFUtil.DecryptAC6Regulation(param);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(p.Name);
            if (_params.TryGetValue(paramName, out Param paramFile))
            {
                IReadOnlyList<Param.Row> backup = paramFile.Rows;
                if (AssetLocator.Type is GameType.ArmoredCoreVI)
                {
                    if (_usedTentativeParamTypes.TryGetValue(paramName, out var oldParamType))
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

        Utils.WriteWithBackup(dir, mod, @"regulation.bin", paramBnd, GameType.ArmoredCoreVI);
        _pendingUpgrade = false;
    }

    public void SaveParams(bool loose = false, bool partialParams = false)
    {
        if (_params == null)
        {
            return;
        }

        if (AssetLocator.Type == GameType.DarkSoulsPTDE)
        {
            SaveParamsDS1();
        }

        if (AssetLocator.Type == GameType.DarkSoulsRemastered)
        {
            SaveParamsDS1R();
        }

        if (AssetLocator.Type == GameType.DemonsSouls)
        {
            SaveParamsDES();
        }

        if (AssetLocator.Type == GameType.DarkSoulsIISOTFS)
        {
            SaveParamsDS2(loose);
        }

        if (AssetLocator.Type == GameType.DarkSoulsIII)
        {
            SaveParamsDS3(loose);
        }

        if (AssetLocator.Type == GameType.Bloodborne || AssetLocator.Type == GameType.Sekiro)
        {
            SaveParamsBBSekiro();
        }

        if (AssetLocator.Type == GameType.EldenRing)
        {
            SaveParamsER(partialParams);
        }

        if (AssetLocator.Type == GameType.ArmoredCoreVI)
        {
            SaveParamsAC6();
        }
    }

    private static Param UpgradeParam(Param source, Param oldVanilla, Param newVanilla, HashSet<int> rowConflicts)
    {
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
                if ((modrow.Name == null && row.Name == null) ||
                    (modrow.Name != null && row.Name != null && modrow.Name == row.Name))
                {
                    editOperations[row.ID].Add(EditOperation.Match);
                    continue;
                }

                // Name was updated
                editOperations[row.ID].Add(EditOperation.NameChange);
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
            }
        }

        if (editOperations.All(kvp => kvp.Value.All(eo => eo == EditOperation.Match)))
        {
            return oldVanilla;
        }

        Param dest = new(newVanilla);

        // Now try to build the destination from the new regulation with the edit operations in mind
        var pendingAdds = addedRows.Keys.OrderBy(e => e).ToArray();
        var currPendingAdd = 0;
        var lastID = 0;
        foreach (Param.Row row in newVanilla.Rows)
        {
            // See if we have any pending adds we can slot in
            while (currPendingAdd < pendingAdds.Length &&
                   pendingAdds[currPendingAdd] >= lastID &&
                   pendingAdds[currPendingAdd] < row.ID)
            {
                if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
                {
                    currPendingAdd++;
                    continue;
                }

                foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
                {
                    dest.AddRow(new Param.Row(arow, dest));
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

    // Param upgrade. Currently for Elden Ring only.
    public ParamUpgradeResult UpgradeRegulation(ParamBank vanillaBank, string oldVanillaParamPath,
        Dictionary<string, HashSet<int>> conflictingParams)
    {
        // First we need to load the old regulation
        if (!File.Exists(oldVanillaParamPath))
        {
            return ParamUpgradeResult.OldRegulationNotFound;
        }    
        
        // Backup modded params
        string modRegulationPath = $@"{AssetLocator.GameModDirectory}\regulation.bin";
        File.Copy(modRegulationPath, $@"{modRegulationPath}.upgrade.bak", true);

        // Load old vanilla regulation
        BND4 oldVanillaParamBnd;
        if (AssetLocator.Type == GameType.EldenRing)
        {
            oldVanillaParamBnd = SFUtil.DecryptERRegulation(oldVanillaParamPath);
        }
        else if (AssetLocator.Type == GameType.ArmoredCoreVI)
        {
            oldVanillaParamBnd = SFUtil.DecryptAC6Regulation(oldVanillaParamPath);
        }
        else
        {
            throw new NotImplementedException(
                $"Param upgrading for game type {AssetLocator.Type} is not supported.");
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
            {
                anyUpgrades = true;
            }

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
        _paramVersion = VanillaBank.ParamVersion;
        _pendingUpgrade = true;

        // Refresh dirty cache
        UICache.ClearCaches();
        RefreshAllParamDiffCaches(false);

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
            var path = AssetLocator.GetStrippedRowNamesPath(p.Key);
            if (File.Exists(path))
            {
                var names = File.ReadAllLines(path);
                if (names.Length != p.Value.Rows.Count)
                {
                    TaskLogs.AddLog($"External row names could not be applied to {p.Key}, row count does not match",
                        LogLevel.Warning, TaskLogs.LogPriority.Low);
                    failCount++;
                    continue;
                }

                for (var i = 0; i < names.Length; i++)
                {
                    p.Value.Rows[i].Name = names[i];
                }
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
                list.Add(r.Name);
                r.Name = "";
            }

            var path = AssetLocator.GetStrippedRowNamesPath(p.Key);
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
