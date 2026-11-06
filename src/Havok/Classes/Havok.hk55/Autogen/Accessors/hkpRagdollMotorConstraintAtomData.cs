// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpRagdollMotorConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpRagdollMotorConstraintAtom>
{
    private static readonly System.Reflection.FieldInfo _motorsInfo = typeof(Havok.hk55.hkpRagdollMotorConstraintAtom).GetField("m_motors")!;
    public hkpRagdollMotorConstraintAtomData(Havok.hk55.hkpRagdollMotorConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_initializedOffset":
            case "initializedOffset":
            {
                value = instance.m_initializedOffset;
                return true;
            }
            case "m_previousTargetAnglesOffset":
            case "previousTargetAnglesOffset":
            {
                value = instance.m_previousTargetAnglesOffset;
                return true;
            }
            case "m_targetFrameAinB":
            case "targetFrameAinB":
            {
                value = instance.m_targetFrameAinB;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                value = instance.m_motors;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk55.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk55.hkpConstraintAtom.AtomType)ushortValue;
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
            case "m_initializedOffset":
            case "initializedOffset":
            {
                if (value is not short castValue) return false;
                instance.m_initializedOffset = castValue;
                return true;
            }
            case "m_previousTargetAnglesOffset":
            case "previousTargetAnglesOffset":
            {
                if (value is not short castValue) return false;
                instance.m_previousTargetAnglesOffset = castValue;
                return true;
            }
            case "m_targetFrameAinB":
            case "targetFrameAinB":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_targetFrameAinB = castValue;
                return true;
            }
            case "m_motors":
            case "motors":
            {
                if (value is not Havok.hk55.hkpConstraintMotor?[] castValue || castValue.Length != 3) return false;
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
