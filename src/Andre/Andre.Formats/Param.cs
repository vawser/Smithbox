using SoulsFormats;
using Andre.Core;

namespace Andre.Formats;

/// <summary>
///     An alternative to the SoulsFormats param class that's designed to be faster to read/write and be
///     much more memory efficient. This tries to match the SoulsFormats PARAM API as much as possible but
///     has some differences out of necessity. The main difference is rows and cells are separate rather
///     than each row having an array of cells. For convenience, a CellHandle struct was added that provides
///     a similar API to the SoulsFormats Cell.
///     A lot of this code is based off the SoulsFormats PARAM class (especially the read/write), so thanks TKGP.
/// </summary>
public class Param : SoulsFile<Param>
{
    /// <summary>
    ///     First set of flags indicating file format; highly speculative.
    /// </summary>
    [Flags]
    public enum FormatFlags1 : byte
    {
        /// <summary>
        ///     No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Unknown.
        /// </summary>
        Flag01 = 0b0000_0001,

        /// <summary>
        ///     Expanded header with 32-bit data offset.
        /// </summary>
        IntDataOffset = 0b0000_0010,

        /// <summary>
        ///     Expanded header with 64-bit data offset.
        /// </summary>
        LongDataOffset = 0b0000_0100,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag08 = 0b0000_1000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag10 = 0b0001_0000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag20 = 0b0010_0000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag40 = 0b0100_0000,

        /// <summary>
        ///     Param type string is written separately instead of fixed-width in the header.
        /// </summary>
        OffsetParamType = 0b1000_0000
    }

    /// <summary>
    ///     Second set of flags indicating file format; highly speculative.
    /// </summary>
    [Flags]
    public enum FormatFlags2 : byte
    {
        /// <summary>
        ///     No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Row names are written as UTF-16.
        /// </summary>
        UnicodeRowNames = 0b0000_0001,

        /// <summary>
        ///     Unknown.
        /// </summary>
        Flag02 = 0b0000_0010,

        /// <summary>
        ///     Unknown.
        /// </summary>
        Flag04 = 0b0000_0100,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag08 = 0b0000_1000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag10 = 0b0001_0000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag20 = 0b0010_0000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag40 = 0b0100_0000,

        /// <summary>
        ///     Unused?
        /// </summary>
        Flag80 = 0b1000_0000
    }

    private StridedByteArray _paramData = new(0, 1);

    private List<Row> _rows = new();

    /// <summary>
    ///     Create an empty param. Param specific header data must be set before saving and ApplyParamdef()
    ///     must be called before using the Row APIs.
    /// </summary>
    public Param()
    {
        BigEndian = false;
        Format2D = FormatFlags1.None;
        Format2E = FormatFlags2.None;
        ParamdefDataVersion = 0;
        ParamdefFormatVersion = 0;
        Unk06 = 0;
        ParamType = null;
    }

    /// <summary>
    ///     Create an empty param conforming to a specified paramdef. Param specific header data must be
    ///     set before saving.
    /// </summary>
    /// <param name="paramdef">The paramdef that this param conforms to</param>
    /// <param name="bigEndian">Whether the param is stored in big endian or not</param>
    /// <param name="regulationVersion">For versioned paramdefs, the regulation version to apply</param>
    public Param(PARAMDEF paramdef, bool bigEndian = false, ulong regulationVersion = ulong.MaxValue)
    {
        BigEndian = bigEndian;
        Format2D = FormatFlags1.None;
        Format2E = FormatFlags2.None;
        ParamdefDataVersion = paramdef.DataVersion;
        ParamdefFormatVersion = 0;
        Unk06 = 0;
        ParamType = paramdef.ParamType;
        ApplyParamdef(paramdef, regulationVersion);
    }

    /// <summary>
    ///     Creates a new empty param inheriting config/paramdef from a source.
    /// </summary>
    /// <param name="source"></param>
    public Param(Param source)
    {
        BigEndian = source.BigEndian;
        Format2D = source.Format2D;
        Format2E = source.Format2E;
        ParamdefFormatVersion = source.ParamdefFormatVersion;
        Unk06 = source.Unk06;
        ParamdefDataVersion = source.ParamdefDataVersion;
        ParamType = source.ParamType;
        RowSize = source.RowSize;
        _paramData = new StridedByteArray((uint)source._rows.Count, (uint)RowSize, BigEndian);
        Columns = source.Columns;
        AppliedParamdef = source.AppliedParamdef;
    }

    /// <summary>
    ///     Whether the file is big-endian; true for PS3/360 files, false otherwise.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    ///     Flags indicating format of the file.
    /// </summary>
    public FormatFlags1 Format2D { get; set; }

