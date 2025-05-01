using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialFieldInput
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialFieldInput(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }
}

