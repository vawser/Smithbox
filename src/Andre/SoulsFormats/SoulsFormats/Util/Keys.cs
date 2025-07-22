using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Util;

public class Keys
{
    /// <summary>
    /// Save Keys
    /// </summary>
    public static readonly byte[] DS2_SAVE_KEY = ParseHexString("B7 FD 46 3E 4A 9C 11 02 DF 17 39 E5 F3 B2 A5 0F");

    public static readonly byte[] DS2S_SAVE_KEY = ParseHexString("59 9F 9B 69 96 40 A5 52 36 EE 2D 70 83 5E C7 44");

    public static readonly byte[] DS3_SAVE_KEY = ParseHexString("FD 46 4D 69 5E 69 A3 9A 10 E3 19 A7 AC E8 B7 FA");

    public static readonly byte[] NR_SAVE_KEY = ParseHexString("18 F6 32 66 05 BD 17 8A 55 24 52 3A C0 A0 C6 09");

    /// <summary>
    /// Regulation Keys
    /// </summary>
    public static readonly byte[] DS2_REGULATION_KEY = { 0x40, 0x17, 0x81, 0x30, 0xDF, 0x0A, 0x94, 0x54, 0x33, 0x09, 0xE1, 0x71, 0xEC, 0xBF, 0x25, 0x4C };

    public static readonly byte[] DS3_REGULATION_KEY = SFEncoding.ASCII.GetBytes("ds3#jn/8_7(rsY9pg55GFN7VFL#+3n/)");

    public static readonly byte[] ER_REGULATION_KEY = ParseHexString("99 BF FC 36 6A 6B C8 C6 F5 82 7D 09 36 02 D6 76 C4 28 92 A0 1C 20 7F B0 24 D3 AF 4E 49 3F EF 99");

    public static readonly byte[] AC6_REGULATION_KEY = ParseHexString("10 CE ED 47 7B 7C D9 D7 E6 93 8E 11 47 13 E7 87 D5 39 13 B1 D 31 8E C1 35 E4 BE 50 50 4E E 10");

    public static readonly byte[] NR_REGULATION_KEY = ParseHexString("9a 8e e9 0c 4c 01 a4 31 68 a1 7d 9d 75 e4 a7 d0 21 07 eb cf 43 d5 ac b0 55 4f 94 16 01 b5 79 18");

    /// <summary>
    /// Converts a hex string in format "AA BB CC DD" to a byte array.
    /// </summary>
    public static byte[] ParseHexString(string str)
    {
        string[] strings = str.Split(' ');
        byte[] bytes = new byte[strings.Length];
        for (int i = 0; i < strings.Length; i++)
            bytes[i] = Convert.ToByte(strings[i], 16);
        return bytes;
    }
}
