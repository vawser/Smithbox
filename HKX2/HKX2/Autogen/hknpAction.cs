using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Result
    {
        RESULT_OK = 0,
        RESULT_REMOVE = 1,
    }
    
    public partial class hknpAction : hkReferencedObject
    {
        public override uint Signature { get => 707340578; }
        
        public ulong m_userData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_userData = br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_userData);
        }
    }
}
