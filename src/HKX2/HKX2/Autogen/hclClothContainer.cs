using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothContainer : hkReferencedObject
    {
        public override uint Signature { get => 890409259; }
        
        public List<hclCollidable> m_collidables;
        public List<hclClothData> m_clothDatas;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_collidables = des.ReadClassPointerArray<hclCollidable>(br);
            m_clothDatas = des.ReadClassPointerArray<hclClothData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hclCollidable>(bw, m_collidables);
            s.WriteClassPointerArray<hclClothData>(bw, m_clothDatas);
        }
    }
}
