// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPoweredChainDataConstraintInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPoweredChainDataConstraintInfo>
{
    private static readonly System.Reflection.FieldInfo _motorsInfo = typeof(Havok.hk2014.hkpPoweredChainDataConstraintInfo).GetField("m_motors")!;
    public hkpPoweredChainDataConstraintInfoData(Havok.hk2014.hkpPoweredChainDataConstraintInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pivotInA":
            case "pivotInA":
            {
                value = instance.m_pivotInA;
                return true;
            }
            case "m_pivotInB":
            case "pivotInB":
            {
                value = instance.m_pivotInB;
                return true;
            }
            case "m_aTc":
            case "aTc":
            {
                value = instance.m_aTc;
                return true;
            }
            case "m_bTc":
            case "bTc":
            {
                value = instance.m_bTc;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                value = instance.m_motors;
                return true;
            }
            case "m_switchBodies":
            case "switchBodies":
            {
                value = instance.m_switchBodies;
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
            case "m_pivotInA":
            case "pivotInA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivotInA = castValue;
                return true;
            }
            case "m_pivotInB":
            case "pivotInB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivotInB = castValue;
                return true;
            }
            case "m_aTc":
            case "aTc":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_aTc = castValue;
                return true;
            }
            case "m_bTc":
            case "bTc":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_bTc = castValue;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                if (value is not Havok.hk2014.hkpConstraintMotor?[] castValue || castValue.Length != 3) return false;
                try
                {
                    _motorsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_switchBodies":
            case "switchBodies":
            {
                if (value is not bool castValue) return false;
                instance.m_switchBodies = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
