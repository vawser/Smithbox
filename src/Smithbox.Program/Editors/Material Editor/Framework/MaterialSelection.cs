using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialSelection
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public MaterialSelection(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view; 
        Project = project;
    }

    public MaterialSourceType SourceType { get; set; }

    public FileDictionaryEntry SelectedBinderEntry { get; set; }

    public MTDWrapper MTDWrapper { get; set; }

    public MATBINWrapper MATBINWrapper { get; set; }

    public string SelectedFileKey { get; set; }

    public MTD SelectedMTD { get; set; }

    public MATBIN SelectedMATBIN { get; set; }

    /// <summary>
    /// The wrapper for the selected MTD or MATBIN, used to track its modified state
    /// </summary>
    public MaterialFileWrapper SelectedFileWrapper { get; set; }

    // Focus
    public bool SelectFileListEntry = false;

    // Jump To
    public MaterialSourceType? RequestedSourceType = null;
    public bool ScrollToContainer = false;
    public bool ScrollToFile = false;

    /// <summary>
    /// Select a material file, switching the source tab and scrolling the lists to it
    /// </summary>
    public void JumpTo(MaterialBank bank, MaterialReference reference)
    {
        SourceType = reference.SourceType;
        RequestedSourceType = reference.SourceType;

        SelectedBinderEntry = reference.BinderEntry;
        SelectedFileKey = reference.FileKey;
        SelectedFileWrapper = reference.File;

        SelectedMTD = null;
        SelectedMATBIN = null;

        if (reference.File is MTDFileWrapper mtdFile)
        {
            MTDWrapper = bank.MTDs[reference.BinderEntry];
            SelectedMTD = mtdFile.MTD;
        }

        if (reference.File is MATBINFileWrapper matbinFile)
        {
            MATBINWrapper = bank.MATBINs[reference.BinderEntry];
            SelectedMATBIN = matbinFile.MATBIN;
        }

        ScrollToContainer = true;
        ScrollToFile = true;
    }
}
