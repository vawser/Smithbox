using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

// Known credits for original research used for HKS reloading:
// - horkrux
// - Meowmaritus
public static class HavokMemoryConsts
{
    public static string EldenRing_WorldChrManPtr_AOB = "48 8B 05 ?? ?? ?? ?? 48 85 C0 74 0F 48 39 88";

    public static int EldenRing_WorldChrManPtr_JumpInstr_StartOffsetInAOB = 3;

    public static int EldenRing_WorldChrManPtr_JumpInstr_EndOffsetInAOB = 7;

    public static string EldenRing_WorldChrManStructOffset = "0x1E668";

    public static string EldenRing_CrashPatchOffset_AOB = "80 65 ?? FD 48 C7 45 ?? 07 00 00 00 ?? 8D 45 48 4C 89 60 ?? 48 83 78 ?? 08 72 03 48 8B 00 66 44 89 20 49 8B 8F ?? ?? ?? ?? 48 8B 01 48 ?? ??";

    public static int EldenRing_CrashPatchOffset_DistFromEndOfAOB = 3;

    public static string EldenRing_CrashPatchOffset_WriteBytes = "48 31 D2";

    public static int? GetHexFromString(string str)
    {
        int? result = null;

        int.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out int asInt);
        result = asInt;

        return result;
    }

    public static byte[] GetByteArrayFromString(string str)
    {
        byte[] result = null;
        result = str.Trim().Split(' ').Select(x => (byte)int.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();

        return result;
    }
}
