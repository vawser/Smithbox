using System.Buffers.Binary;
using SoulsFormats;

namespace Havok.Formats.Tagfile;

/// <summary>
/// Reads tagfile sections and values over SoulsFormats' <see cref="BinaryReaderEx" /> (which is sealed, so this wraps
/// it). Values are little-endian unless a type's format asks for big-endian.
/// </summary>
internal sealed class TagfileReader
{
    private readonly BinaryReaderEx _br;
    private readonly Stack<SectionInfo> _sections = new();

    public TagfileReader(Memory<byte> data)
    {
        _br = new BinaryReaderEx(false, data);
    }

    public long Position
    {
        get => _br.Position;
        set => _br.Position = value;
    }

    public long Length => _br.Length;

    /// <summary>
    /// Verifies that every section was exited.
    /// </summary>
    public void Close()
    {
        if (_sections.Count != 0)
            throw new InvalidOperationException($"Section {_sections.Peek().Id} was not exited.");
    }

    #region Sections

    /// <summary>
    /// Reads a section header and asserts that it has the given id. Headers are a big-endian uint (2 flag bits and
    /// the size including the header) followed by the 4 character id.
    /// </summary>
    public void EnterSection(string sectionId)
    {
        long start = Position;
        int length = (int)(ReadUInt32BigEndian() & 0x0fffffff);
        _br.AssertASCII(sectionId);
        _sections.Push(new SectionInfo(sectionId, length, start + length));
    }

    /// <summary>
    /// Checks the reader didn't read past the end of the section, and skips to its end, asserting the rest is 0.
    /// </summary>
    public void ExitSection()
    {
        SectionInfo section = _sections.Pop();
        if (Position > section.End)
        {
            throw new InvalidOperationException(
                $"Read past the end of section {section.Id}. Current Position: {Position}, Section End: {section.End}");
        }

        while (Position != section.End)
        {
            _br.AssertByte(0);
        }
    }

    /// <summary>
    /// Skips to the end of the section without the checks of <see cref="ExitSection" />.
    /// </summary>
    public void SkipSection()
    {
        Position = _sections.Pop().End;
    }

    /// <summary>
    /// The length of the current section, excluding its header.
    /// </summary>
    public int GetSectionLength() => _sections.Peek().Length - 8;

    public long GetSectionEnd() => _sections.Peek().End;

    /// <summary>
    /// The id of the section header at the current position.
    /// </summary>
    public string GetSectionId() => _br.GetASCII(Position + 4, 4);

    private readonly record struct SectionInfo(string Id, int Length, long End);

    #endregion

    #region Values

    public byte ReadByte() => _br.ReadByte();
    public sbyte ReadSByte() => _br.ReadSByte();
    public byte GetByte(long offset) => _br.GetByte(offset);
    public byte[] ReadBytes(int count) => _br.ReadBytes(count);
    // Not BinaryReaderEx.ReadBytes(buffer, index, count): that one copies without advancing the position
    public void ReadBytes(byte[] buffer, int index, int count) => _br.ReadBytes(count).CopyTo(buffer, index);
    public byte[] GetBytes(long offset, int count) => _br.GetBytes(offset, count);
    public void AssertByte(byte value) => _br.AssertByte(value);

    public short ReadInt16(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadInt16()) : _br.ReadInt16();
    public ushort ReadUInt16(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadUInt16()) : _br.ReadUInt16();
    public int ReadInt32(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadInt32()) : _br.ReadInt32();
    public uint ReadUInt32(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadUInt32()) : _br.ReadUInt32();
    public long ReadInt64(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadInt64()) : _br.ReadInt64();
    public ulong ReadUInt64(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadUInt64()) : _br.ReadUInt64();
    public float ReadSingle(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadSingle()) : _br.ReadSingle();
    public double ReadDouble(bool bigEndian = false) => bigEndian ? WithEndian(() => _br.ReadDouble()) : _br.ReadDouble();

    public ulong[] ReadUInt64s(int count)
    {
        var values = new ulong[count];
        for (int i = 0; i < count; i++) values[i] = _br.ReadUInt64();
        return values;
    }

