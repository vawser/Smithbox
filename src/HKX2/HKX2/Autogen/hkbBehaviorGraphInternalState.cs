using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBehaviorGraphInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3260275680; }
        
        public List<hkbNodeInternalStateInfo> m_nodeInternalStateInfos;
        public hkbVariableValueSet m_variableValueSet;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nodeInternalStateInfos = des.ReadClassPointerArray<hkbNodeInternalStateInfo>(br);
            m_variableValueSet = des.ReadClassPointer<hkbVariableValueSet>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbNodeInternalStateInfo>(bw, m_nodeInternalStateInfos);
            s.WriteClassPointer<hkbVariableValueSet>(bw, m_variableValueSet);
        }
    }
}
