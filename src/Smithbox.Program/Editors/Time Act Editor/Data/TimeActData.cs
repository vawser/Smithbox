using SoulsFormats;
using StudioCore.Editors.ModelEditor;
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

    // Keyed by file name. A template that failed to load stays null.
    private Dictionary<string, TAE.Template.BankTemplate> Templates = new();

    public TimeActMeta Meta = new();

    // Parameters are looked up every frame while they are on display
    private Dictionary<(int EventType, string Name), TimeActField> FieldMetaCache = new();

    public TimeActData(ProjectEntry project)
    {
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Yield();

        LoadMeta();

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

    private void LoadMeta()
    {
        var path = Path.Join(AppContext.BaseDirectory, "Assets", "TAE", "TimeAct-Meta.json");

        try
        {
            Meta = JsonSerializer.Deserialize(File.ReadAllText(path), TimeActEditorJsonSerializerContext.Default.TimeActMeta)
                ?? new TimeActMeta();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Meta", path), e);
        }
    }

    /// <summary>
    /// Gets the meta of a parameter of an event type, or null if there is none. Entries limited to the event type
    /// go before those for every event, and within each, an exact match goes before a prefix.
    /// </summary>
    public TimeActField GetFieldMeta(int eventType, string parameterName)
    {
        if (!FieldMetaCache.TryGetValue((eventType, parameterName), out var field))
        {
            bool IsExact(TimeActField e) => e.Field.Equals(parameterName, StringComparison.OrdinalIgnoreCase);
            bool IsPrefix(TimeActField e) => e.IsPrefix && parameterName.StartsWith(e.Field, StringComparison.OrdinalIgnoreCase);

            bool IsForType(TimeActField e) => e.EventTypes?.Contains(eventType) == true;
            bool IsForAll(TimeActField e) => e.EventTypes == null || e.EventTypes.Count == 0;

            field = Meta.Fields.FirstOrDefault(e => IsForType(e) && IsExact(e))
                ?? Meta.Fields.FirstOrDefault(e => IsForType(e) && IsPrefix(e))
                ?? Meta.Fields.FirstOrDefault(e => IsForAll(e) && IsExact(e))
                ?? Meta.Fields.FirstOrDefault(e => IsForAll(e) && IsPrefix(e));

            FieldMetaCache[(eventType, parameterName)] = field;
        }

        return field;
    }

    /// <summary>
    /// The name a parameter is shown with: its annotation when there is one and annotations are on,
    /// or else the name in the event template
    /// </summary>
    public string GetFieldDisplayName(int eventType, string parameterName)
    {
        if (CFG.Current.TimeActEditor_Field_Name_Mode is TimeActFieldNameMode.Annotation &&
            GetFieldMeta(eventType, parameterName) is { AltName.Length: > 0 } meta)
        {
            return meta.AltName;
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
        var filename = GetTemplateFilename(source);

        if (filename == null)
            return null;

        if (!Templates.TryGetValue(filename, out var template))
        {
            try
            {
                var path = Path.Join(AppContext.BaseDirectory, "Assets", "TAE", filename);

                template = TAE.Template.ReadXMLFile(path)[0];
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Template", filename), e);
            }

            Templates[filename] = template;
        }

        return template;
    }

    private string GetTemplateFilename(TimeActContainerWrapper source)
    {
        var isObject = source.Name.StartsWith('o');

        return Project.Descriptor.ProjectType switch
        {
            ProjectType.DES => "TAE.Template.DES.xml",
            ProjectType.DS1 or ProjectType.DS1R => isObject ? "TAE.Template.DS1.OBJ.xml" : "TAE.Template.DS1.xml",
            ProjectType.BB => "TAE.Template.BB.xml",
            ProjectType.DS3 => "TAE.Template.DS3.xml",
            ProjectType.SDT => "TAE.Template.SDT.xml",
            ProjectType.ER => "TAE.Template.ER.xml",
            ProjectType.AC6 => "TAE.Template.AC6.xml",
            ProjectType.NR => "TAE.Template.ERNR.xml",
            // Duskbloods keeps the template it shared with Nightreign before that was checked against nightreign.exe,
            // as its own time acts don't fit the Nightreign layouts
            ProjectType.DB or ProjectType.DB_NT => "TAE.Template.DB.xml",
            _ => null
        };
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;
    }
    #endregion
}
