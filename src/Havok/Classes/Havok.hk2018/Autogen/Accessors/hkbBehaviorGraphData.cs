// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBehaviorGraphData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBehaviorGraph>
{
    public hkbBehaviorGraphData(Havok.hk2018.hkbBehaviorGraph instance) : base(instance) { }

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
            case "m_variableMode":
            case "variableMode":
            {
                value = instance.m_variableMode;
                return true;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                value = instance.m_rootGenerator;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_variableMode":
            case "variableMode":
            {
                if (value is Havok.hk2018.hkbBehaviorGraph.VariableMode castValue)
                {
                    instance.m_variableMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_variableMode = (Havok.hk2018.hkbBehaviorGraph.VariableMode)sbyteValue;
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
                if (value is Havok.hk2018.hkbGenerator castValue)
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
                if (value is Havok.hk2018.hkbBehaviorGraphData castValue)
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
