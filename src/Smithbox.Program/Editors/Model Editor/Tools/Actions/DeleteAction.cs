using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
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
        // Delete
        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Delete_Title")}##deleteAction"))
        {
            ApplyDelete();
        }
        GUI.Tooltip($"{LOC.Get("MODEL_Tools_Action_Delete_Context_TT", InputManager.GetHint(KeybindID.Delete))}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Delete
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Delete_Title")}##deleteAction",  InputManager.GetHint(KeybindID.Delete)))
        {
            ApplyDelete();
        }
        GUI.Tooltip(LOC.Get("MODEL_Tools_Action_Delete_Menu_TT"));
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

            var action = new DeleteModelObjectAction(View, Project, selection);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            Smithbox.LogError<DeleteAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}
