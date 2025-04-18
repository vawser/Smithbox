using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGenerator : hkbCustomTestGeneratorAnnotatedTypes
    {
        public override uint Signature { get => 1252436108; }
        
        public bool m_protectedHiddenMember;
        public bool m_privateHiddenMember;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_protectedHiddenMember = br.ReadBoolean();
            m_privateHiddenMember = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_protectedHiddenMember);
            bw.WriteBoolean(m_privateHiddenMember);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
