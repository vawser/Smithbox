using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshSimplificationSnapshot : IHavokObject
    {
        public virtual uint Signature { get => 1796332001; }
        
        public hkGeometry m_geometry;
        public List<hkaiCarver> m_carvers;
        public hkBitField m_cuttingTriangles;
        public hkaiNavMeshGenerationSettings m_settings;
        public hkaiNavMesh m_unsimplifiedNavMesh;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_geometry = des.ReadClassPointer<hkGeometry>(br);
            m_carvers = des.ReadClassPointerArray<hkaiCarver>(br);
            m_cuttingTriangles = new hkBitField();
            m_cuttingTriangles.Read(des, br);
            m_settings = new hkaiNavMeshGenerationSettings();
            m_settings.Read(des, br);
            m_unsimplifiedNavMesh = des.ReadClassPointer<hkaiNavMesh>(br);
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkGeometry>(bw, m_geometry);
            s.WriteClassPointerArray<hkaiCarver>(bw, m_carvers);
            m_cuttingTriangles.Write(s, bw);
            m_settings.Write(s, bw);
            s.WriteClassPointer<hkaiNavMesh>(bw, m_unsimplifiedNavMesh);
            bw.WriteUInt64(0);
        }
    }
}
