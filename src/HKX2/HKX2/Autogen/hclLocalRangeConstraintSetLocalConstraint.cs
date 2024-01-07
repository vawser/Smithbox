using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclLocalRangeConstraintSetLocalConstraint : IHavokObject
    {
        public virtual uint Signature { get => 1672211550; }
        
        public ushort m_particleIndex;
        public ushort m_referenceVertex;
        public float m_maximumDistance;
        public float m_maxNormalDistance;
        public float m_minNormalDistance;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_particleIndex = br.ReadUInt16();
            m_referenceVertex = br.ReadUInt16();
            m_maximumDistance = br.ReadSingle();
            m_maxNormalDistance = br.ReadSingle();
            m_minNormalDistance = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_particleIndex);
            bw.WriteUInt16(m_referenceVertex);
            bw.WriteSingle(m_maximumDistance);
            bw.WriteSingle(m_maxNormalDistance);
            bw.WriteSingle(m_minNormalDistance);
        }
    }
}
