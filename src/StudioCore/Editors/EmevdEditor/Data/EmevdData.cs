using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Holds the data banks for Event Scripts.
/// Data Flow: Lazy Load
/// </summary>
public class EmevdData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary EmevdFiles = new();

    public EmevdBank PrimaryBank;
    public EmevdBank VanillaBank;

    public EmevdData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        EmevdFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "emevd")
            .ToList();

        if(Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            try
            {
                var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();

                try
                {
                    var binder = BND4.Read(regulation);
                    foreach (var entry in binder.Files)
                    {
                        if (!entry.Name.EndsWith("emevd"))
                            continue;

                        var fileDictEntry = new FileDictionaryEntry();
                        fileDictEntry.Archive = "Binder";
                        fileDictEntry.Path = entry.Name;
                        fileDictEntry.Folder = "";
                        fileDictEntry.Filename = Path.GetFileNameWithoutExtension(entry.Name);
                        fileDictEntry.Extension = Path.GetExtension(entry.Name);

                        EmevdFiles.Entries.Add(fileDictEntry);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                    return false;
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation.bnd.dcx from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if(!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }
}
