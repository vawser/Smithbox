// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxEnvironmentData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxEnvironment>
{
    public hkxEnvironmentData(Havok.hk2014.hkxEnvironment instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_variables":
            case "variables":
            {
                if (value is not List<Havok.hk2014.hkxEnvironment.Variable> castValue) return false;
                instance.m_variables = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
