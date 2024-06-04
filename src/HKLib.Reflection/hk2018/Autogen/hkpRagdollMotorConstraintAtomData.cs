// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpRagdollMotorConstraintAtomData : HavokData<hkpRagdollMotorConstraintAtom> 
{
    private static readonly System.Reflection.FieldInfo _motorsInfo = typeof(hkpRagdollMotorConstraintAtom).GetField("m_motors")!;
    public hkpRagdollMotorConstraintAtomData(HavokType type, hkpRagdollMotorConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_target_bRca":
            case "target_bRca":
            {
                if (instance.m_target_bRca is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                if (instance.m_motors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_target_bRca":
            case "target_bRca":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_target_bRca = castValue;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                if (value is not hkpConstraintMotor?[] castValue || castValue.Length != 3) return false;
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
            default:
            return false;
        }
    }

}
