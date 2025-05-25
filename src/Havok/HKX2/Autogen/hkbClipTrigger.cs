using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbClipTrigger : IHavokObject
    {
        public virtual uint Signature { get => 2125749482; }
        
        public float m_localTime;
        public hkbEventProperty m_event;
        public bool m_relativeToEndOfClip;
        public bool m_acyclic;
        public bool m_isAnnotation;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_localTime = br.ReadSingle();
            br.ReadUInt32();
            m_event = new hkbEventProperty();
            m_event.Read(des, br);
            m_relativeToEndOfClip = br.ReadBoolean();
            m_acyclic = br.ReadBoolean();
            m_isAnnotation = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_localTime);
            bw.WriteUInt32(0);
            m_event.Write(s, bw);
            bw.WriteBoolean(m_relativeToEndOfClip);
            bw.WriteBoolean(m_acyclic);
            bw.WriteBoolean(m_isAnnotation);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
