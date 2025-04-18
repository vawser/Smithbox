// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStretchLinkSetupObjectData : HavokData<hclStretchLinkSetupObject> 
{
    public hclStretchLinkSetupObjectData(HavokType type, hclStretchLinkSetupObject instance) : base(type, instance) {}

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
            case "m_movableParticlesSelection":
            case "movableParticlesSelection":
            {
                if (instance.m_movableParticlesSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixedParticlesSelection":
            case "fixedParticlesSelection":
            {
                if (instance.m_fixedParticlesSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rigidFactor":
            case "rigidFactor":
            {
                if (instance.m_rigidFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stretchDirection":
            case "stretchDirection":
            {
                if (instance.m_stretchDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useStretchDirection":
            case "useStretchDirection":
            {
                if (instance.m_useStretchDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                if (instance.m_useMeshTopology is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowDynamicLinks":
            case "allowDynamicLinks":
            {
                if (instance.m_allowDynamicLinks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useTopologicalStretchDistance":
            case "useTopologicalStretchDistance":
            {
                if (instance.m_useTopologicalStretchDistance is not TGet castValue) return false;
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
            case "m_movableParticlesSelection":
            case "movableParticlesSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_movableParticlesSelection = castValue;
                return true;
            }
            case "m_fixedParticlesSelection":
            case "fixedParticlesSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_fixedParticlesSelection = castValue;
                return true;
            }
            case "m_rigidFactor":
            case "rigidFactor":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_rigidFactor = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_stretchDirection":
            case "stretchDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_stretchDirection = castValue;
                return true;
            }
            case "m_useStretchDirection":
            case "useStretchDirection":
            {
                if (value is not bool castValue) return false;
                instance.m_useStretchDirection = castValue;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                if (value is not bool castValue) return false;
                instance.m_useMeshTopology = castValue;
                return true;
            }
            case "m_allowDynamicLinks":
            case "allowDynamicLinks":
            {
                if (value is not bool castValue) return false;
                instance.m_allowDynamicLinks = castValue;
                return true;
            }
            case "m_useTopologicalStretchDistance":
            case "useTopologicalStretchDistance":
            {
                if (value is not bool castValue) return false;
                instance.m_useTopologicalStretchDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
