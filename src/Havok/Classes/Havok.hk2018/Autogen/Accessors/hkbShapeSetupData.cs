// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbShapeSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbShapeSetup>
{
    public hkbShapeSetupData(Havok.hk2018.hkbShapeSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                value = instance.m_capsuleHeight;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                value = instance.m_capsuleRadius;
                return true;
            }
            case "m_customPivotEnabled":
            case "customPivotEnabled":
            {
                value = instance.m_customPivotEnabled;
                return true;
            }
            case "m_customPivotIdx":
            case "customPivotIdx":
            {
                value = instance.m_customPivotIdx;
                return true;
            }
            case "m_fileName":
            case "fileName":
            {
                value = instance.m_fileName;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                if (value is not float castValue) return false;
                instance.m_capsuleHeight = castValue;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_capsuleRadius = castValue;
                return true;
            }
            case "m_customPivotEnabled":
            case "customPivotEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_customPivotEnabled = castValue;
                return true;
            }
            case "m_customPivotIdx":
            case "customPivotIdx":
            {
                if (value is not int castValue) return false;
                instance.m_customPivotIdx = castValue;
                return true;
            }
            case "m_fileName":
            case "fileName":
            {
                if (value is null)
                {
                    instance.m_fileName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_fileName = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbShapeSetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbShapeSetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
