using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Read-only FMOD sample bank: FSB5 (DS2, DS2S, BB, DS3, SDT) and FSB4 (DS1, DS1R, DS2, BB). Header, sample table
/// and names only; the audio is left to vgmstream. Encrypted banks (the DS3/SDT music banks) are decrypted with the
/// game's key first. See Claude/Documentation/Sound Editor Plan.md §2a.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class FsbFile
{
    public sealed class Sample
    {
        public int Index;
        public string Name;
        public int SampleRate;
        public int Channels;
        public long SampleCount;
        public long LoopStart = -1;
        public long LoopEnd = -1;
        public string Codec;

        /// <summary>Bytes of audio data; 0 for placeholders like DS2's c7310blank.wav (which still states a length).</summary>
        public long DataSize = -1;
        public bool HasData => DataSize != 0 && SampleCount > 0;

        public double Seconds => SampleRate > 0 ? (double)SampleCount / SampleRate : 0;
    }

    /// <summary>"FSB5" or "FSB4"</summary>
    public string Version;
    public string Codec;
    public bool WasEncrypted;
    public List<Sample> Samples = new();

    /// <summary>The (decrypted) bytes, the form vgmstream is handed. Same array as the input if not encrypted.</summary>
    public byte[] Bytes;

    // FMOD FSB encryption keys of the games that use it (bit-reversed bytes XOR key). Checked by SoundSurvey.
    private static readonly string[] Keys =
    {
        "FDPrVuT4fAFvdHJYAgyMzRF4EcBAnKg", // DS3
        "G0KTrWjS9syqF7vVD6RaVXlFD91gMgkC", // SDT
    };

    private static readonly string[] Fsb5Codecs = { "None", "PCM8", "PCM16", "PCM24", "PCM32", "PCM Float", "GC ADPCM", "IMA ADPCM",
        "VAG", "HEVAG", "XMA", "MPEG", "CELT", "ATRAC9", "XWMA", "Vorbis", "FADPCM", "Opus" };
    private static readonly int[] Fsb5Rates = { 4000, 8000, 11000, 11025, 16000, 22050, 24000, 32000, 44100, 48000, 96000 };

    public static bool LooksLikeFsb(byte[] d) => d.Length >= 4 && d[0] == 'F' && d[1] == 'S' && d[2] == 'B' && (d[3] == '4' || d[3] == '5');

    /// <summary>Reads a bank. Throws on data that is neither FSB4/5 nor decryptable with a known key.</summary>
    public static FsbFile Read(byte[] data)
    {
        var bytes = data;
        bool encrypted = false;
        if (!LooksLikeFsb(bytes))
        {
            bytes = TryDecrypt(data) ?? throw new InvalidOperationException("Not an FSB4/FSB5 bank, and no known key decrypts it.");
            encrypted = true;
        }

        var f = new FsbFile { Bytes = bytes, WasEncrypted = encrypted };
        if (bytes[3] == '5') f.ReadFsb5(bytes);
        else f.ReadFsb4(bytes);
        return f;
    }

    /// <summary>Decrypts with each known key; null if none gives an FSB header.</summary>
    public static byte[] TryDecrypt(byte[] data)
    {
        if (data.Length < 4) return null;
        foreach (var key in Keys)
        {
            // check the magic before decrypting the whole bank
            var head = new byte[4];
            for (int i = 0; i < 4; i++)
                head[i] = (byte)(Reverse(data[i]) ^ key[i % key.Length]);
            if (!LooksLikeFsb(head)) continue;

            var outp = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                outp[i] = (byte)(Reverse(data[i]) ^ key[i % key.Length]);
            return outp;
        }
        return null;
    }

    private static byte Reverse(byte b)
    {
        b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
        b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
        b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
        return b;
    }

    private void ReadFsb5(byte[] d)
    {
        Version = "FSB5";
        int ver = BitConverter.ToInt32(d, 4), num = BitConverter.ToInt32(d, 8), shdr = BitConverter.ToInt32(d, 12),
            names = BitConverter.ToInt32(d, 16), dataSize = BitConverter.ToInt32(d, 20), mode = BitConverter.ToInt32(d, 24);
        var offsets = new List<long>();
        int hdr = ver == 0 ? 0x40 : 0x3C;
        Codec = mode >= 0 && mode < Fsb5Codecs.Length ? Fsb5Codecs[mode] : $"Codec {mode}";

        int pos = hdr;
        for (int i = 0; i < num; i++)
        {
            ulong v = BitConverter.ToUInt64(d, pos); pos += 8;
            int rateIndex = (int)((v >> 1) & 0xF);
            var s = new Sample
            {
                Index = i,
                Codec = Codec,
                SampleRate = rateIndex < Fsb5Rates.Length ? Fsb5Rates[rateIndex] : 0,
                Channels = new[] { 1, 2, 6, 8 }[(int)((v >> 5) & 3)],
                SampleCount = (long)((v >> 34) & 0x3FFFFFFF),
            };
            offsets.Add((long)((v >> 7) & 0x7FFFFFF) << 5);
            bool next = (v & 1) != 0;
            while (next)
            {
                uint c = BitConverter.ToUInt32(d, pos); pos += 4;
                next = (c & 1) != 0;
                int size = (int)((c >> 1) & 0xFFFFFF), type = (int)(c >> 25);
                switch (type)
                {
                    case 1: s.Channels = d[pos]; break;                                  // channels
                    case 2: s.SampleRate = BitConverter.ToInt32(d, pos); break;          // frequency
                    case 3: s.LoopStart = BitConverter.ToUInt32(d, pos);                 // loop
                            s.LoopEnd = size >= 8 ? BitConverter.ToUInt32(d, pos + 4) : -1; break;
                }
                pos += size;
            }
            Samples.Add(s);
        }

        // each sample's data runs to the next sample's offset (the last to the end of the data block)
        for (int i = 0; i < num; i++)
            Samples[i].DataSize = (i + 1 < num ? offsets[i + 1] : dataSize) - offsets[i];

        if (names > 0)
        {
            int nt = hdr + shdr;
            for (int i = 0; i < num; i++)
            {
                int s = nt + BitConverter.ToInt32(d, nt + i * 4), e = s;
                while (e < d.Length && d[e] != 0) e++;
                Samples[i].Name = Encoding.ASCII.GetString(d, s, e - s);
            }
        }
    }

    private void ReadFsb4(byte[] d)
    {
        Version = "FSB4";
        int num = BitConverter.ToInt32(d, 4), headMode = BitConverter.ToInt32(d, 20);
        int pos = 0x30;
        for (int i = 0; i < num; i++)
        {
            // full sample header: u16 size, char[30] name, u32 length (samples), u32 compressed length, u32 loop start,
            // u32 loop end, u32 mode, i32 frequency, u16 vol, i16 pan, u16 pri, u16 channels, ...
            int size = BitConverter.ToUInt16(d, pos);
            uint smode = BitConverter.ToUInt32(d, pos + 0x30);
            var s = new Sample
            {
                Index = i,
                Name = Encoding.ASCII.GetString(d, pos + 2, 30).TrimEnd('\0'),
                SampleCount = BitConverter.ToUInt32(d, pos + 0x20),
                DataSize = BitConverter.ToUInt32(d, pos + 0x24),
                LoopStart = BitConverter.ToUInt32(d, pos + 0x28),
                LoopEnd = BitConverter.ToUInt32(d, pos + 0x2C),
                SampleRate = BitConverter.ToInt32(d, pos + 0x34),
                Channels = BitConverter.ToUInt16(d, pos + 0x3E),
                Codec = Fsb4Codec(smode),
            };
            if (s.LoopStart == 0 && s.LoopEnd == 0) s.LoopStart = s.LoopEnd = -1;
            Samples.Add(s);

            // basic headers (headMode bit 1): every sample after the first is 8 bytes (length, compressed length)
            if ((headMode & 2) != 0 && i == 0)
            {
                pos += size;
                for (int j = 1; j < num; j++)
                {
                    Samples.Add(new Sample { Index = j, Name = $"{s.Name} #{j}", SampleCount = BitConverter.ToUInt32(d, pos),
                        SampleRate = s.SampleRate, Channels = s.Channels, Codec = s.Codec });
                    pos += 8;
                }
                break;
            }
            pos += size;
        }
        Codec = Samples.Count > 0 ? Samples[0].Codec : "";
    }

    // FSOUND_* mode flags: 0x200 MPEG, 0x400000 IMA ADPCM, 0x1000000 XMA, 0x2000000 GC ADPCM, 0x10 8-bit, else PCM16
    private static string Fsb4Codec(uint mode)
    {
        if ((mode & 0x200) != 0) return "MPEG";
        if ((mode & 0x400000) != 0) return "IMA ADPCM";
        if ((mode & 0x1000000) != 0) return "XMA";
        if ((mode & 0x2000000) != 0) return "GC ADPCM";
        if ((mode & 0x10) != 0) return "PCM8";
        return "PCM16";
    }
}
