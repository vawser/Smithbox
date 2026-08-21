using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokCommandQueue
{
    private HavokEditorScreen Editor;
    private ProjectEntry Project;

    public bool DoFocus = false;
    public HavokCommandQueue(HavokEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (initcmd != null && initcmd.Length > 1)
        {

        }
    }
}
