using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class PARAM
    {
        /// <summary>
        /// One row in a param file.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// The paramdef that describes this row.
            /// </summary>
            public PARAMDEF Def { get; set; }

            /// <summary>
            /// The ID number of this row.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public IReadOnlyList<Cell> Cells { get; private set; }

            internal long DataOffset;

            /// <summary>
            /// Creates a new row based on the given paramdef with default values.
            /// </summary>
            public Row(int id, string name, PARAMDEF paramdef)
            {
                Def = paramdef;
                ID = id;
                Name = name;

                var cells = new Cell[paramdef.Fields.Count];
                for (int i = 0; i < paramdef.Fields.Count; i++)
                {
                    PARAMDEF.Field field = paramdef.Fields[i];
                    object value = ParamUtil.ConvertDefaultValue(field);
                    cells[i] = new Cell(field, value);
                }
                Cells = cells;
            }

            /// <summary>
            /// Copy constructor for a row. Does not add to the param.
            /// </summary>
            /// <param name="clone">The row that is being copied</param>
            public Row(Row clone)
            {
                Def = clone.Def;
                ID = clone.ID;
                Name = clone.Name;
                var cells = new List<Cell>(clone.Cells.Count);

                foreach (var cell in clone.Cells)
                {
                    cells.Add(new Cell(cell));
                }
                Cells = cells;
}

            internal Row(BinaryReaderEx br, PARAM parent, ref long actualStringsOffset)
            {
                long nameOffset;
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                {
                    ID = br.ReadInt32();
                    br.ReadInt32(); // I would like to assert 0, but some of the generatordbglocation params in DS2S have garbage here
                    DataOffset = br.ReadInt64();
                    nameOffset = br.ReadInt64();
                }
                else
                {
                    ID = br.ReadInt32();
                    DataOffset = br.ReadUInt32();
                    nameOffset = br.ReadUInt32();
                }

                if (nameOffset != 0)
                {
                    if (actualStringsOffset == 0 || nameOffset < actualStringsOffset)
                        actualStringsOffset = nameOffset;

                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                        Name = br.GetUTF16(nameOffset);
                    else
                        Name = br.GetShiftJIS(nameOffset);
                }
            }

            internal void ReadCells(BinaryReaderEx br, PARAMDEF paramdef, ulong regulationVersion)
            {
                // In case someone decides to add new rows before applying the paramdef (please don't do that)
                if (DataOffset == 0)
                    return;

                Def = paramdef;

                br.Position = DataOffset;
                var cells = new Cell[paramdef.Fields.Count];

                int bitOffset = -1;
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                ulong bitValue = 0; // This is ulong so checkOrphanedBits doesn't fail on offsets of 32
                const int BIT_VALUE_SIZE = 64;

                void checkOrphanedBits()
                {
                    if (bitOffset != -1 && (bitValue >> bitOffset) != 0)
                    {
                        throw new InvalidDataException($"Invalid paramdef {paramdef.ParamType}; bits would be lost before +0x{br.Position - DataOffset:X} in row {ID}.");
                    }
                }

                for (int i = 0; i < paramdef.Fields.Count; i++)
                {
                    // For version aware PARAMDEFs, skip fields that don't exist in the specified version
                    if (paramdef.VersionAware && !paramdef.Fields[i].IsValidForRegulationVersion(regulationVersion))
                        continue;
                    
                    PARAMDEF.Field field = paramdef.Fields[i];
                    object value = null;
                    PARAMDEF.DefType type = field.DisplayType;

                    if (type == PARAMDEF.DefType.s8)
                        value = br.ReadSByte();
                    else if (type == PARAMDEF.DefType.s16)
                        value = br.ReadInt16();
                    else if (type == PARAMDEF.DefType.s32 || type == PARAMDEF.DefType.b32)
                        value = br.ReadInt32();
                    else if (type == PARAMDEF.DefType.f32 || type == PARAMDEF.DefType.angle32)
                        value = br.ReadSingle();
                    else if (type == PARAMDEF.DefType.f64)
                        value = br.ReadDouble();
                    else if (type == PARAMDEF.DefType.fixstr)
                        value = br.ReadFixStr(field.ArrayLength);
                    else if (type == PARAMDEF.DefType.fixstrW)
                        value = br.ReadFixStrW(field.ArrayLength * 2);
                    else if (ParamUtil.IsBitType(type))
                    {
                        if (field.BitSize == -1)
                        {
                            if (type == PARAMDEF.DefType.u8)
                                value = br.ReadByte();
                            else if (type == PARAMDEF.DefType.u16)
                                value = br.ReadUInt16();
                            else if (type == PARAMDEF.DefType.u32)
                                value = br.ReadUInt32();
                            else if (type == PARAMDEF.DefType.dummy8)
                                value = br.ReadBytes(field.ArrayLength);
                        }
                    }
                    else
                        throw new NotImplementedException($"Unsupported field type: {type}");

                    if (value != null)
                    {
                        checkOrphanedBits();
                        bitOffset = -1;
                    }
                    else
                    {
                        PARAMDEF.DefType newBitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                        int bitLimit = ParamUtil.GetBitLimit(newBitType);

                        if (field.BitSize == 0)
                            throw new NotImplementedException($"Bit size 0 is not supported.");
                        if (field.BitSize > bitLimit)
                            throw new InvalidDataException($"Bit size {field.BitSize} is too large to fit in type {newBitType}.");

                        if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit)
                        {
                            checkOrphanedBits();
                            bitOffset = 0;
                            bitType = newBitType;
                            if (bitType == PARAMDEF.DefType.u8)
                                bitValue = br.ReadByte();
                            else if (bitType == PARAMDEF.DefType.u16)
                                bitValue = br.ReadUInt16();
                            else if (bitType == PARAMDEF.DefType.u32)
                                bitValue = br.ReadUInt32();
                        }

                        ulong shifted = bitValue << (BIT_VALUE_SIZE - field.BitSize - bitOffset) >> (BIT_VALUE_SIZE - field.BitSize);
                        bitOffset += field.BitSize;
                        if (bitType == PARAMDEF.DefType.u8)
                            value = (byte)shifted;
                        else if (bitType == PARAMDEF.DefType.u16)
                            value = (ushort)shifted;
                        else if (bitType == PARAMDEF.DefType.u32)
                            value = (uint)shifted;
                    }

                    cells[i] = new Cell(field, value);
                }

                checkOrphanedBits();
                Cells = cells;
            }

            internal void WriteHeader(BinaryWriterEx bw, PARAM parent, int i)
            {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                {
                    bw.WriteInt32(ID);
                    bw.WriteInt32(0);
                    bw.ReserveInt64($"RowOffset{i}");
                    bw.ReserveInt64($"NameOffset{i}");
                }
                else
                {
                    bw.WriteInt32(ID);
                    bw.ReserveUInt32($"RowOffset{i}");
                    bw.ReserveUInt32($"NameOffset{i}");
                }
            }

            internal void WriteCells(BinaryWriterEx bw, PARAM parent, int index)
            {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"RowOffset{index}", bw.Position);
                else
                    bw.FillUInt32($"RowOffset{index}", (uint)bw.Position);

                int bitOffset = -1;
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                ulong bitValue = 0;
                const int BIT_VALUE_SIZE = 64;

                for (int i = 0; i < Cells.Count; i++)
                {
                    Cell cell = Cells[i];
                    object value = cell.Value;
                    PARAMDEF.Field field = cell.Def;
                    PARAMDEF.DefType type = field.DisplayType;

                    if (type == PARAMDEF.DefType.s8)
                        bw.WriteSByte((sbyte)value);
                    else if (type == PARAMDEF.DefType.s16)
                        bw.WriteInt16((short)value);
                    else if (type == PARAMDEF.DefType.s32 || type == PARAMDEF.DefType.b32)
                        bw.WriteInt32((int)value);
                    else if (type == PARAMDEF.DefType.f32 || type == PARAMDEF.DefType.angle32)
                        bw.WriteSingle((float)value);
                    else if (type == PARAMDEF.DefType.f64)
                        bw.WriteDouble((double)value);
                    else if (type == PARAMDEF.DefType.fixstr)
                        bw.WriteFixStr((string)value, field.ArrayLength);
                    else if (type == PARAMDEF.DefType.fixstrW)
                        bw.WriteFixStrW((string)value, field.ArrayLength * 2);
                    else if (ParamUtil.IsBitType(type))
                    {
                        if (field.BitSize == -1)
                        {
                            if (type == PARAMDEF.DefType.u8)
                                bw.WriteByte((byte)value);
                            else if (type == PARAMDEF.DefType.u16)
                                bw.WriteUInt16((ushort)value);
                            else if (type == PARAMDEF.DefType.u32)
                                bw.WriteUInt32((uint)value);
                            else if (type == PARAMDEF.DefType.dummy8)
                                bw.WriteBytes((byte[])value);
                        }
                        else
                        {
                            if (bitOffset == -1)
                            {
                                bitOffset = 0;
                                bitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                                bitValue = 0;
                            }

                            uint shifted = 0;
                            if (bitType == PARAMDEF.DefType.u8)
                                shifted = (byte)value;
                            else if (bitType == PARAMDEF.DefType.u16)
                                shifted = (ushort)value;
                            else if (bitType == PARAMDEF.DefType.u32)
                                shifted = (uint)value;
                            // Shift left first to clear any out-of-range bits
                            shifted = shifted << (BIT_VALUE_SIZE - field.BitSize) >> (BIT_VALUE_SIZE - field.BitSize - bitOffset);
                            bitValue |= shifted;
                            bitOffset += field.BitSize;

                            bool write = false;
                            if (i == Cells.Count - 1)
                            {
                                write = true;
                            }
                            else
                            {
                                PARAMDEF.Field nextField = Cells[i + 1].Def;
                                PARAMDEF.DefType nextType = nextField.DisplayType;
                                int bitLimit = ParamUtil.GetBitLimit(bitType);
                                if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > bitLimit
                                    || (nextType == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : nextType) != bitType)
                                {
                                    write = true;
                                }
                            }

                            if (write)
                            {
                                bitOffset = -1;
                                if (bitType == PARAMDEF.DefType.u8)
                                    bw.WriteByte((byte)bitValue);
                                else if (bitType == PARAMDEF.DefType.u16)
                                    bw.WriteUInt16((ushort)bitValue);
                                else if (bitType == PARAMDEF.DefType.u32)
                                    bw.WriteUInt32((uint)bitValue);
                            }
                        }
                    }
                    else
                        throw new NotImplementedException($"Unsupported field type: {type}");
                }
            }

            internal void WriteName(BinaryWriterEx bw, PARAM parent, int i)
            {
                if (Name == null)
                    Name = string.Empty;
                parent.StringOffsetDictionary.TryGetValue(Name, out long nameOffset);
                
                if (nameOffset == 0) 
                {
                    nameOffset = bw.Position;
                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                        bw.WriteUTF16(Name, true);
                    else
                        bw.WriteShiftJIS(Name, true);
                    
                    parent.StringOffsetDictionary.Add(Name, nameOffset);
                }

                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"NameOffset{i}", nameOffset);
                else
                    bw.FillUInt32($"NameOffset{i}", (uint) nameOffset);
            }

            /// <summary>
            /// Returns a string representation of the row.
            /// </summary>
            public override string ToString()
            {
                return $"{ID} {Name}";
            }

            /// <summary>
            /// Returns the first cell in the row with the given internal name.
            /// </summary>
            public Cell this[string name] => Cells.FirstOrDefault(cell => cell.Def.InternalName == name);
        }
    }
}
