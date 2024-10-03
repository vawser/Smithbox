using DotNext.Collections.Generic;
using ImGuiNET;
using Octokit;
using SoulsFormats;
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

public class EmevdInstructionHandler
{
    private EmevdEditorScreen Screen;
    private EmevdDecorator Decorator;
    private EmevdPropertyEditor PropEditor;

    public EmevdInstructionHandler(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        PropEditor = new EmevdPropertyEditor(screen, this);
    }

    public void OnProjectChanged()
    {

    }

    public List<ArgDoc> ArgumentDocs { get; set; }
    public List<object> Arguments { get; set; }

    public void Display()
    {
        if (Screen._selectedEvent != null && Screen._selectedInstruction != null)
        {
            var instruction = Screen._selectedInstruction;

            if (HasArgDoc(instruction))
            {
                (ArgumentDocs, Arguments) = BuildArgumentList(instruction);
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
                    if (Decorator.HasEntityReference(argDoc.Name))
                    {
                        Decorator.DetermineEntityReferenceSpacing(argDoc.Name, $"{arg}", i);
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

                        var action = new InstructionArgumentChange(instruction, oldArguments, newArguments);
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
                    if (Decorator.HasEntityReference(argDoc.Name))
                    {
                        Decorator.DetermineEntityReference(argDoc.Name, $"{Arguments[i]}", i);
                    }
                }

                ImGui.Columns(1);
            }
            else
            {
                // Display the byte contents as blocks of 4 bytes
                // Also show the potential int and float values
                byte[] blockArr = new byte[4];
                int blockIndex = 0;

                for(int i = 0; i < instruction.ArgData.Length; i++)
                {
                    var block = instruction.ArgData[i];

                    blockArr[blockIndex] = block;

                    blockIndex++;

                    if (i % 4 == 0)
                    {
                        int iValue = BitConverter.ToInt32(blockArr, 0);
                        float fValue = BitConverter.ToSingle(blockArr, 0);
                        short sValue_1 = BitConverter.ToInt16(blockArr[..2], 0);
                        short sValue_2 = BitConverter.ToInt16(blockArr[2..], 0);

                        var arrStr = "";
                        foreach(var item in blockArr)
                        {
                            if(item < 10)
                            {
                                arrStr += $"00{item} ";
                            }
                            else if (item < 100)
                            {
                                arrStr += $"0{item} ";
                            }
                            else
                            {
                                arrStr += $"{item} ";
                            }
                        }

                        UIHelper.WrappedText($"{arrStr} | Int: {iValue} | Float: {fValue} | Shorts: {sValue_1}, {sValue_2}");
                        blockIndex = 0;
                        blockArr = new byte[4];
                    }
                }
            }
        }
    }

    private bool HasArgDoc(Instruction ins)
    {
        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if (classDoc == null)
        {
            return false;
        }

        var instructionDoc = classDoc[ins.ID];

        if (instructionDoc == null)
        {
            return false;
        }

        return true;
    }

    private (List<ArgDoc>, List<object>) BuildArgumentList(Instruction ins)
    {
        var argList = new List<object>();
        var argDocList = new List<ArgDoc>();

        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if(classDoc == null)
        {
            return (argDocList, argList);
        }

        var instructionDoc = classDoc[ins.ID];

        if(instructionDoc == null)
        {
            return (argDocList, argList);
        }

        var data = ins.ArgData;

        List<ArgType> argTypes = instructionDoc.Arguments.Select(arg => arg.Type == 8 ? ArgType.UInt32 : (ArgType)arg.Type).ToList();

        var argObjects = ins.UnpackArgs(argTypes);

        for (int i = 0; i < instructionDoc.Arguments.Length; i++)
        {
            var entry = instructionDoc.Arguments[i];
            var obj = argObjects[i];

            argDocList.Add(entry);
            argList.Add(obj);
        }

        return (argDocList, argList);
    }
}

