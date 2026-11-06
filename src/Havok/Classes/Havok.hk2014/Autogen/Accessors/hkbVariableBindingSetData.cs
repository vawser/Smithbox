// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbVariableBindingSetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbVariableBindingSet>
{
    public hkbVariableBindingSetData(Havok.hk2014.hkbVariableBindingSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bindings":
            case "bindings":
            {
                value = instance.m_bindings;
                return true;
            }
            case "m_indexOfBindingToEnable":
            case "indexOfBindingToEnable":
            {
                value = instance.m_indexOfBindingToEnable;
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
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<Havok.hk2014.hkbVariableBindingSet.Binding> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_indexOfBindingToEnable":
            case "indexOfBindingToEnable":
            {
                if (value is not int castValue) return false;
                instance.m_indexOfBindingToEnable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
