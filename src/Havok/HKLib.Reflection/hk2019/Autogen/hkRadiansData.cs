// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkRadiansData : HavokData<HKLib.hk2019.hkRadians> 
{
    public hkRadiansData(HavokType type, HKLib.hk2019.hkRadians instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_radians":
            case "radians":
            {
                if (instance.m_radians is not TGet castValue) return false;
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
            case "m_radians":
            case "radians":
            {
                if (value is not float castValue) return false;
                instance.m_radians = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
