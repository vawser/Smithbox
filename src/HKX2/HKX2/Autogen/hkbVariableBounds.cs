using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbVariableBounds : IHavokObject
    {
        public virtual uint Signature { get => 2608227172; }
        
        public hkbVariableValue m_min;
        public hkbVariableValue m_max;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_min = new hkbVariableValue();
            m_min.Read(des, br);
            m_max = new hkbVariableValue();
            m_max.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_min.Write(s, bw);
            m_max.Write(s, bw);
        }
    }
}
