// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBendStiffnessSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBendStiffnessSetupObject>
{
    public hclBendStiffnessSetupObjectData(Havok.hk2018.hclBendStiffnessSetupObject instance) : base(instance) { }

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
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                value = instance.m_bendStiffness;
                return true;
            }
            case "m_useRestPoseConfig":
            case "useRestPoseConfig":
            {
                value = instance.m_useRestPoseConfig;
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
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_bendStiffness = castValue;
                return true;
            }
            case "m_useRestPoseConfig":
            case "useRestPoseConfig":
            {
                if (value is not bool castValue) return false;
                instance.m_useRestPoseConfig = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
