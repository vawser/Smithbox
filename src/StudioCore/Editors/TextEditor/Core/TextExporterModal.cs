using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextExporterModal
{
    private TextEditorScreen Editor;
    private TextSelectionManager Selection;

    public bool ShowModal = false;

    public string WrapperName = "";

    public TextExporterModal(TextEditorScreen screen)
    {
        Editor = screen;
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
            var width = new Vector2(520 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
            var buttonSize = new Vector2(260 * DPI.GetUIScale(), 24 * DPI.GetUIScale());

            ImGui.Text("Name");
            ImGui.SetNextItemWidth(width.X);
            ImGui.InputText("##wrapperName", ref WrapperName, 255);

            if(WrapperName == "")
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.Button("Export", buttonSize))
            {
                ShowModal = false;
                Editor.FmgExporter.ProcessExport(WrapperName);
            }
            if (WrapperName == "")
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowModal = false;
            }


            ImGui.EndPopup();
        }
    }
}