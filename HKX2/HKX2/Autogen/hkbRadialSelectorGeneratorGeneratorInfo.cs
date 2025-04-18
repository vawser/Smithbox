using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRadialSelectorGeneratorGeneratorInfo : IHavokObject
    {
        public virtual uint Signature { get => 80516196; }
        
        public hkbGenerator m_generator;
        public float m_angle;
        public float m_radialSpeed;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_generator = des.ReadClassPointer<hkbGenerator>(br);
            m_angle = br.ReadSingle();
            m_radialSpeed = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkbGenerator>(bw, m_generator);
            bw.WriteSingle(m_angle);
            bw.WriteSingle(m_radialSpeed);
        }
    }
}
