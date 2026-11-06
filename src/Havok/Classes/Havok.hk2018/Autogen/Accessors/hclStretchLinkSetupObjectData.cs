// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStretchLinkSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStretchLinkSetupObject>
{
    public hclStretchLinkSetupObjectData(Havok.hk2018.hclStretchLinkSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                value = instance.m_simulationMesh;
                return true;
            }
            case "m_movableParticlesSelection":
            case "movableParticlesSelection":
            {
                value = instance.m_movableParticlesSelection;
                return true;
            }
            case "m_fixedParticlesSelection":
            case "fixedParticlesSelection":
            {
                value = instance.m_fixedParticlesSelection;
                return true;
            }
            case "m_rigidFactor":
            case "rigidFactor":
            {
                value = instance.m_rigidFactor;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_stretchDirection":
            case "stretchDirection":
            {
                value = instance.m_stretchDirection;
                return true;
            }
            case "m_useStretchDirection":
            case "useStretchDirection":
            {
                value = instance.m_useStretchDirection;
                return true;
            }
            case "m_useMeshTopology":
            case "useMeshTopology":
            {
                value = instance.m_useMeshTopology;
                return true;
            }
            case "m_allowDynamicLinks":
            case "allowDynamicLinks":
            {
                value = instance.m_allowDynamicLinks;
                return true;
            }
            case "m_useTopologicalStretchDistance":
            case "useTopologicalStretchDistance":
            {
                value = instance.m_useTopologicalStretchDistance;
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
                if (value is Havok.hk2018.hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_movableParticlesSelection":
            case "movableParticlesSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_movableParticlesSelection = castValue;
                return true;
            }
            case "m_fixedParticlesSelection":
            case "fixedParticlesSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_fixedParticlesSelection = castValue;
                return true;
            }
            case "m_rigidFactor":
            case "rigidFactor":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_rigidFactor = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
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
