using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConvexVerticesShape : hkpConvexShape
    {
        public override uint Signature { get => 3256650586; }
        
        public Vector4 m_aabbHalfExtents;
        public Vector4 m_aabbCenter;
        public List<Matrix4x4> m_rotatedVertices;
        public int m_numVertices;
        public List<Vector4> m_planeEquations;
        public hkpConvexVerticesConnectivity m_connectivity;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_aabbHalfExtents = des.ReadVector4(br);
            m_aabbCenter = des.ReadVector4(br);
            m_rotatedVertices = des.ReadMatrix3Array(br);
            m_numVertices = br.ReadInt32();
            br.ReadUInt32();
            m_planeEquations = des.ReadVector4Array(br);
            m_connectivity = des.ReadClassPointer<hkpConvexVerticesConnectivity>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_aabbHalfExtents);
            s.WriteVector4(bw, m_aabbCenter);
            s.WriteMatrix3Array(bw, m_rotatedVertices);
            bw.WriteInt32(m_numVertices);
            bw.WriteUInt32(0);
            s.WriteVector4Array(bw, m_planeEquations);
            s.WriteClassPointer<hkpConvexVerticesConnectivity>(bw, m_connectivity);
        }
    }
}
