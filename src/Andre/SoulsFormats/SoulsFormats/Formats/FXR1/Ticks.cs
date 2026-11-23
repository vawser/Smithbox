using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        public class FloatTick
        {
            [XmlAttribute]
            public float Time {get; set;}
            [XmlAttribute]
            public float Value {get; set;}


            public static List<FloatTick> ReadListDirectly(BinaryReaderEx br)
            {
                int count = br.ReadInt32();
                var ticks = new List<FloatTick>(count);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new FloatTick());
                    ticks[i].Time = br.ReadSingle();
                }
                for (int i = 0; i < count; i++)
                    ticks[i].Value = br.ReadSingle();
                return ticks;
            }

            internal static void WriteListDirectly(BinaryWriterEx bw, List<FloatTick> ticks)
            {
                bw.WriteInt32(ticks.Count);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Time);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Value);
            }

            public static List<FloatTick> ReadListInFXNode(BinaryReaderEx br)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                int count = ReadFXR1Varint(br);

                var ticks = new List<FloatTick>();

                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new FloatTick());
                    ticks[i].Time = br.ReadSingle();
                }
                br.StepOut();

                br.StepIn(valueOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks[i].Value = br.ReadSingle();
                }
                br.StepOut();

                return ticks;
            }

            internal static void WriteListInFXNode(BinaryWriterEx bw, FxrEnvironment env, List<FloatTick> ticks)
            {
                var times = new List<float>(ticks.Count);
                var flattenedValues = new List<float>(ticks.Count);

                for (int i = 0; i < ticks.Count; i++)
                {
                    times.Add(ticks[i].Time);
                    flattenedValues.Add(ticks[i].Value);
                }

                env.RegisterPointer(times);
                env.RegisterPointer(flattenedValues);
                WriteFXR1Varint(bw, ticks.Count);
            }
        }

        public class IntTick
        {
            [XmlAttribute]
            public float Time;
            [XmlAttribute]
            public int Value;

            public static List<IntTick> ReadListDirectly(BinaryReaderEx br)
            {
                int count = br.ReadInt32();
                var ticks = new List<IntTick>(count);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new IntTick());
                    ticks[i].Time = br.ReadSingle();
                }
                for (int i = 0; i < count; i++)
                    ticks[i].Value = br.ReadInt32();
                return ticks;
            }

            internal static void WriteListDirectly(BinaryWriterEx bw, List<IntTick> ticks)
            {
                bw.WriteInt32(ticks.Count);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Time);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteInt32(ticks[i].Value);
            }

            public static List<IntTick> ReadListInFXNode(BinaryReaderEx br)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                int count = ReadFXR1Varint(br);

                var ticks = new List<IntTick>();

                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new IntTick());
                    ticks[i].Time = br.ReadSingle();
                }
                br.StepOut();

                br.StepIn(valueOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks[i].Value = br.ReadInt32();
                }
                br.StepOut();

                return ticks;
            }

            internal static void WriteListInFXNode(BinaryWriterEx bw, FxrEnvironment env, List<IntTick> ticks)
            {
                List<float> times = new List<float>(ticks.Count);
                List<int> flattenedValues = new List<int>(ticks.Count);

                for (int i = 0; i < ticks.Count; i++)
                {
                    times.Add(ticks[i].Time);
                    flattenedValues.Add(ticks[i].Value);
                }

                env.RegisterPointer(times);
                env.RegisterPointer(flattenedValues);
                WriteFXR1Varint(bw, ticks.Count);
            }
        }

        public class Float3Tick
        {
            [XmlAttribute]
            public float Time;
            [XmlAttribute]
            public float X;
            [XmlAttribute]
            public float Y;
            [XmlAttribute]
            public float Z;

            public static List<Float3Tick> ReadListDirectly(BinaryReaderEx br)
            {
                int count = br.ReadInt32();
                var ticks = new List<Float3Tick>(count);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new Float3Tick());
                    ticks[i].Time = br.ReadSingle();
                }
                for (int i = 0; i < count; i++)
                    ticks[i].X = br.ReadSingle();
                for (int i = 0; i < count; i++)
                    ticks[i].Y = br.ReadSingle();
                for (int i = 0; i < count; i++)
                    ticks[i].Z = br.ReadSingle();
                return ticks;
            }

            internal static void WriteListDirectly(BinaryWriterEx bw, List<Float3Tick> ticks)
            {
                bw.WriteInt32(ticks.Count);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Time);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].X);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Y);
                for (int i = 0; i < ticks.Count; i++)
                    bw.WriteSingle(ticks[i].Z);
            }

            public static List<Float3Tick> ReadListInFXNode(BinaryReaderEx br)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                int count = ReadFXR1Varint(br);

                var ticks = new List<Float3Tick>();

                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new Float3Tick());
                    ticks[i].Time = br.ReadSingle();
                }
                br.StepOut();

                // Every key's value, then every key's in tangent, then every out tangent (as ReadListDirectly).
                // Read key by key instead, BB's cubic sizes had "tangents" of 7.5 … 20 and second keys of π/4;
                // read this way every tangent is an angle in [0, π/2] (FFX plan §20.31).
                br.StepIn(valueOffset);
                for (int i = 0; i < count; i++)
                    ticks[i].X = br.ReadSingle();
                for (int i = 0; i < count; i++)
                    ticks[i].Y = br.ReadSingle();
                for (int i = 0; i < count; i++)
                    ticks[i].Z = br.ReadSingle();
                br.StepOut();

                return ticks;
            }

            internal static void WriteListInFXNode(BinaryWriterEx bw, FxrEnvironment env, List<Float3Tick> ticks)
            {
                var times = new List<float>(ticks.Count);
                var flattenedValues = new List<float>(ticks.Count);

                for (int i = 0; i < ticks.Count; i++)
                    times.Add(ticks[i].Time);
                for (int i = 0; i < ticks.Count; i++)
                    flattenedValues.Add(ticks[i].X);
                for (int i = 0; i < ticks.Count; i++)
                    flattenedValues.Add(ticks[i].Y);
                for (int i = 0; i < ticks.Count; i++)
                    flattenedValues.Add(ticks[i].Z);

                env.RegisterPointer(times);
                env.RegisterPointer(flattenedValues);
                WriteFXR1Varint(bw, ticks.Count);
            }
        }

        /// <summary>Floats written as 16-byte vectors (RGBA keys): laid out 16-byte aligned, after the node pointer lists.</summary>
        internal sealed class VectorList : List<float>
        {
            public VectorList(int capacity) : base(capacity) { }
        }

        public class ColorTick
        {
            [XmlAttribute]
            public float Time;
            [XmlAttribute]
            public float R;
            [XmlAttribute]
            public float G;
            [XmlAttribute]
            public float B;
            [XmlAttribute]
            public float A;

            public static List<ColorTick> ReadListInFXNode(BinaryReaderEx br)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                int count = ReadFXR1Varint(br);

                var ticks = new List<ColorTick>();

                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new ColorTick());
                    ticks[i].Time = br.ReadSingle();
                }
                br.StepOut();

                br.StepIn(valueOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks[i].R = br.ReadSingle();
                    ticks[i].G = br.ReadSingle();
                    ticks[i].B = br.ReadSingle();
                    ticks[i].A = br.ReadSingle();
                }
                br.StepOut();

                return ticks;
            }

            internal static void WriteListInFXNode(BinaryWriterEx bw, FxrEnvironment env, List<ColorTick> ticks)
            {
                var times = new List<float>(ticks.Count);
                // RGBA values are 16-byte vectors: vanilla keeps them with the other 16-byte aligned data (FFX plan §17)
                var flattenedValues = new VectorList(ticks.Count);

                for (int i = 0; i < ticks.Count; i++)
                {
                    times.Add(ticks[i].Time);
                    flattenedValues.Add(ticks[i].R);
                    flattenedValues.Add(ticks[i].G);
                    flattenedValues.Add(ticks[i].B);
                    flattenedValues.Add(ticks[i].A);
                }

                env.RegisterPointer(times);
                env.RegisterPointer(flattenedValues);
                WriteFXR1Varint(bw, ticks.Count);
            }
        }

        public class Color3Tick
        {
            [XmlAttribute]
            public float Time;
            public float R1;
            public float G1;
            public float B1;
            public float A1;
            public float R2;
            public float G2;
            public float B2;
            public float A2;
            public float R3;
            public float G3;
            public float B3;
            public float A3;

            public static List<Color3Tick> ReadListInFXNode(BinaryReaderEx br)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                int count = ReadFXR1Varint(br);

                var ticks = new List<Color3Tick>();

                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks.Add(new Color3Tick());
                    ticks[i].Time = br.ReadSingle();
                }
                br.StepOut();

                br.StepIn(valueOffset);
                for (int i = 0; i < count; i++)
                {
                    ticks[i].R1 = br.ReadSingle();
                    ticks[i].G1 = br.ReadSingle();
                    ticks[i].B1 = br.ReadSingle();
                    ticks[i].A1 = br.ReadSingle();
                    ticks[i].R2 = br.ReadSingle();
                    ticks[i].G2 = br.ReadSingle();
                    ticks[i].B2 = br.ReadSingle();
                    ticks[i].A2 = br.ReadSingle();
                    ticks[i].R3 = br.ReadSingle();
                    ticks[i].G3 = br.ReadSingle();
                    ticks[i].B3 = br.ReadSingle();
                    ticks[i].A3 = br.ReadSingle();
                }
                br.StepOut();

                return ticks;
            }

            internal static void WriteListInFXNode(BinaryWriterEx bw, FxrEnvironment env, List<Color3Tick> ticks)
            {
                var times = new List<float>(ticks.Count);
                var flattenedValues = new VectorList(ticks.Count);

                for (int i = 0; i < ticks.Count; i++)
                {
                    times.Add(ticks[i].Time);
                    flattenedValues.Add(ticks[i].R1);
                    flattenedValues.Add(ticks[i].G1);
                    flattenedValues.Add(ticks[i].B1);
                    flattenedValues.Add(ticks[i].A1);
                    flattenedValues.Add(ticks[i].R2);
                    flattenedValues.Add(ticks[i].G2);
                    flattenedValues.Add(ticks[i].B2);
                    flattenedValues.Add(ticks[i].A2);
                    flattenedValues.Add(ticks[i].R3);
                    flattenedValues.Add(ticks[i].G3);
                    flattenedValues.Add(ticks[i].B3);
                    flattenedValues.Add(ticks[i].A3);
                }

                env.RegisterPointer(times);
                env.RegisterPointer(flattenedValues);
                WriteFXR1Varint(bw, ticks.Count);
            }
        }
    }
}
