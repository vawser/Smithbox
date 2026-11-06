// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSerializeCompatTypeParentInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSerialize.CompatTypeParentInfo>
{
    public hkSerializeCompatTypeParentInfoData(Havok.hk2019.hkSerialize.CompatTypeParentInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_firstParent":
            case "firstParent":
            {
                value = instance.m_firstParent;
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
            case "m_firstParent":
            case "firstParent":
            {
                if (value is null)
                {
                    instance.m_firstParent = default;
                    return true;
                }
                if (value is Havok.hk2019.hkSerialize.CompatTypeParentInfo.Parent castValue)
                {
                    instance.m_firstParent = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
