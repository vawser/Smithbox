using Andre.Formats;
using Andre.IO.VFS;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class ParamBank : IDisposable
{
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

    public ParamBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Name = name;
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

        CacheBank.ClearCaches();

        var successfulLoad = false;

        switch (Project.Descriptor.ProjectType)
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
            case ProjectType.NR: successfulLoad = LoadParameters_NR(); break;
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

        switch (Project.Descriptor.ProjectType)
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
            case ProjectType.NR:
                successfulSave = SaveParameters_NR(); break;
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
            Smithbox.LogError(this, $"[Param Editor] Failed to get regulation version. Params might be corrupt.");
            return;
        }

        // Load every param in the regulation
        foreach (BinderFile f in parambnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(f.Name.Replace('\\', Path.DirectorySeparatorChar));

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
            if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.SDT or ProjectType.DS3)
            {
                _usedTentativeParamTypes = new Dictionary<string, string>();
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                // Missing paramtype
                if (!string.IsNullOrEmpty(curParam.ParamType))
                {
                    if (!Project.Handler.ParamData.ParamDefs.ContainsKey(curParam.ParamType) || Project.Handler.ParamData.ParamTypeInfo.Exceptions.Contains(paramName))
                    {
                        if (Project.Handler.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                        {
                            _usedTentativeParamTypes.Add(paramName, curParam.ParamType);
                            curParam.ParamType = newParamType;
                        }
                        else
                        {
                            Smithbox.LogError(this, $"[Param Editor:{Name}] Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.");

                            continue;
                        }
                    }
                }
                else
                {
                    if (Project.Handler.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                    {
                        _usedTentativeParamTypes.Add(paramName, curParam.ParamType);

                        curParam.ParamType = newParamType;
                    }
                    else
                    {
                        Smithbox.LogError(this, $"[Param Editor:{Name}] Couldn't read ParamType for {paramName} and no tentative ParamType exists.");

                        continue;
                    }
                }
            }
            // Normal
            else
            {
                curParam = Param.ReadIgnoreCompression(f.Bytes);

                //Smithbox.Log(this, $"{paramName}: {curParam.ParamdefDataVersion} - {curParam.ParamdefFormatVersion}");

                if (!Project.Handler.ParamData.ParamDefs.ContainsKey(curParam.ParamType ?? ""))
                {
                    Smithbox.LogError(this, $"[Param Editor:{Name}] Couldn't find ParamDef for param {paramName} with ParamType \"{curParam.ParamType}\".");

                    //ParamDefHelper.GenerateBaseParamDefFile(paramName, $"{curParam.ParamType}");

                    continue;
                }
            }

            ApplyParamFixups(curParam);

            if (curParam.ParamType == null)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to get regulation version. Params might be corrupt.");
            }

            // Skip these for DS1 so the param load is not slowed down by the catching
            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if (paramName is "m99_ToneCorrectBank" or "m99_ToneMapBank" or "default_ToneCorrectBank")
                {
                    Smithbox.Log(this, $"[:Param Editor:{Name}] Skipped this param: {paramName}");
                    continue;
                }
            }

            PARAMDEF def = Project.Handler.ParamData.ParamDefs[curParam.ParamType];

            try
            {
                curParam.ApplyParamdef(def, version, paramName);
                paramBank.Add(paramName, curParam);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                Smithbox.LogError(this, $"[Param Editor] Could not apply ParamDef for {name} in {Name}", e);
            }
        }
    }

    private void ApplyParamFixups(Param p)
    {
        // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
        // it's an easy fixup
        if (Project.Descriptor.ProjectType is ProjectType.ER && ParamVersion >= 10601000)
        {
            if (p.ParamType == "CHR_MODEL_PARAM_ST")
            {
                if (p.ExpandParamSize(12, 16))
                    Smithbox.Log(this, $"[Param Editor] CHR_MODEL_PARAM_ST fixed up.");
            }
        }

        // Add in the new data for these two params added in 1.12.1
        if (Project.Descriptor.ProjectType is ProjectType.ER && ParamVersion >= 11210015)
        {
            if (p.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
            {
                if (p.ExpandParamSize(880, 1024))
                    Smithbox.Log(this, $"[Param Editor] GAME_SYSTEM_COMMON_PARAM_ST fixed up.");
            }
            if (p.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
            {
                if (p.ExpandParamSize(112, 144))
                    Smithbox.Log(this, $"[Param Editor] POSTURE_CONTROL_PARAM_WEP_RIGHT_ST fixed up.");
            }
            if (p.ParamType == "SIGN_PUDDLE_PARAM_ST")
            {
                if (p.ExpandParamSize(32, 48))
                    Smithbox.Log(this, $"[Param Editor] SIGN_PUDDLE_PARAM_ST fixed up.");
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
            Smithbox.LogError(this, $"[Param Editor] Failed to find {paramPath} for {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param for {Name}: {paramPath}", e);

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
                        Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);

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

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);

        var paramPath = GetGameParam_DES(fs);

        if (!fs.FileExists(paramPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files for {Name}. Save failed.");
            return false;
        }

        var data = fs.GetFile(paramPath).GetData().ToArray();

        using var paramBnd = BND3.Read(fs.GetFile(paramPath).GetData());

        if (CFG.Current.ParamEditor_Row_Name_Strip_DES)
        {
            RowNameHelper.RowNameStrip(Project);
        }

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
        var naParamPath = Path.Join("param", "gameparam", "gameparamna.parambnd.dcx");

        if (fs.FileExists(naParamPath))
        {
            ProjectUtils.WriteWithBackup(Project, fs, toFs, naParamPath, paramBnd);
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "gameparam", "gameparam.parambnd.dcx"), paramBnd);

        // Decompressed
        paramBnd.Compression = DCX.Type.None;
        naParamPath = Path.Join("param", "gameparam", "gameparamna.parambnd");
        if (fs.FileExists(naParamPath))
        {
            ProjectUtils.WriteWithBackup(Project, fs, toFs, naParamPath, paramBnd);
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "gameparam", "gameparam.parambnd"), paramBnd);

        // Drawparam
        List<string> drawParambndPaths = new();
        if (fs.DirectoryExists(Path.Join("param", "drawparam")))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions(Path.Join("param", "drawparam"), ".parambnd", ".parambnd.dcx"))
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

                ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "drawparam", Path.GetFileName(bnd)), drawParamBnd);
            }
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_DES)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }

    /// <summary>
    /// Checks for DeS paramBNDs and returns the name of the parambnd with the highest priority.
    /// </summary>
    private string GetGameParam_DES(VirtualFileSystem fs)
    {
        var name = Path.Join("param", "gameparam", "gameparamna.parambnd.dcx");

        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = Path.Join("param", "gameparam", "gameparamna.parambnd");
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = Path.Join("param", "gameparam", "gameparam.parambnd.dcx");
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = Path.Join("param", "gameparam", "gameparam.parambnd");
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
            Smithbox.LogError(this, $"[Param Editor] Failed to find {paramPath} for {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param for {Name}: {paramPath}", e);

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
                        Smithbox.LogError(this, $"[Param Editor:] Failed to load draw param for {Name}: {paramPath}", e);

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

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = Path.Join("param", "GameParam", "GameParam.parambnd");
        if (!fs.FileExists(param))
        {
            param += ".dcx";
            if (!fs.FileExists(param))
            {
                Smithbox.LogError(this, $"[Param Editor] Cannot locate param files for {Name}. Save failed.");

                return false;
            }
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS1)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "GameParam", "GameParam.parambnd"), paramBnd);

        if (fs.DirectoryExists(Path.Join("param", "DrawParam")))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions(Path.Join("param", "DrawParam"), ".parambnd"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "DrawParam", Path.GetFileName(bnd)), drawParamBnd);
            }
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS1)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }

    public string GetGameParam_DS1(VirtualFileSystem fs)
    {
        var name = Path.Join("param", "GameParam", "GameParam.parambnd");
        if (fs.FileExists($@"{name}"))
        {
            return name;
        }

        name = Path.Join("param", "GameParam", "GameParam.parambnd.dcx");
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

        var paramPath = Path.Join("param", "GameParam", "GameParam.parambnd.dcx");

        if (!TargetFS.FileExists(paramPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {paramPath} for {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param for {Name}: {paramPath}", e);

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
                        Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);
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

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project); ;
        string param = Path.Join("param", "GameParam", "GameParam.parambnd.dcx");

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files for {Name}. Save failed.");

            return false;
        }

        using var paramBnd = BND3.Read(fs.GetFile(param).GetData());

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS1)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }
        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "GameParam", "GameParam.parambnd.dcx"), paramBnd);

        //DrawParam
        if (fs.DirectoryExists(Path.Join("param", "DrawParam")))
        {
            foreach (var bnd in fs.GetFileNamesWithExtensions(Path.Join("param", "DrawParam"), ".parambnd.dcx"))
            {
                using var drawParamBnd = BND3.Read(fs.GetFile(bnd).GetData());
                foreach (BinderFile p in drawParamBnd.Files)
                {
                    if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
                    {
                        p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
                    }
                }

                ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "DrawParam", Path.GetFileName(bnd)), drawParamBnd);
            }
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS1)
        {
            RowNameHelper.RowNameRestore(Project);
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
            Smithbox.LogError(this, $"[Param Editor] Failed to load game param for {Name}: {paramPath}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);

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
                PARAMDEF def = Project.Handler.ParamData.ParamDefs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Could not apply ParamDef for {EnemyParam.ParamType} in {Name}", e);

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
                if (CFG.Current.ParamEditor_Loose_Param_Mode_DS2)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = Project.Handler.ParamData.ParamDefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = Project.Handler.ParamData.ParamDefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Could not apply ParamDef for {fname} in {Name}", e);
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
            Smithbox.LogError(this, $"[Param Editor] Failed to load game param for {Name}: {paramPath}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load draw param for {Name}: {paramPath}", e);

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
                PARAMDEF def = Project.Handler.ParamData.ParamDefs[EnemyParam.ParamType];
                EnemyParam.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Could not apply ParamDef for {EnemyParam.ParamType} in {Name}", e);

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
                if (CFG.Current.ParamEditor_Loose_Param_Mode_DS2)
                {
                    // Loose params: override params already loaded via regulation
                    PARAMDEF def = Project.Handler.ParamData.ParamDefs[lp.ParamType];
                    lp.ApplyParamdef(def);
                    _params[name] = lp;
                }
                else
                {
                    // Non-loose params: do not override params already loaded via regulation
                    if (!Params.ContainsKey(name))
                    {
                        PARAMDEF def = Project.Handler.ParamData.ParamDefs[lp.ParamType];
                        lp.ApplyParamdef(def);
                        _params.Add(name, lp);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Could not apply ParamDef for {fname} in {Name}", e);

                successfulLoad = false;
            }
        }

        paramBnd.Dispose();

        return successfulLoad;
    }

    private bool SaveParameters_DS2S()
    {
        var successfulSave = true;

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"enc_regulation.bnd.dcx";

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files for {Name}. Save failed.");

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
                if (CFG.Current.Project_Enable_Backup_Saves)
                {
                    toFs.WriteFile($"{param}.bak", data);
                }
            }
            toFs.WriteFile(param, paramBnd.Write());
        }
        else
        {
            paramBnd = BND4.Read(data);
        }

        if (!CFG.Current.ParamEditor_Loose_Param_Mode_DS2 && CFG.Current.ParamEditor_Row_Name_Strip_DS2)
        {
            // Save params non-loosely: Replace params regulation and write remaining params loosely.
            if (paramBnd.Files.Find(e => e.Name.EndsWith(".param")) == null)
            {
                if (CFG.Current.ParamEditor_Repack_Loose_Params_DS2)
                {
                    paramBnd.Dispose();
                    param = $@"enc_regulation.bnd.dcx";
                    data = Project.VFS.VanillaFS.GetFile(param).GetData().ToArray();

                    if (!BND4.Is(data))
                    {
                        // Decrypt the file.
                        paramBnd = SFUtil.DecryptDS2Regulation(data);

                        // Since the file is encrypted, check for a backup. If it has none, then make one and write a decrypted one.
                        if (!toFs.FileExists($@"{param}.bak"))
                        {
                            if (CFG.Current.Project_Enable_Backup_Saves)
                            {
                                toFs.WriteFile($"{param}.bak", data);
                            }

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
                RowNameHelper.RowNameStrip(Project);

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
                        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("Param", $"{p.Key}.param"), p.Value);
                    }
                }
            }
            catch
            {
                RowNameHelper.RowNameRestore(Project);
                throw;
            }

            RowNameHelper.RowNameRestore(Project);
        }
        else if (CFG.Current.ParamEditor_Row_Name_Strip_DS2)
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
                RowNameHelper.RowNameStrip(Project);

                // Write params to loose files.
                foreach (KeyValuePair<string, Param> p in Params)
                {
                    ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("Param", $"{p.Key}.param"), p.Value);
                }
            }
            catch
            {
                RowNameHelper.RowNameRestore(Project);
                throw;
            }

            RowNameHelper.RowNameRestore(Project);
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
            Smithbox.LogError(this, $"[Param Editor] Failed to find {packedFile} in {Name}");

            successfulLoad = false;
        }
        else
        {
            if (CFG.Current.ParamEditor_Loose_Param_Mode_DS3 && TargetFS.FileExists(looseFile))
            {
                try
                {
                    var data = TargetFS.GetFile(looseFile).GetData();
                    using var bnd = BND4.Read(data);
                    LoadParamFromBinder(bnd, ref _params, out _paramVersion);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"Failed to load game param in {Name}: {looseFile}", e);

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
                    Smithbox.LogError(this, $"Failed to load game param in {Name}: {packedFile}", e);

                    successfulLoad = false;
                }
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_DS3()
    {
        var successfulSave = true;

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"Data0.bdt";

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        BND4 paramBnd = SFUtil.DecryptDS3Regulation(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS3)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        // If not loose write out the new regulation
        if (!CFG.Current.ParamEditor_Loose_Param_Mode_DS3)
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

            ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "gameparam", "gameparam_dlc2.parambnd.dcx"), paramBND);
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_DS3)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }
    #endregion

    #region Bloodborne
    private bool LoadParameters_BB()
    {
        var successfulLoad = true;

        var paramPath = Path.Join("param", "gameparam", "gameparam.parambnd.dcx");

        if (!TargetFS.FileExists(paramPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {paramPath} in {Name}");
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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {paramPath}", e);

                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_BB()
    {
        var successfulSave = true;

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = Path.Join("param", "gameparam", "gameparam.parambnd.dcx");

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_BB)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        // Replace params with edited ones
        foreach (BinderFile p in paramBnd.Files)
        {
            if (Params.ContainsKey(Path.GetFileNameWithoutExtension(p.Name)))
            {
                p.Bytes = Params[Path.GetFileNameWithoutExtension(p.Name)].Write();
            }
        }

        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "gameparam", "gameparam.parambnd.dcx"), paramBnd);

        if (CFG.Current.ParamEditor_Row_Name_Strip_BB)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }
    #endregion

    #region Sekiro: Shadows Die Twice
    private bool LoadParameters_SDT()
    {
        var successfulLoad = true;

        var paramPath = Path.Join("param", "gameparam", "gameparam.parambnd.dcx");

        if (!TargetFS.FileExists(paramPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {paramPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {paramPath}", e);

                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_SDT()
    {
        var successfulSave = true;

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = Path.Join("param", "gameparam", "gameparam.parambnd.dcx");

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var data = fs.GetFile(param).GetData().ToArray();

        var paramBnd = BND4.Read(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_SDT)
        {
            RowNameHelper.RowNameStrip(Project);
        }

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

        ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "gameparam", "gameparam.parambnd.dcx"), paramBnd);

        if (CFG.Current.ParamEditor_Row_Name_Strip_SDT)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }
    #endregion

    #region Elden Ring
    private bool LoadParameters_ER()
    {
        var successfulLoad = true;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");

        if (!TargetFS.FileExists(gameParamPath))
        {
            Smithbox.LogError(this, $"Param Editor] Failed to find {gameParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {gameParamPath}", e);

                successfulLoad = false;
            }
        }

        if (!TargetFS.FileExists(systemParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {systemParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {systemParamPath}", e);

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

        var sourceFs = Project.VFS.FS;
        var gameFs = Project.VFS.VanillaRealFS;
        var writeFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = @"regulation.bin";

        if (!sourceFs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || _pendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptERRegulation(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_ER)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        OverwriteParamsER(regParams);

        ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.ER);

        var sysParam = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");

        if (!sourceFs.FileExists(sysParam))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate system param files in {Name}. Save failed.");

            return false;
        }

        if (ParamUtils.IsSystemParamTouched(Project, this))
        {
            if (sourceFs.TryGetFile(sysParam, out var sysParamF))
            {
                using var sysParams = BND4.Read(sysParamF.GetData());
                OverwriteParamsER(sysParams);
                ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, Path.Join("param", "systemparam", "systemparam.parambnd.dcx"), sysParams);
            }
        }
        if (CFG.Current.ParamEditor_Row_Name_Strip_ER)
        {

            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }
    #endregion

    #region Armored Core VI: Fires of Rubicon
    private bool LoadParameters_AC6()
    {
        var successfulLoad = true;

        var dataPath = Project.Descriptor.DataPath;
        var projectPath = Project.Descriptor.ProjectPath;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");
        var graphicsParamPath = Path.Join("param", "graphicsconfig", "graphicsconfig.parambnd.dcx");
        var eventParamPath = Path.Join("param", "eventparam", "eventparam.parambnd.dcx");

        // Game Param
        if (!TargetFS.FileExists(gameParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {gameParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {gameParamPath}", e);

                successfulLoad = false;
            }
        }

        // System Param
        if (!TargetFS.FileExists(systemParamPath))
        {
            Smithbox.LogError(this, $"Param Editor] Failed to find {systemParamPath} in {Name}");
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
                Smithbox.LogError(this, $"[Param Editor] Failed to load system param in {Name}: {systemParamPath}", e);
                successfulLoad = false;
            }
        }

        // Graphics Param
        if (!TargetFS.FileExists(graphicsParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {graphicsParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load graphics param in {Name}: {graphicsParamPath}", e);
                successfulLoad = false;
            }
        }

        // Event Param
        if (!TargetFS.FileExists(eventParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {eventParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load event param in {Name}: {eventParamPath}", e);

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

        var sourceFs = Project.VFS.FS;
        var gameFs = Project.VFS.VanillaRealFS;
        var writeFs = ProjectUtils.GetFilesystemForWrite(Project);

        string param = @"regulation.bin";
        if (!sourceFs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var data = sourceFs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || _pendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptAC6Regulation(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_AC6)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        OverwriteParamsAC6(regParams);
        ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, @"regulation.bin", regParams, ProjectType.AC6);

        var sysParam = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");

        if (!sourceFs.FileExists(sysParam))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate system param files in {Name}. Save failed.");

            return false;
        }

        if (ParamUtils.IsSystemParamTouched(Project, this))
        {
            if (sourceFs.TryGetFile(sysParam, out var sysParamF))
            {
                using var sysParams = BND4.Read(sysParamF.GetData());
                OverwriteParamsAC6(sysParams);
                ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, sysParam, sysParams);
            }
        }

        var graphicsParam = Path.Join("param", "graphicsconfig", "graphicsconfig.parambnd.dcx");

        if (!sourceFs.FileExists(sysParam))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate graphics param files in {Name}. Save failed.");

            return false;
        }

        if (ParamUtils.IsGraphicsParamTouched(Project, this))
        {
            if (sourceFs.TryGetFile(graphicsParam, out var graphicsParamF))
            {
                using var graphicsParams = BND4.Read(graphicsParamF.GetData());
                OverwriteParamsAC6(graphicsParams);
                ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, graphicsParam, graphicsParams);
            }
        }

        var eventParam = Path.Join("param", "eventparam", "eventparam.parambnd.dcx");

        if (!sourceFs.FileExists(eventParam))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate event param files in {Name}. Save failed.");

            return false;
        }

        if (ParamUtils.IsEventParamTouched(Project, this))
        {
            if (sourceFs.TryGetFile(eventParam, out var eventParamF))
            {
                using var eventParams = BND4.Read(eventParamF.GetData());
                OverwriteParamsAC6(eventParams);
                ProjectUtils.WriteWithBackup(Project, sourceFs, writeFs, eventParam, eventParams);
            }
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_AC6)
        {
            RowNameHelper.RowNameRestore(Project);
        }

        return successfulSave;
    }
    #endregion

    #region Elden Ring: Nightreign
    private bool LoadParameters_NR()
    {
        var successfulLoad = true;

        var gameParamPath = $@"regulation.bin";
        var systemParamPath = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");
        var eventParamPath = Path.Join("param", "eventparam", "eventparam.parambnd.dcx");

        if (!TargetFS.FileExists(gameParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {gameParamPath} in {Name}");

            successfulLoad = false;
        }
        else
        {
            try
            {
                var data = TargetFS.GetFile(gameParamPath).GetData().ToArray();
                using BND4 bnd = SFUtil.DecryptNightreignRegulation(data);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to load game param in {Name}: {gameParamPath}", e);

                successfulLoad = false;
            }
        }

        // System Param
        if (!TargetFS.FileExists(systemParamPath))
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to find {systemParamPath} in {Name}");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load system param in {Name}: {systemParamPath}", e);

                successfulLoad = false;
            }
        }

        // Event Param
        if (!TargetFS.FileExists(eventParamPath))
        {
            //Smithbox.LogError(this, $"[{Project.ProjectName}:Param Editor:{Name}] Failed to find {eventParamPath}", LogPriority.High);
            //successfulLoad = false;
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
                Smithbox.LogError(this, $"[Param Editor] Failed to load event param in {Name}: {eventParamPath}", e);

                successfulLoad = false;
            }
        }

        return successfulLoad;
    }

    private bool SaveParameters_NR()
    {
        var successfulSave = true;

        void OverwriteParamsNR(BND4 paramBnd)
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

        var fs = Project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(Project);
        string param = @"regulation.bin";

        if (!fs.FileExists(param))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate param files in {Name}. Save failed.");

            return false;
        }

        var sourceFs = Project.VFS.FS;
        var gameFs = Project.VFS.VanillaRealFS;
        var data = fs.GetFile(param).GetData().ToArray();

        // Use the game root version in this case
        if (!sourceFs.FileExists(param) || _pendingUpgrade)
        {
            data = gameFs.GetFile(param).GetData().ToArray();
        }

        BND4 regParams = SFUtil.DecryptNightreignRegulation(data);

        if (CFG.Current.ParamEditor_Row_Name_Strip_NR)
        {
            RowNameHelper.RowNameStrip(Project);
        }

        OverwriteParamsNR(regParams);

        ProjectUtils.WriteWithBackup(Project, fs, toFs, @"regulation.bin", regParams, ProjectType.NR);

        // System Param
        var sysParam = Path.Join("param", "systemparam", "systemparam.parambnd.dcx");

        if (!fs.FileExists(sysParam))
        {
            Smithbox.LogError(this, $"[Param Editor] Cannot locate system files in {Name}. Save failed.");

            return false;
        }

        if (ParamUtils.IsSystemParamTouched(Project, this))
        {
            if (fs.TryGetFile(sysParam, out var sysParamF))
            {
                using var sysParams = BND4.Read(sysParamF.GetData());
                OverwriteParamsNR(sysParams);
                ProjectUtils.WriteWithBackup(Project, fs, toFs, Path.Join("param", "systemparam", "systemparam.parambnd.dcx"), sysParams);
            }
        }

        // Event Param
        var eventParam = Path.Join("param", "eventparam", "eventparam.parambnd.dcx");

        if (!Project.VFS.FS.FileExists(eventParam))
        {
            //Smithbox.LogError(this, $"[{Project.ProjectName}:Param Editor:{Name}] Cannot locate event param files. Save failed.", LogPriority.High);
            //return false;
        }

        if (ParamUtils.IsEventParamTouched(Project, this))
        {
            if (fs.TryGetFile(eventParam, out var eventParamF))
            {
                using var eventParams = BND4.Read(eventParamF.GetData());
                OverwriteParamsNR(eventParams);
                ProjectUtils.WriteWithBackup(Project, fs, toFs, eventParam, eventParams);
            }
        }

        if (CFG.Current.ParamEditor_Row_Name_Strip_NR)
        {
            RowNameHelper.RowNameRestore(Project);
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
            _vanillaDiffCache = GetParamDiff(Project.Handler.ParamData.VanillaBank);
        }

        CacheBank.ClearCaches();
    }

    public void RefreshVanillaDiffCaches()
    {
        if (Project.Handler.ParamData.PrimaryBank._vanillaDiffCache != null)
        {
            _primaryDiffCache = Project.Handler.ParamData.PrimaryBank._vanillaDiffCache;
        }

        _primaryDiffCache = GetParamDiff(Project.Handler.ParamData.PrimaryBank);

        CacheBank.ClearCaches();
    }

    public void RefreshAuxDiffCaches(bool checkVanillaDiff)
    {
        if (checkVanillaDiff)
        {
            _vanillaDiffCache = GetParamDiff(Project.Handler.ParamData.VanillaBank);
        }

        _primaryDiffCache = GetParamDiff(Project.Handler.ParamData.PrimaryBank);

        CacheBank.ClearCaches();
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

        if (editor.Project.Handler.ParamData.VanillaBank.Params.ContainsKey(param) && VanillaDiffCache != null && VanillaDiffCache.ContainsKey(param))
        {
            Param.Row[] otherBankRows = editor.Project.Handler.ParamData.VanillaBank.Params[param].Rows.Where(cell => cell.ID == row.ID).ToArray();
            RefreshParamRowDiffCache(row, otherBankRows, VanillaDiffCache[param]);
        }

        foreach (ParamBank aux in editor.Project.Handler.ParamData.AuxBanks.Values)
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

    #region Dispose
    public void Dispose()
    {
        _params.Clear();
        _params = null;

        EnemyParam = null;

        ClipboardRows.Clear();
        ClipboardRows = null;
    }
    #endregion
}