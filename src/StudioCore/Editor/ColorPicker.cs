using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editor;

public static class ColorPicker
{
    public static bool ShowColorPicker = false;
    public static Vector4 currentColor = new Vector4();

    public static void DisplayColorPicker()
    {
        var scale = DPI.GetUIScale();

        if (!ShowColorPicker)
            return;

        ImGui.SetNextWindowSize(new Vector2(1200.0f, 1000.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Color Picker##tool_ColorPicker", ref ShowColorPicker, ImGuiWindowFlags.NoDocking))
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
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }
}
