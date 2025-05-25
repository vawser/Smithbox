using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSenseHandleModifierRange : IHavokObject
    {
        public virtual uint Signature { get => 4216764050; }
        
        public hkbEventProperty m_event;
        public float m_minDistance;
        public float m_maxDistance;
        public bool m_ignoreHandle;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_event = new hkbEventProperty();
            m_event.Read(des, br);
            m_minDistance = br.ReadSingle();
            m_maxDistance = br.ReadSingle();
            m_ignoreHandle = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_event.Write(s, bw);
            bw.WriteSingle(m_minDistance);
            bw.WriteSingle(m_maxDistance);
            bw.WriteBoolean(m_ignoreHandle);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
