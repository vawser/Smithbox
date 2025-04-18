using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshGenerationSettingsRegionPruningSettings : IHavokObject
    {
        public virtual uint Signature { get => 164984537; }
        
        public float m_minRegionArea;
        public float m_minDistanceToSeedPoints;
        public float m_borderPreservationTolerance;
        public bool m_preserveVerticalBorderRegions;
        public bool m_pruneBeforeTriangulation;
        public List<Vector4> m_regionSeedPoints;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_minRegionArea = br.ReadSingle();
            m_minDistanceToSeedPoints = br.ReadSingle();
            m_borderPreservationTolerance = br.ReadSingle();
            m_preserveVerticalBorderRegions = br.ReadBoolean();
            m_pruneBeforeTriangulation = br.ReadBoolean();
            br.ReadUInt16();
            m_regionSeedPoints = des.ReadVector4Array(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_minRegionArea);
            bw.WriteSingle(m_minDistanceToSeedPoints);
            bw.WriteSingle(m_borderPreservationTolerance);
            bw.WriteBoolean(m_preserveVerticalBorderRegions);
            bw.WriteBoolean(m_pruneBeforeTriangulation);
            bw.WriteUInt16(0);
            s.WriteVector4Array(bw, m_regionSeedPoints);
        }
    }
}
