using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class DuplicateAction
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public DuplicateAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Editor.ViewportSelection.IsSelection())
        {
            ApplyDuplicate();
        }
    }


    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Duplicate"))
        {
            ApplyDuplicate();
        }
        UIHelper.Tooltip($"Duplicate the currently selected map objects.\n\nShortcut: {KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
        {
            ApplyDuplicate();
        }
        UIHelper.Tooltip($"Duplicate the currently selected map objects.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Duplicate"))
        {
            DisplayMenu();
        }
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.Button("Duplicate Selection", DPI.WholeWidthButton(windowWidth, 24)))
        {
            ApplyDuplicate();
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyDuplicate()
    {

        if (Editor.Selection.SelectedModelWrapper == null)
            return;

        if (Editor.Selection.SelectedModelWrapper.Container == null)
            return;

        if (Editor.ViewportSelection.IsSelection())
        {
            var selection = Editor.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

            var action = new CloneModelObjectAction(Editor, Project, Editor.Selection.SelectedModelWrapper.Container, selection);

            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}