using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ModelerType
    {
        DEFAULT = 0,
        LOCATOR = 1,
    }
    
    public partial class hkModelerNodeTypeAttribute : IHavokObject
    {
        public virtual uint Signature { get => 864815407; }
        
        public ModelerType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_type = (ModelerType)br.ReadSByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte((sbyte)m_type);
        }
    }
}
