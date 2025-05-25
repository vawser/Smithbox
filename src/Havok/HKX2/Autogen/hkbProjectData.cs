using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbProjectData : hkReferencedObject
    {
        public override uint Signature { get => 909906265; }
        
        public Vector4 m_worldUpWS;
        public hkbProjectStringData m_stringData;
        public EventMode m_defaultEventMode;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_worldUpWS = des.ReadVector4(br);
            m_stringData = des.ReadClassPointer<hkbProjectStringData>(br);
            m_defaultEventMode = (EventMode)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_worldUpWS);
            s.WriteClassPointer<hkbProjectStringData>(bw, m_stringData);
            bw.WriteSByte((sbyte)m_defaultEventMode);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
