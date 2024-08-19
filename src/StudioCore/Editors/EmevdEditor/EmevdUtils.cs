using SoulsFormats;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        AliasUtils.DisplayAlias(alias);

        if (CFG.Current.EmevdEditor_DisplayInstructionParameterNames)
        {
            AliasUtils.DisplayColoredAlias($"({argsStr})", CFG.Current.ImGui_Benefit_Text_Color);
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
}
