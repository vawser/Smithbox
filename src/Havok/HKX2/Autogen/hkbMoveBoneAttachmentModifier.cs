using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbMoveBoneAttachmentModifier : hkbModifier
    {
        public override uint Signature { get => 1355861884; }
        
        public int m_activateEventId;
        public string m_attachment;
        public string m_localFrame;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_activateEventId = br.ReadInt32();
            br.ReadUInt32();
            m_attachment = des.ReadStringPointer(br);
            m_localFrame = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_activateEventId);
            bw.WriteUInt32(0);
            s.WriteStringPointer(bw, m_attachment);
            s.WriteStringPointer(bw, m_localFrame);
        }
    }
}
