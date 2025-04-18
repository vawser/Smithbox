using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRealVariableSequencedData : hkbSequencedData
    {
        public override uint Signature { get => 1533824605; }
        
        public List<hkbRealVariableSequencedDataSample> m_samples;
        public int m_variableIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_samples = des.ReadClassArray<hkbRealVariableSequencedDataSample>(br);
            m_variableIndex = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbRealVariableSequencedDataSample>(bw, m_samples);
            bw.WriteInt32(m_variableIndex);
            bw.WriteUInt32(0);
        }
    }
}
