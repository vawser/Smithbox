using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(FXContainerRef))]
        public class FXContainer : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenFXContainers;

            [XmlAttribute]
            public byte UnkFlag1;
            [XmlAttribute]
            public byte UnkFlag2;
            [XmlAttribute]
            public byte UnkFlag3;

            public List<FXNode> FXNodes;
            public FXActionData ActionData;
            public FXModifier Modifier;

            public virtual bool ShouldSerializeUnkFlag1() => true;
            public virtual bool ShouldSerializeUnkFlag2() => true;
            public virtual bool ShouldSerializeUnkFlag3() => true;
            public virtual bool ShouldSerializeFXNodes() => true;
            public virtual bool ShouldSerializeActionData() => true;
            public virtual bool ShouldSerializeModifier() => true;

            internal override void ToXIDs(FXR1 fxr)
            {
                ActionData = fxr.ReferenceFXActionData(ActionData);
                Modifier = fxr.ReferenceModifier(Modifier);
                for (int i = 0; i < FXNodes.Count; i++)
                    FXNodes[i] = fxr.ReferenceFXNode(FXNodes[i]);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                ActionData = fxr.DereferenceFXActionData(ActionData);
                Modifier = fxr.DereferenceModifier(Modifier);
                for (int i = 0; i < FXNodes.Count; i++)
                    FXNodes[i] = fxr.DereferenceFXNode(FXNodes[i]);
            }

            /// <summary>
            /// FXR v2: the node count stored a second time. Most containers store the count twice, as v1 does; 15,126
            /// vanilla action-call containers store a smaller number here (meaning unknown). -1: the same as the count.
            /// </summary>
            public int V2Count2 = -1;

            /// <summary>FXR v2: the two words after the counts (v1's flag bytes and padding): 4 or 0 and 0 in vanilla, kept as read.</summary>
            public int V2Unk10;
            public int V2Unk14;

            /// <summary>FXR v2: the small record some action-call containers point to, where v1 has ActionData.</summary>
            public FXRecord Record;

            /// <summary>FXR v2: how many leading nodes were in the packed tail when read; -1 for a new container (the writer then decides by action type).</summary>
            public int V2PackedRead = -1;

            /// <summary>The node pointer list made by the last write (the v2 header points at the first root's).</summary>
            internal List<FXNodePointer> WrittenPointers;

            public static int GetSize(bool isLong)
                => isLong ? (IsV2 ? 32 : 40) : 24;

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                if (IsV2)
                {
                    int listOffset = ReadFXR1Varint(br);
                    int count = br.ReadInt32();
                    int count2 = br.ReadInt32();
                    V2Count2 = count2 == count ? -1 : count2;
                    V2Unk10 = br.ReadInt32();
                    V2Unk14 = br.ReadInt32();
                    int recordOffset = ReadFXR1Varint(br);

                    br.StepIn(listOffset);
                    FXNodes = new List<FXNode>(count);
                    for (int i = 0; i < count; i++)
                    {
                        FXNodes.Add(env.GetFXNodePointer(br, br.Position).Node);
                        br.Position += FXNodePointer.GetSize(br.VarintLong);
                    }
                    br.StepOut();
                    Record = env.GetFXRecord(br, recordOffset);

                    // How many leading nodes vanilla laid out in the packed tail: kept, since a few files (another
                    // build of the tool?) leave a packed action type unpacked (FFX plan §19)
                    int lead = 0;
                    while (lead < FXNodes.Count && FXNodes[lead] != null && env.OffsetsByObject.TryGetValue(FXNodes[lead], out long nodeAt) && nodeAt > v2TailStart)
                        lead++;
                    V2PackedRead = lead;
                    return;
                }

                int commandPool1TableOffset = ReadFXR1Varint(br);
                int commandPool1TableCount = br.ReadInt32();
                br.AssertInt32(commandPool1TableCount);
                UnkFlag1 = br.ReadByte();
                UnkFlag2 = br.ReadByte();
                UnkFlag3 = br.ReadByte();
                br.AssertByte(0);

                AssertFXR1Garbage(br); // ???

                int commandPool2Offset = ReadFXR1Varint(br);
                int commandPool3Offset = ReadFXR1Varint(br);

                br.StepIn(commandPool1TableOffset);
                FXNodes = new List<FXNode>(commandPool1TableCount);
                for (int i = 0; i < commandPool1TableCount; i++)
                {
                    //int next = br.ReadInt32();
                    //ast.Pool1List.Add(env.GetEffectPool1(br, next));
                    var paramPointer = env.GetFXNodePointer(br, br.Position);
                    FXNodes.Add(paramPointer.Node);
                    br.Position += FXNodePointer.GetSize(br.VarintLong);
                }
                br.StepOut();

                ActionData = env.GetFXActionData(br, commandPool2Offset);
                Modifier = env.GetFXModifier(br, commandPool3Offset);
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);

                var nodePointers = new List<FXNodePointer>();

                for (int i = 0; i < FXNodes.Count; i++)
                {
                    nodePointers.Add(new FXNodePointer() { Node = FXNodes[i] });
                }

                WrittenPointers = nodePointers;
                env.NoteContainerList(this, nodePointers);

                if (IsV2)
                {
                    env.RegisterPointer(nodePointers);
                    bw.WriteInt32(nodePointers.Count);
                    bw.WriteInt32(V2Count2 >= 0 ? V2Count2 : nodePointers.Count);
                    bw.WriteInt32(V2Unk10);
                    bw.WriteInt32(V2Unk14);
                    env.RegisterPointer(Record);
                    return;
                }

                if (ActionData != null)
                    ActionData.ParentContainer = this;

                env.RegisterPointer(nodePointers);
                bw.WriteInt32(nodePointers.Count);
                bw.WriteInt32(nodePointers.Count);
                bw.WriteByte(UnkFlag1);
                bw.WriteByte(UnkFlag2);
                bw.WriteByte(UnkFlag3);
                bw.WriteByte(0);
                WriteFXR1Garbage(bw);
                env.RegisterPointer(ActionData);
                env.RegisterPointer(Modifier);
            }
        }

        public class FXContainerRef : FXContainer
        {
            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeUnkFlag1() => false;
            public override bool ShouldSerializeUnkFlag2() => false;
            public override bool ShouldSerializeUnkFlag3() => false;
            public override bool ShouldSerializeFXNodes() => false;
            public override bool ShouldSerializeActionData() => false;
            public override bool ShouldSerializeModifier() => false;

            public FXContainerRef(FXContainer refVal)
            {
                ReferenceXID = refVal?.XID;
            }
            public FXContainerRef() 
            {

            }
        }
    }
}
