using HKLib.hk2018.hkaiCollisionAvoidance;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD event instruction selection, viewing and editing.
/// </summary>
public class EmevdInstructionView
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;
    private EmevdFilters Filters;
    private EmevdContextMenu ContextMenu;

    public EmevdInstructionView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the instruction view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Instructions##EventInstructionView");
        Selection.SwitchWindowContext(EmevdEditorContext.InstructionList);

        Filters.DisplayInstructionFilterSearch();

        ImGui.BeginChild("InstructionListSection");
        Selection.SwitchWindowContext(EmevdEditorContext.InstructionList);

        if (Selection.SelectedEvent != null)
        {
            for (int i = 0; i < Selection.SelectedEvent.Instructions.Count; i++)
            {
                var ins = Selection.SelectedEvent.Instructions[i];
                var name = $"{ins.Bank}[{ins.ID}]";

                if (Filters.IsInstructionFilterMatch(ins))
                {
                    if (ImGui.Selectable($@" {name}##eventInstruction{i}", ins == Selection.SelectedInstruction))
                    {
                        Selection.SelectedInstruction = ins;
                        Selection.SelectedInstructionIndex = i;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextInstruction)
                    {
                        Selection.SelectNextInstruction = false;
                        Selection.SelectedInstruction = ins;
                        Selection.SelectedInstructionIndex = i;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextInstruction = true;
                    }

                    // Only apply to selection
                    if (Selection.SelectedInstructionIndex != -1)
                    {
                        if (Selection.SelectedInstructionIndex == i)
                        {
                            ContextMenu.InstructionContextMenu(ins);
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

        foreach (var classEntry in EmevdBank.InfoBank.Classes)
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
