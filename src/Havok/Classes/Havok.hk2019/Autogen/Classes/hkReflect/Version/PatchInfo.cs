// Automatically Generated

namespace Havok.hk2019.hkReflect.Version;

public class PatchInfo : IHavokObject
{
    public string? m_oldName;

    public string? m_newName;

    public int m_oldVersion;

    public int m_newVersion;

    public hkReflect.Version.PatchInfo.Component? m_component;

    public int m_numComponent;

    public bool m_isNewPatch;

    public bool m_isCombine;


    public class Component : IHavokObject
    {
        public hkReflect.Version.PatchInfo.PatchType m_type;

    }

    public enum PatchType : int
    {
        PATCH_INVALID = 0,
        PATCH_MEMBER_ADDED = 1,
        PATCH_MEMBER_REMOVED = 2,
        PATCH_MEMBER_RENAMED = 3,
        PATCH_MEMBER_DEFAULT_SET = 4,
        PATCH_DATAOBJECT_FUNCTION = 5,
        PATCH_CAST = 6,
        PATCH_DEPENDS = 7,
        PATCH_PARENT_SET = 8,
        PATCH_VAR_FUNCTION = 9,
        PATCH_TYPED_MEMBER_ADDED = 10,
        PATCH_TYPED_MEMBER_REMOVED = 11,
        PATCH_TYPE_CHANGED = 12,
        PATCH_VAR_TYPE_CHANGED = 13,
        PATCH_TYPE_TEMPLATE_PARAMETER_ADDED = 14,
        PATCH_VALUE_TEMPLATE_PARAMETER_ADDED = 15,
        PATCH_TYPE_TEMPLATE_PARAMETER_REMOVED = 16,
        PATCH_VALUE_TEMPLATE_PARAMETER_REMOVED = 17,
        PATCH_SET_VALUE = 18,
        PATCH_DEPENDENT_TYPE_ADDED = 19,
        PATCH_TYPE_TEMPLATE_PARAMETER_CHANGED = 20,
        PATCH_END = 21
    }

}
