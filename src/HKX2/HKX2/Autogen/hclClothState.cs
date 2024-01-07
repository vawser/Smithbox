using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothState : hkReferencedObject
    {
        public override uint Signature { get => 2063781147; }
        
        public string m_name;
        public List<uint> m_operators;
        public List<hclClothStateBufferAccess> m_usedBuffers;
        public List<hclClothStateTransformSetAccess> m_usedTransformSets;
        public List<uint> m_usedSimCloths;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_operators = des.ReadUInt32Array(br);
            m_usedBuffers = des.ReadClassArray<hclClothStateBufferAccess>(br);
            m_usedTransformSets = des.ReadClassArray<hclClothStateTransformSetAccess>(br);
            m_usedSimCloths = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteUInt32Array(bw, m_operators);
            s.WriteClassArray<hclClothStateBufferAccess>(bw, m_usedBuffers);
            s.WriteClassArray<hclClothStateTransformSetAccess>(bw, m_usedTransformSets);
            s.WriteUInt32Array(bw, m_usedSimCloths);
        }
    }
}
