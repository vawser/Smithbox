using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelCommandQueue
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelCommandQueue(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {
            // TODO:

            // load

            // select
        }
    }
}

