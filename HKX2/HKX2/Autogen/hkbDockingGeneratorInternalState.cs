using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbDockingGeneratorInternalState : hkReferencedObject
    {
        public override uint Signature { get => 4252706033; }
        
        public float m_localTime;
        public float m_previousLocalTime;
        public float m_intervalStartLocalTime;
        public float m_intervalEndLocalTime;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localTime = br.ReadSingle();
            m_previousLocalTime = br.ReadSingle();
            m_intervalStartLocalTime = br.ReadSingle();
            m_intervalEndLocalTime = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_localTime);
            bw.WriteSingle(m_previousLocalTime);
            bw.WriteSingle(m_intervalStartLocalTime);
            bw.WriteSingle(m_intervalEndLocalTime);
        }
    }
}
