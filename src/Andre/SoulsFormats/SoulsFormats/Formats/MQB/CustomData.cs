using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using SoulsFormats;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class CustomData
        {
            /// <summary>
            /// The different supported data types.
            /// </summary>
            public enum DataType : uint
            {
                /// <summary>
                /// A true or false boolean.
                /// </summary>
                Bool = 1,

                /// <summary>
                /// An signed 8-bit integer.
                /// </summary>
                SByte = 2,

                /// <summary>
                /// An unsigned 8-bit integer.
                /// </summary>
                Byte = 3,

                /// <summary>
                /// A signed 16-bit integer.
                /// </summary>
                Short = 4,

                // I would assume at this point?
                /// <summary>
                /// An unsigned 16-bit integer.
                /// </summary>
                // UShort = 5,

                /// <summary>
                /// A signed 32-bit integer.
                /// </summary>
                Int = 6,

                /// <summary>
                /// An unsigned 32-bit integer.
                /// </summary>
                UInt = 7,

                /// <summary>
                /// A 32-bit floating point number.
                /// </summary>
                Float = 8,

                /// <summary>
                /// A string.
                /// </summary>
                String = 10,

                /// <summary>
                /// A custom data type.
                /// </summary>
                Custom = 11,

                /// <summary>
                /// A color with each member as an unsigned byte.<br/>
                /// Only seen with 3 members so far.
                /// </summary>
                Color = 13,

                /// <summary>
                /// A color with each member as an int.<br/>
                /// Only seen with 4 members so far.
                /// </summary>
                IntColor = 17,

                /// <summary>
                /// A vector.
                /// </summary>
                Vector = 18
            }

            /// <summary>
            /// The name of this <see cref="CustomData"/> entry.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The type of the data contained within this <see cref="CustomData"/>.
            /// </summary>
            public DataType Type { get; set; }

            /// <summary>
            /// The total number of members in a data type.
            /// <para/>
            /// Example:<br/>
            /// 2 for Vector2<br/>
            /// 3 for Vector3<br/>
            /// 4 for Vector4
            /// </summary>
            public int MemberCount { get; set; }

            /// <summary>
            /// The value of the data.
            /// </summary>
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
                MemberCount = br.ReadInt32();

                long valueOffset = br.Position;
                switch (Type)
                {
                    // Actual value
                    case DataType.Bool: Value = br.ReadBoolean(); break;
                    case DataType.SByte: Value = br.ReadSByte(); break;
                    case DataType.Byte: Value = br.ReadByte(); break;
                    case DataType.Short: Value = br.ReadInt16(); break;
                    case DataType.Int: Value = br.ReadInt32(); break;
                    case DataType.UInt: Value = br.ReadUInt32(); break;
                    case DataType.Float: Value = br.ReadSingle(); break;

                    // Length of value fields plus padding
                    case DataType.String:
                    case DataType.Custom:
                    case DataType.Color:
                    case DataType.IntColor:
                    case DataType.Vector: Value = br.ReadInt32(); break;
                    default: throw new NotImplementedException($"Unimplemented custom data type: {Type}");
                }

                long valueEndOffset = br.Position;

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

                if (Type == DataType.String || Type == DataType.Custom || Type == DataType.Color || Type == DataType.IntColor || Type == DataType.Vector)
                {
                    int length = (int)Value;
                    if (Type == DataType.String)
                    {
                        if (length == 0 || length % 0x10 != 0)
                            throw new InvalidDataException($"Unexpected custom {nameof(DataType.String)} {nameof(length)}: {length}");
                        Value = br.ReadFixStrW(length);
                    }
                    else if (Type == DataType.Custom)
                    {
                        if (length % 4 != 0)
                            throw new InvalidDataException($"Unexpected custom {nameof(DataType.Custom)} {nameof(length)}: {length}");
                        Value = br.ReadBytes(length);
                    }
                    else if (Type == DataType.Color)
                    {
                        if (MemberCount != 3)
                            throw new NotImplementedException($"{nameof(MemberCount)} {MemberCount} not implemented for: {nameof(DataType.Color)}");
                        if (length != 4)
                            throw new InvalidDataException($"Unexpected custom {nameof(DataType.Color)} {nameof(length)}: {length}");

                        valueOffset = br.Position;
                        Value = Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
                        valueEndOffset = br.Position;
                        br.AssertByte(0);
                    }
                    else if (Type == DataType.IntColor)
                    {
                        if (MemberCount != 4)
                            throw new NotImplementedException($"{nameof(MemberCount)} {MemberCount} not implemented for: {nameof(DataType.IntColor)}");
                        if (length != 20)
                            throw new InvalidDataException($"Unexpected custom {nameof(DataType.IntColor)} {nameof(length)}: {length}");

                        valueOffset = br.Position;
                        int r = br.ReadInt32();
                        int g = br.ReadInt32();
                        int b = br.ReadInt32();
                        int a = br.ReadInt32();
                        Value = Color.FromArgb(a, r, g, b);
                        valueEndOffset = br.Position;
                        br.AssertInt32(0);
                    }
                    else if (Type == DataType.Vector)
                    {
                        if (length != (MemberCount * 4) + 4)
                            throw new InvalidDataException($"Unexpected custom {nameof(DataType.Vector)} {nameof(length)}: {length}");

                        valueOffset = br.Position;
                        switch (MemberCount)
                        {
                            case 2:
                                Value = br.ReadVector2();
                                break;
                            case 3:
                                Value = br.ReadVector3();
                                break;
                            case 4:
                                Value = br.ReadVector4();
                                break;
                            default:
                                throw new NotImplementedException($"{nameof(MemberCount)} {MemberCount} not implemented for: {nameof(DataType.Vector)}");
                        }

                        valueEndOffset = br.Position;
                        br.AssertInt32(0);
                    }
                }

                Sequences = new List<Sequence>(sequenceCount);
                if (sequenceCount > 0)
                {
                    br.StepIn(sequencesOffset);
                    {
                        for (int i = 0; i < sequenceCount; i++)
                            Sequences.Add(new Sequence(br, valueOffset, valueEndOffset));
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(MemberCount);

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
                    length = MemberCount + 1;
                }
                else if (Type == DataType.IntColor)
                {
                    length = (MemberCount * 4) + 4;
                }
                else if (Type == DataType.Vector)
                {
                    length = (MemberCount * 4) + 4;
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
                    case DataType.Color:
                    case DataType.IntColor:
                    case DataType.Vector: bw.WriteInt32(length); break;
                    default: throw new NotImplementedException($"Unimplemented custom {nameof(DataType)}: {Type}");
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
                    if (MemberCount != 3)
                        throw new NotSupportedException($"{nameof(MemberCount)} {MemberCount} not expected or implemented for: {nameof(DataType.Color)}");

                    var color = (Color)Value;
                    valueOffset = bw.Position;
                    bw.WriteByte(color.R);
                    bw.WriteByte(color.G);
                    bw.WriteByte(color.B);
                    bw.WriteByte(0);
                }
                else if (Type == DataType.IntColor)
                {
                    if (MemberCount != 4)
                        throw new NotSupportedException($"{nameof(MemberCount)} {MemberCount} not expected or implemented for: {nameof(DataType.IntColor)}");

                    var color = (Color)Value;
                    valueOffset = bw.Position;
                    bw.WriteInt32(color.R);
                    bw.WriteInt32(color.G);
                    bw.WriteInt32(color.B);
                    bw.WriteInt32(color.A);
                    bw.WriteInt32(0);
                }
                else if (Type == DataType.Vector)
                {
                    switch (MemberCount)
                    {
                        case 2:
                            var vector2 = (Vector2)Value;
                            valueOffset = bw.Position;
                            bw.WriteVector2(vector2);
                            break;
                        case 3:
                            var vector3 = (Vector3)Value;
                            valueOffset = bw.Position;
                            bw.WriteVector3(vector3);
                            break;
                        case 4:
                            var vector4 = (Vector4)Value;
                            valueOffset = bw.Position;
                            bw.WriteVector4(vector4);
                            break;
                        default:
                            throw new NotImplementedException($"{nameof(MemberCount)} {MemberCount} not implemented for: {nameof(DataType.Vector)}");
                    }

                    // Vector padding
                    bw.WriteInt32(0);
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

                internal Sequence(BinaryReaderEx br, long parentValueOffset, long parentValueEndOffset)
                {
                    br.AssertInt32(0x1C); // Sequence size
                    int pointCount = br.ReadInt32();
                    ValueType = br.ReadEnum32<DataType>();
                    // PointType 0 is only ever used once in ER s35_00_0000.mqb, but otherwise seems identical to PointType 1
                    PointType = br.AssertInt32([0, 1, 2]);
                    br.AssertInt32((PointType == 0 || PointType == 1) ? 0x10 : 0x18); // Point size
                    int pointsOffset = br.ReadInt32();
                    int valueOffset = br.ReadInt32();

                    if (ValueType == DataType.Byte || ValueType == DataType.Float || ValueType == DataType.UInt)
                    {
                        if (valueOffset < parentValueOffset && valueOffset >= parentValueEndOffset)
                            throw new InvalidDataException($"Unexpected value offset {valueOffset:X}/{parentValueOffset:X} for value type {ValueType}.");
                        ValueIndex = valueOffset - (int)parentValueOffset;
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
                    bw.WriteInt32((PointType == 0 || PointType == 1) ? 0x10 : 0x18);
                    bw.ReserveInt32($"PointsOffset[{customDataIndex}:{sequenceIndex}]");
                    if (ValueType == DataType.Byte || ValueType == DataType.Float || ValueType == DataType.UInt)
                        bw.WriteInt32((int)parentValueOffset + ValueIndex);
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
                            case DataType.UInt: Value = br.ReadUInt32(); break;
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
                            case DataType.UInt: bw.WriteUInt32((uint)Value); break;
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
