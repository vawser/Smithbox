// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbScriptGeneratorData : HavokData<hkbScriptGenerator> 
{
    public hkbScriptGeneratorData(HavokType type, hkbScriptGenerator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_child":
            case "child":
            {
                if (instance.m_child is null)
                {
                    return true;
                }
                if (instance.m_child is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_onActivateScript":
            case "onActivateScript":
            {
                if (instance.m_onActivateScript is null)
                {
                    return true;
                }
                if (instance.m_onActivateScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_onPreUpdateScript":
            case "onPreUpdateScript":
            {
                if (instance.m_onPreUpdateScript is null)
                {
                    return true;
                }
                if (instance.m_onPreUpdateScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_onGenerateScript":
            case "onGenerateScript":
            {
                if (instance.m_onGenerateScript is null)
                {
                    return true;
                }
                if (instance.m_onGenerateScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_onHandleEventScript":
            case "onHandleEventScript":
            {
                if (instance.m_onHandleEventScript is null)
                {
                    return true;
                }
                if (instance.m_onHandleEventScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_onDeactivateScript":
            case "onDeactivateScript":
            {
                if (instance.m_onDeactivateScript is null)
                {
                    return true;
                }
                if (instance.m_onDeactivateScript is TGet castValue)
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkbVariableBindingSet castValue)
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
                if (value is hkbGenerator castValue)
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
