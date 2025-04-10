using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;

namespace StudioCore.Editors.TextEditor;

public class TextToolMenubar
{
    private TextEditorScreen Screen;
    private TextActionHandler ActionHandler;

    public TextToolMenubar(TextEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}

