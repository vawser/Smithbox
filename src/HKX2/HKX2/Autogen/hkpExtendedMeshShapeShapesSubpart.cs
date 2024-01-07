using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpExtendedMeshShapeShapesSubpart : hkpExtendedMeshShapeSubpart
    {
        public override uint Signature { get => 2579100016; }
        
        public List<hkpConvexShape> m_childShapes;
        public Quaternion m_rotation;
        public Vector4 m_translation;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childShapes = des.ReadClassPointerArray<hkpConvexShape>(br);
            m_rotation = des.ReadQuaternion(br);
            m_translation = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpConvexShape>(bw, m_childShapes);
            s.WriteQuaternion(bw, m_rotation);
            s.WriteVector4(bw, m_translation);
        }
    }
}
