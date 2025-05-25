using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum VariableType
    {
        VARIABLE_TYPE_INVALID = -1,
        VARIABLE_TYPE_BOOL = 0,
        VARIABLE_TYPE_INT8 = 1,
        VARIABLE_TYPE_INT16 = 2,
        VARIABLE_TYPE_INT32 = 3,
        VARIABLE_TYPE_REAL = 4,
        VARIABLE_TYPE_POINTER = 5,
        VARIABLE_TYPE_VECTOR3 = 6,
        VARIABLE_TYPE_VECTOR4 = 7,
        VARIABLE_TYPE_QUATERNION = 8,
    }
    
    public partial class hkbVariableInfo : IHavokObject
    {
        public virtual uint Signature { get => 2779671522; }
        
        public hkbRoleAttribute m_role;
        public VariableType m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_role = new hkbRoleAttribute();
            m_role.Read(des, br);
            m_type = (VariableType)br.ReadSByte();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_role.Write(s, bw);
            bw.WriteSByte((sbyte)m_type);
            bw.WriteByte(0);
        }
    }
}
