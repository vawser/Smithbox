using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiUserEdgeUtilsUserEdgeSetup : IHavokObject
    {
        public virtual uint Signature { get => 207378532; }
        
        public hkaiUserEdgeUtilsObb m_obbA;
        public hkaiUserEdgeUtilsObb m_obbB;
        public uint m_userDataA;
        public uint m_userDataB;
        public float m_costAtoB;
        public float m_costBtoA;
        public Vector4 m_worldUpA;
        public Vector4 m_worldUpB;
        public UserEdgeDirection m_direction;
        public UserEdgeSetupSpace m_space;
        public bool m_forceAlign;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_obbA = new hkaiUserEdgeUtilsObb();
            m_obbA.Read(des, br);
            m_obbB = new hkaiUserEdgeUtilsObb();
            m_obbB.Read(des, br);
            m_userDataA = br.ReadUInt32();
            m_userDataB = br.ReadUInt32();
            m_costAtoB = br.ReadSingle();
            m_costBtoA = br.ReadSingle();
            m_worldUpA = des.ReadVector4(br);
            m_worldUpB = des.ReadVector4(br);
            m_direction = (UserEdgeDirection)br.ReadByte();
            m_space = (UserEdgeSetupSpace)br.ReadByte();
            m_forceAlign = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_obbA.Write(s, bw);
            m_obbB.Write(s, bw);
            bw.WriteUInt32(m_userDataA);
            bw.WriteUInt32(m_userDataB);
            bw.WriteSingle(m_costAtoB);
            bw.WriteSingle(m_costBtoA);
            s.WriteVector4(bw, m_worldUpA);
            s.WriteVector4(bw, m_worldUpB);
            bw.WriteByte((byte)m_direction);
            bw.WriteByte((byte)m_space);
            bw.WriteBoolean(m_forceAlign);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
