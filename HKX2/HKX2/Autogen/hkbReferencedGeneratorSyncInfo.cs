using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbReferencedGeneratorSyncInfo : hkReferencedObject
    {
        public override uint Signature { get => 2845588803; }
        
        public hkbGeneratorSyncInfo m_syncInfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_syncInfo = new hkbGeneratorSyncInfo();
            m_syncInfo.Read(des, br);
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_syncInfo.Write(s, bw);
            bw.WriteUInt32(0);
        }
    }
}