    /// <summary>
    ///     More flags indicating format of the file.
    /// </summary>
    public FormatFlags2 Format2E { get; set; }

    /// <summary>
    ///     Originally matched the paramdef for version 101, but since is always 0 or 0xFF.
    /// </summary>
    public byte ParamdefFormatVersion { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    public short Unk06 { get; set; }

    /// <summary>
    ///     Indicates a revision of the row data structure.
    /// </summary>
    public short ParamdefDataVersion { get; set; }

    /// <summary>
    ///     Identifies corresponding params and paramdefs.
    /// </summary>
    public string? ParamType { get; set; }

    /// <summary>
    ///     Detected size of the row in bytes. Empty params will have a size of 0 and params constructed
    ///     from scratch without a paramdef applied will have a size of -1
    /// </summary>
    public int RowSize { get; private set; } = -1;

    /// <summary>
    ///     List of rows in the param. The list itself is readonly and the row API should be used to add and
    ///     delete rows, but this can be reset to a specified list of rows so long as all the rows are
    ///     constructed with this param as the parent.
    /// </summary>
    public IReadOnlyList<Row> Rows
    {
        get => _rows;
        set
        {
            if (value == null)
                throw new ArgumentNullException();

            if (Rows.Any(r => r.Parent != this))
            {
                throw new ArgumentException("Attempting to add rows created from another Param");
            }

            _rows = new List<Row>(value);
        }
    }

    /// <summary>
    /// Returns true if the param contains the specified row ID
    /// </summary>
    public bool ContainsRow(int id)
    {
        if(_rows.Any(e => e.ID == id))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     List of columns created from the applied paramdef. You can iterate through these and use the columns
    ///     to access the specific data of rows.
    /// </summary>
    public IReadOnlyList<Column> Columns { get; private set; } = new List<Column>();

    /// <summary>
    ///     The applied paramdef
    /// </summary>
    public PARAMDEF? AppliedParamdef { get; private set; }


    /// <summary>
    ///     Gets the index of the Row with ID id or returns null
    /// </summary>
    /// <param name="id">The ID of the row to find</param>
    public Row? this[int id]
    {
        get
        {
            for (var i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].ID == id)
                    return Rows[i];
            }

            return null;
        }
    }

    /// <summary>
    ///     Gets a param column from a field name in the paramdef. Note that this currently runs in quadratic time
    ///     with respect to the number of paramdef fields and this should not be used in hot code. Mostly available
    ///     for compatability with Soulsformats PARAM class.
    /// </summary>
    /// <param name="name">The internal name of the paramdef field to lookup</param>
    public Column? this[string name] => Columns.FirstOrDefault(cell => cell.Def.InternalName == name);

