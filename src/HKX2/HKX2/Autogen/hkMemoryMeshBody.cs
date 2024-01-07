using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryMeshBody : hkMeshBody
    {
        public override uint Signature { get => 448505389; }
        
        public Matrix4x4 m_transform;
        public hkIndexedTransformSet m_transformSet;
        public hkMeshShape m_shape;
        public List<hkMeshVertexBuffer> m_vertexBuffers;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transform = des.ReadMatrix4(br);
            m_transformSet = des.ReadClassPointer<hkIndexedTransformSet>(br);
            m_shape = des.ReadClassPointer<hkMeshShape>(br);
            m_vertexBuffers = des.ReadClassPointerArray<hkMeshVertexBuffer>(br);
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteMatrix4(bw, m_transform);
            s.WriteClassPointer<hkIndexedTransformSet>(bw, m_transformSet);
            s.WriteClassPointer<hkMeshShape>(bw, m_shape);
            s.WriteClassPointerArray<hkMeshVertexBuffer>(bw, m_vertexBuffers);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
        }
    }
}
