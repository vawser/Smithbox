using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorContents_HKX1
{
    public BehaviorView View;
    public ProjectEntry Project;

    public string ImguiID = "BehaviorContents";

    public BehaviorContents_HKX1(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (View.Selection.SelectedFile == null)
            return;

        if (View.Selection.SelectedFile.Havok.HKX1_Object == null)
            return;

    }
    public void Invalidate()
    {

    }
}
