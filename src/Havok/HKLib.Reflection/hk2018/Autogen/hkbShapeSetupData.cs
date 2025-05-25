// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbShapeSetupData : HavokData<hkbShapeSetup> 
{
    public hkbShapeSetupData(HavokType type, hkbShapeSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                if (instance.m_capsuleHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                if (instance.m_capsuleRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_customPivotEnabled":
            case "customPivotEnabled":
            {
                if (instance.m_customPivotEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_customPivotIdx":
            case "customPivotIdx":
            {
                if (instance.m_customPivotIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fileName":
            case "fileName":
            {
                if (instance.m_fileName is null)
                {
                    return true;
                }
                if (instance.m_fileName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkbShapeSetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkbShapeSetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
