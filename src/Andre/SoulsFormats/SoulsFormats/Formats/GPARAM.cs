using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

// TKGP's latest version of GPARAM
#nullable disable
namespace SoulsFormats
{
    public class GPARAM : SoulsFile<GPARAM>
    {
        public GPARAM.GparamVersion Version { get; set; }

        public bool Unk0d { get; set; }

        public int Count14 { get; set; }

        public List<GPARAM.Param> Params { get; set; }

        public byte[] Data30 { get; set; }

        public List<GPARAM.UnkParamExtra> UnkParamExtras { get; set; }

        public float Unk40 { get; set; }

        public float Unk50 { get; set; }

        public GPARAM()
        {
            this.Unk0d = true;
            this.Params = new List<GPARAM.Param>();
        }

        public GPARAM Clone()
        {
            return (GPARAM)MemberwiseClone();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 4L && br.GetASCII(0L, 8) == "f\0i\0l\0t\0";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("f\0i\0l\0t\0");
            this.Version = br.ReadEnum32<GPARAM.GparamVersion>();
            int num1 = (int)br.AssertByte(new byte[1]);
            this.Unk0d = br.ReadBoolean();
            int num2 = (int)br.AssertInt16(new short[1]);
            int num3 = br.ReadInt32();
            this.Count14 = br.ReadInt32();
            GPARAM.BaseOffsets baseOffsets;
            baseOffsets.ParamOffsets = br.ReadInt32();
            baseOffsets.Params = br.ReadInt32();
            baseOffsets.FieldOffsets = br.ReadInt32();
            baseOffsets.Fields = br.ReadInt32();
            baseOffsets.Values = br.ReadInt32();
            baseOffsets.ValueIds = br.ReadInt32();
            baseOffsets.Unk30 = br.ReadInt32();
            int capacity = br.ReadInt32();
            baseOffsets.ParamExtras = br.ReadInt32();
            baseOffsets.ParamExtraIds = br.ReadInt32();
            this.Unk40 = br.ReadSingle();
            baseOffsets.ParamCommentsOffsets = br.ReadInt32();
            baseOffsets.CommentOffsets = br.ReadInt32();
            baseOffsets.Comments = br.ReadInt32();
            if (this.Version >= GPARAM.GparamVersion.V5)
                this.Unk50 = br.ReadSingle();
            int[] int32s1 = br.GetInt32s((long)baseOffsets.ParamOffsets, num3);
            this.Params = new List<GPARAM.Param>(num3);
            foreach (int num4 in int32s1)
            {
                br.Position = (long)(baseOffsets.Params + num4);
                this.Params.Add(new GPARAM.Param(br, this.Version, baseOffsets));
            }
            br.Position = (long)baseOffsets.Unk30;
            this.Data30 = br.ReadBytes(baseOffsets.ParamExtras - baseOffsets.Unk30);
            br.Position = (long)baseOffsets.ParamExtras;
            this.UnkParamExtras = new List<GPARAM.UnkParamExtra>(capacity);
            for (int index = 0; index < capacity; ++index)
                this.UnkParamExtras.Add(new GPARAM.UnkParamExtra(br, this.Version, baseOffsets));
            int[] int32s2 = br.GetInt32s((long)baseOffsets.ParamCommentsOffsets, num3);
            for (int index = 0; index < num3; ++index)
            {
                int offset = baseOffsets.CommentOffsets + int32s2[index];
                int num5 = ((index >= num3 - 1 ? baseOffsets.Comments : baseOffsets.CommentOffsets + int32s2[index + 1]) - offset) / 4;
                int[] int32s3 = br.GetInt32s((long)offset, num5);
                List<string> stringList = new List<string>(num5);
                foreach (int num6 in int32s3)
                    stringList.Add(br.GetUTF16((long)(baseOffsets.Comments + num6)));
                this.Params[index].Comments = stringList;
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            GPARAM.BaseOffsets baseOffsets = new GPARAM.BaseOffsets();
            bw.BigEndian = false;
            bw.WriteUTF16("filt");
            bw.WriteUInt32((uint)this.Version);
            bw.WriteByte((byte)0);
            bw.WriteBoolean(this.Unk0d);
            bw.WriteInt16((short)0);
            bw.WriteInt32(this.Params.Count);
            bw.WriteInt32(this.Count14);
            bw.ReserveInt32("ParamOffsetsBase");
            bw.ReserveInt32("ParamsBase");
            bw.ReserveInt32("FieldOffsetsBase");
            bw.ReserveInt32("FieldsBase");
            bw.ReserveInt32("ValuesBase");
            bw.ReserveInt32("ValueIdsBase");
            bw.ReserveInt32("Unk30Base");
            bw.WriteInt32(this.UnkParamExtras.Count);
            bw.ReserveInt32("ParamExtrasBase");
            bw.ReserveInt32("ParamExtraIdsBase");
            bw.WriteSingle(this.Unk40);
            bw.ReserveInt32("ParamCommentsOffsetsBase");
            bw.ReserveInt32("CommentOffsetsBase");
            bw.ReserveInt32("CommentsBase");
            if (this.Version >= GPARAM.GparamVersion.V5)
                bw.WriteSingle(this.Unk50);
            baseOffsets.ParamOffsets = (int)bw.Position;
            bw.FillInt32("ParamOffsetsBase", baseOffsets.ParamOffsets);
            DefaultInterpolatedStringHandler interpolatedStringHandler;
            for (int index = 0; index < this.Params.Count; ++index)
            {
                BinaryWriterEx binaryWriterEx = bw;
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
                interpolatedStringHandler.AppendLiteral("ParamOffset[");
                interpolatedStringHandler.AppendFormatted<int>(index);
                interpolatedStringHandler.AppendLiteral("]");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx.ReserveInt32(stringAndClear);
            }
            baseOffsets.Params = (int)bw.Position;
            bw.FillInt32("ParamsBase", baseOffsets.Params);
            for (int index = 0; index < this.Params.Count; ++index)
            {
                BinaryWriterEx binaryWriterEx = bw;
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
                interpolatedStringHandler.AppendLiteral("ParamOffset[");
                interpolatedStringHandler.AppendFormatted<int>(index);
                interpolatedStringHandler.AppendLiteral("]");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                int num = (int)bw.Position - baseOffsets.Params;
                binaryWriterEx.FillInt32(stringAndClear, num);
                this.Params[index].Write(bw, index);
                bw.Pad(4);
            }
            baseOffsets.FieldOffsets = (int)bw.Position;
            bw.FillInt32("FieldOffsetsBase", baseOffsets.FieldOffsets);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteFieldOffsets(bw, baseOffsets, index);
            baseOffsets.Fields = (int)bw.Position;
            bw.FillInt32("FieldsBase", baseOffsets.Fields);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteFields(bw, baseOffsets, index);
            baseOffsets.Values = (int)bw.Position;
            bw.FillInt32("ValuesBase", baseOffsets.Values);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteValues(bw, baseOffsets, index);
            baseOffsets.ValueIds = (int)bw.Position;
            bw.FillInt32("ValueIdsBase", baseOffsets.ValueIds);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteValueIds(bw, this.Version, baseOffsets, index);
            baseOffsets.Unk30 = (int)bw.Position;
            bw.FillInt32("Unk30Base", baseOffsets.Unk30);
            bw.WriteBytes(this.Data30);
            bw.Pad(4);
            baseOffsets.ParamExtras = (int)bw.Position;
            bw.FillInt32("ParamExtrasBase", baseOffsets.ParamExtras);
            for (int index = 0; index < this.UnkParamExtras.Count; ++index)
                this.UnkParamExtras[index].Write(bw, this.Version, index);
            baseOffsets.ParamExtraIds = (int)bw.Position;
            bw.FillInt32("ParamExtraIdsBase", baseOffsets.ParamExtraIds);
            for (int index = 0; index < this.UnkParamExtras.Count; ++index)
                this.UnkParamExtras[index].WriteIds(bw, baseOffsets, index);
            baseOffsets.ParamCommentsOffsets = (int)bw.Position;
            bw.FillInt32("ParamCommentsOffsetsBase", baseOffsets.ParamCommentsOffsets);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteCommentOffsetsOffset(bw, index);
            baseOffsets.CommentOffsets = (int)bw.Position;
            bw.FillInt32("CommentOffsetsBase", baseOffsets.CommentOffsets);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteCommentOffsets(bw, baseOffsets, index);
            baseOffsets.Comments = (int)bw.Position;
            bw.FillInt32("CommentsBase", baseOffsets.Comments);
            for (int index = 0; index < this.Params.Count; ++index)
                this.Params[index].WriteComments(bw, baseOffsets, index);
        }

        public enum FieldType : byte
        {
            Sbyte = 1,
            Short = 2,
            Int = 3,
            Byte = 5,
            Uint = 7,
            Float = 9,
            Bool = 11, // 0x0B
            Vec2 = 12, // 0x0C
            Vec3 = 13, // 0x0D
            Vec4 = 14, // 0x0E
            Color = 15, // 0x0F
        }

        public interface IField
        {
            string Key { get; set; }

            string Name { get; set; }

            IReadOnlyList<GPARAM.IFieldValue> Values { get; }

            internal static GPARAM.IField Read(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
            {
                GPARAM.FieldType enum8 = br.GetEnum8<GPARAM.FieldType>(br.Position + 8L);
                switch (enum8)
                {
                    case GPARAM.FieldType.Sbyte:
                        return (GPARAM.IField)new GPARAM.SbyteField(br, version, baseOffsets);
                    case GPARAM.FieldType.Short:
                        return (GPARAM.IField)new GPARAM.ShortField(br, version, baseOffsets);
                    case GPARAM.FieldType.Int:
                        return (GPARAM.IField)new GPARAM.IntField(br, version, baseOffsets);
                    case GPARAM.FieldType.Byte:
                        return (GPARAM.IField)new GPARAM.ByteField(br, version, baseOffsets);
                    case GPARAM.FieldType.Uint:
                        return (GPARAM.IField)new GPARAM.UintField(br, version, baseOffsets);
                    case GPARAM.FieldType.Float:
                        return (GPARAM.IField)new GPARAM.FloatField(br, version, baseOffsets);
                    case GPARAM.FieldType.Bool:
                        return (GPARAM.IField)new GPARAM.BoolField(br, version, baseOffsets);
                    case GPARAM.FieldType.Vec2:
                        return (GPARAM.IField)new GPARAM.Vector2Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Vec3:
                        return (GPARAM.IField)new GPARAM.Vector3Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Vec4:
                        return (GPARAM.IField)new GPARAM.Vector4Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Color:
                        return (GPARAM.IField)new GPARAM.ColorField(br, version, baseOffsets);
                    default:
                        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
                        interpolatedStringHandler.AppendLiteral("Unknown field type: ");
                        interpolatedStringHandler.AppendFormatted<GPARAM.FieldType>(enum8);
                        throw new NotImplementedException(interpolatedStringHandler.ToStringAndClear());
                }
            }
        }

        internal interface IFieldWriteable
        {
            void Write(BinaryWriterEx bw, int paramIndex, int fieldIndex);

            void WriteValues(
              BinaryWriterEx bw,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex);

            void WriteValueIds(
              BinaryWriterEx bw,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex);
        }

        public abstract class Field<T> : GPARAM.IField, GPARAM.IFieldWriteable
        {
            public string Key { get; set; }

            public string Name { get; set; }

            public List<GPARAM.FieldValue<T>> Values { get; set; }

            IReadOnlyList<GPARAM.IFieldValue> GPARAM.IField.Values
            {
                get => (IReadOnlyList<GPARAM.IFieldValue>)this.Values;
            }

            public Field()
            {
                this.Key = "";
                this.Name = "";
                this.Values = new List<GPARAM.FieldValue<T>>();
            }

            public override string ToString()
            {
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
                interpolatedStringHandler.AppendFormatted(this.Key);
                interpolatedStringHandler.AppendLiteral(" [");
                interpolatedStringHandler.AppendFormatted<int>(this.Values.Count);
                interpolatedStringHandler.AppendLiteral("]");
                return interpolatedStringHandler.ToStringAndClear();
            }

            private protected abstract GPARAM.FieldType Type { get; }

            private protected Field(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
            {
                int num1 = br.ReadInt32();
                int num2 = br.ReadInt32();
                int num3 = (int)br.AssertByte((byte)this.Type);
                byte capacity = br.ReadByte();
                int num4 = (int)br.AssertInt16(new short[1]);
                this.Key = br.ReadUTF16();
                this.Name = br.ReadUTF16();
                br.Position = (long)(baseOffsets.Values + num1);
                T[] objArray = new T[(int)capacity];
                for (int index = 0; index < (int)capacity; ++index)
                    objArray[index] = this.ReadValue(br);
                br.Position = (long)(baseOffsets.ValueIds + num2);
                this.Values = new List<GPARAM.FieldValue<T>>((int)capacity);
                for (int index = 0; index < (int)capacity; ++index)
                    this.Values.Add(new GPARAM.FieldValue<T>(br, version, objArray[index]));
            }

            private protected abstract T ReadValue(BinaryReaderEx br);

            void GPARAM.IFieldWriteable.Write(BinaryWriterEx bw, int paramIndex, int fieldIndex)
            {
                BinaryWriterEx binaryWriterEx1 = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 2);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]Field[");
                interpolatedStringHandler.AppendFormatted<int>(fieldIndex);
                interpolatedStringHandler.AppendLiteral("]ValuesOffset");
                string stringAndClear1 = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx1.ReserveInt32(stringAndClear1);
                BinaryWriterEx binaryWriterEx2 = bw;
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]Field[");
                interpolatedStringHandler.AppendFormatted<int>(fieldIndex);
                interpolatedStringHandler.AppendLiteral("]ValueIdsOffset");
                string stringAndClear2 = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx2.ReserveInt32(stringAndClear2);
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(checked((byte)this.Values.Count));
                bw.WriteInt16((short)0);
                bw.WriteUTF16(this.Key, true);
                bw.WriteUTF16(this.Name, true);
            }

            void GPARAM.IFieldWriteable.WriteValues(
              BinaryWriterEx bw,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex)
            {
                BinaryWriterEx binaryWriterEx = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 2);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]Field[");
                interpolatedStringHandler.AppendFormatted<int>(fieldIndex);
                interpolatedStringHandler.AppendLiteral("]ValuesOffset");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                int num = (int)bw.Position - baseOffsets.Values;
                binaryWriterEx.FillInt32(stringAndClear, num);
                foreach (GPARAM.FieldValue<T> fieldValue in this.Values)
                    this.WriteValue(bw, fieldValue.Value);
            }

            private protected abstract void WriteValue(BinaryWriterEx bw, T value);

            void GPARAM.IFieldWriteable.WriteValueIds(
              BinaryWriterEx bw,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex)
            {
                BinaryWriterEx binaryWriterEx = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]Field[");
                interpolatedStringHandler.AppendFormatted<int>(fieldIndex);
                interpolatedStringHandler.AppendLiteral("]ValueIdsOffset");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                int num = (int)bw.Position - baseOffsets.ValueIds;
                binaryWriterEx.FillInt32(stringAndClear, num);
                foreach (GPARAM.FieldValue<T> fieldValue in this.Values)
                    fieldValue.Write(bw, version);
            }
        }

        public class SbyteField : GPARAM.Field<sbyte>
        {
            public SbyteField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Sbyte;

            internal SbyteField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override sbyte ReadValue(BinaryReaderEx br) => br.ReadSByte();

            private protected override void WriteValue(BinaryWriterEx bw, sbyte value)
            {
                bw.WriteSByte(value);
            }
        }

        public class ShortField : GPARAM.Field<short>
        {
            public ShortField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Short;

            internal ShortField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override short ReadValue(BinaryReaderEx br) => br.ReadInt16();

            private protected override void WriteValue(BinaryWriterEx bw, short value)
            {
                bw.WriteInt16(value);
            }
        }

        public class IntField : GPARAM.Field<int>
        {
            public IntField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Int;

            internal IntField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override int ReadValue(BinaryReaderEx br) => br.ReadInt32();

            private protected override void WriteValue(BinaryWriterEx bw, int value)
            {
                bw.WriteInt32(value);
            }
        }

        public class ByteField : GPARAM.Field<byte>
        {
            public ByteField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Byte;

            internal ByteField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override byte ReadValue(BinaryReaderEx br) => br.ReadByte();

            private protected override void WriteValue(BinaryWriterEx bw, byte value)
            {
                bw.WriteByte(value);
            }
        }

        public class UintField : GPARAM.Field<uint>
        {
            public UintField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Uint;

            internal UintField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override uint ReadValue(BinaryReaderEx br) => br.ReadUInt32();

            private protected override void WriteValue(BinaryWriterEx bw, uint value)
            {
                bw.WriteUInt32(value);
            }
        }

        public class FloatField : GPARAM.Field<float>
        {
            public FloatField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Float;

            internal FloatField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override float ReadValue(BinaryReaderEx br) => br.ReadSingle();

            private protected override void WriteValue(BinaryWriterEx bw, float value)
            {
                bw.WriteSingle(value);
            }
        }

        public class BoolField : GPARAM.Field<bool>
        {
            public BoolField()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Bool;

            internal BoolField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override bool ReadValue(BinaryReaderEx br) => br.ReadBoolean();

            private protected override void WriteValue(BinaryWriterEx bw, bool value)
            {
                bw.WriteBoolean(value);
            }
        }

        public class Vector2Field : GPARAM.Field<Vector2>
        {
            public Vector2Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Vec2;

            internal Vector2Field(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override Vector2 ReadValue(BinaryReaderEx br)
            {
                Vector2 vector2 = br.ReadVector2();
                br.AssertInt64(new long[1]);
                return vector2;
            }

            private protected override void WriteValue(BinaryWriterEx bw, Vector2 value)
            {
                bw.WriteVector2(value);
                bw.WriteInt64(0L);
            }
        }

        public class Vector3Field : GPARAM.Field<Vector3>
        {
            public Vector3Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Vec3;

            internal Vector3Field(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override Vector3 ReadValue(BinaryReaderEx br)
            {
                Vector3 vector3 = br.ReadVector3();
                br.AssertInt32(new int[1]);
                return vector3;
            }

            private protected override void WriteValue(BinaryWriterEx bw, Vector3 value)
            {
                bw.WriteVector3(value);
                bw.WriteInt32(0);
            }
        }

        public class Vector4Field : GPARAM.Field<Vector4>
        {
            public Vector4Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Vec4;

            internal Vector4Field(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override Vector4 ReadValue(BinaryReaderEx br) => br.ReadVector4();

            private protected override void WriteValue(BinaryWriterEx bw, Vector4 value)
            {
                bw.WriteVector4(value);
            }
        }

        public class ColorField : GPARAM.Field<Color>
        {
            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Color;

            public ColorField()
            {
            }

            internal ColorField(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
              : base(br, version, baseOffsets)
            {
            }

            private protected override Color ReadValue(BinaryReaderEx br) => br.ReadRGBA();

            private protected override void WriteValue(BinaryWriterEx bw, Color value)
            {
                bw.WriteRGBA(value);
            }
        }

        public interface IFieldValue
        {
            int Id { get; set; }

            float Unk04 { get; set; }

            object Value { get; }
        }

        public class FieldValue<T> : GPARAM.IFieldValue
        {
            public int Id { get; set; }

            public float Unk04 { get; set; }

            public T Value { get; set; }

            object GPARAM.IFieldValue.Value => (object)this.Value;

            public FieldValue()
            {
            }

            public override string ToString()
            {
                if ((double)this.Unk04 != 0.0)
                {
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 3);
                    interpolatedStringHandler.AppendFormatted<int>(this.Id);
                    interpolatedStringHandler.AppendLiteral(" (");
                    interpolatedStringHandler.AppendFormatted<float>(this.Unk04);
                    interpolatedStringHandler.AppendLiteral(") = ");
                    interpolatedStringHandler.AppendFormatted<T>(this.Value);
                    return interpolatedStringHandler.ToStringAndClear();
                }
                DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(3, 2);
                interpolatedStringHandler1.AppendFormatted<int>(this.Id);
                interpolatedStringHandler1.AppendLiteral(" = ");
                interpolatedStringHandler1.AppendFormatted<T>(this.Value);
                return interpolatedStringHandler1.ToStringAndClear();
            }

            internal FieldValue(BinaryReaderEx br, GPARAM.GparamVersion version, T value)
            {
                this.Id = br.ReadInt32();
                if (version >= GPARAM.GparamVersion.V5)
                    this.Unk04 = br.ReadSingle();
                this.Value = value;
            }

            internal void Write(BinaryWriterEx bw, GPARAM.GparamVersion version)
            {
                bw.WriteInt32(this.Id);
                if (version < GPARAM.GparamVersion.V5)
                    return;
                bw.WriteSingle(this.Unk04);
            }
        }

        public enum GparamVersion : uint
        {
            V3 = 3,
            V5 = 5,
        }

        internal struct BaseOffsets
        {
            public int ParamOffsets;
            public int Params;
            public int FieldOffsets;
            public int Fields;
            public int Values;
            public int ValueIds;
            public int Unk30;
            public int ParamExtras;
            public int ParamExtraIds;
            public int ParamCommentsOffsets;
            public int CommentOffsets;
            public int Comments;
        }

        public class Param
        {
            public List<GPARAM.IField> Fields { get; set; }

            public string Key { get; set; }

            public string Name { get; set; }

            public List<string> Comments { get; set; }

            public Param()
            {
                this.Fields = new List<GPARAM.IField>();
                this.Key = "";
                this.Name = "";
                this.Comments = new List<string>();
            }

            public override string ToString()
            {
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
                interpolatedStringHandler.AppendFormatted(this.Key);
                interpolatedStringHandler.AppendLiteral(" [");
                interpolatedStringHandler.AppendFormatted<int>(this.Fields.Count);
                interpolatedStringHandler.AppendLiteral("]");
                return interpolatedStringHandler.ToStringAndClear();
            }

            internal Param(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
            {
                int num1 = br.ReadInt32();
                int num2 = br.ReadInt32();
                this.Key = br.ReadUTF16();
                this.Name = br.ReadUTF16();
                int[] int32s = br.GetInt32s((long)(baseOffsets.FieldOffsets + num2), num1);
                this.Fields = new List<GPARAM.IField>(num1);
                foreach (int num3 in int32s)
                {
                    br.Position = (long)(baseOffsets.Fields + num3);
                    this.Fields.Add(GPARAM.IField.Read(br, version, baseOffsets));
                }
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(this.Fields.Count);
                BinaryWriterEx binaryWriterEx = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(index);
                interpolatedStringHandler.AppendLiteral("]FieldOffsetsOffset");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx.ReserveInt32(stringAndClear);
                bw.WriteUTF16(this.Key, true);
                bw.WriteUTF16(this.Name, true);
            }

            internal void WriteFieldOffsets(
              BinaryWriterEx bw,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex)
            {
                BinaryWriterEx binaryWriterEx1 = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]FieldOffsetsOffset");
                string stringAndClear1 = interpolatedStringHandler.ToStringAndClear();
                int num = (int)bw.Position - baseOffsets.FieldOffsets;
                binaryWriterEx1.FillInt32(stringAndClear1, num);
                for (int index = 0; index < this.Fields.Count; ++index)
                {
                    BinaryWriterEx binaryWriterEx2 = bw;
                    interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
                    interpolatedStringHandler.AppendLiteral("Param[");
                    interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                    interpolatedStringHandler.AppendLiteral("]Field[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]Offset");
                    string stringAndClear2 = interpolatedStringHandler.ToStringAndClear();
                    binaryWriterEx2.ReserveInt32(stringAndClear2);
                }
            }

            internal void WriteFields(BinaryWriterEx bw, GPARAM.BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < this.Fields.Count; ++index)
                {
                    BinaryWriterEx binaryWriterEx = bw;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
                    interpolatedStringHandler.AppendLiteral("Param[");
                    interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                    interpolatedStringHandler.AppendLiteral("]Field[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]Offset");
                    string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    int num = (int)bw.Position - baseOffsets.Fields;
                    binaryWriterEx.FillInt32(stringAndClear, num);
                    ((GPARAM.IFieldWriteable)this.Fields[index]).Write(bw, paramIndex, index);
                    bw.Pad(4);
                }
            }

            internal void WriteValues(BinaryWriterEx bw, GPARAM.BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < this.Fields.Count; ++index)
                {
                    ((GPARAM.IFieldWriteable)this.Fields[index]).WriteValues(bw, baseOffsets, paramIndex, index);
                    bw.Pad(4);
                }
            }

            internal void WriteValueIds(
              BinaryWriterEx bw,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex)
            {
                for (int index = 0; index < this.Fields.Count; ++index)
                    ((GPARAM.IFieldWriteable)this.Fields[index]).WriteValueIds(bw, version, baseOffsets, paramIndex, index);
            }

            internal void WriteCommentOffsetsOffset(BinaryWriterEx bw, int paramIndex)
            {
                BinaryWriterEx binaryWriterEx = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]CommentOffsetsOffset");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx.ReserveInt32(stringAndClear);
            }

            internal void WriteCommentOffsets(
              BinaryWriterEx bw,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex)
            {
                BinaryWriterEx binaryWriterEx1 = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
                interpolatedStringHandler.AppendLiteral("Param[");
                interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                interpolatedStringHandler.AppendLiteral("]CommentOffsetsOffset");
                string stringAndClear1 = interpolatedStringHandler.ToStringAndClear();
                int num = (int)bw.Position - baseOffsets.CommentOffsets;
                binaryWriterEx1.FillInt32(stringAndClear1, num);
                for (int index = 0; index < this.Comments.Count; ++index)
                {
                    BinaryWriterEx binaryWriterEx2 = bw;
                    interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
                    interpolatedStringHandler.AppendLiteral("Param[");
                    interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                    interpolatedStringHandler.AppendLiteral("]Comment[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]Offset");
                    string stringAndClear2 = interpolatedStringHandler.ToStringAndClear();
                    binaryWriterEx2.ReserveInt32(stringAndClear2);
                }
            }

            internal void WriteComments(
              BinaryWriterEx bw,
              GPARAM.BaseOffsets baseOffsets,
              int paramIndex)
            {
                for (int index = 0; index < this.Comments.Count; ++index)
                {
                    BinaryWriterEx binaryWriterEx = bw;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
                    interpolatedStringHandler.AppendLiteral("Param[");
                    interpolatedStringHandler.AppendFormatted<int>(paramIndex);
                    interpolatedStringHandler.AppendLiteral("]Comment[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]Offset");
                    string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    int num = (int)bw.Position - baseOffsets.Comments;
                    binaryWriterEx.FillInt32(stringAndClear, num);
                    bw.WriteUTF16(this.Comments[index], true);
                    bw.Pad(4);
                }
            }
        }

        public class UnkParamExtra
        {
            // group index
            public int Unk00 { get; set; }

            public List<int> Ids { get; set; }

            public int Unk0c { get; set; }

            public UnkParamExtra() => this.Ids = new List<int>();

            internal UnkParamExtra(
              BinaryReaderEx br,
              GPARAM.GparamVersion version,
              GPARAM.BaseOffsets baseOffsets)
            {
                this.Unk00 = br.ReadInt32();
                int count = br.ReadInt32();
                int num = br.ReadInt32();
                if (version >= GPARAM.GparamVersion.V5)
                    this.Unk0c = br.ReadInt32();
                this.Ids = Enumerable.ToList<int>((IEnumerable<int>)br.GetInt32s((long)(baseOffsets.ParamExtraIds + num), count));
            }

            internal void Write(BinaryWriterEx bw, GPARAM.GparamVersion version, int index)
            {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.Ids.Count);
                BinaryWriterEx binaryWriterEx = bw;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
                interpolatedStringHandler.AppendLiteral("ParamExtra[");
                interpolatedStringHandler.AppendFormatted<int>(index);
                interpolatedStringHandler.AppendLiteral("]IdsOffset");
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                binaryWriterEx.ReserveInt32(stringAndClear);
                if (version < GPARAM.GparamVersion.V5)
                    return;
                bw.WriteInt32(this.Unk0c);
            }

            internal void WriteIds(BinaryWriterEx bw, GPARAM.BaseOffsets baseOffsets, int index)
            {
                if (this.Ids.Count == 0)
                {
                    BinaryWriterEx binaryWriterEx = bw;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
                    interpolatedStringHandler.AppendLiteral("ParamExtra[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]IdsOffset");
                    string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    binaryWriterEx.FillInt32(stringAndClear, 0);
                }
                else
                {
                    BinaryWriterEx binaryWriterEx = bw;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
                    interpolatedStringHandler.AppendLiteral("ParamExtra[");
                    interpolatedStringHandler.AppendFormatted<int>(index);
                    interpolatedStringHandler.AppendLiteral("]IdsOffset");
                    string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    int num = (int)bw.Position - baseOffsets.ParamExtraIds;
                    binaryWriterEx.FillInt32(stringAndClear, num);
                    bw.WriteInt32s((IList<int>)this.Ids);
                }
            }
        }
    }
}
