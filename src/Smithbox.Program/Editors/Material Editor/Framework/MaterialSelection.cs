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

    // Focus
    public bool SelectFileListEntry = false;
}
