using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRotateCharacterModifier : hkbModifier
    {
        public override uint Signature { get => 2166830607; }
        
        public float m_degreesPerSecond;
        public float m_speedMultiplier;
        public Vector4 m_axisOfRotation;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_degreesPerSecond = br.ReadSingle();
            m_speedMultiplier = br.ReadSingle();
            m_axisOfRotation = des.ReadVector4(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_degreesPerSecond);
            bw.WriteSingle(m_speedMultiplier);
            s.WriteVector4(bw, m_axisOfRotation);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
