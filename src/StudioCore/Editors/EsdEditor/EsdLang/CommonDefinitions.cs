using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor.EsdLang;

public static class CommonDefinitions
{
    public static Dictionary<byte, string> OperatorsByByte = new Dictionary<byte, string>
    {
        [0x8C] = "+",
        [0x8D] = "N",
        [0x8E] = "-",
        [0x8F] = "*",
        [0x90] = "/",
        [0x91] = "<=",
        [0x92] = ">=",
        [0x93] = "<",
        [0x94] = ">",
        [0x95] = "==",
        [0x96] = "!=",
        [0x98] = "&&",
        [0x99] = "||",
    };

    public static Dictionary<string, byte> BytesByOperator = OperatorsByByte.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static Dictionary<byte, string> TerminatorsByByte = new Dictionary<byte, string>
    {
        [0xA6] = "~",
        [0xB7] = ".",
    };

    public static Dictionary<char, byte> BytesByTerminator = TerminatorsByByte.ToDictionary(kvp => kvp.Value[0], kvp => kvp.Key);
}
