using HKLib.Serialization.Util;

namespace HKLib.Serialization.hk2018.Binary.Util;

public class HavokBinaryWriter : BinaryWriterEx
{
    private const uint NoChildrenFlag = 0x40000000;

    private readonly Stack<SectionInfo> _sectionInfos = new();

    public HavokBinaryWriter(Stream stream) : base(false, stream) { }

    /// <summary>
    /// Begins a new section, updates the parent section and writes out the section header.
    /// </summary>
    public void BeginSection(string name)
    {
        if (_sectionInfos.TryPeek(out SectionInfo? parent))
        {
            parent.hasChildren = true;
        }

        SectionInfo sectionInfo = new()
        {
            name = name,
            start = Position
        };

        ReserveUInt32($"{name} Length");
        WriteASCII(name);

        _sectionInfos.Push(sectionInfo);
    }

    /// <summary>
    /// Ends the current section and writes its length into the header.
    /// </summary>
    public void EndSection()
    {
        SectionInfo sectionInfo = _sectionInfos.Pop();
        uint length = (uint)(Position - sectionInfo.start);
        if (!sectionInfo.hasChildren)
        {
            length |= NoChildrenFlag;
        }

        FillUInt32($"{sectionInfo.name} Length", length, true);
    }

    private class SectionInfo
    {
        public bool hasChildren;
        public string name = "";

        public long start;
    }

    #region HavokVarUInt

    public void WriteHavokVarUInt(ulong value)
    {
        byte[] bytes = HavokVarUInt.ToBytes(value);
        WriteBytes(bytes);
    }

    public void WriteHavokVarUInts(IList<ulong> values)
    {
        foreach (ulong value in values) WriteHavokVarUInt(value);
    }

    #endregion

    #region HavokVarInt

    public void WriteHavokVarInt(long value)
    {
        byte[] bytes = HavokVarUInt.ToBytes(value);
        WriteBytes(bytes);
    }

    public void WriteHavokVarInts(IList<long> values)
    {
        foreach (long value in values) WriteHavokVarInt(value);
    }

    #endregion
}