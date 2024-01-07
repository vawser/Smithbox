using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSkinBinding : hkMeshShape
    {
        public override uint Signature { get => 1666254878; }
        
        public hkMeshShape m_skin;
        public List<Matrix4x4> m_worldFromBoneTransforms;
        public List<string> m_boneNames;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_skin = des.ReadClassPointer<hkMeshShape>(br);
            m_worldFromBoneTransforms = des.ReadMatrix4Array(br);
            m_boneNames = des.ReadStringPointerArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkMeshShape>(bw, m_skin);
            s.WriteMatrix4Array(bw, m_worldFromBoneTransforms);
            s.WriteStringPointerArray(bw, m_boneNames);
        }
    }
}
