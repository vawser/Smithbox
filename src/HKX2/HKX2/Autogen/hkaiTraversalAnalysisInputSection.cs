using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiTraversalAnalysisInputSection : IHavokObject
    {
        public virtual uint Signature { get => 2909360209; }
        
        public hkaiNavMeshInstance m_navMeshInstance;
        public hkGeometry m_geometry;
        public hkBitField m_walkableBitfield;
        public hkBitField m_cuttingBitfield;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_navMeshInstance = des.ReadClassPointer<hkaiNavMeshInstance>(br);
            m_geometry = des.ReadClassPointer<hkGeometry>(br);
            m_walkableBitfield = new hkBitField();
            m_walkableBitfield.Read(des, br);
            m_cuttingBitfield = new hkBitField();
            m_cuttingBitfield.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkaiNavMeshInstance>(bw, m_navMeshInstance);
            s.WriteClassPointer<hkGeometry>(bw, m_geometry);
            m_walkableBitfield.Write(s, bw);
            m_cuttingBitfield.Write(s, bw);
        }
    }
}
