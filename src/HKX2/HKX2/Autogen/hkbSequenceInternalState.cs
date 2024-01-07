using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbSequenceInternalState : hkReferencedObject
    {
        public override uint Signature { get => 4274199809; }
        
        public List<int> m_nextSampleEvents;
        public List<int> m_nextSampleReals;
        public List<int> m_nextSampleBools;
        public List<int> m_nextSampleInts;
        public float m_time;
        public bool m_isEnabled;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nextSampleEvents = des.ReadInt32Array(br);
            m_nextSampleReals = des.ReadInt32Array(br);
            m_nextSampleBools = des.ReadInt32Array(br);
            m_nextSampleInts = des.ReadInt32Array(br);
            m_time = br.ReadSingle();
            m_isEnabled = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt32Array(bw, m_nextSampleEvents);
            s.WriteInt32Array(bw, m_nextSampleReals);
            s.WriteInt32Array(bw, m_nextSampleBools);
            s.WriteInt32Array(bw, m_nextSampleInts);
            bw.WriteSingle(m_time);
            bw.WriteBoolean(m_isEnabled);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
