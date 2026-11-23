using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {

        [XmlInclude(typeof(ConstFloatSequence))]
        [XmlInclude(typeof(FloatSequence))]
        [XmlInclude(typeof(RangedFloatSequence))]
        [XmlInclude(typeof(FloatSequenceEx))]
        [XmlInclude(typeof(RangedFloatSequenceEx))]
        [XmlInclude(typeof(Float3Sequence))]
        [XmlInclude(typeof(RangedFloat3Sequence))]
        [XmlInclude(typeof(RepeatingConstFloatSequence))]
        [XmlInclude(typeof(RepeatingFloatSequence))]
        [XmlInclude(typeof(RangedFloatSequenceB))]
        [XmlInclude(typeof(NewFloat3Sequence))]
        [XmlInclude(typeof(ConstFloat))]
        [XmlInclude(typeof(FieldType25))]
        [XmlInclude(typeof(FieldType26))]
        [XmlInclude(typeof(FieldType27))]
        [XmlInclude(typeof(Empty))]
        public abstract class FXField
        {
            [XmlIgnore]
            public abstract int Type { get; }

            internal abstract void InnerRead(BinaryReaderEx br, FxrEnvironment env);
            internal abstract void InnerWrite(BinaryWriterEx bw, FxrEnvironment env);

            internal virtual bool IsEmptyPointer() => false;

            internal static FXField Read(BinaryReaderEx br, FxrEnvironment env)
            {
                int type = ReadFXR1Varint(br);
                int offset = ReadFXR1Varint(br);

                FXField v = null;

                
                switch (type)
                {
                    case 0: v = new ConstFloatSequence(); break; //Constant
                    case 4: v = new FloatSequence(); break;
                    case 5: v = new RangedFloatSequence(); break;
                    case 6: v = new FloatSequenceEx(); break;
                    case 7: v = new RangedFloatSequenceEx(); break;
                    case 8: v = new Float3Sequence(); break;
                    case 9: v = new RangedFloat3Sequence(); break;
                    case 12: v = new RepeatingConstFloatSequence(); break; //Constant, repeats
                    case 16: v = new RepeatingFloatSequence(); break; //Repeats
                    case 17: v = new RangedFloatSequenceB(); break;
                    case 20: v = new NewFloat3Sequence(); break; //DS1R Only
                    case 24: v = new ConstFloat(); break;
                    case 25: v = new FieldType25(); break;
                    case 26: v = new FieldType26(); break;
                    case 27: v = new FieldType27(); break;
                    case 28: v = new Empty(); break;
                    default: throw new NotImplementedException();
                }

                br.StepIn(offset);
                v.InnerRead(br, env);
                br.StepOut();

                return v;
            }

            //public static Node[] ReadMany(BinaryReaderEx br, FxrEnvironment env, int count)
            //{
            //    Node[] list = new Node[count];
            //    for (int i = 0; i < count; i++)
            //        list[i] = Read(br, env);
            //    return list;
            //}


            public class ConstFloatSequence : FXField
            {
                public override int Type => 0;

                public List<FloatTick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                }
            }

            public class FloatSequence : FXField
            {
                public override int Type => 4;

                public List<FloatTick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                }
            }

            public class RangedFloatSequence : FXField
            {
                public override int Type => 5;

                public List<FloatTick> Ticks;
                [XmlAttribute]
                public float Min;
                [XmlAttribute]
                public float Max;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                    bw.WriteSingle(Min);
                    bw.WriteSingle(Max);
                }
            }

            public class FloatSequenceEx : FXField
            {
                public override int Type => 6;

                public List<FloatTick> Ticks;
                [XmlAttribute]
                public int ResourceIndex;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                    ResourceIndex = br.ReadInt32();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                    bw.WriteInt32(ResourceIndex);
                }
            }

            public class RangedFloatSequenceEx : FXField
            {
                public override int Type => 7;

                public List<FloatTick> Ticks;
                [XmlAttribute]
                public float Min;
                [XmlAttribute]
                public float Max;
                [XmlAttribute]
                public int ResourceIndex;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                    ResourceIndex = br.ReadInt32();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                    bw.WriteSingle(Min);
                    bw.WriteSingle(Max);
                    bw.WriteInt32(ResourceIndex);
                }
            }

            public class Float3Sequence : FXField
            {
                public override int Type => 8;

                public List<Float3Tick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = Float3Tick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    Float3Tick.WriteListDirectly(bw, Ticks);
                }
            }

            public class RangedFloat3Sequence : FXField
            {
                public override int Type => 9;

                public List<Float3Tick> Ticks;
                [XmlAttribute]
                public float Min;
                [XmlAttribute]
                public float Max;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = Float3Tick.ReadListDirectly(br);
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    Float3Tick.WriteListDirectly(bw, Ticks);
                    bw.WriteSingle(Min);
                    bw.WriteSingle(Max);
                }
            }

            public class RepeatingConstFloatSequence : FXField
            {
                public override int Type => 12;

                public List<FloatTick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                }
            }

            public class RepeatingFloatSequence : FXField
            {
                public override int Type => 16;

                public List<FloatTick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                }
            }

            public class RangedFloatSequenceB : FXField
            {
                public override int Type => 17;

                public List<FloatTick> Ticks;
                [XmlAttribute]
                public float Min;
                [XmlAttribute]
                public float Max;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = FloatTick.ReadListDirectly(br);
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    FloatTick.WriteListDirectly(bw, Ticks);
                    bw.WriteSingle(Min);
                    bw.WriteSingle(Max);
                }
            }

            public class NewFloat3Sequence : FXField
            {
                public override int Type => 20;

                public List<Float3Tick> Ticks;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Ticks = Float3Tick.ReadListDirectly(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    Float3Tick.WriteListDirectly(bw, Ticks);
                }
            }

            public class ConstFloat : FXField
            {
                public override int Type => 24;

                [XmlAttribute]
                public float Value;

                public ConstFloat()
                {

                }

                public ConstFloat(float value)
                {
                    Value = value;
                }

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Value = br.ReadSingle();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Value);
                }
            }

            public class FieldType25 : FXField
            {
                public override int Type => 25;

                [XmlAttribute]
                public float Base;
                [XmlAttribute]
                public float Min;
                [XmlAttribute]
                public float Max;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Base = br.ReadSingle();
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                }
                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Base);
                    bw.WriteSingle(Min);
                    bw.WriteSingle(Max);
                }
            }

            public class FieldType26 : FXField
            {
                public override int Type => 26;

                [XmlAttribute]
                public float Unk1;
                [XmlAttribute]
                public int Unk2;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Unk1 = br.ReadSingle();
                    Unk2 = br.ReadInt32();
                }
                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Unk1);
                    bw.WriteInt32(Unk2);
                }
            }

            public class FieldType27 : FXField
            {
                public override int Type => 27;

                [XmlAttribute]
                public float Unk1;
                [XmlAttribute]
                public float Unk2;
                [XmlAttribute]
                public float Unk3;
                [XmlAttribute]
                public int ResourceIndex;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Unk1 = br.ReadSingle();
                    Unk2 = br.ReadSingle();
                    Unk3 = br.ReadSingle();
                    ResourceIndex = br.ReadInt32();
                }
                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Unk1);
                    bw.WriteSingle(Unk2);
                    bw.WriteSingle(Unk3);
                    bw.WriteInt32(ResourceIndex);
                }
            }

            public class Empty : FXField
            {
                public override int Type => 28;
                
                internal override bool IsEmptyPointer() => true;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {

                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {

                }
            }

        }

        
    }
}
