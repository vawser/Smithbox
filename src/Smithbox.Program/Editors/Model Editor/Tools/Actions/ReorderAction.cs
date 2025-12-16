using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ReorderAction
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ReorderAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
}
