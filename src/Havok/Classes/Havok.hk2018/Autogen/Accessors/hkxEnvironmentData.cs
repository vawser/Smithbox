// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxEnvironmentData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxEnvironment>
{
    public hkxEnvironmentData(Havok.hk2018.hkxEnvironment instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_variables":
            case "variables":
            {
                value = instance.m_variables;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variables":
            case "variables":
            {
                if (value is not List<Havok.hk2018.hkxEnvironment.Variable> castValue) return false;
                instance.m_variables = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
