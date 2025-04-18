using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBehaviorInfo : hkReferencedObject
    {
        public override uint Signature { get => 1045969603; }
        
        public ulong m_characterId;
        public hkbBehaviorGraphData m_data;
        public List<hkbBehaviorInfoIdToNamePair> m_idToNamePairs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_data = des.ReadClassPointer<hkbBehaviorGraphData>(br);
            m_idToNamePairs = des.ReadClassArray<hkbBehaviorInfoIdToNamePair>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteClassPointer<hkbBehaviorGraphData>(bw, m_data);
            s.WriteClassArray<hkbBehaviorInfoIdToNamePair>(bw, m_idToNamePairs);
        }
    }
}
