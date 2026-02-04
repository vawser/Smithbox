using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialCommandQueue
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;
    public MaterialCommandQueue(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {

        }
    }
}
