using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class ParamList : FXSerializable
        {
            internal override string ClassName => "FXSerializableParamList";

            internal override int Version => 2;

            [XmlAttribute]
            public int Unk04 { get; set; }

            public List<Param> Params { get; set; }

            public ParamList()
            {
                Params = new List<Param>();
            }

            internal ParamList(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                int paramCount = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Params = new List<Param>(paramCount);
                for (int i = 0; i < paramCount; i++)
                    Params.Add(Param.Read(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (Param param in Params)
                    param.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(Params.Count);
                bw.WriteInt32(Unk04);
                foreach (Param param in Params)
                    param.Write(bw, classNames);
            }

            // Disabled to get rid of trim warnings since this isn't used in DSMS
            /*
            #region IXmlSerializable
            XmlSchema IXmlSerializable.GetSchema() => null;

            void IXmlSerializable.ReadXml(XmlReader reader)
            {
                reader.MoveToContent();
                bool empty = reader.IsEmptyElement;
                Unk04 = int.Parse(reader.GetAttribute(nameof(Unk04)));
                reader.ReadStartElement();

                if (!empty)
                {
                    while (reader.IsStartElement(nameof(Param)))
                        Params.Add((Param)ParamSerializer.Deserialize(reader));
                    reader.ReadEndElement();
                }
            }

            void IXmlSerializable.WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString(nameof(Unk04), Unk04.ToString());
                for (int i = 0; i < Params.Count; i++)
                {
                    //writer.WriteComment($" {i} ");
                    ParamSerializer.Serialize(writer, Params[i]);
                }
            }
            #endregion*/
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
