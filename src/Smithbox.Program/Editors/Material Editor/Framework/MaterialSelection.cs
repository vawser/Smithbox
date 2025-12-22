using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialSelection
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialSelection(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor; 
        Project = project;
    }

    public MaterialSourceType SourceType { get; set; }

    public FileDictionaryEntry SelectedBinderEntry { get; set; }

    public MTDWrapper MTDWrapper { get; set; }

    public MATBINWrapper MATBINWrapper { get; set; }

    public string SelectedFileKey { get; set; }

    public MTD SelectedMTD { get; set; }

    public MATBIN SelectedMATBIN { get; set; }

    // MTD
    public int SelectedTextureIndex { get; set; } = -1;

}
