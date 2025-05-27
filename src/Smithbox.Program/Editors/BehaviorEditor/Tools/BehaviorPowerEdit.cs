using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorPowerEdit
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorPowerEdit(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
}
