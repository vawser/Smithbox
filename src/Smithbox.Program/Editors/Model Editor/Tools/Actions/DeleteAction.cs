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
        var flags = View.GetEditableSelectionCount() > 0 ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled;

        if (ImGui.Selectable($"{LOC.Get("MODEL_Tools_Action_Delete_Title")}##deleteAction", false, flags))
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
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_Delete_Title")}##deleteAction",  InputManager.GetHint(KeybindID.Delete),
            false, View.GetEditableSelectionCount() > 0))
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

        var container = View.Selection.SelectedModelWrapper.Container;

        if (container == null)
            return;

        // The model root can't be deleted
        var selection = ModelIndexRemapper.GetEditableSelection(View, container);

        if (selection.Count > 0)
        {
            // Removing a material or node that is still referred to would leave those references dangling
            var references = ModelIndexRemapper.FindBlockingReferences(container, selection);

            if (references.Count > 0)
            {
                Smithbox.LogError<DeleteAction>(LOC.Get("MODEL_Action_Delete_Blocked",
                    ModelIndexRemapper.DescribeReferences(references)));
                return;
            }

            var action = new DeleteModelObjectAction(View, Project, selection);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            Smithbox.LogError<DeleteAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}
