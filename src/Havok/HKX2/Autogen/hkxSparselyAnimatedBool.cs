using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxSparselyAnimatedBool : hkReferencedObject
    {
        public override uint Signature { get => 744789732; }
        
        public List<bool> m_bools;
        public List<float> m_times;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bools = des.ReadBooleanArray(br);
            m_times = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteBooleanArray(bw, m_bools);
            s.WriteSingleArray(bw, m_times);
        }
    }
}
