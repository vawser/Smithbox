// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpSetupStabilizationAtomData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpSetupStabilizationAtom>
{
    private static readonly System.Reflection.FieldInfo _paddingInfo = typeof(Havok.hk55.hkpSetupStabilizationAtom).GetField("m_padding")!;
    public hkpSetupStabilizationAtomData(Havok.hk55.hkpSetupStabilizationAtom instance) : base(instance) { }

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
            case "m_enabled":
            case "enabled":
            {
                value = instance.m_enabled;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_enabled":
            case "enabled":
            {
                if (value is not bool castValue) return false;
                instance.m_enabled = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not byte[] castValue || castValue.Length != 8) return false;
                try
                {
                    _paddingInfo.SetValue(instance, value);
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
