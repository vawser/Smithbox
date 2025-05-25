using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkStorageSkinnedMeshShape : hkSkinnedMeshShape
    {
        public override uint Signature { get => 3863520942; }
        
        public List<short> m_bonesBuffer;
        public List<hkSkinnedMeshShapeBoneSet> m_boneSets;
        public List<hkSkinnedMeshShapeBoneSection> m_boneSections;
        public List<hkSkinnedMeshShapePart> m_parts;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bonesBuffer = des.ReadInt16Array(br);
            m_boneSets = des.ReadClassArray<hkSkinnedMeshShapeBoneSet>(br);
            m_boneSections = des.ReadClassArray<hkSkinnedMeshShapeBoneSection>(br);
            m_parts = des.ReadClassArray<hkSkinnedMeshShapePart>(br);
            m_name = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt16Array(bw, m_bonesBuffer);
            s.WriteClassArray<hkSkinnedMeshShapeBoneSet>(bw, m_boneSets);
            s.WriteClassArray<hkSkinnedMeshShapeBoneSection>(bw, m_boneSections);
            s.WriteClassArray<hkSkinnedMeshShapePart>(bw, m_parts);
            s.WriteStringPointer(bw, m_name);
        }
    }
}
