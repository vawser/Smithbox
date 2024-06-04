// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGetWorldFromModelModifierInternalStateData : HavokData<hkbGetWorldFromModelModifierInternalState> 
{
    public hkbGetWorldFromModelModifierInternalStateData(HavokType type, hkbGetWorldFromModelModifierInternalState instance) : base(type, instance) {}

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
            case "m_translationOut":
            case "translationOut":
            {
                if (instance.m_translationOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                if (instance.m_rotationOut is not TGet castValue) return false;
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
            case "m_translationOut":
            case "translationOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationOut = castValue;
                return true;
            }
            case "m_rotationOut":
            case "rotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
