// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSerializeCompatTypeParentInfoParentData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSerialize.CompatTypeParentInfo.Parent>
{
    public hkSerializeCompatTypeParentInfoParentData(Havok.hk2019.hkSerialize.CompatTypeParentInfo.Parent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_version":
            case "version":
            {
                value = instance.m_version;
                return true;
            }
            case "m_next":
            case "next":
            {
                value = instance.m_next;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_version":
            case "version":
            {
                if (value is not int castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            case "m_next":
            case "next":
            {
                if (value is null)
                {
                    instance.m_next = default;
                    return true;
                }
                if (value is Havok.hk2019.hkSerialize.CompatTypeParentInfo.Parent castValue)
                {
                    instance.m_next = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
