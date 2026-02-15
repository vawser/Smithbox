using StudioCore.Application;
using StudioCore.Editors.AnimEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorProperties_HKX2
{
    public BehaviorView View;
    public ProjectEntry Project;

    public BehaviorProperties_HKX2(BehaviorView view, ProjectEntry project)
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

        if(View.Contents_HKX2.TargetObjectType is ChosenObjectType.hkbClipGenerator)
        {
            if(View.Contents_HKX2.Selection.SelectedClipGenerator != null)
            {

            }
        }
    }
}
