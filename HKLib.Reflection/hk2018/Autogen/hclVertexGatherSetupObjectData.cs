// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVertexGatherSetupObjectData : HavokData<hclVertexGatherSetupObject> 
{
    public hclVertexGatherSetupObjectData(HavokType type, hclVertexGatherSetupObject instance) : base(type, instance) {}

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
            case "m_direction":
            case "direction":
            {
                if (instance.m_direction is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_direction is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_simulationBuffer":
            case "simulationBuffer":
            {
                if (instance.m_simulationBuffer is null)
                {
                    return true;
                }
                if (instance.m_simulationBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationParticleSelection":
            case "simulationParticleSelection":
            {
                if (instance.m_simulationParticleSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayBuffer":
            case "displayBuffer":
            {
                if (instance.m_displayBuffer is null)
                {
                    return true;
                }
                if (instance.m_displayBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayVertexSelection":
            case "displayVertexSelection":
            {
                if (instance.m_displayVertexSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gatherAllThreshold":
            case "gatherAllThreshold":
            {
                if (instance.m_gatherAllThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gatherNormals":
            case "gatherNormals":
            {
                if (instance.m_gatherNormals is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_direction":
            case "direction":
            {
                if (value is hclVertexGatherSetupObject.Direction castValue)
                {
                    instance.m_direction = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_direction = (hclVertexGatherSetupObject.Direction)uintValue;
                    return true;
                }
                return false;
            }
            case "m_simulationBuffer":
            case "simulationBuffer":
            {
                if (value is null)
                {
                    instance.m_simulationBuffer = default;
                    return true;
                }
                if (value is hclSimClothBufferSetupObject castValue)
                {
                    instance.m_simulationBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationParticleSelection":
            case "simulationParticleSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_simulationParticleSelection = castValue;
                return true;
            }
            case "m_displayBuffer":
            case "displayBuffer":
            {
                if (value is null)
                {
                    instance.m_displayBuffer = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_displayBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayVertexSelection":
            case "displayVertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_displayVertexSelection = castValue;
                return true;
            }
            case "m_gatherAllThreshold":
            case "gatherAllThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_gatherAllThreshold = castValue;
                return true;
            }
            case "m_gatherNormals":
            case "gatherNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_gatherNormals = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
