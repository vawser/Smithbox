// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpMoppBvTreeShapeData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpMoppBvTreeShape>
{
    public hkpMoppBvTreeShapeData(Havok.hk55.hkpMoppBvTreeShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_code":
            case "code":
            {
                value = instance.m_code;
                return true;
            }
            case "m_child":
            case "child":
            {
                value = instance.m_child;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_code":
            case "code":
            {
                if (value is null)
                {
                    instance.m_code = default;
                    return true;
                }
                if (value is Havok.hk55.hkpMoppCode castValue)
                {
                    instance.m_code = castValue;
                    return true;
                }
                return false;
            }
            case "m_child":
            case "child":
            {
                if (value is not Havok.hk55.hkpSingleShapeContainer castValue) return false;
                instance.m_child = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
