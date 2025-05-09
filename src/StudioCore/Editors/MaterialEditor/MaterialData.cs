using StudioCore.Core;
using StudioCore.Editors.MaterialEditor;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public MaterialBank PrimaryBank;
    public MaterialBank VanillaBank;

    public MaterialData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }
    public async Task<bool> Setup()
    {
        await Task.Delay(1);

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
