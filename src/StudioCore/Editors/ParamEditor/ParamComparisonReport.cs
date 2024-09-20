using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Platform;
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
        var buttonSize = new Vector2(UI.Current.Interface_ModalWidth / 2, UI.Current.Interface_ButtonHeight);
        var textPaneSize = new Vector2(UI.Current.Interface_ModalWidth, UI.Current.Interface_ModalHeight);

        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Comparison Report");
        ImGui.Separator();
        UIHelper.WrappedText($"Project: Param Version: {ParamBank.PrimaryBank.ParamVersion}");
        UIHelper.WrappedText($"Game Root: Param Version: {ParamBank.VanillaBank.ParamVersion}");

        ImGui.InputTextMultiline("##reportText", ref ReportText, 65025, textPaneSize, ImGuiInputTextFlags.ReadOnly);

        if(ImGui.Button("Copy", buttonSize))
        {
            UIHelper.CopyToClipboard(ReportText);
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
