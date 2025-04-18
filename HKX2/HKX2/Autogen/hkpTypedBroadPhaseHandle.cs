using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpTypedBroadPhaseHandle : hkpBroadPhaseHandle
    {
        public override uint Signature { get => 4105238425; }
        
        public sbyte m_type;
        public sbyte m_objectQualityType;
        public uint m_collisionFilterInfo;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_type = br.ReadSByte();
            br.ReadByte();
            m_objectQualityType = br.ReadSByte();
            br.ReadByte();
            m_collisionFilterInfo = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte(m_type);
            bw.WriteByte(0);
            bw.WriteSByte(m_objectQualityType);
            bw.WriteByte(0);
            bw.WriteUInt32(m_collisionFilterInfo);
        }
    }
}
