using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorToolView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorToolView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        Editor.PowerEdit.Display();
    }
}