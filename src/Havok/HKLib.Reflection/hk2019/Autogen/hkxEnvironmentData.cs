// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxEnvironmentData : HavokData<HKLib.hk2019.hkxEnvironment> 
{
    public hkxEnvironmentData(HavokType type, HKLib.hk2019.hkxEnvironment instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variables":
            case "variables":
            {
                if (instance.m_variables is not TGet castValue) return false;
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
            case "m_variables":
            case "variables":
            {
                if (value is not List<HKLib.hk2019.hkxEnvironment.Variable> castValue) return false;
                instance.m_variables = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
