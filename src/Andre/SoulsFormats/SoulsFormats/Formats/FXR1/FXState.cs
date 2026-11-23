using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(FXStateRef))]
        public class FXState : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenStates;

            public List<FXTransition> Transitions;
            public List<FXAction> Actions;

            public virtual bool ShouldSerializeEdges() => true;
            public virtual bool ShouldSerializeActions() => true;

            internal override void ToXIDs(FXR1 fxr)
            {
                for (int i = 0; i < Transitions.Count; i++)
                    Transitions[i] = fxr.ReferenceTransition(Transitions[i]);
                for (int i = 0; i < Actions.Count; i++)
                    Actions[i] = fxr.ReferenceFXAction(Actions[i]);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                for (int i = 0; i < Transitions.Count; i++)
                    Transitions[i] = fxr.DereferenceTransition(Transitions[i]);
                for (int i = 0; i < Actions.Count; i++)
                    Actions[i] = fxr.DereferenceFXAction(Actions[i]);
            }

            public static int GetSize(bool isLong)
                => isLong ? 24 : 16;

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                int edgesOffset = ReadFXR1Varint(br);
                int actionsOffset = ReadFXR1Varint(br);
                int edgeNum = br.ReadInt32();
                int actionNum = br.ReadInt32();

                Transitions = new List<FXTransition>(edgeNum);
                Actions = new List<FXAction>(actionNum);

                br.StepIn(edgesOffset);
                for (int i = 0; i < edgeNum; i++)
                {
                    Transitions.Add(env.GetFXTransition(br, br.Position));
                    br.Position += FXTransition.GetSize(br.VarintLong);
                }
                br.StepOut();

                br.StepIn(actionsOffset);
                for (int i = 0; i < actionNum; i++)
                {
                    Actions.Add(env.GetFXAction(br, br.Position));
                    br.Position += FXAction.GetSize(br.VarintLong);
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterPointer(Transitions);
                env.RegisterPointer(Actions);
                bw.WriteInt32(Transitions.Count);
                bw.WriteInt32(Actions.Count);
            }
        }

        public class FXStateRef : FXState
        {
            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeEdges() => false;
            public override bool ShouldSerializeActions() => false;

            public FXStateRef(FXState refVal)
            {
                ReferenceXID = refVal?.XID;
            }
            public FXStateRef()
            {

            }
        }
    }
}
