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

    /// <summary>Open the modal with the name field filled in (the auto name for the selection; editable).</summary>
    public void Open(string defaultName)
    {
        WrapperName = defaultName ?? "";
        ShowModal = true;
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
            ImGui.BeginChild("TextExporterSection", new Vector2(360f * DPI.UIScale(), 0f),
                ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

            GUI.SimpleHeader(
                LOC.Get("TEXT_Exporter_Header_Filename"),
                LOC.Get("TEXT_Exporter_Header_Filename_TT"));

            GUI.SinglelineTextInput("wrapperFilename", ref WrapperName);

            GUI.MultiButtonInput("exportModalActions",
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

        // Same path as the panel and Quick Export (filename made safe there)
        View.FmgExporter.InitializeExport(WrapperName, View.FmgExporter.CurrentExportType, View.FmgExporter.CurrentExportModifier);

        ShowModal = false;
    }

    public void CloseModal()
    {
        ShowModal = false;
    }
}