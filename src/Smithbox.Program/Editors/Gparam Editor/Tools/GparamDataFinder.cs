using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamDataFinder
{
    public GparamEditorScreen Editor;
    public ProjectEntry Project;
    public GparamDataFinder(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var view = Editor.ViewHandler.ActiveView;


    }
}
