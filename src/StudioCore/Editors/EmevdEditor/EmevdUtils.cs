using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public static class EmevdUtils
{
    public static void DisplayInstructionAlias(EMEVD.Instruction ins)
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

    public static string GetTypeString(long type)
    {
        if (type == 0) return "byte";
        if (type == 1) return "ushort";
        if (type == 2) return "uint";
        if (type == 3) return "sbyte";
        if (type == 4) return "short";
        if (type == 5) return "int";
        if (type == 6) return "float";
        if (type == 8) return "uint";
        throw new Exception("Invalid type in argument definition.");
    }

    public static int GetTypeSize(long type)
    {
        if (type == 0) return 1; // byte
        if (type == 1) return 4; // ushort
        if (type == 2) return 6; // uint
        if (type == 3) return 1; // sbyte
        if (type == 4) return 4; // short
        if (type == 5) return 6; // int
        if (type == 6) return 6; // float
        if (type == 8) return 6; // uint
        throw new Exception("Invalid type in argument definition.");
    }

    public static string GetCurrentFileID()
    {
        return Smithbox.EditorHandler.EmevdEditor._selectedFileInfo.Name;
    }

    public static string GetCurrentEventID()
    {
        return Smithbox.EditorHandler.EmevdEditor._selectedEvent.ID.ToString();
    }

    public static (List<ArgDoc>, List<object>) BuildArgumentList(Instruction ins)
    {
        var argList = new List<object>();
        var argDocList = new List<ArgDoc>();

        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if (classDoc == null)
        {
            return (argDocList, argList);
        }

        var instructionDoc = classDoc[ins.ID];

        if (instructionDoc == null)
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

    public static bool HasArgDoc(Instruction ins)
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


    public static string DetermineUnknownParameters(Instruction instruction, bool display = true)
    {
        var lineOuput = "";

        // Display the byte contents as blocks of 4 bytes
        // Also show the potential int and float values
        byte[] blockArr = new byte[4];
        int blockIndex = 0;

        for (int i = 0; i < instruction.ArgData.Length; i++)
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
                foreach (var item in blockArr)
                {
                    if (item < 10)
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

                var line = $"{arrStr} | Int: {iValue} | Float: {fValue} | Shorts: {sValue_1}, {sValue_2}";
                if (display)
                {
                    UIHelper.WrappedText(line);
                }
                else
                {
                    lineOuput = $"{lineOuput}\n{line}";
                }

                blockIndex = 0;
                blockArr = new byte[4];
            }
        }

        return lineOuput;
    }
}
