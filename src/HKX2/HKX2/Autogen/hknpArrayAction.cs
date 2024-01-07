using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpArrayAction : hknpAction
    {
        public override uint Signature { get => 1747870410; }
        
        public List<uint> m_bodyIds;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bodyIds = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_bodyIds);
        }
    }
}
