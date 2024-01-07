using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class FXEffect : FXSerializable
        {
            internal override string ClassName => "FXSerializableEffect";

            internal override int Version => 5;

            [XmlAttribute]
            public int ID { get; set; }

            public ParamList ParamList1 { get; set; }

            public ParamList ParamList2 { get; set; }

            public StateMap StateMap { get; set; }

            public ResourceSet ResourceSet { get; set; }

            public FXEffect()
            {
                ParamList1 = new ParamList();
                ParamList2 = new ParamList();
                StateMap = new StateMap();
                ResourceSet = new ResourceSet();
            }

            internal FXEffect(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                br.AssertInt32(0);
                ID = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(2); // Param list count?
                br.AssertInt16(0);
                br.AssertInt16(2); // Judging by the order of class names, this must be an always-empty DLVector
                br.AssertInt32(0);

                ParamList1 = new ParamList(br, classNames);
                ParamList2 = new ParamList(br, classNames);

                StateMap = new StateMap(br, classNames);
                ResourceSet = new ResourceSet(br, classNames);
                br.AssertByte(0);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                DLVector.AddClassNames(classNames);

                ParamList1.AddClassNames(classNames);
                ParamList2.AddClassNames(classNames);

                StateMap.AddClassNames(classNames);
                ResourceSet.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(ID);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(2);
                bw.WriteInt16(0);
                bw.WriteInt16(2);
                bw.WriteInt32(0);

                ParamList1.Write(bw, classNames);
                ParamList2.Write(bw, classNames);

                StateMap.Write(bw, classNames);
                ResourceSet.Write(bw, classNames);
                bw.WriteByte(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
