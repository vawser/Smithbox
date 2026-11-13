using System;
using System.IO;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Writes a Wwise PCM .wem from 16-bit PCM (write plan D2a). The layout follows the shipped Vorbis/Opus wems:
/// <c>RIFF, WAVE, "fmt " (0x18 bytes), "data"</c>, with the fmt in Wwise's short extensible form:
/// <c>u16 0xFFFE, u16 channels, u32 rate, u32 bytes/s, u16 block align, u16 bits (16), u16 cbSize (6),
/// u16 valid bits (16), u32 AkChannelConfig</c>. AkChannelConfig = <c>count | type 1 &lt;&lt; 8 | speaker mask &lt;&lt; 12</c>,
/// as every shipped wem has it at fmt+0x14 (SoundSurvey replacecheck: mono 0x4101, stereo 0x3102, 5.1 0x60F106).
/// The matching HIRC plug-in ID is <see cref="PcmPluginId"/>.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public static class WemWriter
{
    /// <summary>Wwise PCM codec (CODECID 1, type codec 1).</summary>
    public const uint PcmPluginId = 0x00010001;

    /// <summary>Speaker masks of the layouts shipped wems use, by channel count.</summary>
    public static uint SpeakerMask(int channels) => channels switch
    {
        1 => 0x4,      // centre
        2 => 0x3,      // L R
        4 => 0x603,    // L R, back L R
        6 => 0x60F,    // 5.1
        8 => 0x63F,    // 7.1
        _ => 0,
    };

    public static uint ChannelConfig(int channels) => (uint)channels | 1u << 8 | SpeakerMask(channels) << 12;

    /// <summary>A PCM wem holding <paramref name="pcm16"/> (interleaved little-endian 16-bit samples).</summary>
    public static byte[] WritePcm(ReadOnlySpan<byte> pcm16, int channels, int sampleRate)
    {
        if (channels is < 1 or > 8 || SpeakerMask(channels) == 0)
            throw new ArgumentException($"{channels} channels: only 1, 2, 4, 6 or 8 are supported");
        if (sampleRate is < 1000 or > 192000)
            throw new ArgumentException($"sample rate {sampleRate}");

        int block = channels * 2;
        int data = pcm16.Length - pcm16.Length % block;

        var ms = new MemoryStream(44 + data + 1);
        var w = new BinaryWriter(ms, Encoding.ASCII);
        w.Write("RIFF"u8);
        w.Write(4 + 8 + 0x18 + 8 + data + (data & 1));
        w.Write("WAVE"u8);

        w.Write("fmt "u8);
        w.Write(0x18);
        w.Write((ushort)0xFFFE);
        w.Write((ushort)channels);
        w.Write(sampleRate);
        w.Write(sampleRate * block);
        w.Write((ushort)block);
        w.Write((ushort)16);
        w.Write((ushort)6);
        w.Write((ushort)16);
        w.Write(ChannelConfig(channels));

        w.Write("data"u8);
        w.Write(data);
        w.Write(pcm16[..data]);
        if ((data & 1) != 0) w.Write((byte)0);
        return ms.ToArray();
    }

    /// <summary>
    /// The 16-bit PCM of a WAV (as vgmstream writes them, or any plain PCM16 WAV): interleaved samples, channels, rate.
    /// Throws for other WAV formats; convert those with vgmstream first (<see cref="VgmstreamCli.DecodeToFile"/>).
    /// </summary>
    public static (byte[] Pcm, int Channels, int SampleRate) ReadPcm16Wav(byte[] wav)
    {
        using var s = new MemoryStream(wav);
        var h = WavHeader.Read(s) ?? throw new InvalidDataException("Not a RIFF/WAVE file");
        if (h.BitsPerSample != 16 || h.FormatTag is not (1 or 0xFFFE))
            throw new InvalidDataException($"WAV is {h.BitsPerSample}-bit format 0x{h.FormatTag:X4}, not 16-bit PCM");

        long available = wav.Length - s.Position;
        long len = h.DataLength > 0 && h.DataLength <= available ? h.DataLength : available;
        var pcm = new byte[len];
        s.ReadExactly(pcm);
        return (pcm, h.Channels, h.SampleRate);
    }

    /// <summary>Is this a wem file (RIFF WAVE with a Wwise codec), as opposed to a plain WAV?</summary>
    public static bool IsWem(byte[] d)
    {
        var h = WemHeader.Read(d);
        return h != null && h.FormatTag is 0xFFFF or 0x3040 or 0x3041 or 0x0002 or 0x8311 or 0xFFFE &&
               (h.FormatTag != 0xFFFE || (h.FmtSize == 0x18 && d.Length > h.FmtBody + 0x18 &&
                                          (BitConverter.ToUInt32(d, h.FmtBody + 0x14) & 0xFF) == h.Channels));
    }

    /// <summary>HIRC plug-in ID for a wem's codec, from its fmt tag; 0 if unknown.</summary>
    public static uint PluginIdFor(byte[] wem)
    {
        var h = WemHeader.Read(wem);
        return h?.FormatTag switch
        {
            0xFFFF => 0x00040001,                     // Vorbis
            0x3040 or 0x3041 => 0x00140001,           // Opus (DB_NT)
            0x0002 => 0x00020001,                     // ADPCM
            0xFFFE or 0x0001 => PcmPluginId,
            _ => 0,
        };
    }
}
