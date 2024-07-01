using ImGuiNET;
using StudioCore.Core;
using StudioCore.Platform;
using StudioCore.Tools;
using System;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class ToolWindow
{
    private bool MenuOpenState;

    private Vector4 currentColor;

    public ToolWindow()
    {
        
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Tools##quickToolWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#QuickToolMenuBar");

            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            DisplayTool_ColorPicker();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private void DisplayTool_ColorPicker()
    {
        if (ImGui.BeginTabItem("Color Picker"))
        {
            ImGui.ColorPicker4("##colorPicker", ref currentColor);

            if (ImGui.Button("Copy RGB Color"))
            {
                var rgbColor = $"<{Math.Round(currentColor.X * 255)}, {Math.Round(currentColor.Y * 255)}, {Math.Round(currentColor.Z * 255)}>";

                PlatformUtils.Instance.SetClipboardText(rgbColor);
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy Decimal Color"))
            {
                PlatformUtils.Instance.SetClipboardText(currentColor.ToString());
            }

            ImGui.EndTabItem();
        }
    }

}
