using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaMeshBinding : hkReferencedObject
    {
        public override uint Signature { get => 850455734; }
        
        public string m_originalSkeletonName;
        public string m_name;
        public hkaSkeleton m_skeleton;
        public List<hkaMeshBindingMapping> m_mappings;
        public List<Matrix4x4> m_boneFromSkinMeshTransforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_originalSkeletonName = des.ReadStringPointer(br);
            m_name = des.ReadStringPointer(br);
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
            m_mappings = des.ReadClassArray<hkaMeshBindingMapping>(br);
            m_boneFromSkinMeshTransforms = des.ReadTransformArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteStringPointer(bw, m_originalSkeletonName);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
            s.WriteClassArray<hkaMeshBindingMapping>(bw, m_mappings);
            s.WriteTransformArray(bw, m_boneFromSkinMeshTransforms);
        }
    }
}