    public void AssertUInt32(uint value) => _br.AssertUInt32(value);
    public void AssertUInt64(ulong value) => _br.AssertUInt64(value);

    public string ReadASCII() => _br.ReadASCII();
    public string ReadASCII(int length) => _br.ReadASCII(length);

    private uint ReadUInt32BigEndian()
    {
        Span<byte> bytes = stackalloc byte[4];
        for (int i = 0; i < 4; i++) bytes[i] = _br.ReadByte();
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    // Only big-endian values go through here, so the little-endian path allocates nothing
    private T WithEndian<T>(Func<T> read)
    {
        _br.BigEndian = true;
        try { return read(); }
        finally { _br.BigEndian = false; }
    }

    public void StepIn(long offset) => _br.StepIn(offset);
    public void StepOut() => _br.StepOut();

    public ulong ReadHavokVarUInt()
    {
        byte prefix = _br.ReadByte();
        int length = HavokVarUInt.GetLength(prefix);
        byte[] bytes = new byte[length];
        bytes[0] = prefix;
        if (length > 1) ReadBytes(bytes, 1, length - 1);
        return HavokVarUInt.ToUInt64(bytes);
    }

    #endregion
}

/// <summary>
/// Writes tagfile sections and values over SoulsFormats' <see cref="BinaryWriterEx" />.
/// </summary>
internal sealed class TagfileWriter : BinaryWriterEx
{
    private const uint NoChildrenFlag = 0x40000000;

    private readonly Stack<SectionInfo> _sections = new();

    public TagfileWriter() : base(false) { }

    /// <summary>
    /// Begins a section: marks the parent as having children and writes a header whose size is filled in by
    /// <see cref="EndSection" />.
    /// </summary>
    public void BeginSection(string id)
    {
        if (_sections.TryPeek(out SectionInfo? parent))
        {
            parent.HasChildren = true;
        }

        _sections.Push(new SectionInfo { Id = id, Start = Position });
        WriteUInt32(0);
        WriteASCII(id);
    }

    /// <summary>
    /// Ends the current section and writes its size (big-endian, with the no-children flag) into its header.
    /// </summary>
    public void EndSection()
    {
        SectionInfo section = _sections.Pop();
        uint length = (uint)(Position - section.Start);
        if (!section.HasChildren)
        {
            length |= NoChildrenFlag;
        }

        long end = Position;
        Position = section.Start;
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, length);
        WriteBytes(bytes);
        Position = end;
    }

    public void WriteInt16(short value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteInt16(value)); else WriteInt16(value); }
    public void WriteUInt16(ushort value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteUInt16(value)); else WriteUInt16(value); }
    public void WriteInt32(int value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteInt32(value)); else WriteInt32(value); }
    public void WriteUInt32(uint value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteUInt32(value)); else WriteUInt32(value); }
    public void WriteInt64(long value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteInt64(value)); else WriteInt64(value); }
    public void WriteUInt64(ulong value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteUInt64(value)); else WriteUInt64(value); }
    public void WriteSingle(float value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteSingle(value)); else WriteSingle(value); }
    public void WriteDouble(double value, bool bigEndian) { if (bigEndian) WithEndian(() => WriteDouble(value)); else WriteDouble(value); }

    public void WriteUInt64s(ulong[] values)
    {
        foreach (ulong value in values) WriteUInt64(value);
    }

    public void WriteHavokVarUInt(ulong value) => WriteBytes(HavokVarUInt.ToBytes(value));

    /// <summary>
    /// Pads to a multiple of 4 with the given byte (string sections of 2019 files are padded with 0xFF).
    /// </summary>
    public void PadWith(int align, byte value)
    {
        while (Position % align != 0) WriteByte(value);
    }

    private void WithEndian(Action write)
    {
        BigEndian = true;
        try { write(); }
        finally { BigEndian = false; }
    }

    private sealed class SectionInfo
    {
        public bool HasChildren;
        public string Id = "";
        public long Start;
    }
}
