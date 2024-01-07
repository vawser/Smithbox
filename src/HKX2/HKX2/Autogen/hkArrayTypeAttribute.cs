using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ArrayType
    {
        NONE = 0,
        POINTSOUP = 1,
        ENTITIES = 2,
    }
    
    public partial class hkArrayTypeAttribute : IHavokObject
    {
        public virtual uint Signature { get => 3557073818; }
        
        public ArrayType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_type = (ArrayType)br.ReadSByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte((sbyte)m_type);
        }
    }
}
