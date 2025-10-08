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

            // Treated as int for now
            Unk_0x4 = 4,
            Unk_0x6 = 6, 
            Unk_0x8 = 8, 
            Unk_0xA = 10, 
            Unk_0x14 = 20,

            // x10 - 16
            Unk_0x10 = 16,
            // x11 - 17
            Unk_0x11 = 17,
            // x12 - 18
            Unk_0x12 = 18,
            // x13 - 19
            Unk_0x13 = 19,
            // x15 - 21
            Unk_0x15 = 21,
            // x16 - 22
            Unk_0x16 = 22,
            // x17 - 23
            Unk_0x17 = 23,
            // x18 - 24
            Unk_0x18 = 24,
            // x1A - 26
            Unk_0x1A = 26,
            // x1B - 27
            Unk_0x1B = 27,
            // x1C - 28
            Unk_0x1C = 28,
            // x1D - 29
            Unk_0x1D = 29,
            // x1E - 30
            Unk_0x1E = 30,
            // x1F - 31
            Unk_0x1F = 31,
            // x20 - 32
            Unk_0x20 = 32,
            // x24 - 36
            Unk_0x24 = 36,
            // x28 - 40
            Unk_0x28 = 40,
            // x29 - 41
            Unk_0x29 = 41,
            // x2A - 42
            Unk_0x2A = 42,
            // x2C - 44
            Unk_0x2C = 44,
            // x2D - 45
            Unk_0x2D = 45,
            // x30 - 48
            Unk_0x30 = 48,
            // x31 - 49
            Unk_0x31 = 49,
            // x69 - 105
            Unk_0x69 = 105,
            // x6C - 108
            Unk_0x6C = 108,
            // x78 - 124
            Unk_0x78 = 120,
            // x7C - 124
            Unk_0x7C = 124,
            // x7F - 127
            Unk_0x7F = 127,
            // x80 - 128
            Unk_0x80 = 128,
            // x80 - 130
            Unk_0x82 = 130,
            // x8B - 139
            Unk_0x8B = 139,
            // x91 - 157
            Unk_0x91 = 145,
            // x9D - 157
            Unk_0x9D = 157,
            // xA3 - 163
            Unk_0xA3 = 163
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

                    case GPARAM.FieldType.Unk_0x4:
                        return (GPARAM.IField)new GPARAM.Unk_4_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x6:
                        return (GPARAM.IField)new GPARAM.Unk_6_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x8:
                        return (GPARAM.IField)new GPARAM.Unk_8_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0xA:
                        return (GPARAM.IField)new GPARAM.Unk_A_Field(br, version, baseOffsets);

                    case GPARAM.FieldType.Unk_0x10:
                        return (GPARAM.IField)new GPARAM.Unk_10_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x11:
                        return (GPARAM.IField)new GPARAM.Unk_11_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x12:
                        return (GPARAM.IField)new GPARAM.Unk_12_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x13:
                        return (GPARAM.IField)new GPARAM.Unk_13_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x14:
                        return (GPARAM.IField)new GPARAM.Unk_14_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x15:
                        return (GPARAM.IField)new GPARAM.Unk_15_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x16:
                        return (GPARAM.IField)new GPARAM.Unk_16_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x17:
                        return (GPARAM.IField)new GPARAM.Unk_17_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x18:
                        return (GPARAM.IField)new GPARAM.Unk_18_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1A:
                        return (GPARAM.IField)new GPARAM.Unk_1A_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1B:
                        return (GPARAM.IField)new GPARAM.Unk_1B_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1C:
                        return (GPARAM.IField)new GPARAM.Unk_1C_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1D:
                        return (GPARAM.IField)new GPARAM.Unk_1D_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1E:
                        return (GPARAM.IField)new GPARAM.Unk_1E_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x1F:
                        return (GPARAM.IField)new GPARAM.Unk_1F_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x20:
                        return (GPARAM.IField)new GPARAM.Unk_20_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x24:
                        return (GPARAM.IField)new GPARAM.Unk_24_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x28:
                        return (GPARAM.IField)new GPARAM.Unk_28_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x29:
                        return (GPARAM.IField)new GPARAM.Unk_29_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x2A:
                        return (GPARAM.IField)new GPARAM.Unk_2A_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x2C:
                        return (GPARAM.IField)new GPARAM.Unk_2C_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x2D:
                        return (GPARAM.IField)new GPARAM.Unk_2D_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x30:
                        return (GPARAM.IField)new GPARAM.Unk_30_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x31:
                        return (GPARAM.IField)new GPARAM.Unk_31_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x69:
                        return (GPARAM.IField)new GPARAM.Unk_69_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x6C:
                        return (GPARAM.IField)new GPARAM.Unk_6C_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x78:
                        return (GPARAM.IField)new GPARAM.Unk_78_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x7C:
                        return (GPARAM.IField)new GPARAM.Unk_7C_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x7F:
                        return (GPARAM.IField)new GPARAM.Unk_7F_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x80:
                        return (GPARAM.IField)new GPARAM.Unk_80_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x82:
                        return (GPARAM.IField)new GPARAM.Unk_82_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x8B:
                        return (GPARAM.IField)new GPARAM.Unk_8B_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x91:
                        return (GPARAM.IField)new GPARAM.Unk_91_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0x9D:
                        return (GPARAM.IField)new GPARAM.Unk_9D_Field(br, version, baseOffsets);
                    case GPARAM.FieldType.Unk_0xA3:
                        return (GPARAM.IField)new GPARAM.Unk_A3_Field(br, version, baseOffsets);
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
                int num4 = br.ReadInt16();
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
        public class Unk_4_Field : GPARAM.Field<int>
        {
            public Unk_4_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x4;

            internal Unk_4_Field(
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

        public class Unk_6_Field : GPARAM.Field<int>
        {
            public Unk_6_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x6;

            internal Unk_6_Field(
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

        public class Unk_8_Field : GPARAM.Field<int>
        {
            public Unk_8_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x8;

            internal Unk_8_Field(
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
        public class Unk_A_Field : GPARAM.Field<int>
        {
            public Unk_A_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0xA;

            internal Unk_A_Field(
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
        public class Unk_10_Field : GPARAM.Field<int>
        {
            public Unk_10_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x10;

            internal Unk_10_Field(
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
        public class Unk_11_Field : GPARAM.Field<int>
        {
            public Unk_11_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x11;

            internal Unk_11_Field(
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
        public class Unk_12_Field : GPARAM.Field<int>
        {
            public Unk_12_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x12;

            internal Unk_12_Field(
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
        public class Unk_13_Field : GPARAM.Field<int>
        {
            public Unk_13_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x13;

            internal Unk_13_Field(
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

        public class Unk_14_Field : GPARAM.Field<int>
        {
            public Unk_14_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x14;

            internal Unk_14_Field(
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
        public class Unk_15_Field : GPARAM.Field<int>
        {
            public Unk_15_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x15;

            internal Unk_15_Field(
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

        public class Unk_16_Field : GPARAM.Field<int>
        {
            public Unk_16_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x16;

            internal Unk_16_Field(
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

        public class Unk_17_Field : GPARAM.Field<int>
        {
            public Unk_17_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x17;

            internal Unk_17_Field(
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

        public class Unk_18_Field : GPARAM.Field<int>
        {
            public Unk_18_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x18;

            internal Unk_18_Field(
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

        public class Unk_1A_Field : GPARAM.Field<int>
        {
            public Unk_1A_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1A;

            internal Unk_1A_Field(
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

        public class Unk_1B_Field : GPARAM.Field<int>
        {
            public Unk_1B_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1B;

            internal Unk_1B_Field(
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

        public class Unk_1C_Field : GPARAM.Field<int>
        {
            public Unk_1C_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1C;

            internal Unk_1C_Field(
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
        public class Unk_1D_Field : GPARAM.Field<int>
        {
            public Unk_1D_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1D;

            internal Unk_1D_Field(
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
        public class Unk_1E_Field : GPARAM.Field<int>
        {
            public Unk_1E_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1E;

            internal Unk_1E_Field(
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
        public class Unk_1F_Field : GPARAM.Field<int>
        {
            public Unk_1F_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x1F;

            internal Unk_1F_Field(
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

        public class Unk_20_Field : GPARAM.Field<int>
        {
            public Unk_20_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x20;

            internal Unk_20_Field(
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

        public class Unk_24_Field : GPARAM.Field<int>
        {
            public Unk_24_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x24;

            internal Unk_24_Field(
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

        public class Unk_28_Field : GPARAM.Field<int>
        {
            public Unk_28_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x28;

            internal Unk_28_Field(
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

        public class Unk_29_Field : GPARAM.Field<int>
        {
            public Unk_29_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x29;

            internal Unk_29_Field(
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

        public class Unk_2A_Field : GPARAM.Field<int>
        {
            public Unk_2A_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x2A;

            internal Unk_2A_Field(
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

        public class Unk_2C_Field : GPARAM.Field<int>
        {
            public Unk_2C_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x2C;

            internal Unk_2C_Field(
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
        public class Unk_2D_Field : GPARAM.Field<int>
        {
            public Unk_2D_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x2D;

            internal Unk_2D_Field(
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

        public class Unk_30_Field : GPARAM.Field<int>
        {
            public Unk_30_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x30;

            internal Unk_30_Field(
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
        public class Unk_31_Field : GPARAM.Field<int>
        {
            public Unk_31_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x31;

            internal Unk_31_Field(
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

        public class Unk_69_Field : GPARAM.Field<int>
        {
            public Unk_69_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x69;

            internal Unk_69_Field(
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
        public class Unk_6C_Field : GPARAM.Field<int>
        {
            public Unk_6C_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x6C;

            internal Unk_6C_Field(
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

        public class Unk_78_Field : GPARAM.Field<int>
        {
            public Unk_78_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x78;

            internal Unk_78_Field(
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

        public class Unk_7C_Field : GPARAM.Field<int>
        {
            public Unk_7C_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x7C;

            internal Unk_7C_Field(
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
        public class Unk_7F_Field : GPARAM.Field<int>
        {
            public Unk_7F_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x7F;

            internal Unk_7F_Field(
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
        public class Unk_80_Field : GPARAM.Field<int>
        {
            public Unk_80_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x80;

            internal Unk_80_Field(
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
        public class Unk_82_Field : GPARAM.Field<int>
        {
            public Unk_82_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x82;

            internal Unk_82_Field(
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

        public class Unk_8B_Field : GPARAM.Field<int>
        {
            public Unk_8B_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x8B;

            internal Unk_8B_Field(
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

        public class Unk_91_Field : GPARAM.Field<int>
        {
            public Unk_91_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x91;

            internal Unk_91_Field(
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

        public class Unk_9D_Field : GPARAM.Field<int>
        {
            public Unk_9D_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0x9D;

            internal Unk_9D_Field(
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

        public class Unk_A3_Field : GPARAM.Field<int>
        {
            public Unk_A3_Field()
            {
            }

            private protected override GPARAM.FieldType Type => GPARAM.FieldType.Unk_0xA3;

            internal Unk_A3_Field(
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
            V6 = 6
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
