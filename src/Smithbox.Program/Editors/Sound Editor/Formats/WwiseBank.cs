using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Read-only Wwise SoundBank (BKHD v135 in ER/NR/AC6, v150 in DB_NT): header, in-bank media (DIDX/DATA), Sound
/// objects (which media each plays and whether it is streamed), event IDs and STID bank names. The HIRC event graph
/// is S4. See Claude/Documentation/Sound Editor Plan.md §2a.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class WwiseBank
{
    /// <summary>A wem stored in the bank: <see cref="Offset"/> is from the start of the file.</summary>
    public readonly record struct Media(uint Id, int Offset, int Size);

    /// <summary>A HIRC Sound object.</summary>
    public readonly record struct Sound(uint Id, uint PluginId, byte StreamType, uint SourceId, uint InMemorySize)
    {
        /// <summary>Stream type 0 = data in this bank's DATA; 1 = prefetch + streamed; 2 = streamed.</summary>
        public bool IsStreamed => StreamType != 0;

        /// <summary>Wwise Sine / Silence / Tone Generator: no media.</summary>
        public bool IsGenerated => (PluginId & 0xFFFF) == 2;
    }

    public uint Version;
    public uint BankId;
    public List<Media> InBankMedia = new();
    public List<Sound> Sounds = new();
    public List<uint> EventIds = new();
    public Dictionary<uint, string> BankNames = new();

    public static WwiseBank Read(byte[] d)
    {
        var b = new WwiseBank();
        var didx = new List<(uint Id, int Off, int Size)>();
        int dataStart = -1, pos = 0;

        while (pos + 8 <= d.Length)
        {
            // about half the banks end in zero padding
            if (d[pos] == 0 && d[pos + 1] == 0 && d[pos + 2] == 0 && d[pos + 3] == 0)
                break;

            var id = Encoding.ASCII.GetString(d, pos, 4);
            int size = BitConverter.ToInt32(d, pos + 4);
            int body = pos + 8;
            if (size < 0 || body + size > d.Length)
                throw new InvalidOperationException($"Bad chunk '{id}' at 0x{pos:X}");

            switch (id)
            {
                case "BKHD":
                    b.Version = BitConverter.ToUInt32(d, body);
                    b.BankId = BitConverter.ToUInt32(d, body + 4);
                    break;
                case "DIDX":
                    for (int i = 0; i < size / 12; i++)
                        didx.Add((BitConverter.ToUInt32(d, body + i * 12), BitConverter.ToInt32(d, body + i * 12 + 4), BitConverter.ToInt32(d, body + i * 12 + 8)));
                    break;
                case "DATA":
                    dataStart = body;
                    break;
                case "HIRC":
                    b.ReadHirc(d, body);
                    break;
                case "STID":
                    {
                        int count = BitConverter.ToInt32(d, body + 4), p = body + 8;
                        for (int i = 0; i < count; i++)
                        {
                            uint sid = BitConverter.ToUInt32(d, p);
                            int len = d[p + 4];
                            b.BankNames[sid] = Encoding.ASCII.GetString(d, p + 5, len);
                            p += 5 + len;
                        }
                    }
                    break;
            }
            pos = body + size;
        }

        if (dataStart >= 0)
            foreach (var (id, off, size) in didx)
                b.InBankMedia.Add(new Media(id, dataStart + off, size));

        return b;
    }

    private void ReadHirc(byte[] d, int pos)
    {
        int count = BitConverter.ToInt32(d, pos);
        pos += 4;
        for (int i = 0; i < count; i++)
        {
            int type = d[pos];
            int size = BitConverter.ToInt32(d, pos + 1);
            int body = pos + 5;
            uint id = BitConverter.ToUInt32(d, body);

            if (type == 4)
                EventIds.Add(id);
            else if (type == 2)
                // CAkSound: ulID, AkBankSourceData { ulPluginID, StreamType u8, sourceID, uInMemoryMediaSize, ... }
                Sounds.Add(new Sound(id, BitConverter.ToUInt32(d, body + 4), d[body + 8], BitConverter.ToUInt32(d, body + 9), BitConverter.ToUInt32(d, body + 13)));

            pos = body + size;
        }
    }

    /// <summary>Codec named by a Sound object's plugin ID.</summary>
    public static string PluginName(uint plugin) => plugin switch
    {
        0x00010001 => "PCM",
        0x00020001 => "ADPCM",
        0x00040001 => "Vorbis",
        0x00130001 or 0x00140001 => "Opus",
        0x00640002 => "Sine (generated)",
        0x00650002 => "Silence (generated)",
        0x00660002 => "Tone (generated)",
        _ => $"Plugin 0x{plugin:X8}",
    };

    /// <summary>FNV-1 32 of the lowercased name: Wwise's ID for banks, events and other named objects.</summary>
    public static uint Hash(string name)
    {
        uint h = 2166136261;
        foreach (var c in name.ToLowerInvariant())
        {
            h *= 16777619;
            h ^= (byte)c;
        }
        return h;
    }
}
