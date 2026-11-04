using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActData : IDisposable
{
    public ProjectEntry Project;

    public TimeActBank PrimaryBank;

    /// <summary>
    /// The metadata of the game, laid out as the param metadata is:
    /// TAE/{Game}/Defs/{Def}.xml holds the event layouts, with internal names,
    /// TAE/{Game}/Enums/{Key}.json the enums the defs name,
    /// TAE/{Game}/Meta/{Def}.json what the fields of each event refer to and how they are edited,
    /// and TAE/{Game}/Annotations/{Language}/{Def}.json the annotated names of the fields of each event.
    /// Meta and annotations are keyed by event ID and the internal name of the field.
    /// </summary>
    private class TimeActDef
    {
        public string Name;
        public TAE.Template.BankTemplate Template;
        public TimeActMeta Meta = new();
        public TimeActAnnotations Annotations = new();

        // Keyed by event type and the internal name of the parameter
        public Dictionary<(int EventType, string Field), TimeActField> MetaFields = new();
        public Dictionary<(int EventType, string Field), TimeActFieldAnnotation> AnnotationFields = new();

        public void BuildIndex()
        {
            MetaFields.Clear();
            AnnotationFields.Clear();

            foreach (var ev in Meta.Events)
            {
                foreach (var field in ev.Fields)
                {
                    MetaFields.TryAdd((ev.ID, field.Field), field);
                }
            }

            foreach (var ev in Annotations.Events)
            {
                foreach (var field in ev.Fields)
                {
                    AnnotationFields.TryAdd((ev.ID, field.Field), field);
                }
            }
        }
    }

    // Keyed by def name. A def whose template failed to load has a null template.
    private Dictionary<string, TimeActDef> Defs = new();

    // The def each event layout came from, so that an event finds its meta
    private Dictionary<TAE.Template.EventTemplate, TimeActDef> EventDefs = new();

    // Shared by the defs of the game. Read on first use.
    private Dictionary<string, ParamEnumEntry> Enums;

    public TimeActData(ProjectEntry project)
    {
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("TAE_Data_Setup_Primary_Bank_PASS"));
        }

        return primaryBankTaskResult;
    }

    /// <summary>
    /// Gets the meta of a parameter of an event, or null if there is none
    /// </summary>
    public TimeActField GetFieldMeta(TAE.Event ev, string parameterName)
    {
        if (ev?.Template is not { } eventTemplate || !EventDefs.TryGetValue(eventTemplate, out var def))
            return null;

        return def.MetaFields.GetValueOrDefault((ev.Type, parameterName));
    }

    /// <summary>
    /// Gets the annotation of a parameter of an event, or null if there is none
    /// </summary>
    public TimeActFieldAnnotation GetFieldAnnotation(TAE.Event ev, string parameterName)
    {
        if (ev?.Template is not { } eventTemplate || !EventDefs.TryGetValue(eventTemplate, out var def))
            return null;

        return def.AnnotationFields.GetValueOrDefault((ev.Type, parameterName));
    }

    /// <summary>
    /// The name a parameter is shown with: its annotation when there is one and annotations are on,
    /// or else the internal name in the event def
    /// </summary>
    public string GetFieldDisplayName(TAE.Event ev, string parameterName)
    {
        if (CFG.Current.TimeActEditor_Field_Name_Mode is TimeActFieldNameMode.Annotation &&
            GetFieldAnnotation(ev, parameterName) is { Name.Length: > 0 } annotation)
        {
            return annotation.Name;
        }

        return parameterName;
    }

    /// <summary>
    /// The writer loses event data in the Demon's Souls format, so those time acts can only be viewed
    /// </summary>
    public bool CanSaveTimeActs => Project.Descriptor.ProjectType is not ProjectType.DES;

    /// <summary>
    /// Gets the event templates for a source. These don't have banks, as each file covers a single set of events.
    /// </summary>
    public TAE.Template.BankTemplate GetTemplate(TimeActContainerWrapper source)
    {
        var defName = GetDefName(source);

        if (defName == null || GetGameFolder() == null)
            return null;

        if (!Defs.TryGetValue(defName, out var def))
        {
            def = LoadDef(defName);
            Defs[defName] = def;
        }

        return def.Template;
    }

    /// <summary>
    /// Object time acts in Dark Souls have their own events. Every other time act uses the character def.
    /// </summary>
    private string GetDefName(TimeActContainerWrapper source)
    {
        return Project.Descriptor.ProjectType switch
        {
            ProjectType.DS1 or ProjectType.DS1R when source.Name.StartsWith('o') => "Object",
            ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3 or
                ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR or
                ProjectType.DB or ProjectType.DB_NT => "Character",
            _ => null
        };
    }

    /// <summary>
    /// The folder under Assets/TAE with the metadata of the project's game. The remaster shares the time acts
    /// of Dark Souls, and both Duskbloods types share one set.
    /// </summary>
    private string GetGameFolder()
    {
        var game = Project.Descriptor.ProjectType switch
        {
            ProjectType.DS1R => ProjectType.DS1,
            ProjectType.DB_NT => ProjectType.DB,
            var type => type
        };

        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "TAE", ProjectUtils.GetGameDirectory(game));

