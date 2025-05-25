using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCircularSurfaceVelocity : hknpSurfaceVelocity
    {
        public override uint Signature { get => 3000298053; }
        
        public bool m_velocityIsLocalSpace;
        public Vector4 m_pivot;
        public Vector4 m_angularVelocity;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_velocityIsLocalSpace = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_pivot = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_velocityIsLocalSpace);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteVector4(bw, m_pivot);
            s.WriteVector4(bw, m_angularVelocity);
        }
    }
}
