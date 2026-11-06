// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVertexGatherSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVertexGatherSetupObject>
{
    public hclVertexGatherSetupObjectData(Havok.hk2016.hclVertexGatherSetupObject instance) : base(instance) { }

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
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
                return true;
            }
            case "m_simulationBuffer":
            case "simulationBuffer":
            {
                value = instance.m_simulationBuffer;
                return true;
            }
            case "m_simulationParticleSelection":
            case "simulationParticleSelection":
            {
                value = instance.m_simulationParticleSelection;
                return true;
            }
            case "m_displayBuffer":
            case "displayBuffer":
            {
                value = instance.m_displayBuffer;
                return true;
            }
            case "m_displayVertexSelection":
            case "displayVertexSelection":
            {
                value = instance.m_displayVertexSelection;
                return true;
            }
            case "m_gatherAllThreshold":
            case "gatherAllThreshold":
            {
                value = instance.m_gatherAllThreshold;
                return true;
            }
            case "m_gatherNormals":
            case "gatherNormals":
            {
                value = instance.m_gatherNormals;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
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
                if (value is Havok.hk2016.hclVertexGatherSetupObject.Direction castValue)
                {
                    instance.m_direction = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_direction = (Havok.hk2016.hclVertexGatherSetupObject.Direction)uintValue;
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
                if (value is Havok.hk2016.hclSimClothBufferSetupObject castValue)
                {
                    instance.m_simulationBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationParticleSelection":
            case "simulationParticleSelection":
            {
                if (value is not Havok.hk2016.hclVertexSelectionInput castValue) return false;
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
                if (value is Havok.hk2016.hclBufferSetupObject castValue)
                {
                    instance.m_displayBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayVertexSelection":
            case "displayVertexSelection":
            {
                if (value is not Havok.hk2016.hclVertexSelectionInput castValue) return false;
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
