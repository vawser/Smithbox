using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FXR1
    {
        /// <summary>The type id a node object is stored with (the first varint), or -1 for null.</summary>
        public static int NodeTypeId(FXNode node) => node switch
        {
            null => -1,
            FXNode.ContainerNode c => c.NodeType,
            FXNode.RangedFloat3SequenceNode r => r.NodeType,
            FXNode.RangedColor3SequenceNode r => r.NodeType,
            _ => NodeTypes.TryGetValue(node.GetType(), out var id) ? id : -1,
        };

        /// <summary>Node classes to the type ids they're stored with (each class but the container and ranged ones has one id).</summary>
        private static readonly Dictionary<Type, int> NodeTypes = new Dictionary<Type, int>()
        {
            [typeof(FXNode.IntNode)] = 1, [typeof(FXNode.IntArrayNode)] = 2, [typeof(FXNode.IntSequenceNode3)] = 3, [typeof(FXNode.IntSequenceNode5)] = 5,
            [typeof(FXNode.IntSequenceNode6)] = 6, [typeof(FXNode.FloatNode)] = 7, [typeof(FXNode.IntSequenceNode9)] = 9,
            [typeof(FXNode.FloatSequenceNode11)] = 11, [typeof(FXNode.FloatSequenceNode12)] = 12, [typeof(FXNode.Float3SequenceNode13)] = 13,
            [typeof(FXNode.Float3SequenceNode14)] = 14, [typeof(FXNode.ColorSequenceNode19)] = 19, [typeof(FXNode.ColorSequenceNode20)] = 20,
            [typeof(FXNode.Color3SequenceNode21)] = 21, [typeof(FXNode.Color3SequenceNode22)] = 22, [typeof(FXNode.ColorSequenceNode27)] = 27,
            [typeof(FXNode.ColorSequenceNode28)] = 28, [typeof(FXNode.Color3SequenceNode29)] = 29, [typeof(FXNode.EffectCallNode)] = 37,
            [typeof(FXNode.ActionCallNode)] = 38, [typeof(FXNode.IntNode41)] = 41, [typeof(FXNode.IndexedIntNode)] = 44, [typeof(FXNode.IndexedIntArrayNode)] = 45,
            [typeof(FXNode.IndexedIntSequenceNode)] = 46, [typeof(FXNode.IndexedFloatNode)] = 47, [typeof(FXNode.IndexedEffectNode)] = 59,
            [typeof(FXNode.IndexedActionNode)] = 60, [typeof(FXNode.IndexedSoundIDNode)] = 66, [typeof(FXNode.SoundIDNode68)] = 68,
            [typeof(FXNode.SoundIDNode69)] = 69, [typeof(FXNode.TickNode)] = 70, [typeof(FXNode.IndexedTickNode)] = 71, [typeof(FXNode.Int2Node)] = 79,
            [typeof(FXNode.Float2Node)] = 81, [typeof(FXNode.Tick2Node)] = 85, [typeof(FXNode.IndexedTick2Node)] = 87, [typeof(FXNode.IntSequenceNode89)] = 89,
            [typeof(FXNode.FloatSequenceNodeNode91)] = 91, [typeof(FXNode.FloatSequenceNodeNode95)] = 95, [typeof(FXNode.IntNode111)] = 111,
            [typeof(FXNode.EmptyNode112)] = 112, [typeof(FXNode.EmptyNode113)] = 113, [typeof(FXNode.UnkIndexedValueNode114)] = 114,
            [typeof(FXNode.UnkIndexedValueNode115)] = 115, [typeof(FXNode.Node2Node120)] = 120, [typeof(FXNode.Node2Node121)] = 121,
            [typeof(FXNode.Node2Node122)] = 122, [typeof(FXNode.Node2Node123)] = 123, [typeof(FXNode.Node2Node124)] = 124, [typeof(FXNode.Node2Node125)] = 125,
            [typeof(FXNode.Node2Node126)] = 126, [typeof(FXNode.Node2Node127)] = 127, [typeof(FXNode.NodeNode128)] = 128, [typeof(FXNode.EmptyNode129)] = 129,
            [typeof(FXNode.EmptyNode130)] = 130, [typeof(FXNode.EmptyNode131)] = 131, [typeof(FXNode.EmptyNode132)] = 132,
            [typeof(FXNode.EffectCreateNode133)] = 133, [typeof(FXNode.EffectCreateNode134)] = 134, [typeof(FXNode.EmptyNode136)] = 136,
            [typeof(FXNode.FXNode137)] = 137, [typeof(FXNode.FXNode138)] = 138, [typeof(FXNode.FXNode139)] = 139, [typeof(FXNode.FXNode140)] = 140,
            [typeof(FXNode.IntSequenceNode4)] = 4, [typeof(FXNode.FloatSequenceNode10)] = 10, [typeof(FXNode.ColorNode15)] = 15,
            [typeof(FXNode.ColorSequenceNode17)] = 17, [typeof(FXNode.Color3SequenceNode30)] = 30, [typeof(FXNode.TextureNode40)] = 40,
            [typeof(FXNode.FloatWrapNode82)] = 82, [typeof(FXNode.Color2Node83)] = 83, [typeof(FXNode.ColorWrapNode84)] = 84,
        };


        // FXR v2 (Bloodborne) node types the v1 files don't have, found by Claude/Tools/FfxSurvey --bbnodes / --bbwalk
        // over every vanilla BB effect (FFX Older Games Plan §19). Value ids follow DS2's DLSE param numbering (40 texture,
        // 82 / 84 wrappers, 83 two colours); 141–148 are container nodes. All varints are 8 bytes (v2 files are wide).
        public abstract partial class FXNode
        {
            /// <summary>Reads a sequence node's time list and value offset; the caller reads the values.</summary>
            private static (List<float> Times, int ValueOffset) ReadTimes(BinaryReaderEx br, out int count)
            {
                int timeOffset = ReadFXR1Varint(br);
                int valueOffset = ReadFXR1Varint(br);
                count = ReadFXR1Varint(br);
                var times = new List<float>(count);
                br.StepIn(timeOffset);
                for (int i = 0; i < count; i++)
                    times.Add(br.ReadSingle());
                br.StepOut();
                return (times, valueOffset);
            }

            /// <summary>Int key sequence (type 4: like 3).</summary>
            public class IntSequenceNode4 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env) { AssertFXR1Varint(br, 4); Ticks = IntTick.ReadListInFXNode(br); }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env) { WriteFXR1Varint(bw, 4); IntTick.WriteListInFXNode(bw, env, Ticks); }
            }

            /// <summary>Float key sequence (type 10: like 9 / 11).</summary>
            public class FloatSequenceNode10 : FXNode
            {
                public List<FloatTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env) { AssertFXR1Varint(br, 10); Ticks = FloatTick.ReadListInFXNode(br); }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env) { WriteFXR1Varint(bw, 10); FloatTick.WriteListInFXNode(bw, env, Ticks); }
            }

            /// <summary>Colour key sequence (type 17: like 19).</summary>
            public class ColorSequenceNode17 : FXNode
            {
                public List<ColorTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env) { AssertFXR1Varint(br, 17); Ticks = ColorTick.ReadListInFXNode(br); }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env) { WriteFXR1Varint(bw, 17); ColorTick.WriteListInFXNode(bw, env, Ticks); }
            }

            /// <summary>Three-colour key sequence (type 30: like 21 / 29).</summary>
            public class Color3SequenceNode30 : FXNode
            {
                public List<Color3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env) { AssertFXR1Varint(br, 30); Ticks = Color3Tick.ReadListInFXNode(br); }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env) { WriteFXR1Varint(bw, 30); Color3Tick.WriteListInFXNode(bw, env, Ticks); }
            }

            /// <summary>A colour constant (type 15): an unknown varint (0 in vanilla), then RGBA.</summary>
            public class ColorNode15 : FXNode
            {
                public int Unk;
                public float R, G, B, A;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 15);
                    Unk = ReadFXR1Varint(br);
                    R = br.ReadSingle(); G = br.ReadSingle(); B = br.ReadSingle(); A = br.ReadSingle();
                }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 15);
                    WriteFXR1Varint(bw, Unk);
                    bw.WriteSingle(R); bw.WriteSingle(G); bw.WriteSingle(B); bw.WriteSingle(A);
                }
            }

            /// <summary>A texture id (type 40, as DLSE Param40).</summary>
            public class TextureNode40 : FXNode
            {
                public int TextureID;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env) { AssertFXR1Varint(br, 40); TextureID = ReadFXR1Varint(br); }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env) { WriteFXR1Varint(bw, 40); WriteFXR1Varint(bw, TextureID); }
            }

            /// <summary>A value with a float factor (type 82, as DLSE Param82): the wrapped node, then the float.</summary>
            public class FloatWrapNode82 : FXNode
            {
                public FXNode Node;
                public float Value;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 82);
                    int offset = ReadFXR1Varint(br);
                    Value = ReadFXR1Single(br);
                    Node = env.GetFXNode(br, offset);
                }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 82);
                    env.RegisterPointer(Node);
                    WriteFXR1Single(bw, Value);
                }
            }

            /// <summary>Two colours (type 83, as DLSE Param83): an unknown varint (0 in vanilla), then two RGBA.</summary>
            public class Color2Node83 : FXNode
            {
                public int Unk;
                public float[] Values = new float[8];
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 83);
                    Unk = ReadFXR1Varint(br);
                    Values = br.ReadSingles(8);
                }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 83);
                    WriteFXR1Varint(bw, Unk);
                    bw.WriteSingles(Values);
                }
            }

            /// <summary>A value with a colour factor (type 84, as DLSE Param84): the wrapped node, then RGBA.</summary>
            public class ColorWrapNode84 : FXNode
            {
                public FXNode Node;
                public float R, G, B, A;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 84);
                    int offset = ReadFXR1Varint(br);
                    R = br.ReadSingle(); G = br.ReadSingle(); B = br.ReadSingle(); A = br.ReadSingle();
                    Node = env.GetFXNode(br, offset);
                }
                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 84);
                    env.RegisterPointer(Node);
                    bw.WriteSingle(R); bw.WriteSingle(G); bw.WriteSingle(B); bw.WriteSingle(A);
                }
            }

            /// <summary>
            /// A container node (types 141–148): an int and an inline container. Every v2 root points to a 141 (its
            /// container lists a 142), and 142, 143, 146 and 147 nest the same way; 147's int is -4, -3, -2 or -1 and
            /// 143's 11, 10 or 8 (FXR3's operand and operator codes, perhaps). Meaning unknown.
            /// </summary>
            public class ContainerNode : FXNode
            {
                public int NodeType;
                public int Unk;
                public FXContainer Container;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    NodeType = ReadFXR1Varint(br);
                    Unk = ReadFXR1Varint(br);
                    Container = env.GetFXContainer(br, br.Position);
                    br.Position += FXContainer.GetSize(br.VarintLong);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, NodeType);
                    WriteFXR1Varint(bw, Unk);
                    Container.Write(bw, env);
                }
            }

            /// <summary>
            /// A float3 key sequence with a range (types 149, 150): keys as 13 / 14, then two floats stored after the
            /// values (in the same list; min / max by analogy with FXR1's ranged fields).
            /// </summary>
            public class RangedFloat3SequenceNode : FXNode
            {
                public int NodeType;
                public List<Float3Tick> Ticks;
                public float Min, Max;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    NodeType = ReadFXR1Varint(br);
                    var (times, valueOffset) = ReadTimes(br, out int count);
                    Ticks = new List<Float3Tick>(count);
                    br.StepIn(valueOffset);
                    for (int i = 0; i < count; i++)
                        Ticks.Add(new Float3Tick { Time = times[i], X = br.ReadSingle(), Y = br.ReadSingle(), Z = br.ReadSingle() });
                    Min = br.ReadSingle();
                    Max = br.ReadSingle();
                    br.StepOut();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, NodeType);
                    var times = new List<float>(Ticks.Count);
                    var values = new List<float>(Ticks.Count * 3 + 2);
                    foreach (var t in Ticks)
                    {
                        times.Add(t.Time);
                        values.Add(t.X); values.Add(t.Y); values.Add(t.Z);
                    }
                    values.Add(Min);
                    values.Add(Max);
                    env.RegisterPointer(times);
                    env.RegisterPointer(values);
                    WriteFXR1Varint(bw, Ticks.Count);
                }
            }

            /// <summary>
            /// A three-colour key sequence with a range (types 151, 152): keys as 21 / 22, then two colours stored after
            /// the values (min / max by analogy).
            /// </summary>
            public class RangedColor3SequenceNode : FXNode
            {
                public int NodeType;
                public List<Color3Tick> Ticks;
                public float[] Range = new float[8];

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    NodeType = ReadFXR1Varint(br);
                    var (times, valueOffset) = ReadTimes(br, out int count);
                    Ticks = new List<Color3Tick>(count);
                    br.StepIn(valueOffset);
                    for (int i = 0; i < count; i++)
                    {
                        var v = br.ReadSingles(12);
                        Ticks.Add(new Color3Tick
                        {
                            Time = times[i],
                            R1 = v[0], G1 = v[1], B1 = v[2], A1 = v[3],
                            R2 = v[4], G2 = v[5], B2 = v[6], A2 = v[7],
                            R3 = v[8], G3 = v[9], B3 = v[10], A3 = v[11],
                        });
                    }
                    Range = br.ReadSingles(8);
                    br.StepOut();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, NodeType);
                    var times = new List<float>(Ticks.Count);
                    var values = new VectorList(Ticks.Count * 12 + 8);
                    foreach (var t in Ticks)
                    {
                        times.Add(t.Time);
                        values.AddRange(new[] { t.R1, t.G1, t.B1, t.A1, t.R2, t.G2, t.B2, t.A2, t.R3, t.G3, t.B3, t.A3 });
                    }
                    values.AddRange(Range);
                    env.RegisterPointer(times);
                    env.RegisterPointer(values);
                    WriteFXR1Varint(bw, Ticks.Count);
                }
            }
        }
    }
}
