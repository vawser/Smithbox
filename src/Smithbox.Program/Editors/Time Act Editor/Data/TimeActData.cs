using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActData : IDisposable
{
    public ProjectEntry Project;

    public TimeActBank PrimaryBank;

    // Keyed by file name. A template that failed to load stays null.
    private Dictionary<string, TAE.Template.BankTemplate> Templates = new();

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
            ProjectType.NR or ProjectType.DB or ProjectType.DB_NT => "TAE.Template.ERNR.xml",
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
