using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSetBehaviorCommand : hkReferencedObject
    {
        public override uint Signature { get => 1686236417; }
        
        public ulong m_characterId;
        public hkbBehaviorGraph m_behavior;
        public hkbGenerator m_rootGenerator;
        public List<hkbBehaviorGraph> m_referencedBehaviors;
        public int m_startStateIndex;
        public bool m_randomizeSimulation;
        public int m_padding;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_behavior = des.ReadClassPointer<hkbBehaviorGraph>(br);
            m_rootGenerator = des.ReadClassPointer<hkbGenerator>(br);
            m_referencedBehaviors = des.ReadClassPointerArray<hkbBehaviorGraph>(br);
            m_startStateIndex = br.ReadInt32();
            m_randomizeSimulation = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_padding = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteClassPointer<hkbBehaviorGraph>(bw, m_behavior);
            s.WriteClassPointer<hkbGenerator>(bw, m_rootGenerator);
            s.WriteClassPointerArray<hkbBehaviorGraph>(bw, m_referencedBehaviors);
            bw.WriteInt32(m_startStateIndex);
            bw.WriteBoolean(m_randomizeSimulation);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_padding);
            bw.WriteUInt32(0);
        }
    }
}
