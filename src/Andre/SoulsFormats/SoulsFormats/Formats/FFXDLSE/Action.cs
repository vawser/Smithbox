using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Action : FXSerializable
        {
            internal override string ClassName => "FXSerializableAction";

            internal override int Version => 1;

            [XmlAttribute]
            public int ID { get; set; }

            public ParamList ParamList { get; set; }

            public Action()
            {
                ParamList = new ParamList();
            }

            internal Action(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                ID = br.ReadInt32();
                ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(ID);
                ParamList.Write(bw, classNames);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
