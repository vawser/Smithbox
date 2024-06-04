// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class CustomLookAtTwistModifierData : HavokData<CustomLookAtTwistModifier> 
{
    public CustomLookAtTwistModifierData(HavokType type, CustomLookAtTwistModifier instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ModifierID":
            case "ModifierID":
            {
                if (instance.m_ModifierID is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationAxisType":
            case "rotationAxisType":
            {
                if (instance.m_rotationAxisType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_rotationAxisType is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_SensingDummyPoly":
            case "SensingDummyPoly":
            {
                if (instance.m_SensingDummyPoly is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twistParam":
            case "twistParam":
            {
                if (instance.m_twistParam is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_UpLimitAngle":
            case "UpLimitAngle":
            {
                if (instance.m_UpLimitAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_DownLimitAngle":
            case "DownLimitAngle":
            {
                if (instance.m_DownLimitAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_RightLimitAngle":
            case "RightLimitAngle":
            {
                if (instance.m_RightLimitAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_LeftLimitAngle":
            case "LeftLimitAngle":
            {
                if (instance.m_LeftLimitAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_UpMinimumAngle":
            case "UpMinimumAngle":
            {
                if (instance.m_UpMinimumAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_DownMinimumAngle":
            case "DownMinimumAngle":
            {
                if (instance.m_DownMinimumAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_RightMinimumAngle":
            case "RightMinimumAngle":
            {
                if (instance.m_RightMinimumAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_LeftMinimumAngle":
            case "LeftMinimumAngle":
            {
                if (instance.m_LeftMinimumAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                if (instance.m_SensingAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setAngleMethod":
            case "setAngleMethod":
            {
                if (instance.m_setAngleMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_setAngleMethod is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                if (instance.m_isAdditive is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_ModifierID":
            case "ModifierID":
            {
                if (value is not int castValue) return false;
                instance.m_ModifierID = castValue;
                return true;
            }
            case "m_rotationAxisType":
            case "rotationAxisType":
            {
                if (value is CustomLookAtTwistModifier.MultiRotationAxisType castValue)
                {
                    instance.m_rotationAxisType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_rotationAxisType = (CustomLookAtTwistModifier.MultiRotationAxisType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_SensingDummyPoly":
            case "SensingDummyPoly":
            {
                if (value is not int castValue) return false;
                instance.m_SensingDummyPoly = castValue;
                return true;
            }
            case "m_twistParam":
            case "twistParam":
            {
                if (value is not List<CustomLookAtTwistModifier.TwistParam> castValue) return false;
                instance.m_twistParam = castValue;
                return true;
            }
            case "m_UpLimitAngle":
            case "UpLimitAngle":
            {
                if (value is not float castValue) return false;
                instance.m_UpLimitAngle = castValue;
                return true;
            }
            case "m_DownLimitAngle":
            case "DownLimitAngle":
            {
                if (value is not float castValue) return false;
                instance.m_DownLimitAngle = castValue;
                return true;
            }
            case "m_RightLimitAngle":
            case "RightLimitAngle":
            {
                if (value is not float castValue) return false;
                instance.m_RightLimitAngle = castValue;
                return true;
            }
            case "m_LeftLimitAngle":
            case "LeftLimitAngle":
            {
                if (value is not float castValue) return false;
                instance.m_LeftLimitAngle = castValue;
                return true;
            }
            case "m_UpMinimumAngle":
            case "UpMinimumAngle":
            {
                if (value is not float castValue) return false;
                instance.m_UpMinimumAngle = castValue;
                return true;
            }
            case "m_DownMinimumAngle":
            case "DownMinimumAngle":
            {
                if (value is not float castValue) return false;
                instance.m_DownMinimumAngle = castValue;
                return true;
            }
            case "m_RightMinimumAngle":
            case "RightMinimumAngle":
            {
                if (value is not float castValue) return false;
                instance.m_RightMinimumAngle = castValue;
                return true;
            }
            case "m_LeftMinimumAngle":
            case "LeftMinimumAngle":
            {
                if (value is not float castValue) return false;
                instance.m_LeftMinimumAngle = castValue;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                if (value is not short castValue) return false;
                instance.m_SensingAngle = castValue;
                return true;
            }
            case "m_setAngleMethod":
            case "setAngleMethod":
            {
                if (value is CustomLookAtTwistModifier.SetAngleMethod castValue)
                {
                    instance.m_setAngleMethod = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_setAngleMethod = (CustomLookAtTwistModifier.SetAngleMethod)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                if (value is not bool castValue) return false;
                instance.m_isAdditive = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
