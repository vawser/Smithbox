using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using System.Collections.Generic;
using static StudioCore.EventScriptEditorNS.EMEDF;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the EMEVD event instruction parameter viewing and editing.
/// </summary>
public class EmevdInstructionPropertyView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public List<ArgDoc> ArgumentDocs { get; set; }
    public List<object> Arguments { get; set; }

    public EmevdInstructionPropertyView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the instruction argument view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Instruction Properties##InstructionParameterView");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.InstructionProperties);

        if (Editor.Selection.SelectedEvent != null && Editor.Selection.SelectedInstruction != null)
        {
            var instruction = Editor.Selection.SelectedInstruction;

            if (EmevdUtils.HasArgDoc(Editor, instruction))
            {
                (ArgumentDocs, Arguments) = EmevdUtils.BuildArgumentList(Editor, instruction);
                Editor.Decorator.StoreInstructionInfo(instruction, ArgumentDocs, Arguments);

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
                    if (Editor.Decorator.HasParamReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineParamReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Text Reference
                    if (Editor.Decorator.HasTextReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineTextReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Alias Reference
                    if (Editor.Decorator.HasAliasReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineAliasReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }

                    // Entity Reference
                    if (Editor.Decorator.HasMapEntityReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineMapEntityReferenceSpacing(argDoc.Name, $"{arg}", i);
                    }
                }

                ImGui.NextColumn();

                // Properties
                for (int i = 0; i < Arguments.Count; i++)
                {
                    var argDoc = ArgumentDocs[i];

                    object newValue;
                    (bool, bool) propEditResults = Editor.PropertyInput.InstructionArgumentPropertyRow(argDoc, Arguments[i], out newValue);

                    var changed = propEditResults.Item1;
                    var committed = propEditResults.Item2;

                    if (changed && committed)
                    {
                        // Update the argument value
                        Arguments[i] = newValue;

                        // Then prepare action that updates all arguments
                        var oldArguments = (byte[])instruction.ArgData.Clone();
                        var newArguments = instruction.UpdateArgs(Arguments);

                        var action = new InstructionArgumentChange(instruction, oldArguments, newArguments);
                        Editor.EditorActionManager.ExecuteAction(action);
                    }

                    //ImGui.Text($"{arg.ArgObject}");

                    // Enum Reference
                    if (argDoc.EnumName != null)
                    {
                        Editor.Decorator.DisplayEnumReference(argDoc, Arguments[i], i);
                    }

                    // Param Reference
                    if (Editor.Decorator.HasParamReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineParamReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Text Reference
                    if (Editor.Decorator.HasTextReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineTextReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Alias Reference
                    if (Editor.Decorator.HasAliasReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineAliasReference(argDoc.Name, $"{Arguments[i]}", i);
                    }

                    // Entity Reference
                    if (Editor.Decorator.HasMapEntityReference(argDoc.Name))
                    {
                        Editor.Decorator.DetermineMapEntityReference(argDoc.Name, $"{Arguments[i]}", i);
                    }
                }

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

   
}

