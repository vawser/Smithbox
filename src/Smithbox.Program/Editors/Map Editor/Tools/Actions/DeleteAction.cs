using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class DeleteAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public DeleteAction(MapEditorView view, ProjectEntry project)
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
        GUI.Tooltip($"Delete the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Delete)}");
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
        GUI.Tooltip($"Delete the currently selected map objects.");
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
        if (View.ViewportSelection.IsSelection())
        {
            var mapObjects = View.ViewportSelection.GetPlacedEntities();

            if (mapObjects.Count > 0)
            {
                ViewportAction action = new EntDeleteAction(View, mapObjects, true);

                // Objects left behind that refer to the deleted ones would fail the next save
                var deleted = mapObjects.Cast<Entity>().ToHashSet();
                var broken = mapObjects
                    .GroupBy(e => e.Container)
                    .SelectMany(g => MsbReferences.FindReferencesTo(g.Key, g))
                    .Where(r => !deleted.Contains(r.Slot.Referrer))
                    .ToList();

                if (broken.Count > 0)
                {
                    var result = PlatformUtils.Instance.MessageBox(
                        LOC.Get("MAP_Delete_Dialog_Broken_References", broken.Count, MsbReferences.Describe(broken)),
                        LOC.Get("MAP_Options_Dialog_Confirm"),
                        MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Cancel || result == DialogResult.None)
                    {
                        View.DelayPicking();
                        return;
                    }

                    // Yes clears the references along with the delete, in one undo step
                    if (result == DialogResult.Yes)
                    {
                        action = new ViewportCompoundAction(new List<ViewportAction>
                        {
                            new ClearReferencesAction(broken.Select(r => r.Slot)),
                            action
                        });
                    }
                }

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
        else
        {
            Smithbox.LogError<DeleteAction>("No object selected.");
        }

        View.DelayPicking();
    }
}