#if DEBUG
        if (CFG.Current.Developer_Enable_Tools)
        {
            folder = Path.Join(CFG.Current.Developer_Smithbox_Build_Folder, "src", "Smithbox.Data", "Assets", "TAE",
                ProjectUtils.GetGameDirectory(game));
        }
#endif

        return Directory.Exists(folder) ? folder : null;
    }

    private TimeActDef LoadDef(string defName)
    {
        var folder = GetGameFolder();
        var def = new TimeActDef { Name = defName };

        var defPath = Path.Join(folder, "Defs", $"{defName}.xml");

        try
        {
            def.Template = TAE.Template.ReadXMLFile(defPath, key => ResolveEnum(defName, key))[0];

            foreach (var eventTemplate in def.Template.Values)
            {
                EventDefs[eventTemplate] = def;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Template", defPath), e);
        }

        var metaPath = Path.Join(folder, "Meta", $"{defName}.json");

        if (File.Exists(metaPath))
        {
            try
            {
                def.Meta = JsonSerializer.Deserialize(File.ReadAllText(metaPath),
                    TimeActEditorJsonSerializerContext.Default.TimeActMeta) ?? new TimeActMeta();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Meta", metaPath), e);
            }
        }

        var annotationPath = GetAnnotationPath(folder, defName);

        if (annotationPath != null)
        {
            try
            {
                def.Annotations = JsonSerializer.Deserialize(File.ReadAllText(annotationPath),
                    TimeActEditorJsonSerializerContext.Default.TimeActAnnotations) ?? new TimeActAnnotations();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Annotations", annotationPath), e);
            }
        }

        def.BuildIndex();

        return def;
    }

    /// <summary>
    /// The annotations in the language picked for the param annotations, or else in English
    /// </summary>
    private static string GetAnnotationPath(string folder, string defName)
    {
        foreach (var language in new[] { CFG.Current.ParamEditor_Annotation_Language, "English" })
        {
            if (string.IsNullOrEmpty(language))
                continue;

            var path = Path.Join(folder, "Annotations", language, $"{defName}.json");

            if (File.Exists(path))
                return path;
        }

        return null;
    }

    /// <summary>
    /// The options of an enum a def names, in their order in the file. They are named in the annotation language
    /// when they have a name in it, and otherwise in English.
    /// </summary>
    private IEnumerable<KeyValuePair<string, string>> ResolveEnum(string defName, string key)
    {
        Enums ??= LoadEnums();

        if (!Enums.TryGetValue(key, out var entry))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Missing_Enum", defName, key));
            return null;
        }

        string NameOf(ParamEnumOption option) =>
            option.Names.FirstOrDefault(n => n.Language == CFG.Current.ParamEditor_Annotation_Language)?.Text
            ?? option.Names.FirstOrDefault(n => n.Language == "English")?.Text
            ?? option.Names.FirstOrDefault()?.Text
            ?? option.Key;

        return entry.Options.Select(o => new KeyValuePair<string, string>(NameOf(o), o.Key)).ToList();
    }

    private Dictionary<string, ParamEnumEntry> LoadEnums()
    {
        var enums = new Dictionary<string, ParamEnumEntry>();
        var folder = Path.Join(GetGameFolder(), "Enums");

        if (!Directory.Exists(folder))
            return enums;

        foreach (var path in Directory.EnumerateFiles(folder, "*.json"))
        {
            try
            {
                var entry = JsonSerializer.Deserialize(File.ReadAllText(path),
                    ParamEditorJsonSerializerContext.Default.ParamEnumEntry);

                if (entry?.Key != null)
                {
                    enums.TryAdd(entry.Key, entry);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Enum", path), e);
            }
        }

        return enums;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;
    }
    #endregion
}
