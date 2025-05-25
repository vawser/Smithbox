// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorGraphData : HavokData<hkbBehaviorGraph> 
{
    public hkbBehaviorGraphData(HavokType type, hkbBehaviorGraph instance) : base(type, instance) {}

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
            case "m_variableMode":
            case "variableMode":
            {
                if (instance.m_variableMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_variableMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                if (instance.m_rootGenerator is null)
                {
                    return true;
                }
                if (instance.m_rootGenerator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
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
            case "m_variableMode":
            case "variableMode":
            {
                if (value is hkbBehaviorGraph.VariableMode castValue)
                {
                    instance.m_variableMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_variableMode = (hkbBehaviorGraph.VariableMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                if (value is null)
                {
                    instance.m_rootGenerator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_rootGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is HKLib.hk2018.hkbBehaviorGraphData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
