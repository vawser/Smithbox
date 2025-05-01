using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialActionHandler
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialActionHandler(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }
}

