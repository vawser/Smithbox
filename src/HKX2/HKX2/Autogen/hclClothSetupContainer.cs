using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothSetupContainer : hkReferencedObject
    {
        public override uint Signature { get => 4181648773; }
        
        public List<hclClothSetupObject> m_clothSetupDatas;
        public List<hclNamedSetupMesh> m_namedSetupMeshWrappers;
        public List<hclNamedTransformSetSetupObject> m_namedTransformSetWrappers;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_clothSetupDatas = des.ReadClassPointerArray<hclClothSetupObject>(br);
            m_namedSetupMeshWrappers = des.ReadClassPointerArray<hclNamedSetupMesh>(br);
            m_namedTransformSetWrappers = des.ReadClassPointerArray<hclNamedTransformSetSetupObject>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hclClothSetupObject>(bw, m_clothSetupDatas);
            s.WriteClassPointerArray<hclNamedSetupMesh>(bw, m_namedSetupMeshWrappers);
            s.WriteClassPointerArray<hclNamedTransformSetSetupObject>(bw, m_namedTransformSetWrappers);
        }
    }
}
