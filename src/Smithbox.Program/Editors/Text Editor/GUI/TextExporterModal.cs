using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

public class TextExporterModal
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public bool ShowModal = false;

    public string WrapperName = "";

    public TextExporterModal(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup("Text Exporter");
        }

        ExportMenu();
    }


    public void ExportMenu()
    {
        if (ImGui.BeginPopupModal("Text Exporter", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.BeginChild("TextExporterSection", new Vector2(200f * DPI.UIScale(), 0f),
                ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

            UIHelper.SimpleHeader("Filename", "");

            UIHelper.SinglelineTextInput("WrapperFilename", ref WrapperName);

            UIHelper.MultiButtonInput("exportModalActions",
                "exportFile", "Export", "", ExportWrapper,
                "closeModal", "Close", "", CloseModal);

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }

    public void ExportWrapper()
    {
        if(WrapperName == "")
        {
            Smithbox.LogError<TextExporterModal>("Filename is empty");
            return;
        }

        var outputWrapper = Parent.FmgExporter.ProcessExport(WrapperName);

        var exportDir = TextUtils.GetStoredTextDirectory(Project);
        if (Parent.Editor.ToolView.DataTransferTool.ExportDirectory != "")
            exportDir = Parent.Editor.ToolView.DataTransferTool.ExportDirectory;

        Parent.FmgExporter.WriteWrapper(exportDir, WrapperName, outputWrapper);

        ShowModal = false;
    }

    public void CloseModal()
    {
        ShowModal = false;
    }
}