using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

public class TextDuplicatePopup
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    private string DuplicateOffset = "";
    private int DuplicateAmount = 1;
    private bool AutoAdjustOffset = true;

    public TextDuplicatePopup(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.BeginPopup("textDuplicatePopup"))
        {
            // Offset
            ImGui.InputText($"{LOC.Get("TEXT_DuplicateModal_Offset_Input")}###offsetInput", ref DuplicateOffset, 255);
            GUI.Tooltip(LOC.Get("TEXT_DuplicateModal_Offset_Input_TT"));

            // Amount
            ImGui.InputInt($"{LOC.Get("TEXT_DuplicateModal_Amount_Input")}###amountInput", ref DuplicateAmount);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                if(DuplicateAmount < 1)
                {
                    DuplicateAmount = 1;
                }
            }
            GUI.Tooltip(LOC.Get("TEXT_DuplicateModal_Amount_Input_TT"));

            // Auto-Adjust
            ImGui.Checkbox($"{LOC.Get("TEXT_DuplicateModal_Checkbox_Auto_Adjust")}###autoAdjustToggle", ref AutoAdjustOffset);
            GUI.Tooltip(LOC.Get("TEXT_DuplicateModal_Checkbox_Auto_Adjust_TT"));

            // Submit
            if (ImGui.Button($"{LOC.Get("TEXT_DuplicateModal_Action_Submit")}###submitAction", DPI.StandardButtonSize))
            {
                int offset = -1;
                var validOffset = int.TryParse(DuplicateOffset, out offset);

                if (validOffset)
                {
                    Parent.ActionHandler.DuplicateEntriesPopup(offset, DuplicateAmount, AutoAdjustOffset);
                }
            }

            ImGui.EndPopup();
        }
    }
}
