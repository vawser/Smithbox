using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the tool menubar entries for this editor
/// </summary>
public class EsdToolMenubar
{
    private EsdEditorScreen Screen;

    public EsdToolMenubar(EsdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

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

