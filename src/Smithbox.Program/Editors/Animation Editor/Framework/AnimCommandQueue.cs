using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;
public class AnimCommandQueue
{
    public AnimEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public AnimCommandQueue(AnimEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;
    }
}