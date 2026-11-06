// Automatically Generated

namespace Havok.hk2014;

public class hkClass : IHavokObject
{
    public string? m_name;

    public hkClass? m_parent;

    public int m_objectSize;

    public int m_numImplementedInterfaces;

    public List<hkClassEnum> m_declaredEnums = new();

    public List<hkClassMember> m_declaredMembers = new();

    public hkClass.FlagValues m_flags;

    public int m_describedVersion;


    [Flags]
    public enum FlagValues : int
    {
        FLAGS_NONE = 0,
        FLAGS_NOT_SERIALIZABLE = 1
    }

}
