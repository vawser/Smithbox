using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class DeleteAction 
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public DeleteAction(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
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
        if (View.Selection.SelectedModelWrapper == null)
            return;

        if (View.Selection.SelectedModelWrapper.Container == null)
            return;

        if (View.ViewportSelection.IsSelection())
        {
            var selection = View.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

            var action = new DeleteModelObjectAction(View, Project, View.Selection.SelectedModelWrapper.Container, selection);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
