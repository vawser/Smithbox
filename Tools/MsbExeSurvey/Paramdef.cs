using SoulsFormats;

/// <summary>
/// Byte offsets of a paramdef's fields, laid out as Andre.Formats lays them out, so a row offset the exe reads can be named
/// </summary>
public static class Paramdef
{
    public static List<(uint Off, int Bit, string Type, string Name, string Display)> Offsets(string xml)
    {
        var def = PARAMDEF.XmlDeserialize(xml);
        var result = new List<(uint, int, string, string, string)>();
        int bitOffset = -1;
        uint byteOffset = 0, lastSize = 0;
        var bitType = PARAMDEF.DefType.u8;

        uint Size(PARAMDEF.DefType t, int arr) => t switch
        {
            PARAMDEF.DefType.s8 or PARAMDEF.DefType.u8 => 1,
            PARAMDEF.DefType.s16 or PARAMDEF.DefType.u16 => 2,
            PARAMDEF.DefType.f64 => 8,
            PARAMDEF.DefType.fixstr or PARAMDEF.DefType.dummy8 => (uint)arr,
            PARAMDEF.DefType.fixstrW => (uint)arr * 2,
            _ => 4,
        };
        int Limit(PARAMDEF.DefType t) => (int)Size(t, 1) * 8;

        foreach (var f in def.Fields)
        {
            var t = f.DisplayType;
            var isBit = t is PARAMDEF.DefType.u8 or PARAMDEF.DefType.u16 or PARAMDEF.DefType.u32 or PARAMDEF.DefType.dummy8;
            if (!isBit || f.BitSize == -1)
            {
                if (bitOffset != -1) byteOffset += lastSize;
                result.Add((byteOffset, -1, t.ToString(), f.InternalName, f.DisplayName));
                byteOffset += Size(t, f.ArrayLength);
                bitOffset = -1;
            }
            else
            {
                var nt = t == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : t;
                lastSize = Size(nt, 1);
                if (bitOffset == -1 || nt != bitType || bitOffset + f.BitSize > Limit(nt))
                {
                    if (bitOffset != -1) byteOffset += lastSize;
                    bitOffset = 0;
                    bitType = nt;
                }
                result.Add((byteOffset, bitOffset, t.ToString(), f.InternalName, f.DisplayName));
                bitOffset += f.BitSize;
            }
        }
        return result;
    }
}
