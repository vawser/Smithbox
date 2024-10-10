using ImGuiNET;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextActionMenubar
{
    private TextEditorScreen Screen;
    private TextActionHandler ActionHandler;

    public TextActionMenubar(TextEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}
