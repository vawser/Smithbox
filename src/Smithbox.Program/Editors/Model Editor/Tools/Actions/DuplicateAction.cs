using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class DuplicateAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public DuplicateAction(ModelEditorView view, ProjectEntry project)
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
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                ApplyDuplicate();
            }
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
        UIHelper.Tooltip($"Duplicate the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Duplicate)}");

    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
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
        if (View.Selection.SelectedModelWrapper == null)
            return;

        if (View.Selection.SelectedModelWrapper.Container == null)
            return;

        if (View.ViewportSelection.IsSelection())
        {
            var selection = View.ViewportSelection.GetFilteredSelection<ModelEntity>().ToList();

            var action = new CloneModelObjectAction(View, Project, View.Selection.SelectedModelWrapper.Container, selection);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}