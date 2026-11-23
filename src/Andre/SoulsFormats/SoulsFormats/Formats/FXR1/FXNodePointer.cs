using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(FXNodePointerRef))]
        public class FXNodePointer : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenFXNodePointers;

            public static int GetSize(bool isLong)
                => isLong ? 8 : 4;

            public FXNode Node;

            internal override void ToXIDs(FXR1 fxr)
            {
                Node = fxr.ReferenceFXNode(Node);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                Node = fxr.DereferenceFXNode(Node);
            }

            public virtual bool ShouldSerializeNode() => true;

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                long funcOffset = ReadFXR1Varint(br);
                Node = env.GetFXNode(br, funcOffset);
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);
                env.RegisterPointer(Node);
            }
        }

        public class FXNodePointerRef : FXNodePointer
        {
            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeNode() => false;

            public FXNodePointerRef(FXNodePointer refVal)
            {
                ReferenceXID = refVal.XID;
            }
            public FXNodePointerRef()
            {

            }
        }
    }
}
