using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActCommandQueue
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public TimeActCommandQueue(TimeActEditorScreen editor, ProjectEntry project)
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
