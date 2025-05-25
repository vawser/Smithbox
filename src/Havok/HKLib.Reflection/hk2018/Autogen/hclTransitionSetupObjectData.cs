// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransitionSetupObjectData : HavokData<hclTransitionSetupObject> 
{
    public hclTransitionSetupObjectData(HavokType type, hclTransitionSetupObject instance) : base(type, instance) {}

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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (instance.m_simulationMesh is null)
                {
                    return true;
                }
                if (instance.m_simulationMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (instance.m_vertexSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                if (instance.m_toAnimDelay is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                if (instance.m_toSimDelay is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                if (instance.m_toSimMaxDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (instance.m_toAnimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (instance.m_toSimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (instance.m_referenceBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_referenceBufferSetup is TGet castValue)
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
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (value is null)
                {
                    instance.m_simulationMesh = default;
                    return true;
                }
                if (value is hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_toAnimDelay = castValue;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_toSimDelay = castValue;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_toSimMaxDistance = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimPeriod = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toSimPeriod = castValue;
                return true;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_referenceBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_referenceBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
