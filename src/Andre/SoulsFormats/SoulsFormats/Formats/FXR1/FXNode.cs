using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(IntNode))]
        [XmlInclude(typeof(IntArrayNode))]
        [XmlInclude(typeof(IntSequenceNode3))]
        [XmlInclude(typeof(IntSequenceNode5))]
        [XmlInclude(typeof(IntSequenceNode6))]
        [XmlInclude(typeof(FloatNode))]
        [XmlInclude(typeof(IntSequenceNode9))]
        [XmlInclude(typeof(FloatSequenceNode11))]
        [XmlInclude(typeof(FloatSequenceNode12))]
        [XmlInclude(typeof(Float3SequenceNode13))]
        [XmlInclude(typeof(Float3SequenceNode14))]
        [XmlInclude(typeof(ColorSequenceNode19))]
        [XmlInclude(typeof(ColorSequenceNode20))]
        [XmlInclude(typeof(Color3SequenceNode21))]
        [XmlInclude(typeof(Color3SequenceNode22))]
        [XmlInclude(typeof(ColorSequenceNode27))]
        [XmlInclude(typeof(ColorSequenceNode28))]
        [XmlInclude(typeof(Color3SequenceNode29))]
        [XmlInclude(typeof(EffectCallNode))]
        [XmlInclude(typeof(ActionCallNode))]
        [XmlInclude(typeof(IntNode41))]
        [XmlInclude(typeof(IndexedIntNode))]
        [XmlInclude(typeof(IndexedIntArrayNode))]
        [XmlInclude(typeof(IndexedIntSequenceNode))]
        [XmlInclude(typeof(IndexedFloatNode))]
        [XmlInclude(typeof(IndexedEffectNode))]
        [XmlInclude(typeof(IndexedActionNode))]
        [XmlInclude(typeof(IndexedSoundIDNode))]
        [XmlInclude(typeof(SoundIDNode68))]
        [XmlInclude(typeof(SoundIDNode69))]
        [XmlInclude(typeof(TickNode))]
        [XmlInclude(typeof(IndexedTickNode))]
        [XmlInclude(typeof(Int2Node))]
        [XmlInclude(typeof(Float2Node))]
        [XmlInclude(typeof(Tick2Node))]
        [XmlInclude(typeof(IndexedTick2Node))]
        [XmlInclude(typeof(IntSequenceNode89))]
        [XmlInclude(typeof(FloatSequenceNodeNode91))]
        [XmlInclude(typeof(FloatSequenceNodeNode95))]
        [XmlInclude(typeof(IntNode111))]
        [XmlInclude(typeof(EmptyNode112))]
        [XmlInclude(typeof(EmptyNode113))]
        [XmlInclude(typeof(UnkIndexedValueNode114))]
        [XmlInclude(typeof(UnkIndexedValueNode115))]
        [XmlInclude(typeof(Node2Node120))]
        [XmlInclude(typeof(Node2Node121))]
        [XmlInclude(typeof(Node2Node122))]
        [XmlInclude(typeof(Node2Node123))]
        [XmlInclude(typeof(Node2Node124))]
        [XmlInclude(typeof(Node2Node126))]
        [XmlInclude(typeof(Node2Node127))]
        [XmlInclude(typeof(NodeNode128))]
        [XmlInclude(typeof(EmptyNode129))]
        [XmlInclude(typeof(EmptyNode130))]
        [XmlInclude(typeof(EmptyNode131))]
        [XmlInclude(typeof(EmptyNode132))]
        [XmlInclude(typeof(EffectCreateNode133))]
        [XmlInclude(typeof(EffectCreateNode134))]
        [XmlInclude(typeof(EmptyNode136))]
        [XmlInclude(typeof(FXNode137))]
        [XmlInclude(typeof(FXNode138))]
        [XmlInclude(typeof(FXNode139))]
        [XmlInclude(typeof(FXNode140))]
        [XmlInclude(typeof(FXNodeRef))]
        public abstract class FXNode : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenFXNodes;

            //public readonly long ID;
            //internal FXNode(long id)
            //{
            //    ID = id;
            //}
            internal abstract void ReadInner(BinaryReaderEx br, FxrEnvironment env);
            internal abstract void WriteInner(BinaryWriterEx bw, FxrEnvironment env);

            //long DEBUG_DataSizeOnRead = -1;

            internal override void ToXIDs(FXR1 fxr)
            {
                InnerToXIDs(fxr);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                InnerFromXIDs(fxr);
            }

            internal virtual void InnerToXIDs(FXR1 fxr)
            {

            }

            internal virtual void InnerFromXIDs(FXR1 fxr)
            {

            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);
                env.RegisterFXNodeOffsetHere();
                //long start = bw.Position;
                //Console.WriteLine($"TYPE: {GetType().Name}");
                WriteInner(bw, env);
                //long end = bw.Position;
                //long dataLength = end - start;
                //if (dataLength != DEBUG_DataSizeOnRead)
                //{
                //    Console.WriteLine($"Warning: NodeType[{GetType().Name}] Read {DEBUG_DataSizeOnRead} bytes of data but wrote {dataLength} bytes.");
                //}
            }

            internal static FXNode GetProperFXNodeType(BinaryReaderEx br, FxrEnvironment env)
            {
                long functionID = GetFXR1Varint(br, br.Position);
                FXNode func = null;
                switch (functionID)
                {
                    case 1: func = new IntNode(); break;
                    case 2: func = new IntArrayNode(); break;
                    case 3: func = new IntSequenceNode3(); break;
                    case 5: func = new IntSequenceNode5(); break;
                    case 6: func = new IntSequenceNode6(); break;
                    case 7: func = new FloatNode(); break;
                    case 9: func = new IntSequenceNode9(); break;
                    case 11: func = new FloatSequenceNode11(); break;
                    case 12: func = new FloatSequenceNode12(); break;
                    case 13: func = new Float3SequenceNode13(); break;
                    case 14: func = new Float3SequenceNode14(); break;
                    case 19: func = new ColorSequenceNode19(); break;
                    case 20: func = new ColorSequenceNode20(); break;
                    case 21: func = new Color3SequenceNode21(); break;
                    case 22: func = new Color3SequenceNode22(); break;
                    case 27: func = new ColorSequenceNode27(); break;
                    case 28: func = new ColorSequenceNode28(); break;
                    case 29: func = new Color3SequenceNode29(); break;
                    case 37: func = new EffectCallNode(); break;
                    case 38: func = new ActionCallNode(); break;
                    case 41: func = new IntNode41(); break;
                    case 44: func = new IndexedIntNode(); break;
                    case 45: func = new IndexedIntArrayNode(); break;
                    case 46: func = new IndexedIntSequenceNode(); break;
                    case 47: func = new IndexedFloatNode(); break;
                    case 59: func = new IndexedEffectNode(); break;
                    case 60: func = new IndexedActionNode(); break;
                    case 66: func = new IndexedSoundIDNode(); break;
                    case 68: func = new SoundIDNode68(); break;
                    case 69: func = new SoundIDNode69(); break;
                    case 70: func = new TickNode(); break;
                    case 71: func = new IndexedTickNode(); break;
                    case 79: func = new Int2Node(); break;
                    case 81: func = new Float2Node(); break;
                    case 85: func = new Tick2Node(); break;
                    case 87: func = new IndexedTick2Node(); break;
                    case 89: func = new IntSequenceNode89(); break;
                    case 91: func = new FloatSequenceNodeNode91(); break;
                    case 95: func = new FloatSequenceNodeNode95(); break;
                    case 111: func = new IntNode111(); break;
                    case 112: func = new EmptyNode112(); break;
                    case 113: func = new EmptyNode113(); break;
                    case 114: func = new UnkIndexedValueNode114(); break;
                    case 115: func = new UnkIndexedValueNode115(); break;
                    case 120: func = new Node2Node120(); break;
                    case 121: func = new Node2Node121(); break;
                    case 122: func = new Node2Node122(); break;
                    case 123: func = new Node2Node123(); break;
                    case 124: func = new Node2Node124(); break;
                    case 126: func = new Node2Node126(); break;
                    case 127: func = new Node2Node127(); break;
                    case 128: func = new NodeNode128(); break;
                    case 129: func = new EmptyNode129(); break;
                    case 130: func = new EmptyNode130(); break;
                    case 131: func = new EmptyNode131(); break;
                    case 132: func = new EmptyNode132(); break;
                    case 133: func = new EffectCreateNode133(); break;
                    case 134: func = new EffectCreateNode134(); break;
                    case 136: func = new EmptyNode136(); break;
                    case 137: func = new FXNode137(); break;
                    case 138: func = new FXNode138(); break;
                    case 139: func = new FXNode139(); break;
                    case 140: func = new FXNode140(); break;
                    default:
                        throw new NotImplementedException();
                }

                return func;
            }

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                //long start = br.Position;
                ReadInner(br, env);
                //long end = br.Position;
                //DEBUG_DataSizeOnRead = end - start;
            }

            public class FXNodeRef : FXNode
            {
                public string ReferenceXID;

                public FXNodeRef(FXNode refVal)
                {
                    ReferenceXID = refVal?.XID;
                }

                public FXNodeRef()
                {

                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    throw new InvalidOperationException("Cannot actually deserialize a FXNodeRef.");
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    throw new InvalidOperationException("Cannot actually serialize a FXNodeRef.");
                }
            }

            public class EffectCallNode : FXNode
            {
                public int EffectID;
                public FXContainer Container;
                public int Unk;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Container = fxr.ReferenceFXContainer(Container);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Container = fxr.DereferenceFXContainer(Container);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 37);

                    EffectID = ReadFXR1Varint(br);
                    int astOffset = ReadFXR1Varint(br);
                    Unk = ReadFXR1Varint(br);

                    Container = env.GetFXContainer(br, astOffset);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 37);

                    WriteFXR1Varint(bw, EffectID);
                    env.RegisterPointer(Container);
                    WriteFXR1Varint(bw, Unk);
                }
            }

            public class ActionCallNode : FXNode
            {
                [XmlAttribute]
                public int ActionType;
                public FXContainer Container;
                [XmlAttribute]
                public int Unk;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Container = fxr.ReferenceFXContainer(Container);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Container = fxr.DereferenceFXContainer(Container);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 38);

                    ActionType = ReadFXR1Varint(br);
                    int astOffset = ReadFXR1Varint(br);
                    Unk = ReadFXR1Varint(br);

                    Container = env.GetFXContainer(br, astOffset);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 38);

                    WriteFXR1Varint(bw, ActionType);
                    env.RegisterPointer(Container);
                    WriteFXR1Varint(bw, Unk);
                }
            }

            public class EffectCreateNode133 : FXNode
            {
                public int EffectID;
                public int Unk;
                public FXContainer Container1;
                public FXContainer Container2;
                public List<FXState> States;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    for (int i = 0; i < States.Count; i++)
                        States[i] = fxr.ReferenceState(States[i]);
                    Container1 = fxr.ReferenceFXContainer(Container1);
                    Container2 = fxr.ReferenceFXContainer(Container2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    for (int i = 0; i < States.Count; i++)
                        States[i] = fxr.DereferenceState(States[i]);
                    Container1 = fxr.DereferenceFXContainer(Container1);
                    Container2 = fxr.DereferenceFXContainer(Container2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 133);

                    EffectID = ReadFXR1Varint(br);
                    for (int i = 0; i < 7; i++)
                        AssertFXR1Varint(br, 0);
                    Unk = ReadFXR1Varint(br);
                    //throw new NotImplementedException();

                    Container1 = env.GetFXContainer(br, br.Position);
                    br.Position += FXContainer.GetSize(br.VarintLong);

                    Container2 = env.GetFXContainer(br, br.Position);
                    br.Position += FXContainer.GetSize(br.VarintLong);

                    int offsetToNodeList = ReadFXR1Varint(br);
                    int nodeCount = ReadFXR1Varint(br);
                    States = new List<FXState>();
                    br.StepIn(offsetToNodeList);
                    for (int i = 0; i < nodeCount; i++)
                    {
                        States.Add(env.GetFXState(br, br.Position));
                        br.Position += FXState.GetSize(br.VarintLong);
                    }
                    br.StepOut();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 133);

                    WriteFXR1Varint(bw, EffectID);
                    for (int i = 0; i < 7; i++)
                        WriteFXR1Varint(bw, 0);
                    WriteFXR1Varint(bw, Unk);
                    Container1.Write(bw, env);
                    Container2.Write(bw, env);
                    env.RegisterPointer(States);
                    WriteFXR1Varint(bw, States.Count);
                }
            }

            public class EffectCreateNode134 : FXNode
            {
                public int EffectID;
                public int Unk;
                public List<FXNode> Nodes;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    for (int i = 0; i < Nodes.Count; i++)
                        Nodes[i] = fxr.ReferenceFXNode(Nodes[i]);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    for (int i = 0; i < Nodes.Count; i++)
                        Nodes[i] = fxr.DereferenceFXNode(Nodes[i]);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 134);

                    EffectID = ReadFXR1Varint(br);
                    Unk = ReadFXR1Varint(br);
                    int offsetToNodeOffsetList = ReadFXR1Varint(br);
                    int funcCount = ReadFXR1Varint(br);
                    Nodes = new List<FXNode>(funcCount);
                    br.StepIn(offsetToNodeOffsetList);
                    for (int i = 0; i < funcCount; i++)
                    {
                        // A varint: 8 bytes in DS1R (the reference read 4, so every other root was lost there)
                        int nextNodeOffset = ReadFXR1Varint(br);
                        var func = env.GetFXNode(br, nextNodeOffset);
                        Nodes.Add(func);
                    }
                    br.StepOut();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 134);

                    WriteFXR1Varint(bw, EffectID);
                    WriteFXR1Varint(bw, Unk);
                    env.RegisterPointer(Nodes);
                    WriteFXR1Varint(bw, Nodes.Count);
                }
            }

            public class IntNode : FXNode
            {
                [XmlAttribute]
                public int Value;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 1);

                    Value = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 1);

                    WriteFXR1Varint(bw, Value);
                }
            }

            public class IntArrayNode : FXNode
            {
                public List<int> Values;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 2);

                    int listOffset = ReadFXR1Varint(br);
                    int listCount = ReadFXR1Varint(br);

                    Values = new List<int>(listCount);

                    br.StepIn(listOffset);
                    for (int i = 0; i < listCount; i++)
                    {
                        Values.Add(br.ReadInt32());
                    }
                    br.StepOut();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 2);

                    env.RegisterPointer(Values);
                    WriteFXR1Varint(bw, Values.Count);
                }
            }

            public class IntSequenceNode3 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 3);

                    Ticks = IntTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 3);

                    IntTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class IntSequenceNode5 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 5);

                    Ticks = IntTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 5);

                    IntTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class IntSequenceNode6 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 6);

                    Ticks = IntTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 6);

                    IntTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class FloatNode : FXNode
            {
                [XmlAttribute]
                public float Value;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 7);

                    Value = ReadFXR1Single(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 7);

                    WriteFXR1Single(bw, Value);
                }
            }

            public class IntSequenceNode9 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 9);

                    Ticks = IntTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 9);

                    IntTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class FloatSequenceNode11 : FXNode
            {
                public List<FloatTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 11);

                    Ticks = FloatTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 11);

                    FloatTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class FloatSequenceNode12 : FXNode
            {
                public List<FloatTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 12);

                    Ticks = FloatTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 12);

                    FloatTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class Float3SequenceNode13 : FXNode
            {
                public List<Float3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 13);

                    Ticks = Float3Tick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 13);

                    Float3Tick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class Float3SequenceNode14 : FXNode
            {
                public List<Float3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 14);

                    Ticks = Float3Tick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 14);

                    Float3Tick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class ColorSequenceNode19 : FXNode
            {
                public List<ColorTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 19);

                    Ticks = ColorTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 19);

                    ColorTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class ColorSequenceNode20 : FXNode
            {
                public List<ColorTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 20);

                    Ticks = ColorTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 20);

                    ColorTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class Color3SequenceNode21 : FXNode
            {
                public List<Color3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 21);

                    Ticks = Color3Tick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 21);

                    Color3Tick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class Color3SequenceNode22 : FXNode
            {
                public List<Color3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 22);

                    Ticks = Color3Tick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 22);

                    Color3Tick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class ColorSequenceNode27 : FXNode
            {
                public List<ColorTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 27);

                    Ticks = ColorTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 27);

                    ColorTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class ColorSequenceNode28 : FXNode
            {
                public List<ColorTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 28);

                    Ticks = ColorTick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 28);

                    ColorTick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class Color3SequenceNode29 : FXNode
            {
                public List<Color3Tick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 29);

                    Ticks = Color3Tick.ReadListInFXNode(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 29);

                    Color3Tick.WriteListInFXNode(bw, env, Ticks);
                }
            }

            public class IntNode41 : FXNode
            {
                [XmlAttribute]
                public int Value;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 41);

                    Value = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 41);

                    WriteFXR1Varint(bw, Value);
                }
            }

            public class IndexedIntNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 44);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 44);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IndexedIntArrayNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 45);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 45);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IndexedIntSequenceNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 46);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 46);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IndexedFloatNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 47);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 47);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IndexedEffectNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 59);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 59);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IndexedActionNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 60);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 60);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }


            public class IndexedSoundIDNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 66);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 66);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class SoundIDNode68 : FXNode
            {
                [XmlAttribute]
                public int SoundID;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 68);

                    SoundID = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 68);

                    WriteFXR1Varint(bw, SoundID);
                }
            }

            public class SoundIDNode69 : FXNode
            {
                [XmlAttribute]
                public int SoundID;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 69);

                    SoundID = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 69);

                    WriteFXR1Varint(bw, SoundID);
                }
            }

            public class TickNode : FXNode
            {
                [XmlAttribute]
                public float Tick;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 70);

                    Tick = ReadFXR1Single(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 70);

                    WriteFXR1Single(bw, Tick);
                }
            }


            public class IndexedTickNode : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 71);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 71);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class Int2Node : FXNode
            {
                [XmlAttribute]
                public int X;
                [XmlAttribute]
                public int Y;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 79);

                    X = br.ReadInt32();
                    Y = br.ReadInt32();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 79);

                    bw.WriteInt32(X);
                    bw.WriteInt32(Y);
                }
            }

            public class Float2Node : FXNode
            {
                [XmlAttribute]
                public float X;
                [XmlAttribute]
                public float Y;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 81);

                    X = br.ReadSingle();
                    Y = br.ReadSingle();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 81);

                    bw.WriteSingle(X);
                    bw.WriteSingle(Y);
                }
            }

            public class Tick2Node : FXNode
            {
                [XmlAttribute]
                public float Tick1;
                [XmlAttribute]
                public float Tick2;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 85);

                    Tick1 = br.ReadSingle();
                    Tick2 = br.ReadSingle();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 85);

                    bw.WriteSingle(Tick1);
                    bw.WriteSingle(Tick2);
                }
            }

            public class IndexedTick2Node : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 87);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 87);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class IntSequenceNode89 : FXNode
            {
                public List<IntTick> Ticks;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 89);

                    Ticks = IntTick.ReadListInFXNode(br);
                    AssertFXR1Varint(br, 1);
                    AssertFXR1Varint(br, 0);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 89);

                    IntTick.WriteListInFXNode(bw, env, Ticks);
                    WriteFXR1Varint(bw, 1);
                    WriteFXR1Varint(bw, 0);
                }
            }

            public class FloatSequenceNodeNode91 : FXNode
            {
                public List<FloatTick> Ticks;
                public FXNode Node;
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 91);

                    Ticks = FloatTick.ReadListInFXNode(br);
                    AssertFXR1Varint(br, 1);
                    long paramOffset = ReadFXR1Varint(br);
                    Node = env.GetFXNode(br, paramOffset);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 91);

                    FloatTick.WriteListInFXNode(bw, env, Ticks);
                    WriteFXR1Varint(bw, 1);
                    env.RegisterPointer(Node);
                }
            }

            public class FloatSequenceNodeNode95 : FXNode
            {
                public List<FloatTick> Ticks;
                public FXNode Node;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node = fxr.ReferenceFXNode(Node);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node = fxr.DereferenceFXNode(Node);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 95);

                    Ticks = FloatTick.ReadListInFXNode(br);
                    AssertFXR1Varint(br, 1);
                    long paramOffset = ReadFXR1Varint(br);
                    Node = env.GetFXNode(br, paramOffset);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 95);

                    FloatTick.WriteListInFXNode(bw, env, Ticks);
                    WriteFXR1Varint(bw, 1);
                    env.RegisterPointer(Node);
                }
            }

            public class IntNode111 : FXNode
            {
                [XmlAttribute]
                public int Value;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 111);

                    Value = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 111);

                    WriteFXR1Varint(bw, Value);
                }
            }

            public class EmptyNode112 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 112);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 112);
                }
            }

            public class EmptyNode113 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 113);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 113);
                }
            }

            public class UnkIndexedValueNode114 : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 114);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 114);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class UnkIndexedValueNode115 : FXNode
            {
                [XmlAttribute]
                public short Type;
                [XmlAttribute]
                public short Index;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 115);

                    Type = br.ReadInt16();
                    Index = br.ReadInt16();

                    AssertFXR1Garbage(br); //????
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 115);

                    bw.WriteInt16(Type);
                    bw.WriteInt16(Index);

                    WriteFXR1Garbage(bw); //????
                }
            }

            public class Node2Node120 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 120);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 120);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class Node2Node121 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 121);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 121);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class Node2Node122 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 122);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 122);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class Node2Node123 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 123);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 123);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class Node2Node124 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 124);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 124);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }
            public class Node2Node126 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 126);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 126);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class Node2Node127 : FXNode
            {
                public FXNode Node1;
                public FXNode Node2;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node1 = fxr.ReferenceFXNode(Node1);
                    Node2 = fxr.ReferenceFXNode(Node2);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node1 = fxr.DereferenceFXNode(Node1);
                    Node2 = fxr.DereferenceFXNode(Node2);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 127);

                    int funcOffset1 = ReadFXR1Varint(br);
                    int funcOffset2 = ReadFXR1Varint(br);

                    Node1 = env.GetFXNode(br, funcOffset1);
                    Node2 = env.GetFXNode(br, funcOffset2);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 127);

                    env.RegisterPointer(Node1);
                    env.RegisterPointer(Node2);
                }
            }

            public class NodeNode128 : FXNode
            {
                public FXNode Node;

                internal override void InnerToXIDs(FXR1 fxr)
                {
                    Node = fxr.ReferenceFXNode(Node);
                }

                internal override void InnerFromXIDs(FXR1 fxr)
                {
                    Node = fxr.DereferenceFXNode(Node);
                }

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 128);

                    int funcOffset = ReadFXR1Varint(br);

                    Node = env.GetFXNode(br, funcOffset);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 128);

                    env.RegisterPointer(Node);
                }
            }

            public class EmptyNode129 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 129);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 129);
                }
            }

            public class EmptyNode130 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 130);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 130);
                }
            }

            public class EmptyNode131 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 131);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 131);
                }
            }

            public class EmptyNode132 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 132);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 132);
                }
            }

            public class EmptyNode136 : FXNode
            {
                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 136);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 136);
                }
            }

            public class FXNode137 : FXNode
            {
                [XmlAttribute]
                public int Unk1;
                [XmlAttribute]
                public int Unk2;
                [XmlAttribute]
                public int Unk3;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 137);

                    Unk1 = ReadFXR1Varint(br);
                    Unk2 = ReadFXR1Varint(br);
                    Unk3 = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 137);

                    WriteFXR1Varint(bw, Unk1);
                    WriteFXR1Varint(bw, Unk2);
                    WriteFXR1Varint(bw, Unk3);
                }
            }

            public class FXNode138 : FXNode
            {
                [XmlAttribute]
                public int Unk1;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 138);

                    Unk1 = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 138);

                    WriteFXR1Varint(bw, Unk1);
                }
            }

            public class FXNode139 : FXNode
            {
                [XmlAttribute]
                public int Unk1;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 139);

                    Unk1 = ReadFXR1Varint(br);
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 139);

                    WriteFXR1Varint(bw, Unk1);
                }
            }

            public class FXNode140 : FXNode
            {
                [XmlAttribute]
                public int Unk1;
                [XmlAttribute]
                public int Unk2;

                internal override void ReadInner(BinaryReaderEx br, FxrEnvironment env)
                {
                    AssertFXR1Varint(br, 140);

                    Unk1 = br.ReadInt32();
                    Unk2 = br.ReadInt32();
                }

                internal override void WriteInner(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteFXR1Varint(bw, 140);

                    bw.WriteInt32(Unk1);
                    bw.WriteInt32(Unk2);
                }
            }


        }
    }
}
