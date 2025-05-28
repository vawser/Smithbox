using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorCommandQueue
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorCommandQueue(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 1)
        {
            // Load


            // Select
        }
    }
}