    /// <summary>
    ///     Delete all the rows in the param
    /// </summary>
    public void ClearRows()
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        _rows.Clear();
    }

    /// <summary>
    ///     Adds a row at the end of the param row lists. Row must be created with this param as the parent.
    /// </summary>
    /// <param name="row">The row to add</param>
    public void AddRow(Row row)
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        if (row.Parent != this)
            throw new ArgumentException();
        _rows.Add(row);
    }

    /// <summary>
    ///     Inserts a row in the row list at a specified index. Row must be created with this param as the parent.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="row"></param>
    public void InsertRow(int index, Row row)
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        if (row.Parent != this)
            throw new ArgumentException();
        _rows.Insert(index, row);
    }

    /// <summary>
    ///     Returns the index of a specified row in this param, or -1 if the row is not found. This runs
    ///     in linear time with respect to the total size of the param and should not be used in hot code.
    /// </summary>
    /// <param name="row"></param>
    /// <returns>The index of the row, or -1 if the row is not found.</returns>
    public int IndexOfRow(Row? row)
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        if (row == null || row.Parent != this)
            throw new ArgumentException();
        return _rows.IndexOf(row);
    }

    /// <summary>
    ///     Removes a row from the param if the row is found. This runs in linear time for the search and then
    ///     the shifting.
    /// </summary>
    /// <param name="row">The row to search for</param>
    /// <returns>True if the row was found and removed</returns>
    public bool RemoveRow(Row row)
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        if (row.Parent != this)
            throw new ArgumentException();
        return _rows.Remove(row);
    }

    /// <summary>
    ///     Removes a row from the param at a specified index. This runs in linear time for the shifting but
    ///     is generally faster than removing by row reference.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveRowAt(int index)
    {
        if (AppliedParamdef == null)
            throw new Exception("Paramdef must be applied to use row management APIs");
        _rows.RemoveAt(index);
    }

    /// <summary>
    ///     Apply a paramdef to a newly created/read param. For params that were read, the computed row
    ///     size from the layout must match the row size of the param file read. For params created from
    ///     scratch, the row size will be computed from the layout. The endianess of the param should be
    ///     set before this method is called.
    /// </summary>
    /// <param name="def">The paramdef to apply</param>
    /// <param name="regulationVersion">
    ///     For version aware paramdefs, the regulation version of the param the
    ///     paramdef is being applied to
    /// </param>
    public void ApplyParamdef(PARAMDEF def, ulong regulationVersion = ulong.MaxValue)
    {
        if (AppliedParamdef != null)
            throw new ArgumentException("Param already has a paramdef applied.");

        AppliedParamdef = def;
        var columns = new List<Column>(def.Fields.Count);

        var bitOffset = -1;
        uint byteOffset = 0;
        uint lastSize = 0;
        var bitType = PARAMDEF.DefType.u8;

        foreach (PARAMDEF.Field? field in def.Fields)
        {
            if (def.VersionAware && !field.IsValidForRegulationVersion(regulationVersion))
                continue;

            PARAMDEF.DefType type = field.DisplayType;

            var isBitType = ParamUtil.IsBitType(type);

            if (!isBitType || (isBitType && field.BitSize == -1))
            {
                // Advance the offset if we were last reading bits
                if (bitOffset != -1)
                    byteOffset += lastSize;

                columns.Add(ParamUtil.IsArrayType(type)
                    ? new Column(field, byteOffset, (uint)field.ArrayLength)
                    : new Column(field, byteOffset));

                switch (type)
                {
                    case PARAMDEF.DefType.s8:
                    case PARAMDEF.DefType.u8:
                        byteOffset += 1;
                        break;
                    case PARAMDEF.DefType.s16:
                    case PARAMDEF.DefType.u16:
                        byteOffset += 2;
                        break;
                    case PARAMDEF.DefType.s32:
                    case PARAMDEF.DefType.u32:
                    case PARAMDEF.DefType.f32:
                    case PARAMDEF.DefType.b32:
                    case PARAMDEF.DefType.angle32:
                        byteOffset += 4;
                        break;
                    case PARAMDEF.DefType.f64:
                        byteOffset += 8;
                        break;
                    case PARAMDEF.DefType.fixstr:
                    case PARAMDEF.DefType.dummy8:
                        byteOffset += (uint)field.ArrayLength;
                        break;
                    case PARAMDEF.DefType.fixstrW:
                        byteOffset += (uint)field.ArrayLength * 2;
                        break;
                    default:
                        throw new NotImplementedException($"Unsupported field type: {type}");
                }

                bitOffset = -1;
            }
            else
            {
                PARAMDEF.DefType newBitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                var bitLimit = ParamUtil.GetBitLimit(newBitType);

                if (field.BitSize == 0)
                {
                    throw new NotImplementedException("Bit size 0 is not supported.");
                }

                if (field.BitSize > bitLimit)
                {
                    throw new InvalidDataException(
                        $"Bit size {field.BitSize} is too large to fit in type {newBitType}.");
                }

                lastSize = (uint)ParamUtil.GetValueSize(newBitType);

                if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit)
                {
                    if (bitOffset != -1)
                        byteOffset += lastSize;

                    bitOffset = 0;
                    bitType = newBitType;
                }

                columns.Add(new Column(field, byteOffset, field.BitSize, (uint)bitOffset));
                bitOffset += field.BitSize;
            }
        }

        // Get the final size
        if (bitOffset != -1)
            byteOffset += lastSize;

        if (RowSize == -1)
        {
            // Param is being created from scratch. Set the row size and create an initial data store
            RowSize = (int)byteOffset;
            _paramData = new StridedByteArray(32, (uint)RowSize, BigEndian);
        }
        // If a row size is already read it must match our computed row size
        else if (byteOffset != RowSize)
        {
            throw new Exception($@"Row size paramdef mismatch for {ParamType} - byteOffset:{byteOffset} - rowSize:{RowSize}");
        }

        Columns = columns;
    }

    /// <summary>
    ///     A bug in prior versions of DSMS and other param editors would save soundCutsceneParam as
    ///     32 bytes instead of 36 bytes. Fortunately appending 0s at the end should be enough to fix
    ///     these params.
    /// </summary>
    private void FixupERSoundCutsceneParam()
    {
        var newData = new StridedByteArray((uint)Rows.Count, 36, BigEndian);
        for (var i = 0; i < Rows.Count; i++)
        {
            newData.AddZeroedElement();
            _paramData.CopyData(newData, (uint)i, (uint)i);
        }

        _paramData = newData;
        RowSize = 36;
    }

    /// <summary>
    /// Fix up function to extend Param rows when they are expanded in regulation patches. Ignored if the Param row is already fixed.
    /// </summary>
    public bool FixupERField(int originalSize, int newSize)
    {
        if (RowSize != originalSize)
            return false;

        var newData = new StridedByteArray((uint)Rows.Count, (uint)newSize, BigEndian);
        for (var i = 0; i < Rows.Count; i++)
        {
            newData.AddZeroedElement();
            _paramData.CopyData(newData, (uint)i, (uint)i);
        }

        _paramData = newData;
        RowSize = newSize;

        return true;
    }

    protected override void Read(BinaryReaderEx br)
    {
        br.Position = 0x2C;
        br.BigEndian = BigEndian = br.AssertByte([0, 0xFF]) == 0xFF;
        Format2D = (FormatFlags1)br.ReadByte();
        Format2E = (FormatFlags2)br.ReadByte();
        ParamdefFormatVersion = br.ReadByte();
        br.Position = 0;

        // The strings offset in the header is highly unreliable; only use it as a last resort
        long actualStringsOffset = 0;
        long stringsOffset = br.ReadUInt32();
        if ((Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset)) ||
            Format2D.HasFlag(FormatFlags1.LongDataOffset))
        {
            br.AssertInt16([0]);
        }
        else
        {
            br.ReadUInt16(); // Data start
        }

        Unk06 = br.ReadInt16();
        ParamdefDataVersion = br.ReadInt16();
        var rowCount = br.ReadUInt16();
        long paramTypeOffset = 0;
        if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
        {
            br.AssertInt32(0);
            paramTypeOffset = br.ReadInt64();
            br.AssertPattern(0x14, 0x00);

            // ParamType itself will be checked after rows.
            actualStringsOffset = paramTypeOffset;
        }
        else
        {
            ParamType = br.ReadFixStr(0x20);
        }

        br.Skip(4); // Format
        if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
        {
            br.ReadInt32(); // Data start
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }
        else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
        {
            br.ReadInt64(); // Data start
            br.AssertInt64(0);
        }

        Rows = new List<Row>(rowCount);
        for (var i = 0; i < rowCount; i++)
        {
            long nameOffset;
            int id;
            string? name = null;
            uint dataIndex;
            if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                id = br.ReadInt32();
                br.ReadInt32(); // I would like to assert 0, but some of the generatordbglocation params in DS2S have garbage here
                dataIndex = (uint)br.ReadInt64();
                nameOffset = br.ReadInt64();
            }
            else
            {
                id = br.ReadInt32();
                dataIndex = br.ReadUInt32();
                nameOffset = br.ReadUInt32();
            }

            if (nameOffset != 0)
            {
                if (actualStringsOffset == 0 || nameOffset < actualStringsOffset)
                    actualStringsOffset = nameOffset;

                if (Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                    name = br.GetUTF16(nameOffset);
                else
                    name = br.GetShiftJIS(nameOffset);
            }

            _rows.Add(new Row(id, name, this, dataIndex));
        }

        if (Rows.Count > 1)
            RowSize = (int)(Rows[1].DataIndex - Rows[0].DataIndex);
        else if (Rows.Count == 1)
            RowSize = (int)((actualStringsOffset == 0 ? (uint)stringsOffset : (uint)actualStringsOffset) -
                            Rows[0].DataIndex);
        else
            RowSize = 0;

        uint dataStart = 0;
        if (Rows.Count > 0)
        {
            dataStart = Rows.Min(row => row.DataIndex);
            br.Position = dataStart;
            var rowData = br.ReadBytes(Rows.Count * RowSize);
            _paramData = new StridedByteArray(rowData, (uint)RowSize, BigEndian);

            // Convert raw data offsets into indices
            foreach (Row r in Rows)
            {
                r.DataIndex = (r.DataIndex - dataStart) / (uint)RowSize;
            }
        }

        if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
        {
            // Check if ParamTypeOffset is valid.
            if (paramTypeOffset == dataStart + (rowCount * RowSize))
            {
                ParamType = br.GetASCII(paramTypeOffset);
            }
        }

        if (ParamType == "SOUND_CUTSCENE_PARAM_ST" && ParamdefDataVersion == 6 && RowSize == 32)
        {
            FixupERSoundCutsceneParam();
        }
    }

    protected override void Write(BinaryWriterEx bw)
    {
        if (AppliedParamdef == null)
            throw new InvalidOperationException("Params cannot be written without applying a paramdef.");

        bw.BigEndian = BigEndian;

        bw.ReserveUInt32("StringsOffset");
        if ((Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset)) ||
            Format2D.HasFlag(FormatFlags1.LongDataOffset))
        {
            bw.WriteInt16(0);
        }
        else
        {
            bw.ReserveUInt16("DataStart");
        }

        bw.WriteInt16(Unk06);
        bw.WriteInt16(ParamdefDataVersion);

        if (Rows.Count > ushort.MaxValue)
            throw new OverflowException(
                $"Param \"{AppliedParamdef.ParamType}\" has more than {ushort.MaxValue} rows and cannot be saved.");
        bw.WriteUInt16((ushort)Rows.Count);

        if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
        {
            bw.WriteInt32(0);
            bw.ReserveInt64("ParamTypeOffset");
            bw.WritePattern(0x14, 0x00);
        }
        else
        {
            // This padding heuristic isn't completely accurate, not that it matters
            bw.WriteFixStr(ParamType, 0x20, (byte)(Format2D.HasFlag(FormatFlags1.Flag01) ? 0x20 : 0x00));
        }

        bw.WriteByte((byte)(BigEndian ? 0xFF : 0x00));
        bw.WriteByte((byte)Format2D);
        bw.WriteByte((byte)Format2E);
        bw.WriteByte(ParamdefFormatVersion);
        if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
        {
            bw.ReserveUInt32("DataStart");
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
        }
        else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
        {
            bw.ReserveInt64("DataStart");
            bw.WriteInt64(0);
        }

        // Write row headers
        for (var i = 0; i < Rows.Count; i++)
        {
            if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                bw.WriteInt32(Rows[i].ID);
                bw.WriteInt32(0);
                bw.ReserveInt64($"RowOffset{i}");
                bw.ReserveInt64($"NameOffset{i}");
            }
            else
            {
                bw.WriteInt32(Rows[i].ID);
                bw.ReserveUInt32($"RowOffset{i}");
                bw.ReserveUInt32($"NameOffset{i}");
            }
        }

        // This is probably pretty stupid
        if (Format2D == FormatFlags1.Flag01)
            bw.WritePattern(0x20, 0x00);

        if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
            bw.FillUInt32("DataStart", (uint)bw.Position);
        else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            bw.FillInt64("DataStart", bw.Position);
        else
            bw.FillUInt16("DataStart", (ushort)bw.Position);

        // Write row data
        for (var i = 0; i < Rows.Count; i++)
        {
            if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
                bw.FillInt64($"RowOffset{i}", bw.Position);
            else
                bw.FillUInt32($"RowOffset{i}", (uint)bw.Position);

            Span<byte> data = _paramData.DataForElement(Rows[i].DataIndex);
            bw.WriteBytes(data);
        }

        bw.FillUInt32("StringsOffset", (uint)bw.Position);

        if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
        {
            bw.FillInt64("ParamTypeOffset", bw.Position);
            bw.WriteASCII(ParamType, true);
        }

        // Write row names
        var stringOffsetDictionary = new Dictionary<string, long>();

        for (var i = 0; i < Rows.Count; i++)
        {
            var rowName = Rows[i].Name ?? string.Empty;

            stringOffsetDictionary.TryGetValue(rowName, out var nameOffset);
            if (nameOffset == 0)
            {
                nameOffset = bw.Position;
                if (Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                    bw.WriteUTF16(rowName, true);
                else
                    bw.WriteShiftJIS(rowName, true);

                stringOffsetDictionary.Add(rowName, nameOffset);
            }

            if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
                bw.FillInt64($"NameOffset{i}", nameOffset);
            else
                bw.FillUInt32($"NameOffset{i}", (uint)nameOffset);
        }

        bw.WriteInt16(0); //FS Seems to end their params with an empty string
    }

    /// <summary>
    ///     A param row, which represents a single collection of values for fields specified in a paramdef. Each
    ///     row has an ID, which is usually unique but not always, and may optionally have a name. Unlike a
    ///     Soulsformats PARAM row, this row is tied to a specific instance of the Param class as the parent,
    ///     and must be cloned to the target Param instance before being added to that Param. This is because
    ///     a Row doesn't store any data on its own but merely references specific data managed by the parent
    ///     Param instance.
    /// </summary>
    public class Row
    {
        internal readonly Param Parent;
        internal uint DataIndex;

        internal Row(int id, string? name, Param parent, uint dataIndex)
        {
            ID = id;
            Name = name;
            Parent = parent;
            DataIndex = dataIndex;
        }

        /// <summary>
        ///     Creates a new empty row with all fields zeroed out. This row must then be manually added to
        ///     the parent Param.
        /// </summary>
        /// <param name="id">The ID for this row</param>
        /// <param name="name">The name for this row</param>
        /// <param name="parent">The Param that this row will be added to/associated with</param>
        public Row(int id, string? name, Param parent)
        {
            ID = id;
            Name = name;
            Parent = parent;
            DataIndex = parent._paramData.AddZeroedElement();
        }

        /// <summary>
        ///     Clones a row with all the field data copied from the existing row to the new one. This
        ///     row will have the same ID and name and must be manually added to the Param instance
        ///     associated with the row this row was cloned from.
        /// </summary>
        /// <param name="clone">The row to clone</param>
        public Row(Row clone)
        {
            Parent = clone.Parent;
            ID = clone.ID;
            Name = clone.Name;
            DataIndex = Parent._paramData.AddZeroedElement();
            Parent._paramData.CopyData(DataIndex, clone.DataIndex);
        }

        /// <summary>
        ///     Clones a row with all the field data copied from the existing row to the new one. This
        ///     row will have the same ID and name, but will be associated with the Param provided instead
        ///     of the Param associated with the original row.
        /// </summary>
        /// <param name="clone">The row to clone</param>
        /// <param name="newParent">The Param to associate this new clone with</param>
        public Row(Row clone, Param newParent)
        {
            Parent = newParent;
            ID = clone.ID;
            Name = clone.Name;
            DataIndex = Parent._paramData.AddZeroedElement();
            clone.Parent._paramData.CopyData(Parent._paramData, DataIndex, clone.DataIndex);
        }

        /// <summary>
        ///     The ID for this row. Should be a unique identifier in theory but in practice it isn't always
        ///     guaranteed to be unique.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     The optional name for this row. These are usually stripped in many Fromsoft games but
        ///     community created names exist for many different games/params.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     The enumerable list of columns representing the fields in this row
        /// </summary>
        public IEnumerable<Column> Columns => Parent.Columns;

        /// <summary>
        ///     Gets a list of cells for this row that allow getting/setting the value for the fields in
        ///     this row. This is provided to assist migrating Soulsformats based code but this has allocation
        ///     overhead and the column API should be used instead.
        /// </summary>
        public IReadOnlyList<Cell> Cells
        {
            get
            {
                var cells = new List<Cell>(Columns.Count());
                foreach (Column cell in Columns)
                {
                    cells.Add(new Cell(this, cell));
                }

                return cells;
            }
        }

        /// <summary>
        ///     The paramdef for this row.
        /// </summary>
        public PARAMDEF? Def => Parent.AppliedParamdef;

        /// <summary>
        ///     Gets a cell for a specific field. This is mainly provided for API compatibility with SoulsFormats
        ///     and is not recommended for new code.
        /// </summary>
        /// <param name="field">The name of the field to look up</param>
        public Cell? this[string field]
        {
            get
            {
                Column? cell = Columns.FirstOrDefault(cell => cell.Def.InternalName == field);
                return cell != null ? new Cell(this, cell) : null;
            }
        }

        /// <summary>
        ///     Gets a cell for a specific field in this row using a column. Using the column API directly is
        ///     the recommended way to access and modify field data, but there are cases where having a value
        ///     type cell handle can be useful.
        /// </summary>
        /// <param name="field">The column representing the field to create a cell for</param>
        public Cell this[Column field] => new(this, field);

        /// <summary>
        ///     Compares if the ID and field data of this row is the same as another row. The other row does
        ///     not need to have the same parent Param as this row.
        /// </summary>
        /// <param name="other">The row to compare to</param>
        /// <returns>True if the IDs of the two rows match and the field data is byte equal.</returns>
        public bool DataEquals(Row? other)
        {
            if (other == null)
                return false;
            if (ID != other.ID)
                return false;

            return Parent._paramData.DataEquals(other.Parent._paramData, other.DataIndex, DataIndex);
        }

        ~Row()
        {
            Parent._paramData.RemoveAt(DataIndex);
        }

        /// <summary>
        ///     Gets a cell handle from a name or throw an exception if the field name is not found. This is
        ///     not very efficient and it is not recommended to use this in a hot code path.
        /// </summary>
        /// <param name="field">The field to look for</param>
        /// <returns>A cell handle for the field</returns>
        /// <exception cref="ArgumentException">Throws if field name doesn't exist</exception>
        public Cell GetCellHandleOrThrow(string field)
        {
            Column? cell = Columns.FirstOrDefault(cell => cell.Def.InternalName == field);
            if (cell == null)
                throw new ArgumentException();
            return new Cell(this, cell);
        }
    }

    /// <summary>
    ///     Similar to the SoulsFormats PARAM Cell class, a cell represents a specific field within a specific
    ///     row. In other words, it represents the cross of a specified row and column. This is meant to be a
    ///     mostly drop in replacement for existing code using the SF PARAM Cell API, but it is now a value type
    ///     that is created on demand and therefore more lightweight. For new code, usage of this is not
    ///     recommended.
    /// </summary>
    public readonly struct Cell
    {
        private readonly Row _row;
        private readonly Column _column;

        internal Cell(Row row, Column column)
        {
            _row = row;
            _column = column;
        }

        /// <summary>
        ///     Property to get and set the value of this cell using the appropriate type for the field.
        /// </summary>
        public object Value
        {
            get => _column.GetValue(_row);
            set => _column.SetValue(_row, value);
        }

        /// <summary>
        ///     Alternate accessor to set the value of the cell in cases where properties can't be used.
        /// </summary>
        /// <param name="value">The value to set the cell to</param>
        public void SetValue(object value)
        {
            _column.SetValue(_row, value);
        }

        public bool IsNull()
        {
            if (_column == null)
                return true;

            return false;
        }

        /// <summary>
        ///     The paramdef field definition for this cell
        /// </summary>
        public PARAMDEF.Field Def => _column.Def;
    }

    /// <summary>
    ///     Represents a Column (param field) in the param. Unlike the Soulsformats Cell, which represents a
    ///     value for a specific param field in a specific row, a column isn't associated with any specific row
    ///     but is instead used as an accessor to a specific paramdef field in any given row.
    /// </summary>
    public class Column
    {
        private readonly uint _arrayLength;
        private readonly uint _bitOffset;
        private readonly int _bitSize;

        private readonly uint _byteOffset;

        internal Column(PARAMDEF.Field def, uint byteOffset, uint arrayLength = 1)
        {
            Def = def;
            _byteOffset = byteOffset;
            _arrayLength = arrayLength;
            _bitSize = -1;
            _bitOffset = 0;
            ValueType = TypeForParamDefType(def.DisplayType, arrayLength > 1);
        }

        internal Column(PARAMDEF.Field def, uint byteOffset, int bitSize, uint bitOffset)
        {
            Def = def;
            _byteOffset = byteOffset;
            _arrayLength = 1;
            _bitSize = bitSize;
            _bitOffset = bitOffset;
            ValueType = TypeForParamDefType(def.DisplayType, false);
        }

        /// <summary>
        ///     The paramdef field definition associated with this column
        /// </summary>
        public PARAMDEF.Field Def { get; }

        /// <summary>
        ///     The C# datatype that is used to represent the data in this column
        /// </summary>
        public Type ValueType { get; private set; }

        private static Type TypeForParamDefType(PARAMDEF.DefType type, bool isArray)
        {
            switch (type)
            {
                case PARAMDEF.DefType.s8:
                    return typeof(sbyte);
                case PARAMDEF.DefType.u8:
                    return typeof(byte);
                case PARAMDEF.DefType.s16:
                    return typeof(short);
                case PARAMDEF.DefType.u16:
                    return typeof(ushort);
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    return typeof(int);
                case PARAMDEF.DefType.u32:
                    return typeof(uint);
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    return typeof(float);
                case PARAMDEF.DefType.f64:
                    return typeof(double);
                case PARAMDEF.DefType.dummy8:
                    return isArray ? typeof(byte[]) : typeof(byte);
                case PARAMDEF.DefType.fixstr:
                case PARAMDEF.DefType.fixstrW:
                    return typeof(string);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        ///     Gets the value of the field associated with this column from the specified row.
        /// </summary>
        /// <param name="row">The row to access the field data from.</param>
        /// <returns>The value for the field with the type ValueType</returns>
        public object GetValue(Row row)
        {
            StridedByteArray data = row.Parent._paramData;
            switch (Def.DisplayType)
            {
                case PARAMDEF.DefType.s8:
                    return data.ReadValueAtElementOffset<sbyte>(row.DataIndex, _byteOffset);
                case PARAMDEF.DefType.s16:
                    return data.ReadValueAtElementOffset<short>(row.DataIndex, _byteOffset);
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    return data.ReadValueAtElementOffset<int>(row.DataIndex, _byteOffset);
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    return data.ReadValueAtElementOffset<float>(row.DataIndex, _byteOffset);
                case PARAMDEF.DefType.f64:
                    return data.ReadValueAtElementOffset<double>(row.DataIndex, _byteOffset);
                case PARAMDEF.DefType.u8:
                case PARAMDEF.DefType.dummy8:
                    if (_arrayLength > 1)
                    {
                        return data.ReadByteArrayAtElementOffset(row.DataIndex, _byteOffset, _arrayLength);
                    }

                    var value8 = data.ReadValueAtElementOffset<byte>(row.DataIndex, _byteOffset);
                    if (_bitSize != -1)
                        value8 = (byte)((value8 >> (int)_bitOffset) & (0xFF >> (8 - _bitSize)));
                    return value8;
                case PARAMDEF.DefType.u16:
                    var value16 = data.ReadValueAtElementOffset<ushort>(row.DataIndex, _byteOffset);
                    if (_bitSize != -1)
                        value16 = (ushort)((value16 >> (int)_bitOffset) & (0xFFFF >> (16 - _bitSize)));
                    return value16;
                case PARAMDEF.DefType.u32:
                    var value32 = data.ReadValueAtElementOffset<uint>(row.DataIndex, _byteOffset);
                    if (_bitSize != -1)
                        value32 = (value32 >> (int)_bitOffset) & (0xFFFFFFFF >> (32 - _bitSize));
                    return value32;
                case PARAMDEF.DefType.fixstr:
                    return data.ReadFixedStringAtElementOffset(row.DataIndex, _byteOffset, _arrayLength);
                case PARAMDEF.DefType.fixstrW:
                    return data.ReadFixedStringWAtElementOffset(row.DataIndex, _byteOffset, _arrayLength);
                default:
                    throw new NotImplementedException($"Unsupported field type: {Def.DisplayType}");
            }
        }

        /// <summary>
        ///     Gets the value of the field associated with this column in the specified row.
        /// </summary>
        /// <param name="row">The row to store the field data to.</param>
        /// <param name="value">The value for the field with the type ValueType</param>
        public void SetValue(Row row, object value)
        {
            StridedByteArray data = row.Parent._paramData;
            switch (Def.DisplayType)
            {
                case PARAMDEF.DefType.s8:
                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, (sbyte)value);
                    break;
                case PARAMDEF.DefType.s16:
                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, (short)value);
                    break;
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, (int)value);
                    break;
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, (float)value);
                    break;
                case PARAMDEF.DefType.f64:
                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, (double)value);
                    break;
                case PARAMDEF.DefType.u8:
                case PARAMDEF.DefType.dummy8:
                    if (_arrayLength > 1)
                    {
                        data.WriteByteArrayAtElementOffset(row.DataIndex, _byteOffset, (byte[])value);
                    }
                    else
                    {
                        var value8 = (byte)value;
                        if (_bitSize != -1)
                        {
                            var o8 = data.ReadValueAtElementOffset<byte>(row.DataIndex, _byteOffset);
                            var mask8 = (byte)((0xFF >> (8 - _bitSize)) << (int)_bitOffset);
                            value8 = (byte)((o8 & ~mask8) | ((value8 << (int)_bitOffset) & mask8));
                        }

                        data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, value8);
                    }

                    break;
                case PARAMDEF.DefType.u16:
                    var value16 = (ushort)value;
                    if (_bitSize != -1)
                    {
                        var o16 = data.ReadValueAtElementOffset<ushort>(row.DataIndex, _byteOffset);
                        var mask16 = (ushort)((0xFFFF >> (16 - _bitSize)) << (int)_bitOffset);
                        value16 = (ushort)((o16 & ~mask16) | ((value16 << (int)_bitOffset) & mask16));
                    }

                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, value16);
                    break;
                case PARAMDEF.DefType.u32:
                    var value32 = (uint)value;
                    if (_bitSize != -1)
                    {
                        var o32 = data.ReadValueAtElementOffset<uint>(row.DataIndex, _byteOffset);
                        var mask32 = (0xFFFFFFFF >> (32 - _bitSize)) << (int)_bitOffset;
                        value32 = (o32 & ~mask32) | ((value32 << (int)_bitOffset) & mask32);
                    }

                    data.WriteValueAtElementOffset(row.DataIndex, _byteOffset, value32);
                    break;
                case PARAMDEF.DefType.fixstr:
                    data.WriteFixedStringAtElementOffset(row.DataIndex, _byteOffset, (string)value, _arrayLength);
                    break;
                case PARAMDEF.DefType.fixstrW:
                    data.WriteFixedStringWAtElementOffset(row.DataIndex, _byteOffset, (string)value, _arrayLength);
                    break;
                default:
                    throw new NotImplementedException($"Unsupported field type: {Def.DisplayType}");
            }
        }

        /// <summary>
        ///     Gets the byte offset of the data for this field in the raw byte data for the row.
        /// </summary>
        public uint GetByteOffset()
        {
            return _byteOffset;
        }

        /// <summary>
        ///     Gets the bit offset of the data for this bitfield in the raw byte data for the row.
        /// </summary>
        public uint GetBitOffset()
        {
            return _bitOffset;
        }
    }
}
