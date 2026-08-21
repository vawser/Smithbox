using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokSelection
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public bool DoFocus = false;

    public HavokSelection(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

}
