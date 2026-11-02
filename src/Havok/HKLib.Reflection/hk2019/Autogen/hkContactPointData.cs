// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkContactPointData : HavokData<HKLib.hk2019.hkContactPoint> 
{
    public hkContactPointData(HavokType type, HKLib.hk2019.hkContactPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_separatingNormal":
            case "separatingNormal":
            {
                if (instance.m_separatingNormal is not TGet castValue) return false;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_separatingNormal":
            case "separatingNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_separatingNormal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
