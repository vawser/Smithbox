using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public class MaterialSelection
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialSelection(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor; 
        Project = project;
    }

    public SourceType SourceType { get; set; }

    public FileDictionaryEntry SelectedBinderEntry { get; set; }

    public MTDWrapper MTDWrapper { get; set; }

    public MATBINWrapper MATBINWrapper { get; set; }

    public string SelectedFileKey { get; set; }

    public MTD SelectedMTD { get; set; }

    public MATBIN SelectedMATBIN { get; set; }

    // MTD
    public int SelectedTextureIndex { get; set; } = -1;

}
