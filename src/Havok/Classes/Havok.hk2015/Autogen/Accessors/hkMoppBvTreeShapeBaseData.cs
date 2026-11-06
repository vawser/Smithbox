// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkMoppBvTreeShapeBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkMoppBvTreeShapeBase>
{
    public hkMoppBvTreeShapeBaseData(Havok.hk2015.hkMoppBvTreeShapeBase instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dispatchType":
            case "dispatchType":
            {
                value = instance.m_dispatchType;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                value = instance.m_bitsPerKey;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                value = instance.m_shapeInfoCodecType;
                return true;
            }
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
            case "m_dispatchType":
            case "dispatchType":
            {
                if (value is not byte castValue) return false;
                instance.m_dispatchType = castValue;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                if (value is not byte castValue) return false;
                instance.m_bitsPerKey = castValue;
                return true;
            }
            case "m_shapeInfoCodecType":
            case "shapeInfoCodecType":
            {
                if (value is not byte castValue) return false;
                instance.m_shapeInfoCodecType = castValue;
                return true;
            }
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
                if (value is not byte castValue) return false;
                instance.m_bvTreeType = castValue;
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
                if (value is Havok.hk2015.hkpMoppCode castValue)
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
