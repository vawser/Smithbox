using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Trigger : FXSerializable
        {
            internal override string ClassName => "FXSerializableTrigger";

            internal override int Version => 1;

            [XmlAttribute]
            public int StateIndex { get; set; }

            public Evaluatable Evaluator { get; set; }

            public Trigger() { }

            internal Trigger(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                StateIndex = br.ReadInt32();
                Evaluator = Evaluatable.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                Evaluator.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(StateIndex);
                Evaluator.Write(bw, classNames);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
