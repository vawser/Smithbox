using ImGuiNET;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;
public static class UIHelper
{
    public static void RestoreImguiIfMissing()
    {
        var curImgui = $@"{AppContext.BaseDirectory}\imgui.ini";
        var defaultImgui = $@"{AppContext.BaseDirectory}\imgui.default";

        if (!File.Exists(curImgui) && File.Exists(defaultImgui))
        {
            var bytes = File.ReadAllBytes(defaultImgui);
            File.WriteAllBytes(curImgui, bytes);
        }
    }

    public static void ShowMenuIcon(string iconStr)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.TextUnformatted(iconStr);
        ImGui.PopStyleVar(1);
        ImGui.SameLine();
    }

    public static void ShowActiveStatus(bool isActive)
    {
        if (isActive)
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{ForkAwesome.CheckSquare}");
            ImGui.PopStyleVar(1);
        }
        else
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{ForkAwesome.Square}");
            ImGui.PopStyleVar(1);
        }
    }

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
        if (UI.Current.System_Show_UI_Tooltips)
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

    public static void ShowHoverTooltip(string desc)
    {
        if (UI.Current.System_Show_UI_Tooltips)
        {
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

    public static void WrappedText(string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    public static void HelpTextColored(Vector4 color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    public static void WrappedTextColored(Vector4 color, string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.PopTextWrapPos();
    }

    public static void ShowWideHoverTooltip(string desc)
    {
        if (UI.Current.System_Show_UI_Tooltips)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(800.0f);
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

    public static void DisplayAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (UI.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{aliasName}");
            }
        }
    }

    public static void DisplayColoredAlias(string aliasName, Vector4 color)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (UI.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(color, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(color, @$"{aliasName}");
            }
        }
    }
}
