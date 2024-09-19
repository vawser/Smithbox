using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public static class ParamComparisonReport
{
    public static bool ShowReportModal = false;

    public static string ReportText = "";

    public static void GenerateReport()
    {
        List<string> reportLines = new List<string>();

        var primaryBank = ParamBank.PrimaryBank;
        var vanillaBank = ParamBank.VanillaBank;

        foreach (var param in primaryBank.Params)
        {
            var paramKey = param.Key;
            var paramData = param.Value;


            // Cells

        }

        ReportText = "";

        ShowReportModal = true;
    }

    public static void Display()
    {
        var width = 800;
        var buttonSize = new Vector2(width / 2, 32);
        var textPaneSize = new Vector2(width, 600);

        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Comparison Report");
        ImGui.Separator();
        ImguiUtils.WrappedText($"Project: Param Version: {ParamBank.PrimaryBank.ParamVersion}");
        ImguiUtils.WrappedText($"Game Root: Param Version: {ParamBank.VanillaBank.ParamVersion}");

        ImGui.InputTextMultiline("##reportText", ref ReportText, 65025, textPaneSize, ImGuiInputTextFlags.ReadOnly);

        if(ImGui.Button("Copy", buttonSize))
        {
            PlatformUtils.Instance.SetClipboardText(ReportText);
        }
        ImGui.SameLine();
        if (ImGui.Button("Close", buttonSize))
        {
            ReportText = "";
            ShowReportModal = false;
        }
    }

    public static void HandleReportModal()
    {
        if (ShowReportModal)
        {
            ImGui.OpenPopup("Param Comparison Report");
        }

        if (ImGui.BeginPopupModal("Param Comparison Report", ref ShowReportModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            Display();

            ImGui.EndPopup();
        }
    }
}
