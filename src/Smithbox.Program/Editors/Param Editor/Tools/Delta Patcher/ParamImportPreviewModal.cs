using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class ParamImportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public string Filename = "";
    public ParamDeltaPatch PatchForImport = null;

    // Selection State
    private readonly ParamDeltaPatchTree Tree = new();

    public ParamImportPreviewModal(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void Show(string filename, ParamDeltaPatch patch)
    {
        Filename = filename;
        PatchForImport = patch;

        Tree.Reset(patch);

        DisplayModal = true;
        InitialLayout = false;
    }

    public void Hide()
    {
        PatchForImport = null;
        Tree.Reset(null);
        DisplayModal = false;
    }

    public void Draw()
    {
        if (!DisplayModal || PatchForImport == null)
            return;

        var popupName = $"{LOC.Get("PARAM_DeltaPatcher_Import_Preview_Modal")}###importPreviewModal";

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

            ImGui.BeginChild("##import_preview_scroll", new Vector2(600, 400) * DPI.UIScale());
            Tree.Draw(null);
            ImGui.EndChild();

            ImGui.Separator();

            DrawFooterButtons();

            ImGui.EndPopup();
        }
    }

    private void DrawHeader()
    {
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Project_Type", PatchForImport.ProjectType));
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Param_Version", PatchForImport.ParamVersion));
        ImGui.Text(LOC.Get("PARAM_DeltaPatcher_Preview_Param_Change_Count", PatchForImport.Params.Count));

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
        // Import
        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Import")}##importAction", new Vector2(120 * DPI.UIScale(), 0)))
        {
            var filtered = Tree.BuildFilteredPatch();

            Patcher.Importer.ImportDelta(Filename, filtered);

            Smithbox.Log(this, LOC.Get("PARAM_DeltaPatcher_Log_Import_PASS", Filename));

            Hide();
        }

        ImGui.SameLine();

        // Cancel
        if (ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_Preview_Action_Cancel")}##cancelAction", new Vector2(120 * DPI.UIScale(), 0)))
        {
            Hide();
        }
    }
}
