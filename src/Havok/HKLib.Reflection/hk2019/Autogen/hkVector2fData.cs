// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkVector2fData : HavokData<HKLib.hk2019.hkVector2f> 
{
    public hkVector2fData(HavokType type, HKLib.hk2019.hkVector2f instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                if (instance.m_x is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (instance.m_y is not TGet castValue) return false;
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
            case "m_x":
            case "x":
            {
                if (value is not float castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not float castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
