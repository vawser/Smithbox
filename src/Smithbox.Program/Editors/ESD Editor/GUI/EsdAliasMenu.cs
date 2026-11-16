using Hexa.NET.ImGui;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The Alias submenu of the file and machine lists' context menus: a name field, filled with the current alias when the
/// submenu opens, and a Confirm button
/// </summary>
public class EsdAliasMenu
{
    private string Buffer = "";

    /// <summary>
    /// Draws the submenu inside an open context menu. Returns true when the user confirms, with the name typed in
    /// <paramref name="name"/> (empty to remove the alias).
    /// </summary>
    /// <param name="current">The current alias, which the field starts with</param>
    /// <param name="hint">An extra line under the field, such as the .edd name; null for none</param>
    public bool Draw(string id, string current, string tooltip, string hint, out string name)
    {
        name = null;
        var confirmed = false;

        if (ImGui.BeginMenu($"{LOC.Get("ESD_Alias_Menu")}##{id}"))
        {
            // The submenu window appears once per opening: start the field from the current alias
            if (ImGui.IsWindowAppearing())
            {
                Buffer = current ?? "";
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.TextDisabled(LOC.Get("ESD_Alias_Name"));

            ImGui.SetNextItemWidth(250f * DPI.UIScale());
            ImGui.InputText($"##{id}_name", ref Buffer, 255);

            // Leaving the field after an edit (Enter, Tab or a click elsewhere, which also closes the menu) applies it
            var committed = GUI.InputCommitted();

            if (!string.IsNullOrEmpty(hint))
                ImGui.TextDisabled(hint);

            if (ImGui.Button($"{LOC.Get("ESD_Alias_Confirm")}##{id}_confirm") || committed)
            {
                name = Buffer;
                // The caller closes its context menu, which closes this submenu with it
                confirmed = true;
            }
            GUI.Tooltip(LOC.Get("ESD_Alias_Confirm_TT"));

            ImGui.EndMenu();
        }
        GUI.Tooltip(tooltip);

        return confirmed;
    }
}
