using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultSuspensionWheelSpringSuspensionParameters : IHavokObject
    {
        public virtual uint Signature { get => 2078654161; }
        
        public float m_strength;
        public float m_dampingCompression;
        public float m_dampingRelaxation;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_strength = br.ReadSingle();
            m_dampingCompression = br.ReadSingle();
            m_dampingRelaxation = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_strength);
            bw.WriteSingle(m_dampingCompression);
            bw.WriteSingle(m_dampingRelaxation);
        }
    }
}
