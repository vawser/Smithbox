// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVolumeConstraintSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVolumeConstraintSetupObject>
{
    public hclVolumeConstraintSetupObjectData(Havok.hk2018.hclVolumeConstraintSetupObject instance) : base(instance) { }

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
            case "m_applyToParticles":
            case "applyToParticles":
            {
                value = instance.m_applyToParticles;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_influenceParticles":
            case "influenceParticles":
            {
                value = instance.m_influenceParticles;
                return true;
            }
            case "m_particleWeights":
            case "particleWeights":
            {
                value = instance.m_particleWeights;
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
            case "m_applyToParticles":
            case "applyToParticles":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_applyToParticles = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_influenceParticles":
            case "influenceParticles":
            {
                if (value is not Havok.hk2018.hclVertexSelectionInput castValue) return false;
                instance.m_influenceParticles = castValue;
                return true;
            }
            case "m_particleWeights":
            case "particleWeights":
            {
                if (value is not Havok.hk2018.hclVertexFloatInput castValue) return false;
                instance.m_particleWeights = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
