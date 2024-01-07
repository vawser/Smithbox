using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpTyremarksInfo : hkReferencedObject
    {
        public override uint Signature { get => 4102345107; }
        
        public float m_minTyremarkEnergy;
        public float m_maxTyremarkEnergy;
        public List<hknpTyremarksWheel> m_tyremarksWheel;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_minTyremarkEnergy = br.ReadSingle();
            m_maxTyremarkEnergy = br.ReadSingle();
            m_tyremarksWheel = des.ReadClassPointerArray<hknpTyremarksWheel>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_minTyremarkEnergy);
            bw.WriteSingle(m_maxTyremarkEnergy);
            s.WriteClassPointerArray<hknpTyremarksWheel>(bw, m_tyremarksWheel);
        }
    }
}
