using System;
using System.Buffers.Binary;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The RIFF header of a .wem: codec, rate, channels and an approximate length (data size / average byte rate).
/// The exact length comes from vgmstream when the sound is selected.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class WemHeader
{
    public int FormatTag;
    public int Channels;
    public int SampleRate;
    public int AvgBytesPerSec;
    public int DataSize;

    /// <summary>Offset of the fmt chunk's body from the start of the wem, and its size.</summary>
    public int FmtBody, FmtSize;

    /// <summary>
    /// The full sound's sample count from the fmt extension (u32 at fmt + 0x18: Wwise Vorbis fmt 0x42 in ER/NR/AC6,
    /// Opus fmt 0x24 in DB_NT; checked against vgmstream by SoundSurvey listing), or -1. For a prefetched sound the
    /// bank holds only the start, but this is still the whole length.
    /// </summary>
    public long SampleCount = -1;

    public double ApproxSeconds => AvgBytesPerSec > 0 ? (double)DataSize / AvgBytesPerSec : 0;

    /// <summary>Exact length if the header has a sample count, else the approximation.</summary>
    public double Seconds => SampleCount > 0 && SampleRate > 0 ? (double)SampleCount / SampleRate : ApproxSeconds;

    public bool SecondsExact => SampleCount > 0 && SampleRate > 0;

    public string Codec => FormatTag switch
    {
        0xFFFF => "Vorbis",
        0x3040 or 0x3041 => "Opus",
        0x0002 => "ADPCM",
        0x8311 => "Platinum ADPCM",
        0x0001 or 0xFFFE => "PCM",
        0x0166 => "XMA2",
        _ => $"0x{FormatTag:X4}",
    };

    /// <summary>Reads from the start of a wem (only the header is needed). Null if it isn't RIFF/RIFX.</summary>
    public static WemHeader Read(ReadOnlySpan<byte> d)
    {
        if (d.Length < 12) return null;
        bool be;
        if (d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F') be = false;
        else if (d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'X') be = true;
        else return null;

        var h = new WemHeader();
        bool gotFmt = false;
        int pos = 12;
        while (pos + 8 <= d.Length)
        {
            var id = Encoding.ASCII.GetString(d.Slice(pos, 4));
            int size = (int)U32(d, pos + 4, be);
            if (id == "fmt " && pos + 24 <= d.Length)
            {
                h.FormatTag = U16(d, pos + 8, be);
                h.Channels = U16(d, pos + 10, be);
                h.SampleRate = (int)U32(d, pos + 12, be);
                h.AvgBytesPerSec = (int)U32(d, pos + 16, be);
                h.FmtBody = pos + 8;
                h.FmtSize = size;
                if (h.FormatTag is 0xFFFF or 0x3040 or 0x3041 && size >= 0x1C && h.FmtBody + 0x1C <= d.Length)
                    h.SampleCount = U32(d, h.FmtBody + 0x18, be);
                gotFmt = true;
            }
            else if (id == "data")
            {
                h.DataSize = size;
                break;
            }
            if (size < 0) break;
            pos += 8 + size + (size & 1);
        }
        return gotFmt ? h : null;
    }

    private static int U16(ReadOnlySpan<byte> d, int o, bool be) => be ? BinaryPrimitives.ReadUInt16BigEndian(d[o..]) : BinaryPrimitives.ReadUInt16LittleEndian(d[o..]);
    private static uint U32(ReadOnlySpan<byte> d, int o, bool be) => be ? BinaryPrimitives.ReadUInt32BigEndian(d[o..]) : BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
}
