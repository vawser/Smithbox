using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Resources.JSON;
using System.Linq;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public MaterialBank PrimaryBank_MTD;
    public MaterialBank PrimaryBank_MATBIN;

    public MaterialBank VanillaBank_MTD;
    public MaterialBank VanillaBank_MATBIN;

    public FileDictionary MTD_Files;
    public FileDictionary MATBIN_Files;

    public MaterialData(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;

        PrimaryBank_MTD = new(this, "Primary", Project.FS, BankType.MTD);
        VanillaBank_MTD = new(this, "Primary", Project.VanillaFS, BankType.MTD);

        PrimaryBank_MATBIN = new(this, "Primary", Project.FS, BankType.MATBIN);
        VanillaBank_MATBIN = new(this, "Primary", Project.VanillaFS, BankType.MATBIN);

        MTD_Files = new();
        MTD_Files.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "mtdbnd").ToList();

        MATBIN_Files = new();
        MATBIN_Files.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "matbinbnd").ToList();
    }
}
