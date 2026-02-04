using Hexa.NET.ImGui;
using StudioCore.Application;

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
            ImGui.OpenPopup("Export Text");
        }

        ExportMenu();
    }


    public void ExportMenu()
    {
        if (ImGui.BeginPopupModal("Export Text", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var windowWidth = 520f;

            ImGui.Text("Name");
            DPI.ApplyInputWidth(windowWidth);
            ImGui.InputText("##wrapperName", ref WrapperName, 255);

            if(WrapperName == "")
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.Button("Export", DPI.HalfWidthButton(windowWidth, 24)))
            {
                ShowModal = false;
                Parent.FmgExporter.ProcessExport(WrapperName);
            }
            if (WrapperName == "")
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Close", DPI.HalfWidthButton(windowWidth, 24)))
            {
                ShowModal = false;
            }


            ImGui.EndPopup();
        }
    }
}