using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclConvexPlanesShape : hclShape
    {
        public override uint Signature { get => 2579907977; }
        
        public List<Vector4> m_planeEquations;
        public Matrix4x4 m_localFromWorld;
        public Matrix4x4 m_worldFromLocal;
        public hkAabb m_objAabb;
        public Vector4 m_geomCentroid;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_planeEquations = des.ReadVector4Array(br);
            br.ReadUInt64();
            m_localFromWorld = des.ReadTransform(br);
            m_worldFromLocal = des.ReadTransform(br);
            m_objAabb = new hkAabb();
            m_objAabb.Read(des, br);
            m_geomCentroid = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_planeEquations);
            bw.WriteUInt64(0);
            s.WriteTransform(bw, m_localFromWorld);
            s.WriteTransform(bw, m_worldFromLocal);
            m_objAabb.Write(s, bw);
            s.WriteVector4(bw, m_geomCentroid);
        }
    }
}
