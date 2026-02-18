using StudioCore.Application;
using StudioCore.Editors.AnimEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorProperties
{
    public BehaviorView View;
    public ProjectEntry Project;

    public BehaviorProperties(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (View.Selection.SelectedFile == null)
            return;

        if (BehaviorUtils.SupportsHKX1(Project))
        {
        }

        if (BehaviorUtils.SupportsHKX2(Project))
        {
            if (View.Selection.SelectedFile.Havok.HKX2_Object == null)
                return;

            if (View.Contents_HKX2.TargetObjectType is ChosenObjectType.hkbClipGenerator)
            {
                if (View.Contents_HKX2.Selection.SelectedClipGenerators.Count > 0)
                {
                    var firstClip = View.Contents_HKX2.Selection.SelectedClipGenerators.First();
                }
            }
        }

        if (BehaviorUtils.SupportsHKX3(Project))
        {

        }
    }

    public void PropertyOrchestrator(object targetObj)
    {

    }
}
