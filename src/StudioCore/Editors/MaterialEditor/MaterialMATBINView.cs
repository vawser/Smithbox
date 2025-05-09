using StudioCore.Core;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

/// <summary>
/// The main window for a MATBIN entry.
/// </summary>
public class MaterialMATBINView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialMATBINView(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
    public void Draw()
    {

    }
}
