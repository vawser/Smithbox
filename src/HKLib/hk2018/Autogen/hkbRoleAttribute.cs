// Automatically Generated

namespace HKLib.hk2018;

public class hkbRoleAttribute : IHavokObject
{
    public hkbRoleAttribute.Role m_role;

    public hkbRoleAttribute.RoleFlags m_flags;


    [Flags]
    public enum RoleFlags : int
    {
        FLAG_NONE = 0,
        FLAG_RAGDOLL = 1,
        FLAG_NORMALIZED = 2,
        FLAG_NOT_VARIABLE = 4,
        FLAG_HIDDEN = 8,
        FLAG_OUTPUT = 16,
        FLAG_NOT_CHARACTER_PROPERTY = 32,
        FLAG_CHAIN = 64
    }

    public enum Role : int
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
        ROLE_CHARACTER_PROPERTY_SHEET = 10
    }

}

