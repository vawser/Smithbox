// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbScriptGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbScriptGenerator>
{
    public hkbScriptGeneratorData(Havok.hk2018.hkbScriptGenerator instance) : base(instance) { }

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
            case "m_child":
            case "child":
            {
                value = instance.m_child;
                return true;
            }
            case "m_onActivateScript":
            case "onActivateScript":
            {
                value = instance.m_onActivateScript;
                return true;
            }
            case "m_onPreUpdateScript":
            case "onPreUpdateScript":
            {
                value = instance.m_onPreUpdateScript;
                return true;
            }
            case "m_onGenerateScript":
            case "onGenerateScript":
            {
                value = instance.m_onGenerateScript;
                return true;
            }
            case "m_onHandleEventScript":
            case "onHandleEventScript":
            {
                value = instance.m_onHandleEventScript;
                return true;
            }
            case "m_onDeactivateScript":
            case "onDeactivateScript":
            {
                value = instance.m_onDeactivateScript;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbGenerator castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            case "m_onActivateScript":
            case "onActivateScript":
            {
                if (value is null)
                {
                    instance.m_onActivateScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_onActivateScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_onPreUpdateScript":
            case "onPreUpdateScript":
            {
                if (value is null)
                {
                    instance.m_onPreUpdateScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_onPreUpdateScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_onGenerateScript":
            case "onGenerateScript":
            {
                if (value is null)
                {
                    instance.m_onGenerateScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_onGenerateScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_onHandleEventScript":
            case "onHandleEventScript":
            {
                if (value is null)
                {
                    instance.m_onHandleEventScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_onHandleEventScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_onDeactivateScript":
            case "onDeactivateScript":
            {
                if (value is null)
                {
                    instance.m_onDeactivateScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_onDeactivateScript = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
