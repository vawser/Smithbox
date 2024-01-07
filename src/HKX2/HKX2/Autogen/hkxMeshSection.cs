using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxMeshSection : hkReferencedObject
    {
        public override uint Signature { get => 1414739601; }
        
        public hkxVertexBuffer m_vertexBuffer;
        public List<hkxIndexBuffer> m_indexBuffers;
        public hkxMaterial m_material;
        public List<hkReferencedObject> m_userChannels;
        public List<hkxVertexAnimation> m_vertexAnimations;
        public List<float> m_linearKeyFrameHints;
        public List<hkMeshBoneIndexMapping> m_boneMatrixMap;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexBuffer = des.ReadClassPointer<hkxVertexBuffer>(br);
            m_indexBuffers = des.ReadClassPointerArray<hkxIndexBuffer>(br);
            m_material = des.ReadClassPointer<hkxMaterial>(br);
            m_userChannels = des.ReadClassPointerArray<hkReferencedObject>(br);
            m_vertexAnimations = des.ReadClassPointerArray<hkxVertexAnimation>(br);
            m_linearKeyFrameHints = des.ReadSingleArray(br);
            m_boneMatrixMap = des.ReadClassArray<hkMeshBoneIndexMapping>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkxVertexBuffer>(bw, m_vertexBuffer);
            s.WriteClassPointerArray<hkxIndexBuffer>(bw, m_indexBuffers);
            s.WriteClassPointer<hkxMaterial>(bw, m_material);
            s.WriteClassPointerArray<hkReferencedObject>(bw, m_userChannels);
            s.WriteClassPointerArray<hkxVertexAnimation>(bw, m_vertexAnimations);
            s.WriteSingleArray(bw, m_linearKeyFrameHints);
            s.WriteClassArray<hkMeshBoneIndexMapping>(bw, m_boneMatrixMap);
        }
    }
}
