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

    /// <summary>
    /// Set by the shortcut handler; the popup is opened from Display() instead.
    /// ImGui popup IDs include the window ID stack, so OpenPopup has to run inside
    /// the same view window as BeginPopup. Opening it from TextShortcuts.Monitor()
    /// (editor-window level) targeted a different ID and the popup never appeared.
    /// </summary>
    public bool OpenRequested = false;

    public TextDuplicatePopup(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        if (OpenRequested)
        {
            OpenRequested = false;
            ImGui.OpenPopup("textDuplicatePopup");
        }

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
                if (int.TryParse(DuplicateOffset, out var offset))
                {
                    var amount = DuplicateAmount < 1 ? 1 : DuplicateAmount;

                    Parent.ActionHandler.DuplicateEntriesPopup(offset, amount, AutoAdjustOffset);

                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    Smithbox.LogWarning(this, LOC.Get("TEXT_DuplicateModal_Invalid_Offset"));
                }
            }

            ImGui.EndPopup();
        }
    }
}
