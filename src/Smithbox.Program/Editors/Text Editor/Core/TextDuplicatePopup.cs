using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

public class TextDuplicatePopup
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    private string DuplicateOffset = "";
    private int DuplicateAmount = 1;
    private bool AutoAdjustOffset = true;

    public TextDuplicatePopup(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.BeginPopup("textDuplicatePopup"))
        {
            ImGui.InputText("Offset", ref DuplicateOffset, 255);
            UIHelper.Tooltip("The offset to apply to the entry ID per duplicate instance.");

            ImGui.InputInt("Amount", ref DuplicateAmount);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                if(DuplicateAmount < 1)
                {
                    DuplicateAmount = 1;
                }
            }
            UIHelper.Tooltip("The amount of duplicate instances to generate per entry.");

            ImGui.Checkbox("Adjust Offset Automatically", ref AutoAdjustOffset);
            UIHelper.Tooltip("When more than 1 duplicate instance is defined, if this is enabled, the offset will be automatically adjusted (e.g. offset set to 100, first instance will be ID + 100, second will be ID + 200, etc.)");

            if (ImGui.Button("Submit", DPI.StandardButtonSize))
            {
                int offset = -1;
                var validOffset = int.TryParse(DuplicateOffset, out offset);

                if (validOffset)
                {
                    Editor.ActionHandler.DuplicateEntriesPopup(offset, DuplicateAmount, AutoAdjustOffset);
                }
            }

            ImGui.EndPopup();
        }
    }
}
