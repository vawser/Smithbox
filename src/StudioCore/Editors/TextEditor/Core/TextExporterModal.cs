using ImGuiNET;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextExporterModal
{
    private TextEditorScreen Screen;
    private TextSelectionManager Selection;

    public bool ShowModal = false;
    public ExportType ExportType = ExportType.All;

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
            ImGui.OpenPopup("Export Text Modal");
        }

        ExportMenu();
    }


    public void ExportMenu()
    {
        if (ImGui.BeginPopupModal("Export Text Modal", ref ShowModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var width = UI.ModalButtonSize;

            ImGui.Text("Name");
            ImGui.SetNextItemWidth(width.X);
            ImGui.InputText("##wrapperName", ref WrapperName, 255);

            if (ImGui.Button("Export", UI.ModalButtonHalfSize))
            {
                ShowModal = false;
                FmgExporter.ProcessExport(WrapperName, ExportType);
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

public enum ExportType
{
    All,
    Selected
}