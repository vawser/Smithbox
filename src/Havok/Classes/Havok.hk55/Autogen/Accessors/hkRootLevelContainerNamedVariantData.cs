// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkRootLevelContainerNamedVariantData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkRootLevelContainer.NamedVariant>
{
    public hkRootLevelContainerNamedVariantData(Havok.hk55.hkRootLevelContainer.NamedVariant instance) : base(instance) { }

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
            case "m_className":
            case "className":
            {
                value = instance.m_className;
                return true;
            }
            case "m_variant":
            case "variant":
            {
                value = instance.m_variant;
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
            case "m_className":
            case "className":
            {
                if (value is null)
                {
                    instance.m_className = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_className = castValue;
                    return true;
                }
                return false;
            }
            case "m_variant":
            case "variant":
            {
                if (value is not Havok.hk55.hkVariant castValue) return false;
                instance.m_variant = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
