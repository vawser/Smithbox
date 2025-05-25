using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGetUpModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2241841700; }
        
        public float m_timeSinceBegin;
        public float m_timeStep;
        public bool m_initNextModify;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_timeSinceBegin = br.ReadSingle();
            m_timeStep = br.ReadSingle();
            m_initNextModify = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_timeSinceBegin);
            bw.WriteSingle(m_timeStep);
            bw.WriteBoolean(m_initNextModify);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
