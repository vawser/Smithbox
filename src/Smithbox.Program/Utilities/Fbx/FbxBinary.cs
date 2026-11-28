#nullable enable
using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Utilities.Fbx;

/// <summary>
///     Binary FBX reading and writing (<see cref="FbxDocument" />).
///     <para>
///         Layout: a 23-byte magic and a uint32 version, then node records, then a footer. A record is its end
///         offset, value count and value byte length (uint32, or uint64 from 7500), a name length byte and the name,
///         the values (type character + data; arrays as count, encoding (0 raw, 1 zlib), byte length, data), the
///         child records and an all-zero end marker (13 or 25 bytes). An all-zero record also ends the top level.
///     </para>
/// </summary>
internal static class FbxBinary
{
    /// <summary>"Kaydara FBX Binary  \0", then 0x1A 0x00.</summary>
    private static readonly byte[] Magic =
    {
        (byte)'K', (byte)'a', (byte)'y', (byte)'d', (byte)'a', (byte)'r', (byte)'a', (byte)' ', (byte)'F', (byte)'B',
        (byte)'X', (byte)' ', (byte)'B', (byte)'i', (byte)'n', (byte)'a', (byte)'r', (byte)'y', (byte)' ', (byte)' ',
        0, 0x1A, 0,
    };

    /// <summary>The footer id the FBX SDK and Blender write after the top-level end marker.</summary>
    private static readonly byte[] FooterId =
        { 0xfa, 0xbc, 0xab, 0x09, 0xd0, 0xc8, 0xd4, 0x66, 0xb1, 0x76, 0xfb, 0x83, 0x1c, 0xf7, 0x26, 0x7e };

    /// <summary>The last 16 bytes of every binary FBX file.</summary>
    private static readonly byte[] FooterMagic =
        { 0xf8, 0x5a, 0x8c, 0x6a, 0xde, 0xf5, 0xd9, 0x7e, 0xec, 0xe9, 0x0c, 0xe3, 0x75, 0x8f, 0x29, 0x0b };

    /// <summary>Arrays from this many bytes are compressed when a property doesn't say (<see cref="FbxProperty.Compressed" />).</summary>
    private const int CompressFromBytes = 128;

    private const int HeaderLength = 27;

    public static bool HasMagic(ReadOnlySpan<byte> data) =>
        data.Length >= HeaderLength && data[..Magic.Length].SequenceEqual(Magic);

    // ------------------------------------------------------------------ reading

    public static FbxDocument Read(byte[] data)
    {
        if (!HasMagic(data))
            throw new InvalidDataException("Not a binary FBX file");

        var doc = new FbxDocument
        {
            IsBinary = true,
            Version = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(Magic.Length)),
        };

        var r = new Reader(data, HeaderLength, doc.Version >= 7500);
        while (true)
        {
            if (r.Pos >= data.Length)
                throw new InvalidDataException("Binary FBX: file ends before the top-level end marker");
            var node = ReadNode(r, doc);
            if (node == null)
                break;
            doc.Nodes.Add(node);
        }

