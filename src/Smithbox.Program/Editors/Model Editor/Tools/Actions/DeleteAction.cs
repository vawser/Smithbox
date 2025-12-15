using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Editor.ViewportSelection.IsSelection())
        {
            ApplyDelete();
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
        UIHelper.Tooltip($"Delete the currently selected model objects.\n\nShortcut: {KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
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
        if (Editor.Selection.SelectedModelWrapper == null)
            return;

        if (Editor.Selection.SelectedModelWrapper.Container == null)
            return;

        if (Editor.ViewportSelection.IsSelection())
        {
            var selection = Editor.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

            var action = new DeleteModelObjectAction(Editor, Project, Editor.Selection.SelectedModelWrapper.Container, selection);

            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
