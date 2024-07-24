using ImGuiNET;
using StudioCore.Editors.ParamEditor;
using StudioCore.Localization;
using System.Numerics;

namespace StudioCore.Editor;

public class UIHints
{
    public static string MassEditHint = $"{LOC.Get("UIHINT_MASSEDIT_HINT")}";

    public static string SearchBarHint = $"{LOC.Get("UIHINT_SEARCH_BAR_HINT")}";

    public static bool AddImGuiHintButton(string id, ref string hint, bool canEdit = false, bool isRowHint = false)
    {
        var scale = Smithbox.GetUIScale();
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
                if (ParamEditorScreen.EditorMode && canEdit) //remove this, editor mode should be called earlier
                {
                    ImGui.InputTextMultiline("", ref hint, 8196, new Vector2(720, 480) * scale);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        ret = true;
                    }
                }
                else
                {
                    ImGui.Text(hint);
                }

                ImGui.EndPopup();
            }

            ImGui.SameLine();
        }
        else
        {
            ImGui.SameLine(0, 20f);
            if (ImGui.Button($"{LOC.Get("UIHINT_HELP")}##helpButton"))
            {
                ImGui.OpenPopup("##ParamHelp");
            }

            if (ImGui.BeginPopup("##ParamHelp"))
            {
                if (ParamEditorScreen.EditorMode && canEdit) //remove this, editor mode should be called earlier
                {
                    ImGui.InputTextMultiline("", ref hint, 8196, new Vector2(720, 480) * scale);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        ret = true;
                    }
                }
                else
                {
                    ImGui.Text(hint);
                }

                ImGui.EndPopup();
            }
        }

        return ret;
    }
}
