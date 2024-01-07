using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BoundIndex
    {
        X_MIN = 0,
        X_MAX = 1,
        Y_MIN = 2,
        Y_MAX = 3,
        Z_MIN = 4,
        Z_MAX = 5,
    }
    
    public partial class hkcdFourAabb : IHavokObject
    {
        public virtual uint Signature { get => 2912663281; }
        
        public Vector4 m_lx;
        public Vector4 m_hx;
        public Vector4 m_ly;
        public Vector4 m_hy;
        public Vector4 m_lz;
        public Vector4 m_hz;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_lx = des.ReadVector4(br);
            m_hx = des.ReadVector4(br);
            m_ly = des.ReadVector4(br);
            m_hy = des.ReadVector4(br);
            m_lz = des.ReadVector4(br);
            m_hz = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_lx);
            s.WriteVector4(bw, m_hx);
            s.WriteVector4(bw, m_ly);
            s.WriteVector4(bw, m_hy);
            s.WriteVector4(bw, m_lz);
            s.WriteVector4(bw, m_hz);
        }
    }
}
