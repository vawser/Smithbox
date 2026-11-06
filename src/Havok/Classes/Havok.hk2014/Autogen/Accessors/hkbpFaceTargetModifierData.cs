// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpFaceTargetModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpFaceTargetModifier>
{
    public hkbpFaceTargetModifierData(Havok.hk2014.hkbpFaceTargetModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                value = instance.m_targetIn;
                return true;
            }
            case "m_offsetAngle":
            case "offsetAngle":
            {
                value = instance.m_offsetAngle;
                return true;
            }
            case "m_onlyOnce":
            case "onlyOnce":
            {
                value = instance.m_onlyOnce;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                if (value is null)
                {
                    instance.m_targetIn = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpTarget castValue)
                {
                    instance.m_targetIn = castValue;
                    return true;
                }
                return false;
            }
            case "m_offsetAngle":
            case "offsetAngle":
            {
                if (value is not float castValue) return false;
                instance.m_offsetAngle = castValue;
                return true;
            }
            case "m_onlyOnce":
            case "onlyOnce":
            {
                if (value is not bool castValue) return false;
                instance.m_onlyOnce = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
