using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGeneratorHiddenTypes : hkbReferencePoseGenerator
    {
        public override uint Signature { get => 2192614159; }
        
        public bool m_inheritedHiddenMember;
        public bool m_protectedInheritedHiddenMember;
        public bool m_privateInheritedHiddenMember;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inheritedHiddenMember = br.ReadBoolean();
            m_protectedInheritedHiddenMember = br.ReadBoolean();
            m_privateInheritedHiddenMember = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_inheritedHiddenMember);
            bw.WriteBoolean(m_protectedInheritedHiddenMember);
            bw.WriteBoolean(m_privateInheritedHiddenMember);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
