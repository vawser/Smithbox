using StudioCore.Application;
using StudioCore.Editors.AnimEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class TimeActView : IAnimView
{
    public AnimEditorView View;
    public ProjectEntry Project;

    public TimeActView(AnimEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
    public void Display()
    {

    }
}
