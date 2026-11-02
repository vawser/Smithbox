// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkPlaneData : HavokData<HKLib.hk2019.hkPlane> 
{
    public hkPlaneData(HavokType type, HKLib.hk2019.hkPlane instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_equation":
            case "equation":
            {
                if (instance.m_equation is not TGet castValue) return false;
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
            case "m_equation":
            case "equation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_equation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
