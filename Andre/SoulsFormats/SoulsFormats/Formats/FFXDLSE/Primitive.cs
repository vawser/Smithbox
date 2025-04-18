using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class PrimitiveInt : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<dl_int32>";

            internal override int Version => 1;

            [XmlAttribute]
            public int Value { get; set; }

            public PrimitiveInt() { }

            public PrimitiveInt(int value)
            {
                Value = value;
            }

            internal PrimitiveInt(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Value);
            }

            internal static int Read(BinaryReaderEx br, List<string> classNames)
                => new PrimitiveInt(br, classNames).Value;

            internal static void AddClassName(List<string> classNames)
                => new PrimitiveInt().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, List<string> classNames, int value)
                => new PrimitiveInt(value).Write(bw, classNames);
        }

        public class PrimitiveFloat : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<dl_float32>";

            internal override int Version => 1;

            [XmlAttribute]
            public float Value { get; set; }

            public PrimitiveFloat() { }

            public PrimitiveFloat(float value)
            {
                Value = value;
            }

            internal PrimitiveFloat(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(Value);
            }

            internal static float Read(BinaryReaderEx br, List<string> classNames)
                => new PrimitiveFloat(br, classNames).Value;

            internal static void AddClassName(List<string> classNames)
                => new PrimitiveFloat().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, List<string> classNames, float value)
                => new PrimitiveFloat(value).Write(bw, classNames);
        }

        public class PrimitiveTick : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<FXTick>";

            internal override int Version => 1;

            [XmlAttribute]
            public float Value { get; set; }

            public PrimitiveTick() { }

            public PrimitiveTick(float value)
            {
                Value = value;
            }

            internal PrimitiveTick(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(Value);
            }

            internal static float Read(BinaryReaderEx br, List<string> classNames)
                => new PrimitiveTick(br, classNames).Value;

            internal static void AddClassName(List<string> classNames)
                => new PrimitiveTick().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, List<string> classNames, float value)
                => new PrimitiveTick(value).Write(bw, classNames);
        }

        public class PrimitiveColor : FXSerializable
        {
            internal override string ClassName => "FXSerializablePrimitive<FXColorRGBA>";

            internal override int Version => 1;

            public float R { get; set; }

            public float G { get; set; }

            public float B { get; set; }

            public float A { get; set; }

            public PrimitiveColor() { }

            public PrimitiveColor(float r, float g, float b, float a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }

            internal PrimitiveColor(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                R = br.ReadSingle();
                G = br.ReadSingle();
                B = br.ReadSingle();
                A = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteSingle(R);
                bw.WriteSingle(G);
                bw.WriteSingle(B);
                bw.WriteSingle(A);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
