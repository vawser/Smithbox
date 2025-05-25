using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiEdgePathFollowingCornerInfo : IHavokObject
    {
        public virtual uint Signature { get => 3260246059; }
        
        public ushort m_data;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_data = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_data);
        }
    }
}
