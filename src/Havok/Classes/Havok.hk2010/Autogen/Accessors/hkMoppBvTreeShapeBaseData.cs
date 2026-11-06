// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkMoppBvTreeShapeBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkMoppBvTreeShapeBase>
{
    public hkMoppBvTreeShapeBaseData(Havok.hk2010.hkMoppBvTreeShapeBase instance) : base(instance) { }

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
            case "m_bvTreeType":
            case "bvTreeType":
            {
                value = instance.m_bvTreeType;
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
            case "m_bvTreeType":
            case "bvTreeType":
            {
                if (value is Havok.hk2010.hkpBvTreeShape.BvTreeType castValue)
                {
                    instance.m_bvTreeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_bvTreeType = (Havok.hk2010.hkpBvTreeShape.BvTreeType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_code":
            case "code":
            {
                if (value is null)
                {
                    instance.m_code = default;
                    return true;
                }
                if (value is Havok.hk2010.hkpMoppCode castValue)
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
