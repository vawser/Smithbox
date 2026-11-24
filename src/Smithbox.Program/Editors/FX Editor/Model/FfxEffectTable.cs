using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

/// <summary>
/// DS2 / DS2S's per-effect tables (FX.md, FFX §22.8, F8b), kept in sfx9999(_Append) as
/// sfxParameter.sfxparam ("sfxp" v5, 64-byte records) and sfxDebugParameter.sfxparam ("sfxd" v1, 8-byte records).
/// Both: a 32-byte header ("sfxp"/"sfxd", version, record count, header size 0x20, zeros), then the records sorted
/// strictly by their first word, the effect id. Records are optional for the game (19 vanilla effects have none, 257
/// records have no effect), so the editor only keeps them in step: a new effect gets a copy of its source's record, a
/// deleted one loses its record. SoulsFormats + BCL only, so the check tools can link it.
/// </summary>
public sealed class FfxEffectTable
{
    /// <summary>The 32 header bytes as stored (the count is rewritten on <see cref="Write"/>).</summary>
    public byte[] Header;

    public int RecordSize;

    /// <summary>The records by effect id, each with its id in the first 4 bytes.</summary>
    public SortedDictionary<int, byte[]> Records = new();

    /// <summary>"sfxp" (the parameter table) or "sfxd" (the debug table).</summary>
    public string Magic => System.Text.Encoding.ASCII.GetString(Header, 0, 4);

    /// <summary>Whether a binder file is one of these tables (by its extension).</summary>
    public static bool IsTable(string name) => name != null && name.EndsWith(".sfxparam", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads a table; null when the bytes aren't one this knows (magic, header size or length don't fit).</summary>
    public static FfxEffectTable Read(ReadOnlySpan<byte> b)
    {
        if (b.Length < 32)
            return null;
        var magic = System.Text.Encoding.ASCII.GetString(b[..4]);
        int size = magic switch { "sfxp" => 64, "sfxd" => 8, _ => 0 };
        int count = BinaryPrimitives.ReadInt32LittleEndian(b[8..]);
        int headerSize = BinaryPrimitives.ReadInt32LittleEndian(b[12..]);
        if (size == 0 || headerSize != 32 || count < 0 || b.Length != 32 + count * size)
            return null;
        var t = new FfxEffectTable { Header = b[..32].ToArray(), RecordSize = size };
        for (int i = 0; i < count; i++)
        {
            var rec = b.Slice(32 + i * size, size).ToArray();
            int id = BinaryPrimitives.ReadInt32LittleEndian(rec);
            if (!t.Records.TryAdd(id, rec))
                return null;
        }
        return t;
    }

    public byte[] Write()
    {
        var b = new byte[32 + Records.Count * RecordSize];
        Header.CopyTo(b, 0);
        BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8), Records.Count);
        int at = 32;
        foreach (var rec in Records.Values)
        {
            rec.CopyTo(b, at);
            at += RecordSize;
        }
        return b;
    }

    /// <summary>The commonest record body (id aside): the default for an effect with no source (171 of DS2's records).</summary>
    public byte[] Commonest()
    {
        var best = Records.Values.GroupBy(r => Convert.ToHexString(r, 4, RecordSize - 4)).OrderByDescending(g => g.Count()).FirstOrDefault()?.First();
        return best ?? new byte[RecordSize];
    }

    /// <summary>
    /// Adds a record for <paramref name="id"/> when there is none: a copy of <paramref name="body"/> (a record kept from
    /// a removal), else of <paramref name="fromId"/>'s record, else the commonest. Returns whether one was added.
    /// </summary>
    public bool Add(int id, int fromId, byte[] body)
    {
        if (Records.ContainsKey(id))
            return false;
        byte[] src = body != null && body.Length == RecordSize ? body
            : fromId >= 0 && Records.TryGetValue(fromId, out var from) ? from
            : Commonest();
        var rec = src.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(rec, id);
        Records[id] = rec;
        return true;
    }

    /// <summary>Removes the record of <paramref name="id"/>; returns it (null when there was none).</summary>
    public byte[] Remove(int id) => Records.Remove(id, out var rec) ? rec : null;
}
