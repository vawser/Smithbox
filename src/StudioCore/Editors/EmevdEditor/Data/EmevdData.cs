using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
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
        await Task.Delay(1);

        EmevdFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "emevd").ToList();

        if(Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();
            var binder = BND4.Read(regulation);
            foreach(var entry in binder.Files)
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

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        return true;
    }
}
