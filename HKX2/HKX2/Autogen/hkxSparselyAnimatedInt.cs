using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxSparselyAnimatedInt : hkReferencedObject
    {
        public override uint Signature { get => 4076501767; }
        
        public List<int> m_ints;
        public List<float> m_times;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_ints = des.ReadInt32Array(br);
            m_times = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt32Array(bw, m_ints);
            s.WriteSingleArray(bw, m_times);
        }
    }
}
