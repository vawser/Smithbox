using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class CustomData
        {
            public enum DataType : uint
            {
                Bool = 1,
                SByte = 2,
                Byte = 3,
                Short = 4,
                Int = 6,
                UInt = 7,
                Float = 8,
                String = 10,
                Custom = 11,
                Color = 13,
            }

            public string Name { get; set; }

            public DataType Type { get; set; }

            public int Unk44 { get; set; }

            public object Value { get; set; }

            public List<Sequence> Sequences { get; set; }

            public CustomData()
            {
                Name = "";
                Type = DataType.Int;
                Value = 0;
                Sequences = new List<Sequence>();
            }

            internal CustomData(BinaryReaderEx br)
            {
                Name = br.ReadFixStrW(0x40);
                Type = br.ReadEnum32<DataType>();
                br.AssertInt32(Type == DataType.Color ? 3 : 0);

                long valueOffset = br.Position;
                switch (Type)
                {
                    case DataType.Bool: Value = br.ReadBoolean(); break;
                    case DataType.SByte: Value = br.ReadSByte(); break;
                    case DataType.Byte: Value = br.ReadByte(); break;
                    case DataType.Short: Value = br.ReadInt16(); break;
                    case DataType.Int: Value = br.ReadInt32(); break;
                    case DataType.UInt: Value = br.ReadUInt32(); break;
                    case DataType.Float: Value = br.ReadSingle(); break;
                    case DataType.String:
                    case DataType.Custom:
                    case DataType.Color: Value = br.ReadInt32(); break;
                    default: throw new NotImplementedException($"Unimplemented custom data type: {Type}");
                }

                if (Type == DataType.Bool || Type == DataType.SByte || Type == DataType.Byte)
                {
                    br.AssertByte(0);
                    br.AssertInt16(0);
                }
                else if (Type == DataType.Short)
                {
                    br.AssertInt16(0);
                }

                br.AssertInt32(0);
                int sequencesOffset = br.ReadInt32();
                int sequenceCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                if (Type == DataType.String || Type == DataType.Color || Type == DataType.Custom)
                {
                    int length = (int)Value;
                    if (Type == DataType.String)
                    {
                        if (length == 0 || length % 0x10 != 0)
                            throw new InvalidDataException($"Unexpected custom data string length: {length}");
                        Value = br.ReadFixStrW(length);
                    }
                    else if (Type == DataType.Custom)
                    {
                        if (length % 4 != 0)
                            throw new InvalidDataException($"Unexpected custom data custom length: {length}");
                        Value = br.ReadBytes(length);
                    }
                    else if (Type == DataType.Color)
                    {
                        if (length != 4)
                            throw new InvalidDataException($"Unexpected custom data color length: {length}");
                        valueOffset = br.Position;
                        Value = Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
                        br.AssertByte(0);
                    }
                }

                Sequences = new List<Sequence>(sequenceCount);
                if (sequenceCount > 0)
                {
                    br.StepIn(sequencesOffset);
                    {
                        for (int i = 0; i < sequenceCount; i++)
                            Sequences.Add(new Sequence(br, valueOffset));
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(Type == DataType.Color ? 3 : 0);

                int length = -1;
                if (Type == DataType.String)
                {
                    length = SFEncoding.UTF16.GetByteCount((string)Value + '\0');
                    if (length % 0x10 != 0)
                        length += 0x10 - length % 0x10;
                }
                else if (Type == DataType.Custom)
                {
                    length = ((byte[])Value).Length;
                    if (length % 4 != 0)
                        throw new InvalidDataException($"Unexpected custom data custom length: {length}");
                }
                else if (Type == DataType.Color)
                {
                    length = 4;
                }

                long valueOffset = bw.Position;
                switch (Type)
                {
                    case DataType.Bool: bw.WriteBoolean((bool)Value); break;
                    case DataType.SByte: bw.WriteSByte((sbyte)Value); break;
                    case DataType.Byte: bw.WriteByte((byte)Value); break;
                    case DataType.Short: bw.WriteInt16((short)Value); break;
                    case DataType.Int: bw.WriteInt32((int)Value); break;
                    case DataType.UInt: bw.WriteUInt32((uint)Value); break;
                    case DataType.Float: bw.WriteSingle((float)Value); break;
                    case DataType.String:
                    case DataType.Custom:
                    case DataType.Color: bw.WriteInt32(length); break;
                    default: throw new NotImplementedException($"Unimplemented custom data type: {Type}");
                }

                if (Type == DataType.Bool || Type == DataType.SByte || Type == DataType.Byte)
                {
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                }
                else if (Type == DataType.Short)
                {
                    bw.WriteInt16(0);
                }

                // This is probably wrong for the 64-bit format
                bw.WriteInt32(0);
                bw.ReserveInt32($"SequencesOffset[{allCustomData.Count}]");
                bw.WriteInt32(Sequences.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                if (Type == DataType.String)
                {
                    bw.WriteFixStrW((string)Value, length, 0x00);
                }
                else if (Type == DataType.Custom)
                {
                    bw.WriteBytes((byte[])Value);
                }
                else if (Type == DataType.Color)
                {
                    var color = (Color)Value;
                    valueOffset = bw.Position;
                    bw.WriteByte(color.R);
                    bw.WriteByte(color.G);
                    bw.WriteByte(color.B);
                    bw.WriteByte(0);
                }

                allCustomData.Add(this);
                customDataValueOffsets.Add(valueOffset);
            }

            internal void WriteSequences(BinaryWriterEx bw, int customDataIndex, long valueOffset)
            {
                if (Sequences.Count == 0)
                {
                    bw.FillInt32($"SequencesOffset[{customDataIndex}]", 0);
                }
                else
                {
                    bw.FillInt32($"SequencesOffset[{customDataIndex}]", (int)bw.Position);
                    for (int i = 0; i < Sequences.Count; i++)
                        Sequences[i].Write(bw, customDataIndex, i, valueOffset);
                }
            }

            internal void WriteSequencePoints(BinaryWriterEx bw, int customDataIndex)
            {
                for (int i = 0; i < Sequences.Count; i++)
                    Sequences[i].WritePoints(bw, customDataIndex, i);
            }

            public class Sequence
            {
                public DataType ValueType { get; set; }

                public int PointType { get; set; }

                public int ValueIndex { get; set; }

                public List<Point> Points { get; set; }

                public Sequence()
                {
                    ValueType = DataType.Byte;
                    PointType = 1;
                    Points = new List<Point>();
                }

                internal Sequence(BinaryReaderEx br, long parentValueOffset)
                {
                    br.AssertInt32(0x1C); // Sequence size
                    int pointCount = br.ReadInt32();
                    ValueType = br.ReadEnum32<DataType>();
                    PointType = br.AssertInt32([1, 2]);
                    br.AssertInt32(PointType == 1 ? 0x10 : 0x18); // Point size
                    int pointsOffset = br.ReadInt32();
                    int valueOffset = br.ReadInt32();

                    if (ValueType == DataType.Byte)
                    {
                        if (valueOffset < parentValueOffset || valueOffset > parentValueOffset + 2)
                            throw new InvalidDataException($"Unexpected value offset {valueOffset:X}/{parentValueOffset:X} for value type {ValueType}.");
                        ValueIndex = valueOffset - (int)parentValueOffset;
                    }
                    else if (ValueType == DataType.Float)
                    {
                        if (valueOffset != parentValueOffset)
                            throw new InvalidDataException($"Unexpected value offset {valueOffset:X}/{parentValueOffset:X} for value type {ValueType}.");
                        ValueIndex = 0;
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported sequence value type: {ValueType}");
                    }

                    br.StepIn(pointsOffset);
                    {
                        Points = new List<Point>(pointCount);
                        for (int i = 0; i < pointCount; i++)
                            Points.Add(new Point(br, ValueType, PointType));
                    }
                    br.StepOut();
                }

                internal void Write(BinaryWriterEx bw, int customDataIndex, int sequenceIndex, long parentValueOffset)
                {
                    bw.WriteInt32(0x1C);
                    bw.WriteInt32(Points.Count);
                    bw.WriteUInt32((uint)ValueType);
                    bw.WriteInt32(PointType);
                    bw.WriteInt32(PointType == 1 ? 0x10 : 0x18);
                    bw.ReserveInt32($"PointsOffset[{customDataIndex}:{sequenceIndex}]");
                    if (ValueType == DataType.Byte)
                        bw.WriteInt32((int)parentValueOffset + ValueIndex);
                    else if (ValueType == DataType.Float)
                        bw.WriteInt32((int)parentValueOffset);
                }

                internal void WritePoints(BinaryWriterEx bw, int customDataIndex, int sequenceIndex)
                {
                    bw.FillInt32($"PointsOffset[{customDataIndex}:{sequenceIndex}]", (int)bw.Position);
                    foreach (Point point in Points)
                        point.Write(bw, ValueType, PointType);
                }

                public class Point
                {
                    public object Value { get; set; }

                    public int Unk08 { get; set; }

                    public float Unk10 { get; set; }

                    public float Unk14 { get; set; }

                    public Point()
                    {
                        Value = (byte)0;
                    }

                    internal Point(BinaryReaderEx br, DataType valueType, int pointType)
                    {
                        switch (valueType)
                        {
                            case DataType.Byte: Value = br.ReadByte(); break;
                            case DataType.Float: Value = br.ReadSingle(); break;
                            default: throw new NotSupportedException($"Unsupported sequence value type: {valueType}");
                        }

                        if (valueType == DataType.Byte)
                        {
                            br.AssertInt16(0);
                            br.AssertByte(0);
                        }

                        br.AssertInt32(0);
                        Unk08 = br.ReadInt32();
                        br.AssertInt32(0);

                        // I suspect these are also variable type, but in the few instances of pointType 2
                        // with valueType 3, they're all just 0.
                        if (pointType == 2)
                        {
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                        }
                    }

                    internal void Write(BinaryWriterEx bw, DataType valueType, int pointType)
                    {
                        switch (valueType)
                        {
                            case DataType.Byte: bw.WriteByte((byte)Value); break;
                            case DataType.Float: bw.WriteSingle((float)Value); break;
                            default: throw new NotSupportedException($"Unsupported sequence value type: {valueType}");
                        }

                        if (valueType == DataType.Byte)
                        {
                            bw.WriteInt16(0);
                            bw.WriteByte(0);
                        }

                        bw.WriteInt32(0);
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(0);

                        if (pointType == 2)
                        {
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                        }
                    }
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
