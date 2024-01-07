using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCameraShakeEventPayload : hkbEventPayload
    {
        public override uint Signature { get => 3444458626; }
        
        public float m_amplitude;
        public float m_halfLife;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_amplitude = br.ReadSingle();
            m_halfLife = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_amplitude);
            bw.WriteSingle(m_halfLife);
        }
    }
}
