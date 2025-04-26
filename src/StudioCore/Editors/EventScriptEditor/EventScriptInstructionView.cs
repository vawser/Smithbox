using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EmevdEditor;
using System.Collections.Generic;
using static StudioCore.Editors.EventScriptEditorNS.EMEDF;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptInstructionView
{
    public Project Project;
    public EventScriptEditor Editor;

    public List<ArgDoc> ArgumentDocs { get; set; }
    public List<object> Arguments { get; set; }

    public EventScriptInstructionView(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.InstructionProperties);

        if (Editor.Selection.SelectedEvent != null && Editor.Selection.SelectedInstruction != null)
        {
            var instruction = Editor.Selection.SelectedInstruction;

            if (EventScriptUtils.HasArgDoc(Editor, instruction))
            {
                (ArgumentDocs, Arguments) = EventScriptUtils.BuildArgumentList(Editor,instruction);
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
                    (bool, bool) propEditResults = Editor.InstructionInput.InstructionArgumentPropertyRow(argDoc, Arguments[i], out newValue);

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
                        Editor.ActionManager.ExecuteAction(action);
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
    }
}

