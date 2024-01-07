using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBoolVariableSequencedData : hkbSequencedData
    {
        public override uint Signature { get => 3787018063; }
        
        public List<hkbBoolVariableSequencedDataSample> m_samples;
        public int m_variableIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_samples = des.ReadClassArray<hkbBoolVariableSequencedDataSample>(br);
            m_variableIndex = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbBoolVariableSequencedDataSample>(bw, m_samples);
            bw.WriteInt32(m_variableIndex);
            bw.WriteUInt32(0);
        }
    }
}
