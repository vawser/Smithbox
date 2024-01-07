using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BlendType
    {
        BLEND_TYPE_BLEND_IN = 0,
        BLEND_TYPE_FULL_ON = 1,
    }
    
    public enum DockingFlagBits
    {
        FLAG_NONE = 0,
        FLAG_DOCK_TO_FUTURE_POSITION = 1,
        FLAG_OVERRIDE_MOTION = 2,
    }
    
    public partial class hkbDockingGenerator : hkbGenerator
    {
        public override uint Signature { get => 2140111424; }
        
        public short m_dockingBone;
        public Vector4 m_translationOffset;
        public Quaternion m_rotationOffset;
        public BlendType m_blendType;
        public ushort m_flags;
        public hkbGenerator m_child;
        public int m_intervalStart;
        public int m_intervalEnd;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_dockingBone = br.ReadInt16();
            br.ReadUInt32();
            br.ReadUInt16();
            m_translationOffset = des.ReadVector4(br);
            m_rotationOffset = des.ReadQuaternion(br);
            m_blendType = (BlendType)br.ReadSByte();
            br.ReadByte();
            m_flags = br.ReadUInt16();
            br.ReadUInt32();
            m_child = des.ReadClassPointer<hkbGenerator>(br);
            m_intervalStart = br.ReadInt32();
            m_intervalEnd = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt16(m_dockingBone);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteVector4(bw, m_translationOffset);
            s.WriteQuaternion(bw, m_rotationOffset);
            bw.WriteSByte((sbyte)m_blendType);
            bw.WriteByte(0);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hkbGenerator>(bw, m_child);
            bw.WriteInt32(m_intervalStart);
            bw.WriteInt32(m_intervalEnd);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
