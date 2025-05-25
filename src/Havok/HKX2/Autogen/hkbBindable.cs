using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBindable : hkReferencedObject
    {
        public override uint Signature { get => 1699538787; }
        
        public hkbVariableBindingSet m_variableBindingSet;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_variableBindingSet = des.ReadClassPointer<hkbVariableBindingSet>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbVariableBindingSet>(bw, m_variableBindingSet);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
