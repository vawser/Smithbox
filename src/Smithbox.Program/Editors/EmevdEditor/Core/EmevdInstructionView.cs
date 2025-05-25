using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the EMEVD event instruction selection, viewing and editing.
/// </summary>
public class EmevdInstructionView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdInstructionView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the instruction view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Instructions##EventInstructionView");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.InstructionList);

        Editor.Filters.DisplayInstructionFilterSearch();

        ImGui.BeginChild("InstructionListSection");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.InstructionList);

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
                        Editor.Selection.SelectInstruction(ins, i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectNextInstruction)
                    {
                        Editor.Selection.SelectNextInstruction = false;
                        Editor.Selection.SelectInstruction(ins, i);
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
                            Editor.ContextMenu.InstructionContextMenu(ins);
                        }
                    }

                    DisplayInstructionAlias(ins);
                }
            }

        }

        ImGui.EndChild();

        ImGui.End();
    }

    /// <summary>
    /// Displays the ArgDoc alias for the passed Instruction if possible
    /// </summary>
    public void DisplayInstructionAlias(EMEVD.Instruction ins)
    {
        var classStr = "Unknown";
        var insStr = "Unknown";
        var argsStr = "";

        foreach (var classEntry in Project.EmevdData.PrimaryBank.InfoBank.Classes)
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
