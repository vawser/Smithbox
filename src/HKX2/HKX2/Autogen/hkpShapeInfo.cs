using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpShapeInfo : hkReferencedObject
    {
        public override uint Signature { get => 3556627176; }
        
        public hkpShape m_shape;
        public bool m_isHierarchicalCompound;
        public bool m_hkdShapesCollected;
        public List<string> m_childShapeNames;
        public List<Matrix4x4> m_childTransforms;
        public Matrix4x4 m_transform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_shape = des.ReadClassPointer<hkpShape>(br);
            m_isHierarchicalCompound = br.ReadBoolean();
            m_hkdShapesCollected = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            m_childShapeNames = des.ReadStringPointerArray(br);
            m_childTransforms = des.ReadTransformArray(br);
            m_transform = des.ReadTransform(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpShape>(bw, m_shape);
            bw.WriteBoolean(m_isHierarchicalCompound);
            bw.WriteBoolean(m_hkdShapesCollected);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteStringPointerArray(bw, m_childShapeNames);
            s.WriteTransformArray(bw, m_childTransforms);
            s.WriteTransform(bw, m_transform);
        }
    }
}
