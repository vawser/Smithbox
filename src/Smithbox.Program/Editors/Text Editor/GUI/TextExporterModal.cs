using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextExporterModal
{
    private TextEditorView View;
    private ProjectEntry Project;

    public bool ShowModal = false;

    public string WrapperName = "";

    public TextExporterModal(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup($"{LOC.Get("TEXT_Exporter_Modal_Name")}###textExporterModal");
        }

        ExportMenu();
    }


    public void ExportMenu()
    {
        if (ImGui.BeginPopupModal($"{LOC.Get("TEXT_Exporter_Modal_Name")}###textExporterModal", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.BeginChild("TextExporterSection", new Vector2(200f * DPI.UIScale(), 0f),
                ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

            UIHelper.SimpleHeader(
                LOC.Get("TEXT_Exporter_Header_Filename"),
                LOC.Get("TEXT_Exporter_Header_Filename_TT"));

            UIHelper.SinglelineTextInput("wrapperFilename", ref WrapperName);

            UIHelper.MultiButtonInput("exportModalActions",
                "exportFile",
                LOC.Get("TEXT_Exporter_Modal_Action_Export"),
                LOC.Get("TEXT_Exporter_Modal_Action_Export_TT"),
                ExportWrapper,

                "closeModal",
                LOC.Get("TEXT_Exporter_Modal_Action_Close"),
                LOC.Get("TEXT_Exporter_Modal_Action_Close_TT"),
                CloseModal);

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    public void ExportWrapper()
    {
        if(WrapperName == "")
        {
            Smithbox.LogError<TextExporterModal>(LOC.Get("TEXT_Exporter_Log_Filename_Empty"));
            return;
        }

        var outputWrapper = View.FmgExporter.ProcessExport(WrapperName);

        var exportDir = TextUtils.GetStoredTextDirectory(Project);
        if (View.ToolView.DataTransferTool.ExportDirectory != "")
            exportDir = View.ToolView.DataTransferTool.ExportDirectory;

        View.FmgExporter.WriteWrapper(exportDir, WrapperName, outputWrapper);

        ShowModal = false;
    }

    public void CloseModal()
    {
        ShowModal = false;
    }
}