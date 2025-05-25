using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomSprjGenerator : hkbGenerator
    {
        public override uint Signature { get => 1052243633; }
        
        public enum OffsetType
        {
            WeaponCategory = 10,
            IdleCategory = 11,
            Variable = 200,
            Float2Step = 201,
            Float3Step = 202,
            OffsetNone = 0,
        }
        
        public hkbGenerator m_child;
        public OffsetType m_offsetType;
        public int m_taeId;
        public int m_valIndex;
        public float m_valRate;
        public bool m_enableTae;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_child = des.ReadClassPointer<hkbGenerator>(br);
            m_offsetType = (OffsetType)br.ReadInt32();
            m_taeId = br.ReadInt32();
            m_valIndex = br.ReadInt32();
            m_valRate = br.ReadSingle();
            m_enableTae = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_child);
            bw.WriteInt32((int)m_offsetType);
            bw.WriteInt32(m_taeId);
            bw.WriteInt32(m_valIndex);
            bw.WriteSingle(m_valRate);
            bw.WriteBoolean(m_enableTae);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
