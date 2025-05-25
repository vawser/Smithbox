// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkQTransformfData : HavokData<hkQTransformf> 
{
    public hkQTransformfData(HavokType type, hkQTransformf instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotation":
            case "rotation":
            {
                if (instance.m_rotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (instance.m_translation is not TGet castValue) return false;
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
            case "m_rotation":
            case "rotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotation = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
