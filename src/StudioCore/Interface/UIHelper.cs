using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Platform;
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
    public static void ApplyBaseStyle()
    {
        var scale = DPI.GetUIScale();
        ImGuiStylePtr style = ImGui.GetStyle();

        // Colors
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_MainBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.ImGui_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, UI.Current.ImGui_PopupBg);
        ImGui.PushStyleColor(ImGuiCol.Border, UI.Current.ImGui_Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Background);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, UI.Current.ImGui_Input_Background_Hover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, UI.Current.ImGui_Input_Background_Active);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.ImGui_TitleBarBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.ImGui_TitleBarBg_Active);
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, UI.Current.ImGui_MenuBarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, UI.Current.ImGui_ScrollbarBg);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, UI.Current.ImGui_ScrollbarGrab);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, UI.Current.ImGui_ScrollbarGrab_Hover);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, UI.Current.ImGui_ScrollbarGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.ImGui_Input_CheckMark);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, UI.Current.ImGui_SliderGrab);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, UI.Current.ImGui_SliderGrab_Active);
        ImGui.PushStyleColor(ImGuiCol.Button, UI.Current.ImGui_Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, UI.Current.ImGui_Button_Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, UI.Current.ImGui_ButtonActive);
        ImGui.PushStyleColor(ImGuiCol.Header, UI.Current.ImGui_Selection);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, UI.Current.ImGui_Selection_Hover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, UI.Current.ImGui_Selection_Active);
        ImGui.PushStyleColor(ImGuiCol.Tab, UI.Current.ImGui_Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, UI.Current.ImGui_Tab_Hover);
        ImGui.PushStyleColor(ImGuiCol.TabSelected, UI.Current.ImGui_Tab_Active);
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, UI.Current.ImGui_UnfocusedTab);
        ImGui.PushStyleColor(ImGuiCol.TabDimmedSelected, UI.Current.ImGui_UnfocusedTab_Active);

        // Sizes
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0.0f);

        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 16.0f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(100f, 100f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, style.CellPadding * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, style.IndentSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, style.ItemInnerSpacing * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
    }

    public static void UnapplyBaseStyle()
    {
        ImGui.PopStyleColor(29);
        ImGui.PopStyleVar(14);
    }

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
            ImGui.TextUnformatted($"{Icons.CheckSquare}");
            ImGui.PopStyleVar(1);
        }
        else
        {
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted($"{Icons.Square}");
            ImGui.PopStyleVar(1);
        }
    }

    public static void Tooltip(string desc)
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

    public static void WrappedText(string text)
    {
        var size = ImGui.GetWindowSize();

        ImGui.PushTextWrapPos(size.X);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
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

    public static void WideTooltip(string desc)
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
            if (CFG.Current.System_WrapAliasDisplay)
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
            if (CFG.Current.System_WrapAliasDisplay)
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

    public static void CopyToClipboard(string text)
    {
        PlatformUtils.Instance.SetClipboardText(text);
    }
    public static void SimpleHeader(string id, string title, string tooltip, Vector4 textColor)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(textColor, $"{title}");
            UIHelper.Tooltip(tooltip);

            ImGui.EndTable();
        }
    }

    public static nuint GetTextInputBuffer(string contents)
    {
        int byteCount = Encoding.UTF8.GetByteCount(contents) + 1;
        return (nuint)byteCount;
    }
}
