using Hexa.NET.ImGui;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorVariableAssist
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorVariableAssist(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Variable Assist"))
        {

        }
    }
}

