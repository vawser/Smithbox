// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimatedQuaternionData : HavokData<HKLib.hk2019.hkxAnimatedQuaternion> 
{
    public hkxAnimatedQuaternionData(HavokType type, HKLib.hk2019.hkxAnimatedQuaternion instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_quaternions":
            case "quaternions":
            {
                if (instance.m_quaternions is not TGet castValue) return false;
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
            case "m_quaternions":
            case "quaternions":
            {
                if (value is not List<float> castValue) return false;
                instance.m_quaternions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
