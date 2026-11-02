// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxEnvironmentVariableData : HavokData<HKLib.hk2019.hkxEnvironment.Variable> 
{
    public hkxEnvironmentVariableData(HavokType type, HKLib.hk2019.hkxEnvironment.Variable instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_value":
            case "value":
            {
                if (instance.m_value is null)
                {
                    return true;
                }
                if (instance.m_value is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
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
            case "m_value":
            case "value":
            {
                if (value is null)
                {
                    instance.m_value = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
