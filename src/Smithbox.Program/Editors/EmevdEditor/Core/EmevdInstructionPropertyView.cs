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

                if (ImGui.BeginTable("InstructionArgsTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuter))
                {
                    for (int i = 0; i < Arguments.Count; i++)
                    {
                        var arg = Arguments[i];
                        var argDoc = ArgumentDocs[i];

                        ImGui.TableNextRow();

                        // Left column: Property label and decorators
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{argDoc.Name}");

                        // Show decorator spacing references under the label if needed
                        if (argDoc.EnumName != null)
                            ImGui.Text("");

                        // Right column: Editable property input and decorator UIs
                        ImGui.TableSetColumnIndex(1);
                        object newValue;
                        (bool changed, bool committed) = Editor.PropertyInput.InstructionArgumentPropertyRow(i, argDoc, arg, out newValue);

                        if (changed && committed)
                        {
                            Arguments[i] = newValue;

                            var oldArguments = (byte[])instruction.ArgData.Clone();
                            var newArguments = instruction.UpdateArgs(Arguments);

                            var action = new InstructionArgumentChange(instruction, oldArguments, newArguments);
                            Editor.EditorActionManager.ExecuteAction(action);
                        }

                        // Show relevant decorators for value
                        if (argDoc.EnumName != null)
                            Editor.Decorator.DisplayEnumReference(argDoc, Arguments[i], i);

                        // Param Ref
                        if (Editor.Project.ParamEditor != null)
                        {
                            if (Editor.Decorator.HasParamReference(argDoc))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.TableSetColumnIndex(1);
                                Editor.Decorator.DetermineParamReference(argDoc, $"{Arguments[i]}", i);
                            }
                        }

                        // Fmg Ref
                        if (Editor.Project.TextEditor != null)
                        {
                            if (Editor.Decorator.HasTextReference(argDoc))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.TableSetColumnIndex(1);
                                Editor.Decorator.DetermineTextReference(argDoc, $"{Arguments[i]}", i);
                            }
                        }

                        // Alias Ref
                        if (Editor.Decorator.HasAliasReference(argDoc))
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.TableSetColumnIndex(1);
                            Editor.Decorator.DetermineAliasReference(argDoc, $"{Arguments[i]}", i);
                        }

                        // Map Ref
                        if (Editor.Decorator.HasMapEntityReference(argDoc))
                        {
                            if (Editor.Project.MapEditor != null)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.TableSetColumnIndex(1);

                                Editor.Decorator.DisplayDefaultEntityReferences(argDoc, $"{Arguments[i]}", i);
                                Editor.Decorator.DetermineMapEntityReference(argDoc, $"{Arguments[i]}", i);
                            }
                        }
                    }

                    ImGui.EndTable();
                }
            }
        }

        ImGui.End();
    }


}

