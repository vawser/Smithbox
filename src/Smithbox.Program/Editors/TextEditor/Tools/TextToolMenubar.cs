using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextToolMenubar
{
    private TextEditorScreen Editor;
    private TextActionHandler ActionHandler;

    public TextToolMenubar(TextEditorScreen screen)
    {
        Editor = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{Icons.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}

