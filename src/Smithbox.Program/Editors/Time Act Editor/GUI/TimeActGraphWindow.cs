using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActGraphWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;


    public TimeActGraphWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        ImGui.BeginChild($"GraphSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayEventGraph();

        ImGui.EndChild();
    }

    public void DisplayEventGraph()
    {

    }
}
