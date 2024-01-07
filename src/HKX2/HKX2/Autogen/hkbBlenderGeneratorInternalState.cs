using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBlenderGeneratorInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2675405271; }
        
        public List<hkbBlenderGeneratorChildInternalState> m_childrenInternalStates;
        public List<short> m_sortedChildren;
        public float m_endIntervalWeight;
        public int m_numActiveChildren;
        public short m_beginIntervalIndex;
        public short m_endIntervalIndex;
        public bool m_initSync;
        public bool m_doSubtractiveBlend;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_childrenInternalStates = des.ReadClassArray<hkbBlenderGeneratorChildInternalState>(br);
            m_sortedChildren = des.ReadInt16Array(br);
            m_endIntervalWeight = br.ReadSingle();
            m_numActiveChildren = br.ReadInt32();
            m_beginIntervalIndex = br.ReadInt16();
            m_endIntervalIndex = br.ReadInt16();
            m_initSync = br.ReadBoolean();
            m_doSubtractiveBlend = br.ReadBoolean();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbBlenderGeneratorChildInternalState>(bw, m_childrenInternalStates);
            s.WriteInt16Array(bw, m_sortedChildren);
            bw.WriteSingle(m_endIntervalWeight);
            bw.WriteInt32(m_numActiveChildren);
            bw.WriteInt16(m_beginIntervalIndex);
            bw.WriteInt16(m_endIntervalIndex);
            bw.WriteBoolean(m_initSync);
            bw.WriteBoolean(m_doSubtractiveBlend);
            bw.WriteUInt16(0);
        }
    }
}
