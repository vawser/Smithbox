using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSequence : hkbModifier
    {
        public override uint Signature { get => 340549561; }
        
        public List<hkbEventSequencedData> m_eventSequencedData;
        public List<hkbRealVariableSequencedData> m_realVariableSequencedData;
        public List<hkbBoolVariableSequencedData> m_boolVariableSequencedData;
        public List<hkbIntVariableSequencedData> m_intVariableSequencedData;
        public int m_enableEventId;
        public int m_disableEventId;
        public hkbSequenceStringData m_stringData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_eventSequencedData = des.ReadClassPointerArray<hkbEventSequencedData>(br);
            m_realVariableSequencedData = des.ReadClassPointerArray<hkbRealVariableSequencedData>(br);
            m_boolVariableSequencedData = des.ReadClassPointerArray<hkbBoolVariableSequencedData>(br);
            m_intVariableSequencedData = des.ReadClassPointerArray<hkbIntVariableSequencedData>(br);
            m_enableEventId = br.ReadInt32();
            m_disableEventId = br.ReadInt32();
            m_stringData = des.ReadClassPointer<hkbSequenceStringData>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbEventSequencedData>(bw, m_eventSequencedData);
            s.WriteClassPointerArray<hkbRealVariableSequencedData>(bw, m_realVariableSequencedData);
            s.WriteClassPointerArray<hkbBoolVariableSequencedData>(bw, m_boolVariableSequencedData);
            s.WriteClassPointerArray<hkbIntVariableSequencedData>(bw, m_intVariableSequencedData);
            bw.WriteInt32(m_enableEventId);
            bw.WriteInt32(m_disableEventId);
            s.WriteClassPointer<hkbSequenceStringData>(bw, m_stringData);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
