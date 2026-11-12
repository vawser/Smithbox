using System;
using System.IO;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The format of a PCM RIFF/WAVE stream, read from its header up to the start of the "data" chunk, so the rest of the
/// stream is raw samples. Works on a non-seekable stream (vgmstream's stdout).
/// </summary>
public sealed class WavHeader
{
    public int SampleRate;
    public int Channels;
    public int BitsPerSample;
    public int FormatTag;

    /// <summary>Byte length of the data chunk as the header states it; may be 0 or wrong for piped output.</summary>
    public uint DataLength;

    public int BlockAlign => Channels * BitsPerSample / 8;

    /// <summary>Reads the header and leaves <paramref name="s"/> at the first sample. Null if it isn't a WAVE.</summary>
    public static WavHeader Read(Stream s)
    {
        var head = new byte[12];
        if (!ReadExactly(s, head, 12)) return null;
        if (Encoding.ASCII.GetString(head, 0, 4) != "RIFF" || Encoding.ASCII.GetString(head, 8, 4) != "WAVE")
            return null;

        var h = new WavHeader();
        var chunk = new byte[8];
        while (ReadExactly(s, chunk, 8))
        {
            var id = Encoding.ASCII.GetString(chunk, 0, 4);
            var size = BitConverter.ToUInt32(chunk, 4);
            if (id == "data")
            {
                h.DataLength = size;
                return h.Channels > 0 ? h : null;
            }

            var body = new byte[size + (size & 1)];
            if (!ReadExactly(s, body, body.Length)) return null;
            if (id == "fmt " && size >= 16)
            {
                h.FormatTag = BitConverter.ToUInt16(body, 0);
                h.Channels = BitConverter.ToUInt16(body, 2);
                h.SampleRate = BitConverter.ToInt32(body, 4);
                h.BitsPerSample = BitConverter.ToUInt16(body, 14);
            }
        }
        return null;
    }

    private static bool ReadExactly(Stream s, byte[] buf, int count)
    {
        int read = 0;
        while (read < count)
        {
            int n = s.Read(buf, read, count - read);
            if (n <= 0) return false;
            read += n;
        }
        return true;
    }
}
