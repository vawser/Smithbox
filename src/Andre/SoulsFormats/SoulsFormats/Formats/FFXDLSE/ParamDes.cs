using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        /// <summary>
        /// DeS param type 66 (named 72 in DS2's numbering; DS2 doesn't use it): an indexed param with a third int.
        /// Body: i32 unk, i32 arg index, i32 unk. Checked against every vanilla DeS file by FfxSurvey.
        /// </summary>
        public class Param72 : Param
        {
            internal override int Type => 72;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            [XmlAttribute]
            public int Unk0C { get; set; }

            public Param72() { }

            internal Param72(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                base.Deserialize(br, classNames);
                Unk04 = br.ReadInt32();
                ArgIndex = br.ReadInt32();
                Unk0C = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
            {
                base.Serialize(bw, classNames);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(ArgIndex);
                bw.WriteInt32(Unk0C);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
