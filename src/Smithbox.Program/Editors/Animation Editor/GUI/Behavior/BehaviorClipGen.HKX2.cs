using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorClipGen_HKX2
{
    public BehaviorView View;
    public ProjectEntry Project;
    public BehaviorClipGen_HKX2(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (View.Selection.SelectedFile == null)
            return;

        if (View.Selection.SelectedFile.Havok.HKX2_Object == null)
            return;

        var root = View.Selection.SelectedFile.Havok.HKX2_Object;

    }
    public void Invalidate()
    {

    }
}
