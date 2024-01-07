using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpRefWorldCinfo : hkReferencedObject
    {
        public override uint Signature { get => 4222265055; }
        
        public hknpWorldCinfo m_info;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_info = new hknpWorldCinfo();
            m_info.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_info.Write(s, bw);
        }
    }
}
