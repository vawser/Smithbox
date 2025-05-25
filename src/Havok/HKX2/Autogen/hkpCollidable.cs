using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ForceCollideOntoPpuReasons
    {
        FORCE_PPU_USER_REQUEST = 1,
        FORCE_PPU_SHAPE_REQUEST = 2,
        FORCE_PPU_MODIFIER_REQUEST = 4,
        FORCE_PPU_SHAPE_UNCHECKED = 8,
    }
    
    public partial class hkpCollidable : hkpCdBody
    {
        public override uint Signature { get => 783215175; }
        
        public byte m_forceCollideOntoPpu;
        public hkpTypedBroadPhaseHandle m_broadPhaseHandle;
        public float m_allowedPenetrationDepth;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadByte();
            m_forceCollideOntoPpu = br.ReadByte();
            br.ReadUInt16();
            m_broadPhaseHandle = new hkpTypedBroadPhaseHandle();
            m_broadPhaseHandle.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_allowedPenetrationDepth = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(0);
            bw.WriteByte(m_forceCollideOntoPpu);
            bw.WriteUInt16(0);
            m_broadPhaseHandle.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteSingle(m_allowedPenetrationDepth);
            bw.WriteUInt32(0);
        }
    }
}
