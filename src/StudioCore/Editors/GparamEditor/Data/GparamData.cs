using SoulsFormats;
using StudioCore.Core;
using StudioCore.EventScriptEditorNS;
using StudioCore.Formats.JSON;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary GparamFiles = new();

    public GparamBank PrimaryBank;
    public GparamBank VanillaBank;

    public GparamData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        GparamFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "gparam").ToList();

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
