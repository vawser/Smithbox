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
    /// <summary>
    /// A graphics config file used since DS2. Extensions: .fltparam, .gparam
    /// </summary>
    public class GPARAM : SoulsFile<GPARAM>
    {
        /// <summary>
        /// Indicates the format of the GPARAM.
        /// </summary>
        public GparamVersion Version { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk0D { get; set; }

        /// <summary>
        /// Unknown; in DS2, some number of entries.
        /// </summary>
        public int Count14 { get; set; }

        /// <summary>
        /// List of Graphics Parameters.
        /// </summary>
        public List<Param> Params { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte[] Data30 { get; set; }

        /// <summary>
        /// List of unknowns.
        /// </summary>
        public List<UnkParamExtra> UnkParamExtras { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public float Unk40 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public float Unk50 { get; set; }

        private bool WideStrings { get; set; }
        private bool HasComments { get; set; }

        public GPARAM()
        {
            Unk0D = true;
            Params = new List<Param>();
        }

        public GPARAM Clone()
        {
            return (GPARAM)MemberwiseClone();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            var header = br.GetASCII(0L, 4);

            var headerCheck = false;
            if (header == "filt")
            {
                headerCheck = true;
            }
            else
            {
                header = br.GetASCII(0L, 8);
                if(header == "f\0i\0l\0t\0")
                {
                    headerCheck = true;
                }
            }

            return br.Length >= 4L && headerCheck;
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            // Header
            var header = br.GetASCII(0L, 4);
            if (header == "filt")
            {
                br.AssertASCII("filt");
                WideStrings = false;
            }
            else
            {
                br.AssertASCII("f\0i\0l\0t\0");
                WideStrings = true;
            }

            Version = br.ReadEnum32<GparamVersion>();

            br.AssertByte(0);
            Unk0D = br.ReadBoolean();
            br.AssertInt16(0);

            // Offsets
            int paramCount = br.ReadInt32();
            Count14 = br.ReadInt32();

            var baseOffsets = new BaseOffsets();

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

            Unk40 = br.ReadSingle();

            baseOffsets.ParamCommentsOffsets = br.ReadInt32();
            baseOffsets.CommentOffsets = br.ReadInt32();
            baseOffsets.Comments = br.ReadInt32();

            // Sekiro and later
            if (Version >= GparamVersion.V5)
            {
                Unk50 = br.ReadSingle();
            }

            // --- Processing

            // Params
            int[] paramOffsets = br.GetInt32s(baseOffsets.ParamOffsets, paramCount);

            Params = new List<Param>(paramCount);
            foreach (int curOffset in paramOffsets)
            {
                br.Position = (baseOffsets.Params + curOffset);
                Params.Add(new Param(br, Version, baseOffsets));
            }

            // Data
            br.Position = baseOffsets.Unk30;
            Data30 = br.ReadBytes(baseOffsets.ParamExtras - baseOffsets.Unk30);

            // Extras
            br.Position = baseOffsets.ParamExtras;
            UnkParamExtras = new List<UnkParamExtra>(capacity);

            for (int index = 0; index < capacity; ++index)
            {
                UnkParamExtras.Add(new UnkParamExtra(br, Version, baseOffsets));
            }

            // Comments
            if (Version > GparamVersion.V2)
            {
                int[] paramCommentOffsets = br.GetInt32s(baseOffsets.ParamCommentsOffsets, paramCount);
                for (int index = 0; index < paramCount; ++index)
                {
                    int offset = baseOffsets.CommentOffsets + paramCommentOffsets[index];

                    int curCount = ((index >= paramCount - 1 ? baseOffsets.Comments : baseOffsets.CommentOffsets + paramCommentOffsets[index + 1]) - offset) / 4;

                    int[] stringOffsets = br.GetInt32s(offset, curCount);

                    List<string> stringList = new List<string>(curCount);

                    foreach (int curString in stringOffsets)
                    {
                        stringList.Add(br.GetUTF16(baseOffsets.Comments + curString));
                    }

                    Params[index].Comments = stringList;
                }
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            BaseOffsets baseOffsets = new BaseOffsets();

            bw.BigEndian = false;

            if(WideStrings)
            {
                bw.WriteASCII("f\0i\0l\0t\0");
            }
            else
            {
                bw.WriteASCII("filt");
            }

            bw.WriteUInt32((uint)Version);
            bw.WriteByte(0);
            bw.WriteBoolean(Unk0D);
            bw.WriteInt16(0);

            // Offsets
            bw.WriteInt32(Params.Count);
            bw.WriteInt32(Count14);
            bw.ReserveInt32("ParamOffsetsBase");
            bw.ReserveInt32("ParamsBase");
            bw.ReserveInt32("FieldOffsetsBase");
            bw.ReserveInt32("FieldsBase");
            bw.ReserveInt32("ValuesBase");
            bw.ReserveInt32("ValueIdsBase");
            bw.ReserveInt32("Unk30Base");
            bw.WriteInt32(UnkParamExtras.Count);
            bw.ReserveInt32("ParamExtrasBase");
            bw.ReserveInt32("ParamExtraIdsBase");
            bw.WriteSingle(Unk40);

            if (Version > GparamVersion.V2)
            {
                bw.ReserveInt32("ParamCommentsOffsetsBase");
                bw.ReserveInt32("CommentOffsetsBase");
                bw.ReserveInt32("CommentsBase");
            }

            // Sekiro and later
            if (Version >= GparamVersion.V5)
            {
                bw.WriteSingle(Unk50);
            }

            // --- Processing

            // Params
            baseOffsets.ParamOffsets = (int)bw.Position;
            bw.FillInt32("ParamOffsetsBase", baseOffsets.ParamOffsets);

            for (int index = 0; index < Params.Count; ++index)
            {
                bw.ReserveInt32($"ParamOffset[{index}]");
            }

            baseOffsets.Params = (int)bw.Position;
            bw.FillInt32("ParamsBase", baseOffsets.Params);

            for (int index = 0; index < Params.Count; ++index)
            {
                int num = (int)bw.Position - baseOffsets.Params;

                bw.FillInt32($"ParamOffset[{index}]", num);
                Params[index].Write(bw, Version, index);
                bw.Pad(4);
            }

            baseOffsets.FieldOffsets = (int)bw.Position;
            bw.FillInt32("FieldOffsetsBase", baseOffsets.FieldOffsets);
            for (int index = 0; index < Params.Count; ++index)
            {
                Params[index].WriteFieldOffsets(bw, baseOffsets, index);
            }

            baseOffsets.Fields = (int)bw.Position;
            bw.FillInt32("FieldsBase", baseOffsets.Fields);
            for (int index = 0; index < Params.Count; ++index)
            {
                Params[index].WriteFields(bw, Version, baseOffsets, index);
            }

            baseOffsets.Values = (int)bw.Position;
            bw.FillInt32("ValuesBase", baseOffsets.Values);
            for (int index = 0; index < Params.Count; ++index)
            {
                Params[index].WriteValues(bw, Version, baseOffsets, index);
            }

            baseOffsets.ValueIds = (int)bw.Position;
            bw.FillInt32("ValueIdsBase", baseOffsets.ValueIds);
            for (int index = 0; index < Params.Count; ++index)
            {
                Params[index].WriteValueIds(bw, Version, baseOffsets, index);
            }

            // Data
            baseOffsets.Unk30 = (int)bw.Position;
            bw.FillInt32("Unk30Base", baseOffsets.Unk30);
            bw.WriteBytes(Data30);
            bw.Pad(4);

            // Extras
            baseOffsets.ParamExtras = (int)bw.Position;
            bw.FillInt32("ParamExtrasBase", baseOffsets.ParamExtras);
            for (int index = 0; index < UnkParamExtras.Count; ++index)
            {
                UnkParamExtras[index].Write(bw, Version, index);
            }

            baseOffsets.ParamExtraIds = (int)bw.Position;
            bw.FillInt32("ParamExtraIdsBase", baseOffsets.ParamExtraIds);
            for (int index = 0; index < UnkParamExtras.Count; ++index)
            {
                UnkParamExtras[index].WriteIds(bw, baseOffsets, index);
            }

            // Comments
            if (Version > GparamVersion.V2)
            {
                baseOffsets.ParamCommentsOffsets = (int)bw.Position;

                bw.FillInt32("ParamCommentsOffsetsBase", baseOffsets.ParamCommentsOffsets);

                for (int index = 0; index < Params.Count; ++index)
                {
                    Params[index].WriteCommentOffsetsOffset(bw, index);
                }

                baseOffsets.CommentOffsets = (int)bw.Position;
                bw.FillInt32("CommentOffsetsBase", baseOffsets.CommentOffsets);
                for (int index = 0; index < Params.Count; ++index)
                {
                    Params[index].WriteCommentOffsets(bw, baseOffsets, index);
                }

                baseOffsets.Comments = (int)bw.Position;
                bw.FillInt32("CommentsBase", baseOffsets.Comments);
                for (int index = 0; index < Params.Count; ++index)
                {
                    Params[index].WriteComments(bw, baseOffsets, index);
                }
            }
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

        /// <summary>
        /// A graphics param.
        /// </summary>
        public class Param
        {
            /// <summary>
            /// List of fields for this param.
            /// </summary>
            public List<IField> Fields { get; set; }

            /// <summary>
            /// Key for this param.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Name of this param.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A set of comments -- might be empty.
            /// </summary>
            public List<string> Comments { get; set; }

            public Param()
            {
                Fields = new List<IField>();
                Key = "";
                Name = "";
                Comments = new List<string>();
            }

            public override string ToString()
            {
                return $"{Key} [{Fields.Count}]";
            }

            internal Param(
              BinaryReaderEx br,
              GparamVersion version,
              BaseOffsets baseOffsets)
            {
                int count = br.ReadInt32();
                int offset = br.ReadInt32();

                if (version is GparamVersion.V2)
                {
                    Key = br.ReadShiftJIS();
                    Name = br.ReadShiftJIS();
                }
                else
                {
                    Key = br.ReadUTF16();
                    Name = br.ReadUTF16();
                }

                int[] fieldOffsets = br.GetInt32s(baseOffsets.FieldOffsets + offset, count);

                Fields = new List<IField>(count);
                foreach (int curOffset in fieldOffsets)
                {
                    br.Position = (baseOffsets.Fields + curOffset);
                    Fields.Add(IField.Read(br, version, baseOffsets));
                }
            }

            internal void Write(BinaryWriterEx bw,
              GparamVersion version, int index)
            {
                bw.WriteInt32(Fields.Count);
                bw.ReserveInt32($"Param[{index}]FieldOffsetsOffset");

                if (version is GparamVersion.V2)
                {
                    // TODO: properly handle the padding here, currently breaks byte-perfect saving
                    // String is padded to 8 bytes if it is 4 bytes
                    // String is padded to 16 bytes if it is 12 bytes

                    bw.WriteShiftJIS(Key);
                    bw.WriteShiftJIS(Name);
                }
                else
                {
                    bw.WriteUTF16(Key, true);
                    bw.WriteUTF16(Name, true);
                }
            }

            internal void WriteFieldOffsets(BinaryWriterEx bw,
              BaseOffsets baseOffsets, int paramIndex)
            {
                int value = (int)bw.Position - baseOffsets.FieldOffsets;

                bw.FillInt32($"Param[{paramIndex}]FieldOffsetsOffset", value);

                for (int index = 0; index < Fields.Count; ++index)
                {
                    bw.ReserveInt32($"Param[{paramIndex}]Field[{index}]Offset");
                }
            }

            internal void WriteFields(BinaryWriterEx bw, 
                GparamVersion version, BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < Fields.Count; ++index)
                {
                    int value = (int)bw.Position - baseOffsets.Fields;

                    bw.FillInt32($"Param[{paramIndex}]Field[{index}]Offset", value);

                    (Fields[index] as IFieldWriteable).Write(bw, version, paramIndex, index);
                    bw.Pad(4);
                }
            }

            internal void WriteValues(BinaryWriterEx bw,
                GparamVersion version, BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < Fields.Count; ++index)
                {
                    (Fields[index] as IFieldWriteable).WriteValues(bw, version, baseOffsets, paramIndex, index);
                    bw.Pad(4);
                }
            }

            internal void WriteValueIds(BinaryWriterEx bw,
              GparamVersion version, BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < Fields.Count; ++index)
                {
                    (Fields[index] as IFieldWriteable).WriteValueIds(bw, version, baseOffsets, paramIndex, index);
                }
            }

            internal void WriteCommentOffsetsOffset(BinaryWriterEx bw, int paramIndex)
            {
                bw.ReserveInt32($"Param[{paramIndex}]CommentOffsetsOffset");
            }

            internal void WriteCommentOffsets(BinaryWriterEx bw,
              BaseOffsets baseOffsets, int paramIndex)
            {
                int value = (int)bw.Position - baseOffsets.CommentOffsets;
                bw.FillInt32($"Param[{paramIndex}]CommentOffsetsOffset", value);

                for (int index = 0; index < Comments.Count; ++index)
                {
                    bw.ReserveInt32($"Param[{paramIndex}]Comment[{index}]Offset");
                }
            }

            internal void WriteComments(BinaryWriterEx bw,
              BaseOffsets baseOffsets, int paramIndex)
            {
                for (int index = 0; index < Comments.Count; ++index)
                {
                    int value = (int)bw.Position - baseOffsets.Comments;

                    bw.FillInt32($"Param[{paramIndex}]Comment[{index}]Offset", value);
                    bw.WriteUTF16(Comments[index], true);
                    bw.Pad(4);
                }
            }
        }

        public class UnkParamExtra
        {
            public int GroupIndex { get; set; }

            public List<int> Ids { get; set; }

            public int Unk0c { get; set; }

            public UnkParamExtra() => Ids = new List<int>();

            internal UnkParamExtra(BinaryReaderEx br,
              GparamVersion version, BaseOffsets baseOffsets)
            {
                GroupIndex = br.ReadInt32();
                int count = br.ReadInt32();
                int num = br.ReadInt32();

                if (version >= GparamVersion.V5)
                {
                    Unk0c = br.ReadInt32();
                }

                Ids = Enumerable.ToList<int>(br.GetInt32s((baseOffsets.ParamExtraIds + num), count));
            }

            internal void Write(BinaryWriterEx bw, 
                GparamVersion version, int index)
            {
                bw.WriteInt32(GroupIndex);
                bw.WriteInt32(Ids.Count);

                bw.ReserveInt32($"ParamExtra[{index}]IdsOffset");

                if (version < GPARAM.GparamVersion.V5)
                {
                    return;
                }

                bw.WriteInt32(Unk0c);
            }

            internal void WriteIds(BinaryWriterEx bw, 
                BaseOffsets baseOffsets, int index)
            {
                if (Ids.Count == 0)
                {
                    bw.FillInt32($"ParamExtra[{index}]IdsOffset", 0);
                }
                else
                {
                    int value = (int)bw.Position - baseOffsets.ParamExtraIds;
                    bw.FillInt32($"ParamExtra[{index}]IdsOffset", value);
                    bw.WriteInt32s(Ids);
                }
            }
        }

        public enum FieldType : byte
        {
            Sbyte = 1,
            Short = 2,
            Int = 3,
            Long = 4,
            Byte = 5,
            Ushort = 6,
            Uint = 7,
            Ulong = 8,
            Float = 9,
            Double = 10,
            Bool = 11, // 0x0B
            Vec2 = 12, // 0x0C
            Vec3 = 13, // 0x0D
            Vec4 = 14, // 0x0E
            Color = 15, // 0x0F
            String = 16, // 0x0F
        }

        /// <summary>
        /// A field of a param.
        /// </summary>
        public interface IField
        {
            /// <summary>
            /// Key for this field.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Name of the field.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A set of values this field has.
            /// </summary>
            IReadOnlyList<IFieldValue> Values { get; }

            internal static IField Read(BinaryReaderEx br,
              GparamVersion version,
              BaseOffsets baseOffsets)
            {
                FieldType enum8;

                if (version < GparamVersion.V6)
                {
                    enum8 = br.GetEnum8<FieldType>(br.Position + 8L);
                }
                else
                {
                    enum8 = br.GetEnum8<FieldType>(br.Position + 10L);
                }

                switch (enum8)
                {
                    case FieldType.Sbyte:
                        return new SbyteField(br, version, baseOffsets);
                    case FieldType.Short:
                        return new ShortField(br, version, baseOffsets);
                    case FieldType.Int:
                        return new IntField(br, version, baseOffsets);
                    case FieldType.Long:
                        return new LongField(br, version, baseOffsets);
                    case FieldType.Byte:
                        return new ByteField(br, version, baseOffsets);
                    case FieldType.Ushort:
                        return new UshortField(br, version, baseOffsets);
                    case FieldType.Uint:
                        return new UintField(br, version, baseOffsets);
                    case FieldType.Ulong:
                        return new UlongField(br, version, baseOffsets);
                    case FieldType.Float:
                        return new FloatField(br, version, baseOffsets);
                    case FieldType.Double:
                        return new DoubleField(br, version, baseOffsets);
                    case FieldType.Bool:
                        return new BoolField(br, version, baseOffsets);
                    case FieldType.Vec2:
                        return new Vector2Field(br, version, baseOffsets);
                    case FieldType.Vec3:
                        return new Vector3Field(br, version, baseOffsets);
                    case FieldType.Vec4:
                        return new Vector4Field(br, version, baseOffsets);
                    case FieldType.Color:
                        return new ColorField(br, version, baseOffsets);
                    case FieldType.String:
                        return new StringField(br, version, baseOffsets);
                    default:
                        throw new NotImplementedException($"Unknown field type: {enum8}");
                        break;
                }
            }
        }

        internal interface IFieldWriteable
        {
            void Write(BinaryWriterEx bw, 
                GparamVersion version, int paramIndex, int fieldIndex);

            void WriteValues(BinaryWriterEx bw,
              GparamVersion version,
              BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex);

            void WriteValueIds(BinaryWriterEx bw,
              GparamVersion version,
              BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex);
        }

        public abstract class Field<T> : IField, IFieldWriteable
        {
            /// <summary>
            /// Key for this field.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Name of the field.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A set of values this field has.
            /// </summary>
            public List<FieldValue<T>> Values { get; set; }

            /// <summary>
            /// The number of values this field holds.
            /// </summary>
            public short Capacity { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk { get; set; }

            IReadOnlyList<IFieldValue> IField.Values
            {
                get => Values;
            }

            public Field()
            {
                Key = "";
                Name = "";
                Values = new List<FieldValue<T>>();
            }

            public override string ToString()
            {
                return $"{Key} [{Values.Count}]";
            }

            private protected abstract FieldType Type { get; }

            private protected Field(BinaryReaderEx br,
              GparamVersion version,
              BaseOffsets baseOffsets)
            {
                int valuesOffset = br.ReadInt32();
                int valueIdsOffset = br.ReadInt32();

                int type;
                short capacity;

                if (version < GparamVersion.V6)
                {
                    type = (int)br.AssertByte((byte)Type);
                    Capacity = br.ReadSByte();
                    Unk = br.AssertInt16(0);

                }
                else
                {
                    Capacity = br.ReadInt16();
                    type = (int)br.AssertByte((byte)Type);
                    Unk = br.ReadByte();
                }

                capacity = Capacity;

                if (version is GparamVersion.V2)
                {
                    Key = br.ReadShiftJIS();
                    Name = br.ReadShiftJIS();
                }
                else
                {
                    Key = br.ReadUTF16();
                    Name = br.ReadUTF16();
                }

                br.Position = (baseOffsets.Values + valuesOffset);

                T[] objArray = new T[capacity];

                for (int index = 0; index < (int)capacity; ++index)
                {
                    objArray[index] = ReadValue(br, version);
                }

                br.Position =(baseOffsets.ValueIds + valueIdsOffset);

                Values = new List<FieldValue<T>>(capacity);

                for (int index = 0; index < capacity; ++index)
                {
                    Values.Add(new GPARAM.FieldValue<T>(br, version, objArray[index]));
                }
            }

            private protected abstract T ReadValue(BinaryReaderEx br, GparamVersion version);

            void IFieldWriteable.Write(BinaryWriterEx bw, 
                GparamVersion version, int paramIndex, int fieldIndex)
            {
                bw.ReserveInt32($"Param[{paramIndex}]Field[{fieldIndex}]ValuesOffset");
                bw.ReserveInt32($"Param[{paramIndex}]Field[{fieldIndex}]ValueIdsOffset");

                if (version < GparamVersion.V6)
                {
                    bw.WriteByte((byte)Type);
                    bw.WriteSByte((sbyte)Capacity);
                    bw.WriteInt16(Unk);
                }
                else
                {
                    bw.WriteInt16(Capacity);
                    bw.WriteByte((byte)Type);
                    bw.WriteByte((byte)Unk);
                }

                if (version is GparamVersion.V2)
                {
                    // TODO: properly handle the padding here, currently breaks byte-perfect saving
                    // String is padded to 8 bytes if it is 4 bytes
                    // String is padded to 16 bytes if it is 12 bytes

                    bw.WriteShiftJIS(Key);
                    bw.WriteShiftJIS(Name);
                }
                else
                {
                    bw.WriteUTF16(Key, true);
                    bw.WriteUTF16(Name, true);
                }
            }

            void IFieldWriteable.WriteValues(
              BinaryWriterEx bw,
              GparamVersion version,
              BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex)
            {
                int value = (int)bw.Position - baseOffsets.Values;

                bw.FillInt32($"Param[{paramIndex}]Field[{fieldIndex}]ValuesOffset", value);

                foreach (FieldValue<T> fieldValue in Values)
                {
                    WriteValue(bw, version, fieldValue.Value);
                }
            }

            private protected abstract void WriteValue(BinaryWriterEx bw, GparamVersion version, T value);

            void GPARAM.IFieldWriteable.WriteValueIds(BinaryWriterEx bw,
              GparamVersion version,
              BaseOffsets baseOffsets,
              int paramIndex,
              int fieldIndex)
            {
                int value = (int)bw.Position - baseOffsets.ValueIds;

                bw.FillInt32($"Param[{paramIndex}]Field[{fieldIndex}]ValueIdsOffset", value);

                foreach (FieldValue<T> fieldValue in Values)
                {
                    fieldValue.Write(bw, version);
                }
            }
        }

        public class SbyteField : Field<sbyte>
        {
            public SbyteField() { }

            private protected override FieldType Type => FieldType.Sbyte;

            internal SbyteField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override sbyte ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadSByte();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, sbyte value)
            {
                bw.WriteSByte(value);
            }
        }

        public class ShortField : Field<short>
        {
            public ShortField() { }

            private protected override FieldType Type => FieldType.Short;

            internal ShortField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override short ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadInt16();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, short value)
            {
                bw.WriteInt16(value);
            }
        }

        public class IntField : Field<int>
        {
            public IntField() { }

            private protected override FieldType Type => FieldType.Int;

            internal IntField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override int ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadInt32();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, int value)
            {
                bw.WriteInt32(value);
            }
        }

        public class ByteField : Field<byte>
        {
            public ByteField() { }

            private protected override FieldType Type => FieldType.Byte;

            internal ByteField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override byte ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadByte();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, byte value)
            {
                bw.WriteByte(value);
            }
        }

        public class UintField : Field<uint>
        {
            public UintField() { }

            private protected override FieldType Type => FieldType.Uint;

            internal UintField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override uint ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadUInt32();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, uint value)
            {
                bw.WriteUInt32(value);
            }
        }

        public class FloatField : Field<float>
        {
            public FloatField() { }

            private protected override FieldType Type => FieldType.Float;

            internal FloatField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override float ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadSingle();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, float value)
            {
                bw.WriteSingle(value);
            }
        }

        public class BoolField : Field<bool>
        {
            public BoolField() { }

            private protected override FieldType Type => FieldType.Bool;

            internal BoolField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override bool ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadBoolean();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, bool value)
            {
                bw.WriteBoolean(value);
            }
        }

        public class Vector2Field : Field<Vector2>
        {
            public Vector2Field() { }

            private protected override FieldType Type => FieldType.Vec2;

            internal Vector2Field(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override Vector2 ReadValue(BinaryReaderEx br, GparamVersion version)
            {
                Vector2 vector2 = br.ReadVector2();
                br.AssertInt64(0);
                return vector2;
            }

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, Vector2 value)
            {
                bw.WriteVector2(value);
                bw.WriteInt64(0L);
            }
        }

        public class Vector3Field : Field<Vector3>
        {
            public Vector3Field() { }

            private protected override FieldType Type => FieldType.Vec3;

            internal Vector3Field(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override Vector3 ReadValue(BinaryReaderEx br, GparamVersion version)
            {
                Vector3 vector3 = br.ReadVector3();
                br.AssertInt32(0);
                return vector3;
            }

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, Vector3 value)
            {
                bw.WriteVector3(value);
                bw.WriteInt32(0);
            }
        }

        public class Vector4Field : Field<Vector4>
        {
            public Vector4Field() { }

            private protected override FieldType Type => FieldType.Vec4;

            internal Vector4Field(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override Vector4 ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadVector4();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, Vector4 value)
            {
                bw.WriteVector4(value);
            }
        }

        public class ColorField : Field<Color>
        {
            private protected override FieldType Type => FieldType.Color;

            public ColorField() { }

            internal ColorField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override Color ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadRGBA();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, Color value)
            {
                bw.WriteRGBA(value);
            }
        }

        public class LongField : Field<long>
        {
            public LongField() { }

            private protected override FieldType Type => FieldType.Long;

            internal LongField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override long ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadInt64();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, long value)
            {
                bw.WriteInt64(value);
            }
        }

        public class UshortField : Field<ushort>
        {
            public UshortField() { }

            private protected override FieldType Type => FieldType.Ushort;

            internal UshortField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override ushort ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadUInt16();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, ushort value)
            {
                bw.WriteUInt16(value);
            }
        }

        public class UlongField : Field<ulong>
        {
            public UlongField() { }

            private protected override FieldType Type => FieldType.Ulong;

            internal UlongField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override ulong ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadUInt64();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, ulong value)
            {
                bw.WriteUInt64(value);
            }
        }

        public class DoubleField : Field<double>
        {
            public DoubleField() { }

            private protected override FieldType Type => FieldType.Double;

            internal DoubleField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            private protected override double ReadValue(BinaryReaderEx br, GparamVersion version) => br.ReadDouble();

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, double value)
            {
                bw.WriteDouble(value);
            }
        }

        public class StringField : Field<string>
        {
            public StringField() { }

            private protected override FieldType Type => FieldType.String;

            internal StringField(BinaryReaderEx br, GparamVersion version, BaseOffsets baseOffsets) : base(br, version, baseOffsets) { }

            //TODO: no gparam exist that has this type, whether it works correctly is unconfirmed
            private protected override string ReadValue(BinaryReaderEx br, GparamVersion version)
            {
                if (version is GparamVersion.V2)
                {
                    return br.ReadShiftJIS();
                }
                else
                {
                    return br.ReadUTF16();
                }
            }

            private protected override void WriteValue(BinaryWriterEx bw, GparamVersion version, string value)
            {
                if (version is GparamVersion.V2)
                {
                    bw.WriteShiftJIS(value);
                }
                else
                {
                    bw.WriteUTF16(value, true);
                }
            }
        }

        public interface IFieldValue
        {
            int Id { get; set; }

            float Unk04 { get; set; }

            object Value { get; set; }
        }

        public class FieldValue<T> : IFieldValue
        {
            public int Id { get; set; }

            public float Unk04 { get; set; }

            public T Value { get; set; }

            object GPARAM.IFieldValue.Value
            {
                get
                {
                    return (object)Value;
                }
                set
                {
                    Value = (T)value;
                }
            }

            public FieldValue() { }

            public override string ToString()
            {
                if ((double)Unk04 != 0.0)
                {
                    return $"{Id} ({Unk04}) = {Value}";
                }

                return $"{Id} = {Value}";
            }

            internal FieldValue(BinaryReaderEx br, GparamVersion version, T value)
            {
                Id = br.ReadInt32();

                if (version >= GparamVersion.V5)
                {
                    Unk04 = br.ReadSingle();
                }

                Value = value;
            }

            internal void Write(BinaryWriterEx bw, GparamVersion version)
            {
                bw.WriteInt32(Id);

                if (version < GparamVersion.V5)
                {
                    return;
                }

                bw.WriteSingle(Unk04);
            }
        }

        /// <summary>
        /// GPARAM file version.
        /// </summary>
        public enum GparamVersion : uint
        {
            /// <summary>
            /// Initial version, in Dark Souls 2
            /// </summary>
            V2 = 2,

            /// <summary>
            /// Bloodborne and later
            /// </summary>
            V3 = 3,

            /// <summary>
            /// Sekiro and later
            /// </summary>
            V5 = 5,

            /// <summary>
            /// Armored Core 6 and later
            /// </summary>
            V6 = 6
        }

    }
}
