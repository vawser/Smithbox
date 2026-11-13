using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

/// <summary>A media ID's new audio: the whole new .wem and the HIRC plug-in ID of its codec.</summary>
public sealed class WwiseMediaReplacement
{
    public byte[] Wem;
    public uint PluginId;

    /// <summary>
    /// Size of the media's old streamed .wem, or 0 if it has none. An in-bank copy smaller than that is a prefetch
    /// prefix even in a bank with no prefetch object of its own (the object is in another bank: SoundSurvey replacecheck
    /// finds 194 such copies in ER, 145 NR, 49 AC6, 51 DB_NT).
    /// </summary>
    public int OldStreamedSize;
}

/// <summary>What <see cref="WwiseBankWriter.Write"/> changed in one bank.</summary>
public sealed class WwiseBankWriteResult
{
    public byte[] Bytes;

    /// <summary>In-bank copies replaced: whole wems, and prefetch prefixes.</summary>
    public int MediaReplaced, PrefixesReplaced;

    /// <summary>HIRC sources patched (Sound objects + music track sources).</summary>
    public int SourcesPatched;

    public bool Changed => MediaReplaced + PrefixesReplaced + SourcesPatched > 0;
}

/// <summary>
/// Rewrites a Wwise bank with some media replaced (write plan W1). The layout is the one SoundSurvey writecheck
/// reproduces byte for byte on every bank of ER, NR, AC6 and DB_NT:
/// <list type="bullet">
/// <item>chunks are copied in order, the zero tail after the last chunk is kept;</item>
/// <item>DATA holds the media in DIDX order, each at the next 16-byte boundary with zero padding; DIDX gets the new
/// offsets and sizes.</item>
/// </list>
/// HIRC sources (<c>AkBankSourceData</c> in Sound objects and music tracks) of a replaced media get the new plug-in ID
/// and in-memory size, by stream type (SoundSurvey replacecheck):
/// <list type="bullet">
/// <item>0 (in memory): the new wem's size, also where the media sits in another bank;</item>
/// <item>1 (prefetch + stream): this bank keeps a byte prefix of the streamed wem (writecheck: always a prefix); the new
/// prefix is the new wem's first <c>old prefix size</c> bytes, and the size field is the prefix's size. A copy is also
/// treated as a prefix when it is smaller than <see cref="WwiseMediaReplacement.OldStreamedSize"/>;</item>
/// <item>2 (streamed): left as is. The field holds 1–4 KB whose meaning isn't known (not the header, nor a fmt offset).</item>
/// </list>
/// Music track durations (<c>fSrcDuration</c>) aren't updated yet (W3).
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public static class WwiseBankWriter
{
    private const int DataAlignment = 16;

    private readonly record struct Chunk(string Id, int Body, int Size);

    // a HIRC source: where its fields are in the original file
    private readonly record struct Source(int PluginAt, byte StreamType, uint SourceId, int SizeAt);

    public static WwiseBankWriteResult Write(byte[] d, IReadOnlyDictionary<uint, WwiseMediaReplacement> replace)
    {
        var result = new WwiseBankWriteResult();

        var chunks = new List<Chunk>();
        int pos = 0;
        while (pos + 8 <= d.Length && !(d[pos] == 0 && d[pos + 1] == 0 && d[pos + 2] == 0 && d[pos + 3] == 0))
        {
            int size = BitConverter.ToInt32(d, pos + 4);
            if (size < 0 || pos + 8 + size > d.Length)
                throw new InvalidDataException($"Bad chunk at 0x{pos:X}");
            chunks.Add(new Chunk(Encoding.ASCII.GetString(d, pos, 4), pos + 8, size));
            pos += 8 + size;
        }
        int tail = pos;

        // HIRC sources of replaced media, and which media this bank prefetches
        var sources = new List<Source>();
        var prefetched = new HashSet<uint>();
        foreach (var c in chunks)
            if (c.Id == "HIRC")
                ReadSources(d, c.Body, c.Body + c.Size, replace, sources);
        foreach (var s in sources)
            if (s.StreamType == 1) prefetched.Add(s.SourceId);

        // DIDX / DATA
        byte[] newDidx = null, newData = null;
        var newSizes = new Dictionary<uint, int>();
        var didxC = chunks.Find(c => c.Id == "DIDX");
        var dataC = chunks.Find(c => c.Id == "DATA");
        if (didxC.Id != null && dataC.Id != null)
        {
            var didx = new MemoryStream(didxC.Size);
            var data = new MemoryStream(dataC.Size);
            for (int i = 0; i < didxC.Size / 12; i++)
            {
                uint id = BitConverter.ToUInt32(d, didxC.Body + i * 12);
                int off = BitConverter.ToInt32(d, didxC.Body + i * 12 + 4), size = BitConverter.ToInt32(d, didxC.Body + i * 12 + 8);

                ReadOnlySpan<byte> bytes = d.AsSpan(dataC.Body + off, size);
                if (replace.TryGetValue(id, out var r))
                {
                    if (prefetched.Contains(id) || (r.OldStreamedSize > 0 && size < r.OldStreamedSize))
                    {
                        bytes = r.Wem.AsSpan(0, Math.Min(size, r.Wem.Length));
                        result.PrefixesReplaced++;
                    }
                    else
                    {
                        bytes = r.Wem;
                        result.MediaReplaced++;
                    }
                    newSizes[id] = bytes.Length;
                }

                while (data.Length % DataAlignment != 0) data.WriteByte(0);
                didx.Write(BitConverter.GetBytes(id));
                didx.Write(BitConverter.GetBytes((int)data.Length));
                didx.Write(BitConverter.GetBytes(bytes.Length));
                data.Write(bytes);
            }
            newDidx = didx.ToArray();
            newData = data.ToArray();
        }

        // patch the sources in a copy of the file, then lay the chunks out again
        var patched = (byte[])d.Clone();
        foreach (var s in sources)
        {
            var r = replace[s.SourceId];
            BitConverter.GetBytes(r.PluginId).CopyTo(patched, s.PluginAt);
            switch (s.StreamType)
            {
                case 0:
                    BitConverter.GetBytes(newSizes.TryGetValue(s.SourceId, out var n0) ? n0 : r.Wem.Length).CopyTo(patched, s.SizeAt);
                    break;
                case 1:
                    if (newSizes.TryGetValue(s.SourceId, out var n1))
                        BitConverter.GetBytes(n1).CopyTo(patched, s.SizeAt);
                    else
                        BitConverter.GetBytes(Math.Min(BitConverter.ToInt32(d, s.SizeAt), r.Wem.Length)).CopyTo(patched, s.SizeAt);
                    break;
            }
            result.SourcesPatched++;
        }

        var ms = new MemoryStream(d.Length);
        foreach (var c in chunks)
        {
            var body = c.Id == "DIDX" && newDidx != null ? newDidx
                : c.Id == "DATA" && newData != null ? newData
                : patched.AsSpan(c.Body, c.Size).ToArray();
            ms.Write(Encoding.ASCII.GetBytes(c.Id));
            ms.Write(BitConverter.GetBytes(body.Length));
            ms.Write(body);
        }
        ms.Write(d, tail, d.Length - tail);

        result.Bytes = ms.ToArray();
        return result;
    }

    /// <summary>
    /// The bytes a bank should hold for a prefetched sound whose streamed wem is now <paramref name="newWem"/>, given the
    /// old prefix size. (The writer does this itself; exposed for the edit model's previews.)
    /// </summary>
    public static byte[] PrefetchPrefix(byte[] newWem, int oldPrefixSize) => newWem.AsSpan(0, Math.Min(oldPrefixSize, newWem.Length)).ToArray();

    /// <summary>
    /// AkBankSourceData of Sound objects (type 2: ulID, then plug-in u32, stream type u8, source ID u32, in-memory
    /// size u32) and music tracks (type 11: ulID, u8 flags, u32 count, then 14-byte sources with the same fields), for
    /// the replaced media only.
    /// </summary>
    private static void ReadSources(byte[] d, int pos, int end, IReadOnlyDictionary<uint, WwiseMediaReplacement> replace, List<Source> list)
    {
        int count = BitConverter.ToInt32(d, pos);
        pos += 4;
        for (int i = 0; i < count && pos + 5 <= end; i++)
        {
            int type = d[pos], size = BitConverter.ToInt32(d, pos + 1), body = pos + 5;
            if (type == 2 && size >= 17)
            {
                uint src = BitConverter.ToUInt32(d, body + 9);
                if (replace.ContainsKey(src))
                    list.Add(new Source(body + 4, d[body + 8], src, body + 13));
            }
            else if (type == 11 && size >= 9)
            {
                int n = BitConverter.ToInt32(d, body + 5), p = body + 9;
                for (int k = 0; k < n && k < 1000 && p + 14 <= body + size; k++, p += 14)
                {
                    uint src = BitConverter.ToUInt32(d, p + 5);
                    if (replace.ContainsKey(src))
                        list.Add(new Source(p, d[p + 4], src, p + 9));
                }
            }
            pos = body + size;
        }
    }
}
