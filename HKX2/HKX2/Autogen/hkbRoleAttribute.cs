using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Role
    {
        ROLE_DEFAULT = 0,
        ROLE_FILE_NAME = 1,
        ROLE_BONE_INDEX = 2,
        ROLE_EVENT_ID = 3,
        ROLE_VARIABLE_INDEX = 4,
        ROLE_ATTRIBUTE_INDEX = 5,
        ROLE_TIME = 6,
        ROLE_SCRIPT = 7,
        ROLE_LOCAL_FRAME = 8,
        ROLE_BONE_ATTACHMENT = 9,
    }
    
    public enum RoleFlags
    {
        FLAG_NONE = 0,
        FLAG_RAGDOLL = 1,
        FLAG_NORMALIZED = 2,
        FLAG_NOT_VARIABLE = 4,
        FLAG_HIDDEN = 8,
        FLAG_OUTPUT = 16,
        FLAG_NOT_CHARACTER_PROPERTY = 32,
        FLAG_CHAIN = 64,
    }
    
    public partial class hkbRoleAttribute : IHavokObject
    {
        public virtual uint Signature { get => 4274976361; }
        
        public Role m_role;
        public short m_flags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_role = (Role)br.ReadInt16();
            m_flags = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16((short)m_role);
            bw.WriteInt16(m_flags);
        }
    }
}
