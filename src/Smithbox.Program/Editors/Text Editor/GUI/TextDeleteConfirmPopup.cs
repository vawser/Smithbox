using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Confirmation shown before deleting two or more selected entries.
/// Opened from Display() (inside the view window) rather than from the shortcut
/// handler, because ImGui popup IDs depend on the window ID stack.
/// </summary>
public class TextDeleteConfirmPopup
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    private bool OpenRequested = false;
    private int SelectedCount = 0;
    private int AssociatedCount = 0;

    private const string PopupId = "##textDeleteConfirmModal";

    public TextDeleteConfirmPopup(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Request(int selectedCount, int associatedCount)
    {
        SelectedCount = selectedCount;
        AssociatedCount = associatedCount;
        OpenRequested = true;
    }

    public void Display()
    {
        var title = $"{LOC.Get("TEXT_DeleteConfirm_Title")}{PopupId}";

        if (OpenRequested)
        {
            OpenRequested = false;
            ImGui.OpenPopup(title);
        }

        // Set width, auto height: with AlwaysAutoResize alone, wrapped text never wraps
        // (see Documentation/Claude/Tool UI Styling.md, Modals)
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var message = AssociatedCount > 0
                ? LOC.Get("TEXT_DeleteConfirm_Message_Associated", SelectedCount, AssociatedCount)
                : LOC.Get("TEXT_DeleteConfirm_Message", SelectedCount);

            GUI.WrappedText(message);
            GUI.WrappedText(LOC.Get("TEXT_DeleteConfirm_Undo_Hint", InputManager.GetHint(KeybindID.Undo)));

            ImGui.Spacing();

            if (ImGui.Button($"{LOC.Get("TEXT_DeleteConfirm_Action_Delete")}##confirmDelete", DPI.StandardButtonSize))
            {
                Parent.ActionHandler.ExecuteDelete();
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            // Cancel is the default: Escape closes without deleting
            if (ImGui.Button($"{LOC.Get("TEXT_DeleteConfirm_Action_Cancel")}##cancelDelete", DPI.StandardButtonSize) ||
                ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
}
