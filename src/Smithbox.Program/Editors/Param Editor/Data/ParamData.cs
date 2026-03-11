using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
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
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM definitions.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM definitions.");
        }

        // Param Meta
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (!paramMetaTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM meta.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM meta.");
        }

        // Param Annotations
        Task<bool> paramAnnotationTask = SetupParamAnnotations();
        bool paramAnnotationTaskResult = await paramAnnotationTask;

        if (!paramAnnotationTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM annotations.");
        }

        // Param Enums
        Task<bool> paramEnumsTask = SetupParamEnums();
        bool paramEnumsTaskResult = await paramEnumsTask;

        if (!paramEnumsTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM enums.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM enums.");
        }

        // Graph Annotations
        Task<bool> graphLegendsTask = SetupGraphAnnotations();
        bool graphLegendsTaskResult = await graphLegendsTask;

        if (!graphLegendsTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Graph annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Graph annotations.");
        }

        // Icon Configurations
        Task<bool> iconConfigTask = SetupIconConfigurations();
        bool iconConfigTaskResult = await iconConfigTask;

        if (!iconConfigTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Icon Configuration data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Icon Configuration data.");
        }

        // Table Param List
        Task<bool> tableParamTask = SetupTableParamList();
        bool tableParamTaskResult = await tableParamTask;

        if (!tableParamTaskResult)
        {
            //Smithbox.LogError(this, $"[Param Editor] Failed to setup table param list.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Table Param list.");
        }

        // Table Group Names
        Task<bool> tableGroupNameTask = SetupTableGroupNames();
        bool tableGroupNameTaskResult = await tableGroupNameTask;

        if (!tableGroupNameTaskResult)
        {
            // Smithbox.LogError(this, $"[Param Editor] Failed to setup table group name bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Table Group Name data.");
        }

        // Game Offsets (per project)
        Task<bool> gameOffsetTask = SetupParamReloaderOffsets();
        bool gameOffsetResult = await gameOffsetTask;

        if (!gameOffsetResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Param Memory Offset data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Param Memory Offset data.");
        }

        // Param Categories (per project)
        Task<bool> paramCategoryTask = SetupParamCategories();
        bool paramCategoryResult = await paramCategoryTask;

        if (!paramCategoryResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Param Categories data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Param Categories data.");
        }

        // Commutative Param Groups (per project)
        Task<bool> commutativeParamGroupTask = SetupParamCommutativityGroups();
        bool commutativeParamGroupResult = await commutativeParamGroupTask;

        if (!commutativeParamGroupResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Commutative Param Groups data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Commutative Param Groups data.");
        }

        // Field Reference Groups
        Task<bool> groupRefTask = SetupFieldReferenceGroups();
        bool groupRefTaskResult = await groupRefTask;

        if (!groupRefTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Group Reference data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Group Reference data.");
        }

        // Field Layouts
        Task<bool> fieldLayoutsTask = SetupFieldLayouts();
        bool fieldLayoutsTaskResult = await fieldLayoutsTask;

        if (!fieldLayoutsTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Field Layout data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Field Layout data.");
        }


        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Load();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Primary Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Load();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup Vanilla Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Vanilla Bank.");
        }

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DES)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS1)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS2)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.BB:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_BB)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS3:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS3)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.ER:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_ER)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.AC6:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_AC6)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.NR:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_NR)
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
            Smithbox.LogError(this, $"[Param Editor] Failed to setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        Smithbox.Log(this, $"[Param Editor] Setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to deseralize {f} as PARAMDEF", e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize Param Type Info: {paramTypeInfoPath}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read Param Type Info: {paramTypeInfoPath}", e);
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
            Smithbox.Log(this, $"[Param Editor] Reloaded PARAM meta.");
        }
        else
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to reload PARAM meta.");
        }
    }

    public void RefreshParamDifferenceCacheTask(bool checkAuxVanillaDiff = false)
    {
        // Refresh diff cache
        TaskManager.LiveTask task = new(
            "paramEditor_refreshDifferenceCache",
            "[Param Editor]",
            "Difference cache between param banks has been refreshed.",
            "Difference cache refresh has failed.",
            TaskManager.RequeueType.Repeat,
            true,
            LogPriority.Low,
            () => RefreshAllParamDiffCaches(checkAuxVanillaDiff)
        );

        TaskManager.Run(task);
    }


    public void RefreshAllParamDiffCaches(bool checkAuxVanillaDiff)
    {
        PrimaryBank.RefreshPrimaryDiffCaches(true);
        VanillaBank.RefreshVanillaDiffCaches();

        foreach (KeyValuePair<string, ParamBank> bank in AuxBanks)
        {
            bank.Value.RefreshAuxDiffCaches(checkAuxVanillaDiff);
        }

        CacheBank.ClearCaches();
    }

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

    public ParamAnnotationEntry GetParamAnnotations(string paramName)
    {
        var curLangString = CFG.Current.ParamEditor_Annotation_Language;
        var curLang = ParamAnnotationLanguages.Languages.FirstOrDefault(e => e.Name == curLangString);

        if (curLang == null)
            return null;

        var curAnnotations = ParamAnnotations.Entries[curLang];
        var curParam = curAnnotations.Params.FirstOrDefault(e => e.Param == paramName);

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

    public async Task<bool> SetupParamMeta()
    {
        await Task.Yield();

        var rootMetaDir = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        var projectMetaDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        if (CFG.Current.Param_Editor_Enable_Param_Meta_Override)
        {
            CreateProjectParamMeta();
        }

        foreach ((var f, PARAMDEF pdef) in ParamDefsByFilename)
        {
            ParamMeta meta = new(this);

            var fName = f.Substring(f.LastIndexOf('\\') + 1);

            try
            {
                if(CFG.Current.Param_Editor_Enable_Param_Meta_Override)
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
                Smithbox.LogError(this, $"[Param Editor] Failed to deseralize {fName} as PARAMMETA", e);
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
                Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param annotation languages: {file}", LogPriority.High, e);
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param annotation entry: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupParamEnums()
    {
        await Task.Yield();

        Enums = new();

        if (CFG.Current.Param_Editor_Enable_Param_Enum_Addition)
        {
            // Build project-local first, so it takes precedence over the base versions
            var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Enums");

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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param enum entry: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Param_Enum_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Enums");

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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param enum entry: {file}", LogPriority.High, e);
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

        if (CFG.Current.Param_Editor_Enable_Graph_Annotation_Addition)
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize graph annotation groups: {file}", LogPriority.High, e);
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Graph_Annotation_Override)
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize graph annotation groups: {file}", LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupIconConfigurations()
    {
        await Task.Yield();

        IconConfigurations = new();

        if (CFG.Current.Param_Editor_Enable_Icon_Configuration_Addition)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize icon configuration group: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Icon_Configuration_Override)
        {
            var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Icon Configurations");

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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize icon configuration group: {file}", LogPriority.High, e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param reloader offset data: {file}", LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupParamCategories()
    {
        await Task.Yield();

        ParamCategories = new();

        if (CFG.Current.Param_Editor_Enable_Param_Category_Addition)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param category group: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Param_Category_Override)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param category group: {file}", LogPriority.High, e);
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

        if (CFG.Current.Param_Editor_Enable_Param_Commutativity_Group_Addition)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param commutativity group: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Param_Commutativity_Group_Override)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize param commutativity group: {file}", LogPriority.High, e);
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

        if (CFG.Current.Param_Editor_Enable_Param_Field_Reference_Group_Addition)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field reference group: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Param_Field_Reference_Group_Override)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field reference group: {file}", LogPriority.High, e);
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

        if (CFG.Current.Param_Editor_Enable_Field_Layout_Addition)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field layout: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        if (!CFG.Current.Param_Editor_Enable_Param_Field_Layout_Override)
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
                        Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field layout: {file}", LogPriority.High, e);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupTableGroupNames()
    {
        await Task.Yield();

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Community Table Names");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to load {file} for table group name import during Base Store step.", e);
            }
        }

        // Project Store
        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Community Table Names");

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
                    Smithbox.LogError(this, $"[Param Editor] Failed to load {file} for table group name import during Project Store step.", e);
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

        if (Directory.Exists(projFile))
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
            Smithbox.LogError(this, $"[Param Editor] Failed to load table param list.", e);
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
        IconConfigurations = null;
        TableParamList = null;
        TableGroupNames = null;
        ParamReloaderOffsets = null;
        ParamCategories = null;
        ParamCommutativityGroups = null;
    }
    #endregion
}
