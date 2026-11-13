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

            internal PrimitiveInt(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                Value = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
            {
                bw.WriteInt32(Value);
            }

            internal static int Read(BinaryReaderEx br, ClassTable classNames)
                => new PrimitiveInt(br, classNames).Value;

            internal static void AddClassName(ClassTable classNames)
                => new PrimitiveInt().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, ClassTable classNames, int value)
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

            internal PrimitiveFloat(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
            {
                bw.WriteSingle(Value);
            }

            internal static float Read(BinaryReaderEx br, ClassTable classNames)
                => new PrimitiveFloat(br, classNames).Value;

            internal static void AddClassName(ClassTable classNames)
                => new PrimitiveFloat().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, ClassTable classNames, float value)
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

            internal PrimitiveTick(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                Value = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
            {
                bw.WriteSingle(Value);
            }

            internal static float Read(BinaryReaderEx br, ClassTable classNames)
                => new PrimitiveTick(br, classNames).Value;

            internal static void AddClassName(ClassTable classNames)
                => new PrimitiveTick().AddClassNames(classNames);

            internal static void Write(BinaryWriterEx bw, ClassTable classNames, float value)
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

            internal PrimitiveColor(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                R = br.ReadSingle();
                G = br.ReadSingle();
                B = br.ReadSingle();
                A = br.ReadSingle();
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
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
