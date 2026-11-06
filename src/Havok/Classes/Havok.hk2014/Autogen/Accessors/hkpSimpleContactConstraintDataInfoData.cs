// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSimpleContactConstraintDataInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSimpleContactConstraintDataInfo>
{
    private static readonly System.Reflection.FieldInfo _rhsRollingInfo = typeof(Havok.hk2014.hkpSimpleContactConstraintDataInfo).GetField("m_rhsRolling")!;
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2014.hkpSimpleContactConstraintDataInfo).GetField("m_data")!;
    public hkpSimpleContactConstraintDataInfoData(Havok.hk2014.hkpSimpleContactConstraintDataInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_biNormalAxis":
            case "biNormalAxis":
            {
                value = instance.m_biNormalAxis;
                return true;
            }
            case "m_rollingFrictionMultiplier":
            case "rollingFrictionMultiplier":
            {
                value = instance.m_rollingFrictionMultiplier;
                return true;
            }
            case "m_internalData1":
            case "internalData1":
            {
                value = instance.m_internalData1;
                return true;
            }
            case "m_rhsRolling":
            case "rhsRolling":
            {
                value = instance.m_rhsRolling;
                return true;
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                value = instance.m_contactRadius;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_flags":
            case "flags":
            {
                if (value is not ushort castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_biNormalAxis":
            case "biNormalAxis":
            {
                if (value is not ushort castValue) return false;
                instance.m_biNormalAxis = castValue;
                return true;
            }
            case "m_rollingFrictionMultiplier":
            case "rollingFrictionMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_rollingFrictionMultiplier = castValue;
                return true;
            }
            case "m_internalData1":
            case "internalData1":
            {
                if (value is not float castValue) return false;
                instance.m_internalData1 = castValue;
                return true;
            }
            case "m_rhsRolling":
            case "rhsRolling":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _rhsRollingInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                if (value is not float castValue) return false;
                instance.m_contactRadius = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not float[] castValue || castValue.Length != 4) return false;
                try
                {
                    _dataInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
