using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class StateMap : FXSerializable
        {
            internal override string ClassName => "FXSerializableStateMap";

            internal override int Version => 1;

            public List<State> States { get; set; }

            public StateMap()
            {
                States = new List<State>();
            }

            internal StateMap(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                int stateCount = br.ReadInt32();
                States = new List<State>(stateCount);
                for (int i = 0; i < stateCount; i++)
                    States.Add(new State(br, classNames));
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                foreach (State state in States)
                    state.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                bw.WriteInt32(States.Count);
                foreach (State state in States)
                    state.Write(bw, classNames);
            }

            // Disabled to get rid of trim warnings since this isn't used in DSMS
            /*
            #region IXmlSerializable
            XmlSchema IXmlSerializable.GetSchema() => null;

            void IXmlSerializable.ReadXml(XmlReader reader)
            {
                reader.MoveToContent();
                bool empty = reader.IsEmptyElement;
                reader.ReadStartElement();

                if (!empty)
                {
                    while (reader.IsStartElement(nameof(State)))
                        States.Add((State)StateSerializer.Deserialize(reader));
                    reader.ReadEndElement();
                }
            }

            void IXmlSerializable.WriteXml(XmlWriter writer)
            {
                for (int i = 0; i < States.Count; i++)
                {
                    writer.WriteComment($" State {i} ");
                    StateSerializer.Serialize(writer, States[i]);
                }
            }
            #endregion*/
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