        doc.Footer = data.AsSpan(r.Pos).ToArray();
        return doc;
    }

    private static FbxNode? ReadNode(Reader r, FbxDocument doc)
    {
        var start = r.Pos;
        var end = r.Offset();
        var count = r.Offset();
        var length = r.Offset();
        var nameLength = r.U8();
        if (end == 0 && count == 0 && length == 0 && nameLength == 0)
            return null;

        var name = Encoding.UTF8.GetString(r.Bytes(nameLength));
        var node = new FbxNode(name);

        var valuesStart = r.Pos;
        for (long i = 0; i < count; i++)
            node.Properties.Add(ReadProperty(r, name));
        if (r.Pos != valuesStart + length)
            throw new InvalidDataException(
                $"Binary FBX: values of node {name} at {start} take {r.Pos - valuesStart} bytes, header says {length}");

        var marker = false;
        if (r.Pos < end)
        {
            while (true)
            {
                if (r.Pos >= end)
                    throw new InvalidDataException($"Binary FBX: node {name} at {start} has no end marker");
                var child = ReadNode(r, doc);
                if (child == null)
                {
                    marker = true;
                    break;
                }

                node.Children.Add(child);
            }
        }

        if (r.Pos != end)
            throw new InvalidDataException($"Binary FBX: node {name} at {start} ends at {r.Pos}, header says {end}");

        // A marker on a node with values and no children: an empty body, kept so the node is written the same way.
        if (marker && node.Children.Count == 0 && node.Properties.Count > 0)
            node.EmptyBody = true;

        if (marker != NeedsEndMarker(node))
        {
            doc.SentinelRuleMismatches++;
            if (doc.SentinelRuleMismatchNodes.Count < 10)
                doc.SentinelRuleMismatchNodes.Add(
                    $"{name} ({node.Properties.Count} values, {node.Children.Count} children, " +
                    $"{(marker ? "with" : "without")} end marker)");
        }
        return node;
    }

    private static FbxProperty ReadProperty(Reader r, string node)
    {
        var type = (char)r.U8();
        switch (type)
        {
            case 'C': return FbxProperty.Char(r.U8());
            case 'Y': return FbxProperty.Int16(BinaryPrimitives.ReadInt16LittleEndian(r.Bytes(2)));
            case 'I': return FbxProperty.Int32(BinaryPrimitives.ReadInt32LittleEndian(r.Bytes(4)));
            case 'L': return FbxProperty.Int64(BinaryPrimitives.ReadInt64LittleEndian(r.Bytes(8)));
            case 'F': return FbxProperty.Float(BinaryPrimitives.ReadSingleLittleEndian(r.Bytes(4)));
            case 'D': return FbxProperty.Double(BinaryPrimitives.ReadDoubleLittleEndian(r.Bytes(8)));
            case 'S': return FbxProperty.String(Encoding.UTF8.GetString(r.Bytes((int)r.U32())));
            case 'R': return FbxProperty.Raw(r.Bytes((int)r.U32()).ToArray());
            case 'b': return ReadArray(r, 1, b => FbxProperty.BoolArrayBytes(b.ToArray()));
            case 'i': return ReadArray(r, 4, b => FbxProperty.Int32Array(MemoryMarshal.Cast<byte, int>(b).ToArray()));
            case 'l': return ReadArray(r, 8, b => FbxProperty.Int64Array(MemoryMarshal.Cast<byte, long>(b).ToArray()));
            case 'f': return ReadArray(r, 4, b => FbxProperty.FloatArray(MemoryMarshal.Cast<byte, float>(b).ToArray()));
            case 'd': return ReadArray(r, 8, b => FbxProperty.DoubleArray(MemoryMarshal.Cast<byte, double>(b).ToArray()));
            default:
                throw new InvalidDataException($"Binary FBX: unknown value type '{type}' in node {node} at {r.Pos - 1}");
        }
    }

    private delegate FbxProperty ArrayMaker(ReadOnlySpan<byte> bytes);

    private static FbxProperty ReadArray(Reader r, int elementSize, ArrayMaker make)
    {
        var count = (int)r.U32();
        var encoding = r.U32();
        var length = (int)r.U32();
        var stored = r.Bytes(length);
        var size = checked(count * elementSize);

        FbxProperty prop;
        if (encoding == 0)
        {
            if (length != size)
                throw new InvalidDataException($"Binary FBX: raw array of {count} takes {length} bytes, expected {size}");
            prop = make(stored);
        }
        else if (encoding == 1)
        {
            var raw = new byte[size];
            using (var z = new ZLibStream(new MemoryStream(stored.ToArray()), CompressionMode.Decompress))
                z.ReadExactly(raw);
            prop = make(raw);
        }
        else
        {
            throw new InvalidDataException($"Binary FBX: unknown array encoding {encoding}");
        }

        prop.Compressed = encoding == 1;
        return prop;
    }

    private sealed class Reader
    {
        private readonly byte[] _data;
        private readonly bool _wide;
        public int Pos;

        public Reader(byte[] data, int pos, bool wide)
        {
            _data = data;
            Pos = pos;
            _wide = wide;
        }

        public ReadOnlySpan<byte> Bytes(int n)
        {
            if (n < 0 || Pos + n > _data.Length)
                throw new InvalidDataException($"Binary FBX: read of {n} bytes at {Pos} runs past the end");
            var s = _data.AsSpan(Pos, n);
            Pos += n;
            return s;
        }

        public byte U8() => Bytes(1)[0];
        public uint U32() => BinaryPrimitives.ReadUInt32LittleEndian(Bytes(4));

        /// <summary>A record header field: uint32, or uint64 from version 7500.</summary>
        public long Offset() => _wide ? (long)BinaryPrimitives.ReadUInt64LittleEndian(Bytes(8)) : U32();
    }

    // ------------------------------------------------------------------ writing

    /// <summary>
    ///     A node gets an end marker when it has children, no values, or an empty body
    ///     (<see cref="FbxNode.EmptyBody" />), as the FBX SDK and Blender write them.
    /// </summary>
    private static bool NeedsEndMarker(FbxNode node) =>
        node.Children.Count > 0 || node.Properties.Count == 0 || node.EmptyBody;

    public static byte[] Write(FbxDocument doc, int version)
    {
        var wide = version >= 7500;
        var ms = new MemoryStream();
        var w = new BinaryWriter(ms);

        w.Write(Magic);
        w.Write((uint)version);
        foreach (var node in doc.Nodes)
            WriteNode(w, node, wide);
        WriteEndMarker(w, wide);

        // The FBX SDK writes a per-file id at the start of the footer; keep it, and rebuild the padding after it
        // (its length depends on where the footer starts, which changes when arrays compress differently).
        var id = doc.IsBinary && doc.Footer is { Length: >= 16 } ? doc.Footer.AsSpan(0, 16) : FooterId;
        WriteFooter(w, version, id);

        w.Flush();
        return ms.ToArray();
    }

    private static void WriteNode(BinaryWriter w, FbxNode node, bool wide)
    {
        var name = Encoding.UTF8.GetBytes(node.Name);
        if (name.Length > 255)
            throw new InvalidDataException($"FBX node name longer than 255 bytes: {node.Name}");

        var s = w.BaseStream;
        var start = s.Position;
        WriteOffset(w, 0, wide);
        WriteOffset(w, 0, wide);
        WriteOffset(w, 0, wide);
        w.Write((byte)name.Length);
        w.Write(name);

        var valuesStart = s.Position;
        foreach (var p in node.Properties)
            WriteProperty(w, p);
        var valuesLength = s.Position - valuesStart;

        if (NeedsEndMarker(node))
        {
            foreach (var c in node.Children)
                WriteNode(w, c, wide);
            WriteEndMarker(w, wide);
        }

        var end = s.Position;
        if (!wide && end > uint.MaxValue)
            throw new InvalidDataException("FBX file over 4 GB needs version 7500 or later");

        s.Position = start;
        WriteOffset(w, end, wide);
        WriteOffset(w, node.Properties.Count, wide);
        WriteOffset(w, valuesLength, wide);
        s.Position = end;
    }

    private static void WriteOffset(BinaryWriter w, long v, bool wide)
    {
        if (wide)
            w.Write((ulong)v);
        else
            w.Write((uint)v);
    }

    private static void WriteEndMarker(BinaryWriter w, bool wide) => w.Write(new byte[wide ? 25 : 13]);

    private static void WriteProperty(BinaryWriter w, FbxProperty p)
    {
        w.Write((byte)p.Type);
        switch (p.Value)
        {
            case byte b when p.Type == FbxPropertyType.Bool: w.Write(b); return;
            case short v: w.Write(v); return;
            case int v: w.Write(v); return;
            case long v: w.Write(v); return;
            case float v: w.Write(v); return;
            case double v: w.Write(v); return;
            case string v:
            {
                var bytes = Encoding.UTF8.GetBytes(v);
                w.Write((uint)bytes.Length);
                w.Write(bytes);
                return;
            }
            case byte[] v when p.Type == FbxPropertyType.Raw:
                w.Write((uint)v.Length);
                w.Write(v);
                return;
            case byte[] v: WriteArray(w, p, v.Length, v); return;
            case int[] v: WriteArray(w, p, v.Length, MemoryMarshal.AsBytes(v.AsSpan())); return;
            case long[] v: WriteArray(w, p, v.Length, MemoryMarshal.AsBytes(v.AsSpan())); return;
            case float[] v: WriteArray(w, p, v.Length, MemoryMarshal.AsBytes(v.AsSpan())); return;
            case double[] v: WriteArray(w, p, v.Length, MemoryMarshal.AsBytes(v.AsSpan())); return;
            default:
                throw new InvalidDataException($"FBX value of type {p.Type} holds a {p.Value.GetType().Name}");
        }
    }

    private static void WriteArray(BinaryWriter w, FbxProperty p, int count, ReadOnlySpan<byte> raw)
    {
        var compress = p.Compressed ?? raw.Length >= CompressFromBytes;
        w.Write((uint)count);
        w.Write(compress ? 1u : 0u);
        if (!compress)
        {
            w.Write((uint)raw.Length);
            w.Write(raw);
            return;
        }

        var packed = new MemoryStream();
        using (var z = new ZLibStream(packed, CompressionLevel.Optimal, leaveOpen: true))
            z.Write(raw);
        w.Write((uint)packed.Length);
        w.Write(packed.GetBuffer(), 0, (int)packed.Length);
    }

    /// <summary>Footer id, 4 zero bytes, zero padding to a 16-byte boundary (1–16 bytes), version, 120 zero bytes, magic.</summary>
    private static void WriteFooter(BinaryWriter w, int version, ReadOnlySpan<byte> id)
    {
        w.Write(id);
        w.Write(new byte[4]);
        var at = w.BaseStream.Position;
        var pad = (int)(((at + 15) & ~15L) - at);
        if (pad == 0)
            pad = 16;
        w.Write(new byte[pad]);
        w.Write((uint)version);
        w.Write(new byte[120]);
        w.Write(FooterMagic);
    }
}
