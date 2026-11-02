// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkOptionalData : HavokData<HKLib.hk2019.hk.Optional> 
{
    public hkOptionalData(HavokType type, HKLib.hk2019.hk.Optional instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_optional":
            case "optional":
            {
                if (instance.m_optional is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_implicit":
            case "implicit":
            {
                if (instance.m_implicit is not TGet castValue) return false;
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
            case "m_optional":
            case "optional":
            {
                if (value is not bool castValue) return false;
                instance.m_optional = castValue;
                return true;
            }
            case "m_implicit":
            case "implicit":
            {
                if (value is not bool castValue) return false;
                instance.m_implicit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
