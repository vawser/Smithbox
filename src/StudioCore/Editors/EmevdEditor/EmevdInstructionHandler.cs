using ImGuiNET;
using SoulsFormats;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public class EmevdInstructionHandler
{
    private EmevdEditorScreen Screen;
    private EmevdDecorator Decorator;

    public EmevdInstructionHandler(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
    }

    public void OnProjectChanged()
    {

    }

    public List<ArgDataObject> Arguments { get; set; }

    public void Display()
    {
        if (Screen._selectedEvent != null && Screen._selectedInstruction != null)
        {
            var instruction = Screen._selectedInstruction;

            Arguments = BuildArgumentList(instruction);
            Decorator.StoreInstructionInfo(instruction, Arguments);

            ImGui.Columns(2);

            // Names
            for(int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i];

                // Property Name
                ImGui.Text($"{arg.ArgDoc.Name}");

                // Enum Reference
                if(arg.ArgDoc.EnumName != null)
                {
                    ImGui.Text("");
                }

                // Param Reference
                if (Decorator.HasParamReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineParamReferenceSpacing(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }

                // Alias Reference
                if (Decorator.HasAliasReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineAliasReferenceSpacing(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }

                // Entity Reference
                if (Decorator.HasEntityReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineEntityReferenceSpacing(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }
            }

            ImGui.NextColumn();

            // Properties
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i];

                // Property Value
                // TODO: add property edit
                ImGui.Text($"{arg.ArgObject}");

                // Enum Reference
                if (arg.ArgDoc.EnumName != null)
                {
                    Decorator.DisplayEnumReference(arg, i);
                }

                // Param Reference
                if (Decorator.HasParamReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineParamReference(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }

                // Alias Reference
                if (Decorator.HasAliasReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineAliasReference(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }

                // Entity Reference
                if (Decorator.HasEntityReference(arg.ArgDoc.Name))
                {
                    Decorator.DetermineEntityReference(arg.ArgDoc.Name, $"{arg.ArgObject}", i);
                }
            }


            ImGui.Columns(1);
        }
    }

    private List<ArgDataObject> BuildArgumentList(Instruction ins)
    {
        var argList = new List<ArgDataObject>();

        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if(classDoc == null)
        {
            return argList;
        }

        var instructionDoc = classDoc[ins.ID];

        if(instructionDoc == null)
        {
            return argList;
        }

        var data = ins.ArgData;

        List<ArgType> argTypes = instructionDoc.Arguments.Select(arg => arg.Type == 8 ? ArgType.UInt32 : (ArgType)arg.Type).ToList();

        var argObjects = UnpackArguments(ins, argTypes);

        for (int i = 0; i < instructionDoc.Arguments.Length; i++)
        {
            var entry = instructionDoc.Arguments[i];
            var obj = argObjects[i];

            argList.Add(new ArgDataObject(entry, obj));
        }

        return argList;
    }

    private List<object> UnpackArguments(Instruction ins, IEnumerable<ArgType> argStruct, bool bigEndian = false)
    {
        var result = new List<object>();
        using (var ms = new MemoryStream(ins.ArgData))
        {
            byte[] bytes = ms.ToArray();

            var br = new BinaryReaderEx(bigEndian, bytes);
            foreach (ArgType arg in argStruct)
            {
                switch (arg)
                {
                    case ArgType.Byte:
                        result.Add(br.ReadByte()); break;
                    case ArgType.UInt16:
                        AssertZeroPad(br, 2);
                        result.Add(br.ReadUInt16()); break;
                    case ArgType.UInt32:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadUInt32()); break;
                    case ArgType.SByte:
                        result.Add(br.ReadSByte()); break;
                    case ArgType.Int16:
                        AssertZeroPad(br, 2);
                        result.Add(br.ReadInt16()); break;
                    case ArgType.Int32:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadInt32()); break;
                    case ArgType.Single:
                        AssertZeroPad(br, 4);
                        result.Add(br.ReadSingle()); break;

                    default:
                        throw new NotImplementedException($"Unimplemented argument type: {arg}");
                }
            }
            AssertZeroPad(br, 4);
        }
        return result;
    }

    private void AssertZeroPad(BinaryReaderEx br, int align)
    {
        if (br.Position % align > 0)
        {
            br.AssertPattern(align - (int)(br.Position % align), 0);
        }
    }

}

public class ArgDataObject
{
    public ArgDoc ArgDoc { get; set; }

    public object ArgObject { get; set; }

    public ArgDataObject(ArgDoc argDoc, object argObj)
    {
        ArgDoc = argDoc;
        ArgObject = argObj;
    }
}