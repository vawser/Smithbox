using HKLib.Serialization.Util;

namespace HKLib.Serialization.hk2018.Binary.Util;

public class HavokBinaryReader : BinaryReaderEx
{
    private readonly Stack<SectionInfo> _sectionInfos = new();

    public HavokBinaryReader(byte[] input) : base(false, input) { }

    public HavokBinaryReader(Stream stream) : base(false, stream) { }

    /// <summary>
    /// Verifies that all sections have been exited and closes the underlying stream
    /// </summary>
    public override void Close()
    {
        if (_sectionInfos.Count != 0)
        {
            throw new InvalidOperationException($"Section {_sectionInfos.Peek().Id} was not exited.");
        }

        base.Close();
    }

    /// <summary>
    /// Reads the section header and asserts that it matches the provided identifier.
    /// </summary>
    public void EnterSection(string sectionId)
    {
        long start = Position;
        int length = ReadInt32(true) & 0x0fffffff;
        AssertASCII(sectionId);

        SectionInfo sectionInfo = new(sectionId, length, start + length);
        _sectionInfos.Push(sectionInfo);
    }

    /// <summary>
    /// Makes sure the reader did not read past the end of the current section and advances to the end of the current
    /// section
    /// while asserting that all remaining bytes are 0.
    /// </summary>
    public void ExitSection()
    {
        SectionInfo sectionInfo = _sectionInfos.Pop();

        if (Position > sectionInfo.End)
        {
            throw new InvalidOperationException(
                $"Read past the end of section {_sectionInfos.Peek().Id}. Current Position: {Position}, Section End: {sectionInfo.End}");
        }

        while (Position != sectionInfo.End)
        {
            AssertByte(0);
        }
    }

    /// <summary>
    /// Skips to the end of the section without any of the checks performed by <see cref="ExitSection" />.
    /// Use <see cref="ExitSection" /> instead if possible.
    /// </summary>
    public void SkipSection()
    {
        SectionInfo sectionInfo = _sectionInfos.Pop();
        Position = sectionInfo.End;
    }

    /// <summary>
    /// Gets the length of the section excluding the header
    /// </summary>
    public int GetSectionLength()
    {
        return _sectionInfos.Peek().Length - 8;
    }

    /// <summary>
    /// Gets the end position of the current section
    /// </summary>
    public long GetSectionEnd()
    {
        return _sectionInfos.Peek().End;
    }

    /// <summary>
    /// Gets a section identifier from a section header at the current position
    /// </summary>
    public string GetSectionId()
    {
        return GetASCII(Position + 4, 4);
    }

    private record struct SectionInfo(string Id, int Length, long End);

    #region HavokVarUInt

    public ulong ReadHavokVarUInt()
    {
        byte prefix = ReadByte();
        int length = HavokVarUInt.GetLength(prefix);
        byte[] bytes = new byte[length];
        bytes[0] = prefix;
        ReadBytes(bytes, 1, length - 1);
        return HavokVarUInt.ToUInt64(bytes);
    }

    public ulong[] ReadHavokVarUInts(int count)
    {
        ulong[] res = new ulong[count];
        for (int i = 0; i < count; i++)
        {
            res[i] = ReadHavokVarUInt();
        }

        return res;
    }

    public ulong GetHavokVarUInt(long offset)
    {
        return GetValue(ReadHavokVarUInt, offset);
    }

    public ulong[] GetHavokVarUInts(long offset, int count)
    {
        return GetValues(ReadHavokVarUInts, offset, count);
    }

    public ulong AssertHavokVarUInt(params ulong[] options)
    {
        return AssertValue(ReadHavokVarUInt(), "HavokVarUInt", "0x{0:X}", options);
    }

    #endregion

    #region HavokVarInt

    public long ReadHavokVarInt()
    {
        byte prefix = GetByte(0);
        int length = HavokVarUInt.GetLength(prefix);
        byte[] bytes = ReadBytes(length);
        return HavokVarUInt.ToInt64(bytes);
    }

    public long[] ReadHavokVarInts(int count)
    {
        long[] res = new long[count];
        for (int i = 0; i < count; i++)
        {
            res[i] = ReadHavokVarInt();
        }

        return res;
    }

    public long GetHavokVarInt(long offset)
    {
        return GetValue(ReadHavokVarInt, offset);
    }

    public long[] GetHavokVarInts(long offset, int count)
    {
        return GetValues(ReadHavokVarInts, offset, count);
    }

    public long AssertHavokVarInt(params long[] options)
    {
        return AssertValue(ReadHavokVarInt(), "HavokVarInt", "0x{0:X}", options);
    }

    #endregion
}