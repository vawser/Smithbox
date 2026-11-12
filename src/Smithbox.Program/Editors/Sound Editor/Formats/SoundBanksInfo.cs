using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// FromSoftware's soundbanksinfo.mobnkinfo (ER, NR, AC6, DB_NT): which Wwise bank holds which events, so a sound ID can
/// be found without opening every bank.
/// <code>
/// "SBI\0", u32 version, u32 header size (0x18), u32 0, u32 section count, u32 0
/// per section (one per language: SFX, enus, ja...): 16-byte key, u64 bank count, u64 offset
/// per bank: u32 bank ID (FNV-1 of its name), u32 event count, u64 offset
/// per event: u32 event ID, u32 flag (1 on looping Play_ events)
/// </code>
/// Checked against every bank's HIRC events by SoundSurvey index.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class SoundBanksInfo
{
    /// <summary>Event ID → the IDs of the banks holding it (a few events are in several banks).</summary>
    public Dictionary<uint, List<uint>> EventBanks = new();

    public int SectionCount, BankCount, EventCount;

    public static SoundBanksInfo Read(byte[] d)
    {
        if (d.Length < 0x18 || Encoding.ASCII.GetString(d, 0, 3) != "SBI")
            throw new FormatException("Not a soundbanksinfo (SBI) file");

        var info = new SoundBanksInfo { SectionCount = BitConverter.ToInt32(d, 0x10) };
        for (int s = 0; s < info.SectionCount; s++)
        {
            int sec = 0x18 + s * 0x20;
            long banks = BitConverter.ToInt64(d, sec + 0x10), offset = BitConverter.ToInt64(d, sec + 0x18);
            for (long b = 0; b < banks; b++)
            {
                int e = (int)offset + (int)b * 16;
                uint bankId = BitConverter.ToUInt32(d, e);
                int count = BitConverter.ToInt32(d, e + 4);
                long evOffset = BitConverter.ToInt64(d, e + 8);
                info.BankCount++;
                for (int k = 0; k < count; k++)
                {
                    uint eventId = BitConverter.ToUInt32(d, (int)evOffset + k * 8);
                    if (!info.EventBanks.TryGetValue(eventId, out var list))
                        info.EventBanks[eventId] = list = new List<uint>(1);
                    if (!list.Contains(bankId)) list.Add(bankId);
                    info.EventCount++;
                }
            }
        }
        return info;
    }

    /// <summary>The FMOD Designer STRR string table of a RIFF FEV (event names, definition names), without parsing LGCY.</summary>
    public static List<string> FevStrings(byte[] d)
    {
        var r = new List<string>();
        if (d.Length < 12 || Encoding.ASCII.GetString(d, 0, 4) != "RIFF")
            return r;

        int strr = -1, size = 0;
        void Walk(int p, int end)
        {
            while (p + 8 <= end)
            {
                var id = Encoding.ASCII.GetString(d, p, 4);
                int s = BitConverter.ToInt32(d, p + 4);
                if (s < 0 || p + 8 + s > end) return;
                if (id == "LIST") Walk(p + 12, p + 8 + s);
                else if (id == "STRR") { strr = p + 8; size = s; }
                p += 8 + s + (s & 1);
            }
        }
        Walk(12, d.Length);
        if (strr < 0)
            return r;

        int count = BitConverter.ToInt32(d, strr);
        for (int i = 0; i < count; i++)
        {
            int start = strr + 4 + count * 4 + BitConverter.ToInt32(d, strr + 4 + i * 4), e = start;
            while (e < strr + size && d[e] != 0) e++;
            r.Add(Encoding.ASCII.GetString(d, start, e - start));
        }
        return r;
    }
}
