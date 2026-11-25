using Octokit;
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Holds the data banks for params.
/// Data Flow: Full Load
/// </summary>
public class ParamData : IDisposable
{
    public ProjectEntry Project;

    public ParamBank PrimaryBank;
    public ParamBank VanillaBank;
    public Dictionary<string, ParamBank> AuxBanks = new();

    public Dictionary<string, PARAMDEF> ParamDefs = new();
    public Dictionary<string, PARAMDEF> ParamDefsByFilename = new();
    public Dictionary<PARAMDEF, ParamMeta> ParamMeta = new();

    public ParamAnnotationLanguages ParamAnnotationLanguages = new();
    public ParamAnnotations ParamAnnotations = new();

    public ParamImportLanguages RowImportLanguages = new();

    // Base meta data
    public ParamTypeInfo ParamTypeInfo;
    public TableParams TableParamList;
    public ParamReloaderOffsets ParamReloaderOffsets;

    // User-overridable meta data
    public ParamEnums Enums;
    public ParamCategories ParamCategories;
    public ParamCommutativityGroups ParamCommutativityGroups;
    public FieldReferenceGroups FieldReferenceGroups;
    public FieldLayouts FieldLayouts;
    public IconConfigurations IconConfigurations;
    public GraphAnnotations GraphAnnotations;
    public RowFmgAnnotations RowFmgAnnotations;

    // Special-case
    public TableGroupNameStore TableGroupNames;

    public ParamData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Param Defs
        Task<bool> paramDefTask = SetupParamDefs();
        bool paramDefTaskResult = await paramDefTask;

        if (!paramDefTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamDefs_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamDefs_PASS"));
        }

        // Param Meta
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (!paramMetaTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamMeta_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamMeta_PASS"));
        }

        // Param Annotations
        Task<bool> paramAnnotationTask = SetupParamAnnotations();
        bool paramAnnotationTaskResult = await paramAnnotationTask;

        if (!paramAnnotationTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamAnnotations_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamAnnotations_PASS"));
        }

        // Param Enums
        Task<bool> paramEnumsTask = SetupParamEnums();
        bool paramEnumsTaskResult = await paramEnumsTask;

        if (!paramEnumsTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamEnums_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamEnums_PASS"));
        }

        // Secondary Language Options
        Task<bool> secondaryLanguageOptionsTask = SetupSecondaryLanguageOptions();
        bool secondaryLanguageOptionsTaskResult = await secondaryLanguageOptionsTask;

        if (!secondaryLanguageOptionsTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_RowImportLanguage_PASS"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_RowImportLanguage_FAIL"));
        }

        // Graph Annotations
        Task<bool> graphLegendsTask = SetupGraphAnnotations();
        bool graphLegendsTaskResult = await graphLegendsTask;

        if (!graphLegendsTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_GraphAnnotations_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_GraphAnnotations_PASS"));
        }

        // Row FMG Annotations
        Task<bool> rowFmgAnnotationTask = SetupRowFmgAnnotations();
        bool rowFmgAnnotationTaskResult = await rowFmgAnnotationTask;

        if (!rowFmgAnnotationTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_RowFmgAnnotations_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_RowFmgAnnotations_PASS"));
        }

        // Icon Configurations
        Task<bool> iconConfigTask = SetupIconConfigurations();
        bool iconConfigTaskResult = await iconConfigTask;

        if (!iconConfigTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_IconConfiguration_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_IconConfiguration_PASS"));
        }

        // Table Param List
        Task<bool> tableParamTask = SetupTableParamList();
        bool tableParamTaskResult = await tableParamTask;

        if (tableParamTaskResult)
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_TableParamList_PASS"));
        }

        // Table Group Names
        Task<bool> tableGroupNameTask = SetupTableGroupNames();
        bool tableGroupNameTaskResult = await tableGroupNameTask;

        if (tableGroupNameTaskResult)
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_TableGroupNames_PASS"));
        }

        // Game Offsets (per project)
        Task<bool> gameOffsetTask = SetupParamReloaderOffsets();
        bool gameOffsetResult = await gameOffsetTask;

        if (!gameOffsetResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamReloaderOffsets_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamReloaderOffsets_PASS"));
        }

        // Param Categories (per project)
        Task<bool> paramCategoryTask = SetupParamCategories();
        bool paramCategoryResult = await paramCategoryTask;

        if (!paramCategoryResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_ParamCategories_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_ParamCategories_PASS"));
        }

        // Commutative Param Groups (per project)
        Task<bool> commutativeParamGroupTask = SetupParamCommutativityGroups();
        bool commutativeParamGroupResult = await commutativeParamGroupTask;

        if (!commutativeParamGroupResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_CommutativeParamGroups_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_CommutativeParamGroups_PASS"));
        }

        // Field Reference Groups
        Task<bool> groupRefTask = SetupFieldReferenceGroups();
        bool groupRefTaskResult = await groupRefTask;

        if (!groupRefTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_FieldReferenceGroups_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_FieldReferenceGroups_PASS"));
        }

        // Field Layouts
        Task<bool> fieldLayoutsTask = SetupFieldLayouts();
        bool fieldLayoutsTaskResult = await fieldLayoutsTask;

        if (!fieldLayoutsTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_FieldLayouts_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_FieldLayouts_PASS"));
        }


        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Load();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_PrimaryBank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_PrimaryBank_PASS"));
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Load();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_VanillaBank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_VanillaBank_PASS"));

            // Param Upgrader: keep a copy of the game's regulation while no diff leads on from its version
            try
            {
                var snapshot = Upgrader.RegulationHistory.For(Project)?.TrySnapshot(VanillaBank.ParamVersion);
                if (snapshot != null)
                    Smithbox.Log(this, LOC.Get("PARAM_Upgrader_Log_Snapshot_Taken", ParamUtils.ParseRegulationVersion(VanillaBank.ParamVersion)));
            }
            catch (Exception e)
            {
                Smithbox.LogWarning(this, LOC.Get("PARAM_Upgrader_Log_Snapshot_Failed"), e);
            }
        }

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_DES)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_DS1)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_DS2)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.BB:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_BB)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS3:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_DS3)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.ER:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_ER)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.AC6:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_AC6)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.NR:
                if (CFG.Current.ParamEditor.ParamEditor_Stripped_Row_Name_Load_NR)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            default: break;
        }

        return true;
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Smithbox.Orchestrator.LoadAuxiliaryProject(targetProject, ProjectInitType.ParamEditorOnly, reloadProject);

        // Pass in the target project's filesystem,
        // so we fill it with the param data from that project
        var newAuxBank = new ParamBank(Project.Descriptor.ProjectName, Project, targetProject.VFS.FS);

        Task<bool> auxBankTask = newAuxBank.Load();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Setup_AuxBank_FAIL", targetProject.Descriptor.ProjectName));
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        // The new bank has no diffs yet (against primary or vanilla). Callers used to rely on the next param click,
        // which re-diffed everything.
        RequestFullDiffRefresh(true);

        Smithbox.Log(this, LOC.Get("PARAM_Data_Setup_AuxBank_PASS", targetProject.Descriptor.ProjectName));

        return true;
    }

    public async Task<bool> SetupParamDefs()
    {
        await Task.Yield();

        ParamDefs = new Dictionary<string, PARAMDEF>();
        ParamDefsByFilename = new Dictionary<string, PARAMDEF>();
        ParamTypeInfo = new();
        ParamTypeInfo.Mapping = new();
        ParamTypeInfo.Exceptions = new();

        var dir = ParamLocator.GetParamdefDir(Project);
        var files = Directory.GetFiles(dir, "*.xml");
        List<(string, PARAMDEF)> defPairs = new();

        foreach (var f in files)
        {
            try
            {
                var pdef = PARAMDEF.XmlDeserialize(f, true);
                ParamDefs.Add(pdef.ParamType, pdef);
                ParamDefsByFilename.Add(f, pdef);
                defPairs.Add((f, pdef));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_ParamDef", f), e);
            }
        }

        // Param Type Info
        var paramTypeInfoPath = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Param Type Info.json");

        if (File.Exists(paramTypeInfoPath))
        {
            var paramTypeInfo = new ParamTypeInfo();
            paramTypeInfo.Mapping = new();
            paramTypeInfo.Exceptions = new();

            ParamTypeInfo = paramTypeInfo;

            try
            {
                var filestring = await File.ReadAllTextAsync(paramTypeInfoPath);

                try
                {
                    paramTypeInfo = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamTypeInfo);
                    ParamTypeInfo = paramTypeInfo;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_ParamTypeInfo", paramTypeInfoPath), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Read_ParamTypeInfo", paramTypeInfoPath), e);
            }
        }

        return true;
    }

    // For the project meta switch
    public async void ReloadMeta()
    {
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (paramMetaTaskResult)
        {
            Smithbox.Log(this, LOC.Get("PARAM_Data_Reload_Param_Meta_PASS"));
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Reload_Param_Meta_FAIL"));
        }
    }

    #region Diff Refresh
    // The diff caches drive the modified-row colours and the "modified" searches.
    //
    // Edits mark their params in ParamDirtyTracker. Once a frame, UpdateDiffRefresh (UI thread) takes those marks,
    // snapshots the rows of just those params, and re-diffs them on a worker. Everything is re-diffed only on the
    // first refresh, when a bank has been replaced, or when asked for (an aux bank load, a merge, a delta import).
    // This used to re-diff every param of every bank, twice, after each undo, redo, paste or param click, and the
    // worker enumerated live row lists the UI thread was changing.

    private readonly object _diffRequestLock = new();
    private bool _diffFullRequested = true;
    private bool _diffAuxVanillaRequested = false;
    private ParamBank _diffBaselinePrimary;
    private ParamBank _diffBaselineVanilla;
    private int _diffTaskRunning = 0;

    /// <summary>
    /// One param of one bank to re-diff, with row snapshots taken on the UI thread
    /// </summary>
    private sealed class DiffItem
    {
        public ParamBank Bank;
        public string Key;
        public bool IntoVanillaCache;
        public Param.Row[] Rows;
        public Param.Row[] OtherRows;
        public HashSet<Param.Row> Result;
    }

    private sealed class DiffJob
    {
        public bool Full;
        public List<DiffItem> Items = new();
    }

    /// <summary>
    /// Kept for the existing callers. Edits are picked up by <see cref="UpdateDiffRefresh"/> from their dirty marks,
    /// so a plain call needs nothing more. <paramref name="checkAuxVanillaDiff"/> asks for a full refresh, including
    /// the aux banks' own diffs against vanilla (used after an aux bank load or a merge).
    /// </summary>
    public void RefreshParamDifferenceCacheTask(bool checkAuxVanillaDiff = false)
    {
        if (checkAuxVanillaDiff)
        {
            RequestFullDiffRefresh(true);
        }
    }

    /// <summary>
    /// Re-diff every param on the next frame. Safe to call from any thread.
    /// </summary>
    public void RequestFullDiffRefresh(bool includeAuxVanillaDiff)
    {
        lock (_diffRequestLock)
        {
            _diffFullRequested = true;
            _diffAuxVanillaRequested |= includeAuxVanillaDiff;
        }
    }

    /// <summary>
    /// Called once a frame from the Param Editor (UI thread). Starts a refresh for the params changed since the
    /// last one, or a full refresh if one is due. Does nothing while a refresh is still running: the marks wait
    /// for the next frame after it.
    /// </summary>
    public void UpdateDiffRefresh()
    {
        if (PrimaryBank?.Params == null || VanillaBank?.Params == null)
            return;

        if (Volatile.Read(ref _diffTaskRunning) == 1)
            return;

        bool full;
        bool auxVanilla;

        lock (_diffRequestLock)
        {
            full = _diffFullRequested
                || _diffBaselinePrimary != PrimaryBank
                || _diffBaselineVanilla != VanillaBank;

            auxVanilla = _diffAuxVanillaRequested;
        }

        // Taken either way: a full refresh covers them too
        var stale = ParamDirtyTracker.TakeDiffStaleParams();

        List<string> keys = null;

        if (!full)
        {
            if (stale.Count == 0)
                return;

            keys = stale
                .Select(p => PrimaryBank.GetKeyForParam(p))
                .Where(k => k != null)
                .Distinct()
                .ToList();

            if (keys.Count == 0)
                return;
        }

        lock (_diffRequestLock)
        {
            if (full)
            {
                _diffFullRequested = false;
                _diffAuxVanillaRequested = false;
                _diffBaselinePrimary = PrimaryBank;
                _diffBaselineVanilla = VanillaBank;
            }
        }

        var job = BuildDiffJob(full, auxVanilla, keys);

        Volatile.Write(ref _diffTaskRunning, 1);

        Task.Run(() =>
        {
            try
            {
                RunDiffJob(job);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"{LOC.Get("SYS_Header")} {LOC.Get("PARAM_Data_Task_Refresh_Diff_Cache_FAIL")}", e);

                // Try again from scratch next frame
                RequestFullDiffRefresh(auxVanilla);
            }
            finally
            {
                Volatile.Write(ref _diffTaskRunning, 0);
            }
        });
    }

    /// <summary>
    /// Re-diffs every param now, on the calling thread (used by the param upgrader, which needs the result
    /// straight away)
    /// </summary>
    public void RefreshAllParamDiffCaches(bool checkAuxVanillaDiff)
    {
        if (PrimaryBank?.Params == null || VanillaBank?.Params == null)
            return;

        ParamDirtyTracker.TakeDiffStaleParams();

        lock (_diffRequestLock)
        {
            _diffFullRequested = false;
            _diffBaselinePrimary = PrimaryBank;
            _diffBaselineVanilla = VanillaBank;
        }

        RunDiffJob(BuildDiffJob(true, checkAuxVanillaDiff, null));
    }

    /// <summary>
    /// Lists the param diffs to compute, with row snapshots. Call on the UI thread (or while nothing edits the
    /// params). <paramref name="keys"/> is the changed params; null with <paramref name="full"/> means every param.
    /// <list type="bullet">
    /// <item>primary bank, vanilla cache: primary rows that differ from vanilla</item>
    /// <item>vanilla bank, primary cache: vanilla rows that differ from primary</item>
    /// <item>each aux bank, primary cache: aux rows that differ from primary</item>
    /// <item>each aux bank, vanilla cache (only when asked for): aux rows that differ from vanilla. Edits to the
    /// primary bank don't change these, so a normal refresh leaves them alone.</item>
    /// </list>
    /// </summary>
    private DiffJob BuildDiffJob(bool full, bool auxVanilla, List<string> keys)
    {
        var job = new DiffJob { Full = full };
        var snapshots = new Dictionary<(ParamBank, string), Param.Row[]>();

        Param.Row[] Snapshot(ParamBank bank, string key)
        {
            if (bank?.Params == null || !bank.Params.TryGetValue(key, out var param) || param == null)
                return null;

            if (!snapshots.TryGetValue((bank, key), out var rows))
            {
                rows = param.Rows.ToArray();
                snapshots[(bank, key)] = rows;
            }

            return rows;
        }

        void Add(ParamBank bank, string key, bool intoVanillaCache, ParamBank otherBank)
        {
            var rows = Snapshot(bank, key);

            if (rows == null)
                return;

            job.Items.Add(new DiffItem
            {
                Bank = bank,
                Key = key,
                IntoVanillaCache = intoVanillaCache,
                Rows = rows,
                OtherRows = Snapshot(otherBank, key)
            });
        }

        var primaryKeys = full ? PrimaryBank.Params.Keys.ToList() : keys;

        foreach (var key in primaryKeys)
        {
            Add(PrimaryBank, key, true, VanillaBank);
        }

        foreach (var key in full ? VanillaBank.Params.Keys.ToList() : primaryKeys)
        {
            Add(VanillaBank, key, false, PrimaryBank);
        }

        foreach (var aux in AuxBanks.Values.ToList())
        {
            if (aux?.Params == null)
                continue;

            foreach (var key in full ? aux.Params.Keys.ToList() : primaryKeys)
            {
                Add(aux, key, false, PrimaryBank);

                if (auxVanilla)
                {
                    Add(aux, key, true, VanillaBank);
                }
            }
        }

        return job;
    }

    /// <summary>
    /// Computes the job's diffs (in parallel; each only reads its snapshots) and publishes them, one cache swap per
    /// bank and cache
    /// </summary>
    private void RunDiffJob(DiffJob job)
    {
        Parallel.ForEach(job.Items, item =>
        {
            item.Result = ParamBank.ComputeParamDiff(item.Rows, item.OtherRows);
        });

        foreach (var bankItems in job.Items.GroupBy(i => i.Bank))
        {
            Dictionary<string, HashSet<Param.Row>> vanillaUpdates = null;
            Dictionary<string, HashSet<Param.Row>> primaryUpdates = null;

            foreach (var item in bankItems)
            {
                if (item.IntoVanillaCache)
                {
                    vanillaUpdates ??= new();
                    vanillaUpdates[item.Key] = item.Result;
                }
                else
                {
                    primaryUpdates ??= new();
                    primaryUpdates[item.Key] = item.Result;
                }
            }

            bankItems.Key.PublishDiffCaches(vanillaUpdates, primaryUpdates, job.Full);
        }

        // Only the Param Editor's cached lists depend on the diffs, so leave the other editors' caches alone
        var paramEditor = Project.Handler.ParamEditor;

        if (paramEditor != null)
        {
            CacheBank.RemoveCache(paramEditor);
        }
        else
        {
            CacheBank.ClearCaches();
        }
    }
    #endregion

    public ParamMeta GetParamMeta(PARAMDEF def)
    {
        if (ParamMeta.ContainsKey(def))
        {
            return ParamMeta[def];
        }
        else
        {
            return null;
        }
    }

    public ParamAnnotationEntry GetParamAnnotations(string paramType)
    {
        var curLangString = CFG.Current.ParamEditor.ParamEditor_Annotation_Language;
        var curLang = ParamAnnotationLanguages.Languages.FirstOrDefault(e => e.Name == curLangString);

        if (curLang == null)
            return null;

        var curAnnotations = ParamAnnotations.Entries[curLang];
        var curParam = curAnnotations.Params.FirstOrDefault(e => e.Type == paramType);

        if(curParam == null)
            return null;

        return curParam;
    }

    public ParamAnnotationFieldEntry GetFieldAnnotation(ParamAnnotationEntry annotations, string fieldName)
    {
        if (annotations == null)
            return null;

        var match = annotations.Fields.FirstOrDefault(e => e.Field == fieldName);

        if (match == null) 
            return null;

        return match;
    }

    public ParamFieldMeta GetParamFieldMeta(ParamMeta curMeta, PARAMDEF.Field def)
    {
        if (curMeta != null && curMeta.Fields != null && curMeta.Fields.ContainsKey(def))
        {
            return curMeta.Fields[def];
        }
        else
        {
            return null;
        }
    }

    public ParamFieldMeta GetParamFieldMeta(ParamMeta curMeta, string internalFieldName)
    {
        if (curMeta != null && curMeta.Fields != null && curMeta.Fields.Any(e => e.Key.InternalName == internalFieldName))
        {
            return curMeta.Fields.FirstOrDefault(e => e.Key.InternalName == internalFieldName).Value;
        }
        else
        {
            return null;
        }
    }

    public async Task<bool> SetupParamMeta()
    {
        await Task.Yield();

        var rootMetaDir = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Param Meta");

#if DEBUG
        if (CFG.Current.Developer.Developer_Enable_Tools)
        {
            rootMetaDir = Path.Join(CFG.Current.Developer.Developer_Smithbox_Build_Folder, $@"\src\Smithbox.Data\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Param Meta");
        }
#endif

        var projectMetaDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Param Meta");

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Param_Meta_Override)
        {
            CreateProjectParamMeta();
        }

        foreach ((var f, PARAMDEF pdef) in ParamDefsByFilename)
        {
            ParamMeta meta = new(this);

            var fName = f.Substring(f.LastIndexOf('\\') + 1);

            try
            {
                if(CFG.Current.ParamEditor.Param_Editor_Enable_Param_Meta_Override)
                {
                    meta.XmlDeserialize(Path.Join(projectMetaDir, fName), pdef);
                }
                else
                {
                    meta.XmlDeserialize(Path.Join(rootMetaDir, fName), pdef);
                }

                ParamMeta.Add(pdef, meta);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_PARAM_META", fName), e);
            }
        }

        return true;
    }

    public async Task<bool> SetupParamAnnotations()
    {
        await Task.Yield(); 

        ParamAnnotationLanguages = new();

        // Build the language list first
        var sourcefile = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", "Annotation Languages.json");

        if (Path.Exists(sourcefile))
        {
            var file = File.ReadAllText(sourcefile);
            try
            {
                ParamAnnotationLanguages = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamAnnotationLanguages);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Annotation_Languages", file), LogPriority.High, e);
            }
        }
        else
        {
            // Default to English if the file is missing
            var english = new ParamAnnotationLanguageEntry();
            english.Name = "English";
            english.Folder = "English";

            ParamAnnotationLanguages.Languages.Add(english);
        }

        // Then build the annotations
        ParamAnnotations = new();

        foreach (var lang in ParamAnnotationLanguages.Languages)
        {
            var paramList = new ParamAnnotationList();
            ParamAnnotations.Entries.Add(lang, paramList);

            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Annotations", lang.Folder);

#if DEBUG
            if (CFG.Current.Developer.Developer_Enable_Tools)
            {
                sourceFolder = Path.Join(CFG.Current.Developer.Developer_Smithbox_Build_Folder, $@"\src\Smithbox.Data\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Param Annotations", lang.Folder);
            }
#endif

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamAnnotationEntry);

                        if (!ParamAnnotations.Entries[lang].Params.Any(e => e.Param == layout.Param))
                        {
                            ParamAnnotations.Entries[lang].Params.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Annotation_Entry", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupSecondaryLanguageOptions()
    {
        await Task.Yield();

        // Row Names
        RowImportLanguages = new();

        // Build the language list first
        var sourcefile = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Row Import Languages.json");

        if (Path.Exists(sourcefile))
        {
            var file = File.ReadAllText(sourcefile);
            try
            {
                RowImportLanguages = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamImportLanguages);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Row_Name_Import_Languages", file), LogPriority.High, e);
            }
        }
        else
        {
            // Default to English if the file is missing
            var english = new ParamImportLanguageEntry();
            english.Key = "PARAM_Language_English";
            english.Name = "English";
            english.Folder = "English";

            RowImportLanguages.Options.Add(english);
        }

        return true;
    }

    public async Task<bool> SetupParamEnums()
    {
        await Task.Yield();

        Enums = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Param_Enum_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Enums");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamEnumEntry);

                        if (!Enums.List.Any(e => e.Key == layout.Key))
                        {
                            Enums.List.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Enum_Entry", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Param_Enum_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Enums");

#if DEBUG
            if (CFG.Current.Developer.Developer_Enable_Tools)
            {
                sourceFolder = Path.Join(CFG.Current.Developer.Developer_Smithbox_Build_Folder, $@"\src\Smithbox.Data\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Param Enums");
            }
#endif
            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamEnumEntry);

                        if (!Enums.List.Any(e => e.Key == layout.Key))
                        {
                            Enums.List.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Enum_Entry", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }


    public async Task<bool> SetupGraphAnnotations()
    {
        await Task.Yield();

        GraphAnnotations = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Graph_Annotation_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Graph Annotations");

            if (Path.Exists(projectFolder))
            {
                var fileName = Path.Combine(projectFolder, $"Annotations.json");

                var file = File.ReadAllText(fileName);

                try
                {
                    GraphAnnotations = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.GraphAnnotations);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Graph_Annotation", file), LogPriority.High, e);
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Graph_Annotation_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Graph Annotations");

            if (Path.Exists(sourceFolder))
            {
                var fileName = Path.Combine(sourceFolder, $"Annotations.json");

                var file = File.ReadAllText(fileName);

                try
                {
                    GraphAnnotations = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.GraphAnnotations);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Graph_Annotation", file), LogPriority.High, e);
                }
            }
        }

        return true;
    }
    public async Task<bool> SetupRowFmgAnnotations()
    {
        await Task.Yield();

        RowFmgAnnotations = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Row_FMG_Annotation_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Row FMG Annotations");

            if (Path.Exists(projectFolder))
            {
                var fileName = Path.Combine(projectFolder, $"Annotations.json");

                var file = File.ReadAllText(fileName);

                try
                {
                    RowFmgAnnotations = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.RowFmgAnnotations);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Row_FMG_Annotation", file), LogPriority.High, e);
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Row_FMG_Annotation_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Row FMG Annotations");

#if DEBUG
            if (CFG.Current.Developer.Developer_Enable_Tools)
            {
                sourceFolder = Path.Join(CFG.Current.Developer.Developer_Smithbox_Build_Folder, $@"\src\Smithbox.Data\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Row FMG Annotations");
            }
#endif

            if (Path.Exists(sourceFolder))
            {
                var fileName = Path.Combine(sourceFolder, $"Annotations.json");

                var file = File.ReadAllText(fileName);

                try
                {
                    RowFmgAnnotations = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.RowFmgAnnotations);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Row_FMG_Annotation", file), LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupIconConfigurations()
    {
        await Task.Yield();

        IconConfigurations = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Icon_Configuration_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Icon Configurations");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.IconConfigurationEntry);

                        if (!IconConfigurations.Groups.Any(e => e.Name == layout.Name))
                        {
                            IconConfigurations.Groups.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Icon_Configuration", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Icon_Configuration_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Icon Configurations");
#if DEBUG
            if (CFG.Current.Developer.Developer_Enable_Tools)
            {
                sourceFolder = Path.Join(CFG.Current.Developer.Developer_Smithbox_Build_Folder, $@"\src\Smithbox.Data\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Icon Configurations");
            }
#endif

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.IconConfigurationEntry);

                        if (!IconConfigurations.Groups.Any(e => e.Name == layout.Name))
                        {
                            IconConfigurations.Groups.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Icon_Configuration", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupParamReloaderOffsets()
    {
        await Task.Yield();

        ParamReloaderOffsets = new();

        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Reloader");

        if (Path.Exists(sourceFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(sourceFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamReloaderOffsetEntry);

                    if (!ParamReloaderOffsets.Groups.Any(e => e.exeVersion == layout.exeVersion))
                    {
                        ParamReloaderOffsets.Groups.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Reloader_Offsets", file), LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupParamCategories()
    {
        await Task.Yield();

        ParamCategories = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Param_Category_Addition)
        {
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Categories");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamCategoryEntry);

                        if (!ParamCategories.Categories.Any(e => e.Key == layout.Key))
                        {
                            ParamCategories.Categories.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Categories", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Param_Category_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Categories");

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamCategoryEntry);

                        if (!ParamCategories.Categories.Any(e => e.Key == layout.Key))
                        {
                            ParamCategories.Categories.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Categories", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupParamCommutativityGroups()
    {
        await Task.Yield();

        ParamCommutativityGroups = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Param_Commutativity_Group_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Commutativity Groups");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamCommutativityEntry);

                        if (!ParamCommutativityGroups.Groups.Any(e => e.Name == layout.Name))
                        {
                            ParamCommutativityGroups.Groups.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Commutative_Groups", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Param_Commutativity_Group_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Commutativity Groups");

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.ParamCommutativityEntry);

                        if (!ParamCommutativityGroups.Groups.Any(e => e.Name == layout.Name))
                        {
                            ParamCommutativityGroups.Groups.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Commutative_Groups", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupFieldReferenceGroups()
    {
        await Task.Yield();

        FieldReferenceGroups = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Param_Field_Reference_Group_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Reference Groups");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldReferenceGroup);

                        if (!FieldReferenceGroups.Entries.Any(e => e.Name == layout.Name))
                        {
                            FieldReferenceGroups.Entries.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Field_Reference_Group", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Param_Field_Reference_Group_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Reference Groups");

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldReferenceGroup);

                        if (!FieldReferenceGroups.Entries.Any(e => e.Name == layout.Name))
                        {
                            FieldReferenceGroups.Entries.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Field_Reference_Group", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupFieldLayouts()
    {
        await Task.Yield();

        FieldLayouts = new();

        if (CFG.Current.ParamEditor.Param_Editor_Enable_Field_Layout_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Layouts");

            if (Path.Exists(projectFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(projectFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldLayout);

                        if (!FieldLayouts.Entries.Any(e => e.Name == layout.Name))
                        {
                            FieldLayouts.Entries.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Field_Layouts", file), LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.ParamEditor.Param_Editor_Enable_Param_Field_Layout_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Layouts");

            if (Path.Exists(sourceFolder))
            {
                foreach (var entry in Directory.EnumerateFiles(sourceFolder))
                {
                    var file = File.ReadAllText(entry);
                    try
                    {
                        var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldLayout);

                        if (!FieldLayouts.Entries.Any(e => e.Name == layout.Name))
                        {
                            FieldLayouts.Entries.Add(layout);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Deseralize_Param_Field_Layouts", file), LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupTableGroupNames()
    {
        await Task.Yield();

        var lang = CFG.Current.ParamEditor.ParamEditor_Import_Language;
        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Param Table Names", lang);

        if (!Directory.Exists(srcDir))
        {
            // Create blank version so user can add names
            TableGroupNames = new();
            TableGroupNames.Groups = new();

            return false;
        }

        // Base Store
        var baseStore = new TableGroupNameStore();
        baseStore.Groups = new();

        foreach (var file in Directory.EnumerateFiles(srcDir))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(file);

                var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableGroupParamEntry);

                if (item != null)
                {
                    baseStore.Groups.Add(item);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Table_Group_Load", file), e);
            }
        }

        // Project Store
        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Param Table Names", lang);

        var projStore = new TableGroupNameStore();
        projStore.Groups = new();

        if (Directory.Exists(projDir))
        {
            foreach (var file in Directory.EnumerateFiles(projDir))
            {
                try
                {
                    var filestring = await File.ReadAllTextAsync(file);

                    var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableGroupParamEntry);

                    if (item != null)
                    {
                        projStore.Groups.Add(item);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Table_Group_Load_Project", file), e);
                }
            }
        }

        // Final Store
        TableGroupNames = baseStore;

        // Add unique groups from project store
        foreach (var entry in projStore.Groups)
        {
            if (!TableGroupNames.Groups.Any(e => e.Param == entry.Param))
            {
                TableGroupNames.Groups.Add(entry);
            }
        }

        // Merge in unique table group names from the project store,
        // and replace base group names if the project store contains entries that match
        foreach (var group in TableGroupNames.Groups)
        {
            var projGroup = projStore.Groups.FirstOrDefault(e => e.Param == group.Param);

            if (projGroup != null)
            {
                // Update the names if any project entries should replace the base entries
                foreach (var entry in group.Entries)
                {
                    if (projGroup.Entries.Any(e => e.ID == entry.ID))
                    {
                        entry.Name = projGroup.Entries.FirstOrDefault(e => e.ID == entry.ID).Name;
                    }
                }

                foreach (var entry in projGroup.Entries)
                {
                    if (!group.Entries.Any(e => e.ID == entry.ID))
                    {
                        var newEntry = new TableGroupEntry();
                        newEntry.ID = entry.ID;
                        newEntry.Name = entry.Name;

                        group.Entries.Add(newEntry);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupTableParamList()
    {
        await Task.Yield();

        var srcFile = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Table Params.json");
        var projFile = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Table Params.json");

        // A file, so File.Exists (Directory.Exists was always false, so the project override never loaded)
        if (File.Exists(projFile))
        {
            srcFile = projFile;
        }

        TableParamList = new();
        TableParamList.Params = new();

        if (!File.Exists(srcFile))
        {
            return false;
        }

        try
        {
            var filestring = await File.ReadAllTextAsync(srcFile);

            var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableParams);

            if (item != null)
            {
                TableParamList = item;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Data_Log_Failed_Table_Param_List"), e);
        }

        return true;
    }


    #region Project Metadata Creation
    public void CreateProjectParamMeta()
    {
        var metaDir = ParamLocator.GetParammetaDir(Project);
        var rootDir = Path.Combine(AppContext.BaseDirectory, metaDir);
        var projectDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", metaDir);

        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
            var files = Directory.GetFileSystemEntries(rootDir);

            foreach (var f in files)
            {
                var name = Path.GetFileName(f);
                var tPath = Path.Combine(rootDir, name);
                var pPath = Path.Combine(projectDir, name);
                if (File.Exists(tPath) && !File.Exists(pPath))
                {
                    File.Copy(tPath, pPath);
                }
            }
        }
    }

    public void CopyMetadataFile(string name)
    {
        var srcFolder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var srcFile = Path.Combine(srcFolder, name);

        var targetFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project");
        var targetFile = Path.Combine(targetFolder, name);

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        if (File.Exists(srcFile))
        {
            File.Copy(srcFile, targetFile, true);
        }
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        foreach (var entry in AuxBanks)
        {
            entry.Value?.Dispose();
        }

        PrimaryBank = null;
        VanillaBank = null;
        AuxBanks = null;

        ParamDefs = null;
        ParamDefsByFilename = null;
        ParamMeta = null;
        ParamTypeInfo = null;
        GraphAnnotations = null;
        RowFmgAnnotations = null;
        IconConfigurations = null;
        TableParamList = null;
        TableGroupNames = null;
        ParamReloaderOffsets = null;
        ParamCategories = null;
        ParamCommutativityGroups = null;
        GC.SuppressFinalize(this);
    }
    #endregion
}
