using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TextEditor;

namespace StudioCore.Editors.TextEditor;

public class TextExporterModal
{
    private TextEditorScreen Screen;
    private TextSelectionManager Selection;

    public bool ShowModal = false;

    public string WrapperName = "";

    public TextExporterModal(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
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
            var width = UI.ModalButtonSize;

            ImGui.Text("Name");
            ImGui.SetNextItemWidth(width.X);
            ImGui.InputText("##wrapperName", ref WrapperName, 255);

            if(WrapperName == "")
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.Button("Export", UI.ModalButtonHalfSize))
            {
                ShowModal = false;
                FmgExporter.ProcessExport(WrapperName);
            }
            if (WrapperName == "")
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Close", UI.ModalButtonHalfSize))
            {
                ShowModal = false;
            }


            ImGui.EndPopup();
        }
    }
}