using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialTextures
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public MaterialTextures(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }
}
