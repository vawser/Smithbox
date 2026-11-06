// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkMoppBvTreeShapeBaseData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkMoppBvTreeShapeBase>
{
    public hkMoppBvTreeShapeBaseData(Havok.hk55.hkMoppBvTreeShapeBase instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
