using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(FXTransitionRef))]
        public class FXTransition : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenTransitions;

            public FXState TargetState;
            public FXNode EvaluatorNode;

            public virtual bool ShouldSerializeTargetState() => true;
            public virtual bool ShouldSerializeEvaluatorNode() => true;

            internal override void ToXIDs(FXR1 fxr)
            {
                TargetState = fxr.ReferenceState(TargetState);
                EvaluatorNode = fxr.ReferenceFXNode(EvaluatorNode);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                TargetState = fxr.DereferenceState(TargetState);
                EvaluatorNode = fxr.DereferenceFXNode(EvaluatorNode);
            }

            public static int GetSize(bool isLong)
                => isLong ? 16 : 8;

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                int endNodeOffset = ReadFXR1Varint(br);
                int functionOffset = ReadFXR1Varint(br);

                TargetState = env.GetFXState(br, endNodeOffset);
                EvaluatorNode = env.GetFXNode(br, functionOffset);
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);
                env.RegisterPointer(TargetState);
                env.RegisterPointer(EvaluatorNode);
            }
        }

        public class FXTransitionRef : FXTransition
        {
            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeTargetState() => false;
            public override bool ShouldSerializeEvaluatorNode() => false;

            public FXTransitionRef(FXTransition refVal)
            {
                ReferenceXID = refVal?.XID;
            }
            public FXTransitionRef()
            {

            }
        }
    }
}
