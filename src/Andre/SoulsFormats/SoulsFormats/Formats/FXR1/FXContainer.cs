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

            public static int GetSize(bool isLong)
                => isLong ? 40 : 24;

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
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
