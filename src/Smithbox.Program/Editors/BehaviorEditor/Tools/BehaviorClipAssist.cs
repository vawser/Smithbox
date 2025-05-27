using Hexa.NET.ImGui;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorClipAssist
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorClipAssist(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Clip Assist"))
        {

        }
    }
}

