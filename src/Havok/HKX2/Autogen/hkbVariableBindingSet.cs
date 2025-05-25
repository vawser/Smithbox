using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbVariableBindingSet : hkReferencedObject
    {
        public override uint Signature { get => 3913478969; }
        
        public List<hkbVariableBindingSetBinding> m_bindings;
        public int m_indexOfBindingToEnable;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bindings = des.ReadClassArray<hkbVariableBindingSetBinding>(br);
            m_indexOfBindingToEnable = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbVariableBindingSetBinding>(bw, m_bindings);
            bw.WriteInt32(m_indexOfBindingToEnable);
            bw.WriteUInt32(0);
        }
    }
}
