using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRadialSelectorGeneratorGeneratorPair : IHavokObject
    {
        public virtual uint Signature { get => 2970612073; }
        
        public hkbRadialSelectorGeneratorGeneratorInfo m_generators_0;
        public hkbRadialSelectorGeneratorGeneratorInfo m_generators_1;
        public float m_minAngle;
        public float m_maxAngle;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_generators_0 = new hkbRadialSelectorGeneratorGeneratorInfo();
            m_generators_0.Read(des, br);
            m_generators_1 = new hkbRadialSelectorGeneratorGeneratorInfo();
            m_generators_1.Read(des, br);
            m_minAngle = br.ReadSingle();
            m_maxAngle = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_generators_0.Write(s, bw);
            m_generators_1.Write(s, bw);
            bw.WriteSingle(m_minAngle);
            bw.WriteSingle(m_maxAngle);
        }
    }
}
