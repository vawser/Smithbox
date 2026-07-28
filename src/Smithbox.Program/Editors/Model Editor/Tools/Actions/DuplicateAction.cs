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
        // Duplicate
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Duplicate_Title")}##duplicateAction"))
        {
            ApplyDuplicate();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Duplicate_Context_TT", InputManager.GetHint(KeybindID.Duplicate)));

    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Duplicate
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Duplicate_Title")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
        {
            ApplyDuplicate();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Duplicate_Menu_TT"));
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
            Smithbox.LogError<DuplicateAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}