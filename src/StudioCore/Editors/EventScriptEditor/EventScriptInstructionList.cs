using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptInstructionList
{
    public Project Project;
    public EventScriptEditor Editor;

    public EventScriptInstructionList(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.InstructionList);

        ImGui.BeginChild("InstructionListSection");

        if (Editor.Selection.SelectedEvent != null)
        {
            for (int i = 0; i < Editor.Selection.SelectedEvent.Instructions.Count; i++)
            {
                var ins = Editor.Selection.SelectedEvent.Instructions[i];
                var name = $"{ins.Bank}[{ins.ID}]";

                if (Editor.Filters.IsInstructionFilterMatch(ins))
                {
                    if (ImGui.Selectable($@" {name}##eventInstruction{i}", ins == Editor.Selection.SelectedInstruction))
                    {
                        Editor.Selection.SelectedInstruction = ins;
                        Editor.Selection.SelectedInstructionIndex = i;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextInstruction)
                    {
                        Editor.Selection.SelectNextInstruction = false;
                        Editor.Selection.SelectedInstruction = ins;
                        Editor.Selection.SelectedInstructionIndex = i;
                    }

                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectNextInstruction = true;
                    }

                    // Only apply to selection
                    if (Editor.Selection.SelectedInstructionIndex != -1)
                    {
                        if (Editor.Selection.SelectedInstructionIndex == i)
                        {
                            DisplayInstructionContextMenu(ins);
                        }
                    }

                    DisplayInstructionAlias(ins);
                }
            }

        }

        ImGui.EndChild();
    }

    public void DisplayInstructionContextMenu(EMEVD.Instruction ins)
    {
        if (ImGui.BeginPopupContextItem($"InstructionContext##InstructionContext{ins.ID}"))
        {
            if (ImGui.Selectable($"Create##createActionInstruction{ins.ID}"))
            {
                // TODO: add creation modal
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Displays the ArgDoc alias for the passed Instruction if possible
    /// </summary>
    public void DisplayInstructionAlias(EMEVD.Instruction ins)
    {
        var classStr = "Unknown";
        var insStr = "Unknown";
        var argsStr = "";

        foreach (var classEntry in Editor.Project.EventScriptData.EventInformation.Classes)
        {
            if (ins.Bank == classEntry.Index)
            {
                classStr = classEntry.Name;

                foreach (var insEntry in classEntry.Instructions)
                {
                    if (ins.ID == insEntry.Index)
                    {
                        insStr = insEntry.Name;

                        for (int i = 0; i < insEntry.Arguments.Length; i++)
                        {
                            var argEntry = insEntry.Arguments[i];
                            string separator = ", ";

                            if (i == insEntry.Arguments.Length - 1)
                            {
                                separator = "";
                            }

                            argsStr = $"{argsStr}{argEntry.Name}{separator}";
                        }
                    }
                }

            }
        }

        if (argsStr == "")
            argsStr = "Unknown";

        var alias = $"{insStr}";

        if (CFG.Current.EmevdEditor_DisplayInstructionCategory)
        {
            alias = $"{classStr} [{insStr}]";
        }

        UIHelper.DisplayAlias(alias);

        if (CFG.Current.EmevdEditor_DisplayInstructionParameterNames)
        {
            UIHelper.DisplayColoredAlias($"({argsStr})", UI.Current.ImGui_Benefit_Text_Color);
        }
    }
}
