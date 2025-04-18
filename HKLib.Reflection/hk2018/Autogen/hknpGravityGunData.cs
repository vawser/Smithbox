// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpGravityGunData : HavokData<hknpGravityGun> 
{
    public hknpGravityGunData(HavokType type, hknpGravityGun instance) : base(type, instance) {}

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
            case "m_keyboardKey":
            case "keyboardKey":
            {
                if (instance.m_keyboardKey is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_keyboardKey is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxNumObjectsPicked":
            case "maxNumObjectsPicked":
            {
                if (instance.m_maxNumObjectsPicked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invMaxMassOfObjectPicked":
            case "invMaxMassOfObjectPicked":
            {
                if (instance.m_invMaxMassOfObjectPicked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxDistOfObjectPicked":
            case "maxDistOfObjectPicked":
            {
                if (instance.m_maxDistOfObjectPicked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulseAppliedWhenObjectNotPicked":
            case "impulseAppliedWhenObjectNotPicked":
            {
                if (instance.m_impulseAppliedWhenObjectNotPicked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_throwVelocity":
            case "throwVelocity":
            {
                if (instance.m_throwVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capturedObjectPosition":
            case "capturedObjectPosition":
            {
                if (instance.m_capturedObjectPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capturedObjectsOffset":
            case "capturedObjectsOffset":
            {
                if (instance.m_capturedObjectsOffset is not TGet castValue) return false;
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
            case "m_keyboardKey":
            case "keyboardKey":
            {
                if (value is hknpFirstPersonGun.KeyboardKey castValue)
                {
                    instance.m_keyboardKey = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_keyboardKey = (hknpFirstPersonGun.KeyboardKey)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxNumObjectsPicked":
            case "maxNumObjectsPicked":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumObjectsPicked = castValue;
                return true;
            }
            case "m_invMaxMassOfObjectPicked":
            case "invMaxMassOfObjectPicked":
            {
                if (value is not float castValue) return false;
                instance.m_invMaxMassOfObjectPicked = castValue;
                return true;
            }
            case "m_maxDistOfObjectPicked":
            case "maxDistOfObjectPicked":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistOfObjectPicked = castValue;
                return true;
            }
            case "m_impulseAppliedWhenObjectNotPicked":
            case "impulseAppliedWhenObjectNotPicked":
            {
                if (value is not float castValue) return false;
                instance.m_impulseAppliedWhenObjectNotPicked = castValue;
                return true;
            }
            case "m_throwVelocity":
            case "throwVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_throwVelocity = castValue;
                return true;
            }
            case "m_capturedObjectPosition":
            case "capturedObjectPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_capturedObjectPosition = castValue;
                return true;
            }
            case "m_capturedObjectsOffset":
            case "capturedObjectsOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_capturedObjectsOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
