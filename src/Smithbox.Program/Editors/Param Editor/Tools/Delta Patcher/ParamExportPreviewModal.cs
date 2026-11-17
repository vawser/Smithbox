using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class ParamExportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamDeltaPatch PatchForExport = null;

    private readonly ParamDeltaPatchTree Tree = new();

    public ParamExportPreviewModal(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void Show(ParamDeltaPatch exportPatch)
    {
        PatchForExport = exportPatch;

        // Default everything to enabled
        Tree.Reset(exportPatch);

        DisplayModal = true;
        InitialLayout = false;
    }

    public void Hide()
    {
        PatchForExport = null;
        Tree.Reset(null);
        DisplayModal = false;
    }

    public void Draw()
    {
        if (!DisplayModal || PatchForExport == null)
            return;

        var popupName = $"{LOC.Get("PARAM_DeltaPatcher_Export_Preview_Modal")}###exportPreviewModal";

        ImGui.OpenPopup(popupName);

        if (!InitialLayout)
        {
            GUI.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            DrawHeader();

            ImGui.Separator();

            ImGui.BeginChild("##preview_scroll", new System.Numerics.Vector2(600, 400));
            Tree.Draw(LOC.Get("PARAM_DeltaPatcher_Preview_Export_Row_Toggle_TT"));
            ImGui.EndChild();

            ImGui.Separator();

            DrawFooterButtons();

            ImGui.EndPopup();
        }
    }

    private void DrawHeader()
    {
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Project_Type", PatchForExport.ProjectType));
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Param_Version", PatchForExport.ParamVersion));
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Param_Change_Count", PatchForExport.Params.Count));

        ImGui.Spacing();

        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Enable_All")}##enableAllAction"))
        {
            Tree.SetAll(true);
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Disable_All")}##disableAllAction"))
        {
            Tree.SetAll(false);
        }

        ImGui.Spacing();
    }

    private void DrawFooterButtons()
    {
        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Export")}##exportAction", DPI.StandardButtonSize))
        {
            var filteredPatch = Tree.BuildFilteredPatch();

            Patcher.WriteDeltaPatch(filteredPatch, Patcher.Selection.ExportName);

            Smithbox.Log(this, LOC.Get("PARAM_DeltaPatcher_Log_Export_PASS", Patcher.Selection.ExportName));

            Patcher.Selection.QueueImportListRefresh = true;

            Hide();
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Cancel")}##cancelAction", DPI.StandardButtonSize))
        {
            Hide();
        }
    }
}
