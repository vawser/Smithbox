// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class CustomLookAtTwistModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2016.CustomLookAtTwistModifier>
{
    public CustomLookAtTwistModifierData(Havok.hk2016.CustomLookAtTwistModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_ModifierID":
            case "ModifierID":
            {
                value = instance.m_ModifierID;
                return true;
            }
            case "m_rotationAxisType":
            case "rotationAxisType":
            {
                value = instance.m_rotationAxisType;
                return true;
            }
            case "m_SensingDummyPoly":
            case "SensingDummyPoly":
            {
                value = instance.m_SensingDummyPoly;
                return true;
            }
            case "m_twistParam":
            case "twistParam":
            {
                value = instance.m_twistParam;
                return true;
            }
            case "m_UpLimitAngle":
            case "UpLimitAngle":
            {
                value = instance.m_UpLimitAngle;
                return true;
            }
            case "m_DownLimitAngle":
            case "DownLimitAngle":
            {
                value = instance.m_DownLimitAngle;
                return true;
            }
            case "m_RightLimitAngle":
            case "RightLimitAngle":
            {
                value = instance.m_RightLimitAngle;
                return true;
            }
            case "m_LeftLimitAngle":
            case "LeftLimitAngle":
            {
                value = instance.m_LeftLimitAngle;
                return true;
            }
            case "m_UpMinimumAngle":
            case "UpMinimumAngle":
            {
                value = instance.m_UpMinimumAngle;
                return true;
            }
            case "m_DownMinimumAngle":
            case "DownMinimumAngle":
            {
                value = instance.m_DownMinimumAngle;
                return true;
            }
            case "m_RightMinimumAngle":
            case "RightMinimumAngle":
            {
                value = instance.m_RightMinimumAngle;
                return true;
            }
            case "m_LeftMinimumAngle":
            case "LeftMinimumAngle":
            {
                value = instance.m_LeftMinimumAngle;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                value = instance.m_SensingAngle;
                return true;
            }
            case "m_setAngleMethod":
            case "setAngleMethod":
            {
                value = instance.m_setAngleMethod;
                return true;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                value = instance.m_isAdditive;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
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
                if (value is Havok.hk2016.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2016.CustomLookAtTwistModifier.MultiRotationAxisType castValue)
                {
                    instance.m_rotationAxisType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_rotationAxisType = (Havok.hk2016.CustomLookAtTwistModifier.MultiRotationAxisType)sbyteValue;
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
                if (value is not List<Havok.hk2016.CustomLookAtTwistModifier.TwistParam> castValue) return false;
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
                if (value is Havok.hk2016.CustomLookAtTwistModifier.SetAngleMethod castValue)
                {
                    instance.m_setAngleMethod = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_setAngleMethod = (Havok.hk2016.CustomLookAtTwistModifier.SetAngleMethod)sbyteValue;
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
