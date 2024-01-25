using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;
public static class ImguiUtils
{
    public static void ShowHelpButton(string title, string desc, string id)
    {
        if (ImGui.Button($"{title}"))
            ImGui.OpenPopup($"##{id}HelpPopup");

        if (ImGui.BeginPopup($"##{id}HelpPopup"))
        {
            ImGui.Text($"{desc}");
            ImGui.EndPopup();
        }
    }

    public static void ShowHelpMarker(string desc)
    {
        if (CFG.Current.System_Show_UI_Tooltips)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(450.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }

    public static string GetKeybindHint(string hint)
    {
        if (hint == "")
            return "None";
        else
            return hint;
    }
}
