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
using Andre.IO.VFS;
using StudioCore.Utilities;
using StudioCore.Formats.JSON;
using System.Text.Json;
using Octokit;
using StudioCore.Editors.TextEditor.Enums;

namespace StudioCore.Editors.ParamEditor.Data;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class ParamBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS;

    public string Name;

    private readonly HashSet<int> EMPTYSET = new();

    public Dictionary<string, Param> _params;

    public ulong _paramVersion;

    public bool _pendingUpgrade;
    private Dictionary<string, HashSet<int>> _primaryDiffCache; //If param != primaryparam

    private Dictionary<string, string> _usedTentativeParamTypes;

    private Dictionary<string, HashSet<int>> _vanillaDiffCache; //If param != vanillaparam

    private Param EnemyParam;

    public string ClipboardParam = null;
    public List<Param.Row> ClipboardRows = new();

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

    public ParamBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Name = name;
        BaseEditor = baseEditor;
        Project = project;
        TargetFS = targetFs;
    }

    /// <summary>
    /// Load task for this Param bank
    /// </summary>
    public async Task<bool> Load()
    {
        await Task.Yield();

        _params = new Dictionary<string, Param>();

        UICache.ClearCaches();

        var successfulLoad = false;

        switch (Project.ProjectType)
        {
            case ProjectType.DES: successfulLoad = LoadParameters_DES(); break;
            case ProjectType.DS1: successfulLoad = LoadParameters_DS1(); break;
            case ProjectType.DS1R: successfulLoad = LoadParameters_DS1R(); break;
            case ProjectType.DS2: successfulLoad = LoadParameters_DS2(); break;
            case ProjectType.DS2S: successfulLoad = LoadParameters_DS2S(); break;
            case ProjectType.DS3: successfulLoad = LoadParameters_DS3(); break;
            case ProjectType.BB: successfulLoad = LoadParameters_BB(); break;
            case ProjectType.SDT: successfulLoad = LoadParameters_SDT(); break;
            case ProjectType.ER: successfulLoad = LoadParameters_ER(); break;
            case ProjectType.AC6: successfulLoad = LoadParameters_AC6(); break;
            case ProjectType.ERN: successfulLoad = LoadParameters_ERN(); break;
        }

        ClearParamDiffCaches();

        return successfulLoad;
    }

    /// <summary>
    /// Save task for this Param bank
    /// </summary>
    public async Task<bool> Save()
    {
        await Task.Yield();

        if (_params == null)
        {
            return false;
        }

        var successfulSave = false;

        switch (Project.ProjectType)
        {
            case ProjectType.DES:
                successfulSave = SaveParameters_DES(); break;
            case ProjectType.DS1:
                successfulSave = SaveParameters_DS1(); break;
            case ProjectType.DS1R:
                successfulSave = SaveParameters_DS1R(); break;
            case ProjectType.DS2:
                successfulSave = SaveParameters_DS2(); break;
            case ProjectType.DS2S:
                successfulSave = SaveParameters_DS2S(); break;
            case ProjectType.DS3:
                successfulSave = SaveParameters_DS3(); break;
            case ProjectType.BB:
                successfulSave = SaveParameters_BB(); break;
            case ProjectType.SDT:
                successfulSave = SaveParameters_SDT(); break;
            case ProjectType.ER:
                successfulSave = SaveParameters_ER(); break;
            case ProjectType.AC6:
                successfulSave = SaveParameters_AC6(); break;
            case ProjectType.ERN:
                successfulSave = SaveParameters_ERN(); break;
            default: break;
        }

        return successfulSave;
    }

    #region Param Load
    private void LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version, bool checkVersion = false, string parambndFileName = "")
    {
        var success = ulong.TryParse(parambnd.Version, out version);

        if (checkVersion && !success)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to get regulation version. Params might be corrupt.", LogLevel.Error, Tasks.LogPriority.High);
            return;
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

            Param curParam;

            // AC6/SDT - Tentative ParamTypes
            if (Project.ProjectType is ProjectType.AC6 or ProjectType.SDT)
            {
                _usedTentativeParamTypes = new Dictionary<string, string>();
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                // Missing paramtype
                if (!string.IsNullOrEmpty(curParam.ParamType))
                {
                    if (!Project.ParamData.ParamDefs.ContainsKey(curParam.ParamType) || Project.ParamData.ParamTypeInfo.Exceptions.Contains(paramName))
                    {
                        if (Project.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                        {
                            _usedTentativeParamTypes.Add(paramName, curParam.ParamType);
                            curParam.ParamType = newParamType;
                        }
                        else
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.", LogLevel.Error, Tasks.LogPriority.High);

                            continue;
                        }
                    }
                }
                else
                {
                    if (Project.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                    {
                        _usedTentativeParamTypes.Add(paramName, curParam.ParamType);

                        curParam.ParamType = newParamType;
                    }
                    else
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Couldn't read ParamType for {paramName} and no tentative ParamType exists.", LogLevel.Error, Tasks.LogPriority.High);

                        continue;
                    }
                }
            }
            // Normal
            else
            {
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                if (!Project.ParamData.ParamDefs.ContainsKey(curParam.ParamType ?? ""))
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Couldn't find ParamDef for param {paramName} with ParamType \"{curParam.ParamType}\".", LogLevel.Error, Tasks.LogPriority.High);

                    continue;
                }
            }

            ApplyParamFixups(curParam);

            if (curParam.ParamType == null)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Param type is unexpectedly null", LogLevel.Error, Tasks.LogPriority.High);
            }

            // Skip these for DS1 so the param load is not slowed down by the catching
            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if (paramName is "m99_ToneCorrectBank" or "m99_ToneMapBank" or "default_ToneCorrectBank")
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Skipped this param: {paramName}");
                    continue;
                }
            }

            PARAMDEF def = Project.ParamData.ParamDefs[curParam.ParamType];

            try
            {
                curParam.ApplyParamdef(def, version);
                paramBank.Add(paramName, curParam);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Could not apply ParamDef for {name}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }
    }

    private void ApplyParamFixups(Param p)
    {
        // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
        // it's an easy fixup
        if (Project.ProjectType is ProjectType.ER && ParamVersion >= 10601000)
        {
            if (p.ParamType == "CHR_MODEL_PARAM_ST")
            {
                if (p.FixupERField(12, 16))
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] CHR_MODEL_PARAM_ST fixed up.");
            }
        }

        // Add in the new data for these two params added in 1.12.1
        if (Project.ProjectType is ProjectType.ER && ParamVersion >= 11210015)
        {
            if (p.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
            {
                if (p.FixupERField(880, 1024))
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] GAME_SYSTEM_COMMON_PARAM_ST fixed up.");
            }
            if (p.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
            {
                if (p.FixupERField(112, 144))
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] POSTURE_CONTROL_PARAM_WEP_RIGHT_ST fixed up.");
            }
            if (p.ParamType == "SIGN_PUDDLE_PARAM_ST")
            {
                if (p.FixupERField(32, 48))
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] SIGN_PUDDLE_PARAM_ST fixed up.");
            }
        }
    }
    #endregion

    #region Demon's Souls
    private bool LoadParameters_DES()
    {
        var successfulLoad = true;

        var paramPath = GetGameParam_DES(TargetFS);

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in TargetFS.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = TargetFS.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_DES()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);

        var paramPath = GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.",
                LogLevel.Error);
            return false;
        }

        var data = fs.GetFile(paramPath).GetData().ToArray();

        using var paramBnd = BND3.Read(fs.GetFile(paramPath).GetData());

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // Write all gameparam variations since we don't know which one the the game will use.
        // Compressed
        paramBnd.Compression = DCX.Type.DCX_EDGE;
        var naParamPath = @"param\gameparam\gameparamna.parambnd.dcx";

        if (fs.FileExists(naParamPath))
        {
            ProjectUtils.WriteWithBackup(Project, fs, toFs, naParamPath, paramBnd);
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        // Decompressed
        paramBnd.Compression = DCX.Type.None;
        naParamPath = @"param\gameparam\gameparamna.parambnd";
        if (fs.FileExists(naParamPath))
        {
            ProjectUtils.WriteWithBackup(Project, fs, toFs, naParamPath, paramBnd);
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\gameparam\gameparam.parambnd", paramBnd);

        // Drawparam
        List<string> drawParambndPaths = new();
        if (fs.DirectoryExists(@"param\drawparam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions($@"param\drawparam", ".parambnd", ".parambnd.dcx"))
            {
                drawParambndPaths.Add(bnd);
            }

            foreach (var bnd in drawParambndPaths)
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());

                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                ProjectUtils.WriteWithBackup(Project, fs, toFs, @$"param\drawparam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }

    /// <summary>
    /// Checks for DeS paramBNDs and returns the name of the parambnd with the highest priority.
    /// </summary>
    private string GetGameParam_DES(VirtualFileSystem fs)
    {
        var name = $@"param\gameparam\gameparamna.parambnd.dcx";

        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparamna.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\gameparam\gameparam.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        return name;
    }

    #endregion

    #region Dark Souls
    private bool LoadParameters_DS1()
    {
        var successfulLoad = true;

        var paramPath = GetGameParam_DS1(TargetFS);

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in TargetFS.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = TargetFS.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_DS1()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = @"param\GameParam\GameParam.parambnd";
        if (!fs.FileExists(param))
        {
            param += ".dcx";
            if (!fs.FileExists(param))
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);
                return false;
            }
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());

        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }
        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\GameParam\GameParam.parambnd", paramBnd);

        if (fs.DirectoryExists($@"param\DrawParam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions(@"param\DrawParam", ".parambnd"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                ProjectUtils.WriteWithBackup(Project, fs, toFs, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }

    public string GetGameParam_DS1(VirtualFileSystem fs)
    {
        var name = $@"param\GameParam\GameParam.parambnd";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = $@"param\GameParam\GameParam.parambnd.dcx";
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }
        return name;
    }
    #endregion

    #region Dark Souls Remastered
    private bool LoadParameters_DS1R()
    {
        var successfulLoad = true;

        var paramPath = $@"param\GameParam\GameParam.parambnd.dcx";

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(paramPath).GetData();
                using var bnd = BND3.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }

            // Draw Params
            foreach (var f in TargetFS.FsRoot.GetDirectory("param")?.GetDirectory("drawparam")?.EnumerateFileNames() ?? [])
            {
                if (f.EndsWith(".parambnd.dcx"))
                {
                    paramPath = $"param/drawparam/{f}";

                    try
                    {
                        var data = TargetFS.GetFile(paramPath).GetData();
                        using var bnd = BND3.Read(data);
                        LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                        successfulLoad = false;
                    }
                }
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_DS1R()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project); ;
        string param = @"param\GameParam\GameParam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);
            return false;
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());
        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }
        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\GameParam\GameParam.parambnd.dcx", paramBnd);

        //DrawParam
        if (fs.DirectoryExists($@"param\DrawParam"))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions($@"param\DrawParam", ".parambnd.dcx"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                ProjectUtils.WriteWithBackup(Project, fs, toFs, @$"param\DrawParam\{Path.GetFileName(bnd)}", drawParamBnd);
            }
        }

        return successfulSave;
    }
    #endregion

    #region Dark Souls II
    private bool LoadParameters_DS2()
    {
        var successfulLoad = true;

        var paramPath = $@"enc_regulation.bnd.dcx";
        var enemyPath = $@"Param\\EnemyParam.param";

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }

        // Load loose params (prioritizing ones in mod folder)
        List<string> looseParams = ProjectUtils.GetLooseParamsInDir(TargetFS, "");

        BND4 paramBnd = null;
        byte[] data = TargetFS.GetFile(paramPath).GetData().ToArray();

        if (!BND4.Is(data))
        {
            try
            {
                paramBnd = SFUtil.DecryptDS2Regulation(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }
        else
        {
            try
            {
                paramBnd = BND4.Read(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        BinderFile bndfile = paramBnd.Files.Find(x => Path.GetFileName(x.Name) == "EnemyParam.param");

        if (bndfile != null)
        {
            EnemyParam = Param.Read(bndfile.Bytes);
        }

        // Otherwise the param is a loose param
        if (TargetFS.FileExists(enemyPath))
        {
            var paramData = TargetFS.GetFile(enemyPath).GetData();
            EnemyParam = Param.Read(paramData);
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
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        LoadParamFromBinder(paramBnd, ref _params, out _paramVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var paramData = TargetFS.GetFile(p).GetData();
            var lp = Param.Read(paramData);
            var fname = lp.ParamType;

            if (fname is "GENERATOR_DBG_LOCATION_PARAM")
                continue;

            try
            {
                if (CFG.Current.UseLooseParams)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Could not apply ParamDef for {fname}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        paramBnd.Dispose();

        return successfulLoad;
    }

    private bool SaveParameters_DS2()
    {
        // No need to duplicate code here
        var successfulSave = SaveParameters_DS2S();

        return successfulSave;
    }
    #endregion

    #region Dark Souls II: Scholar of the First Sin
    private bool LoadParameters_DS2S()
    {
        var successfulLoad = true;

        var paramPath = $@"enc_regulation.bnd.dcx";
        var enemyPath = $@"Param\\EnemyParam.param";

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }

        // Load loose params (prioritizing ones in mod folder)
        List<string> looseParams = ProjectUtils.GetLooseParamsInDir(TargetFS, "");

        BND4 paramBnd = null;
        byte[] data = TargetFS.GetFile(paramPath).GetData().ToArray();

        if (!BND4.Is(data))
        {
            try
            {
                paramBnd = SFUtil.DecryptDS2Regulation(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }
        else
        {
            try
            {
                paramBnd = BND4.Read(data);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load draw param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        BinderFile bndfile = paramBnd.Files.Find(x => Path.GetFileName(x.Name) == "EnemyParam.param");

        if (bndfile != null)
        {
            EnemyParam = Param.Read(bndfile.Bytes);
        }

        // Otherwise the param is a loose param
        if (TargetFS.FileExists(enemyPath))
        {
            var paramData = TargetFS.GetFile(enemyPath).GetData();
            EnemyParam = Param.Read(paramData);
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
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Could not apply ParamDef for {EnemyParam.ParamType}",
                    LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        LoadParamFromBinder(paramBnd, ref _params, out _paramVersion);

        foreach (var p in looseParams)
        {
            var name = Path.GetFileNameWithoutExtension(p);
            var paramData = TargetFS.GetFile(p).GetData();
            var lp = Param.Read(paramData);
            var fname = lp.ParamType;

            if (fname is "GENERATOR_DBG_LOCATION_PARAM")
                continue;

            try
            {
                if (CFG.Current.UseLooseParams)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = Project.ParamData.ParamDefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Could not apply ParamDef for {fname}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        paramBnd.Dispose();

        return successfulLoad;
    }

    private bool SaveParameters_DS2S()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"enc_regulation.bnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        BND4 paramBnd;
        var data = fs.GetFile(param).GetData().ToArray();
        if (!BND4.Is(data))
        {
            // Decrypt the file
            paramBnd = SFUtil.DecryptDS2Regulation(data);
            // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
            if (!toFs.FileExists($"{param}.bak"))
            {
                toFs.WriteFile($"{param}.bak", data);
            }
            toFs.WriteFile(param, paramBnd.Write());
        }
        else
        {
            paramBnd = BND4.Read(data);
        }

        if (!CFG.Current.UseLooseParams)
        {
            // Save params non-loosely: Replace params regulation and write remaining params loosely.
            if (paramBnd.Files.Find(e => e.Name.EndsWith(".param")) == null)
            {
                if (CFG.Current.RepackLooseDS2Params)
                {
                    paramBnd.Dispose();
                    param = $@"enc_regulation.bnd.dcx";
                    data = Project.VanillaFS.GetFile(param).GetData().ToArray();

                    if (!BND4.Is(data))
                    {
                        // Decrypt the file.
                        paramBnd = SFUtil.DecryptDS2Regulation(data);

                        // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                        if (!toFs.FileExists($@"{param}.bak"))
                        {
                            toFs.WriteFile($"{param}.bak", data);
                            toFs.WriteFile(param, paramBnd.Write());
                        }
                    }
                    else
                        paramBnd = BND4.Read(data);
                }
            }

            try
            {
                // Strip and store row names before saving, as too many row names can cause DS2 to crash.
                RowNameStrip();

                foreach (KeyValuePair<string, Param> p in Params)
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
                        ProjectUtils.WriteWithBackup(Project, fs, toFs, $@"Param\{p.Key}.param", p.Value);
                    }
                }
            }
            catch
            {
                RowNameRestore();
                throw;
            }

            RowNameRestore();
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
                RowNameStrip();

                // Write params to loose files.
                foreach (KeyValuePair<string, Param> p in Params)
                {
                    ProjectUtils.WriteWithBackup(Project, fs, toFs, $@"Param\{p.Key}.param", p.Value);
                }
            }
            catch
            {
                RowNameRestore();
                throw;
            }

            RowNameRestore();
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"enc_regulation.bnd.dcx", paramBnd);
        paramBnd.Dispose();

        return successfulSave;
    }
    #endregion

    #region Dark Souls III
    /// <summary>
    /// Dark Souls III
    /// </summary>
    private bool LoadParameters_DS3()
    {
        var successfulLoad = true;

        var looseFile = $@"param\\gameparam\\gameparam_dlc2.parambnd.dcx";
        var packedFile = $@"Data0.bdt";

        if (!TargetFS.FileExists(packedFile))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {packedFile}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            if (CFG.Current.UseLooseParams && TargetFS.FileExists(looseFile))
            {
                try
                {
                    var data = TargetFS.GetFile(looseFile).GetData();
                    using var bnd = BND4.Read(data);
                    LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {looseFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                    successfulLoad = false;
                }
            }
            else
            {
                try
                {
                    var data = TargetFS.GetFile(packedFile).GetData().ToArray();
                    using var bnd = SFUtil.DecryptDS3Regulation(data);
                    LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load game param: {packedFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                    successfulLoad = false;
                }
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_DS3()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"Data0.bdt";
        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();
        BND4 paramBnd = SFUtil.DecryptDS3Regulation(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // If not loose write out the new regulation
        if (!CFG.Current.UseLooseParams)
        {
            ProjectUtils.WriteWithBackup(Project, fs, toFs, @"Data0.bdt", paramBnd, ProjectType.DS3);
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

            ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\gameparam\gameparam_dlc2.parambnd.dcx", paramBND);
        }

        return successfulSave;
    }
    #endregion

    #region Bloodborne
    private bool LoadParameters_BB()
    {
        var successfulLoad = true;

        var paramPath = $@"param\gameparam\gameparam.parambnd.dcx";

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(paramPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_BB()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        return successfulSave;
    }
    #endregion

    #region Sekiro: Shadows Die Twice
    private bool LoadParameters_SDT()
    {
        var successfulLoad = true;

        var paramPath = $@"param\gameparam\gameparam.parambnd.dcx";

        if (!TargetFS.FileExists(paramPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {paramPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(paramPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {paramPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_SDT()
    {
        var successfulSave = true;

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"param\gameparam\gameparam.parambnd.dcx";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(p.Name);
            if (Params.TryGetValue(paramName, out Param paramFile))
            {
                IReadOnlyList<Param.Row> backup = paramFile.Rows;

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

                p.Bytes = paramFile.Write();
                paramFile.Rows = backup;
            }
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\gameparam\gameparam.parambnd.dcx", paramBnd);

        return successfulSave;
    }
    #endregion

    #region Elden Ring
    private bool LoadParameters_ER()
    {
        var successfulLoad = true;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";

        if (!TargetFS.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptERRegulation(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        if (!TargetFS.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_ER()
    {
        var successfulSave = true;

        void OverwriteParamsER(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = Params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var sourceFs = Project.FS;
        var gameFs = Project.VanillaRealFS;
        var writeFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = @"regulation.bin";

        if (!sourceFs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || _pendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptERRegulation(data);

        OverwriteParamsER(regParams);

        ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.ER);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate system param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        if (sourceFs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsER(sysParams);
            ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        return successfulSave;
    }
    #endregion

    #region Armored Core VI: Fires of Rubicon
    private bool LoadParameters_AC6()
    {
        var successfulLoad = true;

        var dataPath = Project.DataPath;
        var projectPath = Project.ProjectPath;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";
        var graphicsParamPath = $@"param\graphicsconfig\graphicsconfig.parambnd.dcx";
        var eventParamPath = $@"param\eventparam\eventparam.parambnd.dcx";

        // Game Param
        if (!TargetFS.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptAC6Regulation(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        // System Param
        if (!TargetFS.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        // Graphics Param
        if (!TargetFS.FileExists(graphicsParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {graphicsParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(graphicsParamPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {graphicsParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        // Event Param
        if (!TargetFS.FileExists(eventParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {eventParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(eventParamPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {eventParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_AC6()
    {
        var successfulSave = true;

        void OverwriteParamsAC6(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                var paramName = Path.GetFileNameWithoutExtension(p.Name);
                if (Params.TryGetValue(paramName, out Param paramFile))
                {
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

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

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var sourceFs = Project.FS;
        var gameFs = Project.VanillaRealFS;
        var writeFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = @"regulation.bin";
        if (!sourceFs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || _pendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptAC6Regulation(data);
        OverwriteParamsAC6(regParams);
        ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.AC6);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate system param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        if (sourceFs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsAC6(sysParams);
            ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, sysParam, sysParams);
        }

        var graphicsParam = @"param\graphicsconfig\graphicsconfig.parambnd.dcx";

        if (!sourceFs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate graphics param files. Save failed.", LogLevel.Error);

            return false;
        }

        if (sourceFs.TryGetFile(graphicsParam, out var graphicsParamF))
        {
            using var graphicsParams = BND4.Read(graphicsParamF.GetData());
            OverwriteParamsAC6(graphicsParams);
            ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, graphicsParam, graphicsParams);
        }

        var eventParam = @"param\eventparam\eventparam.parambnd.dcx";

        if (!sourceFs.FileExists(eventParam))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate event param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        if (sourceFs.TryGetFile(eventParam, out var eventParamF))
        {
            using var eventParams = BND4.Read(eventParamF.GetData());
            OverwriteParamsAC6(eventParams);
            ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, eventParam, eventParams);
        }

        return successfulSave;
    }
    #endregion

    #region Elden Ring: Nightreign
    private bool LoadParameters_ERN()
    {
        var successfulLoad = true;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = $@"param\systemparam\systemparam.parambnd.dcx";

        if (!TargetFS.FileExists(gameParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptERRegulation(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {gameParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        if (!TargetFS.FileExists(systemParamPath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High);
            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(systemParamPath).GetData();
                using var bnd = BND4.Read(data);
                LoadParamFromBinder(bnd, ref _params, out _);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load game param: {systemParamPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_ERN()
    {
        var successfulSave = true;

        void OverwriteParamsER(BND4 paramBnd)
        {
            // Replace params with edited ones
            foreach (BinderFile p in paramBnd.Files)
            {
                if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                {
                    Param paramFile = Params[Path.GetFileNameWithoutExtension(p.Name)];
                    IReadOnlyList<Param.Row> backup = paramFile.Rows;

                    p.Bytes = paramFile.Write();
                    paramFile.Rows = backup;
                }
            }
        }

        var fs = Project.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"regulation.bin";

        if (!fs.FileExists(param))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        BND4 regParams = SFUtil.DecryptNightreignRegulation(data);

        OverwriteParamsER(regParams);

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"regulation.bin", regParams, ProjectType.ERN);

        var sysParam = @"param\systemparam\systemparam.parambnd.dcx";

        if (!fs.FileExists(sysParam))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate system param files. Save failed.", LogLevel.Error, Tasks.LogPriority.High);

            return false;
        }

        if (fs.TryGetFile(sysParam, out var sysParamF))
        {
            using var sysParams = BND4.Read(sysParamF.GetData());
            OverwriteParamsER(sysParams);
            ProjectUtils.WriteWithBackup(Project, fs, toFs, @"param\systemparam\systemparam.parambnd.dcx", sysParams);
        }

        return successfulSave;
    }

    #endregion

    #region Param Difference Cache
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
    #endregion

    #region Utils
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
    #endregion

    #region Row Name Strip / Restore
    /// <summary>
    /// Strip and store the row names for this param bank
    /// </summary>
    public void RowNameStrip()
    {
        var store = new RowNameStore();
        store.Params = new();

        foreach (KeyValuePair<string, Param> p in Params)
        {
            var paramEntry = new RowNameParam();
            paramEntry.Name = p.Key;
            paramEntry.Entries = new();

            for (int i = 0; i < p.Value.Rows.Count; i++)
            {
                var row = p.Value.Rows[i];

                // Store
                var rowEntry = new RowNameEntry();

                rowEntry.Index = i;
                rowEntry.ID = row.ID;
                rowEntry.Name = row.Name;

                paramEntry.Entries.Add(rowEntry);

                // Strip
                p.Value.Rows[i].Name = "";
            }

            store.Params.Add(paramEntry);
        }

        var folder = ProjectUtils.GetLocalProjectFolder(Project);

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var file = Path.Combine(folder, "Stripped Row Names.json");

        var json = JsonSerializer.Serialize(store, SmithboxSerializerContext.Default.RowNameStore);

        File.WriteAllText(file, json);

        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Stripped row names and stored them in {file}");
    }

    public void RowNameRestore()
    {
        RowNameStore store = null;

        var folder = ProjectUtils.GetLocalProjectFolder(Project);
        var file = Path.Combine(folder, "Stripped Row Names.json");

        if (!File.Exists(file))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to located {file} for row name restore.", LogLevel.Error);
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    var options = new JsonSerializerOptions();
                    store = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.RowNameStore);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to deserialize {file} for row name restore.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load {file} for row name restore.", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        // Only proceed if we have row names to work with
        if (store != null)
        {
            var storeDict = store.Params.ToDictionary(e => e.Name);

            foreach (KeyValuePair<string, Param> p in Params)
            {
                if (!storeDict.ContainsKey(p.Key))
                    continue;

                var rowNames = storeDict[p.Key];
                var rowNameDict = rowNames.Entries.ToDictionary(e => e.Index);

                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    if (CFG.Current.UseIndexMatchForRowNameRestore)
                    {
                        p.Value.Rows[i].Name = rowNameDict[i].Name;
                    }
                    else
                    {
                        // ID may not be unique, so we will manually loop here
                        foreach (var entry in rowNames.Entries)
                        {
                            if (entry.ID == p.Value.Rows[i].ID)
                            {
                                p.Value.Rows[i].Name = entry.Name;
                            }
                        }
                    }
                }
            }
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Restored row names from {file}");
    }

    #endregion

    #region Row Name Import
    /// <summary>
    /// Import row names
    /// </summary>
    /// <param name="importType"></param>
    /// <param name="sourceType"></param>
    /// <param name="filepath"></param>
    public async void ImportRowNames(ImportRowNameType importType, ImportRowNameSourceType sourceType, string filepath = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask(importType, sourceType, filepath, "");
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Imported row names.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to import row names.");
        }
    }

    public async void ImportRowNamesForParam(ImportRowNameType importType, ImportRowNameSourceType sourceType, string targetParam = "", string filepath = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask(importType, sourceType, filepath, targetParam);
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Imported row names for {targetParam}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to import row names for {targetParam}");
        }
    }

    public async Task<bool> ImportRowNamesTask(ImportRowNameType importType, ImportRowNameSourceType sourceType, string filepath = "", string targetParam = "")
    {
        await Task.Yield();

        var sourceFilepath = filepath;
        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";

        switch (sourceType)
        {
            case ImportRowNameSourceType.Community:
                sourceFilepath = Path.Combine(folder, "Community Row Names.json");
                break;
            case ImportRowNameSourceType.Developer:
                sourceFilepath = Path.Combine(folder, "Developer Row Names.json");
                break;
        }

        if (!File.Exists(sourceFilepath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {sourceFilepath}");
            return false;
        }

        RowNameStore store = null;

        try
        {
            var filestring = File.ReadAllText(sourceFilepath);
            var options = new JsonSerializerOptions();
            store = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.RowNameStore);

            if (store == null)
            {
                throw new Exception($"[{Project.ProjectName}:Param Editor:{Name}] JsonConvert returned null.");
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load {sourceFilepath} for row name import.", LogLevel.Error, Tasks.LogPriority.High, e);
        }

        if (store == null)
            return false;

        var storeDict = store.Params.ToDictionary(e => e.Name);

        foreach (KeyValuePair<string, Param> p in Params)
        {
            if (!storeDict.ContainsKey(p.Key))
                continue;

            if(targetParam != "")
            {
                if (p.Key != targetParam)
                    continue;
            }

            var rowNames = storeDict[p.Key];
            var rowNameDict = rowNames.Entries.ToDictionary(e => e.Index);

            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                if (importType is ImportRowNameType.Index)
                {
                    if (rowNameDict.ContainsKey(i))
                    {
                        p.Value.Rows[i].Name = rowNameDict[i].Name;
                    }
                }
                else if (importType is ImportRowNameType.ID)
                {
                    // ID may not be unique, so we will manually loop here
                    foreach (var entry in rowNames.Entries)
                    {
                        if (entry.ID == p.Value.Rows[i].ID)
                        {
                            p.Value.Rows[i].Name = entry.Name;
                        }
                    }
                }
            }
        }

        return true;
    }

    #endregion

    #region Row Name Export
    /// <summary>
    /// Export row names
    /// </summary>
    /// <param name="exportType"></param>
    /// <param name="filepath"></param>
    /// <param name="paramName"></param>
    public async void ExportRowNames(ExportRowNameType exportType, string filepath, string paramName = "")
    {
        Task<bool> exportRowNameTask = ExportRowNamesTask(exportType, filepath, paramName);
        bool rowNamesExported = await exportRowNameTask;

        if (rowNamesExported)
        {
            // JSON
            if (exportType is ExportRowNameType.JSON)
            {
                var outputName = $"{Project.ProjectName} -- Row Names.json";

                if (paramName != "")
                {
                    outputName = $"{Project.ProjectName} -- {paramName} -- Row Names.json";
                }

                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Exported row names to {outputName}");
            }

            // Text
            if (exportType is ExportRowNameType.Text)
            {
                var outputName = $"{Project.ProjectName} -- Row Names.txt";

                if (paramName != "")
                {
                    outputName = $"{Project.ProjectName} -- {paramName} -- Row Names.txt";
                }

                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Exported row names to {outputName}");
            }
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to export row names.");
        }
    }

    public async Task<bool> ExportRowNamesTask(ExportRowNameType exportType, string filepath, string paramName = "")
    {
        await Task.Yield();

        var store = new RowNameStore();
        store.Params = new();

        foreach (KeyValuePair<string, Param> p in Params)
        {
            if (paramName != "")
            {
                if (p.Key != paramName)
                    continue;
            }

            var paramEntry = new RowNameParam();
            paramEntry.Name = p.Key;
            paramEntry.Entries = new();

            for (int i = 0; i < p.Value.Rows.Count; i++)
            {
                var row = p.Value.Rows[i];

                // Store
                var rowEntry = new RowNameEntry();

                rowEntry.Index = i;
                rowEntry.ID = row.ID;
                rowEntry.Name = row.Name;

                paramEntry.Entries.Add(rowEntry);
            }

            store.Params.Add(paramEntry);
        }

        // JSON
        if (exportType is ExportRowNameType.JSON)
        {
            var outputName = $"{Project.ProjectName} -- Row Names.json";

            if (paramName != "")
            {
                outputName = $"{Project.ProjectName} -- {paramName} -- Row Names.json";
            }

            var file = Path.Combine(filepath, outputName);

            var json = JsonSerializer.Serialize(store, SmithboxSerializerContext.Default.RowNameStore);

            File.WriteAllText(file, json);

            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Exported row names to {file}");
        }

        // Text
        if (exportType is ExportRowNameType.Text)
        {
            var outputName = $"{Project.ProjectName} -- Row Names.txt";

            if (paramName != "")
            {
                outputName = $"{Project.ProjectName} -- {paramName} -- Row Names.txt";
            }

            var file = Path.Combine(filepath, outputName);

            var textOuput = "";

            foreach (var entry in store.Params)
            {
                if (paramName != "")
                {
                    if (entry.Name != paramName)
                        continue;
                }
                textOuput = $"{textOuput}\n##{entry.Name}";

                foreach (var row in entry.Entries)
                {
                    textOuput = $"{textOuput}\n{row.ID};{row.Name}";
                }
            }


            File.WriteAllText(file, textOuput);
        }

        return true;
    }

    #endregion

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

    public enum ImportRowNameType
    {
        Index,
        ID
    }

    public enum ImportRowNameSourceType
    {
        Community,
        Developer,
        External
    }

    public enum ExportRowNameType
    {
        JSON,
        Text
    }

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
    
    // Legacy CSV import
    public async void ImportRowNamesForParam_CSV(string filepath = "", string targetParam = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask_CSV(filepath, targetParam);
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Imported row names for {targetParam}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to import row names for {targetParam}");
        }
    }

    public async Task<bool> ImportRowNamesTask_CSV(string filepath = "", string targetParam = "")
    {
        await Task.Yield();

        var sourceFilepath = filepath;

        if (!File.Exists(sourceFilepath))
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {sourceFilepath}");
            return false;
        }

        try
        {
            var filestring = File.ReadAllText(sourceFilepath);

            var entries = filestring.Split("\n");
            var mapping = new Dictionary<int, string>();
            foreach(var entry in entries)
            {
                // TODO: add CFG option for the delimiter
                var parts = entry.Split(";");
                if(parts.Length > 1)
                {
                    var id = parts[0];
                    var name = parts[1];

                    try
                    {
                        var realID = int.Parse(id);

                        mapping.Add(realID, name);
                    }
                    catch { }
                }
            }

            foreach (KeyValuePair<string, Param> p in Params)
            {
                if (targetParam != "")
                {
                    if (p.Key != targetParam)
                        continue;
                }

                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    foreach (var entry in mapping)
                    {
                        if (entry.Key == p.Value.Rows[i].ID)
                        {
                            p.Value.Rows[i].Name = entry.Value;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Failed to load {sourceFilepath} for row name import.", LogLevel.Error, Tasks.LogPriority.High, e);
        }

        return true;
    }
}