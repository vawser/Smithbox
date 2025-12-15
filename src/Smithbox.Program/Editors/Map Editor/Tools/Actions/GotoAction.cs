using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;

namespace StudioCore.Editors.MapEditor;

public class GotoAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public GotoAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && Editor.ViewportSelection.IsSelection())
        {
            GotoMapObjectEntry();
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Not shown here
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Go to in List", KeyBindings.Current.MAP_GoToInList.HintText))
        {
            GotoMapObjectEntry();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void GotoMapObjectEntry()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            Editor.ViewportSelection.GotoTreeTarget = Editor.ViewportSelection.GetSingleSelection();
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}