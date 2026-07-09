using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public static class ParamEditorHints
{
    public static string SearchBarHint =
        @"For help with regex or examples, consult the main help menu.
This searchbar utilise Regex, and words surrounded by ! in commands indicate that a Regex expression may be used instead of plain text.
All other words in capitals are parameters for the given command.
Regex searches are case-insensitive and the searched term may appear anywhere in the target rows. To specify an exact match, surround the text with ^ and $ (eg. ^10$) or use a range variant.
Multiple selectors can be given by separating them with &&.

Row selection is done through any of the following:
    !VALUE!: to select rows with a matching ID or a matching name
    modified: to select rows changed from vanilla
    !modified: to select unmodified rows
    id !VALUE!: to select rows with a matching ID
    idrange MIN MAX: to select rows with an ID within the given bounds
    name !NAME!: to select rows with a matching name
    prop FIELD !VALUE!: to select rows who have a matching value for the given field
    proprange FIELD MIN MAX: to select rows who have a value for the given field within the given bounds
    propref FIELD !NAME!: to select rows that have a reference to another row with a matching name.

A complete search may look like the following examples:
id 10000 (This searches for all rows with an id containing 10000. This includes 10000, 1000010, 210000)
name Dagger (This searches for all rows with a name containing Dagger. This includes Blood Dagger, Sharp daggers and daggerfall)
propref originEquipWep0 Dagger (This searches for all rows whose field originEquipWep0 refers to a row with a name containing Dagger, following the same rules above.
name Dagger && idrange 10000 Infinity (This searches for all rows with a name containing Dagger and that have an id greater than 9999)";

    public static bool AddImGuiHintButton(string id, ref string hint, bool canEdit = false, bool isRowHint = false)
    {
        var scale = DPI.UIScale();
        var ret = false;
        /*
        ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1.0f), "Help");
        if (ImGui.BeginPopupContextItem(id))
        {
            if (ParamEditor.ParamEditorScreen.EditorMode && canEdit) //remove this, editor mode should be called earlier
            {
                ImGui.InputTextMultiline("", ref hint, 8196, new Vector2(720, 480));
                if (ImGui.IsItemDeactivatedAfterEdit())
                    ret = true;
            }
            else
                ImGui.Text(hint);
            ImGui.EndPopup();
        }
        */

        // Even worse of a hack than it was before. eat my shorts (all of this should be redone)
        if (isRowHint)
        {
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1f), "?");
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.OpenPopup(id);
            }

            if (ImGui.BeginPopupContextItem(id))
            {
                ImGui.Text(hint);

                ImGui.EndPopup();
            }

            ImGui.SameLine();
        }
        else
        {
            ImGui.SameLine(0, 20f);
            if (ImGui.Button("Help", DPI.StandardButtonSize))
            {
                ImGui.OpenPopup("##ParamHelp");
            }

            if (ImGui.BeginPopup("##ParamHelp"))
            {
                ImGui.Text(hint);

                ImGui.EndPopup();
            }
        }

        return ret;
    }
}
