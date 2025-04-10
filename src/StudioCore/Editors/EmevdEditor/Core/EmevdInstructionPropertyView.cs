using DotNext.Collections.Generic;
using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD event instruction parameter viewing and editing.
/// </summary>
public class EmevdInstructionPropertyView
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;
    private EmevdPropertyEditor PropEditor;
    private EmevdParameterManager ParameterManager;

    public List<ArgDoc> ArgumentDocs { get; set; }
    public List<object> Arguments { get; set; }

    public EmevdInstructionPropertyView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        ParameterManager = screen.ParameterManager;
        PropEditor = new EmevdPropertyEditor(screen, this);
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the instruction argument view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Instruction Properties##InstructionParameterView");
        Selection.SwitchWindowContext(EmevdEditorContext.InstructionProperties);

        if (Selection.SelectedEvent != null && Selection.SelectedInstruction != null)
        {
            var instruction = Selection.SelectedInstruction;

            if (EmevdUtils.HasArgDoc(instruction))
            {
                (ArgumentDocs, Arguments) = EmevdUtils.BuildArgumentList(instruction);
                Decorator.StoreInstructionInfo(instruction, ArgumentDocs, Arguments);

                ImGui.Columns(2);

                // Names
                for (int i = 0; i < Arguments.Count; i++)
                {
                    var arg = Arguments[i];
                    var argDoc = ArgumentDocs[i];

                    // Property Name
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{argDoc.Name}");

                    // Enum Reference
                    if (argDoc.EnumName != null)
                    {
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("");
                    }

                    // Param Reference
                    if (Decorator.HasParamReference(argDoc.Name))
                    {
                        Decorator.DetermineParamReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Text Reference
                    if (Decorator.HasTextReference(argDoc.Name))
                    {
                        Decorator.DetermineTextReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Alias Reference
                    if (Decorator.HasAliasReference(argDoc.Name))
                    {
                        Decorator.DetermineAliasReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Entity Reference
                    if (Decorator.HasMapEntityReference(argDoc.Name))
                    {
                        Decorator.DetermineMapEntityReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }
                }

                ImGui.NextColumn();

                // Properties
                for (int i = 0; i < Arguments.Count; i++)
                {
                    var argDoc = ArgumentDocs[i];

                    object newValue;
                    (bool, bool) propEditResults = PropEditor.InstructionArgumentPropertyRow(argDoc, Arguments[i], out newValue);

                    var changed = propEditResults.Item1;
                    var committed = propEditResults.Item2;

                    if (changed && committed)
                    {
                        // Update the argument value
                        Arguments[i] = newValue;

                        // Then prepare action that updates all arguments
                        var oldArguments = (byte[])instruction.ArgData.Clone();
                        var newArguments = instruction.UpdateArgs(Arguments);

                        var currentInfo = Selection.SelectedFileInfo;

                        var action = new InstructionArgumentChange(currentInfo, instruction, oldArguments, newArguments);
                        Screen.EditorActionManager.ExecuteAction(action);
                    }

                    //ImGui.Text($"{arg.ArgObject}");

                    // Enum Reference
                    if (argDoc.EnumName != null)
                    {
                        Decorator.DisplayEnumReference(argDoc, Arguments[i], i);
                    }

                    // Param Reference
                    if (Decorator.HasParamReference(argDoc.Name))
                    {
                        Decorator.DetermineParamReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Text Reference
                    if (Decorator.HasTextReference(argDoc.Name))
                    {
                        Decorator.DetermineTextReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Alias Reference
                    if (Decorator.HasAliasReference(argDoc.Name))
                    {
                        Decorator.DetermineAliasReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Entity Reference
                    if (Decorator.HasMapEntityReference(argDoc.Name))
                    {
                        Decorator.DetermineMapEntityReference(argDoc.Name, $"{Arguments[i]}", i);
                    }
                }

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

   
}

