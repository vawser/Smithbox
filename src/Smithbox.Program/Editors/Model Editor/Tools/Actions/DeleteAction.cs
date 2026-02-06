using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class DeleteAction 
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public DeleteAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (activeView.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                ApplyDelete();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Delete"))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected model objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Delete)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
        {
            ApplyDelete();
        }
        UIHelper.Tooltip($"Delete the currently selected model objects.");
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
    public void ApplyDelete()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (activeView.Selection.SelectedModelWrapper == null)
            return;

        if (activeView.Selection.SelectedModelWrapper.Container == null)
            return;

        if (activeView.ViewportSelection.IsSelection())
        {
            var selection = activeView.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

            var action = new DeleteModelObjectAction(activeView, Project, activeView.Selection.SelectedModelWrapper.Container, selection);

            activeView.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
